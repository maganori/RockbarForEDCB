using EpgTimer;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RockbarForEDCB
{
    /// <summary>
    /// メインフォームクラス
    /// </summary>
    public partial class MainForm : Form
    {
        // 設定とサービスリスト
        private ConfigManager _configManager;

        // EdcbClient(EpgTimerSrv通信)
        private readonly EdcbClient _edcbClient;

        // EpgTimerSrvからのプッシュ通知を受け取るクラス
        private readonly EdcbNotifyReceiver _edcbNotifyReceiver;

        // 通知が続けて届いたときに、まとめて1回だけ更新するためのタイマー
        // System.Threading.Timerと名前が重なるため、フルネームで記載
        private readonly System.Windows.Forms.Timer _edcbNotifyDebounceTimer =
            new System.Windows.Forms.Timer { Interval = 300 };

        // 通知で溜めた取り直し対象
        private EpgDataManager.RefreshTargets _pendingRefreshTargets = EpgDataManager.RefreshTargets.None;

        // EPGデータ管理
        private EpgDataManager _epgDataManager;

        // ListViewアイテム作成
        private ListViewBuilder _listViewBuilder;
        private ListViewEventHandler _listViewEventHandler;

        // SettingForm
        private SettingForm _settingForm = null;

        // TVTestマネージャー
        private TVTestManager _tvtestManager;

        // マウスのクリック位置を記憶
        private Point _mousePoint;

        // マウスのシングルクリック、ダブルクリック判別用
        private CancellationTokenSource _leftCts;
        private CancellationTokenSource _rightCts;

        // statusButtonのツールチップ（接続状態の説明）
        private readonly ToolTip _statusToolTip = new ToolTip();

        // 次回の画面更新が必要になる最早時刻
        private DateTime _nextMainListViewRefreshTime = DateTime.MinValue;
        private DateTime _nextSubListViewRefreshTime = DateTime.MinValue;

        /// <summary>
        /// コンストラクタ
        /// 各種初期化・初回表示処理を行う。
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            // 右クリックメニューにバージョン情報を設定
            versionDisplayToolStripMenuItem.Text = $"Version : {AppVersionAttribute.GetVersion()}";

            // フォーカスが外れたときの強調表示反転防止
            mainListView.HideSelection = true;
            subListView.HideSelection = true;

            // ConfigManagerのインスタンス化
            _configManager = new ConfigManager();

            // EdcbClientのインスタンス化
            _edcbClient = new EdcbClient();

            // 接続状態が変わったらstatusButtonの表示を更新する
            _edcbClient.AvailabilityChanged += EdcbClient_AvailabilityChanged;

            // EdcbNotifyReceiverのインスタンス化と、イベントの登録
            _edcbNotifyReceiver = new EdcbNotifyReceiver(_edcbClient);
            _edcbNotifyReceiver.NotificationReceived += EdcbNotifyReceiver_NotificationReceived;
            _edcbNotifyReceiver.Reconnected += EdcbNotifyReceiver_Reconnected;
            _edcbNotifyDebounceTimer.Tick += EdcbNotifyDebounceTimer_Tick;

            // EpgDataManagerのインスタンス化
            _epgDataManager = new EpgDataManager(_configManager, _edcbClient);

            // ListViewBuilderのインスタンス化およびデリゲートの初期化
            _listViewBuilder = new ListViewBuilder(_configManager, _epgDataManager);

            // TVTestManagerのインスタンス化
            _tvtestManager = new TVTestManager(_configManager, _edcbClient);

            // ListViewEventHandler のインスタンス化
            _listViewEventHandler = new ListViewEventHandler(
                _configManager,
                _epgDataManager,
                _tvtestManager,
                _edcbClient,
                listContextMenuStrip
            );

            // MainFormのボタンの設定(デザイナーで出来ないもの)
            SetupMainFormButton();

            // 設定反映
            ApplySetting();

            // コンフィグファイルにwidth, height指定時のみ前回位置・サイズ・スプリッタ位置で起動
            if (_configManager.RockbarSetting.Width != 0 && _configManager.RockbarSetting.Height != 0)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(_configManager.RockbarSetting.X, _configManager.RockbarSetting.Y);
                this.Size = new Size(_configManager.RockbarSetting.Width, _configManager.RockbarSetting.Height);
                splitContainer.SplitterDistance = _configManager.RockbarSetting.SplitterDistance;
            }

            // 初回仮表示
            mainListView.Items.Clear();
            subListView.Items.Clear();

            // タイマーを有効化
            scheduleTimer.Enabled = true;

            // フォーム表示完了イベント
            // リストビューの列幅を再調整して水平スクロールバー表示を防ぐ。
            this.Shown += MainForm_Shown;
        }

        /// <summary>
        /// ウィンドウプロシージャ
        /// リサイズ機能とダブルクリック無効化をwindowに追加
        /// 参照) https://stackoverflow.com/questions/31199437/borderless-and-resizable-form-c
        /// </summary>
        /// <param name="m">Windowsメッセージ</param>
        protected override void WndProc(ref Message m)
        {
            const int RESIZE_HANDLE_SIZE = 5;

            switch (m.Msg)
            {
                case 0x0084: /*NCHITTEST*/
                    base.WndProc(ref m);

                    if ((int) m.Result == 0x01) /*HTCLIENT*/
                    {
                        Point screenPoint = new Point(m.LParam.ToInt32());
                        Point clientPoint = this.PointToClient(screenPoint);
                        if (clientPoint.Y <= RESIZE_HANDLE_SIZE)
                        {
                            if (clientPoint.X <= RESIZE_HANDLE_SIZE)
                                m.Result = (IntPtr) 13; /*HTTOPLEFT*/
                            else if (clientPoint.X < (Size.Width - RESIZE_HANDLE_SIZE))
                                m.Result = (IntPtr) 12; /*HTTOP*/
                            else
                                m.Result = (IntPtr) 14; /*HTTOPRIGHT*/
                        }
                        else if (clientPoint.Y <= (Size.Height - RESIZE_HANDLE_SIZE))
                        {
                            if (clientPoint.X <= RESIZE_HANDLE_SIZE)
                                m.Result = (IntPtr) 10; /*HTLEFT*/
                            else if (clientPoint.X < (Size.Width - RESIZE_HANDLE_SIZE))
                                m.Result = (IntPtr) 2; /*HTCAPTION*/
                            else
                                m.Result = (IntPtr) 11; /*HTRIGHT*/
                        }
                        else
                        {
                            if (clientPoint.X <= RESIZE_HANDLE_SIZE)
                                m.Result = (IntPtr) 16; /*HTBOTTOMLEFT*/
                            else if (clientPoint.X < (Size.Width - RESIZE_HANDLE_SIZE))
                                m.Result = (IntPtr) 15; /*HTBOTTOM*/
                            else
                                m.Result = (IntPtr) 17; /*HTBOTTOMRIGHT*/
                        }
                    }
                    return;
                case 0x00A3: // WM_NCLBUTTONDBLCLK
                    // 非クライアント領域のダブルクリックによる最大化無効
                    m.Result = IntPtr.Zero;
                    return;
            }

            base.WndProc(ref m);
        }

        /// <summary>
        /// MainFormのボタンの設定処理(主にデザイナーで出来ないもの)
        /// </summary>
        private void SetupMainFormButton()
        {
            // ResetボタンをfilterTextBoxの中に入れる
            filterTextBox.Controls.Add(resetButton);

            resetButton.Text = "";
            resetButton.Dock = DockStyle.Right;
            resetButton.Width = filterTextBox.ClientSize.Height;
            resetButton.Cursor = Cursors.Default;
            resetButton.FlatStyle = FlatStyle.Flat;
            resetButton.FlatAppearance.BorderSize = 0;

            // ×を中央に線で描く
            resetButton.Paint += (s, e) =>
            {
                int size = (int)(resetButton.Height * 0.4f);   // 高さの4割を×の大きさにする
                int x = (resetButton.Width - size) / 2;
                int y = (resetButton.Height - size) / 2;

                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new Pen(resetButton.ForeColor, 1.5f))
                {
                    e.Graphics.DrawLine(pen, x, y, x + size, y + size);
                    e.Graphics.DrawLine(pen, x + size, y, x, y + size);
                }
            };

            // filterTextBoxに文字があるときだけResetボタンを表示
            resetButton.Visible = filterTextBox.Text.Length > 0;
            filterTextBox.TextChanged += (s, e) =>
            {
                resetButton.Visible = filterTextBox.Text.Length > 0;
            };


            // statusButton（EDCB接続状態表示・再接続ボタン）の初期設定
            statusButton.Text = "";
            statusButton.Cursor = Cursors.Hand;
            statusButton.FlatStyle = FlatStyle.Flat;
            statusButton.FlatAppearance.BorderSize = 0;

            // 接続状態に応じて図形を描く
            statusButton.Paint += statusButton_Paint;

            // 初期表示（起動直後は未接続の表示）
            UpdateStatusButton();


            // 設定ボタンを歯車マークにする
            settingButton.Text = "\uE713";
            settingButton.Font = new Font("Segoe MDL2 Assets", settingButton.Font.Size);
            settingButton.TextAlign = ContentAlignment.MiddleCenter;


            // 「×ボタン」の設定
            closeButton.Text = "";
            closeButton.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (var pen = new Pen(closeButton.ForeColor, 1.5f))
                {
                    // 「×ボタンでタスクトレイに格納する」が有効時は＿の絵にする
                    if (_configManager.RockbarSetting.StoreTaskTrayByClosing)
                    {
                        // ＿: 下寄りに水平線
                        int size = (int)(closeButton.Height * 0.4f);
                        int x = (closeButton.Width - size) / 2;
                        int y = (int)(closeButton.Height * 0.68f);
                        e.Graphics.DrawLine(pen, x, y, x + size, y);
                    }
                    else
                    {
                        // ×: 対角線2本
                        int size = (int)(closeButton.Height * 0.34f);
                        int x = (closeButton.Width - size) / 2;
                        int y = (closeButton.Height - size) / 2;
                        e.Graphics.DrawLine(pen, x, y, x + size, y + size);
                        e.Graphics.DrawLine(pen, x + size, y, x, y + size);
                    }
                }
            };
        }

        /// <summary>
        /// statusButtonの描画
        /// 接続中は緑の塗りつぶし円、未接続は赤枠の円に斜め線を描く
        /// </summary>
        private void statusButton_Paint(object sender, PaintEventArgs e)
        {
            // ボタンの高さの半分を直径にして、中央に描く
            float size = statusButton.Height * 0.5f;
            float x = (statusButton.Width - size) / 2f;
            float y = (statusButton.Height - size) / 2f;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (_edcbClient.IsAvailable)
            {
                // オンライン: 緑の塗りつぶし円
                using (var brush = new SolidBrush(Color.FromArgb(0, 160, 0)))
                {
                    e.Graphics.FillEllipse(brush, x, y, size, size);
                }
            }
            else
            {
                using (var pen = new Pen(Color.Crimson, 1.5f))
                {
                    // オフライン: 赤枠の円
                    e.Graphics.DrawEllipse(pen, x, y, size, size);

                    // ＋斜め線（左上から右下へ。円の内側に少し余白を取る）
                    float inset = size * 0.15f;
                    e.Graphics.DrawLine(pen, x + inset, y + inset, x + size - inset, y + size - inset);
                }
            }
        }

        /// <summary>
        /// statusButtonの表示（ツールチップ含む）を現在の接続状態に合わせて更新する（UIスレッドで呼ぶこと）
        /// </summary>
        private void UpdateStatusButton()
        {
            _statusToolTip.SetToolTip(statusButton,
                _edcbClient.IsAvailable
                    ? "接続中(クリックで再接続)"
                    : "未接続(クリックで再接続)");

            // 描き直しを要求する（Paintイベントで図形が変わる）
            statusButton.Invalidate();
        }

        /// <summary>
        /// EdcbClientの接続状態が変わったときの処理
        /// 別スレッドから呼ばれるため、UIスレッドに渡して更新する
        /// </summary>
        private void EdcbClient_AvailabilityChanged(object sender, EventArgs e)
        {
            // フォームが閉じている最中は何もしない
            if (IsDisposed || !IsHandleCreated) return;

            try
            {
                BeginInvoke(new Action(OnAvailabilityChanged));
            }
            catch (InvalidOperationException)
            {
                // 閉じる処理と重なってハンドルが消えた場合は無視する
            }
        }

        /// <summary>
        /// statusButtonクリック処理
        /// WOLを送ってからEDCBへ再接続する（WOLを送るかどうかは WakeOnLan 側で判断）
        /// </summary>
        private async void statusButton_Click(object sender, EventArgs e)
        {
            // WOLを送る（送らない設定なら Skipped が返る）
            WolResult wol = await WakeOnLan.SendForSettingAsync(_configManager.RockbarSetting);

            // 送信に失敗したときだけ知らせる
            if (wol == WolResult.Failed)
            {
                MessageBox.Show(
                    "WOLの送信に失敗しました。\nWolMacAddress・WolTargetHost（またはHostName）・WolPortNumberを確認してください。",
                    "WOL送信失敗",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            // 再接続する（結果はアイコンで示すため、ここではエラーを出さない）
            ConnectEdcbAsync(false);
        }

        /// <summary>
        /// 設定反映処理
        /// 主に見た目部分の設定をフォームに反映する。初回起動時・設定変更時に実行
        /// </summary>
        private void ApplySetting()
        {
            // タスクトレイアイコン常時表示
            if (_configManager.RockbarSetting.ShowTaskTrayIcon)
            {
                notifyIcon.Visible = true;
            }
            // タスクトレイアイコン常時表示がOFFで、メイン画面が出ていればタスクトレイアイコンを消す
            else if (this.Visible)
            {
                notifyIcon.Visible = false;
            }

            // 縦に並べて表示
            if (_configManager.RockbarSetting.IsHorizontalSplit)
            {
                splitContainer.Orientation = Orientation.Horizontal;
            }
            else
            {
                splitContainer.Orientation = Orientation.Vertical;
            }

            // フォント
            TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));
            Font mainFormFont = (Font)fontConverter.ConvertFromString(_configManager.RockbarSetting.MainFormFont);
            Font listFont = (Font)fontConverter.ConvertFromString(_configManager.RockbarSetting.ListFont);
            Font menuFont = (Font)fontConverter.ConvertFromString(_configManager.RockbarSetting.MenuFont);
            Font tabFont = (Font)fontConverter.ConvertFromString(_configManager.RockbarSetting.TabFont);
            Font textBoxFont = (Font)fontConverter.ConvertFromString(_configManager.RockbarSetting.TextBoxFont);

            // OSによるDPI変更(画面拡大)時の考慮
            if (_configManager.RockbarSetting.UseMainFormFontForScaling)
            {
                // 指定したフォントをフォーム全体に設定し、タブやボタンのサイズを変える
                this.Font = mainFormFont;
            }
            else
            {
                // システム指定のまま(タブの文字が途切れる場合がある)
                this.Font = SystemFonts.DefaultFont;
            }

            //タブとフィルタテキストボックスのフォント
            if (_configManager.RockbarSetting.UseIndividualMainFormFonts)
            {
                mainFormTabControl.Font = tabFont;
                filterTextBox.Font = textBoxFont;
            }
            else
            {
                mainFormTabControl.Font = mainFormFont;
                filterTextBox.Font = mainFormFont;
            }

            // 設定ボタンの歯車マークはフォント固定、フォントサイズは自動
            settingButton.Font = new Font("Segoe MDL2 Assets", this.Font.Size);

            mainListView.Font = listFont;
            subListView.Font = listFont;
            listContextMenuStrip.Font = menuFont;

            // タブや検索ボックスの位置調整
            AdjustTopControls();

            // DPI変更(画面拡大)時、最小サイズも伸ばされるためオーバライド
            this.MinimumSize = new Size(630, 100);

            // 色
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
            this.BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.FormBackColor);

            mainListView.BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.ListBackColor);
            mainListView.ForeColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.ListForeColor);

            subListView.BackColor = mainListView.BackColor;
            subListView.ForeColor = mainListView.ForeColor;

            listContextMenuStrip.BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.MenuBackColor);

            // ListViewBuilderクラスに設定適用
            _listViewBuilder.ApplySettings();

            // Web番組表機能を使用するときのみタスクトレイアイコンの右クリックメニューに「テレビ番組表」を表示
            this.openWebEpgTopToolStripMenuItem.Visible = _configManager.RockbarSetting.UseWebLink;

            // 「✕ボタンでタスクトレイに格納する」が有効時は＿ボタンにする
            closeButton.Invalidate();

            // 録画済み一覧の最大保持数(表示数)
            _epgDataManager.RecListMaxCount = _configManager.RockbarSetting.RecListMaxCount;

            // チャンネルキー -> サービス名 のキャッシュを再構築
            _epgDataManager.RebuildServiceNameCache();
        }

        /// <summary>
        /// 上部のタブ・検索ボックスの位置を調整する
        /// </summary>
        private void AdjustTopControls()
        {
            // ハンドルを確実に生成しておく(GetTabRectの結果を正しくするため)
            var handle = mainFormTabControl.Handle;

            // 最後のタブの右端 = タブ見出しが実際に占める幅
            Rectangle lastTabRect = mainFormTabControl.GetTabRect(mainFormTabControl.TabCount - 1);
            mainFormTabControl.Width = lastTabRect.Right + 1;

            // 検索ボックスをTabControlの右側へ
            filterTextBox.Left = mainFormTabControl.Right + 18;
        }

        /// <summary>
        /// 描画更新処理
        /// 必要があればEpgTimerSrv通信を行い、MainListView・SubListViewの表示を更新する。
        /// </summary>
        /// <param name="targets">EpgTimerSrvから取り直す対象</param>
        /// <param name="forceMainListRefresh">MainListView(Main Pane)の強制リフレッシュ要否</param>
        /// <param name="forceSubListRefresh">SubListtView(Sub Pane)の強制リフレッシュ要否</param>
        private void RefreshList(EpgDataManager.RefreshTargets targets, bool forceMainListRefresh, bool forceSubListRefresh)
        {
            DateTime now = DateTime.Now;

            DateTime mainListRefreshTime = _nextMainListViewRefreshTime;
            DateTime subListRefreshTime = _nextSubListViewRefreshTime;

            // 対象だけをEpgTimerSrvから取得
            EpgDataManager.RefreshResult changed = _epgDataManager.UpdateData(targets);

            // -----mainListView-----
            // 予約タブ
            if (mainFormTabControl.SelectedTab == reserveTabPage)
            {
                if (forceMainListRefresh || changed.ReserveChanged || (now >= mainListRefreshTime))
                {
                    DateTime nextTime = _listViewBuilder.BuildReserveList(mainListView);
                    _nextMainListViewRefreshTime = nextTime;
                }
            }
            // 録画タブ
            else if (mainFormTabControl.SelectedTab == recTabPage)
            {
                if (forceMainListRefresh || changed.RecChanged)
                {
                    _listViewBuilder.BuildRecList(mainListView);
                    _nextMainListViewRefreshTime = DateTime.MaxValue;
                }
            }
            // 新番組タブ
            else if (mainFormTabControl.SelectedTab == newProgramTabPage)
            {
                if (forceMainListRefresh || changed.ServiceChanged || changed.ReserveChanged || (now >= mainListRefreshTime))
                {
                    DateTime nextTime = _listViewBuilder.BuildNewProgramList(mainListView);
                    _nextMainListViewRefreshTime = nextTime;
                }
            }
            // チャンネルタブ
            else
            {
                if (forceMainListRefresh || changed.ServiceChanged || changed.ReserveChanged || (now >= mainListRefreshTime))
                {
                    DateTime nextTime = _listViewBuilder.BuildServiceList(mainListView, GetCurrentMainFormTabType());
                    _nextMainListViewRefreshTime = nextTime;
                }
            }

            // -----subListView-----
            // チューナー一覧
            if (forceSubListRefresh || changed.TunerChanged || (now >= subListRefreshTime))
            {
                DateTime nextTime = _listViewBuilder.BuildTunerList(subListView);
                _nextSubListViewRefreshTime = nextTime;
            }
        }

        /// <summary>
        /// 選択しているタブ種別を応答する
        /// </summary>
        private MainFormTabType GetCurrentMainFormTabType()
        {
            TabPage selected = mainFormTabControl.SelectedTab;
            if (selected == allTabPage) return MainFormTabType.All;
            if (selected == dttvTabPage) return MainFormTabType.DTTV;
            if (selected == bsTabPage) return MainFormTabType.BS;
            if (selected == csTabPage) return MainFormTabType.CS;
            if (selected == favoriteTabPage) return MainFormTabType.Favorite;
            if (selected == newProgramTabPage) return MainFormTabType.NewProgram;
            if (selected == reserveTabPage) return MainFormTabType.Reserve;
            if (selected == recTabPage) return MainFormTabType.Rec;

            return MainFormTabType.All;
        }

        /// <summary>
        /// mainListViewマウスクリック時処理(右クリック)
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void mainListView_MouseClick(object sender, MouseEventArgs e)
        {
            // 予約タブ
            if (mainFormTabControl.SelectedTab == reserveTabPage)
            {
                _listViewEventHandler.HandleReserveClick(sender, e, mainListView);
                return;
            }
            // 録画タブ
            else if (mainFormTabControl.SelectedTab == recTabPage)
            {
                _listViewEventHandler.HandleRecClick(sender, e, mainListView);
                return;
            }
            // 新番組タブ
            else if (mainFormTabControl.SelectedTab == newProgramTabPage)
            {
                _listViewEventHandler.HandleNewProgramClick(sender, e, mainListView);
                return;
            }
            // チャンネルタブ
            else
            {
                _listViewEventHandler.HandleServiceClick(sender, e, mainListView);
                return;
            }
        }

        /// <summary>
        /// mainListViewマウスアップ時処理
        /// 中央ボタンのクリック検出用。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void mainListView_MouseUp(object sender, MouseEventArgs e)
        {
            // 予約タブ
            if (mainFormTabControl.SelectedTab == reserveTabPage)
            {
                _listViewEventHandler.HandleReserveMouseUp(sender, e, mainListView);
                return;
            }
            // 録画タブ
            else if (mainFormTabControl.SelectedTab == recTabPage)
            {
                // 何もしない
                return;
            }
            // 新番組タブ
            else if (mainFormTabControl.SelectedTab == newProgramTabPage)
            {
                _listViewEventHandler.HandleNewProgramMouseUp(sender, e, mainListView);
                return;
            }
            // チャンネルタブ
            else
            {
                _listViewEventHandler.HandleServiceMouseUp(sender, e, mainListView);
                return;
            }
        }

        /// <summary>
        /// mainListViewマウスダブルクリック処理(左ダブルクリック)
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void mainListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // 予約タブ
            if (mainFormTabControl.SelectedTab == reserveTabPage)
            {
                _listViewEventHandler.HandleReserveDoubleClick(sender, e, mainListView);
                return;
            }
            // 録画タブ
            else if (mainFormTabControl.SelectedTab == recTabPage)
            {
                _listViewEventHandler.HandleRecDoubleClick(sender, e, mainListView);
                return;
            }
            // 新番組タブ
            else if (mainFormTabControl.SelectedTab == newProgramTabPage)
            {
                _listViewEventHandler.HandleNewProgramDoubleClick(sender, e, mainListView);
                return;
            }
            // チャンネルタブ
            else
            {
                _listViewEventHandler.HandleServiceDoubleClick(sender, e, mainListView);
                return;
            }
        }

        /// <summary>
        /// チャンネル一覧および予約一覧選択状態変更処理
        /// ヘッダを選択しようとした場合、選択状態を解除する。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void mainListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected && string.IsNullOrEmpty(e.Item.Name))
            {
                e.Item.Selected = false;
            }
        }

        /// <summary>
        /// subListViewマウスクリック時処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void subListView_MouseClick(object sender, MouseEventArgs e)
        {
            _listViewEventHandler.HandleTunerClick(sender, e, subListView);
        }

        /// <summary>
        /// subListViewマウスアップ時処理
        /// 中央ボタンのクリック検出用。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void subListView_MouseUp(object sender, MouseEventArgs e)
        {
            _listViewEventHandler.HandleTunerMouseUp(sender, e, subListView);
        }

        /// <summary>
        /// フォーム初回表示完了時処理
        /// </summary>
        private void MainForm_Shown(object sender, EventArgs e)
        {
            // リストビューの列幅を再調整して水平スクロールバー表示を防ぐ
            AdjustAllListViewColumns();

            // EdcbClientの接続確認、一覧の取得、通知の受け取りを開始する（起動時はエラーを出さない）
            ConnectEdcbAsync(false);
        }

        /// <summary>
        /// 接続設定を反映して接続を確認し、一覧を取り直して通知の受け取りを要望する
        /// 接続できなくても通知の要望は残し、死活監視の結果で接続を試みる
        /// </summary>
        /// <param name="showError">接続できなかったときにメッセージを出すか（利用者が操作したときだけtrue）</param>
        private async void ConnectEdcbAsync(bool showError)
        {
            // async voidのメソッドで例外が出るとアプリが落ちるため、全体を囲んで受け止める
            try
            {
                var setting = _configManager.RockbarSetting;

                // 設定を反映する前に止める（古い設定で受け取った通知を使わないため）
                _edcbNotifyReceiver.Stop();

                ErrCode? errCode = await _edcbClient.ApplySettingsAndCheckAsync(
                    setting.UseTcpIp, setting.HostName, setting.PortNumber, setting.TcpWatchIntervalSec);

                // 確認の途中で別の設定が反映された場合は、この結果を使わないため何もしない
                if (errCode == null) return;

                bool connected = _edcbClient.IsAvailable;

                if (!connected && showError)
                {
                    MessageBox.Show(
                        $"EpgTimerSrvと接続できません。\nオプション設定を見直してください。\n\nErrCode: {errCode}",
                        "EpgTimerSrv接続チェック失敗",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                // 接続できたときだけ一覧を取り直す（できなければ前の一覧を残すため、表示だけ更新する）
                RefreshList(connected ? EpgDataManager.RefreshTargets.All : EpgDataManager.RefreshTargets.None, true, true);

                // 接続できなくても要望は残す（サーバーが起動したら死活監視の結果で接続される）
                _edcbNotifyReceiver.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[MainForm] EDCBの接続処理で例外: " + ex);
            }
        }

        /// <summary>
        /// 通信可能状態が変わったとき
        /// </summary>
        private void OnAvailabilityChanged()
        {
            UpdateStatusButton();

            // 通信が戻ったときはすべて取り直す（止まっている間の通知を取りこぼしているかもしれないため）
            if (_edcbClient.IsAvailable)
            {
                PostRefreshRequest(EpgDataManager.RefreshTargets.All);
            }
        }

        /// <summary>
        /// 通知を受けたときの処理（受信スレッドから呼ばれる）
        /// 通知の種類から取り直す対象を決めて溜める
        /// </summary>
        private void EdcbNotifyReceiver_NotificationReceived(object sender, UpdateNotifyItem item)
        {
            EpgDataManager.RefreshTargets targets = EpgDataManager.FromNotify(item);
            if (targets == EpgDataManager.RefreshTargets.None) return;

            PostRefreshRequest(targets);
        }

        /// <summary>
        /// 通知の受け取りを接続したときの処理（接続処理のスレッドから呼ばれる）
        /// </summary>
        private void EdcbNotifyReceiver_Reconnected(object sender, EventArgs e)
        {
            // 止まっている間の変更を取りこぼしている可能性があるため、すべて取り直す
            PostRefreshRequest(EpgDataManager.RefreshTargets.All);
        }

        /// <summary>
        /// 受信スレッドから一覧の更新を要求する
        /// UIスレッドに渡してデバウンスタイマーを仕切り直す（BeginInvokeは待たずに戻るため、受信スレッドを止めない）
        /// </summary>
        private void PostRefreshRequest(EpgDataManager.RefreshTargets targets)
        {
            // フォームが閉じている最中は何もしない
            if (IsDisposed || !IsHandleCreated) return;

            try
            {
                // UIスレッドで、取り直す対象を溜めてからデバウンスを仕切り直す
                // （BeginInvokeは待たずに戻るため、受信スレッドを止めない）
                BeginInvoke(new Action(() =>
                {
                    _pendingRefreshTargets |= targets;
                    RestartDebounceTimer();
                }));
            }
            catch (InvalidOperationException)
            {
                // 閉じる処理と重なってハンドルが消えた場合は無視する
            }
        }

        /// <summary>
        /// デバウンスタイマーを仕切り直す（UIスレッドで呼ぶこと）
        /// 通知が続けて届いている間は、最後の通知から一定時間後に1回だけ更新される
        /// </summary>
        private void RestartDebounceTimer()
        {
            _edcbNotifyDebounceTimer.Stop();
            _edcbNotifyDebounceTimer.Start();
        }

        /// <summary>
        /// デバウンス後の更新処理
        /// 溜めた対象だけを取り直し、一覧を再描画する
        /// </summary>
        private void EdcbNotifyDebounceTimer_Tick(object sender, EventArgs e)
        {
            _edcbNotifyDebounceTimer.Stop();

            // 溜めた対象を取り出して、空にする
            EpgDataManager.RefreshTargets targets = _pendingRefreshTargets;
            _pendingRefreshTargets = EpgDataManager.RefreshTargets.None;

            RefreshList(targets, true, false);
        }

        /// <summary>
        /// フォームサイズ変更処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            AdjustAllListViewColumns();
        }

        /// <summary>
        /// スプリッタ位置調整処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            AdjustAllListViewColumns();
        }

        /// <summary>
        /// リストビューのカラム幅を調整する。
        /// </summary>
        private void AdjustAllListViewColumns()
        {
            if (_listViewBuilder == null) return;

            _listViewBuilder.AdjustListViewColumns(mainListView);
            _listViewBuilder.AdjustListViewColumns(subListView);
        }

        /// <summary>
        /// タイマー処理
        /// 毎秒の表示番組更新と、TVTestの起動・終了を起動
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void scheduleTimer_Tick(object sender, EventArgs e)
        {
            // 放送中・次番組などの表示を、時刻に合わせて描き直す
            RefreshList(EpgDataManager.RefreshTargets.None, false, false);

            // TVTest自動起動、自動終了
            _tvtestManager.AutoStartAndCloseTVTest(_epgDataManager.ReserveDatas, _configManager.FavoriteServiceList);
        }

        /// <summary>
        /// チャンネルタブ切り替え処理
        /// EpgTimerSrv通信せずに、対象チャンネルの表示切替を行う
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void mainFormTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshList(EpgDataManager.RefreshTargets.None, true, false);
        }

        /// <summary>
        /// メインフォームマウスボタン押下処理
        /// ドラッグ時の位置取得。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                //位置を記憶する
                _mousePoint = new Point(e.X, e.Y);
            }

            // フォームと同様にドラッグしたいラベルはEnabled = falseにしておく
        }

        /// <summary>
        /// メインフォームマウス移動処理
        /// ドラッグ時フォーム移動処理。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                this.Left += e.X - _mousePoint.X;
                this.Top += e.Y - _mousePoint.Y;
            }
        }

        /// <summary>
        /// フィルタテキストボックス入力処理
        /// リアルタイムにフィルタを行う
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void filterTextBox_TextChanged(object sender, EventArgs e)
        {
            bool hasText = !string.IsNullOrWhiteSpace(filterTextBox.Text);

            // テキストボックスに文字が入力されていたら色を変える（空ならデフォルト色）
            filterTextBox.BackColor = hasText ? Color.Pink : SystemColors.Window;

            // ListViewBuilder 側に現在の検索文字列を渡す
            _listViewBuilder.FilterText = filterTextBox.Text;

            RefreshList(EpgDataManager.RefreshTargets.None, true, false);
        }

        /// <summary>
        /// フィルタ入力欄キー押下処理
        /// Esc入力時にフィルタをリセットする。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void filterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Escキーが押されたらフィルタをリセットする
            if (e.KeyCode == Keys.Escape)
            {
                ResetFilter();
                e.SuppressKeyPress = true;  // ビープ音を抑止
            }
        }

        /// <summary>
        /// リセットボタン押下処理
        /// フィルタ文字列をクリアしフィルタ結果をリセットする。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void resetButton_Click(object sender, EventArgs e)
        {
            ResetFilter();
            filterTextBox.Focus();
        }

        /// <summary>
        /// フィルタのリセット処理
        /// フィルタ文字列をクリアしフィルタ結果をリセットする。
        /// </summary>
        private void ResetFilter()
        {
            filterTextBox.Clear();
            RefreshList(EpgDataManager.RefreshTargets.None, true, false);
        }

        /// <summary>
        /// 設定ボタン押下処理
        /// 設定フォームを開き、設定変更があった場合は設定を再読込して画面をリフレッシュする。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void settingButton_Click(object sender, EventArgs e)
        {
            OpenSettingForm();
        }

        /// <summary>
        /// 設定フォームをモーダレスで開く（既に開いている場合はフォーカスを移動）
        /// </summary>
        private void OpenSettingForm()
        {
            if (_settingForm == null || _settingForm.IsDisposed)
            {
                _settingForm = new SettingForm(_configManager,  _edcbClient);

                // 「設定適用」、「設定保存」ボタンが押されたときのイベントをハンドリング
                _settingForm.ApplyRequested += SettingForm_ApplyRequested;

                // モーダレスで表示（MainFormの操作が可能）
                _settingForm.Show(this);
            }
            else
            {
                // 既に開いている場合は前面に移動してフォーカス
                if (_settingForm.WindowState == FormWindowState.Minimized)
                {
                    _settingForm.WindowState = FormWindowState.Normal;
                }
                _settingForm.Activate();
            }
        }

        /// <summary>
        /// SettingForm の「設定適用」、「設定保存」ボタンが押された際のイベントハンドラー
        /// </summary>
        private void SettingForm_ApplyRequested(object sender, EventArgs e)
        {
            ApplyConfigAndRefresh();
        }

        /// <summary>
        /// 設定ファイルを再読み込みし、MainForm全体の表示および動作設定を再反映する
        /// </summary>
        public void ApplyConfigAndRefresh()
        {
            ApplySetting();
            ConnectEdcbAsync(true);
        }

        /// <summary>
        /// ✕ボタン押下処理
        /// タスクトレイ格納オプションON時、タスクトレイに格納。そうでない場合、フォームを閉じる
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void closeButton_Click(object sender, EventArgs e)
        {
            if (_configManager.RockbarSetting.StoreTaskTrayByClosing)
            {
                // 他のオプションにかかわらず、最小化(もどき)をする場合はタスクトレイにアイコンを表示する
                notifyIcon.Visible = true;
                this.Visible = false;
            }
            else
            {
                this.Close();
            }
        }

        /// <summary>
        /// フォームクローズ中処理
        /// フォームクローズ時に、位置・サイズ情報を設定ファイルに出力する。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _configManager.RockbarSetting.X = this.Location.X;
            _configManager.RockbarSetting.Y = this.Location.Y;
            _configManager.RockbarSetting.Width = this.Size.Width;
            _configManager.RockbarSetting.Height = this.Size.Height;
            _configManager.RockbarSetting.SplitterDistance = splitContainer.SplitterDistance;

            _configManager.SaveToFile();

            // 通知の受け取りを止める（EpgTimerSrvへの登録解除も行う）
            _edcbNotifyDebounceTimer.Stop();
            _edcbNotifyReceiver.Dispose();   // 先に通知の受け取りを止める
            _edcbClient.Dispose();           // 死活監視のタイマーを止める
        }

        /// <summary>
        /// タスクトレイメニュー「終了」クリック処理
        /// フォームを閉じる。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// タスクトレイメニュー「設定」クリック処理
        /// 設定フォームを開く。
        /// </summary>
        private void openSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSettingForm();
        }

        /// <summary>
        /// タスクトレイメニュー「表示位置をリセット」クリック処理
        /// メイン画面を初期の位置・サイズに戻し、表示する
        /// </summary>
        private void resetPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 初期の位置・サイズ・スプリッタ位置に戻す
            this.Location = new Point(RockbarSetting.DEFAULT_WINDOW_X, RockbarSetting.DEFAULT_WINDOW_Y);
            this.Size = new Size(RockbarSetting.DEFAULT_WINDOW_WIDTH, RockbarSetting.DEFAULT_WINDOW_HEIGHT);
            splitContainer.SplitterDistance = RockbarSetting.DEFAULT_SPLITTER_DISTANCE;

            // タスクトレイに格納中でも見えるように表示する
            this.Visible = true;
            this.Activate();
        }

        /// <summary>
        /// タスクトレイメニュー「ReadMe(GitHub)」クリック処理
        /// ReadMe(GitHub)を開く。
        /// </summary>
        private void readMeGitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RockbarUtility.OpenBrowser("https://github.com/maganori/RockbarForEDCB#rockbar-for-edcb");
        }

        /// <summary>
        /// タスクトレイメニュー「更新の確認(GitHub)」クリック処理
        /// 更新の確認(GitHub)を開く。
        /// </summary>
        private void checkReleaseGitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RockbarUtility.OpenBrowser("https://github.com/maganori/RockbarForEDCB/releases");
        }

        /// <summary>
        /// タスクトレイメニュー「テレビ番組表」クリック処理
        /// テレビ番組表を開く
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void openWebEpgTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWebEpgTop();
        }

        /// <summary>
        /// タスクトレイアイコンマウスボタン押下処理
        /// シングルクリックとダブルクリックの判定
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private async void notifyIcon_MouseDown(object sender, MouseEventArgs e)
        {
            // ----- 左クリックの判定 -----
            if (e.Button == MouseButtons.Left)
            {
                if (string.IsNullOrWhiteSpace(_configManager.RockbarSetting.TaskTrayIconLeftDoubleClick))
                {
                    Debug.WriteLine("左ダブルクリック設定無し");
                    ExecuteTaskTrayIconClickAction(_configManager.RockbarSetting.TaskTrayIconLeftClick);
                    return;
                }

                // 既に待機中のクリックがある場合（＝ダブルクリック成立）
                if (_leftCts != null)
                {
                    _leftCts.Cancel();
                    Debug.WriteLine("左ダブルクリック実行");
                    ExecuteTaskTrayIconClickAction(_configManager.RockbarSetting.TaskTrayIconLeftDoubleClick);
                    return;
                }

                // --- 1回目のクリック処理 ---
                _leftCts = new CancellationTokenSource();
                try
                {
                    // OS設定のダブルクリック時間だけ非同期で待機
                    await Task.Delay(SystemInformation.DoubleClickTime, _leftCts.Token);

                    // 時間内にキャンセルされなかった場合のみシングルクリック実行
                    Debug.WriteLine("左シングルクリック実行");
                    ExecuteTaskTrayIconClickAction(_configManager.RockbarSetting.TaskTrayIconLeftClick);
                }
                catch (OperationCanceledException) { }
                finally
                {
                    _leftCts?.Dispose();
                    _leftCts = null;
                }
            }
            // ----- 右クリックの判定 -----
            else if (e.Button == MouseButtons.Right)
            {
                if (string.IsNullOrWhiteSpace(_configManager.RockbarSetting.TaskTrayIconRightDoubleClick))
                {
                    Debug.WriteLine("右ダブルクリック設定無し");
                    ExecuteTaskTrayIconClickAction(_configManager.RockbarSetting.TaskTrayIconRightClick);
                    return;
                }

                // 既に待機中のクリックがある場合（＝ダブルクリック成立）
                if (_rightCts != null)
                {
                    _rightCts.Cancel();
                    Debug.WriteLine("右ダブルクリック実行");
                    ExecuteTaskTrayIconClickAction(_configManager.RockbarSetting.TaskTrayIconRightDoubleClick);
                    return;
                }

                // --- 1回目のクリック処理 ---
                _rightCts = new CancellationTokenSource();
                try
                {
                    // OS設定のダブルクリック時間だけ非同期で待機
                    await Task.Delay(SystemInformation.DoubleClickTime, _rightCts.Token);

                    // 時間内にキャンセルされなかった場合のみシングルクリック実行
                    Debug.WriteLine("右シングルクリック実行");
                    ExecuteTaskTrayIconClickAction(_configManager.RockbarSetting.TaskTrayIconRightClick);
                }
                catch (OperationCanceledException) { }
                finally
                {
                    _rightCts?.Dispose();
                    _rightCts = null;
                }
            }
        }

        /// <summary>
        /// タスクトレイアイコンクリック時の割り当て動作の実行
        /// </summary>
        private void ExecuteTaskTrayIconClickAction(string settingValue)
        {
            switch (settingValue)
            {
                case "テレビ番組表":
                    OpenWebEpgTop();
                    break;
                case "Rockバー表示":
                    ToggleRockbarVisibility();
                    break;
                case "メニュー":
                    // 一時的に notifyIconにContextMenuStrip を割り当て
                    this.notifyIcon.ContextMenuStrip = this.taskTrayContextMenuStrip;

                    // .NET の NotifyIcon クラスに存在するOS 標準の ShowContextMenu メソッドを取得
                    // (ShowContextMenuという名前 であり
                    //  (1.インスタンスに属するメソッド かつ 2.publicではなく、private や protectedなもの の2条件を同時に満たすもの) )
                    var method = typeof(NotifyIcon).GetMethod("ShowContextMenu",
                        System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    // 見つかれば発火
                    method?.Invoke(this.notifyIcon, null);

                    // 右クリックで呼び出されないように解除
                    this.notifyIcon.ContextMenuStrip = null;

                    break;

                default:
                    // 未設定（""）などの場合は何もしない
                    break;
            }
        }

        /// <summary>
        /// タスクトレイアイコンシングルクリック処理
        /// トグルオプションONの場合、表示・非表示切り替え＋アクティブ化。それ以外の場合アクテイブ化のみ
        /// </summary>
        private void ToggleRockbarVisibility()
        {
            if (this.Visible)
            {
                // フォーム表示時に左クリックした場合はオプションにより挙動切り替え
                if (_configManager.RockbarSetting.ToggleVisibleTaskTrayIconClick)
                {
                    this.Visible = false;
                }
                else
                {
                    this.Activate();
                }
            }
            else
            {
                // フォーム非表示時に左クリックした場合は必ず表示
                this.Visible = true;
                this.Activate();

                // オプションによりタスクトレイアイコン表示を切り替え
                if (!_configManager.RockbarSetting.ShowTaskTrayIcon)
                {
                    notifyIcon.Visible = false;
                }
            }
        }

        /// <summary>
        /// テレビ番組表を開く処理
        /// </summary>
        private void OpenWebEpgTop()
        {
            // Webリンク使用時のみ
            if (_configManager.RockbarSetting.UseWebLink)
            {
                RockbarUtility.OpenBrowser(_configManager.RockbarSetting.WebEpgUrl);
            }
        }
    }
}
