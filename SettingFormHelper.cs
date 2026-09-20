using EpgTimer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RockbarForEDCB
{
    /// <summary>
    /// ListView用の比較クラス
    /// ListViewごとに作成・状態保持しておき、列ヘッダクリックごとにソートを行う
    /// </summary>
    public class ListViewItemComparer : IComparer
    {
        private int _columnIndex = -1;
        private SortOrder _sortOrder = SortOrder.Ascending;
        private bool _isNumber = false;
        private bool _isNetworkTypeColumn = false;

        /// <summary>
        /// 列index更新処理
        /// </summary>
        /// <param name="columnIndex">列index</param>
        /// <param name="isNumber">数値列の場合はtrueにする</param>
        /// <param name="isNetworkTypeColumn">ネットワーク種別列の場合はtrueにする</param>
        public void SetColumn(int columnIndex, bool isNumber = false, bool isNetworkTypeColumn = false)
        {
            _isNumber = isNumber;
            _isNetworkTypeColumn = isNetworkTypeColumn;

            // 同じカラムがクリックされた場合はソート方向を反転
            if (_columnIndex == columnIndex)
            {
                if (_sortOrder == SortOrder.Ascending)
                {
                    _sortOrder = SortOrder.Descending;
                }
                else
                {
                    _sortOrder = SortOrder.Ascending;
                }
            }
            else
            {
                _sortOrder = SortOrder.Ascending;
            }

            _columnIndex = columnIndex;
        }

        /// <summary>
        /// 比較処理
        /// 要素1<要素2 → 負, 要素1>要素2 → 正, 要素1=要素2 → 0
        /// </summary>
        /// <param name="x1">要素1</param>
        /// <param name="x2">要素2</param>
        /// <returns>比較結果</returns>
        public int Compare(object x1, object x2)
        {
            if (_columnIndex < 0)
            {
                return 0;
            }

            ListViewItem item1 = (ListViewItem)x1;
            ListViewItem item2 = (ListViewItem)x2;

            string text1 = item1.SubItems[_columnIndex].Text;
            string text2 = item2.SubItems[_columnIndex].Text;

            int result = 0;

            // ネットワーク種別カラムの場合
            if (_isNetworkTypeColumn)
            {
                // ネットワーク種別文字列に応じた並び順インデックスを取得して並べ替え
                int order1 = GetNetworkTypeOrder(text1);
                int order2 = GetNetworkTypeOrder(text2);
                result = order1.CompareTo(order2);

                // 同じ種別（地デジ同士など）なら名前で比較
                if (result == 0)
                {
                    result = string.Compare(text1, text2);
                }
            }
            // 数値カラムの場合
            else if (_isNumber)
            {
                if (int.TryParse(text1, out int num1) && int.TryParse(text2, out int num2))
                {
                    result = num1.CompareTo(num2);
                }
                else
                {
                    result = string.Compare(text1, text2);
                }
            }
            // その他(文字列カラム)の場合
            else
            {
                result = string.Compare(text1, text2);
            }

            if (_sortOrder == SortOrder.Descending)
            {
                result *= -1;
            }

            return result;
        }

        /// <summary>
        /// ネットワーク種別文字列に応じた表示優先度（並び順インデックス）を取得する
        /// </summary>
        private int GetNetworkTypeOrder(string value)
        {
            if (value.StartsWith("地"))
                return 0;

            // BSで引っかかる前に設定
            if (value.StartsWith("BS4K"))
                return 5;

            if (value.StartsWith("BS"))
                return 1;

            if (value.StartsWith("CS"))
                return 2;

            if (value.StartsWith("CATV"))
                return 3;

            if (value.StartsWith("SPHD"))
                return 4;

            return 99;
        }
    }

    /// <summary>
    /// サービス一覧 ListView の列インデックス定義
    /// </summary>
    public static class ServiceListViewColumns
    {
        public const int Mark = 0;
        public const int NetworkType = 1;
        public const int Name = 2;
        public const int Tsid = 3;
        public const int Sid = 4;
        public const int TvtestOption = 5;
    }

    /// <summary>
    /// 選択チャンネルタブの全サービス一覧と選択サービス一覧を管理するクラス
    /// </summary>
    public class SelectedServiceListViewManager
    {
        private readonly ConfigManager _configManager;
        private readonly List<EpgServiceInfo> _serviceInfos;

        private readonly ListView _allServiceListView;
        private readonly ListView _selectedServiceListView;

        private readonly ListViewItemComparer _allServiceListViewSorter;
        private readonly ListViewItemComparer _selectedServiceListViewSorter;

        /// <summary>
        /// サービス新規追加編集フォームクラス
        /// </summary>
        private class ServiceEditDialog : Form
        {
            private readonly TextBox _tsidTextBox = new TextBox();
            private readonly TextBox _sidTextBox = new TextBox();
            private readonly TextBox _nameTextBox = new TextBox();
            private readonly ComboBox _typeComboBox = new ComboBox();
            private readonly TextBox _tvtestOptionTextBox = new TextBox();

            private readonly List<EpgServiceInfo> _serviceInfos;
            private readonly HashSet<string> _existingKeys;

            /// <summary>
            /// ダイアログ確定後の編集・追加結果（ListViewItem）を取得します。
            /// </summary>
            public ListViewItem ResultItem { get; private set; }

            /// <summary>
            /// サービス新規追加編集
            /// </summary>
            /// <param name="initialItem">編集対象のListViewItem（新規追加時は null）</param>
            /// <param name="epgServiceInfos">全サービス情報一覧</param>
            /// <param name="existingKeys">既に登録されているキー（TSID-SID）の集合</param>
            /// <param name="rockBarSetting">フォント設定</param>
            public ServiceEditDialog(
                ListViewItem initialItem,
                List<EpgServiceInfo> epgServiceInfos,
                HashSet<string> existingKeys,
                RockBarSetting rockBarSetting = null)
            {
                this.Text = (initialItem != null) ? "チャンネル編集" : "チャンネル追加";
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.StartPosition = FormStartPosition.CenterParent;
                this.MinimizeBox = false;
                this.MaximizeBox = false;
                this.ShowInTaskbar = false;
                this.ClientSize = new Size(360, 210);

                _serviceInfos = epgServiceInfos;
                _existingKeys = existingKeys;

                Label tsidLabel = new Label { Left = 16, Top = 18, Width = 100, Text = "TSID" };
                _tsidTextBox.Left = 120;
                _tsidTextBox.Top = 14;
                _tsidTextBox.Width = 200;

                Label sidLabel = new Label { Left = 16, Top = 48, Width = 100, Text = "SID" };
                _sidTextBox.Left = 120;
                _sidTextBox.Top = 44;
                _sidTextBox.Width = 200;

                Label nameLabel = new Label { Left = 16, Top = 78, Width = 100, Text = "名前" };
                _nameTextBox.Left = 120;
                _nameTextBox.Top = 74;
                _nameTextBox.Width = 200;

                Label typeLabel = new Label { Left = 16, Top = 108, Width = 100, Text = "Type" };
                _typeComboBox.Left = 120;
                _typeComboBox.Top = 104;
                _typeComboBox.Width = 200;
                _typeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                _typeComboBox.Items.AddRange(new object[] { "自動判別", "地", "BS", "CS", "CATV", "SPHD", "BS4K" });
                _typeComboBox.SelectedIndex = 0; // デフォルトは"自動判別"

                Label tvtestOptionLabel = new Label { Left = 16, Top = 138, Width = 100, Text = "TVTestオプション" };
                _tvtestOptionTextBox.Left = 120;
                _tvtestOptionTextBox.Top = 134;
                _tvtestOptionTextBox.Width = 200;

                Button okButton = new Button { Left = 164, Top = 172, Width = 75, Text = "OK", DialogResult = DialogResult.OK };
                Button cancelButton = new Button { Left = 245, Top = 172, Width = 75, Text = "キャンセル",
                                                    DialogResult = DialogResult.Cancel };

                okButton.Click += OkButton_Click;

                this.Controls.AddRange(new Control[] {
                    tsidLabel, _tsidTextBox,
                    sidLabel, _sidTextBox,
                    nameLabel, _nameTextBox,
                    typeLabel, _typeComboBox,
                    tvtestOptionLabel, _tvtestOptionTextBox,
                    okButton, cancelButton
                });

                this.AcceptButton = okButton;
                this.CancelButton = cancelButton;

                // 編集モード：ListViewItem から値を直接展開
                if (initialItem != null)
                {
                    _tsidTextBox.Text = initialItem.SubItems[ServiceListViewColumns.Tsid].Text;
                    _sidTextBox.Text = initialItem.SubItems[ServiceListViewColumns.Sid].Text;
                    _nameTextBox.Text = initialItem.SubItems[ServiceListViewColumns.Name].Text;

                    string networkType = initialItem.SubItems[ServiceListViewColumns.NetworkType].Text;
                    int typeIndex = _typeComboBox.FindStringExact(
                        RockbarUtility.GetShortNetworkTypeName(RockbarUtility.GetNetworkType(networkType, null)));
                    if (typeIndex >= 0)
                    {
                        _typeComboBox.SelectedIndex = typeIndex;
                    }

                    _tvtestOptionTextBox.Text = initialItem.SubItems.Count > ServiceListViewColumns.TvtestOption
                        ? initialItem.SubItems[ServiceListViewColumns.TvtestOption].Text
                        : "";
                }

                UiFontHelper.ApplyUiFontSettings(this, rockBarSetting);
            }

            /// <summary>
            /// サービス新規追加編集フォームOKボタン
            /// </summary>
            /// <param name="sender">イベント発生元オブジェクト</param>
            /// <param name="e">イベント引数</param>
            private void OkButton_Click(object sender, EventArgs e)
            {
                if (!ushort.TryParse(_tsidTextBox.Text, out ushort tsid) || !ushort.TryParse(_sidTextBox.Text, out ushort sid))
                {
                    MessageBox.Show("TSID と SID は有効数値で入力してください。", "入力エラー",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                string key = RockbarUtility.GetKey(tsid.ToString(), sid.ToString());

                // 重複チェック（追加・編集共通）
                if (_existingKeys.Contains(key))
                {
                    MessageBox.Show(
                        "同じ TSID / SID のチャンネルが既に登録されています。",
                        "重複登録はできません",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    this.DialogResult = DialogResult.None;
                    return;
                }

                EpgServiceInfo matchedService = null;
                // 全チャンネル側に同じチャンネルがあれば取得
                if (_serviceInfos != null)
                {
                    matchedService = _serviceInfos
                        .FirstOrDefault(x => x.TSID == tsid && x.SID == sid);
                }

                // 名前補完
                // 未指定なら全チャンネル側の名前を使用、なければTSID-SID
                if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
                {
                    if (matchedService != null)
                    {
                        _nameTextBox.Text = matchedService.service_name;
                    }
                    else
                    {
                        _nameTextBox.Text = RockbarUtility.GetKey(tsid, sid);
                    }
                }

                // 種別補正
                // 種別指定が正しいか判定。誤りがあれば全チャンネル側の情報を使用
                if (matchedService != null)
                {
                    var networkType = RockbarUtility.GetNetworkType(_typeComboBox.Text.Trim(), matchedService.ONID);
                    _typeComboBox.Text = RockbarUtility.GetShortNetworkTypeName(networkType);
                }
                else
                {
                    var networkType = RockbarUtility.GetNetworkType(_typeComboBox.Text.Trim(), null);
                    _typeComboBox.Text = RockbarUtility.GetShortNetworkTypeName(networkType);
                }

                // ListViewItem を生成
                string typeStr = _typeComboBox.Text.Trim();
                string nameStr = _nameTextBox.Text.Trim();
                string tsidStr = tsid.ToString();
                string sidStr = sid.ToString();
                string optionStr = string.IsNullOrWhiteSpace(_tvtestOptionTextBox.Text) ? "" : _tvtestOptionTextBox.Text.Trim();

                ListViewItem item = new ListViewItem(""); // 0: Mark
                item.SubItems.Add(typeStr);                // 1: NetworkType
                item.SubItems.Add(nameStr);                // 2: Name
                item.SubItems.Add(tsidStr);                // 3: Tsid
                item.SubItems.Add(sidStr);                 // 4: Sid
                item.SubItems.Add(optionStr);              // 5: TvtestOption
                item.Name = key;

                ResultItem = item;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SelectedServiceListViewManager(
            ConfigManager configManager,
            List<EpgServiceInfo> serviceInfos,
            ListView allServiceListView,
            ListView selectedServiceListView)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _serviceInfos = serviceInfos ?? throw new ArgumentNullException(nameof(serviceInfos));

            _allServiceListView = allServiceListView ?? throw new ArgumentNullException(nameof(allServiceListView));
            _selectedServiceListView = selectedServiceListView ?? throw new ArgumentNullException(nameof(selectedServiceListView));

            _allServiceListViewSorter = new ListViewItemComparer();
            _selectedServiceListViewSorter = new ListViewItemComparer();
        }

        /// <summary>
        /// 全サービス一覧と選択サービス一覧を読み込む
        /// </summary>
        public void Load()
        {
            LoadAllServiceList();
            LoadSelectedServiceList();
        }

        /// <summary>
        /// 全サービス一覧を読み込む
        /// </summary>
        private void LoadAllServiceList()
        {
            foreach (EpgServiceInfo epgServiceInfo in _serviceInfos)
            {
                NetworkType networkType = RockbarUtility.GetNetworkType(epgServiceInfo.ONID);

                string typeName = RockbarUtility.GetShortNetworkTypeName(networkType);

                string[] data =
                {
                    "",
                    typeName,
                    epgServiceInfo.service_name,
                    epgServiceInfo.TSID.ToString(),
                    epgServiceInfo.SID.ToString()
                };

                ListViewItem item = new ListViewItem(data);
                item.Name = RockbarUtility.GetKey(epgServiceInfo.TSID, epgServiceInfo.SID);

                _allServiceListView.Items.Add(item);
            }

            // ネットワーク種別順で並べ替える
            SortAllServiceList(ServiceListViewColumns.NetworkType);
        }

        /// <summary>
        /// 設定ファイルの選択サービス一覧を読み込む
        /// </summary>
        private void LoadSelectedServiceList()
        {
            List<ListViewItem> needCheckItems = new List<ListViewItem>();

            foreach (Service service in _configManager.SelectedServiceList)
            {
                string key = RockbarUtility.GetKey(service.Tsid, service.Sid);

                if (_allServiceListView.Items.ContainsKey(key))
                {
                    ListViewItem allServiceItem = _allServiceListView.Items[key];
                    ListViewItem targetItem = (ListViewItem)allServiceItem.Clone();
                    targetItem.Name = allServiceItem.Name;

                    // NetworkType指定があれば上書きする
                    if (!string.IsNullOrWhiteSpace(service.TypeName))
                    {
                        targetItem.SubItems[ServiceListViewColumns.NetworkType].Text =
                            RockbarUtility.GetShortNetworkTypeName(
                                RockbarUtility.GetNetworkType(service.TypeName, null)
                            );
                    }

                    // チャンネル名指定があれば上書きする
                    if (!string.IsNullOrWhiteSpace(service.Name))
                    {
                        targetItem.SubItems[ServiceListViewColumns.Name].Text = service.Name;
                    }

                    // Tvtestオプションカラムを作成し設定
                    while (targetItem.SubItems.Count <= 5)
                    {
                        targetItem.SubItems.Add("");
                    }

                    targetItem.SubItems[ServiceListViewColumns.TvtestOption].Text = service.TvtestOption ?? "";

                    // Type または Name が左リストの元データと異なる場合、1列目に mod 印を付ける。
                    ServiceListViewHelper.ApplyModMarkIfChanged(allServiceItem, targetItem);

                    _selectedServiceListView.Items.Add(targetItem);

                    // 選択チャンネルにあるチャンネルはチェック表示
                    needCheckItems.Add(allServiceItem);
                }
                else
                {
                    ListViewItem manualAddItem = CreateServiceListItem(service);
                    manualAddItem.Name = key;

                    // 左リストに存在しないためadd印を付ける
                    ServiceListViewHelper.ApplyManualAddMark(manualAddItem);

                    _selectedServiceListView.Items.Add(manualAddItem);
                }
            }

            // チェックする
            needCheckItems.ForEach(x => ServiceListViewHelper.CheckServiceItem(x));
        }

        /// <summary>
        /// Service オブジェクトから ListViewItem を生成する。
        /// </summary>
        /// <param name="service">表示対象のサービス情報</param>
        /// <returns>生成された ListViewItem</returns>
        private static ListViewItem CreateServiceListItem(Service service)
        {
            string typeName = RockbarUtility.GetShortNetworkTypeName(RockbarUtility.GetNetworkType(service.TypeName, null));

            ListViewItem item = new ListViewItem(new string[] {
                "",
                typeName,
                service.Name ?? "",
                service.Tsid ?? "",
                service.Sid ?? "",
                service.TvtestOption ?? ""
            });
            item.Name = RockbarUtility.GetKey(service.Tsid, service.Sid);
            return item;
        }

        /// <summary>
        /// 選択サービス一覧の保存
        /// </summary>
        public void Save()
        {
            // TOMLだと編集しづらいかもしれないので、チャンネル系はTSVに保存
            _configManager.SelectedServiceList.Clear();

            foreach (ListViewItem item in _selectedServiceListView.Items)
            {
                _configManager.SelectedServiceList.Add(new Service
                {
                    Tsid = item.SubItems[ServiceListViewColumns.Tsid].Text,
                    Sid = item.SubItems[ServiceListViewColumns.Sid].Text,
                    Name = item.SubItems[ServiceListViewColumns.Name].Text,
                    TypeName = item.SubItems[ServiceListViewColumns.NetworkType].Text,
                    TvtestOption = item.SubItems[ServiceListViewColumns.TvtestOption].Text
                });
            }
        }

        /// <summary>
        /// 選択サービスを全サービスから追加する
        /// </summary>
        public void AddService()
        {
            ServiceListViewHelper.AddService(_allServiceListView, _selectedServiceListView);
        }

        /// <summary>
        /// 選択サービスを削除する
        /// </summary>
        public void RemoveService()
        {
            ServiceListViewHelper.RemoveService(_allServiceListView, _selectedServiceListView);
        }

        /// <summary>
        /// 新規サービスを追加する
        /// </summary>
        public void AddNewService()
        {
            EditServiceWithDialog(null);
        }

        /// <summary>
        /// 選択サービスを編集する
        /// </summary>
        public void EditService()
        {
            if (_selectedServiceListView.SelectedItems.Count > 0)
            {
                EditServiceWithDialog(_selectedServiceListView.SelectedItems[0]);
            }
        }

        /// <summary>
        /// 選択サービスの「新規追加」と「編集」処理。
        /// item == null → 新規追加モード
        /// item != null → 編集モード
        /// </summary>
        /// <param name="item">編集する ListViewItem</param>
        private void EditServiceWithDialog(ListViewItem item)
        {
            // 既存キー一覧（編集モードなら自分自身を除外）
            var existingKeys = _selectedServiceListView.Items.Cast<ListViewItem>()
                    .Where(x => x != item)
                    .Select(x => x.Name)
                    .ToHashSet();

            // 初期値（編集モードなら既存サービス、新規追加モードなら null）
            using (var dialog = new ServiceEditDialog(
                    item, _serviceInfos, existingKeys, _configManager.RockbarSetting))
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                ListViewItem newItem = dialog.ResultItem;
                string newKey = newItem.Name;

                if (_allServiceListView.Items.ContainsKey(newKey))
                {
                    // Type または Name が左リストの元データと異なる場合、1列目に mod 印を付ける。
                    ListViewItem originalItem = _allServiceListView.Items[newKey];
                    ServiceListViewHelper.ApplyModMarkIfChanged(originalItem, newItem);
                }
                else
                {
                    // 左リストにないため1列目に add 印を付ける。
                    ServiceListViewHelper.ApplyManualAddMark(newItem);
                }

                // 新規追加モード
                if (item == null)
                {
                    _selectedServiceListView.Items.Add(newItem);
                }

                // 編集モード
                else
                {
                    int index = item.Index;
                    _selectedServiceListView.Items.Remove(item);
                    _selectedServiceListView.Items.Insert(index, newItem);
                    newItem.Selected = true;

                    // 編集前のチャンネルが左リストにあれば印を外す。
                    if (_allServiceListView.Items.ContainsKey(item.Name))
                    {
                        ServiceListViewHelper.UncheckServiceItem(_allServiceListView.Items[item.Name]);
                    }
                }

                // 編集後のチャンネルが左リストにあれば印を付ける。
                if (_allServiceListView.Items.ContainsKey(newKey))
                {
                    ServiceListViewHelper.CheckServiceItem(_allServiceListView.Items[newKey]);
                }
            }
        }

        /// <summary>
        /// 選択サービスを上に移動する
        /// </summary>
        public void MoveUp()
        {
            ServiceListViewHelper.MoveUp(_selectedServiceListView);
        }

        /// <summary>
        /// 選択サービスを下に移動する
        /// </summary>
        public void MoveDown()
        {
            ServiceListViewHelper.MoveDown(_selectedServiceListView);
        }

        /// <summary>
        /// 全サービス一覧をソートする
        /// </summary>
        public void SortAllServiceList(int column)
        {
            _allServiceListViewSorter.SetColumn(column,
                column == ServiceListViewColumns.Tsid || column == ServiceListViewColumns.Sid,
                column == ServiceListViewColumns.NetworkType);
            _allServiceListView.ListViewItemSorter = _allServiceListViewSorter;
            _allServiceListView.Sort();
        }

        /// <summary>
        /// 選択サービス一覧をソートする
        /// </summary>
        public void SortSelectedServiceList(int column)
        {
            _selectedServiceListViewSorter.SetColumn(column,
                column == ServiceListViewColumns.Tsid || column == ServiceListViewColumns.Sid,
                column == ServiceListViewColumns.NetworkType);
            _selectedServiceListView.ListViewItemSorter = _selectedServiceListViewSorter;
            _selectedServiceListView.Sort();
        }
    }

    /// <summary>
    /// お気に入りチャンネルタブの選択サービスとお気に入りサービス一覧を管理するクラス
    /// </summary>
    public class FavoriteServiceListViewManager
    {
        private readonly ConfigManager _configManager;
        private readonly ListView _selectedServiceListView;

        private readonly ListView _selectedServiceListView2;
        private readonly ListView _favoriteServiceListView;

        private readonly ListViewItemComparer _selectedServiceListView2Sorter;
        private readonly ListViewItemComparer _favoriteServiceListViewSorter;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public FavoriteServiceListViewManager(
            ConfigManager configManager,
            ListView selectedServiceListView,
            ListView selectedServiceListView2,
            ListView favoriteServiceListView)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _selectedServiceListView = selectedServiceListView ?? throw new ArgumentNullException(nameof(selectedServiceListView));

            _selectedServiceListView2 = selectedServiceListView2 ?? throw new ArgumentNullException(nameof(selectedServiceListView2));
            _favoriteServiceListView = favoriteServiceListView ?? throw new ArgumentNullException(nameof(favoriteServiceListView));

            _selectedServiceListView2Sorter = new ListViewItemComparer();
            _favoriteServiceListViewSorter = new ListViewItemComparer();
        }

        /// <summary>
        /// お気に入りサービス一覧を読み込む
        /// </summary>
        public void Load()
        {
            foreach (Service service in _configManager.FavoriteServiceList)
            {
                // 1回キーとTSID, SIDだけで追加
                string[] data =
                {
                    "",
                    "",
                    "",
                    service.Tsid.ToString(),
                    service.Sid.ToString(),
                    ""
                };

                ListViewItem item = new ListViewItem(data);
                item.Name = RockbarUtility.GetKey(service.Tsid, service.Sid);
                _favoriteServiceListView.Items.Add(item);
            }
        }

        /// <summary>
        /// お気に入りサービス一覧の保存
        /// </summary>
        public void Save()
        {
            // 保存前に選択サービスListViewの変更内容（Name, NetworkType等）をお気に入りListView側に反映
            SyncAndRefresh();

            // TOMLだと編集しづらいかもしれないので、チャンネル系はTSVに保存
            _configManager.FavoriteServiceList.Clear();

            foreach (ListViewItem item in _favoriteServiceListView.Items)
            {
                _configManager.FavoriteServiceList.Add(new Service
                {
                    Tsid = item.SubItems[ServiceListViewColumns.Tsid].Text,
                    Sid = item.SubItems[ServiceListViewColumns.Sid].Text,
                    Name = item.SubItems[ServiceListViewColumns.Name].Text,
                    TypeName = item.SubItems[ServiceListViewColumns.NetworkType].Text,
                    TvtestOption = item.SubItems[ServiceListViewColumns.TvtestOption].Text
                });
            }
        }

        /// <summary>
        /// お気に入りサービスを追加する
        /// </summary>
        public void AddService()
        {
            ServiceListViewHelper.AddService(_selectedServiceListView2, _favoriteServiceListView);
        }

        /// <summary>
        /// お気に入りサービスを削除する
        /// </summary>
        public void RemoveService()
        {
            ServiceListViewHelper.RemoveService(_selectedServiceListView2, _favoriteServiceListView);
        }

        /// <summary>
        /// お気に入りサービスを上に移動する
        /// </summary>
        public void MoveUp()
        {
            ServiceListViewHelper.MoveUp(_favoriteServiceListView);
        }

        /// <summary>
        /// お気に入りサービスを下に移動する
        /// </summary>
        public void MoveDown()
        {
            ServiceListViewHelper.MoveDown(_favoriteServiceListView);
        }

        /// <summary>
        /// 選択サービス一覧を指定されたListViewからコピーし、お気に入りサービスの表示を更新する
        /// </summary>
        public void SyncAndRefresh()
        {
            // 選択サービスを選択サービスタブからコピーし直す
            _selectedServiceListView2.Items.Clear();

            foreach (ListViewItem item in _selectedServiceListView.Items)
            {
                ListViewItem copiedItem = (ListViewItem)item.Clone();
                copiedItem.Name = item.Name;
                _selectedServiceListView2.Items.Add(copiedItem);
            }

            // お気に入りサービス一覧側へ同期
            List<ListViewItem> needCheckItems = new List<ListViewItem>();

            foreach (ListViewItem favoriteItem in _favoriteServiceListView.Items)
            {
                string key = favoriteItem.Name;

                if (_selectedServiceListView2.Items.ContainsKey(key))
                {
                    // 登録済みはチェック表示
                    ListViewItem service2Item = _selectedServiceListView2.Items[key];
                    favoriteItem.SubItems[ServiceListViewColumns.Mark].Text = service2Item.SubItems[ServiceListViewColumns.Mark].Text;
                    favoriteItem.SubItems[ServiceListViewColumns.NetworkType].Text = service2Item.SubItems[ServiceListViewColumns.NetworkType].Text;
                    favoriteItem.SubItems[ServiceListViewColumns.Name].Text = service2Item.SubItems[ServiceListViewColumns.Name].Text;
                    favoriteItem.SubItems[ServiceListViewColumns.TvtestOption].Text = service2Item.SubItems[ServiceListViewColumns.TvtestOption].Text;
                    needCheckItems.Add(service2Item);
                }
                else
                {
                    favoriteItem.SubItems[ServiceListViewColumns.Mark].Text = "！";
                }
            }

            // チェックする
            needCheckItems.ForEach(x => ServiceListViewHelper.CheckServiceItem(x));
        }

        /// <summary>
        /// お気に入りサービス一覧をソートする。
        /// </summary>
        public void SortSelectedService2List(int column)
        {
            _selectedServiceListView2Sorter.SetColumn(column,
                column == ServiceListViewColumns.Tsid || column == ServiceListViewColumns.Sid,
                column == ServiceListViewColumns.NetworkType);
            _selectedServiceListView2.ListViewItemSorter = _selectedServiceListView2Sorter;
            _selectedServiceListView2.Sort();
        }

        /// <summary>
        /// お気に入りサービス一覧をソートする。
        /// </summary>
        public void SortFavoriteServiceList(int column)
        {
            _favoriteServiceListViewSorter.SetColumn(column,
                column == ServiceListViewColumns.Tsid || column == ServiceListViewColumns.Sid,
                column == ServiceListViewColumns.NetworkType);
            _favoriteServiceListView.ListViewItemSorter = _favoriteServiceListViewSorter;
            _favoriteServiceListView.Sort();
        }
    }

    public static class ServiceListViewHelper
    {
        /// <summary>
        /// サービスへのチェック処理
        /// 1列目に✔を表示して色を変える。
        /// </summary>
        /// <param name="item">ListViewアイテム</param>
        public static void CheckServiceItem(ListViewItem item)
        {
            item.SubItems[ServiceListViewColumns.Mark].Text = "✔";
            item.BackColor = Color.LightGray;
        }

        /// <summary>
        /// checkServiceItem の効果（✔ と背景色）を解除する。
        /// </summary>
        /// <param name="item">解除対象の ListViewItem</param>
        public static void UncheckServiceItem(ListViewItem item)
        {
            // ✔ を消す
            item.SubItems[ServiceListViewColumns.Mark].Text = "";

            // 背景色を通常に戻す
            if (item.ListView != null)
            {
                item.BackColor = item.ListView.BackColor;
            }
            else
            {
                item.BackColor = SystemColors.Window;
            }
        }

        /// <summary>
        /// 左リストに存在しないサービスの場合、1列目に "A" を付ける。
        /// </summary>
        public static void ApplyManualAddMark(ListViewItem item)
        {
                item.SubItems[ServiceListViewColumns.Mark].Text = "A";
        }

        /// <summary>
        /// NetworkType または ServiceName が左リストの元データと異なる場合、1列目に mod 印を付ける。
        /// </summary>
        public static void ApplyModMarkIfChanged(ListViewItem originalItem, ListViewItem newItem)
        {
            bool isTypeChanged = newItem.SubItems[ServiceListViewColumns.NetworkType].Text != originalItem.SubItems[ServiceListViewColumns.NetworkType].Text;
            bool isNameChanged = newItem.SubItems[ServiceListViewColumns.Name].Text != originalItem.SubItems[ServiceListViewColumns.Name].Text;

            if (isTypeChanged || isNameChanged)
            {
                newItem.SubItems[ServiceListViewColumns.Mark].Text = "M";
            }
        }

        /// <summary>
        /// サービス追加処理
        /// 左リストビューで選択中のサービスを右リストビューに追加し、左リストビューのアイテムにチェックをつけて選択をクリアする。
        /// </summary>
        /// <param name="leftListView">左ListView</param>
        /// <param name="rightListView">右ListView</param>
        public static void AddService(ListView leftListView, ListView rightListView)
        {
            // >>ボタン
            foreach (ListViewItem item in leftListView.SelectedItems)
            {
                string key = item.Name;

                if (!rightListView.Items.ContainsKey(key))
                {
                    ListViewItem targetItem = leftListView.Items[key];

                    ListViewItem newItem = (ListViewItem)targetItem.Clone();
                    newItem.Name = key;

                    // TVTestOption 列まで SubItems を揃える
                    while (newItem.SubItems.Count < rightListView.Columns.Count)
                    {
                        newItem.SubItems.Add("");
                    }

                    rightListView.Items.Add(newItem);

                    CheckServiceItem(targetItem);
                }
            }

            leftListView.SelectedItems.Clear();
        }

        /// <summary>
        /// サービス削除処理
        /// 右リストビューで選択中のサービスを右リストビューから削除し、左リストビューのアイテムのチェックを外して選択をクリアする。
        /// </summary>
        /// <param name="leftListView">左ListView</param>
        /// <param name="rightListView">右ListView</param>
        public static void RemoveService(ListView leftListView, ListView rightListView)
        {
            // <<ボタン
            foreach (ListViewItem item in rightListView.SelectedItems)
            {
                rightListView.Items.Remove(item);

                if (leftListView.Items.ContainsKey(item.Name))
                {
                    // 左リストに同じチャンネルがあれば印を外す。
                    UncheckServiceItem(leftListView.Items[item.Name]);
                }
            }

            // 選択をクリア
            rightListView.SelectedItems.Clear();
        }

        /// <summary>
        /// アイテム上移動処理
        /// 対象ListViewの選択中のItemをひとつ上に移動する。複数箇所の選択に対応する。
        /// </summary>
        /// <param name="listview">対象ListView</param>
        public static void MoveUp(ListView listview)
        {
            // ↑処理
            int selectedEndIndex = -1;

            // 選択されたアイテムを範囲ごとにリスト化
            List<(int, int)> ranges = new List<(int, int)>();

            for (int i = listview.Items.Count - 1; i >= 0; i--)
            {
                if (listview.Items[i].Selected)
                {
                    if (selectedEndIndex < 0)
                    {
                        selectedEndIndex = i;
                    }
                }
                else if (selectedEndIndex >= 0)
                {
                    ranges.Add((i + 1, selectedEndIndex));
                    selectedEndIndex = -1;
                }
            }

            // 末尾処理不要(リストの先頭行を含む選択範囲は移動しない)

            foreach ((int startIndex, int endIndex) in ranges)
            {
                // startindexのひとつ上を削除して、endIndexにinsertする
                ListViewItem tempItem = listview.Items[startIndex - 1];
                listview.Items.Remove(tempItem);
                listview.Items.Insert(endIndex, tempItem);
            }
        }

        /// <summary>
        /// アイテム下移動処理
        /// 対象ListViewの選択中のItemをひとつ下に移動する。複数箇所の選択に対応する。
        /// </summary>
        /// <param name="listview"></param>
        public static void MoveDown(ListView listview)
        {
            // ↓処理
            int selectedStartIndex = -1;

            // 選択されたアイテムを範囲ごとにリスト化
            List<(int, int)> ranges = new List<(int, int)>();

            for (int i = 0; i < listview.Items.Count; i++)
            {
                if (listview.Items[i].Selected)
                {
                    if (selectedStartIndex < 0)
                    {
                        selectedStartIndex = i;
                    }
                }
                else if (selectedStartIndex >= 0)
                {
                    ranges.Add((selectedStartIndex, i - 1));
                    selectedStartIndex = -1;
                }
            }

            // 末尾処理不要(リストの最終行を含む選択範囲は移動しない)

            foreach ((int startIndex, int endIndex) in ranges)
            {
                // 下へ移動
                // startindexのひとつ下を削除して、startIndexにinsertする
                ListViewItem tempItem = listview.Items[endIndex + 1];
                listview.Items.Remove(tempItem);
                listview.Items.Insert(startIndex, tempItem);
            }
        }
    }

    /// <summary>
    /// チューナー名設定のListViewを管理するクラス
    /// </summary>
    public class TunerNameListViewManager
    {
        private readonly ConfigManager _configManager;
        private readonly List<TunerReserveInfo> _tunerReserveInfos;
        private readonly ListView _tunerNameListView;

        private readonly ListViewItemComparer _listViewSorter;
        private const int _bonDriverNameColumnIndex = 2;
        private const int _tunerNameColumnIndex = 3;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TunerNameListViewManager(
            ConfigManager configManager,
            List<TunerReserveInfo> tunerReserveInfos,
            ListView tunerNameListView)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _tunerReserveInfos = tunerReserveInfos ?? throw new ArgumentNullException(nameof(tunerReserveInfos));
            _tunerNameListView = tunerNameListView ?? throw new ArgumentNullException(nameof(tunerNameListView));

            _listViewSorter = new ListViewItemComparer();
        }

        /// <summary>
        /// チューナー名設定の読み込み
        /// </summary>
        public void Load()
        {
            _tunerNameListView.Items.Clear();

            // チューナー名タブのListView作成
            foreach (TunerReserveInfo tunerReserveInfo in _tunerReserveInfos)
            {
                // ListView作成
                if (!_tunerNameListView.Items.ContainsKey(tunerReserveInfo.tunerName))
                {
                    string[] data;
                    if (_configManager.RockbarSetting.BonDriverNameToTunerName.ContainsKey(tunerReserveInfo.tunerName))
                    {
                        data = new[]
                        {
                            "",
                            (tunerReserveInfo.tunerID & 0xffff0000).ToString("x8").Substring(0, 4),
                            tunerReserveInfo.tunerName,
                            _configManager.RockbarSetting.BonDriverNameToTunerName[tunerReserveInfo.tunerName]
                        };
                    }
                    else
                    {
                        data = new[]
                        {
                            "",
                            (tunerReserveInfo.tunerID & 0xffff0000).ToString("x8").Substring(0, 4),
                            tunerReserveInfo.tunerName,
                            RockbarUtility.GetDefaultTunerName(tunerReserveInfo.tunerName)
                        };
                    }

                    ListViewItem item = new ListViewItem(data)
                    {
                        Name = tunerReserveInfo.tunerName
                    };
                    _tunerNameListView.Items.Add(item);
                }
            }

            // 取得したチューナに含まれず、設定にだけあるものを表示
            foreach (var kv in _configManager.RockbarSetting.BonDriverNameToTunerName)
            {
                // ListView上になければ追加しておく
                if (!_tunerNameListView.Items.ContainsKey(kv.Key))
                {
                    string[] data =
                    {
                        "！",
                        "",
                        kv.Key,
                        kv.Value
                    };

                    ListViewItem item = new ListViewItem(data)
                    {
                        Name = kv.Key
                    };
                    _tunerNameListView.Items.Add(item);
                }
            }
        }

        /// <summary>
        /// チューナー名設定の保存
        /// </summary>
        public void Save()
        {
            _configManager.RockbarSetting.BonDriverNameToTunerName = new Dictionary<string, string>();

            foreach (ListViewItem item in _tunerNameListView.Items)
            {
                _configManager.RockbarSetting.BonDriverNameToTunerName.Add(
                    item.SubItems[_bonDriverNameColumnIndex].Text,
                    item.SubItems[_tunerNameColumnIndex].Text
                );
            }
        }

        /// <summary>
        /// 列ヘッダクリック時のソート処理
        /// </summary>
        public void Sort(int columnIndex)
        {
            _listViewSorter.SetColumn(columnIndex);
            _tunerNameListView.ListViewItemSorter = _listViewSorter;
            _tunerNameListView.Sort();
        }

        /// <summary>
        /// 選択されているチューナー名をテキストボックスに反映する
        /// </summary>
        public void DisplaySelectedNameTo(TextBox targetTextBox)
        {
            if (_tunerNameListView.SelectedItems.Count > 0)
            {
                targetTextBox.Text = _tunerNameListView.SelectedItems[0].SubItems[_tunerNameColumnIndex].Text;
            }
        }

        /// <summary>
        /// テキストボックスの値を選択中アイテムのチューナー名に反映する
        /// </summary>
        public void UpdateSelectedNameFrom(TextBox sourceTextBox)
        {
            if (_tunerNameListView.SelectedItems.Count > 0)
            {
                _tunerNameListView.SelectedItems[0].SubItems[_tunerNameColumnIndex].Text = sourceTextBox.Text;
            }
        }
    }

    public static class UiFontHelper
    {
        private static readonly TypeConverter _fontConverter = TypeDescriptor.GetConverter(typeof(Font));

        /// <summary>
        /// 設定値に基づいて、フォームおよび配下コントロールのフォントに一括適用
        /// </summary>
        public static void ApplyUiFontSettings(Control rootControl, RockBarSetting setting)
        {
            if (rootControl == null || setting == null) return;

            try
            {
                Font defaultFont = ParseFont(setting.Font);
                Font menuFont = ParseFont(setting.MenuFont) ?? defaultFont;
                Font tabFont = ParseFont(setting.TabFont) ?? defaultFont;
                Font buttonFont = ParseFont(setting.ButtonFont) ?? defaultFont;
                Font labelFont = ParseFont(setting.LabelFont) ?? defaultFont;
                Font textBoxFont = ParseFont(setting.TextBoxFont) ?? defaultFont;

                // フォーム全体のベースフォントを設定
                if (defaultFont != null)
                {
                    rootControl.Font = defaultFont;
                }

                // メニュー類のフォント設定
                if (rootControl is Form form && form.MainMenuStrip != null && menuFont != null)
                {
                    form.MainMenuStrip.Font = menuFont;
                }

                // コントロールツリーを走査してフォントを上書き適用
                ApplyFontRecursive(rootControl, menuFont, tabFont, buttonFont, labelFont, textBoxFont);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplyUiFontSettings Error: {ex.Message}");
            }
        }

        /// <summary>
        /// コントロールとその子コントロールに対して再帰的にフォントを適用する
        /// </summary>
        private static void ApplyFontRecursive(
            Control control,
            Font menuFont,
            Font tabFont,
            Font buttonFont,
            Font labelFont,
            Font textBoxFont)
        {
            if (control == null) return;

            if (control is ToolStrip toolStrip && menuFont != null)
            {
                toolStrip.Font = menuFont;
            }
            else if (control is TabControl && tabFont != null)
            {
                control.Font = tabFont;
            }
            else if (control is Button && buttonFont != null)
            {
                control.Font = buttonFont;
            }
            else if ((control is Label || control is CheckBox || control is RadioButton ||
                      control is GroupBox || control is ListView) && labelFont != null)
            {
                control.Font = labelFont;
            }
            else if ((control is TextBox || control is ComboBox || control is NumericUpDown ||
                      control is ListBox) && textBoxFont != null)
            {
                control.Font = textBoxFont;
            }

            // 子コントロールを再帰的に走査
            foreach (Control child in control.Controls)
            {
                ApplyFontRecursive(child, menuFont, tabFont, buttonFont, labelFont, textBoxFont);
            }
        }

        /// <summary>
        /// フォント設定文字列を Font オブジェクトに変換
        /// 変換に失敗した場合や空文字列の場合は null を返す
        /// </summary>
        /// <param name="fontString">変換対象のフォント設定文字列</param>
        /// <returns>生成された Font オブジェクト（失敗時は null）</returns>
        private static Font ParseFont(string fontString)
        {
            if (string.IsNullOrWhiteSpace(fontString)) return null;
            try
            {
                return _fontConverter.ConvertFromString(fontString) as Font;
            }
            catch
            {
                return null;
            }
        }
    }
}
