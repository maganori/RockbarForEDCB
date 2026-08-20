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

        // CtrlCmdUtil
        private CtrlCmdUtil ctrlCmdUtil = new CtrlCmdUtil();

        // TVTest管理マネージャー
        private TVTestManager _tvtestManager;

        // マウスのクリック位置を記憶
        private Point mousePoint;

        // マウスのシングルクリック、ダブルクリック判別用
        private CancellationTokenSource _cts;

        // 色の設定情報(設定情報から変数にロードしたもの)
        private Color formBackColor;
        private Color listBackColor;
        private Color foreColor;

        private Color menuBackColor;
        private Color okReserveMenuBackColor;
        private Color partialReserveMenuBackColor;
        private Color ngReserveMenuBackColor;
        private Color disabledReserveMenuBackColor;

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
            _listViewBuilder = new ListViewBuilder(_configManager, _epgDataManager)
            {
                CreateRecInfoTooltipTexts = this.createRecInfoTooltipTexts
            };

            // TVTestManagerの初期化
            _tvtestManager = new TVTestManager(_configManager.RockbarSetting, ctrlCmdUtil);

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
            this.formBackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.FormBackColor);
            this.listBackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.ListBackColor);
            this.foreColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.ForeColor);

            this.BackColor = this.formBackColor;

            this.menuBackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.MenuBackColor);
            this.okReserveMenuBackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.OkReserveMenuBackColor);
            this.partialReserveMenuBackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.PartialReserveMenuBackColor);
            this.ngReserveMenuBackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.NgReserveMenuBackColor);
            this.disabledReserveMenuBackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.DisabledReserveMenuBackColor);

            mainListView.BackColor = this.listBackColor;
            mainListView.ForeColor = this.foreColor;

            subListView.BackColor = this.listBackColor;
            subListView.ForeColor = this.foreColor;

            listContextMenuStrip.BackColor = this.menuBackColor;

            // ListViewBuilderクラスに設定適用
            _listViewBuilder.ApplySettings();

            // Web番組表機能を使用するときのみタスクトレイアイコンの右クリックメニューに「テレビ番組表」を表示
            this.openWebEPGToolStripMenuItem.Visible = _configManager.RockbarSetting.UseWebLink;

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
        /// 録画モード有効・無効の切り替え処理
        /// 録画有効時は無効化、無効時は有効化する。
        /// </summary>
        private void ToggleRecMode(ReserveData reserve)
        {
            if (reserve.RecSetting.IsNoRec())
            {
                reserve.RecSetting.RecMode = _configManager.RockbarSetting.FixNoRecToServiceOnly ? (byte)1 : reserve.RecSetting.GetRecMode();
            }
            else
            {
                // 録画モード情報を維持して無効化
                var recMode = reserve.RecSetting.RecMode;
                reserve.RecSetting.RecMode = (byte)(_configManager.RockbarSetting.FixNoRecToServiceOnly ? 5 : 5 + (recMode + 4) % 5);
            }

            var err = ctrlCmdUtil.SendChgReserve(new List<ReserveData>() { reserve });
            if (err != ErrCode.CMD_SUCCESS)
            {
                MessageBox.Show("予約変更でエラーが発生しました。", "予約変更エラー");
            }
            RefreshList(true, false, false);
        }

        /// <summary>
        /// 番組の右クリックコンテキストメニューItem作成処理
        /// 番組情報・予約情報からチャンネル一覧・チューナー一覧用のコンテキストメニューitemを作成する。
        /// </summary>
        /// <param name="ev">番組情報</param>
        /// <param name="reserve">予約情報</param>
        /// <param name="isTuner">チューナー一覧用？</param>
        /// <returns></returns>
        private ToolStripMenuItem createEventToolStripMenuItem(EpgEventInfo ev, ReserveData reserve, bool isTuner)
        {
            ReserveStatus reserveStatus = ReserveStatus.NONE;

            // 予約ステータスを判別
            if (reserve != null)
            {
                if (reserve.RecSetting.IsNoRec())
                {
                    reserveStatus = ReserveStatus.DISABLED;
                }
                else if (reserve.OverlapMode == 0)
                {
                    if (ev == null)
                    {
                        reserveStatus = ReserveStatus.DISAPPEARED;
                    }
                    else
                    {
                        reserveStatus = ReserveStatus.OK;
                    }
                }
                else if (reserve.OverlapMode == 1)
                {
                    reserveStatus = ReserveStatus.PARTIAL;
                }
                else
                {
                    reserveStatus = ReserveStatus.NG;
                }
            }

            ToolStripMenuItem item = new ToolStripMenuItem();

            // 録画ステータスに異常があれば色を変える
            switch (reserveStatus)
            {
                case ReserveStatus.OK:
                    if (!isTuner)
                    {
                        item.BackColor = this.okReserveMenuBackColor;
                    }
                    break;
                case ReserveStatus.PARTIAL:
                    item.BackColor = this.partialReserveMenuBackColor;
                    break;
                case ReserveStatus.NG:
                    item.BackColor = this.ngReserveMenuBackColor;
                    break;
                case ReserveStatus.DISABLED:
                    item.BackColor = this.disabledReserveMenuBackColor;
                    break;
            }

            string reserveString = RockbarUtility.GetReserveStatusString(reserveStatus);

            if (isTuner)
            {
                // --- チャンネル名の取得 ---
                // 設定ファイルのチャンネル名を最優先で使用し、設定がなければEDCBのStationNameを使用
                string serviceName = GetServiceName(reserve.TransportStreamID, reserve.ServiceID);

                // チューナーから開いた場合は予約情報を表示(必ず予約情報あり)
                item.Text = reserve.StartTime.ToString("MM/dd(ddd) HH:mm") + "～" + reserve.StartTime.AddSeconds(reserve.DurationSecond).ToString("HH:mm") + "  " +
                    reserveString + "    " + serviceName + "    " + reserve.Title;
            }
            else
            {
                // サービスから開いた場合は番組情報を表示(必ず番組情報あり)
                item.Text = ev.start_time.ToString("MM/dd(ddd) HH:mm") + "～" + ev.start_time.AddSeconds(ev.durationSec).ToString("HH:mm") + "  " +
                    reserveString + "  " + ev.ShortInfo?.event_name;
            }

            // 番組情報がある場合はサブメニューに番組情報を追加
            if (ev != null)
            {
                // Webリンク使用時のみ1行目にWebリンク用のボタンを表示
                if (_configManager.RockbarSetting.UseWebLink)
                {
                    item.DropDownItems.Add(">>");
                    item.DropDownItems[item.DropDownItems.Count - 1].Click += (s2, e2) => accessWebUrl(ev);

                    item.DropDownItems.Add(new ToolStripSeparator());
                }

                // 基本情報をサブメニューに追加
                var shortStrs = RockbarUtility.BreakString(ev.ShortInfo?.text_char);

                if (shortStrs != null)
                {
                    foreach (string str in shortStrs)
                    {
                        item.DropDownItems.Add(str);
                        item.DropDownItems[item.DropDownItems.Count - 1].Enabled = false;
                    }
                }

                item.DropDownItems.Add(new ToolStripSeparator());

                // 拡張情報をサブメニューに追加
                var longStrs = RockbarUtility.BreakString(ev.ExtInfo?.text_char);

                if (longStrs != null)
                {
                    foreach (string str in longStrs)
                    {
                        item.DropDownItems.Add(str);
                        item.DropDownItems[item.DropDownItems.Count - 1].Enabled = false;
                    }
                }
            }

            return item;
        }

        /// <summary>
        /// 録画済み情報のテキスト生成処理
        /// </summary>
        /// <param name="recFile">録画済み情報</param>
        private List<List<RecInfoDetailText>> createRecInfoDetailTexts(RecFileInfo recFile)
        {
            List<List<RecInfoDetailText>> detailTexts = new List<List<RecInfoDetailText>>();
            {
                var text = $"{recFile.StartTime.ToString("yyyy/MM/dd(ddd) HH:mm")}～{recFile.StartTime.AddSeconds(recFile.DurationSecond).ToString("HH:mm")}";
                var dateTimes = new List<RecInfoDetailText>()
                    {
                        new RecInfoDetailText { Value = text, CopyText = text },
                    };
                detailTexts.Add(dateTimes);
            }

            // --- チャンネル名の取得 ---
            // 設定ファイルのチャンネル名を最優先で使用し、設定がなければEDCBのServiceNameを使用
            string serviceName = GetServiceName(recFile.TransportStreamID, recFile.ServiceID);

            var services = new List<RecInfoDetailText>()
                {
                    new RecInfoDetailText { Value = serviceName, CopyText = serviceName },
                    new RecInfoDetailText { Value = recFile.Title, CopyText = recFile.Title },
                };
            detailTexts.Add(services);

            {
                var resultText = $"結果 : {recFile.Comment}";
                var results = new List<RecInfoDetailText>()
                    {
                        new RecInfoDetailText { Value = resultText, CopyText = recFile.Comment },
                    };
                var pathTexts = RockbarUtility.BreakString($"録画ファイル : {recFile.RecFilePath}", 50);
                results.AddRange(pathTexts.Select(t => new RecInfoDetailText { Value = t, CopyText = recFile.RecFilePath }));
                detailTexts.Add(results);
            }

            {
                var copyText = $"{recFile.OriginalNetworkID} (0x{recFile.OriginalNetworkID.ToString("X4")})";
                var value = $"OriginalNetworkID : {copyText}";
                var ids = new List<RecInfoDetailText>()
                    {
                        new RecInfoDetailText { Value = value, CopyText = copyText },
                    };

                copyText = $"{recFile.TransportStreamID} (0x{recFile.TransportStreamID.ToString("X4")})";
                value = $"TransportStreamID : {copyText}";
                ids.Add(new RecInfoDetailText { Value = value, CopyText = copyText });

                copyText = $"{recFile.ServiceID} (0x{recFile.ServiceID.ToString("X4")})";
                value = $"ServiceID : {copyText}";
                ids.Add(new RecInfoDetailText { Value = value, CopyText = copyText });

                copyText = $"{recFile.EventID} (0x{recFile.EventID.ToString("X4")})";
                value = $"EventID : {copyText}";
                ids.Add(new RecInfoDetailText { Value = value, CopyText = copyText });
                detailTexts.Add(ids);
            }

            var scrambles = new List<RecInfoDetailText>()
                {
                    new RecInfoDetailText { Value = $"Drop : {recFile.Drops}", CopyText = recFile.Drops.ToString() },
                    new RecInfoDetailText { Value = $"Scramble : {recFile.Scrambles}", CopyText = recFile.Scrambles.ToString() },
                };
            detailTexts.Add(scrambles);

            return detailTexts;
        }

        /// <summary>
        /// 録画済み情報のツールチップテキスト生成処理
        /// </summary>
        /// <param name="recFile">録画済み情報</param>
        private string createRecInfoTooltipTexts(RecFileInfo recFile)
        {
            var detailTexts = createRecInfoDetailTexts(recFile);
            return string.Join("\n\n", detailTexts.Select(d => string.Join("\n", d.Select(t => t.Value))));
        }

        /// <summary>
        /// Web番組詳細にアクセスする
        /// </summary>
        /// <param name="ev">番組情報</param>
        private void accessWebUrl(EpgEventInfo ev)
        {
            string url = _configManager.RockbarSetting.WebLinkUrl;
            url = url.Replace("{ONID}", ev.original_network_id.ToString());
            url = url.Replace("{TSID}", ev.transport_stream_id.ToString());
            url = url.Replace("{SID}", ev.service_id.ToString());
            url = url.Replace("{EID}", ev.event_id.ToString());

            try
            {
                // URLがパースできるかどうかを事前判定しておく
                new Uri(url);

                var startInfo = new System.Diagnostics.ProcessStartInfo(url);
                startInfo.UseShellExecute = true;
                System.Diagnostics.Process.Start(startInfo);
            }
            catch
            {
                MessageBox.Show($"Web番組詳細URLが不正です。Web番組詳細URLの設定を見直してください。\nURL: {url}", "ブラウザ起動エラー");
                return;
            }
        }

        /// <summary>
        /// Web番組詳細にアクセスする
        /// </summary>
        /// <param name="recFile">録画済み情報</param>
        private void accessWebUrl(RecFileInfo recFile)
        {
            string url = _configManager.RockbarSetting.RecInfoWebLinkUrl;
            url = url.Replace("{RecID}", recFile.ID.ToString());

            try
            {
                // URLがパースできるかどうかを事前判定しておく
                new Uri(url);

                var startInfo = new System.Diagnostics.ProcessStartInfo(url);
                startInfo.UseShellExecute = true;
                System.Diagnostics.Process.Start(startInfo);
            }
            catch
            {
                MessageBox.Show($"Web番組詳細URLが不正です。Web番組詳細URLの設定を見直してください。\nURL: {url}", "ブラウザ起動エラー");
                return;
            }
        }

        /// <summary>
        /// マウスクリック処理に応じてテキストのコピーをする
        /// </summary>
        /// <param name="text">対象テキスト</param>
        private void copyText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            Clipboard.SetText(text);
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
                reserveListView_MouseClick(sender, e, mainListView);
                return;
            }
            else if (mainFormTabControl.SelectedTab == recTabPage)
            {
                recListView_MouseClick(sender, e, mainListView);
                return;
            }
            else if (mainFormTabControl.SelectedTab == newProgramTabPage)
            {
                newProgramListView_MouseClick(sender, e, mainListView);
                return;
            }
            else
            {
                serviceListView_MouseClick(sender, e, mainListView);
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
                reserveListView_MouseUp(sender, e, mainListView);
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
                reserveListView_MouseDoubleClick(sender, e, mainListView);
                return;
            }
            else if (mainFormTabControl.SelectedTab == recTabPage)
            {
                recListView_MouseDoubleClick(sender, e, mainListView);
                return;
            }
            else if (mainFormTabControl.SelectedTab == newProgramTabPage)
            {
                newProgramListView_MouseDoubleClick(sender, e, mainListView);
                return;
            }
            else
            {
                serviceListView_MouseDoubleClick(sender, e, mainListView);
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
        /// チャンネル一覧マウスクリック時処理
        /// 右クリック時、直近30件の番組情報をコンテキストメニューに表示。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        private void serviceListView_MouseClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            // 右クリック
            // 現在の番組を含め、今後の番組を30件までコンテキストメニューで表示(TVRockの仕様踏襲)
            if (e.Button == MouseButtons.Right)
            {
                // クリックした箇所が自動選択されるので拾う
                var selected = targetListView.SelectedItems[0];

                listContextMenuStrip.Items.Clear();

                EpgServiceEventInfo sv = null;
                _epgDataManager.ServiceMap.TryGetValue(selected.Name, out sv);

                // EPG未取得チャンネルは処理を抜ける
                if (sv == null)
                {
                    return;
                }

                var afterEventList = sv.eventList.FindAll(x => x.start_time.AddSeconds(x.durationSec) >= DateTime.Now).OrderBy(a => a.start_time);

                int i = 0;

                foreach (var ev in afterEventList)
                {
                    string eventKey = RockbarUtility.GetKey(ev.transport_stream_id, ev.service_id, ev.event_id);

                    ReserveData reserveData = null;

                    if (_epgDataManager.ReserveMap.ContainsKey(eventKey))
                    {
                        reserveData = _epgDataManager.ReserveMap[eventKey];
                    }

                    listContextMenuStrip.Items.Add(createEventToolStripMenuItem(ev, reserveData, false));

                    i++;

                    if (i >= 30)
                    {
                        break;
                    }
                }

                listContextMenuStrip.Show((Control)sender, new Point(0, e.Y));
            }
        }

        /// <summary>
        /// serviceListViewマウスダブルクリック処理
        /// TVTest使用オプションがONの場合、カーソル箇所の番組を対象にTVTestを起動する。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        private void serviceListView_MouseDoubleClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            // TVTest使用時のみ
            if (!_configManager.RockbarSetting.UseDoubleClickTvtest)
            {
                return;
            }

            // 左ダブルクリック
            // TVTestを起動する
            if (e.Button == MouseButtons.Left)
            {
                // クリックした箇所が自動選択されるので拾う
                var selected = targetListView.SelectedItems[0];

                Service service = selected.Tag as Service;
                EpgServiceEventInfo sv = null;
                _epgDataManager.ServiceMap.TryGetValue(selected.Name, out sv);

                if (service == null)
                {
                    return;
                }

                uint tsid = uint.Parse(service.Tsid);
                uint sid = uint.Parse(service.Sid);

                NetworkType networkType = sv != null
                    ? RockbarUtility.GetNetworkType(service.TypeName, sv.serviceInfo.ONID)
                    : RockbarUtility.GetNetworkType(service.TypeName, null);

                _tvtestManager.StartTVTest(networkType, tsid, sid, service.TvtestOption);
            }
        }

        /// <summary>
        /// 予約一覧マウスクリック処理
        /// 右クリック時、対象の番組情報をコンテキストメニューに表示。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        private void reserveListView_MouseClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            var selectedCount = targetListView.SelectedItems.Count;
            if (selectedCount == 0)
            {
                return;
            }

            // 右クリック
            // 対象の番組情報をコンテキストメニューで表示
            if (e.Button == MouseButtons.Right)
            {
                listContextMenuStrip.Items.Clear();

                try
                {
                    // クリックした箇所が自動選択されるので拾う
                    var selected = targetListView.SelectedItems[0];

                    EpgEventInfo ev = _epgDataManager.AllEventMap[selected.Name];

                    // Webリンク使用時のみ1行目にWebリンク用のボタンを表示
                    if (_configManager.RockbarSetting.UseWebLink)
                    {
                        var item = listContextMenuStrip.Items.Add(">>");
                        item.Click += (s2, e2) => accessWebUrl(ev);

                        listContextMenuStrip.Items.Add(new ToolStripSeparator());
                    }

                    // 有効化・無効化を追加
                    var hasData = _epgDataManager.ReserveMap.TryGetValue(selected.Name, out var reserveData);
                    if (hasData)
                    {
                        var item = listContextMenuStrip.Items.Add(reserveData.RecSetting.IsNoRec() ? "録画を有効にする" : "録画を無効にする");
                        item.Click += (s2, e2) => ToggleRecMode(reserveData);
                        listContextMenuStrip.Items.Add(new ToolStripSeparator());
                    }

                    var dateTime = $"{ev.start_time.ToString("yyyy/MM/dd(ddd) HH:mm")}～{ev.start_time.AddSeconds(ev.durationSec).ToString("HH:mm")}";
                    var dateItem = listContextMenuStrip.Items.Add(dateTime);
                    dateItem.Click += (s2, e2) => copyText(dateTime);
                    listContextMenuStrip.Items.Add(new ToolStripSeparator());

                    // 予約情報をメニューに追加
                    if (hasData)
                    {
                        const string copyCommandText = "テキストをコピー";
                        const string copyCommandTooltipText = "クリックでテキストをコピー";
                        const string filterCommandText = "フィルタリング";

                        // --- チャンネル名の取得 ---
                        // 設定ファイルのチャンネル名を最優先で使用し、設定がなければEDCBのStationNameを使用
                        string serviceName = GetServiceName(reserveData.TransportStreamID, reserveData.ServiceID);

                        // サービス名を追加
                        if (!string.IsNullOrEmpty(serviceName))
                        {
                            var item = new ToolStripMenuItem(serviceName);
                            listContextMenuStrip.Items.Add(item);
                            var subItem = item.DropDownItems.Add(copyCommandText);
                            subItem.Click += (s2, e2) => copyText(serviceName);
                            subItem = item.DropDownItems.Add(filterCommandText);
                            subItem.Click += (s2, e2) => filterWith(serviceName);
                        }
                        // 予約番組名を追加
                        if (!string.IsNullOrEmpty(reserveData.Title))
                        {
                            var item = new ToolStripMenuItem(reserveData.Title);
                            listContextMenuStrip.Items.Add(item);
                            var subItem = item.DropDownItems.Add(copyCommandText);
                            subItem.Click += (s2, e2) => copyText(reserveData.Title);
                            subItem = item.DropDownItems.Add(filterCommandText);
                            subItem.Click += (s2, e2) => filterWith(reserveData.Title);
                            listContextMenuStrip.Items.Add(new ToolStripSeparator());
                        }
                        // 予約コメントを追加
                        if (!string.IsNullOrEmpty(reserveData.Comment))
                        {
                            var item = listContextMenuStrip.Items.Add(reserveData.Comment);
                            item.Click += (s2, e2) => copyText(reserveData.Comment);
                            item.ToolTipText = copyCommandTooltipText;
                            listContextMenuStrip.Items.Add(new ToolStripSeparator());
                        }
                        // 録画予定ファイル名を追加
                        if (reserveData.RecSetting.RecFolderList.Count > 0)
                        {
                            var recFolderList = reserveData.RecSetting.RecFolderList;
                            foreach (var recInfo in recFolderList)
                            {
                                if (!string.IsNullOrEmpty(recInfo.RecFolder))
                                {
                                    var item = listContextMenuStrip.Items.Add(recInfo.RecFolder);
                                    item.Click += (s2, e2) => copyText(recInfo.RecFolder);
                                    item.ToolTipText = copyCommandTooltipText;
                                }
                            }
                            foreach (var fileName in reserveData.RecFileNameList)
                            {
                                if (!string.IsNullOrEmpty(fileName))
                                {
                                    var item = listContextMenuStrip.Items.Add(fileName);
                                    item.Click += (s2, e2) => copyText(fileName);
                                    item.ToolTipText = copyCommandTooltipText;
                                }
                            }
                            listContextMenuStrip.Items.Add(new ToolStripSeparator());
                        }
                    }

                    // 基本情報をメニューに追加
                    var shortStrs = RockbarUtility.BreakString(ev.ShortInfo?.text_char);

                    if (shortStrs != null)
                    {
                        foreach (string str in shortStrs)
                        {
                            var item = listContextMenuStrip.Items.Add(str);
                            item.Enabled = false;
                        }
                    }

                    listContextMenuStrip.Items.Add(new ToolStripSeparator());

                    // 拡張情報をメニューに追加
                    var longStrs = RockbarUtility.BreakString(ev.ExtInfo?.text_char);

                    if (longStrs != null)
                    {
                        foreach (string str in longStrs)
                        {
                            var item = listContextMenuStrip.Items.Add(str);
                            item.Enabled = false;
                        }
                    }
                }
                catch
                {
                    listContextMenuStrip.Items.Add("番組情報を取得できませんでした");
                }

                listContextMenuStrip.Show((Control)sender, new Point(e.X, e.Y));
            }
        }

        /// <summary>
        /// 予約一覧マウスアップ処理
        /// 中央クリック時、対象の予約情報の録画有効・無効を切り替える。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        private void reserveListView_MouseUp(object sender, MouseEventArgs e, ListView targetListView)
        {
            // 中央クリック
            // 予約情報の録画有効・無効を切り替える
            if (e.Button == MouseButtons.Middle)
            {
                // クリックした箇所にある項目を取得する
                var selected = targetListView.GetItemAt(e.Location.X, e.Location.Y);

                if (selected != null && _epgDataManager.ReserveMap.TryGetValue(selected.Name, out var reserve))
                {
                    ToggleRecMode(reserve);
                }
            }
        }

        /// <summary>
        /// 予約一覧マウスダブルクリック処理
        /// Web LinkオプションがONの場合、カーソル箇所の番組のWeb番組情報にアクセスする。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        private void reserveListView_MouseDoubleClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            // Web番組詳細使用時のみ
            if (!_configManager.RockbarSetting.UseWebLink)
            {
                return;
            }

            var selectedCount = targetListView.SelectedItems.Count;
            if (selectedCount == 0)
            {
                return;
            }

            // 左ダブルクリック
            // Web番組詳細にアクセスする
            if (e.Button == MouseButtons.Left)
            {
                try
                {
                    // クリックした箇所が自動選択されるので拾う
                    var selected = targetListView.SelectedItems[0];

                    EpgEventInfo ev = _epgDataManager.AllEventMap[selected.Name];
                    accessWebUrl(ev);
                }
                catch
                {
                    MessageBox.Show("番組情報を取得できませんでした。", "ブラウザ起動エラー");
                    return;
                }
            }
        }

        /// <summary>
        /// 録画済み情報マウスクリック処理
        /// 右クリック時、対象の番組情報をコンテキストメニューに表示。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        private void recListView_MouseClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            var selectedCount = targetListView.SelectedItems.Count;
            if (selectedCount == 0)
            {
                return;
            }

            // 右クリック
            // 対象の番組情報をコンテキストメニューで表示
            if (e.Button == MouseButtons.Right)
            {
                listContextMenuStrip.Items.Clear();

                try
                {
                    // クリックした箇所が自動選択されるので拾う
                    var selected = targetListView.SelectedItems[0];
                    uint recID;
                    if (!uint.TryParse(selected.Name, out recID))
                    {
                        MessageBox.Show("録画情報IDの取得に失敗しました。", "録画情報IDエラー");
                        return;
                    }

                    RecFileInfo recFile = _epgDataManager.RecMap[recID];

                    // Webリンク使用時のみ1行目にWebリンク用のボタンを表示
                    if (_configManager.RockbarSetting.UseWebLink)
                    {
                        var item = listContextMenuStrip.Items.Add(">>");
                        item.Click += (s2, e2) => accessWebUrl(recFile);

                        listContextMenuStrip.Items.Add(new ToolStripSeparator());
                    }

                    var detailTexts = createRecInfoDetailTexts(recFile);
                    if (detailTexts.Count > 0)
                    {
                        const string copyCommandText = "テキストをコピー";
                        const string copyCommandTooltipText = "クリックでテキストをコピー";
                        const string filterCommandText = "フィルタリング";
                        // 録画日時を追加
                        if (detailTexts[0].Count > 0)
                        {
                            foreach (var text in detailTexts[0])
                            {
                                var item = listContextMenuStrip.Items.Add(text.Value);
                                item.Click += (s2, e2) => copyText(text.CopyText);
                                item.ToolTipText = copyCommandTooltipText;
                            }
                            listContextMenuStrip.Items.Add(new ToolStripSeparator());
                        }
                        // 録画サービス名・番組名を追加
                        if (detailTexts[1].Count > 0)
                        {
                            foreach (var text in detailTexts[1])
                            {
                                var item = new ToolStripMenuItem(text.Value);
                                listContextMenuStrip.Items.Add(item);
                                var subItem = item.DropDownItems.Add(copyCommandText);
                                subItem.Click += (s2, e2) => copyText(text.CopyText);
                                subItem = item.DropDownItems.Add(filterCommandText);
                                subItem.Click += (s2, e2) => filterWith(text.CopyText);
                            }
                            listContextMenuStrip.Items.Add(new ToolStripSeparator());
                        }
                        // その他の録画情報を追加
                        foreach (var texts in detailTexts.GetRange(2, detailTexts.Count - 3))
                        {
                            if (texts.Count == 0)
                            {
                                continue;
                            }
                            foreach (var text in texts)
                            {
                                var item = listContextMenuStrip.Items.Add(text.Value);
                                item.Click += (s2, e2) => copyText(text.CopyText);
                                item.ToolTipText = copyCommandTooltipText;
                            }
                            listContextMenuStrip.Items.Add(new ToolStripSeparator());
                        }
                        var lastTexts = detailTexts[detailTexts.Count - 1];
                        if (lastTexts.Count > 0)
                        {
                            foreach (var text in lastTexts)
                            {
                                var item = listContextMenuStrip.Items.Add(text.Value);
                                item.Click += (s2, e2) => copyText(text.CopyText);
                                item.ToolTipText = copyCommandTooltipText;
                            }
                        }
                    }
                }
                catch
                {
                    listContextMenuStrip.Items.Add("番組情報を取得できませんでした");
                }

                listContextMenuStrip.Show((Control)sender, new Point(e.X, e.Y));
            }
        }

        /// <summary>
        /// 録画済み情報マウスダブルクリック処理
        /// TVTest使用オプションがONの場合、カーソル箇所の番組を対象にTVTestを起動する。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        private void recListView_MouseDoubleClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            // TVTest使用時のみ
            if (!_configManager.RockbarSetting.UseDoubleClickTvtest)
            {
                return;
            }

            var selectedCount = targetListView.SelectedItems.Count;
            if (selectedCount == 0)
            {
                return;
            }

            // 左ダブルクリック
            // TVTestを起動する
            if (e.Button == MouseButtons.Left)
            {
                // クリックした箇所が自動選択されるので拾う
                var selected = targetListView.SelectedItems[0];

                uint recID;
                if (!uint.TryParse(selected.Name, out recID))
                {
                    MessageBox.Show("録画情報IDの取得に失敗しました。", "録画情報IDエラー");
                    return;
                }

                RecFileInfo recFile = _epgDataManager.RecMap[recID];

                _tvtestManager.PlayRecFile(recFile);
            }
        }

        /// <summary>
        /// 録画済み情報マウスクリック処理
        /// 右クリック時、対象の番組情報をコンテキストメニューに表示。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        private void newProgramListView_MouseClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            var selectedCount = targetListView.SelectedItems.Count;
            if (selectedCount == 0)
            {
                return;
            }

            // 右クリック以外は処理しない
            // 対象の番組情報をコンテキストメニューで表示
            if (e.Button != MouseButtons.Right) return;

            // クリックされた位置にあるアイテム（番組）を取得
            var hitTest = targetListView.HitTest(e.Location);
            var selectedItem = hitTest.Item;

            if (selectedItem == null) return;

            // Tagに格納されているものがEpgEventInfoならevへ代入
            if (selectedItem.Tag is EpgEventInfo ev)
            {
                listContextMenuStrip.Items.Clear();

                // Web番組詳細リンク (オプション有効時)
                if (_configManager.RockbarSetting.UseWebLink)
                {
                    var item = listContextMenuStrip.Items.Add(">> Web番組詳細を開く");
                    item.Click += (s2, e2) => accessWebUrl(ev);
                    listContextMenuStrip.Items.Add(new ToolStripSeparator());
                }

                // 予約データの検索し、有無結果を格納
                string eventKey = RockbarUtility.GetKey(ev.transport_stream_id, ev.service_id, ev.event_id);
                var hasData = _epgDataManager.ReserveMap.TryGetValue(eventKey, out var reserveData);

                // 録画の有効・無効切り替え (予約データが存在する場合)
                if (hasData)
                {
                    var item = listContextMenuStrip.Items.Add(reserveData.RecSetting.IsNoRec() ? "録画を有効にする" : "録画を無効にする");
                    item.Click += (s2, e2) => ToggleRecMode(reserveData);
                    listContextMenuStrip.Items.Add(new ToolStripSeparator());
                }

                // 放送日時
                var dateTime = $"{ev.start_time:yyyy/MM/dd(ddd) HH:mm}～{ev.start_time.AddSeconds(ev.durationSec):HH:mm}";
                var dateItem = listContextMenuStrip.Items.Add(dateTime);
                dateItem.Click += (s2, e2) => copyText(dateTime);
                dateItem.ToolTipText = "クリックで日時をコピー";

                // チャンネル名
                // 設定ファイルのチャンネル名を最優先で使用し、設定がなければEDCBのStationNameを使用
                string serviceName = GetServiceName(ev.transport_stream_id, ev.service_id);

                if (!string.IsNullOrEmpty(serviceName))
                {
                    var serviceItem = listContextMenuStrip.Items.Add(serviceName);
                    serviceItem.Click += (s2, e2) => copyText(serviceName);
                    serviceItem.ToolTipText = "クリックでチャンネル名をコピー";
                }

                // 番組名
                string eventTitle = ev.ShortInfo?.event_name;
                if (!string.IsNullOrEmpty(eventTitle))
                {
                    var titleItem = listContextMenuStrip.Items.Add(eventTitle);
                    titleItem.Click += (s2, e2) => copyText(eventTitle);
                    titleItem.ToolTipText = "クリックで番組名をコピー";
                }

                listContextMenuStrip.Items.Add(new ToolStripSeparator());

                // 番組基本情報 (説明文)
                var shortStrs = RockbarUtility.BreakString(ev.ShortInfo?.text_char);
                if (shortStrs != null)
                {
                    foreach (string str in shortStrs)
                    {
                        var item = listContextMenuStrip.Items.Add(str);
                        item.Enabled = false; // テキスト表示用（クリック不可）
                    }
                }

                // 番組拡張情報
                var longStrs = RockbarUtility.BreakString(ev.ExtInfo?.text_char);
                if (longStrs != null && longStrs.Count > 0)
                {
                    listContextMenuStrip.Items.Add(new ToolStripSeparator());
                    foreach (string str in longStrs)
                    {
                        var item = listContextMenuStrip.Items.Add(str);
                        item.Enabled = false; // テキスト表示用（クリック不可）
                    }
                }

                // メニューを表示
                listContextMenuStrip.Show((Control)sender, e.Location);
            }
        }

        /// <summary>
        /// 新番組一覧マウスダブルクリック処理
        /// Web LinkオプションがONの場合、カーソル箇所の番組のWeb番組情報にアクセスする。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        private void newProgramListView_MouseDoubleClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            // Web番組詳細使用時のみ
            if (!_configManager.RockbarSetting.UseWebLink)
            {
                return;
            }

            var selectedCount = targetListView.SelectedItems.Count;
            if (selectedCount == 0)
            {
                return;
            }

            // 左ダブルクリック
            // Web番組詳細にアクセスする
            if (e.Button == MouseButtons.Left)
            {
                try
                {
                    // 選択されている行（アイテム）を取得
                    var selected = targetListView.SelectedItems[0];

                    // Tag から EpgEventInfo を安全に取り出して Web ページを開く
                    if (selected.Tag is EpgEventInfo ev)
                    {
                        accessWebUrl(ev);
                    }
                    else
                    {
                        MessageBox.Show("番組情報を取得できませんでした。", "ブラウザ起動エラー");
                    }
                }
                catch
                {
                    MessageBox.Show("番組情報を取得できませんでした。", "ブラウザ起動エラー");
                    return;
                }
            }
        }

        /// <summary>
        /// subListViewマウスクリック時処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void subListView_MouseClick(object sender, MouseEventArgs e)
        {
            tunerListView_MouseClick(sender, e, subListView);
        }

        /// <summary>
        /// チューナー一覧マウスクリック時処理
        /// 右クリック時、直近30件の予約情報をコンテキストメニューに表示。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        private void tunerListView_MouseClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            // 右クリック
            // 今後の予約を30件までコンテキストメニューで表示
            //TODO DRY
            if (e.Button == MouseButtons.Right)
            {
                // クリックした箇所が自動選択されるので拾う
                var selected = targetListView.SelectedItems[0];

                listContextMenuStrip.Items.Clear();

                TunerReserveInfo tunerReserveInfo = _epgDataManager.TunerReserveInfos.Find((TunerReserveInfo x) => x.tunerID.ToString() == selected.Name);

                // TunerReserveInfoには予約IDしか入っていないので、ReserveDataから予約情報を取り直す
                HashSet<uint> reserveIds = tunerReserveInfo.reserveList.ToHashSet();

                var reserves = _epgDataManager.ReserveDatas.FindAll(x => reserveIds.Contains(x.ReserveID)).OrderBy(x => x.StartTime);

                int i = 0;

                foreach (var reserveData in reserves)
                {
                    string key = RockbarUtility.GetKey(reserveData.TransportStreamID, reserveData.ServiceID, reserveData.EventID);

                    EpgEventInfo ev = null;

                    if (_epgDataManager.AllEventMap.ContainsKey(key))
                    {
                        ev = _epgDataManager.AllEventMap[key];
                    }

                    listContextMenuStrip.Items.Add(createEventToolStripMenuItem(ev, reserveData, true));

                    i++;

                    if (i >= 30)
                    {
                        break;
                    }
                }

                listContextMenuStrip.Show((Control)sender, new Point(0, e.Y));
            }
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
                //ReloadSetting();
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

            //Toml.WriteFile(_configManager.RockbarSetting, RockbarUtility.GetTomlSettingFilePath());
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
        private void openWebEPGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenWebEPG();
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
            OpenWebEPG();
        }

        /// <summary>
        /// テレビ番組表を開く処理
        /// </summary>
        private void OpenWebEPG()
        {
            // Webリンク使用時のみ
            if (_configManager.RockbarSetting.UseWebLink)
            {
                // Webを開く
                try
                {
                    // 設定内容のURLを開く
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = _configManager.RockbarSetting.WebEpgUrl,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Webページを開けませんでした。\n{ex.Message}", "エラー");
                }
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

        /// <summary>
        /// サービス名を取得します。
        /// </summary>
        private string GetServiceName(ushort transportStreamId, ushort serviceId)
        {
            string key = RockbarUtility.GetKey(transportStreamId, serviceId);

            // 設定ファイルのチャンネル名を最優先
            Service service = _configManager.SelectedServiceList?.FirstOrDefault(
                x => RockbarUtility.GetKey(x.Tsid, x.Sid) == key);

            if (!string.IsNullOrWhiteSpace(service?.Name))
            {
                return service.Name;
            }

            // 設定ファイルのチャンネル名がない場合はEPGのサービス名を使用
            if (_epgDataManager.ServiceMap.TryGetValue(key, out EpgServiceEventInfo matchedService))
            {
                return matchedService.serviceInfo?.service_name ?? key;
            }

            // EPGにも存在しない場合
            return key;
        }

    }
}
