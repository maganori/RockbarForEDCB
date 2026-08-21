using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using EpgTimer;

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
        /// serviceListViewマウスクリック処理
        /// 右クリック時、直近30件の番組情報をコンテキストメニューに表示。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        public void HandleServiceClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            // 右クリック
            // 現在の番組を含め、今後の番組を30件までコンテキストメニューで表示(TVRockの仕様踏襲)
            if (e.Button == MouseButtons.Right)
            {
                if (targetListView.SelectedItems.Count == 0)
                {
                    return;
                }
                
                // クリックした箇所が自動選択されるので拾う
                var selected = targetListView.SelectedItems[0];

                _contextMenu.Items.Clear();

                EpgServiceEventInfo sv = null;
                _epgDataManager.ServiceMap.TryGetValue(selected.Name, out sv);

                // EPG未取得チャンネルは処理を抜ける
                if (sv == null)
                {
                    return;
                }

                // 終了していないイベントのみ抽出して開始日時順にソート
                var afterEventList = sv.eventList.FindAll(x => x.start_time.AddSeconds(x.durationSec) >= DateTime.Now).OrderBy(a => a.start_time);

                int i = 0;

                foreach (var ev in afterEventList)
                {
                    string eventKey = RockbarUtility.GetKey(ev.transport_stream_id, ev.service_id, ev.event_id);

                    ReserveData reserveData = null;

                    // 該当イベントが予約済みかどうか確認
                    if (_epgDataManager.ReserveMap.ContainsKey(eventKey))
                    {
                        reserveData = _epgDataManager.ReserveMap[eventKey];
                    }

                    // コンテキストメニューに番組項目を追加
                    _contextMenu.Items.Add(CreateEventToolStripMenuItem(ev, reserveData, false));

                    i++;
                    if (i >= 30)
                    {
                        break; // 最大30件まで表示
                    }
                }

                _contextMenu.Show((Control)sender, new Point(0, e.Y));
            }
        }

        /// <summary>
        /// serviceListViewマウスダブルクリック処理
        /// TVTest使用オプションがONの場合、カーソル箇所の番組を対象にTVTestを起動する。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        /// <param name="targetListView">対象のListView</param>
        public void HandleServiceDoubleClick(object sender, MouseEventArgs e, ListView targetListView)
        {
            // TVTest使用時のみ
            if (!_configManager.RockbarSetting.UseDoubleClickTvtest || targetListView.SelectedItems.Count == 0)
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

            if (e.Button != MouseButtons.Right)
            {
                return;
            }

            // クリックされた位置にあるアイテム（番組）を取得
            var hitTest = targetListView.HitTest(e.Location);
            var selectedItem = hitTest.Item;

            if (selectedItem == null)
            {
                return;
            }

            // Tagに格納されているものがEpgEventInfoならevへ代入
            if (selectedItem.Tag is EpgEventInfo ev)
            {
                _contextMenu.Items.Clear();

                // Web番組詳細リンク (オプション有効時)
                if (_configManager.RockbarSetting.UseWebLink)
                {
                    var item = _contextMenu.Items.Add(">> Web番組詳細を開く");
                    item.Click += (s2, e2) => AccessWebUrl(ev);
                    _contextMenu.Items.Add(new ToolStripSeparator());
                }

                // 予約データの検索し、有無結果を格納
                string eventKey = RockbarUtility.GetKey(ev.transport_stream_id, ev.service_id, ev.event_id);
                var hasData = _epgDataManager.ReserveMap.TryGetValue(eventKey, out var reserveData);

                // 録画の有効・無効切り替え (予約データが存在する場合)
                if (hasData)
                {
                    var item = _contextMenu.Items.Add(reserveData.RecSetting.IsNoRec() ? "予約を有効にする" : "予約を無効にする");
                    item.Click += (s2, e2) => ToggleRecMode(reserveData);
                    _contextMenu.Items.Add(new ToolStripSeparator());
                }

                // 日時情報の表示
                var dateTime = $"{ev.start_time:yyyy/MM/dd(ddd) HH:mm}～{ev.start_time.AddSeconds(ev.durationSec):HH:mm}";
                var dateItem = _contextMenu.Items.Add(dateTime);
                dateItem.Click += (s2, e2) => CopyText(dateTime);
                dateItem.ToolTipText = "クリックで日時をコピー";

                // チャンネル名
                // 設定ファイルのチャンネル名を最優先で使用し、設定がなければEDCBのStationNameを使用
                string serviceName = _listViewBuilder.GetServiceName(ev.transport_stream_id, ev.service_id);

                if (!string.IsNullOrEmpty(serviceName))
                {
                    var serviceItem = _contextMenu.Items.Add(serviceName);
                    serviceItem.Click += (s2, e2) => CopyText(serviceName);
                    serviceItem.ToolTipText = "クリックでチャンネル名をコピー";
                }

                // 番組名
                string eventTitle = ev.ShortInfo?.event_name;
                if (!string.IsNullOrEmpty(eventTitle))
                {
                    var titleItem = _contextMenu.Items.Add(eventTitle);
                    titleItem.Click += (s2, e2) => CopyText(eventTitle);
                    titleItem.ToolTipText = "クリックで番組名をコピー";
                }

                _contextMenu.Items.Add(new ToolStripSeparator());

                // 短い番組説明
                var shortStrs = RockbarUtility.BreakString(ev.ShortInfo?.text_char);
                if (shortStrs != null)
                {
                    foreach (string str in shortStrs)
                    {
                        var item = _contextMenu.Items.Add(str);
                        item.Enabled = false;
                    }
                }

                // 詳細な番組説明
                var longStrs = RockbarUtility.BreakString(ev.ExtInfo?.text_char);
                if (longStrs != null && longStrs.Count > 0)
                {
                    _contextMenu.Items.Add(new ToolStripSeparator());
                    foreach (string str in longStrs)
                    {
                        var item = _contextMenu.Items.Add(str);
                        item.Enabled = false;
                    }
                }

                _contextMenu.Show((Control)sender, e.Location);
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
                        AccessWebUrl(ev);
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
        public void HandleReserveClick(object sender, MouseEventArgs e, ListView targetListView, Action<string> onFilterRequest)
        {
            if (targetListView.SelectedItems.Count == 0)
            {
                return;
            }

            // 右クリック
            // 対象の番組情報をコンテキストメニューで表示
            if (e.Button == MouseButtons.Right)
            {
                _contextMenu.Items.Clear();

                try
                {
                    // クリックした箇所が自動選択されるので拾う
                    var selected = targetListView.SelectedItems[0];

                    EpgEventInfo ev = _epgDataManager.AllEventMap[selected.Name];

                    // Webリンク使用時のみ1行目にWebリンク用のボタンを表示
                    if (_configManager.RockbarSetting.UseWebLink)
                    {
                        var item = _contextMenu.Items.Add(">>");
                        item.Click += (s2, e2) => AccessWebUrl(ev);

                        _contextMenu.Items.Add(new ToolStripSeparator());
                    }

                    // 予約有効/無効の切り替えメニュー
                    var hasData = _epgDataManager.ReserveMap.TryGetValue(selected.Name, out var reserveData);
                    if (hasData)
                    {
                        var item = _contextMenu.Items.Add(reserveData.RecSetting.IsNoRec() ? "予約を有効にする" : "予約を無効にする");
                        item.Click += (s2, e2) => ToggleRecMode(reserveData);
                        _contextMenu.Items.Add(new ToolStripSeparator());
                    }

                    // 放送日時の表示・コピー
                    var dateTime = $"{ev.start_time:yyyy/MM/dd(ddd) HH:mm}～{ev.start_time.AddSeconds(ev.durationSec):HH:mm}";
                    var dateItem = _contextMenu.Items.Add(dateTime);
                    dateItem.Click += (s2, e2) => CopyText(dateTime);
                    _contextMenu.Items.Add(new ToolStripSeparator());

                    // 予約情報をメニューに追加
                    if (hasData)
                    {
                        const string copyCommandText = "テキストをコピー";
                        const string copyCommandTooltipText = "クリックでテキストをコピー";
                        const string filterCommandText = "フィルタリング";

                        // --- チャンネル名の取得 ---
                        // 設定ファイルのチャンネル名を最優先で使用し、設定がなければEDCBのStationNameを使用
                        string serviceName = _listViewBuilder.GetServiceName(reserveData.TransportStreamID, reserveData.ServiceID);

                        // サービス名を追加
                        if (!string.IsNullOrEmpty(serviceName))
                        {
                            var item = new ToolStripMenuItem(serviceName);
                            _contextMenu.Items.Add(item);
                            var subItem = item.DropDownItems.Add(copyCommandText);
                            subItem.Click += (s2, e2) => CopyText(serviceName);
                            subItem = item.DropDownItems.Add(filterCommandText);
                            subItem.Click += (s2, e2) => onFilterRequest?.Invoke(serviceName);
                        }

                        // 予約番組名を追加
                        if (!string.IsNullOrEmpty(reserveData.Title))
                        {
                            var item = new ToolStripMenuItem(reserveData.Title);
                            _contextMenu.Items.Add(item);
                            var subItem = item.DropDownItems.Add(copyCommandText);
                            subItem.Click += (s2, e2) => CopyText(reserveData.Title);
                            subItem = item.DropDownItems.Add(filterCommandText);
                            subItem.Click += (s2, e2) => onFilterRequest?.Invoke(reserveData.Title);
                            _contextMenu.Items.Add(new ToolStripSeparator());
                        }

                        // 予約コメントを追加
                        if (!string.IsNullOrEmpty(reserveData.Comment))
                        {
                            var item = _contextMenu.Items.Add(reserveData.Comment);
                            item.Click += (s2, e2) => CopyText(reserveData.Comment);
                            item.ToolTipText = copyCommandTooltipText;
                            _contextMenu.Items.Add(new ToolStripSeparator());
                        }

                        // 録画フォルダおよび指定ファイル名を追加
                        if (reserveData.RecSetting.RecFolderList.Count > 0)
                        {
                            var recFolderList = reserveData.RecSetting.RecFolderList;
                            foreach (var recInfo in recFolderList)
                            {
                                if (!string.IsNullOrEmpty(recInfo.RecFolder))
                                {
                                    var item = _contextMenu.Items.Add(recInfo.RecFolder);
                                    item.Click += (s2, e2) => CopyText(recInfo.RecFolder);
                                    item.ToolTipText = copyCommandTooltipText;
                                }
                            }
                            foreach (var fileName in reserveData.RecFileNameList)
                            {
                                if (!string.IsNullOrEmpty(fileName))
                                {
                                    var item = _contextMenu.Items.Add(fileName);
                                    item.Click += (s2, e2) => CopyText(fileName);
                                    item.ToolTipText = copyCommandTooltipText;
                                }
                            }
                            _contextMenu.Items.Add(new ToolStripSeparator());
                        }
                    }

                    // 短い番組説明テキストの整形と追加
                    var shortStrs = RockbarUtility.BreakString(ev.ShortInfo?.text_char);

                    if (shortStrs != null)
                    {
                        foreach (string str in shortStrs)
                        {
                            var item = _contextMenu.Items.Add(str);
                            item.Enabled = false;
                        }
                    }

                    _contextMenu.Items.Add(new ToolStripSeparator());

                    // 詳細番組説明テキストの整形と追加
                    var longStrs = RockbarUtility.BreakString(ev.ExtInfo?.text_char);

                    if (longStrs != null)
                    {
                        foreach (string str in longStrs)
                        {
                            var item = _contextMenu.Items.Add(str);
                            item.Enabled = false;
                        }
                    }
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

                    EpgEventInfo ev = _epgDataManager.AllEventMap[selected.Name];
                    AccessWebUrl(ev);
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
        public void HandleRecClick(object sender, MouseEventArgs e, ListView targetListView, Action<string> onFilterRequest)
        {
            if (targetListView.SelectedItems.Count == 0)
            {
                return;
            }

            // 右クリック
            // 対象の番組情報をコンテキストメニューで表示
            if (e.Button == MouseButtons.Right)
            {
                _contextMenu.Items.Clear();

                try
                {
                    // クリックした箇所が自動選択されるので拾う
                    var selected = targetListView.SelectedItems[0];

                    if (!uint.TryParse(selected.Name, out uint recID))
                    {
                        MessageBox.Show("録画情報IDの取得に失敗しました。", "録画情報IDエラー");
                        return;
                    }

                    RecFileInfo recFile = _epgDataManager.RecMap[recID];

                    // Webリンク使用時のみ1行目にWebリンク用のボタンを表示
                    if (_configManager.RockbarSetting.UseWebLink)
                    {
                        var item = _contextMenu.Items.Add(">>");
                        item.Click += (s2, e2) => AccessWebUrl(recFile);
                        _contextMenu.Items.Add(new ToolStripSeparator());
                    }

                    // 録画ファイル詳細情報のテキスト群を生成してメニューに追加
                    var detailTexts = _listViewBuilder.CreateRecInfoDetailTexts(recFile);
                    if (detailTexts.Count > 0)
                    {
                        const string copyCommandText = "テキストをコピー";
                        const string copyCommandTooltipText = "クリックでテキストをコピー";
                        const string filterCommandText = "フィルタリング";

                        // 録画日時
                        if (detailTexts[0].Count > 0)
                        {
                            foreach (var text in detailTexts[0])
                            {
                                var item = _contextMenu.Items.Add(text.Value);
                                item.Click += (s2, e2) => CopyText(text.CopyText);
                                item.ToolTipText = copyCommandTooltipText;
                            }
                            _contextMenu.Items.Add(new ToolStripSeparator());
                        }

                        // 録画サービス名・番組名（サブメニュー化してコピー/フィルタリング選択）
                        if (detailTexts[1].Count > 0)
                        {
                            foreach (var text in detailTexts[1])
                            {
                                var item = new ToolStripMenuItem(text.Value);
                                _contextMenu.Items.Add(item);
                                var subItem = item.DropDownItems.Add(copyCommandText);
                                subItem.Click += (s2, e2) => CopyText(text.CopyText);
                                subItem = item.DropDownItems.Add(filterCommandText);
                                subItem.Click += (s2, e2) => onFilterRequest?.Invoke(text.CopyText);
                            }
                            _contextMenu.Items.Add(new ToolStripSeparator());
                        }

                        // 結果・パス・各種ID群
                        foreach (var texts in detailTexts.GetRange(2, detailTexts.Count - 3))
                        {
                            if (texts.Count == 0)
                            {
                                continue;
                            }
                            foreach (var text in texts)
                            {
                                var item = _contextMenu.Items.Add(text.Value);
                                item.Click += (s2, e2) => CopyText(text.CopyText);
                                item.ToolTipText = copyCommandTooltipText;
                            }
                            _contextMenu.Items.Add(new ToolStripSeparator());
                        }

                        // Drop・Scramble情報
                        var lastTexts = detailTexts[detailTexts.Count - 1];
                        if (lastTexts.Count > 0)
                        {
                            foreach (var text in lastTexts)
                            {
                                var item = _contextMenu.Items.Add(text.Value);
                                item.Click += (s2, e2) => CopyText(text.CopyText);
                                item.ToolTipText = copyCommandTooltipText;
                            }
                        }
                    }
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

                RecFileInfo recFile = _epgDataManager.RecMap[recID];

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
            // 右クリック
            // 今後の予約を30件までコンテキストメニューで表示
            if (e.Button == MouseButtons.Right)
            {
                if (targetListView.SelectedItems.Count == 0)
                {
                    return;
                }

                // クリックした箇所が自動選択されるので拾う
                var selected = targetListView.SelectedItems[0];

                _contextMenu.Items.Clear();

                TunerReserveInfo tunerReserveInfo = _epgDataManager.TunerReserveInfos.Find(x => x.tunerID.ToString() == selected.Name);
                if (tunerReserveInfo == null)
                {
                    return;
                }

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

                    // チューナー予約一覧のコンテキストメニュー項目生成
                    _contextMenu.Items.Add(CreateEventToolStripMenuItem(ev, reserveData, true));

                    i++;
                    if (i >= 30)
                    {
                        break; // 最大30件
                    }
                }

                _contextMenu.Show((Control)sender, new Point(0, e.Y));
            }
        }

        /// <summary>
        /// 録画モード有効・無効の切り替え処理
        /// 録画有効時は無効化、無効時は有効化する。
        /// </summary>
        public void ToggleRecMode(ReserveData reserve)
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

            var err = _ctrlCmdUtil.SendChgReserve(new List<ReserveData>() { reserve });
            if (err != ErrCode.CMD_SUCCESS)
            {
                MessageBox.Show("予約変更でエラーが発生しました。", "予約変更エラー");
            }
            _refreshList?.Invoke(true, false, false);
        }

        /// <summary>
        /// マウスクリック処理に応じてテキストのコピーをする
        /// </summary>
        /// <param name="text">対象テキスト</param>
        public void CopyText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text);
            }
        }

        /// <summary>
        /// EpgEventInfo に基づいて Web番組詳細を表示
        /// </summary>
        public void AccessWebUrl(EpgEventInfo ev)
        {
            string url = _configManager.RockbarSetting.WebLinkUrl
                .Replace("{ONID}", ev.original_network_id.ToString())
                .Replace("{TSID}", ev.transport_stream_id.ToString())
                .Replace("{SID}", ev.service_id.ToString())
                .Replace("{EID}", ev.event_id.ToString());

            OpenBrowser(url);
        }

        /// <summary>
        /// RecFileInfo に基づいて Web録画詳細を表示
        /// </summary>
        public void AccessWebUrl(RecFileInfo recFile)
        {
            string url = _configManager.RockbarSetting.RecInfoWebLinkUrl
                .Replace("{RecID}", recFile.ID.ToString());

            OpenBrowser(url);
        }

        /// <summary>
        /// URLをブラウザで開く処理
        /// </summary>
        private void OpenBrowser(string url)
        {
            try
            {
                new Uri(url);
                var startInfo = new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true };
                System.Diagnostics.Process.Start(startInfo);
            }
            catch
            {
                MessageBox.Show($"Web番組詳細URLが不正です。Web番組詳細URLの設定を見直してください。\nURL: {url}", "ブラウザ起動エラー");
            }
        }

        /// <summary>
        /// 番組の右クリックコンテキストメニューItem作成処理
        /// 番組情報・予約情報からチャンネル一覧・チューナー一覧用のコンテキストメニューitemを作成する。
        /// </summary>
        /// <param name="ev">番組情報</param>
        /// <param name="reserve">予約情報</param>
        /// <param name="isTuner">チューナー一覧用？</param>
        /// <returns></returns>
        public ToolStripMenuItem CreateEventToolStripMenuItem(EpgEventInfo ev, ReserveData reserve, bool isTuner)
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
            if (ev != null)
            {
                // Webリンク使用時のみ1行目にWebリンク用のボタンを表示
                if (_configManager.RockbarSetting.UseWebLink)
                {
                    item.DropDownItems.Add(">>");
                    item.DropDownItems[item.DropDownItems.Count - 1].Click += (s2, e2) => AccessWebUrl(ev);
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

    }
}