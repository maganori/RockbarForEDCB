using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
        // 設定情報
        private RockBarSetting rockbarSetting = null;

        // EpgTimerSrv接続可否
        private bool canConnect = true;

        // CtrlCmdの結果格納用
        private List<EpgServiceEventInfo> serviceEvents = new List<EpgServiceEventInfo>();
        private List<TunerReserveInfo> tunerReserveInfos = new List<TunerReserveInfo>();
        private List<ReserveData> reserveDatas = new List<ReserveData>();
        private List<RecFileInfo> recFileInfos = new List<RecFileInfo>();

        // CtrlCmdの結果のハッシュ
        private string prevReserveHash = "";
        private string prevServiceHash = "";
        private string prevTunerHash = "";
        private string prevRecHash = "";

        // サービス一覧(+番組)の保持用(TSID + SID → サービス情報(+番組))
        private Dictionary<string, EpgServiceEventInfo> serviceMap = new Dictionary<string, EpgServiceEventInfo>();

        // 番組一覧の保持用(TSID + SID + EventID → 番組情報)
        private Dictionary<string, EpgEventInfo> allEventMap = new Dictionary<string, EpgEventInfo>();

        // 予約情報の保持用(TSID + SID + EventID → 予約情報)
        private Dictionary<string, ReserveData> reserveMap = new Dictionary<string, ReserveData>();

        // 録画済み情報の保持用(TSID + SID + EventID → 録画済み情報)
        private Dictionary<uint, RecFileInfo> recMap = new Dictionary<uint, RecFileInfo>();

        // CSVサービスリストの格納
        private List<Service> allServiceList = null;
        private List<Service> favoriteServiceList = null;

        // CtrlCmdUtil
        private CtrlCmdUtil ctrlCmdUtil = new CtrlCmdUtil();

        // Rockbarから自動起動したTVTestのプロセス一覧
        private Dictionary<string, System.Diagnostics.Process> tvtestProcesses = new Dictionary<string, System.Diagnostics.Process>();

        // マウスのクリック位置を記憶
        private Point mousePoint;

        // 色の設定情報(設定情報から変数にロードしたもの)
        private Color formBackColor;
        private Color listBackColor;
        private Color okReserveListBackColor;
        private Color partialReserveListBackColor;
        private Color ngReserveListBackColor;
        private Color disabledReserveListBackColor;
        private Color listHeaderForeColor;
        private Color listHeaderBackColor;
        private Color foreColor;

        private Color menuBackColor;
        private Color okReserveMenuBackColor;
        private Color partialReserveMenuBackColor;
        private Color ngReserveMenuBackColor;
        private Color disabledReserveMenuBackColor;

        // フィルタリング中かどうか
        private bool isFiltering => filteringLabel != null && filteringLabel.Visible;

        // 各列の余白
        private int columnPadding = -1;

        // 各列の幅
        private int maxServiceNameWidth = -1;
        private int maxTunerNameWidth = -1;

        // 次回の画面更新が必要になる最早時刻
        private DateTime nextServiceListRefreshTime = DateTime.MinValue;
        private DateTime nextReserveListRefreshTime = DateTime.MinValue;

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
            tunerListView.HideSelection = true;
            serviceListView.HideSelection = true;

            try
            {
                rockbarSetting = Toml.ReadFile<RockBarSetting>(RockbarUtility.GetTomlSettingFilePath());
            }
            catch (FileNotFoundException)
            {
                // TOML設定ファイルが存在しない場合は準正常系として空設定で起動。それ以外の場合は例外を投げる
                rockbarSetting = new RockBarSetting();
            }

            // コンフィグファイルにwidth, height指定時のみ前回位置・サイズ・スプリッタ位置で起動
            if (rockbarSetting.Width != 0 && rockbarSetting.Height != 0)
            {
                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(rockbarSetting.X, rockbarSetting.Y);
                this.Size = new Size(rockbarSetting.Width, rockbarSetting.Height);
                splitContainer.SplitterDistance = rockbarSetting.SplitterDistance;
            }

            allServiceList = RockbarUtility.GetAllServicesFromSetting();
            favoriteServiceList = RockbarUtility.GetFavoriteServicesFromSetting();

            // 設定反映
            applySetting();

            if (rockbarSetting.UseTcpIp) {
                // TCP/IP通信にする
                ctrlCmdUtil.SetSendMode(true);
                ctrlCmdUtil.SetNWSetting(rockbarSetting.IpAddress, rockbarSetting.PortNumber);
            }
            else
            {
                // Pipe通信にする
                ctrlCmdUtil.SetSendMode(false);
            }

            // 適当な通信を行って、通信可否を確認し問題があればメッセージを出す
            tunerReserveInfos.Clear();
            ErrCode errCode = ctrlCmdUtil.SendEnumTunerReserve(ref tunerReserveInfos);

            if (errCode != ErrCode.CMD_SUCCESS)
            {
                canConnect = false;
                MessageBox.Show(
                    $"EpgTimerSrvと接続できません。以降の通信を停止します。\nオプション設定を見直してアプリケーションを再起動してください。\n\nErrCode: {errCode}",
                    "EpgTimerSrv接続チェック失敗",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }

            // フィルタ未適用状態
            filteringLabel.Visible = false;

            // 初回表示
            RefreshEvent(true, true);

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
            if (rockbarSetting.ShowTaskTrayIcon)
            {
                notifyIcon.Visible = true;
            }
            else
            {
                // 初回起動時・設定画面からの戻りで格納状態はないはず
                notifyIcon.Visible = false;
            }

            // 縦に並べて表示
            if (rockbarSetting.IsHorizontalSplit)
            {
                splitContainer.Orientation = Orientation.Horizontal;
            }
            else
            {
                splitContainer.Orientation = Orientation.Vertical;
            }

            // フォント
            TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));
            Font font = (Font) fontConverter.ConvertFromString(rockbarSetting.Font);
            Font menuFont = (Font)fontConverter.ConvertFromString(rockbarSetting.MenuFont);
            Font tabFont = (Font)fontConverter.ConvertFromString(rockbarSetting.TabFont);
            Font buttonFont = (Font)fontConverter.ConvertFromString(rockbarSetting.ButtonFont);
            Font labelFont = (Font)fontConverter.ConvertFromString(rockbarSetting.LabelFont);
            Font textBoxFont = (Font)fontConverter.ConvertFromString(rockbarSetting.TextBoxFont);

            serviceListView.Font = font;
            tunerListView.Font = font;
            listContextMenuStrip.Font = menuFont;
            serviceTabControl.Font = tabFont;
            resetButton.Font = buttonFont;
            closeButton.Font = buttonFont;
            filteringLabel.Font = labelFont;
            filterTextBox.Font = textBoxFont;
            filterButton.Font = buttonFont;
            settingButton.Font = buttonFont;

            // 列の幅
            columnPadding = GetColumnPadding(font);
            maxServiceNameWidth = GetMaxServiceNameWidth(font);
            maxTunerNameWidth = GetMaxTunerNameWidth(font);

            // 色
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
            this.formBackColor = (Color)colorConverter.ConvertFromString(rockbarSetting.FormBackColor);
            this.listBackColor = (Color)colorConverter.ConvertFromString(rockbarSetting.ListBackColor);
            this.okReserveListBackColor = (Color)colorConverter.ConvertFromString(rockbarSetting.OkReserveListBackColor);
            this.partialReserveListBackColor = (Color)colorConverter.ConvertFromString(rockbarSetting.PartialReserveListBackColor);
            this.ngReserveListBackColor = (Color)colorConverter.ConvertFromString(rockbarSetting.NgReserveListBackColor);
            this.disabledReserveListBackColor = (Color)colorConverter.ConvertFromString(rockbarSetting.DisabledReserveListBackColor);
            this.listHeaderForeColor = (Color)colorConverter.ConvertFromString(rockbarSetting.ListHeaderForeColor);
            this.listHeaderBackColor = (Color)colorConverter.ConvertFromString(rockbarSetting.ListHeaderBackColor);
            this.foreColor = (Color)colorConverter.ConvertFromString(rockbarSetting.ForeColor);

            this.BackColor = this.formBackColor;

            this.menuBackColor = (Color)colorConverter.ConvertFromString(rockbarSetting.MenuBackColor);
            this.okReserveMenuBackColor = (Color)colorConverter.ConvertFromString(rockbarSetting.OkReserveMenuBackColor);
            this.partialReserveMenuBackColor = (Color)colorConverter.ConvertFromString(rockbarSetting.PartialReserveMenuBackColor);
            this.ngReserveMenuBackColor = (Color)colorConverter.ConvertFromString(rockbarSetting.NgReserveMenuBackColor);
            this.disabledReserveMenuBackColor = (Color)colorConverter.ConvertFromString(rockbarSetting.DisabledReserveMenuBackColor);

            serviceListView.BackColor = this.listBackColor;
            serviceListView.ForeColor = this.foreColor;

            tunerListView.BackColor = this.listBackColor;
            tunerListView.ForeColor = this.foreColor;

            listContextMenuStrip.BackColor = this.menuBackColor;
        }

        /// <summary>
        /// 描画更新処理
        /// 必要があればEpgTimerSrv通信を行い、チャンネルListView・チューナListViewの表示を更新する。
        /// </summary>
        /// <param name="isChannelRefresh">対象チャンネルリストの切り替え要否</param>
        /// <param name="isTrasnmission">EpgTimerSrvと通信要否</param>
        private void RefreshEvent(bool isForceRefreshRequested, bool isTrasnmission)
        {
            DateTime now = DateTime.Now;

            DateTime reserveRefreshTime = nextReserveListRefreshTime;
            DateTime serviceRefreshTime = nextServiceListRefreshTime;

            bool isReserveChanged = false;
            bool isServiceChanged = false;
            bool isTunerChanged = false;
            bool isRecChanged = false;

            // EpgTimerSrvと通信する
            if (isTrasnmission && canConnect)
            {
                GetEpgTimerData(out isServiceChanged, out isTunerChanged, out isReserveChanged, out isRecChanged);
            }
            
            // 現在アクティブなタブに応じて描画処理を分岐
            // 予約タブ
            if (serviceTabControl.SelectedTab == reserveTabPage)
            {
                if (isForceRefreshRequested || isReserveChanged || (now >= reserveRefreshTime))
                { 
                    BuildReserveList();
                }
            }
            // 録画タブ
            else if (serviceTabControl.SelectedTab == recTabPage)
            {
                if (isForceRefreshRequested || isRecChanged)
                {
                    BuildRecList();
                }
            }
            // チャンネルタブ
            else
            {
                if (isForceRefreshRequested || isServiceChanged || isReserveChanged || (now >= serviceRefreshTime))
                {
                    BuildServiceList();
                }
            }

            // チューナー一覧
            if (isTunerChanged || (now >= reserveRefreshTime))
            {
                BuildTunerList();
            }
        }

        /// <summary>
        /// EpgTimerSrvから予約一覧・番組一覧・チューナー一覧・録画一覧を取得し、
        /// reserveMap / serviceMap / allEventMap / recMap を構築する。
        private void GetEpgTimerData(out bool isServiceChanged, out bool isTunerChanged, out bool isReserveChanged, out bool isRecChanged)
        {
            // 予約一覧取得
            reserveDatas.Clear();
            ctrlCmdUtil.SendEnumReserve(ref reserveDatas);

            // 予約一覧関連のハッシュを作成
            reserveMap.Clear();
            foreach (ReserveData reserveData in reserveDatas)
            {
                // TSID + SID + EventID → 予約情報
                string evkey = RockbarUtility.GetKey(reserveData.TransportStreamID, reserveData.ServiceID, reserveData.EventID);
                reserveMap[evkey] = reserveData;
            }

            // 予約一覧の差分有無チェック
            // 予約数、予約ID、重複状態、録画モード(全サービス録画、無効など)
            string currentReserveHash = $"{reserveDatas.Count}_" +
                string.Join(",", reserveDatas.Select(r => $"{r.ReserveID}_{r.OverlapMode}_{r.RecSetting.RecMode}"));
            isReserveChanged = (currentReserveHash != prevReserveHash);
            prevReserveHash = currentReserveHash;


            // 番組一覧取得
            serviceEvents.Clear();
            allEventMap.Clear();
            ctrlCmdUtil.SendEnumPgAll(ref serviceEvents);

            // 番組一覧関連のハッシュを作成
            serviceMap.Clear();
            foreach (EpgServiceEventInfo service in serviceEvents)
            {
                // TSID + SID → サービス一覧(serviceが番組情報を保持している)
                string key = RockbarUtility.GetKey(service.serviceInfo.TSID, service.serviceInfo.SID);
                serviceMap[key] = service;

                // TSID + SID + EventID → 番組情報
                foreach (EpgEventInfo ev in service.eventList)
                {
                    string evKey = RockbarUtility.GetKey(ev.transport_stream_id, ev.service_id, ev.event_id);
                    allEventMap[evKey] = ev;
                }
            }

            // 番組一覧の差分有無チェック
            // チャンネル数、前番組データ数
            string currentServiceHash = $"{serviceEvents.Count}_{allEventMap.Count}";
            isServiceChanged = (currentServiceHash != prevServiceHash);
            prevServiceHash = currentServiceHash;


            // チューナーごとの予約一覧取得
            tunerReserveInfos.Clear();
            ctrlCmdUtil.SendEnumTunerReserve(ref tunerReserveInfos);

            // チューナーごとの予約一覧の差分有無チェック
            // チューナー台数、各チューナーの識別ID、各チューナーに割り当てられている予約の件数
            string currentTunerHash = $"{tunerReserveInfos.Count}_" +
                string.Join(",", tunerReserveInfos.Select(t => $"{t.tunerID}_{t.reserveList.Count}"));
            isTunerChanged = (currentTunerHash != prevTunerHash);
            prevTunerHash = currentTunerHash;

            // 録画済み情報の一覧取得
            if (serviceTabControl.SelectedTab == recTabPage)
            {
                FetchRecList();
            }
            else
            {
                recFileInfos.Clear();
                recMap.Clear();
            }

            // 録画済み情報の一覧の差分有無チェック
            // 録画済みファイルの件数、各録画済みファイルの固有ID
            string currentRecHash = $"{recFileInfos.Count}_" +
                string.Join(",", recFileInfos.Select(r => r.ID));
            isRecChanged = (currentRecHash != prevRecHash);
            prevRecHash = currentRecHash;
        }

        /// <summary>
        /// 予約一覧の生成
        /// </summary>
        private void BuildReserveList()
        {
            // チラつきを抑えるため描画を停止
            serviceListView.BeginUpdate();

            try
            {
                // 一覧を全件クリア
                serviceListView.Items.Clear();

                foreach (var reserveData in reserveDatas.OrderBy(r => r.StartTime))
                {
                    DateTime startTime = reserveData.StartTime;
                    DateTime endTime = startTime.AddSeconds(reserveData.DurationSecond);

                    // 過去の予約（すでに終了しているもの）は表示しない
                    if (endTime <= DateTime.Now)
                    {
                        continue;
                    }

                    // フィルタ条件チェック
                    if (!IsMatchFilter(reserveData.StationName, reserveData.Title))
                    {
                        continue;
                    }

                    // --- 表示文字列の作成 ---
                    ReserveStatus reserveStatus = ReserveStatus.OK;
                    if (reserveData.RecSetting.IsNoRec())
                    {
                        reserveStatus = ReserveStatus.DISABLED;
                    }
                    else if (reserveData.OverlapMode == 1)
                    {
                        reserveStatus = ReserveStatus.PARTIAL;
                    }
                    else if (reserveData.OverlapMode == 2)
                    {
                        reserveStatus = ReserveStatus.NG;
                    }

                    string statusText = RockbarUtility.GetReserveStatusString(reserveStatus);

                    // ◎（正常予約）の場合はチューナー名を表示する
                    if (reserveStatus == ReserveStatus.OK)
                    {
                        var matchedTuner = tunerReserveInfos.FirstOrDefault(t => t.reserveList.Contains(reserveData.ReserveID));
                        if (matchedTuner != null)
                        {
                            if (rockbarSetting.BonDriverNameToTunerName.ContainsKey(matchedTuner.tunerName))
                            {
                                statusText = rockbarSetting.BonDriverNameToTunerName[matchedTuner.tunerName];
                            }
                            else
                            {
                                statusText = RockbarUtility.GetDefaultTunerName(matchedTuner.tunerName);
                            }

                            if (matchedTuner.tunerID != 0xffffffff)
                            {
                                statusText += (matchedTuner.tunerID & 0xffff).ToString();
                            }
                        }
                    }

                    string dateTimeText = $"{startTime:MM/dd(ddd) HH:mm}-{endTime:HH:mm}";
                    string serviceName = reserveData.StationName;
                    string title = reserveData.Title;

                    // --- UI要素（ListViewItem）の生成 ---
                    string[] rowData = { statusText, dateTimeText, serviceName, title };
                    ListViewItem item = new ListViewItem(rowData);

                    item.Name = RockbarUtility.GetKey(reserveData.TransportStreamID, reserveData.ServiceID, reserveData.EventID);
                    item.ToolTipText = $"{startTime:yyyy/MM/dd(ddd) HH:mm}-{endTime:HH:mm} {title}";
                    item.ForeColor = this.foreColor;

                    // --- 背景色の設定 ---
                    // 無効予約の場合
                    if (reserveStatus == ReserveStatus.DISABLED)
                    {
                        item.BackColor = this.disabledReserveListBackColor;
                    }
                    // 変な予約がある場合警告として色を変える
                    else if (reserveStatus == ReserveStatus.PARTIAL)
                    {
                        // 一部予約の場合、黃背景色で警告
                        item.BackColor = this.partialReserveListBackColor;
                    }
                    else if (reserveStatus == ReserveStatus.NG)
                    {
                        // TU不足の場合、赤背景色で警告
                        item.BackColor = this.ngReserveListBackColor;
                    }
                    else if (startTime <= DateTime.Now && endTime >= DateTime.Now)
                    {
                        // 現在録画中の場合、正常予約背景色で表示
                        item.BackColor = this.okReserveListBackColor;
                    }
                    else
                    {
                        item.BackColor = this.listBackColor;
                    }

                    serviceListView.Items.Add(item);
                }

                // --- 列幅の設定（設定ファイルの最長文字数に基づく計算） ---
                // 列0: チューナー名の最長幅
                int col0Width = maxTunerNameWidth + columnPadding;

                // 列1: 日時表記の固定幅
                int col1Width = TextRenderer.MeasureText("00/00(日) 00:00-00:00", serviceListView.Font).Width + columnPadding;

                // 列2: チャンネル名の最長幅
                int col2Width = maxServiceNameWidth + columnPadding;

                serviceListView.Columns[0].Width = col0Width;
                serviceListView.Columns[1].Width = col1Width;
                serviceListView.Columns[2].Width = col2Width;

                // 列3: 番組名（残りの幅をすべて充当）
                int remainingWidth = serviceListView.ClientSize.Width - (col0Width + col1Width + col2Width);
                serviceListView.Columns[3].Width = Math.Max(100, remainingWidth - 2);
            }
            finally
            {
                // 描画を再開
                serviceListView.EndUpdate();
            }
            // 前回垂直スクロールバーがない状態からある状態になると水平スクロールバーが出るため再調整
            adjustListViewColumns(serviceListView);
        }

        /// <summary>
        /// 録画済み一覧の取得処理
        /// </summary>
        private void FetchRecList()
        {
            recFileInfos.Clear();
            recMap.Clear();
            ctrlCmdUtil.SendEnumRecInfoBasic(ref recFileInfos);

            int recListMaxCount = rockbarSetting.RecListMaxCount;
            if (recListMaxCount > 0 && recFileInfos.Count > recListMaxCount)
            {
                recFileInfos = recFileInfos.GetRange(recFileInfos.Count - recListMaxCount, recListMaxCount);
            }
            recFileInfos.Reverse();

            recMap = recFileInfos.ToDictionary(r => r.ID);
        }

        /// <summary>
        /// 録画済み一覧の生成
        /// </summary>
        private void BuildRecList()
        {
            if (recFileInfos.Count == 0)
            {
                FetchRecList();
            }

            // チラつきを抑えるため描画を停止
            serviceListView.BeginUpdate();

            try
            {
                // 一覧を全件クリア
                serviceListView.Items.Clear();

                foreach (var recFile in recFileInfos)
                {
                    DateTime startTime = recFile.StartTime;
                    DateTime endTime = startTime.AddSeconds(recFile.DurationSecond);

                    // フィルタ条件チェック
                    if (!IsMatchFilter(recFile.ServiceName, recFile.Title))
                    {
                        continue;
                    }

                    // --- 表示文字列の作成 ---
                    RecEndStatus recEndStatus = (RecEndStatus)recFile.RecStatus;
                    string statusText = RockbarUtility.GetRecEndStatusString(recEndStatus);
                    string dateTimeText = $"{startTime:yy/MM/dd(ddd) HH:mm}-{endTime:HH:mm}";
                    string serviceName = recFile.ServiceName;
                    string title = recFile.Title;

                    // --- UI要素（ListViewItem）の生成 ---
                    string[] rowData = { statusText, dateTimeText, serviceName, title };
                    ListViewItem item = new ListViewItem(rowData);

                    item.Name = recFile.ID.ToString();
                    item.ToolTipText = createRecInfoTooltipTexts(recFile);
                    item.ForeColor = this.foreColor;

                    // --- 背景色の設定 ---
                    // 録画結果が正常の場合
                    if (recEndStatus == RecEndStatus.NORMAL || recEndStatus == RecEndStatus.CHG_TIME || recEndStatus == RecEndStatus.NEXT_START_END)
                    {
                        if (recFile.Scrambles > 0)
                        {
                            // スクランブル解除漏れありの場合、黃背景色で警告
                            item.BackColor = this.partialReserveListBackColor;
                        }
                        else if (recFile.Drops > 0)
                        {
                            // ドロップありの場合、赤背景色で警告
                            item.BackColor = this.ngReserveListBackColor;
                        }
                        else
                        {
                            item.BackColor = this.listBackColor;
                        }
                    }
                    // 録画結果が正常ではない場合
                    else if (recEndStatus == RecEndStatus.END_SUBREC)
                    {
                        // サブフォルダへの録画の場合、ダークスレートグレー背景色で通知
                        item.BackColor = this.okReserveListBackColor;
                    }
                    else if (recEndStatus == RecEndStatus.ERR_END || recEndStatus == RecEndStatus.NOT_START_HEAD)
                    {
                        // 録画中のエラー、一部のみ録画の場合、黃背景色で警告
                        item.BackColor = this.partialReserveListBackColor;
                    }
                    else if (recEndStatus == RecEndStatus.NO_RECMODE)
                    {
                        // 無効扱いの場合、グレー背景色で通知
                        item.BackColor = this.disabledReserveListBackColor;
                    }
                    else
                    {
                        // それ以外のエラーの場合、赤背景色で警告
                        item.BackColor = this.ngReserveListBackColor;
                    }

                    serviceListView.Items.Add(item);
                }

                // --- 列幅の設定 ---
                // 列0: 録画状態
                int col0Width = TextRenderer.MeasureText("◎", serviceListView.Font).Width + columnPadding;

                // 列1: 日時表記の固定幅
                int col1Width = TextRenderer.MeasureText("00/00/00(日) 00:00-00:00", serviceListView.Font).Width + columnPadding;

                // 列2: チャンネル名の最長幅
                int col2Width = maxServiceNameWidth + columnPadding;

                serviceListView.Columns[0].Width = col0Width;
                serviceListView.Columns[1].Width = col1Width;
                serviceListView.Columns[2].Width = col2Width;

                // 列3: 番組名（残りの幅をすべて充当）
                int remainingWidth = serviceListView.ClientSize.Width - (col0Width + col1Width + col2Width);
                serviceListView.Columns[3].Width = Math.Max(100, remainingWidth - 2);
            }
            finally
            {
                // 描画を再開
                serviceListView.EndUpdate();
            }
            // 前回垂直スクロールバーがない状態からある状態になると水平スクロールバーが出るため再調整
            adjustListViewColumns(serviceListView);
        }

        /// <summary>
        /// チャンネル一覧の生成
        /// </summary>
        private void BuildServiceList()
        {
            DateTime now = DateTime.Now;
            DateTime minNextStartTime = DateTime.MaxValue;

            // チラつきを抑えるため描画を停止
            serviceListView.BeginUpdate();

            try
            {
                // 一覧を全件クリア
                serviceListView.Items.Clear();

                List<Service> services = (serviceTabControl.SelectedTab == favoriteTabPage)
                    ? favoriteServiceList
                    : allServiceList;

                // チャンネル表示
                foreach (var service in services)
                {
                    string key = RockbarUtility.GetKey(service.Tsid, service.Sid);

                    EpgServiceEventInfo matchedService = null;
                    serviceMap.TryGetValue(key, out matchedService);

                    // CSVのチャンネル一覧で設定されているサービスタイプを最優先で使用する
                    // サービスタイプ設定が無い場合はEDCBからのデータをもとに自動判別
                    ServiceType serviceType = matchedService != null
                        ? RockbarUtility.GetServiceType(service.Type, matchedService.serviceInfo.ONID)
                        : RockbarUtility.GetServiceType(service.Type, null);

                    // 選択中タブ（地デジ / BS / CS）と不一致のサービスは除外する
                    if (serviceTabControl.SelectedTab == dttvTabPage && serviceType != ServiceType.DTTV) continue;
                    if (serviceTabControl.SelectedTab == bsTabPage && serviceType != ServiceType.BS) continue;
                    if (serviceTabControl.SelectedTab == csTabPage && serviceType != ServiceType.CS) continue;

                    // CSVのチャンネル一覧で設定されているチャンネル名を最優先で使用する
                    // チャンネル名設定がない場合はEDCBからのデータを使用し、それもなければTsid_Sid
                    string serviceName = !string.IsNullOrWhiteSpace(service.Name)
                        ? service.Name
                        : (matchedService != null ? matchedService.serviceInfo.service_name : key);

                    // 現在放送中の番組を探す
                    EpgEventInfo ev = matchedService?.eventList.Find(x =>
                        x.start_time <= DateTime.Now &&
                        x.start_time.AddSeconds(x.durationSec) >= DateTime.Now);

                    // EPG情報が無い場合と現在放送中の番組が無い場合のタイトル設定
                    string eventTitle = ev?.ShortInfo?.event_name ?? (matchedService == null ? "EPG未取得" : "");

                    // フィルタ条件チェック（チャンネル名・番組名の双方を対象とする）
                    if (!IsMatchFilter(serviceName, eventTitle))
                    {
                        continue;
                    }

                    // --- 時刻および予約状態テキストの設定 ---
                    string timeText = "";
                    string reserveString = "";
                    ReserveStatus reserveStatus = ReserveStatus.NONE;

                    if (ev != null)
                    {
                        timeText = $"{ev.start_time:HH:mm}-{ev.start_time.AddSeconds(ev.durationSec):HH:mm}";
                        string eventKey = RockbarUtility.GetKey(ev.transport_stream_id, ev.service_id, ev.event_id);

                        // 予約状態文字列を取得
                        if (reserveMap.TryGetValue(eventKey, out ReserveData reserveData))
                        {
                            reserveStatus = ReserveStatus.OK;
                            if (reserveData.RecSetting.IsNoRec()) reserveStatus = ReserveStatus.DISABLED;
                            else if (reserveData.OverlapMode == 1) reserveStatus = ReserveStatus.PARTIAL;
                            else if (reserveData.OverlapMode == 2) reserveStatus = ReserveStatus.NG;
                        }
                        reserveString = RockbarUtility.GetReserveStatusString(reserveStatus);
                    }

                    // --- UI要素（ListViewItem）の生成 ---
                    string[] rowData = { serviceName, timeText, reserveString, eventTitle };
                    ListViewItem item = new ListViewItem(rowData)
                    {
                        Name = key,
                        Tag = service,
                        ToolTipText = eventTitle,
                        ForeColor = this.foreColor
                    };

                    // 色変更
                    switch (reserveStatus)
                    {
                        case ReserveStatus.NONE:
                            item.BackColor = this.listBackColor;
                            break;
                        case ReserveStatus.OK:
                            item.BackColor = this.okReserveListBackColor;
                            break;
                        case ReserveStatus.PARTIAL:
                            item.BackColor = this.partialReserveListBackColor;
                            break;
                        case ReserveStatus.NG:
                            item.BackColor = this.ngReserveListBackColor;
                            break;
                        case ReserveStatus.DISABLED:
                            item.BackColor = this.disabledReserveListBackColor;
                            break;
                    }

                    serviceListView.Items.Add(item);

                    // 次に始まる最初の番組の「開始時刻」を探す
                    if (matchedService?.eventList != null)
                    {
                        var nextEv = matchedService.eventList
                            .Where(x => x.start_time > now)
                            .OrderBy(x => x.start_time)
                            .FirstOrDefault();

                        if (nextEv != null && nextEv.start_time < minNextStartTime)
                        {
                            minNextStartTime = nextEv.start_time;
                        }
                    }
                }

                // --- 次回更新時刻の設定 ---
                if (minNextStartTime != DateTime.MaxValue)
                {
                    this.nextServiceListRefreshTime = minNextStartTime; 
                }
                
                // --- 列幅の設定 ---
                // 列0: チャンネル名の最長幅
                int col0Width = maxServiceNameWidth + columnPadding;

                // 列1: 時間表記の固定幅
                int col1Width = TextRenderer.MeasureText("00:00-00:00", serviceListView.Font).Width + columnPadding;

                // 列2: 予約状態
                int col2Width = TextRenderer.MeasureText("◎", serviceListView.Font).Width + columnPadding;

                serviceListView.Columns[0].Width = col0Width;
                serviceListView.Columns[1].Width = col1Width;
                serviceListView.Columns[2].Width = col2Width;

                // 列3: 番組名（残りの幅をすべて充当）
                int remainingWidth = serviceListView.ClientSize.Width - (col0Width + col1Width + col2Width);
                serviceListView.Columns[3].Width = Math.Max(100, remainingWidth - 2);
            }
            finally
            {
                // 描画を再開
                serviceListView.EndUpdate();
            }
            // 前回垂直スクロールバーがない状態からある状態になると水平スクロールバーが出るため再調整
            adjustListViewColumns(serviceListView);
        }

        /// <summary>
        /// チューナー一覧の生成
        /// </summary>
        private void BuildTunerList()
        {
            // チラつきを抑えるため描画を停止
            tunerListView.BeginUpdate();

            // 次回更新時間判定用：現在時刻以降で最も近いイベント時刻（開始 or 終了）を保持
            DateTime now = DateTime.Now;
            DateTime nearestEventTime = DateTime.MaxValue;

            try
            {
                // 一覧を全件クリア
                tunerListView.Items.Clear();

                // チューナー表示
                foreach (var tuner in tunerReserveInfos)
                {
                    string tunerName = null;

                    // 設定にチューナー名があれば取得し、なければデフォルト名で表示
                    if (rockbarSetting.BonDriverNameToTunerName.ContainsKey(tuner.tunerName))
                    {
                        tunerName = rockbarSetting.BonDriverNameToTunerName[tuner.tunerName];
                    }
                    else
                    {
                        tunerName = RockbarUtility.GetDefaultTunerName(tuner.tunerName);
                    }

                    // 連番付与
                    if (tuner.tunerID != 0xffffffff)
                    {
                        tunerName += (tuner.tunerID & 0xffff).ToString();
                    }

                    // 直近の予約タイトルとツールチップをを抽出する
                    HashSet<uint> reserveIds = tuner.reserveList.ToHashSet();

                    List<ReserveData> reserves = reserveDatas.FindAll(x => reserveIds.Contains(x.ReserveID));

                    var nearestReserve = reserves
                        .Where(x => x.StartTime.AddSeconds(x.DurationSecond) > now)
                        .OrderBy(x => x.StartTime.AddSeconds(x.DurationSecond)) // 終了時刻が近い順
                        .FirstOrDefault();

                    // --- 表示文字列の作成 ---
                    string dateTimeText = "";
                    string serviceName = "";
                    string title = "";
                    bool isRecording = false;


                    if (nearestReserve != null)
                    {
                        DateTime start = nearestReserve.StartTime;
                        DateTime end = start.AddSeconds(nearestReserve.DurationSecond);

                        // 録画中判定
                        isRecording = start <= DateTime.Now && end >= DateTime.Now;

                        dateTimeText = $"{start:MM/dd(ddd) HH:mm}-{end:HH:mm}";
                        serviceName = nearestReserve.StationName;
                        title = nearestReserve.Title;

                        // --- 次回更新時刻の候補判定 ---
                        // 開始時刻が現在以降かつ、これまでの最小値より近ければ更新
                        if (start >= now && start < nearestEventTime)
                        {
                            nearestEventTime = start;
                        }
                        // 終了時刻が現在以降かつ、これまでの最小値より近ければ更新
                        if (end >= now && end < nearestEventTime)
                        {
                            nearestEventTime = end;
                        }
                    }

                    // --- ListViewItem の生成 ---
                    string[] rowData = { tunerName, dateTimeText, serviceName, title };
                    ListViewItem item = new ListViewItem(rowData)
                    {
                        Name = tuner.tunerID.ToString(),
                        ToolTipText = $"{dateTimeText} {serviceName} {title}",
                        ForeColor = this.foreColor
                    };

                    // 背景色判定
                    // 直近30件に限らず、将来変な予約がある場合警告として色を変える
                    if (reserves.Any(x => x.OverlapMode == 1))
                    {
                        // 一部予約に1件でも予約が入っている場合、黃背景色で警告
                        item.BackColor = this.partialReserveListBackColor;
                    }
                    else if (reserves.Any(x => x.OverlapMode == 2))
                    {
                        // TU不足に1件でも予約が入っている場合、赤背景色で警告
                        item.BackColor = this.ngReserveListBackColor;
                    }
                    else if (isRecording)
                    {
                        // 録画中の場合、正常予約背景
                        item.BackColor = this.okReserveListBackColor;
                    }
                    else
                    {
                        item.BackColor = this.listBackColor;
                    }

                    tunerListView.Items.Add(item);
                }

                // --- 次回更新時間の設定 ---
                // 現在以降のイベント（開始 or 終了）を設定
                if (nearestEventTime != DateTime.MaxValue)
                {
                    this.nextReserveListRefreshTime = nearestEventTime;
                }

                // --- 列幅の設定（設定ファイルの最長文字数に基づく計算） ---
                // 列0: チューナー名の最長幅
                int col0Width = maxTunerNameWidth + columnPadding;
                
                // 列1: 日時表記の固定幅
                int col1Width = TextRenderer.MeasureText("00/00(水) 00:00-00:00", tunerListView.Font).Width + columnPadding;

                // 列2: チャンネル名の最長幅
                int col2Width = maxServiceNameWidth + columnPadding;

                tunerListView.Columns[0].Width = col0Width;
                tunerListView.Columns[1].Width = col1Width;
                tunerListView.Columns[2].Width = col2Width;

                // 列3: 番組名（残りの幅をすべて充当）
                int remainingWidth = tunerListView.ClientSize.Width - (col0Width + col1Width + col2Width);
                tunerListView.Columns[3].Width = Math.Max(100, remainingWidth - 2);
            }
            finally
            {
                // 描画を再開
                tunerListView.EndUpdate();
            }
            // 前回垂直スクロールバーがない状態からある状態になると水平スクロールバーが出るため再調整
            adjustListViewColumns(tunerListView);
        }

        /// <summary>
        /// フォントに応じた適切な余白（パディング）幅を取得する
        /// </summary>
        private int GetColumnPadding(Font font)
        {
            // "M"（比較的大幅な文字）の幅を基準に余白を計算
            // フォントサイズやDPIが大きくなれば、この幅も自動的に大きくなる
            int charWidth = TextRenderer.MeasureText("M", font).Width;
            return charWidth*8/14;
        }

        /// <summary>
        /// 設定ファイル内のチューナー名定義から最長ピクセル幅を取得する
        /// </summary>
        private int GetMaxTunerNameWidth(Font font)
        {
            int maxWidth = 0;

            if (rockbarSetting?.BonDriverNameToTunerName != null)
            {
                foreach (var name in rockbarSetting.BonDriverNameToTunerName.Values)
                {
                    if (string.IsNullOrEmpty(name))
                        continue;

                    int width = TextRenderer.MeasureText(name, font).Width;
                    if (width > maxWidth)
                    {
                        maxWidth = width;
                    }
                }
            }

            if (maxWidth == 0)
            {
                maxWidth = TextRenderer.MeasureText("BS/CS1", font).Width;
            }

            maxTunerNameWidth = maxWidth;
            return maxTunerNameWidth;
        }

        /// <summary>
        /// 設定ファイル内の全サービスリストから最長チャンネル名のピクセル幅を取得する
        /// </summary>
        private int GetMaxServiceNameWidth(Font font)
        {
            int maxWidth = 0;

            if (allServiceList != null)
            {
                foreach (var service in allServiceList)
                {
                    if (string.IsNullOrEmpty(service.Name))
                        continue;
                    
                    int width = TextRenderer.MeasureText(service.Name, font).Width;
                    if (width > maxWidth)
                    {
                        maxWidth = width;
                    }
                }
            }

            if (maxWidth == 0)
            {
                maxWidth = TextRenderer.MeasureText("ＮＨＫ総合１・東京", font).Width;
            }

            maxServiceNameWidth = maxWidth;
            return maxServiceNameWidth;
        }

        /// <summary>
        /// 設定ファイルの再読み込み処理
        /// </summary>
        private void ReloadSetting()
        {
            // 設定ファイルを書き込んだあとの読み込みなので基本的に例外は発生しないはず。発生した場合は例外を投げる
            rockbarSetting = Toml.ReadFile<RockBarSetting>(RockbarUtility.GetTomlSettingFilePath());

            allServiceList = RockbarUtility.GetAllServicesFromSetting();
            favoriteServiceList = RockbarUtility.GetFavoriteServicesFromSetting();
        }

        /// <summary>
        /// フィルタ文字列が検索対象の文字列群に含まれるか判定する
        /// </summary>
        /// <param name="targets">検索対象の文字列（チャンネル名、番組名等）</param>
        /// <returns>表示対象ならtrue</returns>
        private bool IsMatchFilter(params string[] targets)
        {
            var filterText = filterTextBox.Text;

            // フィルタ文字列が空の場合は全件表示対象とする
            if (string.IsNullOrWhiteSpace(filterText))
            {
                return true;
            }

            // スペース区切りによるAND検索に対応するため分割する
            var separators = new char[] { ' ', ' ' };
            var filterWords = filterText.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            // すべてのキーワードが含まれているか検証する
            foreach (var word in filterWords)
            {
                bool wordMatched = false;
                foreach (var target in targets)
                {
                    if (!string.IsNullOrEmpty(target) && target.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        wordMatched = true;
                        break;
                    }
                }

                // 1つでも一致しないキーワードがあれば除外対象とする
                if (!wordMatched)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// フィルタのリセット処理
        /// フィルタ文字列をクリアしフィルタ結果をリセットする。
        /// </summary>
        private void ResetFilter()
        {
            filterTextBox.Clear();

            // フィルタテキストが存在する場合はラベルを表示してフィルタ中であることを通知する
            filteringLabel.Visible = !string.IsNullOrWhiteSpace(filterTextBox.Text);

            RefreshEvent(true, false);
        }

        /// <summary>
        /// 録画モード有効・無効の切り替え処理
        /// 録画有効時は無効化、無効時は有効化する。
        /// </summary>
        private void ToggleRecMode(ReserveData reserve)
        {
            if (reserve.RecSetting.IsNoRec())
            {
                reserve.RecSetting.RecMode = rockbarSetting.FixNoRecToServiceOnly ? (byte)1 : reserve.RecSetting.GetRecMode();
            }
            else
            {
                // 録画モード情報を維持して無効化
                var recMode = reserve.RecSetting.RecMode;
                reserve.RecSetting.RecMode = (byte)(rockbarSetting.FixNoRecToServiceOnly ? 5 : 5 + (recMode + 4) % 5);
            }

            var err = ctrlCmdUtil.SendChgReserve(new List<ReserveData>() { reserve });
            if (err != ErrCode.CMD_SUCCESS)
            {
                MessageBox.Show("予約変更でエラーが発生しました。", "予約変更エラー");
            }
            RefreshEvent(false, true);
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
                // チューナーから開いた場合は予約情報を表示(必ず予約情報あり)
                item.Text = reserve.StartTime.ToString("MM/dd HH:mm") + "～" + reserve.StartTime.AddSeconds(reserve.DurationSecond).ToString("HH:mm") + "  " +
                    reserveString + "    " + reserve.StationName + "    " + reserve.Title;
            }
            else
            {
                // サービスから開いた場合は番組情報を表示(必ず番組情報あり)
                item.Text = ev.start_time.ToString("MM/dd HH:mm") + "～" + ev.start_time.AddSeconds(ev.durationSec).ToString("HH:mm") + "  " +
                    reserveString + "  " + ev.ShortInfo?.event_name;
            }

            // 番組情報がある場合はサブメニューに番組情報を追加
            if (ev != null)
            {
                // Webリンク使用時のみ1行目にWebリンク用のボタンを表示
                if (rockbarSetting.UseWebLink)
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
                var text = $"{recFile.StartTime.ToString("yyyy/MM/dd(ddd) HH:mm")}-{recFile.StartTime.AddSeconds(recFile.DurationSecond).ToString("HH:mm")}";
                var dateTimes = new List<RecInfoDetailText>()
                    {
                        new RecInfoDetailText { Value = text, CopyText = text },
                    };
                detailTexts.Add(dateTimes);
            }

            var services = new List<RecInfoDetailText>()
                {
                    new RecInfoDetailText { Value = recFile.ServiceName, CopyText = recFile.ServiceName },
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
            string url = rockbarSetting.WebLinkUrl;
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
            string url = rockbarSetting.RecInfoWebLinkUrl;
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
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            else if (isFiltering)
            {
                ResetFilter();
            }

            filterTextBox.Text = text;
            filterTextBox.Focus();
            RefreshEvent(true, false);
        }

        /// <summary>
        /// リストビューカラム幅調整処理
        /// 1列目を内容に合わせ、最終列コントロールいっぱいまで広げる。
        /// </summary>
        /// <param name="listView">対象リストビュー</param>
        private void adjustListViewColumns(ListView listView)
        {
            // 1列目を内容で広げる
            //listView.AutoResizeColumn(0, ColumnHeaderAutoResizeStyle.ColumnContent);

            // 1列目・最終列以外は固定幅
            int columnWidth = 0;

            for (var i = 0; i < listView.Columns.Count - 1; i++)
            {
                columnWidth += listView.Columns[i].Width;
            }

            // 最終列をいっぱいに広げる
            listView.Columns[listView.Columns.Count - 1].Width = listView.ClientRectangle.Width - columnWidth - 2;
        }

        /// <summary>
        /// TVTest起動処理
        /// TSID, SIDを指定してTVTestを起動する。地デジ・BS/CSで異なるオプションを使用する。
        /// </summary>
        /// <param name="isDttv">地デジ？</param>
        /// <param name="tsid">TSID</param>
        /// <param name="sid">SID</param>
        /// <returns>TVTestプロセス</returns>
        private System.Diagnostics.Process startTvTest(bool isDttv, uint tsid, uint sid)
        {
            System.Diagnostics.Process result = null;

            try
            {
                if (isDttv)
                {
                    result = System.Diagnostics.Process.Start(rockbarSetting.TvtestPath, $"{rockbarSetting.TvtestDttvOption} /tsid {tsid} /sid {sid}");
                }
                else
                {
                    result = System.Diagnostics.Process.Start(rockbarSetting.TvtestPath, $"{rockbarSetting.TvtestBscsOption} /tsid {tsid} /sid {sid}");
                }
            }
            catch
            {
                MessageBox.Show("TVTestの起動に失敗しました。TVTestの設定を見直してください。", "TVTest起動エラー");
            }

            return result;
        }

        /// <summary>
        /// TVTest起動処理
        /// TSID, SIDを指定してTVTestを起動する。地デジ・BS/CSで異なるオプションを使用する。
        /// </summary>
        /// <param name="isDttv">地デジ？</param>
        /// <param name="tsid">TSID</param>
        /// <param name="sid">SID</param>
        /// <param name="tvtestOption">TVTest起動オプション（オプション）</param>
        /// <returns>TVTestプロセス</returns>
        private System.Diagnostics.Process startTvTest(bool isDttv, uint tsid, uint sid, string tvtestOption = null)
        {
            System.Diagnostics.Process result = null;

            try
            {
                if (tvtestOption != null)
                {
                    result = System.Diagnostics.Process.Start(rockbarSetting.TvtestPath, $"{tvtestOption} /tsid {tsid} /sid {sid}");
                }
                else if (isDttv)
                {
                    result = System.Diagnostics.Process.Start(rockbarSetting.TvtestPath, $"{rockbarSetting.TvtestDttvOption} /tsid {tsid} /sid {sid}");
                }
                else
                {
                    result = System.Diagnostics.Process.Start(rockbarSetting.TvtestPath, $"{rockbarSetting.TvtestBscsOption} /tsid {tsid} /sid {sid}");
                }
            }
            catch
            {
                MessageBox.Show("TVTestの起動に失敗しました。TVTestの設定を見直してください。", "TVTest起動エラー");
            }
            return result;
        }

        /// <summary>
        /// TvtPlayプラグインを有効化してTVTest起動処理
        /// ファイルパスとTvtPlayの起動オプションを指定してTVTestを起動する。
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>TVTestプロセス</returns>
        private System.Diagnostics.Process startTvtPlay(string filePath)
        {
            System.Diagnostics.Process result = null;

            try
            {
                result = System.Diagnostics.Process.Start(rockbarSetting.TvtestPath, $"{rockbarSetting.TvtestTsFileOption} \"{filePath}\"");
            }
            catch
            {
                MessageBox.Show("TVTestの起動に失敗しました。TVTestの設定を見直してください。", "TVTest起動エラー");
            }

            return result;
        }

        /// <summary>
        /// チャンネル一覧マウスクリック時処理
        /// 右クリック時、直近30件の番組情報をコンテキストメニューに表示。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void serviceListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (serviceTabControl.SelectedTab == reserveTabPage)
            {
                reserveListView_MouseClick(sender, e);
                return;
            }
            else if (serviceTabControl.SelectedTab == recTabPage)
            {
                recListView_MouseClick(sender, e);
                return;
            }

            // 右クリック
            // 現在の番組を含め、今後の番組を30件までコンテキストメニューで表示(TVRockの仕様踏襲)
            if (e.Button == MouseButtons.Right)
            {
                // クリックした箇所が自動選択されるので拾う
                var selected = serviceListView.SelectedItems[0];

                listContextMenuStrip.Items.Clear();

                EpgServiceEventInfo sv = null;
                serviceMap.TryGetValue(selected.Name, out sv);

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
                    
                    if (reserveMap.ContainsKey(eventKey))
                    {
                        reserveData = reserveMap[eventKey];
                    }

                    listContextMenuStrip.Items.Add(createEventToolStripMenuItem(ev, reserveData, false));

                    i++;

                    if (i >= 30)
                    {
                        break;
                    }
                }

                listContextMenuStrip.Show((Control) sender, new Point(0, e.Y) );
            }
        }

        /// <summary>
        /// チャンネル一覧マウスアップ時処理
        /// 中央ボタンのクリック検出用。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void serviceListView_MouseUp(object sender, MouseEventArgs e)
        {
            if (serviceTabControl.SelectedTab == reserveTabPage)
            {
                reserveListView_MouseUp(sender, e);
                return;
            }
        }

        /// <summary>
        /// チャンネル一覧マウスダブルクリック処理
        /// TVTest使用オプションがONの場合、カーソル箇所の番組を対象にTVTestを起動する。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void serviceListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (serviceTabControl.SelectedTab == reserveTabPage)
            {
                reserveListView_MouseDoubleClick(sender, e);
                return;
            }
            else if (serviceTabControl.SelectedTab == recTabPage)
            {
                recListView_MouseDoubleClick(sender, e);
                return;
            }

            // TVTest使用時のみ
            if (! rockbarSetting.UseDoubleClickTvtest)
            {
                return;
            }

            // 左ダブルクリック
            // TVTestを起動する
            if (e.Button == MouseButtons.Left)
            {
                // クリックした箇所が自動選択されるので拾う
                var selected = serviceListView.SelectedItems[0];

                Service service = selected.Tag as Service;
                EpgServiceEventInfo sv = null;
                serviceMap.TryGetValue(selected.Name, out sv);

                if (service == null)
                {
                    return;
                }

                uint tsid = uint.Parse(service.Tsid);
                uint sid = uint.Parse(service.Sid);

                ServiceType serviceType = sv != null
                    ? RockbarUtility.GetServiceType(service.Type, sv.serviceInfo.ONID)
                    : RockbarUtility.GetServiceType(service.Type, null);

                if (serviceType == ServiceType.DTTV) {
                    // 地上波
                    startTvTest(true, tsid, sid, service.TvtestOption);
                }
                else
                {
                    // BS, CS
                    startTvTest(false, tsid, sid, service.TvtestOption);
                }
            }
        }

        /// <summary>
        /// チャンネル一覧および予約一覧選択状態変更処理
        /// ヘッダを選択しようとした場合、選択状態を解除する。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void serviceListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected && string.IsNullOrEmpty(e.Item.Name))
            {
                e.Item.Selected = false;
            }
        }

        /// <summary>
        /// 予約一覧マウスクリック処理
        /// 右クリック時、対象の番組情報をコンテキストメニューに表示。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void reserveListView_MouseClick(object sender, MouseEventArgs e)
        {
            var selectedCount = serviceListView.SelectedItems.Count;
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
                    var selected = serviceListView.SelectedItems[0];

                    EpgEventInfo ev = allEventMap[selected.Name];

                    // Webリンク使用時のみ1行目にWebリンク用のボタンを表示
                    if (rockbarSetting.UseWebLink)
                    {
                        var item = listContextMenuStrip.Items.Add(">>");
                        item.Click += (s2, e2) => accessWebUrl(ev);

                        listContextMenuStrip.Items.Add(new ToolStripSeparator());
                    }

                    // 有効化・無効化を追加
                    var hasData = reserveMap.TryGetValue(selected.Name, out var reserveData);
                    if (hasData)
                    {
                        var item = listContextMenuStrip.Items.Add(reserveData.RecSetting.IsNoRec() ? "録画を有効にする" : "録画を無効にする");
                        item.Click += (s2, e2) => ToggleRecMode(reserveData);
                        listContextMenuStrip.Items.Add(new ToolStripSeparator());
                    }

                    var dateTime = $"{ev.start_time.ToString("yyyy/MM/dd(ddd) HH:mm")}-{ev.start_time.AddSeconds(ev.durationSec).ToString("HH:mm")}";
                    var dateItem = listContextMenuStrip.Items.Add(dateTime);
                    dateItem.Click += (s2, e2) => copyText(dateTime);
                    listContextMenuStrip.Items.Add(new ToolStripSeparator());

                    // 予約情報をメニューに追加
                    if (hasData)
                    {
                        const string copyCommandText = "テキストをコピー";
                        const string copyCommandTooltipText = "クリックでテキストをコピー";
                        const string filterCommandText = "フィルタリング";
                        // サービス名を追加
                        if (! string.IsNullOrEmpty(reserveData.StationName))
                        {
                            var item = new ToolStripMenuItem(reserveData.StationName);
                            listContextMenuStrip.Items.Add(item);
                            var subItem = item.DropDownItems.Add(copyCommandText);
                            subItem.Click += (s2, e2) => copyText(reserveData.StationName);
                            subItem = item.DropDownItems.Add(filterCommandText);
                            subItem.Click += (s2, e2) => filterWith(reserveData.StationName);
                        }
                        // 予約番組名を追加
                        if (! string.IsNullOrEmpty(reserveData.Title))
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
                        if (! string.IsNullOrEmpty(reserveData.Comment))
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
                                if (! string.IsNullOrEmpty(recInfo.RecFolder))
                                {
                                    var item = listContextMenuStrip.Items.Add(recInfo.RecFolder);
                                    item.Click += (s2, e2) => copyText(recInfo.RecFolder);
                                    item.ToolTipText = copyCommandTooltipText;
                                }
                            }
                            foreach (var fileName in reserveData.RecFileNameList)
                            {
                                if (! string.IsNullOrEmpty(fileName))
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
        private void reserveListView_MouseUp(object sender, MouseEventArgs e)
        {
            // 中央クリック
            // 予約情報の録画有効・無効を切り替える
            if (e.Button == MouseButtons.Middle)
            {
                // クリックした箇所にある項目を取得する
                var selected = serviceListView.GetItemAt(e.Location.X, e.Location.Y);

                if (selected != null && reserveMap.TryGetValue(selected.Name, out var reserve))
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
        private void reserveListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Web番組詳細使用時のみ
            if (! rockbarSetting.UseWebLink)
            {
                return;
            }

            var selectedCount = serviceListView.SelectedItems.Count;
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
                    var selected = serviceListView.SelectedItems[0];

                    EpgEventInfo ev = allEventMap[selected.Name];
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
        private void recListView_MouseClick(object sender, MouseEventArgs e)
        {
            var selectedCount = serviceListView.SelectedItems.Count;
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
                    var selected = serviceListView.SelectedItems[0];
                    uint recID;
                    if (! uint.TryParse(selected.Name, out recID))
                    {
                        MessageBox.Show("録画情報IDの取得に失敗しました。", "録画情報IDエラー");
                        return;
                    }

                    RecFileInfo recFile = recMap[recID];

                    // Webリンク使用時のみ1行目にWebリンク用のボタンを表示
                    if (rockbarSetting.UseWebLink)
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
        private void recListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // TVTest使用時のみ
            if (! rockbarSetting.UseDoubleClickTvtest)
            {
                return;
            }

            var selectedCount = serviceListView.SelectedItems.Count;
            if (selectedCount == 0)
            {
                return;
            }

            // 左ダブルクリック
            // TVTestを起動する
            if (e.Button == MouseButtons.Left)
            {
                // クリックした箇所が自動選択されるので拾う
                var selected = serviceListView.SelectedItems[0];

                uint recID;
                if (! uint.TryParse(selected.Name, out recID))
                {
                    MessageBox.Show("録画情報IDの取得に失敗しました。", "録画情報IDエラー");
                    return;
                }

                RecFileInfo recFile = recMap[recID];

                if (rockbarSetting.UseTcpIp && rockbarSetting.IpAddress.IndexOf("127.0.0.1") < 0)
                {
                    string networkPath = "";
                    ErrCode errCode = ctrlCmdUtil.SendGetRecFileNetworkPath(recFile.RecFilePath, ref networkPath);
                    if (errCode != ErrCode.CMD_SUCCESS || string.IsNullOrEmpty(networkPath))
                    {
                        MessageBox.Show("ネットワークパスの取得に失敗しました。EDCBの設定を見直してください。", "ネットワークパスエラー");
                        return;
                    }

                    // TvtPlay
                    startTvtPlay(networkPath);
                }
                else
                {
                    // TvtPlay
                    startTvtPlay(recFile.RecFilePath);
                }
            }
        }

        /// <summary>
        /// チューナー一覧マウスクリック時処理
        /// 右クリック時、直近30件の予約情報をコンテキストメニューに表示。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void tunerListView_MouseClick(object sender, MouseEventArgs e)
        {
            // 右クリック
            // 今後の予約を30件までコンテキストメニューで表示
            //TODO DRY
            if (e.Button == MouseButtons.Right)
            {
                // クリックした箇所が自動選択されるので拾う
                var selected = tunerListView.SelectedItems[0];

                listContextMenuStrip.Items.Clear();

                TunerReserveInfo tunerReserveInfo = tunerReserveInfos.Find((TunerReserveInfo x) => x.tunerID.ToString() == selected.Name);

                // TunerReserveInfoには予約IDしか入っていないので、ReserveDataから予約情報を取り直す
                HashSet<uint> reserveIds = tunerReserveInfo.reserveList.ToHashSet();

                var reserves = reserveDatas.FindAll(x => reserveIds.Contains(x.ReserveID)).OrderBy(x => x.StartTime);

                int i = 0;

                foreach (var reserveData in reserves)
                {
                    string key = RockbarUtility.GetKey(reserveData.TransportStreamID, reserveData.ServiceID, reserveData.EventID);

                    EpgEventInfo ev = null;

                    if (allEventMap.ContainsKey(key))
                    {
                        ev = allEventMap[key];
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
                RefreshEvent(false, true);
            }
            
            if (! rockbarSetting.IsAutoOpenTvtest)
            {
                return;
            }

            // TVTest自動起動
            // 実実装としては毎秒チェックするのではなく、毎分(59-マージン)秒タイミングで次の1分間に始まる番組をオープンする
            if (timerTime.Second == (59 - rockbarSetting.AutoOpenMargin)) {
                // 0の場合はこの1分間なので59秒加算
                DateTime checkTime = timerTime.AddSeconds(59);

                // お気に入りサービスのキー(大した件数ではない想定なので毎秒計算し直しで良いものとする)
                HashSet<string> favoriteServiceKeys = favoriteServiceList.Select(x => RockbarUtility.GetKey(x.Tsid, x.Sid)).ToHashSet();

                foreach (var reserve in reserveDatas)
                {
                    if (reserve.StartTime.Date == checkTime.Date && reserve.StartTime.Hour == checkTime.Hour && reserve.StartTime.Minute == checkTime.Minute)
                    {
                        string key = RockbarUtility.GetKey(reserve.TransportStreamID, reserve.ServiceID);

                        // すでに自動起動中のTVTestとTSID・SIDが同一の場合、(TVTest側でチャンネルが変わっていない前提で)起動スキップする
                        if (tvtestProcesses.ContainsKey(key))
                        {
                            continue;
                        }

                        // お気に入りサービスオプションが設定されている場合、お気に入りサービスにキーが含まれていなかったらスキップ
                        if (rockbarSetting.IsAutoOpenTvtestFavoriteService && ! favoriteServiceKeys.Contains(key)) 
                        {
                            continue;
                        }

                        // 地上波
                        if (RockbarUtility.GetServiceType(reserve.OriginalNetworkID) == ServiceType.DTTV && rockbarSetting.IsAutoOpenTvtestDttv)
                        {
                            Service service = allServiceList.FirstOrDefault(x => RockbarUtility.GetKey(reserve.TransportStreamID, reserve.ServiceID) == key);
                            var p = startTvTest(true, reserve.TransportStreamID, reserve.ServiceID, service?.TvtestOption);
                            tvtestProcesses.Add(key, p);
                        }

                        // BS, CS
                        if (
                            RockbarUtility.GetServiceType(reserve.OriginalNetworkID) == ServiceType.BS && rockbarSetting.IsAutoOpenTvtestBs ||
                            RockbarUtility.GetServiceType(reserve.OriginalNetworkID) == ServiceType.CS && rockbarSetting.IsAutoOpenTvtestCs
                        )
                        {
                            Service service = allServiceList.FirstOrDefault(x => RockbarUtility.GetKey(reserve.TransportStreamID, reserve.ServiceID) == key);
                            var p = startTvTest(false, reserve.TransportStreamID, reserve.ServiceID, service?.TvtestOption);
                            tvtestProcesses.Add(key, p);
                        }
                    }
                }
            }

            // TVTest自動終了
            // 現時点のオプションにかかわらず、自身が開いたTVTestは予約終了時間でクローズ
            // 実実装としては毎秒チェックするのではなく、毎分マージン秒タイミングで現在放送してない番組をクローズ
            if (timerTime.Second == rockbarSetting.AutoCloseMargin)
            {
                // 閉じてるプロセスは取り除く
                var keys = tvtestProcesses.Keys.ToList();

                foreach (var key in keys)
                {
                    try
                    {
                        if (tvtestProcesses[key].HasExited)
                        {
                            tvtestProcesses.Remove(key);
                        }
                    }
                    catch
                    {
                        // 何らかの理由でプロセスにアクセスできない場合もキーを削除
                        tvtestProcesses.Remove(key);
                    }
                }

                // 把握してる生存プロセスの中で、録画放送に該当してるものがない場合はクローズ
                Dictionary<string, ReserveData> currentReserves = new Dictionary<string, ReserveData>();

                // 現在放送中番組を抽出
                foreach (var data in reserveDatas)
                {
                    // 途中処理があまりに遅いと、タイマー開始時とNowでズレが生じる可能性あり。問題がでたら検討
                    if (data.StartTime < DateTime.Now && data.StartTime.AddSeconds(data.DurationSecond) > DateTime.Now)
                    {
                        var key = RockbarUtility.GetKey(data.TransportStreamID, data.ServiceID);

                        // 予約方法次第で同一番組が二重に登録されているケースあり
                        if (! currentReserves.ContainsKey(key))
                        {
                            currentReserves.Add(key, data);
                        }
                    }
                }

                // 放送中番組でないTVTestを閉じる
                foreach (var p in tvtestProcesses)
                {
                    if (! currentReserves.ContainsKey(p.Key))
                    {
                        try
                        {
                            p.Value.CloseMainWindow();
                        }
                        catch
                        {
                            // 何らかの理由でプロセスがクローズできない場合
                            // 想定外ケースなのでここに落ちる場合は原因究明要
                        }
                    }
                }
            }
        }

        /// <summary>
        /// チャンネルタブ切り替え処理
        /// EpgTimerSrv通信せずに、対象チャンネルの表示切替を行う
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void serviceTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshEvent(true, false);
        }

        /// <summary>
        /// ✕ボタン押下処理
        /// タスクトレイ格納オプションON時、タスクトレイに格納。そうでない場合、フォームを閉じる
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void closeButton_Click(object sender, EventArgs e)
        {
            if (rockbarSetting.StoreTaskTrayByClosing)
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
        /// Enter入力時にフィルタを行う。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void filterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // フィルタテキストが存在する場合はラベルを表示してフィルタ中であることを通知する
                filteringLabel.Visible = !string.IsNullOrWhiteSpace(filterTextBox.Text);

                RefreshEvent(true, false);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                ResetFilter();
                e.SuppressKeyPress = true;
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
        /// フィルタボタン押下処理
        /// フィルタを行う。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void filterButton_Click(object sender, EventArgs e)
        {
            // フィルタテキストが存在する場合はラベルを表示してフィルタ中であることを通知する
            filteringLabel.Visible = !string.IsNullOrWhiteSpace(filterTextBox.Text);

            RefreshEvent(true, false);
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
                ReloadSetting();
                applySetting();
                RefreshEvent(true, true);
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
            rockbarSetting.X = this.Location.X;
            rockbarSetting.Y = this.Location.Y;
            rockbarSetting.Width = this.Size.Width;
            rockbarSetting.Height = this.Size.Height;
            rockbarSetting.SplitterDistance = splitContainer.SplitterDistance;

            Toml.WriteFile(rockbarSetting, RockbarUtility.GetTomlSettingFilePath());
        }

        /// <summary>
        /// タスクトレイ終了コンテキストメニュークリック処理
        /// フォームを閉じる。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// タスクトレイアイコンマウスボタン押下処理
        /// トグルオプションONの場合、表示・非表示切り替え＋アクティブ化。それ以外の場合アクテイブ化のみ
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void notifyIcon_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                if (this.Visible)
                {
                    // フォーム表示時に左クリックした場合はオプションにより挙動切り替え
                    if (rockbarSetting.ToggleVisibleTaskTrayIconClick)
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
                    if (!rockbarSetting.ShowTaskTrayIcon)
                    {
                        notifyIcon.Visible = false;
                    }
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
            adjustListViewColumns(serviceListView);
            adjustListViewColumns(tunerListView);
        }

        /// <summary>
        /// スプリッタ位置調整処理
        /// 左右のリストビューのカラム幅を調整する。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void splitContainer_SplitterMoved(object sender, SplitterEventArgs e)
        {
            adjustListViewColumns(serviceListView);
            adjustListViewColumns(tunerListView);
        }

        /// <summary>
        /// フォーム初回表示完了時処理
        /// 実際の画面描画サイズ確定後にリストビューの列幅を再調整して水平スクロールバーを防ぐ。
        /// </summary>
        private void MainForm_Shown(object sender, EventArgs e)
        {
            adjustListViewColumns(serviceListView);
            adjustListViewColumns(tunerListView);
        }
    }
}
