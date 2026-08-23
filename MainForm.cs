using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;
using EpgTimer;
using Nett;

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

        // EpgTimerSrv接続可否
        private bool canConnect = true;

        // ListViewアイテム作成
        private ListViewBuilder _listViewBuilder;
        private ListViewEventHandler _listViewEventHandler;

        // CtrlCmdUtil
        private CtrlCmdUtil ctrlCmdUtil = new CtrlCmdUtil();

        // TVTest管理マネージャー
        private TVTestManager _tvtestManager;

        // マウスのクリック位置を記憶
        private Point mousePoint;

        // マウスのシングルクリック、ダブルクリック判別用
        private CancellationTokenSource _cts;

        // 次回の画面更新が必要になる最早時刻
        private DateTime nextMainListViewRefreshTime = DateTime.MinValue;
        private DateTime nextSubListViewRefreshTime = DateTime.MinValue;

        private readonly TimeSpan dataUpdateInterval = TimeSpan.FromMinutes(1);

        /// <summary>
        /// コンストラクタ
        /// コンフィグの読み込み・CtrlCmdの初期化・初回表示処理を行う。
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            // フォーム表示完了イベント
            this.Shown += MainForm_Shown;

            // フォーカスが外れたときの強調表示反転防止
            mainListView.HideSelection = true;
            subListView.HideSelection = true;

            // ConfigManagerのインスタンス化
            _configManager = new ConfigManager();

            // コンフィグファイルにwidth, height指定時のみ前回位置・サイズ・スプリッタ位置で起動
            if (_configManager.RockbarSetting.Width != 0 && _configManager.RockbarSetting.Height != 0)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(_configManager.RockbarSetting.X, _configManager.RockbarSetting.Y);
                this.Size = new Size(_configManager.RockbarSetting.Width, _configManager.RockbarSetting.Height);
                splitContainer.SplitterDistance = _configManager.RockbarSetting.SplitterDistance;
            }

            // EpgDataManagerのインスタンス化
            _epgDataManager = new EpgDataManager(this.ctrlCmdUtil);

            // ListViewBuilderのインスタンス化およびデリゲートの初期化
            _listViewBuilder = new ListViewBuilder(_configManager, _epgDataManager);

            // TVTestManagerのインスタンス化
            _tvtestManager = new TVTestManager(_configManager.RockbarSetting, ctrlCmdUtil);

            // ListViewEventHandler のインスタンス化
            _listViewEventHandler = new ListViewEventHandler(
                _configManager,
                _epgDataManager,
                _listViewBuilder,
                _tvtestManager,
                ctrlCmdUtil,
                listContextMenuStrip,
                RefreshList
            );

            // 設定反映
            applySetting();

            if (_configManager.RockbarSetting.UseTcpIp) {
                // TCP/IP通信にする
                ctrlCmdUtil.SetSendMode(true);
                ctrlCmdUtil.SetNWSetting(_configManager.RockbarSetting.IpAddress, _configManager.RockbarSetting.PortNumber);
            }
            else
            {
                // Pipe通信にする
                ctrlCmdUtil.SetSendMode(false);
            }

            // 適当な通信を行って、通信可否を確認し問題があればメッセージを出す
            if (!_epgDataManager.CheckConnection(out ErrCode errCode))
            {
                canConnect = false;
                MessageBox.Show(
                    $"EpgTimerSrvと接続できません。以降の通信を停止します。\nオプション設定を見直してアプリケーションを再起動してください。\n\nErrCode: {errCode}",
                    "EpgTimerSrv接続チェック失敗",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }

            // 初回表示
            RefreshList(true, true, true);

            // タイマーを有効化
            timer.Enabled = true;
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
        /// 設定反映処理
        /// 主に見た目部分の設定をフォームに反映する。初回起動時・設定変更時に実行
        /// </summary>
        private void applySetting()
        {
            // タスクトレイアイコン常時表示
            if (_configManager.RockbarSetting.ShowTaskTrayIcon)
            {
                notifyIcon.Visible = true;
            }
            else
            {
                // 初回起動時・設定画面からの戻りで格納状態はないはず
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
            Font font = (Font) fontConverter.ConvertFromString(_configManager.RockbarSetting.Font);
            Font menuFont = (Font)fontConverter.ConvertFromString(_configManager.RockbarSetting.MenuFont);
            Font tabFont = (Font)fontConverter.ConvertFromString(_configManager.RockbarSetting.TabFont);
            Font buttonFont = (Font)fontConverter.ConvertFromString(_configManager.RockbarSetting.ButtonFont);
            Font labelFont = (Font)fontConverter.ConvertFromString(_configManager.RockbarSetting.LabelFont);
            Font textBoxFont = (Font)fontConverter.ConvertFromString(_configManager.RockbarSetting.TextBoxFont);

            mainListView.Font = font;
            subListView.Font = font;
            listContextMenuStrip.Font = menuFont;
            mainFormTabControl.Font = tabFont;
            resetButton.Font = buttonFont;
            closeButton.Font = buttonFont;
            filterTextBox.Font = textBoxFont;
            settingButton.Font = buttonFont;

            // 色
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
            this.BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.FormBackColor);

            mainListView.BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.ListBackColor);
            mainListView.ForeColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.ForeColor);

            subListView.BackColor = mainListView.BackColor;
            subListView.ForeColor = mainListView.ForeColor;

            listContextMenuStrip.BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.MenuBackColor);

            // ListViewBuilderクラスに設定適用
            _listViewBuilder.ApplySettings();

            // Web番組表機能を使用するときのみタスクトレイアイコンの右クリックメニューに「テレビ番組表」を表示
            this.openWebEpgTopToolStripMenuItem.Visible = _configManager.RockbarSetting.UseWebLink;

            // 録画済み一覧の最大表示数
            _epgDataManager.RecListMaxCount = this._configManager.RockbarSetting.RecListMaxCount;
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

            DateTime mainListRefreshTime = nextMainListViewRefreshTime;
            DateTime subListRefreshTime = nextSubListViewRefreshTime;

            bool isReserveChanged = false;
            bool isServiceChanged = false;
            bool isTunerChanged = false;
            bool isRecChanged = false;

            // 録画情報更新必要有無判定
            bool isRecDataUpdateRequired = 
                mainFormTabControl.SelectedTab == recTabPage && //録画タブを開いているか
                DateTime.Now - _epgDataManager.LastRecDataUpdateTime >= dataUpdateInterval; //前回更新時間から時間経過しているか

            // EpgTimerSrvと通信する
            if (canConnect)
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
                    this.nextMainListViewRefreshTime = nextTime;
                }
            }
            // 録画タブ
            else if (mainFormTabControl.SelectedTab == recTabPage)
            {
                if (forceMainListRefresh || isRecChanged)
                {
                    _listViewBuilder.BuildRecList(mainListView);
                    this.nextMainListViewRefreshTime = DateTime.MaxValue;
                }
            }
            // 新番組タブ
            else if (mainFormTabControl.SelectedTab == newProgramTabPage)
            {
                if (forceMainListRefresh || isServiceChanged || isReserveChanged || (now >= mainListRefreshTime))
                {
                    DateTime nextTime = _listViewBuilder.BuildNewProgramList(mainListView);
                    this.nextMainListViewRefreshTime = nextTime;
                }
            }
            // チャンネルタブ
            else
            {
                if (forceMainListRefresh || isServiceChanged || isReserveChanged || (now >= mainListRefreshTime))
                {
                    DateTime nextTime = _listViewBuilder.BuildServiceList(mainListView, GetCurrentMainFormTabType());
                    this.nextMainListViewRefreshTime = nextTime;
                }
            }

            // -----subListView-----
            // チューナー一覧
            if (forceSubListRefresh || isTunerChanged || (now >= subListRefreshTime))
            {
                DateTime nextTime = _listViewBuilder.BuildTunerList(subListView);
                this.nextSubListViewRefreshTime = nextTime;
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
        /// フィルタのリセット処理
        /// フィルタ文字列をクリアしフィルタ結果をリセットする。
        /// </summary>
        private void ResetFilter()
        {
            filterTextBox.Clear();
            RefreshList(false, true, false);
        }

        /// <summary>
        /// マウスクリック処理に応じてテキストでフィルタリングをする
        /// </summary>
        /// <param name="text">対象テキスト</param>
        private void filterWith(string text)
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
        /// mainListViewマウスクリック時処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void mainListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (mainFormTabControl.SelectedTab == reserveTabPage)
            {
                _listViewEventHandler.HandleReserveClick(sender, e, mainListView);
                return;
            }
            else if (mainFormTabControl.SelectedTab == recTabPage)
            {
                _listViewEventHandler.HandleRecClick(sender, e, mainListView);
                return;
            }
            else if (mainFormTabControl.SelectedTab == newProgramTabPage)
            {
                _listViewEventHandler.HandleNewProgramClick(sender, e, mainListView);
                return;
            }
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
            if (mainFormTabControl.SelectedTab == reserveTabPage)
            {
                _listViewEventHandler.HandleReserveMouseUp(sender, e, mainListView);
                return;
            }
        }

        /// <summary>
        /// mainListViewマウスダブルクリック処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void mainListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (mainFormTabControl.SelectedTab == reserveTabPage)
            {
                _listViewEventHandler.HandleReserveDoubleClick(sender, e, mainListView);
                return;
            }
            else if (mainFormTabControl.SelectedTab == recTabPage)
            {
                _listViewEventHandler.HandleRecDoubleClick(sender, e, mainListView);
                return;
            }
            else if (mainFormTabControl.SelectedTab == newProgramTabPage)
            {
                _listViewEventHandler.HandleNewProgramDoubleClick(sender, e, mainListView);
                return;
            }
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
        /// タイマー処理
        /// 毎分0秒のEpgTimerSrv通信と、TVTestの起動・終了を行う
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void timer_Tick(object sender, EventArgs e)
        {
            DateTime timerTime = DateTime.Now;

            if (timerTime.Second == 0) {
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
                mousePoint = new Point(e.X, e.Y);
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
                this.Left += e.X - mousePoint.X;
                this.Top += e.Y - mousePoint.Y;
            }
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
        /// リセットボタン押下処理
        /// フィルタ文字列をクリアしフィルタ結果をリセットする。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void resetButton_Click(object sender, EventArgs e)
        {
            ResetFilter();
        }

        /// <summary>
        /// 設定ボタン押下処理
        /// 設定フォームを開き、設定変更があった場合は設定を再読込して画面をリフレッシュする。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void settingButton_Click(object sender, EventArgs e)
        {
            SettingForm settingForm = new SettingForm(ctrlCmdUtil, canConnect);
            DialogResult result = settingForm.ShowDialog();
            settingForm.Dispose();

            if (result == DialogResult.OK)
            {
                _configManager.LoadFromFile();
                applySetting();
                RefreshList(true, true, true);
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

            _configManager.SaveFromFile();
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
            // 左クリック以外は無視
            if (e.Button != MouseButtons.Left) return;

            // 既に待機中のクリックがある場合（＝ダブルクリック成立）
            if (_cts != null)
            {
                // 1回目のシングルクリック待ち（Task.Delay）をキャンセルする
                _cts.Cancel();

                // ダブルクリック処理を実行
                Debug.WriteLine("★ダブルクリック実行");
                notifyIconDoubleClickAction();
                return;
            }

            // --- 1回目のクリック処理 ---
            _cts = new CancellationTokenSource();

            try
            {
                // OS設定のダブルクリック時間だけ非同期で待機
                await Task.Delay(SystemInformation.DoubleClickTime, _cts.Token);

                // 時間内にキャンセルされなかった場合のみシングルクリック実行
                Debug.WriteLine("★シングルクリック実行");
                notifyIconSingleClickAction();
            }
            catch (OperationCanceledException)
            {
                // ダブルクリックによってキャンセルされた場合はここを通る（正常動作なので無視）
                Debug.WriteLine("シングルクリック待機がキャンセルされました");
            }
            catch (Exception ex)
            {
                // その他の予期せぬエラー用ログ
                Debug.WriteLine($"エラーが発生しました: {ex.Message}");
            }
            finally
            {
                // ダブルクリックの場合もここで後処理
                _cts?.Dispose();
                _cts = null;
            }
        }

        /// <summary>
        /// タスクトレイアイコンシングルクリック処理
        /// トグルオプションONの場合、表示・非表示切り替え＋アクティブ化。それ以外の場合アクテイブ化のみ
        /// </summary>
        private void notifyIconSingleClickAction()
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
        /// タスクトレイアイコンダブルクリック処理
        /// テレビ番組表を開く
        /// </summary>
        private void notifyIconDoubleClickAction()
        {
            OpenWebEpgTop();
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

        /// <summary>
        /// フォームサイズ変更処理
        /// 左右のリストビューのカラム幅を調整する。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (_listViewBuilder != null)
            {
                _listViewBuilder.AdjustListViewColumns(mainListView);
                _listViewBuilder.AdjustListViewColumns(subListView);
            }
        }

        /// <summary>
        /// スプリッタ位置調整処理
        /// 左右のリストビューのカラム幅を調整する。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (_listViewBuilder != null)
            {
                _listViewBuilder.AdjustListViewColumns(mainListView);
                _listViewBuilder.AdjustListViewColumns(subListView);
            }
        }

        /// <summary>
        /// フォーム初回表示完了時処理
        /// 実際の画面描画サイズ確定後にリストビューの列幅を再調整して水平スクロールバーを防ぐ。
        /// </summary>
        private void MainForm_Shown(object sender, EventArgs e)
        {
            if (_listViewBuilder != null)
            {
                _listViewBuilder.AdjustListViewColumns(mainListView);
                _listViewBuilder.AdjustListViewColumns(subListView);
            }
        }

    }
}
