using EpgTimer;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private int columnIndex = -1;
        private SortOrder sortOrder = SortOrder.Ascending;
        private bool isNumber = false;

        /// <summary>
        /// 列index更新処理
        /// </summary>
        /// <param name="columnIndex">列index</param>
        public void setColumn(int columnIndex, bool isNumber = false)
        {
            this.isNumber = isNumber;

            // 同じカラムがクリックされた場合はソート方向を反転
            if (this.columnIndex == columnIndex)
            {
                if (this.sortOrder == SortOrder.Ascending)
                {
                    this.sortOrder = SortOrder.Descending;
                }
                else
                {
                    this.sortOrder = SortOrder.Ascending;
                }
            }
            else
            {
                this.sortOrder = SortOrder.Ascending;
            }

            this.columnIndex = columnIndex;
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
            if (columnIndex < 0)
            {
                return 0;
            }

            ListViewItem item1 = (ListViewItem)x1;
            ListViewItem item2 = (ListViewItem)x2;

            // 1回文字列比較
            int result = string.Compare(item1.SubItems[columnIndex].Text, item2.SubItems[columnIndex].Text);

            // 数値の場合の比較
            if (this.isNumber)
            {
                try
                {
                    result = int.Parse(item1.SubItems[columnIndex].Text) - int.Parse(item2.SubItems[columnIndex].Text);
                }
                catch
                {
                    // 数値変換で比較が失敗した場合は文字列比較の結果を残す
                }
            }

            if (this.sortOrder == SortOrder.Descending)
            {
                result *= -1;
            }

            return result;
        }
    }

    /// <summary>
    /// サービス新規追加編集フォームクラス
    /// </summary>
    public class ServiceEditDialog : Form
    {
        private readonly TextBox tsidTextBox = new TextBox();
        private readonly TextBox sidTextBox = new TextBox();
        private readonly TextBox nameTextBox = new TextBox();
        private readonly ComboBox typeComboBox = new ComboBox();
        private readonly TextBox tvtestOptionTextBox = new TextBox();
        private readonly List<EpgServiceInfo> serviceInfos;
        private readonly HashSet<string> existingKeys;

        public Service ResultService { get; private set; }

        /// <summary>
        /// サービス新規追加編集
        /// </summary>
        /// <param name="initialService">入力済みサービス</param>
        /// <param name="epgServiceInfos">サービス一覧</param>
        public ServiceEditDialog(Service initialService, List<EpgServiceInfo> epgServiceInfos, HashSet<string> existingKeys)
        {
            this.Text = (initialService != null) ? "チャンネル編集" : "チャンネル追加";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ShowInTaskbar = false;
            this.ClientSize = new Size(360, 210);
            this.serviceInfos = epgServiceInfos;
            this.existingKeys = existingKeys;

            Label tsidLabel = new Label { Left = 16, Top = 18, Width = 100, Text = "TSID" };
            tsidTextBox.Left = 120;
            tsidTextBox.Top = 14;
            tsidTextBox.Width = 200;

            Label sidLabel = new Label { Left = 16, Top = 48, Width = 100, Text = "SID" };
            sidTextBox.Left = 120;
            sidTextBox.Top = 44;
            sidTextBox.Width = 200;

            Label nameLabel = new Label { Left = 16, Top = 78, Width = 100, Text = "名前" };
            nameTextBox.Left = 120;
            nameTextBox.Top = 74;
            nameTextBox.Width = 200;

            Label typeLabel = new Label { Left = 16, Top = 108, Width = 100, Text = "Type" };
            typeComboBox.Left = 120;
            typeComboBox.Top = 104;
            typeComboBox.Width = 200;
            typeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            typeComboBox.Items.AddRange(new object[] { "自動判別", "地", "BS", "CS", "CATV", "SPHD", "BS4K" });

            Label tvtestOptionLabel = new Label { Left = 16, Top = 138, Width = 100, Text = "TVTestオプション" };
            tvtestOptionTextBox.Left = 120;
            tvtestOptionTextBox.Top = 134;
            tvtestOptionTextBox.Width = 200;

            Button okButton = new Button { Left = 164, Top = 172, Width = 75, Text = "OK", DialogResult = DialogResult.OK };
            Button cancelButton = new Button { Left = 245, Top = 172, Width = 75, Text = "キャンセル", DialogResult = DialogResult.Cancel };

            okButton.Click += okButton_Click;

            this.Controls.AddRange(new Control[] {
                    tsidLabel, tsidTextBox,
                    sidLabel, sidTextBox,
                    nameLabel, nameTextBox,
                    typeLabel, typeComboBox,
                    tvtestOptionLabel, tvtestOptionTextBox,
                    okButton, cancelButton
                });

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;

            if (initialService != null)
            {
                tsidTextBox.Text = initialService.Tsid;
                sidTextBox.Text = initialService.Sid;
                nameTextBox.Text = initialService.Name;
                typeComboBox.SelectedItem = RockbarUtility.GetShortNetworkTypeName(RockbarUtility.GetNetworkType(initialService.TypeName, null));
                tvtestOptionTextBox.Text = initialService.TvtestOption;
            }
            else
            {
                typeComboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// サービス新規追加編集フォームOKボタン
        /// </summary>
        private void okButton_Click(object sender, EventArgs e)
        {
            ushort tsid;
            ushort sid;

            if (!ushort.TryParse(tsidTextBox.Text, out tsid) || !ushort.TryParse(sidTextBox.Text, out sid))
            {
                MessageBox.Show("TSID と SID は有効数値で入力してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }

            string key = RockbarUtility.GetKey(tsid.ToString(), sid.ToString());

            // 重複チェック（追加・編集共通）
            if (existingKeys.Contains(key))
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
            if (serviceInfos != null)
            {
                matchedService = serviceInfos
                    .FirstOrDefault(x => x.TSID == tsid && x.SID == sid);
            }

            // 名前補完
            // 未指定なら全チャンネル側の名前を使用、なければTSID-SID
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                if (matchedService != null)
                {
                    nameTextBox.Text = matchedService.service_name;
                }
                else
                {
                    nameTextBox.Text = RockbarUtility.GetKey(tsid, sid);
                }
            }

            // 種別補正
            // 種別指定が正しいか判定。誤りがあれば全チャンネル側の情報を使用
            if (matchedService != null)
            {
                var stype = RockbarUtility.GetNetworkType(typeComboBox.Text.Trim(), matchedService.ONID);
                typeComboBox.Text = RockbarUtility.GetShortNetworkTypeName(stype);
            }
            else
            {
                var stype = RockbarUtility.GetNetworkType(typeComboBox.Text.Trim(), null);
                typeComboBox.Text = RockbarUtility.GetShortNetworkTypeName(stype);
            }

            ResultService = new Service
            {
                Tsid = tsid.ToString(),
                Sid = sid.ToString(),
                Name = nameTextBox.Text.Trim(),
                TypeName = typeComboBox.Text.Trim(),
                TvtestOption = string.IsNullOrWhiteSpace(tvtestOptionTextBox.Text) ? null : tvtestOptionTextBox.Text.Trim()
            };
        }
    }

    public static class ServiceListViewHelper
    {
        static int markColumnIndex = 0;
        static int networkTypeColumnIndex = 1;
        static int nameColumnIndex = 2;
        static int tsidColumnIndex = 3;
        static int sidColumnIndex = 4;
        static int tvtestOptionColumnIndex = 5;

        /// <summary>
        /// サービスへのチェック処理
        /// 1列目に✔を表示して色を変える。
        /// </summary>
        /// <param name="item">ListViewアイテム</param>
        public static void CheckServiceItem(ListViewItem item)
        {
            item.SubItems[markColumnIndex].Text = "✔";
            item.BackColor = Color.LightGray;
        }

        /// <summary>
        /// checkServiceItem の効果（✔ と背景色）を解除する。
        /// </summary>
        /// <param name="item">解除対象の ListViewItem</param>
        public static void UncheckServiceItem(ListViewItem item)
        {
            // ✔ を消す
            item.SubItems[markColumnIndex].Text = "";

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
                item.SubItems[markColumnIndex].Text = "A";
        }

        /// <summary>
        /// NetworkType または ServiceName が左リストの元データと異なる場合、1列目に mod 印を付ける。
        /// </summary>
        public static void ApplyModMarkIfChanged(ListViewItem originalItem, ListViewItem newItem)
        {
            bool isTypeChanged = newItem.SubItems[networkTypeColumnIndex].Text != originalItem.SubItems[networkTypeColumnIndex].Text;
            bool isNameChanged = newItem.SubItems[nameColumnIndex].Text != originalItem.SubItems[nameColumnIndex].Text;

            if (isTypeChanged || isNameChanged)
            {
                newItem.SubItems[markColumnIndex].Text = "M";
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

        /// <summary>
        /// ListViewItem に表示されている各列（TSID / SID / 名前(チャンネル名) / 種別 / TVTestオプション）
        /// の値をそのまま Service オブジェクトとして組み立てて返す。
        /// </summary>
        public static Service GetServiceFromListViewItem(ListViewItem item)
        {
            return new Service
            {
                Tsid = item.SubItems[tsidColumnIndex].Text,
                Sid = item.SubItems[sidColumnIndex].Text,
                Name = item.SubItems[nameColumnIndex].Text,
                TypeName = item.SubItems[networkTypeColumnIndex].Text,
                TvtestOption = item.SubItems[tvtestOptionColumnIndex].Text
            };
        }

        /// <summary>
        /// Service オブジェクトから ListViewItem を生成する。
        /// </summary>
        /// <param name="service">表示対象のサービス情報</param>
        /// <returns>生成された ListViewItem</returns>
        public static ListViewItem CreateServiceListItem(Service service)
        {
            string type = RockbarUtility.GetShortNetworkTypeName(RockbarUtility.GetNetworkType(service.TypeName, null));

            ListViewItem item = new ListViewItem(new string[] {
                "",
                type,
                service.Name ?? "",
                service.Tsid ?? "",
                service.Sid ?? "",
                service.TvtestOption ?? ""
            });
            item.Name = RockbarUtility.GetKey(service.Tsid, service.Sid);
            return item;
        }

        /// <summary>
        /// 選択サービスの「新規追加」と「編集」処理。
        /// item == null → 新規追加モード
        /// item != null → 編集モード
        /// </summary>
        /// <param name="listView">編集対象の ListView</param>
        /// <param name="item">編集する ListViewItem</param>
        public static void EditServiceItem(List<EpgServiceInfo> serviceInfos, ListView allServiceListView, ListView listView, ListViewItem item)
        {
            // 既存キー一覧（編集モードなら自分自身を除外）
            var existingKeys = listView.Items.Cast<ListViewItem>()
                .Where(x => x != item)
                .Select(x => x.Name)
                .ToHashSet();

            // 初期値（編集モードなら既存サービス、新規追加モードなら null）
            Service initialService = item != null ? GetServiceFromListViewItem(item) : null;

            using (ServiceEditDialog dialog = new ServiceEditDialog(initialService, serviceInfos, existingKeys))
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                Service service = dialog.ResultService;
                string newKey = RockbarUtility.GetKey(service.Tsid, service.Sid);

                ListViewItem newItem = CreateServiceListItem(service);

                ListViewItem originalItem = allServiceListView.Items[newKey];
                if (originalItem != null)
                {
                    // Type または Name が左リストの元データと異なる場合、1列目に mod 印を付ける。
                    ApplyModMarkIfChanged(originalItem, newItem);
                }
                else
                {
                    // 左リストにないため1列目に add 印を付ける。
                    ApplyManualAddMark(newItem);
                }

                // 新規追加モード
                if (item == null)
                {
                    listView.Items.Add(newItem);
                }

                // 編集モード
                else
                {
                    int index = item.Index;
                    listView.Items.Remove(item);
                    listView.Items.Insert(index, newItem);
                    newItem.Selected = true;

                    // 編集前のチャンネルが左リストにあれば印を外す。
                    if (allServiceListView.Items.ContainsKey(item.Name))
                    {
                        UncheckServiceItem(allServiceListView.Items[item.Name]);
                    }
                }

                // 編集後のチャンネルが左リストにあれば印を付ける。
                if (allServiceListView.Items.ContainsKey(newKey))
                {
                    CheckServiceItem(allServiceListView.Items[newKey]);
                }
            }
        }

        /// <summary>
        /// 選択サービスをリフレッシュしお気に入りサービスキー情報以外を更新する。
        /// </summary>
        public static void RefreshFavoriteService(ListView selectedListView, ListView selectedListView2, ListView favoriteListView)
        {
            // 選択サービスを選択サービスタブからコピーし直す
            selectedListView2.Items.Clear();

            foreach (ListViewItem item in selectedListView.Items)
            {
                ListViewItem copiedItem = (ListViewItem)item.Clone();
                copiedItem.Name = item.Name;
                selectedListView2.Items.Add(copiedItem);
            }

            // 設定ファイルの選択サービス一覧の表示
            List<ListViewItem> needCheckItems = new List<ListViewItem>();

            foreach (ListViewItem favoriteItem in favoriteListView.Items)
            {
                string key = favoriteItem.Name;

                if (selectedListView2.Items.ContainsKey(key))
                {
                    // 登録済みはチェック表示
                    ListViewItem service2Item = selectedListView2.Items[key];

                    favoriteItem.SubItems[markColumnIndex].Text = service2Item.SubItems[markColumnIndex].Text;
                    favoriteItem.SubItems[networkTypeColumnIndex].Text = service2Item.SubItems[networkTypeColumnIndex].Text;
                    favoriteItem.SubItems[nameColumnIndex].Text = service2Item.SubItems[nameColumnIndex].Text;
                    favoriteItem.SubItems[tvtestOptionColumnIndex].Text = service2Item.SubItems[tvtestOptionColumnIndex].Text;

                    needCheckItems.Add(service2Item);
                }
                else
                {
                    favoriteItem.SubItems[markColumnIndex].Text = "！";
                }
            }

            // チェックする
            needCheckItems.ForEach(x => CheckServiceItem(x));
        }
    }
}
