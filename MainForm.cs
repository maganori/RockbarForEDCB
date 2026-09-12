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
        private bool _canConnect = true;

        // ListViewアイテム作成
        private ListViewBuilder _listViewBuilder;
        private ListViewEventHandler _listViewEventHandler;

        // CtrlCmdUtil
        private CtrlCmdUtil _ctrlCmdUtil = new CtrlCmdUtil();

        // TVTest管理マネージャー
        private TVTestManager _tvtestManager;

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
            _epgDataManager = new EpgDataManager(_ctrlCmdUtil);

            // ListViewBuilderのインスタンス化およびデリゲートの初期化
            _listViewBuilder = new ListViewBuilder(_configManager, _epgDataManager);

            // TVTestManagerのインスタンス化
            _tvtestManager = new TVTestManager(_configManager, _ctrlCmdUtil);

            // ListViewEventHandler のインスタンス化
            _listViewEventHandler = new ListViewEventHandler(
                _configManager,
                _epgDataManager,
                _listViewBuilder,
                _tvtestManager,
                _ctrlCmdUtil,
                listContextMenuStrip,
                RefreshList
            );

            // 設定反映
            ApplySetting();

            if (_configManager.RockbarSetting.UseTcpIp) {
                // TCP/IP通信にする
                _ctrlCmdUtil.SetSendMode(true);
                _ctrlCmdUtil.SetNWSetting(_configManager.RockbarSetting.IpAddress, _configManager.RockbarSetting.PortNumber);
            }
            else
            {
                // Pipe通信にする
                _ctrlCmdUtil.SetSendMode(false);
            }

            // 適当な通信を行って、通信可否を確認し問題があればメッセージを出す
            if (!_epgDataManager.CheckConnection(out ErrCode errCode))
            {
                _canConnect = false;
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

            // 録画済み一覧の最大保持数(表示数)
            _epgDataManager.RecListMaxCount = _configManager.RockbarSetting.RecListMaxCount;
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
            if (_canConnect)
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
            SettingForm settingForm = new SettingForm(_ctrlCmdUtil, _canConnect);
            DialogResult result = settingForm.ShowDialog();
            settingForm.Dispose();

            if (result == DialogResult.OK)
            {
                _configManager.LoadFromFile();
                ApplySetting();
                RefreshList(true, true, true);
            }
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
}
