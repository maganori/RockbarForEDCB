using EpgTimer;
using System;
using System.Collections.Generic;
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

        // EpgTimerSrv通信とEPGデータ管理
        private EpgDataManager _epgDataManager;

        // ListViewアイテム作成
        private ListViewBuilder _listViewBuilder;
        private ListViewEventHandler _listViewEventHandler;

        // CtrlCmdUtil
        //private CtrlCmdUtil _ctrlCmdUtil = new CtrlCmdUtil();

        // SettingForm
        private SettingForm _settingForm = null;

        // TVTestマネージャー
        private TVTestManager _tvtestManager;

        private readonly EdcbConnection _edcbConnection;
        // クラスのフィールド定義（関連変数の名前変更と追加）
        //private CancellationTokenSource _edcbInitCts;


        // マウスのクリック位置を記憶
        private Point _mousePoint;

        // マウスのシングルクリック、ダブルクリック判別用
        private CancellationTokenSource _leftCts;
        private CancellationTokenSource _rightCts;

        // 次回の画面更新が必要になる最早時刻
        private DateTime _nextMainListViewRefreshTime = DateTime.MinValue;
        private DateTime _nextSubListViewRefreshTime = DateTime.MinValue;

        // 録画タブの更新インターバル(前回表示から1分以上経っていると再取得)
        private readonly TimeSpan _recDataUpdateInterval = TimeSpan.FromMinutes(1);

        /// <summary>
        /// コンストラクタ
        /// コンフィグの読み込み・CtrlCmdの初期化・初回表示処理を行う。
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

            _edcbConnection = new EdcbConnection();

            // EpgDataManagerのインスタンス化
            _epgDataManager = new EpgDataManager(_edcbConnection);

            // ListViewBuilderのインスタンス化およびデリゲートの初期化
            _listViewBuilder = new ListViewBuilder(_configManager, _epgDataManager);

            // TVTestManagerのインスタンス化
            _tvtestManager = new TVTestManager(_configManager, _edcbConnection);

            // ListViewEventHandler のインスタンス化
            _listViewEventHandler = new ListViewEventHandler(
                _configManager,
                _epgDataManager,
                _listViewBuilder,
                _tvtestManager,
                _edcbConnection,
                listContextMenuStrip,
                RefreshList
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

            //if (_configManager.RockbarSetting.UseTcpIp) {
            //    // TCP/IP通信にする
            //    _ctrlCmdUtil.SetSendMode(true);
            //    _ctrlCmdUtil.SetNWSetting(_configManager.RockbarSetting.IpAddress, _configManager.RockbarSetting.PortNumber);
            //}
            //else
            //{
            //    // Pipe通信にする
            //    _ctrlCmdUtil.SetSendMode(false);
            //}

            //// 適当な通信を行って、通信可否を確認し問題があればメッセージを出す
            //if (!_epgDataManager.CheckConnection(out ErrCode errCode))
            //{
            //    _canConnect = false;
            //    MessageBox.Show(
            //        $"EpgTimerSrvと接続できません。以降の通信を停止します。\n" +
            //        $"オプション設定を見直してアプリケーションを再起動してください。\n\nErrCode: {errCode}",
            //        "EpgTimerSrv接続チェック失敗",
            //        MessageBoxButtons.OK,
            //        MessageBoxIcon.Information
            //    );
            //}

            //// 初回表示
            RefreshList(false, true, true);

            // 初回接続チェックの呼び出し箇所
            InitializeEdcbConnectionAsync();

            // タイマーを有効化
            timer.Enabled = true;

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
        /// <param name="isTransmission">EpgTimerSrvと通信要否</param>
        /// <param name="forceMainListRefresh">MainListView(Main Pane)の強制リフレッシュ要否</param>
        /// <param name="forceSubListRefresh">SubListtView(Sub Pane)の強制リフレッシュ要否</param>
        private void RefreshList(bool isTransmission, bool forceMainListRefresh, bool forceSubListRefresh)
        {
            DateTime now = DateTime.Now;

            DateTime mainListRefreshTime = _nextMainListViewRefreshTime;
            DateTime subListRefreshTime = _nextSubListViewRefreshTime;

            bool isReserveChanged = false;
            bool isServiceChanged = false;
            bool isTunerChanged = false;
            bool isRecChanged = false;

            // 録画情報更新必要有無判定
            bool isRecDataUpdateRequired = 
                mainFormTabControl.SelectedTab == recTabPage && //録画タブを開いて かつ
                ((isTransmission == true && forceMainListRefresh) || //通信要で強制リフレッシュ要　または
                now - _epgDataManager.LastRecDataUpdateTime >= _recDataUpdateInterval); //前回更新時間から時間経過しているか

            // EpgTimerSrvと通信する
            if (_edcbConnection.CanConnect)
            {
                // 定期更新など、通信が必要な場合
                if (isTransmission)
                {
                    isServiceChanged = _epgDataManager.UpdateServiceData();
                    isReserveChanged = _epgDataManager.UpdateReserveData();
                    isTunerChanged = _epgDataManager.UpdateTunerData();
                }

                // 録画情報の更新が必要な場合
                if (isRecDataUpdateRequired)
                {
                    isRecChanged = _epgDataManager.UpdateRecData();
                }
            }

            // 現在アクティブなタブに応じて描画処理を分岐
            // -----mainListView-----
            // 予約タブ
            if (mainFormTabControl.SelectedTab == reserveTabPage)
            {
                if (forceMainListRefresh || isReserveChanged || (now >= mainListRefreshTime))
                {
                    DateTime nextTime = _listViewBuilder.BuildReserveList(mainListView);
                    _nextMainListViewRefreshTime = nextTime;
                }
            }
            // 録画タブ
            else if (mainFormTabControl.SelectedTab == recTabPage)
            {
                if (forceMainListRefresh || isRecChanged)
                {
                    _listViewBuilder.BuildRecList(mainListView);
                    _nextMainListViewRefreshTime = DateTime.MaxValue;
                }
            }
            // 新番組タブ
            else if (mainFormTabControl.SelectedTab == newProgramTabPage)
            {
                if (forceMainListRefresh || isServiceChanged || isReserveChanged || (now >= mainListRefreshTime))
                {
                    DateTime nextTime = _listViewBuilder.BuildNewProgramList(mainListView);
                    _nextMainListViewRefreshTime = nextTime;
                }
            }
            // チャンネルタブ
            else
            {
                if (forceMainListRefresh || isServiceChanged || isReserveChanged || (now >= mainListRefreshTime))
                {
                    DateTime nextTime = _listViewBuilder.BuildServiceList(mainListView, GetCurrentMainFormTabType());
                    _nextMainListViewRefreshTime = nextTime;
                }
            }

            // -----subListView-----
            // チューナー一覧
            if (forceSubListRefresh || isTunerChanged || (now >= subListRefreshTime))
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
            AdjustAllListViewColumns();
        }

        /// <summary>
        /// EpgTimerSrvへの接続確認と、成功時の初回表示を行う。
        /// </summary>
        private async void InitializeEdcbConnectionAsync()
        {
            // 設定ファイルから通信方式（TCP/IP or パイプ）を読み込む
            var s = _configManager.RockbarSetting;

            // 接続設定の反映と接続確認をまとめて行う。
            // CanConnect の更新や、先発・後発の判断は EdcbConnection の中でしてくれる。
            ErrCode? result = await _edcbConnection.ConnectAsync(s.UseTcpIp, s.IpAddress, s.PortNumber);

            // null は「確認中に新しい呼び出しがあり、この結果は古い」という意味。何もせず終わる
            if (result == null) return;

            // 通信できなかった場合は、メッセージを出して終了
            if (!_edcbConnection.CanConnect)
            {
                MessageBox.Show(
                    $"EpgTimerSrvと接続できません。以降の通信を停止します。\n" +
                    $"オプション設定を見直してください。\n\nErrCode: {result}",
                    "EpgTimerSrv接続チェック失敗",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // 通信できた場合は、データを取得して画面を更新する
            RefreshList(true, true, true);
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
        /// 毎分0秒のEpgTimerSrv通信と、TVTestの起動・終了を行う
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void timer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            if (now.Second == 0)
            {
                RefreshList(true, false, false);
            }

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
            RefreshList(false, true, false);
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
        /// マウスクリック処理に応じてテキストでフィルタリングをする
        /// </summary>
        /// <param name="text">対象テキスト</param>
        private void FilterWith(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            filterTextBox.Text = text;
            filterTextBox.Focus();
            RefreshList(false, true, false);
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

            RefreshList(false, true, false);
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
            RefreshList(false, true, false);
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
                _settingForm = new SettingForm(_configManager,  _edcbConnection);

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
            InitializeEdcbConnectionAsync();
            //RefreshList(true, true, true);
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
        }

        /// <summary>
        /// タスクトレイ終了コンテキストメニュークリック処理
        /// フォームを閉じる。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// タスクトレイ設定コンテキストメニュークリック処理
        /// 設定フォームを開く。
        /// </summary>
        private void openSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSettingForm();
        }

        /// <summary>
        /// タスクトレイ ReadMe(GitHub) コンテキストメニュークリック処理
        /// ReadMe(GitHub)を開く。
        /// </summary>
        private void readMeGitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RockbarUtility.OpenBrowser("https://github.com/maganori/RockbarForEDCB#rockbar-for-edcb");
        }

        /// <summary>
        /// タスクトレイ 更新の確認(GitHub) コンテキストメニュークリック処理
        /// 更新の確認(GitHub)を開く。
        /// </summary>
        private void checkReleaseGitHubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RockbarUtility.OpenBrowser("https://github.com/maganori/RockbarForEDCB/releases");
        }

        /// <summary>
        /// タスクトレイテレビ番組表コンテキストメニュークリック処理
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

    public class EdcbConnection
    {
        // 通信1回あたりのタイムアウト時間（ミリ秒）。接続確認と普段の通信で共通に使う
        private const int TimeoutMs = 10000;

        // 普段の通信用のCtrlCmdUtil
        private readonly CtrlCmdUtil _ctrlCmdUtil = new CtrlCmdUtil();

        // 「EpgTimerSrvと通信できる状態か」を表す
        private volatile bool _canConnect;

        // 「EpgTimerSrvと通信できる状態か」の公開用
        public bool CanConnect => _canConnect;

        // ConnectAsync が何回呼ばれたかを数える変数
        // 呼び出しが重なったときに「どれが最新の呼び出しか」を見分けるために使う
        private volatile int _connectCallCount;

        /// <summary>
        /// CtrlCmdUtilのラッパーメソッド
        /// 使い方の例： ErrCode err = _edcbConnection.Send(c => c.SendEnumReserve(ref list));
        /// CtrlCmdUtil：ErrCode err = _ctrlCmdUtil.SendEnumReserve(ref list);
        /// </summary>
        /// <param name="command">CtrlCmdUtilを使った通信処理（ErrCodeを返すもの）</param>
        /// <returns>通信の結果。通信しなかった場合やタイムアウトの場合も、ErrCodeで返す</returns>
        public ErrCode Send(Func<CtrlCmdUtil, ErrCode> command)
        {
            // 通信できない状態なら、待たずにすぐ失敗として返す
            if (!_canConnect) return ErrCode.CMD_ERR_CONNECT;

            // 通信を始めた時点の「接続カウント」を覚えておく。
            int myGeneration = _connectCallCount;

            // 通信は別のスレッドで実行（CtrlCmdUtilには通信を途中でやめる仕組みがないため）
            Task<ErrCode> task = Task.Run(() =>
            {
                try
                {
                    return command(_ctrlCmdUtil);
                }
                catch (Exception ex)
                {
                    // パイプ通信の失敗や、受信データの不正で例外が出たとき用。
                    System.Diagnostics.Trace.WriteLine(ex);
                    return ErrCode.CMD_ERR_CONNECT;
                }
            });

            // 応答があれば結果を、終わらなければタイムアウトとする
            ErrCode errCode = task.Wait(TimeoutMs) ? task.Result : ErrCode.CMD_ERR_TIMEOUT;

            // 「接続できない」系のエラーだったら、通信できない状態にする。
            if (IsConnectionError(errCode) && myGeneration == _connectCallCount)
            {
                _canConnect = false;
            }

            return errCode;
        }

        /// <summary>
        /// 「サーバーと繋がらない」ことを表すエラーかどうかを判定する。
        /// 忙しいだけ（CMD_ERR_BUSY）や、引数の間違い（CMD_ERR_INVALID_ARG）などは含めない。
        /// </summary>
        private static bool IsConnectionError(ErrCode errCode)
        {
            return errCode == ErrCode.CMD_ERR_CONNECT      // サーバーにコネクトできなかった
                || errCode == ErrCode.CMD_ERR_DISCONNECT   // サーバーから切断された
                || errCode == ErrCode.CMD_ERR_TIMEOUT;     // タイムアウト発生
        }

        /// <summary>
        /// 接続設定を反映し、EpgTimerSrvと通信できるかを確認する。
        /// 何度も呼ばれた場合、有効なのは最後の呼び出しだけ。
        /// </summary>
        /// <returns>
        /// 確認の結果（CMD_SUCCESSなら通信できた）。
        /// 確認中に新しい呼び出しがあって、この結果が古くなった場合は null を返す。
        /// </returns>
        public async Task<ErrCode?> ConnectAsync(bool useTcpIp, string ipAddress, uint portNumber)
        {
            // 呼び出し回数を1増やし、「自分は何回目の呼び出しか」を覚えておく
            int myCallNumber = ++_connectCallCount;

            // 新しい接続設定を使い始めるので、「通信できる」状態をいったん取り消す。
            _canConnect = false;

            // 普段の通信用のCtrlCmdUtilに、接続設定を反映する
            Configure(_ctrlCmdUtil, useTcpIp, ipAddress, portNumber);

            // 接続確認をする（通信中は待つだけで、画面は固まらない）
            ErrCode errCode = await CheckAsync(useTcpIp, ipAddress, portNumber);

            // 待っている間に新しい呼び出しがあった場合、この結果は古いので捨てる。
            // CanConnect は false のままだが、それで正しい（後発の呼び出しが結果を入れてくれる）。
            if (myCallNumber != _connectCallCount) return null;

            // 確認結果を保存する（CMD_SUCCESSなら通信できる）
            _canConnect = (errCode == ErrCode.CMD_SUCCESS);
            return errCode;
        }

        /// <summary>
        /// 渡されたCtrlCmdUtilに、接続設定を反映する。
        /// 「普段用」と「接続確認用」の両方で同じ設定コードを使うため、1つにまとめている。
        /// </summary>
        private static void Configure(CtrlCmdUtil cmd, bool useTcpIp, string ipAddress, uint portNumber)
        {
            // true ならTCP/IP通信、false ならパイプ通信にする
            cmd.SetSendMode(useTcpIp);

            // TCP/IP通信のときだけ、接続先のIPアドレスとポート番号を設定する
            if (useTcpIp)
            {
                cmd.SetNWSetting(ipAddress, portNumber);
            }
        }

        /// <summary>
        /// EpgTimerSrvと通信できるかを確認する
        /// </summary>
        private static async Task<ErrCode> CheckAsync(bool useTcpIp, string ipAddress, uint portNumber)
        {
            // 接続確認用に、新しいCtrlCmdUtilを作る。
            // 普段用と同じものを使うと、他の通信が終わるまで待たされる可能性があるため。
            var testCmd = new CtrlCmdUtil();
            Configure(testCmd, useTcpIp, ipAddress, portNumber);

            // 通信は別スレッド（Task.Run）で開始する
            Task<ErrCode> checkTask = Task.Run(() =>
            {
                try
                {
                    // チューナーごとの予約一覧を取得するだけの軽い通信。
                    // 一覧の中身は使わず、通信が成功したかどうかだけを見る。
                    var tuners = new List<TunerReserveInfo>();
                    return testCmd.SendEnumTunerReserve(ref tuners);
                }
                catch (Exception ex)
                {
                    // パイプ通信は、接続できないときに例外が出ることがある。
                    // アプリが落ちないように、ここで受け止めて「接続失敗」として返す。
                    System.Diagnostics.Trace.WriteLine(ex);
                    return ErrCode.CMD_ERR_CONNECT;
                }
            });

            // 「通信が終わる」か「タイムアウト」かまで待つ
            Task done = await Task.WhenAny(checkTask, Task.Delay(TimeoutMs));

            // 通信のほうが先に終わっていればその結果を、そうでなければタイムアウトを返す
            return done == checkTask ? await checkTask : ErrCode.CMD_ERR_TIMEOUT;
        }
    }
}
