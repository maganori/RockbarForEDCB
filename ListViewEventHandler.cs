using EpgTimer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RockbarForEDCB
{
    /// <summary>
    /// ListViewの各種イベント処理（クリック、ダブルクリック、コンテキストメニュー構築等）を担当するクラス
    /// </summary>
    public class ListViewEventHandler
    {
        private readonly ConfigManager _configManager;
        private readonly EpgDataManager _epgDataManager;
        private readonly ListViewBuilder _listViewBuilder;
        private readonly TVTestManager _tvtestManager;
        private readonly CtrlCmdUtil _ctrlCmdUtil;
        private readonly ContextMenuStrip _contextMenu;
        private readonly Action<bool, bool, bool> _refreshList;

        public ListViewEventHandler(
            ConfigManager configManager,
            EpgDataManager epgDataManager,
            ListViewBuilder listViewBuilder,
            TVTestManager tvtestManager,
            CtrlCmdUtil ctrlCmdUtil,
            ContextMenuStrip contextMenu,
            Action<bool, bool, bool> refreshList)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _epgDataManager = epgDataManager ?? throw new ArgumentNullException(nameof(epgDataManager));
            _listViewBuilder = listViewBuilder ?? throw new ArgumentNullException(nameof(listViewBuilder));
            _tvtestManager = tvtestManager ?? throw new ArgumentNullException(nameof(tvtestManager));
            _ctrlCmdUtil = ctrlCmdUtil ?? throw new ArgumentNullException(nameof(ctrlCmdUtil));
            _contextMenu = contextMenu ?? throw new ArgumentNullException(nameof(contextMenu));
            _refreshList = refreshList ?? throw new ArgumentNullException(nameof(refreshList));
        }

        /// <summary>
        /// チャンネル一覧マウスクリック処理
        /// 右クリック時、直近30件の番組情報をコンテキストメニューに表示。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        public void HandleServiceClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            if (targetListView.SelectedItems.Count == 0)
            {
                return;
            }

            // 右クリック
            // 現在の番組を含め、今後の番組を30件までコンテキストメニューで表示(TVRockの仕様踏襲)
            if (e.Button == MouseButtons.Right)
            {
                // クリックした箇所が自動選択されるので拾う
                var selected = targetListView.SelectedItems[0];

                ClearContextMenu();

                _epgDataManager.ServiceMap.TryGetValue(selected.Name, out EpgServiceEventInfo sv);

                // EPG未取得チャンネルは処理を抜ける
                if (sv == null)
                {
                    return;
                }

                DateTime now = DateTime.Now;

                // 終了していないイベントのみ抽出して開始日時順にソート
                var afterEventList = sv.eventList
                    .Where(x => x.start_time.AddSeconds(x.durationSec) >= now)
                    .OrderBy(a => a.start_time)
                    .Take(30);// 最大30件まで表示

                foreach (var ev in afterEventList)
                {
                    // 映像情報が無いものは含めない (他候補メモ：ShortInfo, AudioInfo)
                    if (ev.ComponentInfo == null) continue;

                    // 該当イベントが予約済みであれば予約情報を取得
                    string eventKey = RockbarUtility.GetKey(ev.transport_stream_id, ev.service_id, ev.event_id);
                    _epgDataManager.ReserveMap.TryGetValue(eventKey, out ReserveData reserveData);

                    // コンテキストメニューに番組項目を追加
                    _contextMenu.Items.Add(CreateProgramSummaryMenuItem(ev, reserveData, false));
                }

                // 追加された番組情報が0件だった場合の処理
                if (_contextMenu.Items.Count == 0)
                {
                    _contextMenu.Items.Add(new ToolStripMenuItem("直近の30件で表示する番組情報はありません") { Enabled = false });
                }

                _contextMenu.Show((Control)sender, new Point(0, e.Y));
            }
        }

        /// <summary>
        /// チャンネル一覧のマウス中央クリック処理
        /// 中央クリック時、対象の番組情報を予約追加か、予約有効・無効を切り替える。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        public void HandleServiceMouseUp(object sender, MouseEventArgs e, ListView targetListView)
        {
            //中央クリック
            if (e.Button == MouseButtons.Middle)
            {
                // クリックした箇所が自動選択されるので拾う
                var selected = targetListView.GetItemAt(e.Location.X, e.Location.Y); //フォーカスが当たっていなくても取得

                if (selected == null)
                {
                    return;
                }

                _epgDataManager.ServiceMap.TryGetValue(selected.Name, out EpgServiceEventInfo sv);

                // EPG未取得チャンネルは処理を抜ける
                if (sv == null)
                {
                    return;
                }

                DateTime now = DateTime.Now;

                // 終了していないイベントのみ抽出して開始日時順にソート
                var ev = sv.eventList
                    .Where(x => x.start_time.AddSeconds(x.durationSec) >= now)
                    .OrderBy(a => a.start_time)
                    .FirstOrDefault();

                // 映像情報が無い場合は処理を抜ける (他候補メモ：ShortInfo, AudioInfo)
                if (ev == null || ev.ComponentInfo == null)
                {
                    return;
                }

                string eventKey = RockbarUtility.GetKey(ev.transport_stream_id, ev.service_id, ev.event_id);
                bool isReserved = _epgDataManager.ReserveMap.TryGetValue(eventKey, out var reserveData);

                // 既に予約済みであれば、予約の有効・無効を切り替える
                if (isReserved && _configManager.RockbarSetting.UseRockbarReserveMod)
                {
                    ToggleRecMode(reserveData);
                }
                // 予約がなければ予約を追加する
                else if (!isReserved && _configManager.RockbarSetting.UseRockbarReserveAdd)
                {
                    AddProgramReserve(ev);
                }
                // Rockbarで予約の追加や変更を行う設定ではないがWebLinkは使用するになっている場合
                else if (_configManager.RockbarSetting.UseWebLink)
                {
                    OpenWebEpgInfo(ev);
                }
            }
        }

        /// <summary>
        /// チャンネル一覧マウスダブルクリック処理
        /// TVTest使用オプションがONの場合、カーソル箇所の番組を対象にTVTestを起動する。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        public void HandleServiceDoubleClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            if (targetListView.SelectedItems.Count == 0)
            {
                return;
            }

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

                // ネットワークタイプの取得
                NetworkType networkType = sv != null
                    ? RockbarUtility.GetNetworkType(service.TypeName, sv.serviceInfo.ONID)
                    : RockbarUtility.GetNetworkType(service.TypeName, null);

                // TVTestの起動処理呼び出し
                _tvtestManager.StartTVTest(networkType, tsid, sid, service.TvtestOption);
            }
        }

        /// <summary>
        /// 新番組一覧のマウスクリック処理
        /// 右クリック時、対象の番組情報をコンテキストメニューに表示。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        public void HandleNewProgramClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            if (targetListView.SelectedItems.Count == 0)
            {
                return;
            }

            //右クリック
            if (e.Button == MouseButtons.Right)
            {
                // クリックした箇所が自動選択されるので拾う
                var selected = targetListView.SelectedItems[0];

                if (selected == null)
                {
                    return;
                }

                // Tagに格納されているものがEpgEventInfoならevへ代入
                if (selected.Tag is EpgEventInfo ev)
                {
                    ClearContextMenu();

                    string eventKey = RockbarUtility.GetKey(ev.transport_stream_id, ev.service_id, ev.event_id);
                    _epgDataManager.ReserveMap.TryGetValue(eventKey, out var reserveData);

                    CreateProgramDetailMenuItems(_contextMenu.Items, ev, reserveData);

                    _contextMenu.Show((Control)sender, e.Location);
                }
            }
        }

        /// <summary>
        /// 新番組一覧のマウス中央クリック処理
        /// 中央クリック時、対象の番組情報を予約追加か、予約有効・無効を切り替える。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        public void HandleNewProgramMouseUp(object sender, MouseEventArgs e, ListView targetListView)
        {
            //中央クリック
            if (e.Button == MouseButtons.Middle)
            {
                // クリックした箇所が自動選択されるので拾う
                var selected = targetListView.GetItemAt(e.Location.X, e.Location.Y); //フォーカスが当たっていなくても取得

                if (selected == null)
                {
                    return;
                }

                // Tagに格納されているものがEpgEventInfoならevへ代入
                if (selected.Tag is EpgEventInfo ev)
                {
                    string eventKey = RockbarUtility.GetKey(ev.transport_stream_id, ev.service_id, ev.event_id);
                    bool isReserved = _epgDataManager.ReserveMap.TryGetValue(eventKey, out var reserveData);

                    // 既に予約済みであれば、予約の有効・無効を切り替える
                    if (isReserved && _configManager.RockbarSetting.UseRockbarReserveMod)
                    {
                        ToggleRecMode(reserveData);
                    }
                    // 予約がなければ予約を追加する
                    else if (!isReserved && _configManager.RockbarSetting.UseRockbarReserveAdd)
                    {
                        AddProgramReserve(ev);
                    }
                    // Rockbarで予約の追加や変更を行う設定ではないがWebLinkは使用するになっている場合
                    else if (_configManager.RockbarSetting.UseWebLink)
                    {
                        OpenWebEpgInfo(ev);
                    }
                }
            }
        }

        /// <summary>
        /// 新番組一覧マウスダブルクリック処理
        /// Web LinkオプションがONの場合、カーソル箇所の番組のWeb番組情報にアクセスする。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        public void HandleNewProgramDoubleClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            // Web番組詳細使用時のみ
            if (!_configManager.RockbarSetting.UseWebLink)
            {
                return;
            }

            if (targetListView.SelectedItems.Count == 0)
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
                        OpenWebEpgInfo(ev);
                    }
                    else
                    {
                        MessageBox.Show("番組情報を取得できませんでした。", "ブラウザ起動エラー");
                    }
                }
                catch
                {
                    MessageBox.Show("番組情報を取得できませんでした。", "ブラウザ起動エラー");
                }
            }
        }

        /// <summary>
        /// 予約一覧マウスクリック処理
        /// 右クリック時、対象の番組情報をコンテキストメニューに表示。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        public void HandleReserveClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            if (targetListView.SelectedItems.Count == 0)
            {
                return;
            }

            // 右クリック
            // 対象の番組情報をコンテキストメニューで表示
            if (e.Button == MouseButtons.Right)
            {
                ClearContextMenu();

                try
                {
                    // クリックした箇所が自動選択されるので拾う
                    var selected = targetListView.SelectedItems[0];

                    _epgDataManager.AllEventMap.TryGetValue(selected.Name, out var ev);
                    _epgDataManager.ReserveMap.TryGetValue(selected.Name, out var reserveData);

                    CreateProgramDetailMenuItems(_contextMenu.Items, ev, reserveData);
                }
                catch
                {
                    _contextMenu.Items.Add("番組情報を取得できませんでした");
                }

                _contextMenu.Show((Control)sender, new Point(e.X, e.Y));
            }
        }

        /// <summary>
        /// 予約一覧マウスアップ処理
        /// 中央クリック時、対象の予約情報の予約有効・無効を切り替える。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        public void HandleReserveMouseUp(object sender, MouseEventArgs e, ListView targetListView)
        {
            // 中央クリック
            // 予約の有効・無効を切り替える
            if (e.Button == MouseButtons.Middle)
            {
                // クリックした箇所にある項目を取得する
                var selected = targetListView.GetItemAt(e.Location.X, e.Location.Y);

                if (selected != null)
                {
                    // 予約の有効・無効を切り替える
                    if (_configManager.RockbarSetting.UseRockbarReserveMod && _epgDataManager.ReserveMap.TryGetValue(selected.Name, out var reserveData))
                    {
                        ToggleRecMode(reserveData);
                    }
                    // Rockbarで予約の追加や変更を行う設定ではないがWebLinkは使用するになっている場合
                    else if (_configManager.RockbarSetting.UseWebLink && _epgDataManager.AllEventMap.TryGetValue(selected.Name, out var ev))
                    {
                        OpenWebEpgInfo(ev);
                    }
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
        public void HandleReserveDoubleClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            // Web番組詳細使用時のみ
            if (!_configManager.RockbarSetting.UseWebLink)
            {
                return;
            }

            if (targetListView.SelectedItems.Count == 0)
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

                    // 番組情報取得
                    if (!_epgDataManager.AllEventMap.TryGetValue(selected.Name, out var ev))
                    {
                        MessageBox.Show("番組情報を取得できませんでした。", "ブラウザ起動エラー");
                        return;
                    }

                    OpenWebEpgInfo(ev);
                }
                catch
                {
                    MessageBox.Show("番組情報を取得できませんでした。", "ブラウザ起動エラー");
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
        public void HandleRecClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            if (targetListView.SelectedItems.Count == 0)
            {
                return;
            }

            // 右クリック
            // 対象の番組情報をコンテキストメニューで表示
            if (e.Button == MouseButtons.Right)
            {
                ClearContextMenu();

                try
                {
                    // クリックした箇所が自動選択されるので拾う
                    var selected = targetListView.SelectedItems[0];

                    if (!uint.TryParse(selected.Name, out uint recID))
                    {
                        MessageBox.Show("録画情報IDの取得に失敗しました。", "録画情報IDエラー");
                        return;
                    }

                    // 録画情報取得
                    if (!_epgDataManager.RecMap.TryGetValue(recID, out var recFile))
                    {
                        _contextMenu.Items.Add("録画情報を取得できませんでした");
                        _contextMenu.Show((Control)sender, new Point(e.X, e.Y));
                        return;
                    }

                    // Webリンク使用時のみ1行目にWebリンク用のボタンを表示
                    if (_configManager.RockbarSetting.UseWebLink)
                    {
                        var item = _contextMenu.Items.Add(">> Web番組詳細を開く");
                        item.Click += (s2, e2) => OpenWebRecInfo(recFile);
                        _contextMenu.Items.Add(new ToolStripSeparator());
                    }

                    // 検索機能
                    if (!string.IsNullOrEmpty(recFile.Title))
                    {
                        AddWebSearchMenuItems(_contextMenu.Items, recFile.Title);
                        _contextMenu.Items.Add(new ToolStripSeparator());
                    }

                    // 録画済み情報削除機能
                    var delItem = _contextMenu.Items.Add(">> 録画済み情報を削除する");
                    delItem.Click += (s2, e2) => DelRecInfo(recFile);
                    _contextMenu.Items.Add(new ToolStripSeparator());

                    // 録画結果
                    var resultItem = _contextMenu.Items.Add($"録画結果 : {recFile.Comment}");
                    resultItem.Click += (s2, e2) => CopyText(recFile.Comment);
                    resultItem.ToolTipText = "クリックで結果をコピー";

                    var dropItem = _contextMenu.Items.Add($"  Drop : {recFile.Drops}");
                    dropItem.Click += (s2, e2) => CopyText(recFile.Drops.ToString());
                    dropItem.ToolTipText = "クリックでドロップ数をコピー";

                    var scrambleItem = _contextMenu.Items.Add($"  Scramble : {recFile.Scrambles}");
                    scrambleItem.Click += (s2, e2) => CopyText(recFile.Scrambles.ToString());
                    scrambleItem.ToolTipText = "クリックでスクランブル数をコピー";

                    // 録画ファイルパス
                    if (!string.IsNullOrEmpty(recFile.RecFilePath))
                    {
                        AddBrokenTextMenuItems(_contextMenu.Items, "録画ファイル : ", recFile.RecFilePath, "録画ファイル", "クリックで録画ファイルをコピー");
                    }

                    _contextMenu.Items.Add(new ToolStripSeparator());

                    // 放送日時 (録画日時)
                    var dateTime = $"{recFile.StartTime:yyyy/MM/dd(ddd) HH:mm}～{recFile.StartTime.AddSeconds(recFile.DurationSecond):HH:mm}";
                    var dateItem = _contextMenu.Items.Add(dateTime);
                    dateItem.Click += (s2, e2) => CopyText(dateTime);
                    dateItem.ToolTipText = "クリックで日時をコピー";

                    // チャンネル名
                    string serviceName = !string.IsNullOrEmpty(recFile.ServiceName)
                        ? recFile.ServiceName
                        : _listViewBuilder.GetServiceName(recFile.TransportStreamID, recFile.ServiceID);

                    if (!string.IsNullOrEmpty(serviceName))
                    {
                        AddBrokenTextMenuItems(_contextMenu.Items, "", serviceName, "チャンネル名", "クリックでチャンネル名をコピー");
                    }

                    // 番組名
                    AddBrokenTextMenuItems(_contextMenu.Items, "", recFile.Title, "番組名", "クリックで番組名をコピー");

                    // 録画ファイルのテキスト情報（.program.txt の内容表示）
                    // 取得実績無いが残しておく
                    //System.Diagnostics.Debug.WriteLine($"[Debug] ID: {recFile.ID}");
                    //System.Diagnostics.Debug.WriteLine($"[Debug] Title: {recFile.Title}");
                    //System.Diagnostics.Debug.WriteLine($"[Debug] _ProgramInfo Length: {recFile._ProgramInfo?.Length ?? 0}");
                    //System.Diagnostics.Debug.WriteLine($"[Debug] _ProgramInfo Content:\n{recFile._ProgramInfo}");
                    if (!string.IsNullOrEmpty(recFile._ProgramInfo))
                    {
                        _contextMenu.Items.Add(new ToolStripSeparator());
                        AddBrokenTextMenuItems(_contextMenu.Items, "", recFile._ProgramInfo, "番組詳細", "クリックで番組詳細をコピー");
                    }

                    _contextMenu.Items.Add(new ToolStripSeparator());

                    // 一括コピーメニュー
                    AddCopyAllMenuItem(
                        _contextMenu.Items,
                        new[]
                        {
                            ($"録画結果 : {recFile.Comment}"),
                            ($"  Drop : {recFile.Drops}"),
                            ($"  Scramble : {recFile.Scrambles}"),
                            ($"録画ファイル : {recFile.RecFilePath}\n"),
                            dateTime,
                            serviceName,
                            !string.IsNullOrEmpty(recFile.Title) ? $"{recFile.Title}\n" : null,
                            !string.IsNullOrEmpty(recFile._ProgramInfo) ? $"{recFile._ProgramInfo}\n" : null
                         },
                         ">> すべての録画情報をコピー",
                         "すべての録画情報をまとめてコピー"
                      );
                }
                catch
                {
                    _contextMenu.Items.Add("番組情報を取得できませんでした");
                }

                _contextMenu.Show((Control)sender, new Point(e.X, e.Y));
            }
        }

        /// <summary>
        /// 録画済み情報マウスダブルクリック処理
        /// TVTest使用オプションがONの場合、カーソル箇所の番組を対象にTVTestを起動する。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        public void HandleRecDoubleClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            // TVTest使用時のみ
            if (!_configManager.RockbarSetting.UseDoubleClickTvtest)
            {
                return;
            }

            if (targetListView.SelectedItems.Count == 0)
            {
                return;
            }

            // 左ダブルクリック
            // TVTestを起動する
            if (e.Button == MouseButtons.Left)
            {
                // クリックした箇所が自動選択されるので拾う
                var selected = targetListView.SelectedItems[0];
                if (!uint.TryParse(selected.Name, out uint recID))
                {
                    MessageBox.Show("録画情報IDの取得に失敗しました。", "録画情報IDエラー");
                    return;
                }

                // 録画情報取得
                if (!_epgDataManager.RecMap.TryGetValue(recID, out var recFile))
                {
                    MessageBox.Show("録画情報の取得に失敗しました。", "録画情報エラー");
                    return;
                }

                _tvtestManager.PlayRecFile(recFile);
            }
        }

        /// <summary>
        /// チューナー一覧マウスクリック時処理
        /// 右クリック時、直近30件の予約情報をコンテキストメニューに表示。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        public void HandleTunerClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            if (targetListView.SelectedItems.Count == 0)
            {
                return;
            }

            // 右クリック
            // 今後の予約を30件までコンテキストメニューで表示
            if (e.Button == MouseButtons.Right)
            {
                // クリックした箇所が自動選択されるので拾う
                var selected = targetListView.SelectedItems[0];

                ClearContextMenu();

                TunerReserveInfo tunerReserveInfo = _epgDataManager.TunerReserveInfos.Find(x => x.tunerID.ToString() == selected.Name);
                if (tunerReserveInfo == null)
                {
                    return;
                }

                // TunerReserveInfoには予約IDしか入っていないので、ReserveDataから予約情報を取り直す
                HashSet<uint> reserveIds = tunerReserveInfo.reserveList.ToHashSet();

                var reserves = _epgDataManager.ReserveDatas
                    .Where(x => reserveIds.Contains(x.ReserveID))
                    .OrderBy(x => x.StartTime)
                    .Take(30); // 最大30件

                foreach (var reserveData in reserves)
                {
                    string key = RockbarUtility.GetKey(reserveData.TransportStreamID, reserveData.ServiceID, reserveData.EventID);

                    // 予約情報を取得
                    _epgDataManager.AllEventMap.TryGetValue(key, out var ev);

                    // チューナー予約一覧のコンテキストメニュー項目生成
                    _contextMenu.Items.Add(CreateProgramSummaryMenuItem(ev, reserveData, true));
                }

                _contextMenu.Show((Control)sender, new Point(0, e.Y));
            }
        }

        /// <summary>
        /// チューナー一覧のマウス中央クリック処理
        /// 中央クリック時、対象の番組情報を予約追加か、予約有効・無効を切り替える。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        public void HandleTunerMouseUp(object sender, MouseEventArgs e, ListView targetListView)
        {
            ////中央クリック
            if (e.Button == MouseButtons.Middle)
            {
                // クリックした箇所が自動選択されるので拾う
                var selected = targetListView.GetItemAt(e.Location.X, e.Location.Y); //フォーカスが当たっていなくても取得

                if (selected == null)
                {
                    return;
                }

                // selected.Name (tunerID.ToString()) から TunerReserveInfo を検索
                TunerReserveInfo tunerReserveInfo = _epgDataManager.TunerReserveInfos
                            .Find(x => x.tunerID.ToString() == selected.Name);

                if (tunerReserveInfo == null || tunerReserveInfo.reserveList == null)
                {
                    return;
                }

                DateTime now = DateTime.Now;

                // 対象チューナーに割り当てられた予約ID集合を取得
                HashSet<uint> reserveIds = tunerReserveInfo.reserveList.ToHashSet();

                // 現在時刻以降に終了する予約の中で、最も終了時刻が早いものを取得
                var reserveData = _epgDataManager.ReserveDatas
                    .Where(x => reserveIds.Contains(x.ReserveID) && x.StartTime.AddSeconds(x.DurationSecond) > now)
                    .OrderBy(x => x.StartTime.AddSeconds(x.DurationSecond))
                    .FirstOrDefault();

                if (reserveData == null)
                {
                    return;
                }

                // 既に予約済みのため、予約の有効・無効を切り替える
                ToggleRecMode(reserveData);
            }
        }

        /// <summary>
        /// コンテキストメニューおよびサブメニューの既存項目を破棄してクリア
        /// </summary>
        private void ClearContextMenu()
        {
            for (int i = _contextMenu.Items.Count - 1; i >= 0; i--)
            {
                _contextMenu.Items[i].Dispose();
            }
            _contextMenu.Items.Clear();
        }

        /// <summary>
        /// 録画予約モード有効・無効の切り替え処理
        /// 予約有効時は無効化、無効時は有効化する。
        /// </summary>
        private void ToggleRecMode(ReserveData reserve)
        {
            if (reserve.RecSetting.IsNoRec())
            {
                reserve.RecSetting.RecMode = _configManager.RockbarSetting.FixNoRecToServiceOnly ? (byte)1 : reserve.RecSetting.GetRecMode();
            }
            else
            {
                // 録画予約モード情報を維持して無効化
                var recMode = reserve.RecSetting.RecMode;
                reserve.RecSetting.RecMode = (byte)(_configManager.RockbarSetting.FixNoRecToServiceOnly ? 5 : 5 + (recMode + 4) % 5);
            }

            var err = _ctrlCmdUtil.SendChgReserve(new List<ReserveData>() { reserve });
            if (err != ErrCode.CMD_SUCCESS)
            {
                MessageBox.Show("録画予約変更でエラーが発生しました。", "録画予約変更エラー");
            }
            _refreshList?.Invoke(true, false, false);
        }

        /// <summary>
        /// 録画予約の追加処理
        /// </summary>
        private void AddProgramReserve(EpgEventInfo ev, bool? overridePrioritizeView = null)
        {
            // 放送がない時間帯
            if (ev.ShortInfo == null || ev.ShortInfo.event_name == null)
            {
                Debug.WriteLine("放送がない時間帯");
                return;
            }

            // 重複予約になっていないかチェック
            string eventKey = RockbarUtility.GetKey(ev.transport_stream_id, ev.service_id, ev.event_id);
            if (_epgDataManager.ReserveMap.TryGetValue(eventKey, out var reserveData))
            {
                //System.Diagnostics.Debug.WriteLine($"[Debug] Title: {reserveData.Title}");
                return;
            }

            // EPGのサービス名を使用
            string key = RockbarUtility.GetKey(ev.transport_stream_id, ev.service_id);
            _epgDataManager.ServiceMap.TryGetValue(key, out EpgServiceEventInfo matchedService);
            string serviceName = matchedService.serviceInfo?.service_name;

            // RecSettingDataに入れるRecModeの値を生成するロジック
            byte recMode = 0;
            bool enableReserve = _configManager.RockbarSetting.EnableReserve;
            // 引数で指定された場合はその値を優先し、null の場合は設定ファイル（RockbarSetting）の値を使用
            bool prioritizeView = overridePrioritizeView ?? _configManager.RockbarSetting.PrioritizeView;
            byte settingRecMode = _configManager.RockbarSetting.RecMode;

            if (enableReserve)
            {
                if (!prioritizeView)
                {
                    // EnableReserve: True(予約有効), prioritizeView: False(録画)
                    recMode = settingRecMode;
                }
                else
                {
                    // EnableReserve: True(予約有効), prioritizeView: True(視聴)
                    recMode = 4; // 視聴
                }
            }
            else
            {
                if (!prioritizeView)
                {
                    // EnableReserve: False(予約無効), prioritizeView: False(録画)
                    switch (settingRecMode)
                    {
                        case 0: recMode = 9; break; // 全サービス
                        case 1: recMode = 5; break; // 指定サービス
                        case 2: recMode = 6; break; // 全サービス(デコード処理なし)
                        case 3: recMode = 7; break; // 指定サービス(デコード処理なし)
                        default: recMode = settingRecMode; break;
                    }
                }
                else
                {
                    // EnableReserve: False(予約無効), prioritizeView: True(視聴)
                    recMode = 8; // 視聴
                }
            }

            // EpgEventInfo から基本情報とID群をセット
            var reserve = new ReserveData
            {
                Title = ev.ShortInfo.event_name,
                StartTime = ev.start_time,
                DurationSecond = ev.durationSec,
                StationName = serviceName,

                OriginalNetworkID = ev.original_network_id,
                TransportStreamID = ev.transport_stream_id,
                ServiceID = ev.service_id,
                EventID = ev.event_id,

                Comment = _configManager.RockbarSetting.RecComment,

                ReserveID = 0, // 新規登録時は 0（自動採番）

                ////UnusedRecWaitFlag = 0,
                //OverlapMode = 0,
                ////UnusedRecFilePath = "",
                StartTimeEpg = ev.start_time, // EPG上の番組開始時刻

                RecSetting = new RecSettingData
                {
                    // ・byte RecMode
                    // 予約有効 全サービス：0
                    // 予約有効 指定サービス：1
                    // 予約有効 全サービス(デコード処理なし)：2
                    // 予約有効 指定サービス(デコード処理なし)：3
                    // 予約有効 視聴：4

                    // 予約無効 全サービス：9
                    // 予約無効 指定サービス：5
                    // 予約無効 全サービス(デコード処理なし)：6
                    // 予約無効 指定サービス(デコード処理なし)：7
                    // 予約無効 視聴：8
                    RecMode = recMode,
                    Priority = _configManager.RockbarSetting.RecPriority,
                    TuijyuuFlag = _configManager.RockbarSetting.RecTuijyuu ? (byte)1 : (byte)0,

                    // ・uint ServiceMode
                    // 8765 4321
                    // 1:デフォルトを使用(0:ON, 1:OFF)
                    // 2:？
                    // 3:？
                    // 4:？
                    // 5:字幕を含める(0:OFF, 1:ON)
                    // 6:データカルーセルを含める(0:OFF, 1:ON)
                    ServiceMode = _configManager.RockbarSetting.RecServiceMode,
                    PittariFlag = _configManager.RockbarSetting.RecPittari ? (byte)1 : (byte)0,
                    //BatFilePath = _configManager.RockbarSetting.RecBatFilePath,
                    //RecTag = _configManager.RockbarSetting.RecTag,
                    RecFolderList = _configManager.RockbarSetting.RecFolderList,
                    // ・byte SuspendMode
                    // デフォルト有効 ：0 ※RebootFlagも0(無効)にされる
                    // デフォルト無効 何もしない：4
                    // デフォルト無効 スタンバイ：1
                    // デフォルト無効 休止：2
                    // デフォルト無効 シャットダウン：3
                    SuspendMode = _configManager.RockbarSetting.SuspendModeAfterRec,
                    RebootFlag = _configManager.RockbarSetting.RebootAfterReturn ? (byte)1 : (byte)0,
                    UseMargineFlag = _configManager.RockbarSetting.UseCustomRecMargin ? (byte)1 : (byte)0,
                    StartMargine = _configManager.RockbarSetting.StartRecMargin,
                    EndMargine = _configManager.RockbarSetting.EndRecMargin,
                    ContinueRecFlag = _configManager.RockbarSetting.ContinueRecSameFile ? (byte)1 : (byte)0,
                    PartialRecFlag = _configManager.RockbarSetting.PartialRecSeparateFile ? (byte)1 : (byte)0,
                    TunerID = _configManager.RockbarSetting.RecTunerID,
                    PartialRecFolder = _configManager.RockbarSetting.PartialRecFolderList,
                },
                //ReserveStatus = 0,
                //RecFileNameList = new List<string>(), //未指定でよい
                ////UnusedParam1 = 0,
                //AutoAddInfo = new List<EpgAutoAddBasicInfo>(),

            };
            reserve.RecSetting.SetBatFilePathAndRecTag(_configManager.RockbarSetting.RecBatFilePath);

            // サーバーへ予約追加コマンド送信
            var err = _ctrlCmdUtil.SendAddReserve(new List<ReserveData> { reserve });
            if (err != ErrCode.CMD_SUCCESS)
            {
                MessageBox.Show("録画予約登録でエラーが発生しました。", "録画予約登録エラー");
            }
            _refreshList?.Invoke(true, false, false);
        }

        /// <summary>
        /// 録画予約の削除処理
        /// </summary>
        private void DelProgramReserve(ReserveData reserve)
        {
            // 削除確認ダイアログを表示
            if (_configManager.RockbarSetting.UseRockbarReserveDelConfirm)
            {
                // 視聴予約か録画予約の確認
                string viewOrRecText = (reserve.RecSetting.RecMode == 4 || reserve.RecSetting.RecMode == 8) ? "視聴" : "録画";

                var result = MessageBox.Show(
                $"{reserve.StartTime:MM/dd(ddd) HH:mm}～{reserve.StartTime.AddSeconds(reserve.DurationSecond):HH:mm}\n" +
                $"{reserve.StationName}\n{reserve.Title}\n\nの{viewOrRecText}予約を削除しますか？",
                $"{viewOrRecText}予約削除確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

                // 「はい」以外を選択した場合は処理を中断
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            var err = _ctrlCmdUtil.SendDelReserve(new List<uint>() { reserve.ReserveID });
            if (err != ErrCode.CMD_SUCCESS)
            {
                MessageBox.Show($"録画予約削除でエラーが発生しました。", "録画予約削除エラー");
            }
            _refreshList?.Invoke(true, false, false);
        }

        /// <summary>
        /// 録画済み情報の削除処理
        /// </summary>
        private void DelRecInfo(RecFileInfo recFileInfo)
        {
            // 削除確認ダイアログを表示
            if (_configManager.RockbarSetting.UseRockbarReserveDelConfirm)
            {
                // ファイルパスが存在する場合のみ補足文を作成
                string pathNotice = string.IsNullOrEmpty(recFileInfo.RecFilePath)
                    ? $"\n\n録画ファイル情報がないため録画に失敗した可能性があります。"
                    : $"\n\n{recFileInfo.RecFilePath}\nは削除されません。";

                var result = MessageBox.Show(
                    $"{recFileInfo.StartTime:MM/dd(ddd) HH:mm}～{recFileInfo.StartTime.AddSeconds(recFileInfo.DurationSecond):HH:mm}\n" +
                    $"{recFileInfo.ServiceName}\n{recFileInfo.Title}\n\nの録画済み情報を削除しますか？" +
                    pathNotice,
                    "録画済み情報削除確認",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                // 「はい」以外を選択した場合は処理を中断
                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            var err = _ctrlCmdUtil.SendDelRecInfo(new List<uint>() { recFileInfo.ID });
            if (err != ErrCode.CMD_SUCCESS)
            {
                MessageBox.Show("録画済み情報削除でエラーが発生しました。", "録画済み情報削除エラー");
            }
            _refreshList?.Invoke(true, true, false);
        }

        /// <summary>
        /// マウスクリック処理に応じてテキストのコピーをする
        /// </summary>
        /// <param name="text">対象テキスト</param>
        private void CopyText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text);
            }
        }

        /// <summary>
        /// EpgEventInfo に基づいて Web番組詳細を表示
        /// </summary>
        private void OpenWebEpgInfo(EpgEventInfo ev)
        {
            string url = _configManager.RockbarSetting.WebLinkUrl
                .Replace("{ONID}", ev.original_network_id.ToString())
                .Replace("{TSID}", ev.transport_stream_id.ToString())
                .Replace("{SID}", ev.service_id.ToString())
                .Replace("{EID}", ev.event_id.ToString());

            RockbarUtility.OpenBrowser(url);
        }

        /// <summary>
        /// RecFileInfo に基づいて Web録画詳細を表示
        /// </summary>
        private void OpenWebRecInfo(RecFileInfo recFile)
        {
            string url = _configManager.RockbarSetting.RecInfoWebLinkUrl
                .Replace("{RecID}", recFile.ID.ToString());

            RockbarUtility.OpenBrowser(url);
        }

        /// <summary>
        /// 番組の右クリックコンテキストメニューItem作成処理
        /// 番組情報・予約情報からチャンネル一覧・チューナー一覧用のコンテキストメニューitemを作成する。
        /// </summary>
        /// <param name="ev">番組情報</param>
        /// <param name="reserve">予約情報</param>
        /// <param name="isTuner">チューナー一覧用？</param>
        /// <returns></returns>
        private ToolStripMenuItem CreateProgramSummaryMenuItem(EpgEventInfo ev, ReserveData reserve, bool isTuner)
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
                    reserveStatus = (ev == null) ? ReserveStatus.DISAPPEARED : ReserveStatus.OK;
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

            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            // 予約状態に応じた背景色の設定
            // 録画ステータスに異常があれば色を変える
            switch (reserveStatus)
            {
                case ReserveStatus.OK:
                    if (!isTuner)
                    {
                        item.BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.OkReserveMenuBackColor);
                    }
                    break;
                case ReserveStatus.PARTIAL:
                    item.BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.PartialReserveMenuBackColor);
                    break;
                case ReserveStatus.NG:
                    item.BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.NgReserveMenuBackColor);
                    break;
                case ReserveStatus.DISABLED:
                    item.BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.DisabledReserveMenuBackColor);
                    break;
            }

            string reserveString = RockbarUtility.GetReserveStatusString(reserveStatus);

            if (isTuner)
            {
                // --- チャンネル名の取得 ---
                // 設定ファイルのチャンネル名を最優先で使用し、設定がなければEDCBのStationNameを使用
                string serviceName = _listViewBuilder.GetServiceName(reserve.TransportStreamID, reserve.ServiceID);

                // チューナーから開いた場合は予約情報を表示(必ず予約情報あり)
                item.Text = $"{reserve.StartTime:MM/dd(ddd) HH:mm}～{reserve.StartTime.AddSeconds(reserve.DurationSecond):HH:mm}  {reserveString}    {serviceName}    {reserve.Title}";
            }
            else
            {
                // サービスから開いた場合は番組情報を表示(必ず番組情報あり)
                item.Text = $"{ev.start_time:MM/dd(ddd) HH:mm}～{ev.start_time.AddSeconds(ev.durationSec):HH:mm}  {reserveString}  {ev.ShortInfo?.event_name}";
            }

            // 番組情報がある場合はサブメニューに番組情報を追加
            if (ev != null || reserve != null)
            {
                CreateProgramDetailMenuItems(item.DropDownItems, ev, reserve);
            }

            return item;
        }

        /// <summary>
        /// 番組の右クリックコンテキストメニューItem作成処理
        /// 番組情報・予約情報から詳細情報を追加する共通処理
        /// </summary>
        /// <param name="menuItems">メニュー項目を追加する対象コレクション</param>
        /// <param name="ev">番組情報</param>
        /// <param name="reserveData">予約情報</param>
        /// <returns></returns>
        private void CreateProgramDetailMenuItems(
            ToolStripItemCollection menuItems,
            EpgEventInfo ev,
            ReserveData reserveData)
        {
            // Web番組詳細リンク (オプション有効時)
            if (_configManager.RockbarSetting.UseWebLink && ev != null)
            {
                var item = menuItems.Add(">> Web番組詳細を開く");
                item.Click += (s2, e2) => OpenWebEpgInfo(ev);
                menuItems.Add(new ToolStripSeparator());
            }

            // 検索機能 (番組名が存在する場合)
            string title = ev?.ShortInfo?.event_name ?? reserveData?.Title;
            if (!string.IsNullOrEmpty(title))
            {
                AddWebSearchMenuItems(menuItems, title);
                menuItems.Add(new ToolStripSeparator());
            }

            // 予約データが存在しない場合
            if (reserveData == null && ev != null)
            {
                // Rockbarで予約追加を行う機能が有効の場合、予約メニューを追加
                if (_configManager.RockbarSetting.UseRockbarReserveAdd)
                {
                    var item = menuItems.Add(">> 録画予約をする");
                    item.Click += (s2, e2) => AddProgramReserve(ev, false);
                    item = menuItems.Add(">> 視聴予約をする");
                    item.Click += (s2, e2) => AddProgramReserve(ev, true);
                    menuItems.Add(new ToolStripSeparator());
                }
            }
            // 予約データが存在する場合
            else if (reserveData != null)
            {
                // 視聴予約か録画予約の確認
                string viewOrRecText = (reserveData.RecSetting.RecMode == 4 || reserveData.RecSetting.RecMode == 8) ? "視聴" : "録画";

                // Rockbarで予約変更を行う機能が有効の場合、予約の有効・無効切り替えメニューを追加
                if (_configManager.RockbarSetting.UseRockbarReserveMod)
                {
                    var reserveItem = menuItems.Add(reserveData.RecSetting.IsNoRec() ? $">> {viewOrRecText}予約を有効にする" : $">> {viewOrRecText}予約を無効にする");
                    reserveItem.Click += (s2, e2) => ToggleRecMode(reserveData);
                }

                // Rockbarで予約削除を行う機能が有効の場合、予約の削除メニューを追加
                if (_configManager.RockbarSetting.UseRockbarReserveDel)
                {
                    var reserveItem = menuItems.Add($">> {viewOrRecText}予約を削除する");
                    reserveItem.Click += (s2, e2) => DelProgramReserve(reserveData);
                }

                if (_configManager.RockbarSetting.UseRockbarReserveMod || _configManager.RockbarSetting.UseRockbarReserveDel)
                {
                    menuItems.Add(new ToolStripSeparator());
                }

                // 予約コメント
                string displayText = !string.IsNullOrEmpty(reserveData.Comment)
                ? $"予約コメント : {reserveData.Comment}"
                : "予約コメントなし(EPG手動予約)";

                var item = menuItems.Add(displayText);
                item.Click += (s2, e2) => CopyText(reserveData.Comment);
                item.ToolTipText = "クリックで予約コメントをコピー";

                // 録画フォルダ（デフォルト以外が設定された場合に表示される）
                if (reserveData.RecSetting.RecFolderList.Count > 0)
                {
                    foreach (var recInfo in reserveData.RecSetting.RecFolderList)
                    {
                        if (!string.IsNullOrEmpty(recInfo.RecFolder))
                        {
                            AddBrokenTextMenuItems(menuItems, "録画フォルダ : ", recInfo.RecFolder, "録画フォルダ", "クリックで録画フォルダをコピー");
                        }
                    }
                }

                //// 指定ファイル名 
                //if (reserveData.RecFileNameList.Count > 0)
                //{
                //    foreach (var fileName in reserveData.RecFileNameList)
                //    {
                //        if (!string.IsNullOrEmpty(fileName))
                //        {
                //            AddBrokenTextMenuItems(menuItems, "ファイル名 : ", fileName, "ファイル名", "クリックで録画ファイル名をコピー");
                //        }
                //    }
                //}

                menuItems.Add(new ToolStripSeparator());
            }

            // 放送日時
            string dateTime = null;

            if (ev != null || reserveData != null)
            {
                DateTime startTime = ev?.start_time ?? reserveData.StartTime;
                uint duration = ev?.durationSec ?? reserveData.DurationSecond;

                dateTime = $"{startTime:yyyy/MM/dd(ddd) HH:mm}～{startTime.AddSeconds(duration):HH:mm}";
                var dateItem = menuItems.Add(dateTime);
                dateItem.Click += (s2, e2) => CopyText(dateTime);
                dateItem.ToolTipText = "クリックで日時をコピー";
            }

            // チャンネル名
            uint tsid = ev?.transport_stream_id ?? reserveData?.TransportStreamID ?? 0;
            uint sid = ev?.service_id ?? reserveData?.ServiceID ?? 0;
            string serviceName = (tsid != 0 || sid != 0) ? _listViewBuilder.GetServiceName((ushort)tsid, (ushort)sid) : null;

            if (!string.IsNullOrEmpty(serviceName))
            {
                var item = menuItems.Add(serviceName);
                item.Click += (s2, e2) => CopyText(serviceName);
                item.ToolTipText = "クリックでチャンネル名をコピー";
            }

            // 番組名
            if (!string.IsNullOrEmpty(title))
            {
                AddBrokenTextMenuItems(menuItems, "", title, "番組名", "クリックで番組名をコピー");
            }

            // 番組説明文（ShortInfo / ExtInfo）
            string shortText = ev?.ShortInfo?.text_char?.Trim();
            string longText = ev?.ExtInfo?.text_char?.Trim();
            if (ev != null)
            {
                // 番組概要
                if (!string.IsNullOrEmpty(shortText))
                {
                    menuItems.Add(new ToolStripSeparator());
                    AddBrokenTextMenuItems(menuItems, "", shortText, "番組概要", "クリックで番組概要をコピー");
                }

                // 番組詳細
                if (!string.IsNullOrEmpty(longText))
                {
                    menuItems.Add(new ToolStripSeparator());
                    AddBrokenTextMenuItems(menuItems, "", longText, "番組詳細", "クリックで番組詳細をコピー");
                }
            }

            menuItems.Add(new ToolStripSeparator());

            // 一括コピーメニュー
            AddCopyAllMenuItem(
                menuItems,
                new[]
                {
                    dateTime,
                    serviceName,
                    !string.IsNullOrEmpty(title) ? $"{title}\n" : null,
                    !string.IsNullOrEmpty(shortText) ? $"{shortText}\n" : null,
                    !string.IsNullOrEmpty(longText) ? $"{longText}\n" : null
                },
                ">> すべての番組情報をコピー",
                "すべての番組情報をまとめてコピー"
            );
        }

        /// <summary>
        /// タイトル文字列から検索用メニュー（Google / Lucky）を構築して追加
        /// </summary>
        private void AddWebSearchMenuItems(ToolStripItemCollection menuItems, string rawTitle)
        {
            if (string.IsNullOrEmpty(rawTitle)) return;

            // [新] [字] [再] などの角括弧囲みを除去
            string searchKeyword = System.Text.RegularExpressions.Regex.Replace(rawTitle, @"\[.*?\]|【.*?】", "").Trim();
            if (string.IsNullOrEmpty(searchKeyword)) return;

            string encodedKeyword = Uri.EscapeDataString(searchKeyword);

            // Google検索
            var searchItem = menuItems.Add(">> 番組名をGoogleで検索");
            searchItem.Click += (s, e) =>
            {
                string searchUrl = $"https://www.google.com/search?q={encodedKeyword}";
                RockbarUtility.OpenBrowser(searchUrl);
            };

            // I'm Feeling Lucky 検索
            var luckyItem = menuItems.Add(">> 番組名でI'm Feeling Lucky(DuckDuckGo)");
            luckyItem.Click += (s, e) =>
            {
                //string luckyUrl = $"https://www.google.com/search?q={encodedKeyword}&btnI=1";
                string luckyUrl = $"https://duckduckgo.com/?q=\\{encodedKeyword}";
                RockbarUtility.OpenBrowser(luckyUrl);
            };
        }

        /// <summary>
        /// 長文テキストを分解し、行数に応じてサブメニュー化またはダイレクトにメニュー項目を追加
        /// </summary>
        /// <param name="menuItems">追加対象のメニューコレクション</param>
        /// <param name="preText">表示・折り返し用の接頭辞（例: "録画ファイル : " や "予約コメント : "）</param>
        /// <param name="rawText">本文（コピー対象のテキスト）</param>
        /// <param name="menuTitle">サブメニュー化時の親メニュー表示名</param>
        /// <param name="toolTipText">ツールチップ表示文字列</param>
        private void AddBrokenTextMenuItems(
            ToolStripItemCollection menuItems,
            string preText,
            string rawText,
            string menuTitle,
            string toolTipText)
        {
            if (string.IsNullOrEmpty(rawText)) return;

            string trimmedText = rawText.Trim();

            // 表示および折り返し計算には preText + rawText を使用
            string fullDisplayText = (preText ?? "") + trimmedText;
            var lines = RockbarUtility.BreakString(fullDisplayText);
            if (lines == null || lines.Count == 0) return;

            ToolStripItemCollection targetItems = menuItems;

            // 番組詳細など行数が多い場合はサブメニュー化する
            if (lines.Count >= 16)
            {
                // 親メニュー項目を作成（コピー対象は rawText のみ）
                var parentItem = new ToolStripMenuItem(menuTitle);
                parentItem.Click += (s, e) => CopyText(trimmedText);
                menuItems.Add(parentItem);

                // targetItems変数の参照先をサブメニューに変更する
                targetItems = parentItem.DropDownItems;
            }

            // 分割した各行をtargetItemsに設定（コピー対象は rawText のみ）
            foreach (string line in lines)
            {
                var item = targetItems.Add(line);
                item.Click += (s, e) => CopyText(trimmedText);
                item.ToolTipText = toolTipText;
            }
        }

        /// <summary>
        /// 収集したテキスト要素から一括コピー用のメニュー項目を作成して追加します
        /// </summary>
        /// <param name="menuItems">追加対象のメニューコレクション</param>
        /// <param name="infoList">結合対象のテキストリスト</param>
        /// <param name="menuTitle">メニュー項目名（例: ">> すべての番組情報をコピー"）</param>
        /// <param name="toolTipText">ツールチップ表示文字列（例: "すべての番組情報をまとめてコピー"）</param>
        private void AddCopyAllMenuItem(
            ToolStripItemCollection menuItems,
            IEnumerable<string> infoList,
            string menuTitle,
            string toolTipText)
        {
            var validList = infoList?.Where(x => !string.IsNullOrEmpty(x)).ToList();
            if (validList == null || validList.Count == 0) return;

            string fullProgramInfo = string.Join(Environment.NewLine, validList);
            var copyAllItem = menuItems.Add(menuTitle);
            copyAllItem.Click += (s, e) => CopyText(fullProgramInfo);
            copyAllItem.ToolTipText = toolTipText;
        }
    }
}