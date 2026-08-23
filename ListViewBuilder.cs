using EpgTimer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RockbarForEDCB
{
    /// <summary>
    /// ListViewの構築および描画ロジックを担当するクラス
    /// </summary>
    public class ListViewBuilder
    {
        private readonly ConfigManager _configManager;
        private readonly EpgDataManager _epgDataManager;

        // 色設定
        private Color _foreColor;
        private Color _listBackColor;
        private Color _okReserveListBackColor;
        private Color _partialReserveListBackColor;
        private Color _ngReserveListBackColor;
        private Color _disabledReserveListBackColor;
        private Color _listHeaderForeColor;
        private Color _listHeaderBackColor;

        // レイアウト設定
        private int _columnPadding = -1;
        private int _maxServiceNameWidth = -1;
        private int _maxTunerNameWidth = -1;

        /// <summary>
        /// 検索フィルタ文字列
        /// </summary>
        public string FilterText { get; set; } = string.Empty;

        /// <summary>
        /// 録画済み情報のツールチップテキスト作成デリゲート
        /// </summary>
        //public Func<RecFileInfo, string> CreateRecInfoTooltipTexts { get; set; }

        public ListViewBuilder(ConfigManager configManager, EpgDataManager epgDataManager)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _epgDataManager = epgDataManager ?? throw new ArgumentNullException(nameof(epgDataManager));
        }

        /// <summary>
        /// フォントや色やレイアウト設定を更新
        /// </summary>
        public void ApplySettings()
        {
            var setting = _configManager.RockbarSetting;
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            // 色情報の読み込み・保持
            _foreColor = (Color)colorConverter.ConvertFromString(setting.ForeColor);
            _listBackColor = (Color)colorConverter.ConvertFromString(setting.ListBackColor);
            _okReserveListBackColor = (Color)colorConverter.ConvertFromString(setting.OkReserveListBackColor);
            _partialReserveListBackColor = (Color)colorConverter.ConvertFromString(setting.PartialReserveListBackColor);
            _ngReserveListBackColor = (Color)colorConverter.ConvertFromString(setting.NgReserveListBackColor);
            _disabledReserveListBackColor = (Color)colorConverter.ConvertFromString(setting.DisabledReserveListBackColor);
            _listHeaderForeColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.ListHeaderForeColor);
            _listHeaderBackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.ListHeaderBackColor);

            // フォント情報の読み込みとカラム幅の計算
            TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));
            Font font = (Font)fontConverter.ConvertFromString(setting.Font);

            _columnPadding = GetColumnPadding(font);
            _maxServiceNameWidth = GetMaxServiceNameWidth(font);
            _maxTunerNameWidth = GetMaxTunerNameWidth(font);
        }

        /// <summary>
        /// チャンネル一覧の生成
        /// 全て、地デジ、BS、CS（ListView）の生成および描画を行い、次回画面更新が必要となる最速の時刻を取得します。
        /// </summary>
        /// <param name="targetListView">描画対象のListView</param>
        /// <param name="selectedTab">現在選択中のタブ</param>
        /// <returns>次回更新が必要になる時刻</returns>
        public DateTime BuildServiceList(ListView targetListView, MainFormTabType selectedTab)
        {
            DateTime now = DateTime.Now;
            DateTime minNextRefreshTime = DateTime.MaxValue;

            // チラつきを抑えるため描画を停止
            targetListView.BeginUpdate();

            try
            {
                // 一覧を全件クリア
                targetListView.Items.Clear();

                List<Service> services = (selectedTab == MainFormTabType.Favorite)
                     ? _configManager.FavoriteServiceList
                     : _configManager.SelectedServiceList;

                // チャンネル表示
                foreach (var service in services)
                {
                    string key = RockbarUtility.GetKey(service.Tsid, service.Sid);

                    _epgDataManager.ServiceMap.TryGetValue(key, out EpgServiceEventInfo matchedService);

                    // TSVのチャンネル一覧で設定されているネットワークタイプを最優先で使用する
                    // ネットワークタイプ設定が無い場合はEDCBからのデータをもとに自動判別
                    NetworkType networkType = matchedService != null
                        ? RockbarUtility.GetNetworkType(service.TypeName, matchedService.serviceInfo.ONID)
                        : RockbarUtility.GetNetworkType(service.TypeName, null);

                    // 選択中タブ（地デジ / BS / CS）と不一致のサービスは除外する
                    if (selectedTab == MainFormTabType.DTTV && networkType != NetworkType.DTTV) continue;
                    if (selectedTab == MainFormTabType.BS && (networkType != NetworkType.BS && networkType != NetworkType.BS4K)) continue;
                    if (selectedTab == MainFormTabType.CS && (networkType != NetworkType.CS && networkType != NetworkType.SPHD)) continue;

                    // 設定ファイルのチャンネル名を最優先で使用し、設定がなければEDCBのStationNameを使用
                    string serviceName = GetServiceName(ushort.Parse(service.Tsid), ushort.Parse(service.Sid));

                    // 現在放送中の番組を取得
                    EpgEventInfo ev = matchedService?.eventList.Find(x =>
                        x.start_time <= DateTime.Now &&
                        x.start_time.AddSeconds(x.durationSec) >= DateTime.Now);

                    // EPG情報が無い場合と現在放送中の番組が無い場合のタイトル設定
                    string eventTitle = ev?.ShortInfo?.event_name ?? (matchedService == null ? "EPG未取得" : "");

                    // フィルタ条件チェック（チャンネル名・番組名の双方を対象とする）
                    if (!IsMatchFilter(new[] { serviceName, eventTitle }))
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
                        if (_epgDataManager.ReserveMap.TryGetValue(eventKey, out ReserveData reserveData))
                        {
                            reserveStatus = GetReserveStatus(reserveData);
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
                        ForeColor = _foreColor
                    };

                    // 色変更
                    if (ev != null)
                    {
                        item.BackColor = GetReserveBackColor(reserveStatus, ev.start_time, ev.start_time.AddSeconds(ev.durationSec));
                    }

                    targetListView.Items.Add(item);

                    // 次に始まる最初の番組の「開始時刻」を探す
                    if (matchedService?.eventList != null)
                    {
                        var nextEv = matchedService.eventList
                            .Where(x => x.start_time > now)
                            .OrderBy(x => x.start_time)
                            .FirstOrDefault();

                        if (nextEv != null && nextEv.start_time < minNextRefreshTime)
                        {
                            minNextRefreshTime = nextEv.start_time;
                        }
                    }
                }

                // --- 列幅の設定 ---
                // 列0: チャンネル名の最長幅
                int col0Width = _maxServiceNameWidth + _columnPadding;

                // 列1: 時間表記の固定幅
                int col1Width = TextRenderer.MeasureText("00:00-00:00", targetListView.Font).Width + _columnPadding;

                // 列2: 予約状態
                int col2Width = TextRenderer.MeasureText("◎", targetListView.Font).Width + _columnPadding;

                targetListView.Columns[0].Width = col0Width;
                targetListView.Columns[1].Width = col1Width;
                targetListView.Columns[2].Width = col2Width;

                // 列3: 番組名（残りの幅をすべて充当）
                int remainingWidth = targetListView.ClientSize.Width - (col0Width + col1Width + col2Width);
                targetListView.Columns[3].Width = Math.Max(100, remainingWidth - 2);
            }
            finally
            {
                // 描画を再開
                targetListView.EndUpdate();
            }
            // 前回垂直スクロールバーがない状態からある状態になると水平スクロールバーが出るため再調整
            AdjustListViewColumns(targetListView);

            // 次回更新候補時刻を返す
            return minNextRefreshTime;
        }

        /// <summary>
        /// 新番組一覧の生成
        /// 新番組一覧（ListView）の生成および描画を行い、次回画面更新が必要となる最速の時刻を取得します。
        /// </summary>
        /// <param name="targetListView">描画対象のListView</param>
        /// <returns>次回更新が必要になる時刻</returns>
        public DateTime BuildNewProgramList(ListView targetListView)
        {
            DateTime now = DateTime.Now;
            DateTime minNextRefreshTime = DateTime.MaxValue;

            // チラつきを抑えるため描画を停止
            targetListView.BeginUpdate();

            try
            {
                // 一覧を全件クリア
                targetListView.Items.Clear();

                if (_epgDataManager.ServiceEvents == null) return DateTime.MinValue;

                // --- 全サービスの番組リストから「[新]」が含まれる番組を抽出し、開始時間順に平坦化（Flatten）してソート ---
                var newPrograms = _epgDataManager.ServiceEvents
                    .Where(se => se != null && se.eventList != null)
                    .SelectMany(se => se.eventList, (se, ev) => new { ServiceEvent = se, Event = ev })
                    .Where(x => x.Event.ShortInfo != null &&
                                !string.IsNullOrEmpty(x.Event.ShortInfo.event_name) &&
                                (x.Event.ShortInfo.event_name.Contains("[新]") || x.Event.ShortInfo.event_name.Contains("［新］")))
                    .Where(x => x.Event.start_time.AddSeconds(x.Event.durationSec) > now) // 終了していない番組
                    .OrderBy(x => x.Event.start_time)
                    .ToList();

                // --- 抽出結果を ListViewItem として登録 ---
                foreach (var itemData in newPrograms)
                {
                    var matchedService = itemData.ServiceEvent;
                    var ev = itemData.Event;

                    // サービス（チャンネル）の決定
                    string key = RockbarUtility.GetKey(ev.transport_stream_id, ev.service_id);
                    Service service = _configManager.SelectedServiceList?.FirstOrDefault(x => RockbarUtility.GetKey(x.Tsid, x.Sid) == key);
                    if (service == null)
                    {
                        continue; // 登録チャンネル一覧にない場合は処理を飛ばす
                    }

                    // 設定ファイルのチャンネル名を最優先で使用し、設定がなければEDCBのStationNameを使用
                    string serviceName = GetServiceName(ev.transport_stream_id, ev.service_id);

                    // 番組タイトルの取得
                    string eventTitle = ev.ShortInfo?.event_name ?? "";

                    // フィルタ条件チェック（既存の検索テキストボックス用）
                    if (!IsMatchFilter(new[] { serviceName, eventTitle }))
                    {
                        continue;
                    }

                    // 時刻テキストの設定（未来の番組が多いため日付曜日付き）
                    string timeText = $"{ev.start_time:MM/dd(ddd) HH:mm}-{ev.start_time.AddSeconds(ev.durationSec):HH:mm}";

                    // 予約状態の設定
                    string reserveString = "";
                    ReserveStatus reserveStatus = ReserveStatus.NONE;
                    string eventKey = RockbarUtility.GetKey(ev.transport_stream_id, ev.service_id, ev.event_id);

                    if (_epgDataManager.ReserveMap.TryGetValue(eventKey, out ReserveData reserveData))
                    {
                        reserveStatus = GetReserveStatus(reserveData);
                    }
                    reserveString = RockbarUtility.GetReserveStatusString(reserveStatus);

                    // UI要素（ListViewItem）の生成
                    ListViewItem item = new ListViewItem(new[] { reserveString, timeText, serviceName, eventTitle })
                    {
                        Name = key,
                        Tag = ev,
                        ToolTipText = ev.ShortInfo?.text_char ?? eventTitle,
                        ForeColor = _foreColor,
                        BackColor = GetReserveBackColor(reserveStatus, ev.start_time, ev.start_time.AddSeconds(ev.durationSec))
                    };

                    targetListView.Items.Add(item);
                }

                // --- リストアップされた番組のうち、終了時間が直近のものを取得して次回更新時刻に設定 ---
                minNextRefreshTime = newPrograms
                    .Select(x => x.Event.start_time.AddSeconds(x.Event.durationSec))
                    .Where(endTime => endTime > now)
                    .DefaultIfEmpty(DateTime.MaxValue)
                    .Min();

                // --- 列幅の設定 ---
                // 列0: 予約状態
                int col0Width = TextRenderer.MeasureText("◎", targetListView.Font).Width + _columnPadding;

                // 列1: 時間表記の固定幅
                int col1Width = TextRenderer.MeasureText("MM/dd(ddd) 00:00-00:00", targetListView.Font).Width + _columnPadding;

                // 列2: チャンネル名の最長幅
                int col2Width = _maxServiceNameWidth + _columnPadding;

                targetListView.Columns[0].Width = col0Width;
                targetListView.Columns[1].Width = col1Width;
                targetListView.Columns[2].Width = col2Width;

                // 列3: 番組名（残りの幅をすべて充当）
                int remainingWidth = targetListView.ClientSize.Width - (col0Width + col1Width + col2Width);
                targetListView.Columns[3].Width = Math.Max(100, remainingWidth - 2);
            }
            finally
            {
                // 描画再開
                targetListView.EndUpdate();
            }
            // 前回垂直スクロールバーがない状態からある状態になると水平スクロールバーが出るため再調整
            AdjustListViewColumns(targetListView);

            // 次回更新候補時刻を返す
            return minNextRefreshTime;
        }

        /// <summary>
        /// 予約一覧（ListView）の生成および描画を行い、次回画面更新が必要となる最速の時刻を取得します。
        /// </summary>
        /// <param name="targetListView">描画対象のListView</param>
        /// <returns>次回更新が必要になる時刻</returns>
        public DateTime BuildReserveList(ListView targetListView)
        {
            DateTime now = DateTime.Now;
            DateTime minNextRefreshTime = DateTime.MaxValue;

            // チラつきを抑えるため描画を停止
            targetListView.BeginUpdate();

            try
            {
                // 一覧を全件クリア
                targetListView.Items.Clear();

                foreach (var reserveData in _epgDataManager.ReserveDatas.OrderBy(r => r.StartTime))
                {
                    DateTime startTime = reserveData.StartTime;
                    DateTime endTime = startTime.AddSeconds(reserveData.DurationSecond);

                    // 過去の予約（すでに終了しているもの）は表示しない
                    if (endTime <= DateTime.Now)
                    {
                        continue;
                    }

                    // --- 次回画面更新時間の判定 ---
                    // 開始時刻が現在以降かつ、これまでの最小値より近ければ更新
                    if (startTime > now && startTime < minNextRefreshTime)
                    {
                        minNextRefreshTime = startTime;
                    }

                    // 終了時刻が現在以降かつ、これまでの最小値より近ければ更新
                    if (endTime > now && endTime < minNextRefreshTime)
                    {
                        minNextRefreshTime = endTime;
                    }

                    // --- チャンネル名の取得 ---
                    // 設定ファイルのチャンネル名を最優先で使用し、設定がなければEDCBのStationNameを使用
                    string serviceName = GetServiceName(reserveData.TransportStreamID, reserveData.ServiceID);

                    // フィルタ条件チェック
                    if (!IsMatchFilter(new[] { serviceName, reserveData.Title }))
                    {
                        continue;
                    }

                    // --- 表示文字列の作成 ---
                    ReserveStatus reserveStatus = GetReserveStatus(reserveData);
                    string statusText = RockbarUtility.GetReserveStatusString(reserveStatus);

                    // ◎（正常予約）の場合はチューナー名を表示する
                    if (reserveStatus == ReserveStatus.OK)
                    {
                        var matchedTuner = _epgDataManager.TunerReserveInfos.FirstOrDefault(t => t.reserveList.Contains(reserveData.ReserveID));
                        if (matchedTuner != null)
                        {
                            statusText = GetTunerName(matchedTuner);
                        }
                    }

                    string dateTimeText = $"{startTime:MM/dd(ddd) HH:mm}-{endTime:HH:mm}";
                    string title = reserveData.Title;

                    // --- UI要素（ListViewItem）の生成 ---
                    string[] rowData = { statusText, dateTimeText, serviceName, title };
                    ListViewItem item = new ListViewItem(rowData)
                    {
                        Name = RockbarUtility.GetKey(reserveData.TransportStreamID, reserveData.ServiceID, reserveData.EventID),
                        ToolTipText = $"{startTime:yyyy/MM/dd(ddd) HH:mm}～{endTime:HH:mm} {title}",
                        ForeColor = _foreColor,
                        BackColor = GetReserveBackColor(reserveStatus, startTime, endTime)
                    };

                    targetListView.Items.Add(item);
                }

                // --- 列幅の設定（設定ファイルの最長文字数に基づく計算） ---
                // 列0: チューナー名の最長幅
                int col0Width = _maxTunerNameWidth + _columnPadding;

                // 列1: 日時表記の固定幅
                int col1Width = TextRenderer.MeasureText("00/00(日) 00:00-00:00", targetListView.Font).Width + _columnPadding;

                // 列2: チャンネル名の最長幅
                int col2Width = _maxServiceNameWidth + _columnPadding;

                targetListView.Columns[0].Width = col0Width;
                targetListView.Columns[1].Width = col1Width;
                targetListView.Columns[2].Width = col2Width;

                // 列3: 番組名（残りの幅をすべて充当）
                int remainingWidth = targetListView.ClientSize.Width - (col0Width + col1Width + col2Width);
                targetListView.Columns[3].Width = Math.Max(100, remainingWidth - 2);
            }
            finally
            {
                // 描画を再開
                targetListView.EndUpdate();
            }
            // 前回垂直スクロールバーがない状態からある状態になると水平スクロールバーが出るため再調整
            AdjustListViewColumns(targetListView);

            // 次回更新候補時刻を返す
            return minNextRefreshTime;
        }

        /// <summary>
        /// 録画済み一覧の生成
        /// </summary>
        /// <param name="targetListView">描画対象のListView</param>
        public void BuildRecList(ListView targetListView)
        {
            // チラつきを抑えるため描画を停止
            targetListView.BeginUpdate();

            try
            {
                // 一覧を全件クリア
                targetListView.Items.Clear();

                foreach (var recFile in _epgDataManager.RecFileInfos)
                {
                    DateTime startTime = recFile.StartTime;
                    DateTime endTime = startTime.AddSeconds(recFile.DurationSecond);

                    // --- チャンネル名の取得 ---
                    // 設定ファイルのチャンネル名を最優先で使用し、設定がなければEDCBのServiceNameを使用
                    string serviceName = GetServiceName(recFile.TransportStreamID, recFile.ServiceID);

                    // フィルタ条件チェック
                    if (!IsMatchFilter(new[] { serviceName, recFile.Title }))
                    {
                        continue;
                    }

                    // --- 表示文字列の作成 ---
                    RecEndStatus recEndStatus = (RecEndStatus)recFile.RecStatus;
                    string statusText = RockbarUtility.GetRecEndStatusString(recEndStatus);
                    string dateTimeText = $"{startTime:yy/MM/dd(ddd) HH:mm}-{endTime:HH:mm}";
                    string title = recFile.Title;

                    // --- UI要素（ListViewItem）の生成 ---
                    string[] rowData = { statusText, dateTimeText, serviceName, title };
                    ListViewItem item = new ListViewItem(rowData)
                    {
                        Name = recFile.ID.ToString(),
                        ToolTipText = CreateRecInfoTooltipTexts(recFile),
                        ForeColor = _foreColor
                    };

                    // --- 背景色の設定 ---
                    // 録画結果が正常の場合
                    if (recEndStatus == RecEndStatus.NORMAL || recEndStatus == RecEndStatus.CHG_TIME || recEndStatus == RecEndStatus.NEXT_START_END)
                    {
                        if (recFile.Scrambles > 0)
                        {
                            // スクランブル解除漏れありの場合、黃背景色で警告
                            item.BackColor = _partialReserveListBackColor;
                        }
                        else if (recFile.Drops > 0)
                        {
                            // ドロップありの場合、赤背景色で警告
                            item.BackColor = _ngReserveListBackColor;
                        }
                        else
                        {
                            item.BackColor = _listBackColor;
                        }
                    }
                    // 録画結果が正常ではない場合
                    else if (recEndStatus == RecEndStatus.END_SUBREC)
                    {
                        // サブフォルダへの録画の場合、ダークスレートグレー背景色で通知
                        item.BackColor = _okReserveListBackColor;
                    }
                    else if (recEndStatus == RecEndStatus.ERR_END || recEndStatus == RecEndStatus.NOT_START_HEAD)
                    {
                        // 録画中のエラー、一部のみ録画の場合、黃背景色で警告
                        item.BackColor = _partialReserveListBackColor;
                    }
                    else if (recEndStatus == RecEndStatus.NO_RECMODE)
                    {
                        // 無効扱いの場合、グレー背景色で通知
                        item.BackColor = _disabledReserveListBackColor;
                    }
                    else
                    {
                        // それ以外のエラーの場合、赤背景色で警告
                        item.BackColor = _ngReserveListBackColor;
                    }

                    targetListView.Items.Add(item);
                }

                // --- 列幅の設定 ---
                // 列0: 録画状態
                int col0Width = TextRenderer.MeasureText("◎", targetListView.Font).Width + _columnPadding;

                // 列1: 日時表記の固定幅
                int col1Width = TextRenderer.MeasureText("00/00/00(日) 00:00-00:00", targetListView.Font).Width + _columnPadding;

                // 列2: チャンネル名の最長幅
                int col2Width = _maxServiceNameWidth + _columnPadding;

                targetListView.Columns[0].Width = col0Width;
                targetListView.Columns[1].Width = col1Width;
                targetListView.Columns[2].Width = col2Width;

                // 列3: 番組名（残りの幅をすべて充当）
                int remainingWidth = targetListView.ClientSize.Width - (col0Width + col1Width + col2Width);
                targetListView.Columns[3].Width = Math.Max(100, remainingWidth - 2);
            }
            finally
            {
                // 描画を再開
                targetListView.EndUpdate();
            }
            // 前回垂直スクロールバーがない状態からある状態になると水平スクロールバーが出るため再調整
            AdjustListViewColumns(targetListView);
        }

        /// <summary>
        /// チューナー一覧の生成
        /// </summary>
        /// <param name="targetListView">描画対象のListView</param>
        /// <returns>次回更新が必要になる時刻</returns>
        public DateTime BuildTunerList(ListView targetListView)
        {
            // チラつきを抑えるため描画を停止
            targetListView.BeginUpdate();

            // 次回更新時間判定用：現在時刻以降で最も近いイベント時刻（開始 or 終了）を保持
            DateTime now = DateTime.Now;
            DateTime minNextRefreshTime = DateTime.MaxValue;

            try
            {
                // 一覧を全件クリア
                targetListView.Items.Clear();

                // チューナー表示
                foreach (var tuner in _epgDataManager.TunerReserveInfos)
                {
                    // 設定にチューナー名があれば取得し、なければデフォルト名で表示
                    string tunerName = GetTunerName(tuner);

                    // 直近の予約タイトルとツールチップをを抽出する
                    HashSet<uint> reserveIds = tuner.reserveList.ToHashSet();
                    List<ReserveData> reserves = _epgDataManager.ReserveDatas.FindAll(x => reserveIds.Contains(x.ReserveID));

                    var nearestReserve = reserves
                        .Where(x => x.StartTime.AddSeconds(x.DurationSecond) > now)
                        .OrderBy(x => x.StartTime.AddSeconds(x.DurationSecond))
                        .FirstOrDefault();

                    // --- 表示文字列の作成 ---
                    string dateTimeText = "";
                    string toolTipDateTimeText = "";
                    string serviceName = "";
                    string title = "";
                    bool isRecording = false;

                    if (nearestReserve != null)
                    {
                        DateTime startTime = nearestReserve.StartTime;
                        DateTime endTime = startTime.AddSeconds(nearestReserve.DurationSecond);

                        // 録画中判定
                        isRecording = startTime <= DateTime.Now && endTime >= DateTime.Now;

                        // --- チャンネル名の取得 ---
                        // 設定ファイルのチャンネル名を最優先で使用し、設定がなければEDCBのStationNameを使用
                        serviceName = GetServiceName(nearestReserve.TransportStreamID, nearestReserve.ServiceID);

                        dateTimeText = $"{startTime:MM/dd(ddd) HH:mm}-{endTime:HH:mm}";
                        toolTipDateTimeText = $"{startTime:MM/dd(ddd) HH:mm}～{endTime:HH:mm}";
                        title = nearestReserve.Title;

                        // --- 次回更新時刻の候補判定 ---
                        // 開始時刻が現在以降かつ、これまでの最小値より近ければ更新
                        if (startTime >= now && startTime < minNextRefreshTime)
                        {
                            minNextRefreshTime = startTime;
                        }
                        // 終了時刻が現在以降かつ、これまでの最小値より近ければ更新
                        if (endTime >= now && endTime < minNextRefreshTime)
                        {
                            minNextRefreshTime = endTime;
                        }
                    }

                    // --- ListViewItem の生成 ---
                    string[] rowData = { tunerName, dateTimeText, serviceName, title };
                    ListViewItem item = new ListViewItem(rowData)
                    {
                        Name = tuner.tunerID.ToString(),
                        ToolTipText = $"{toolTipDateTimeText} {serviceName} {title}",
                        ForeColor = _foreColor
                    };

                    // 背景色判定
                    // 直近30件に限らず、将来変な予約がある場合警告として色を変える
                    if (reserves.Any(x => x.OverlapMode == 1))
                    {
                        // 一部予約に1件でも予約が入っている場合、黃背景色で警告
                        item.BackColor = _partialReserveListBackColor;
                    }
                    else if (reserves.Any(x => x.OverlapMode == 2))
                    {
                        // TU不足に1件でも予約が入っている場合、赤背景色で警告
                        item.BackColor = _ngReserveListBackColor;
                    }
                    else if (isRecording)
                    {
                        // 録画中の場合、正常予約背景
                        item.BackColor = _okReserveListBackColor;
                    }
                    else
                    {
                        item.BackColor = _listBackColor;
                    }

                    targetListView.Items.Add(item);
                }

                // --- 列幅の設定（設定ファイルの最長文字数に基づく計算） ---
                // 列0: チューナー名の最長幅
                int col0Width = _maxTunerNameWidth + _columnPadding;

                // 列1: 日時表記の固定幅
                int col1Width = TextRenderer.MeasureText("00/00(水) 00:00-00:00", targetListView.Font).Width + _columnPadding;

                // 列2: チャンネル名の最長幅
                int col2Width = _maxServiceNameWidth + _columnPadding;

                targetListView.Columns[0].Width = col0Width;
                targetListView.Columns[1].Width = col1Width;
                targetListView.Columns[2].Width = col2Width;

                // 列3: 番組名（残りの幅をすべて充当）
                int remainingWidth = targetListView.ClientSize.Width - (col0Width + col1Width + col2Width);
                targetListView.Columns[3].Width = Math.Max(100, remainingWidth - 2);
            }
            finally
            {
                // 描画を再開
                targetListView.EndUpdate();
            }
            // 前回垂直スクロールバーがない状態からある状態になると水平スクロールバーが出るため再調整
            AdjustListViewColumns(targetListView);

            // 次回更新候補時刻を返す
            return minNextRefreshTime;
        }

        /// <summary>
        /// リストビューカラム幅調整処理
        /// </summary>
        /// <param name="listView">対象のListView</param>
        public void AdjustListViewColumns(ListView listView)
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
        /// TSIDとSIDからサービス名を取得します。
        /// </summary>
        public string GetServiceName(ushort transportStreamId, ushort serviceId)
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

        /// <summary>
        /// チューナー名を取得します。
        /// </summary>
        public string GetTunerName(TunerReserveInfo tuner)
        {
            string tunerName;

            // 設定ファイルのチューナー名を最優先
            if (_configManager.RockbarSetting.BonDriverNameToTunerName.ContainsKey(tuner.tunerName))
            {
                tunerName = _configManager.RockbarSetting.BonDriverNameToTunerName[tuner.tunerName];
            }
            else
            {
                tunerName = RockbarUtility.GetDefaultTunerName(tuner.tunerName);
            }

            if (tuner.tunerID != 0xffffffff)
            {
                tunerName += (tuner.tunerID & 0xffff).ToString();
            }

            return tunerName;
        }

        /// <summary>
        /// 予約データから予約ステータスを取得します。
        /// </summary>
        public ReserveStatus GetReserveStatus(ReserveData reserveData)
        {
            if (reserveData.RecSetting.IsNoRec())
            {
                return ReserveStatus.DISABLED;
            }

            if (reserveData.OverlapMode == 1)
            {
                return ReserveStatus.PARTIAL;
            }

            if (reserveData.OverlapMode == 2)
            {
                return ReserveStatus.NG;
            }

            return ReserveStatus.OK;
        }

        /// <summary>
        /// 予約ステータスと時刻から適切な背景色を取得します。
        /// </summary>
        private Color GetReserveBackColor(ReserveStatus reserveStatus, DateTime startTime, DateTime endTime)
        {
            // 予約情報がない場合
            if (reserveStatus == ReserveStatus.NONE)
            {
                return _listBackColor;
            }
            // 無効予約の場合
            else if (reserveStatus == ReserveStatus.DISABLED)
            {
                return _disabledReserveListBackColor;
            }
            // 一部予約の場合、黃背景色で警告
            else if (reserveStatus == ReserveStatus.PARTIAL)
            {
                return _partialReserveListBackColor;
            }
            // TU不足の場合、赤背景色で警告
            else if (reserveStatus == ReserveStatus.NG)
            {
                return _ngReserveListBackColor;
            }
            // 現在録画中の場合、正常予約背景色で表示
            else if (startTime <= DateTime.Now && endTime >= DateTime.Now)
            {
                return _okReserveListBackColor;
            }
            else
            {
                return _listBackColor;
            }
        }

        /// <summary>
        /// 録画済み情報のテキスト生成処理
        /// </summary>
        /// <param name="recFile">録画済み情報</param>
        public List<List<RecInfoDetailText>> CreateRecInfoDetailTexts(RecFileInfo recFile)
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
        public string CreateRecInfoTooltipTexts(RecFileInfo recFile)
        {
            var detailTexts = CreateRecInfoDetailTexts(recFile);
            return string.Join("\n\n", detailTexts.Select(d => string.Join("\n", d.Select(t => t.Value))));
        }

        /// <summary>
        /// フィルタ文字列が検索対象の文字列群に含まれるか判定する
        /// </summary>
        private bool IsMatchFilter(params string[] targets)
        {
            if (string.IsNullOrWhiteSpace(FilterText))
            {
                return true;
            }

            var separators = new char[] { ' ', ' ' };
            var filterWords = FilterText.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            var normalizedTargets = targets
                .Where(t => !string.IsNullOrEmpty(t))
                .Select(t => ToHankaku(t))
                .ToList();

            foreach (var word in filterWords)
            {
                string normalizedWord = ToHankaku(word);

                bool wordMatched = false;
                foreach (var target in normalizedTargets)
                {
                    if (target.IndexOf(normalizedWord, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        wordMatched = true;
                        break;
                    }
                }

                if (!wordMatched)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 全角英数字および全角記号を半角に変換するヘルパーメソッド
        /// </summary>
        private static string ToHankaku(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            char[] chars = input.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] >= '！' && chars[i] <= '～')
                {
                    chars[i] = (char)(chars[i] - 0xfee0);
                }
                else if (chars[i] == ' ')
                {
                    chars[i] = ' ';
                }
            }
            return new string(chars);
        }

        /// <summary>
        /// フォントに応じた適切な余白（パディング）幅を取得する
        /// </summary>
        private int GetColumnPadding(Font font)
        {
            // "M"（比較的大幅な文字）の幅を基準に余白を計算
            // フォントサイズやDPIが大きくなれば、この幅も自動的に大きくなる
            int charWidth = TextRenderer.MeasureText("M", font).Width;
            return charWidth * 8 / 14;
        }

        /// <summary>
        /// 選択サービスリストから最長チャンネル名のピクセル幅を取得する
        /// </summary>
        private int GetMaxServiceNameWidth(Font font)
        {
            int maxWidth = 0;

            if (_configManager.SelectedServiceList != null)
            {
                foreach (var service in _configManager.SelectedServiceList)
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

            _maxServiceNameWidth = maxWidth;
            return _maxServiceNameWidth;
        }

        /// <summary>
        /// 設定ファイル内のチューナー名定義から最長ピクセル幅を取得する
        /// </summary>
        private int GetMaxTunerNameWidth(Font font)
        {
            int maxWidth = 0;

            if (_configManager.RockbarSetting?.BonDriverNameToTunerName != null)
            {
                foreach (var name in _configManager.RockbarSetting.BonDriverNameToTunerName.Values)
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

            _maxTunerNameWidth = maxWidth;
            return _maxTunerNameWidth;
        }
    }
}