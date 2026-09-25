using EpgTimer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RockbarForEDCB
{
    /// <summary>
    /// 録画フォルダ一覧（recFolderListView）の表示・操作・保存を管理するクラス
    /// </summary>
    public class RecFolderListViewManager
    {
        private readonly ConfigManager _configManager;
        private readonly ListView _recFolderListView;
        private readonly CtrlCmdUtil _ctrlCmdUtil;

        /// <summary>
        /// 録画フォルダ追加・編集用ダイアログクラス
        /// </summary>
        private class RecFolderEditDialog : Form
        {
            private readonly CheckBox _partialCheckBox = new CheckBox();
            private readonly TextBox _recFolderTextBox = new TextBox();
            private readonly ComboBox _writePlugInComboBox = new ComboBox();
            private readonly ComboBox _fileNamePlugInComboBox = new ComboBox();
            private readonly TextBox _fileNamePlugInOptionTextBox = new TextBox();

            /// <summary>
            /// ダイアログ確定後の結果データ（ListViewItem にセットする SubItem 文字列配列）
            /// </summary>
            public string[] ResultSubItems { get; private set; }

            /// <summary>
            /// 録画フォルダ追加・編集用ダイアログクラス
            /// </summary>
            /// <param name="writePlugIns">出力PlugInリスト</param>
            /// <param name="fileNamePlugIns">ファイル名PlugInリスト</param>
            /// <param name="initialItem">編集対象のListViewItem（新規追加時は null）</param>
            /// <param name="rockbarSetting">フォント設定</param>
            public RecFolderEditDialog(
                List<string> writePlugIns,
                List<string> fileNamePlugIns,
                ListViewItem initialItem = null,
                RockbarSetting rockbarSetting = null)
            {
                this.Text = (initialItem != null) ? "録画フォルダの変更" : "録画フォルダ、使用PlugIn設定";
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.StartPosition = FormStartPosition.CenterParent;
                this.MinimizeBox = false;
                this.MaximizeBox = false;
                this.ShowInTaskbar = false;
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.ClientSize = new Size(450, 210);

                _partialCheckBox.Text = "部分受信";
                _partialCheckBox.Location = new Point(16, 12);
                _partialCheckBox.AutoSize = true;

                Label folderLabel = new Label { Left = 16, Top = 40, Width = 110, Text = "録画フォルダ" };
                _recFolderTextBox.Left = 130;
                _recFolderTextBox.Top = 36;
                _recFolderTextBox.Width = 230;

                Button browseButton = new Button { Left = 365, Top = 34, Width = 70, Text = "開く" };
                browseButton.Click += (sender, e) =>
                {
                    using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                    {
                        if (fbd.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                        {
                            _recFolderTextBox.Text = fbd.SelectedPath;
                        }
                    }
                };

                Label writePlugInLabel = new Label { Left = 16, Top = 70, Width = 110, Text = "出力PlugIn" };
                _writePlugInComboBox.Left = 130;
                _writePlugInComboBox.Top = 66;
                _writePlugInComboBox.Width = 230;
                _writePlugInComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

                // 出力PlugIn の選択肢設定
                if (writePlugIns != null && writePlugIns.Count > 0)
                {
                    _writePlugInComboBox.Items.AddRange(writePlugIns.ToArray());
                }
                int defaultWriteIndex = _writePlugInComboBox.FindStringExact("Write_Default.dll");
                _writePlugInComboBox.SelectedIndex = defaultWriteIndex >= 0
                    ? defaultWriteIndex
                    : (_writePlugInComboBox.Items.Count > 0 ? 0 : -1);

                Label fileNamePlugInLabel = new Label { Left = 16, Top = 100, Width = 110, Text = "ファイル名PlugIn" };
                _fileNamePlugInComboBox.Left = 130;
                _fileNamePlugInComboBox.Top = 96;
                _fileNamePlugInComboBox.Width = 230;
                _fileNamePlugInComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

                // ファイル名PlugIn の選択肢設定（未指定 "" を選択可能にする）
                _fileNamePlugInComboBox.Items.Add("");
                if (fileNamePlugIns != null && fileNamePlugIns.Count > 0)
                {
                    _fileNamePlugInComboBox.Items.AddRange(fileNamePlugIns.ToArray());
                }
                _fileNamePlugInComboBox.SelectedIndex = 0; // デフォルトは未指定 ("")

                Label fileNameOptionLabel = new Label { Left = 16, Top = 130, Width = 130, Text = "ファイル名PlugInオプション" };
                _fileNamePlugInOptionTextBox.Left = 150;
                _fileNamePlugInOptionTextBox.Top = 126;
                _fileNamePlugInOptionTextBox.Width = 210;

                Button okButton = new Button 
                        { Left = 235, Top = 170, Width = 80, Text = "OK", DialogResult = DialogResult.OK };
                Button cancelButton = new Button
                        { Left = 330, Top = 170, Width = 80, Text = "キャンセル", DialogResult = DialogResult.Cancel };

                okButton.Click += OkButton_Click;

                this.Controls.AddRange(new Control[] {
                    _partialCheckBox, folderLabel, _recFolderTextBox, browseButton,
                    writePlugInLabel, _writePlugInComboBox,
                    fileNamePlugInLabel, _fileNamePlugInComboBox,
                    fileNameOptionLabel, _fileNamePlugInOptionTextBox,
                    okButton, cancelButton
                });

                this.AcceptButton = okButton;
                this.CancelButton = cancelButton;

                // フォントの適用
                TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));
                Font settingFormFont = (Font)fontConverter.ConvertFromString(rockbarSetting.SettingFormFont);
                this.Font = settingFormFont;

                // 編集モード：ListViewItem から値を展開
                if (initialItem != null)
                {
                    _partialCheckBox.Checked = initialItem.SubItems[0].Text == "はい";
                    _recFolderTextBox.Text = initialItem.SubItems[1].Text;

                    string writePlugIn = initialItem.SubItems[2].Text;
                    int index = _writePlugInComboBox.FindStringExact(writePlugIn ?? "");
                    if (index >= 0)
                    {
                        _writePlugInComboBox.SelectedIndex = index;
                    }

                    // ファイル名PlugInは "?" でオプションと分離
                    string combinedFileName = initialItem.SubItems[3].Text;
                    string[] nameParts = combinedFileName.Split(new[] { '?' }, 2);
                    index = _fileNamePlugInComboBox.FindStringExact(nameParts[0] ?? "");
                    if (index >= 0)
                    {
                        _fileNamePlugInComboBox.SelectedIndex = index;
                    }
                    _fileNamePlugInOptionTextBox.Text = nameParts.Length > 1 ? nameParts[1] : "";
                }
            }

            /// <summary>
            /// OKボタンクリック時の処理。入力値を結果用配列にまとめる。
            /// </summary>
            /// <param name="sender">イベント発生元オブジェクト</param>
            /// <param name="e">イベント引数</param>
            private void OkButton_Click(object sender, EventArgs e)
            {
                string fileNamePlugIn = _fileNamePlugInComboBox.Text.Trim();
                string fileNameOption = _fileNamePlugInOptionTextBox.Text.Trim();

                string fileNamePlugInCombined;
                if (string.IsNullOrWhiteSpace(fileNamePlugIn))
                {
                    fileNamePlugInCombined = "";
                }
                else if (string.IsNullOrWhiteSpace(fileNameOption))
                {
                    fileNamePlugInCombined = fileNamePlugIn;
                }
                else
                {
                    fileNamePlugInCombined = $"{fileNamePlugIn}?{fileNameOption}";
                }

                ResultSubItems = new string[] {
                    _partialCheckBox.Checked ? "はい" : "いいえ",
                    _recFolderTextBox.Text.Trim(),
                    _writePlugInComboBox.Text.Trim(),
                    fileNamePlugInCombined
                };
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RecFolderListViewManager(
            ConfigManager configManager,
            ListView recFolderListView,
            CtrlCmdUtil ctrlCmdUtil)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _recFolderListView = recFolderListView ?? throw new ArgumentNullException(nameof(recFolderListView));
            _ctrlCmdUtil = ctrlCmdUtil;
        }

        /// <summary>
        /// 設定から録画フォルダ一覧を読み込んでListViewに表示する
        /// </summary>
        public void Load()
        {
            _recFolderListView.Items.Clear();

            // 通常の録画フォルダ（部分受信：いいえ）の読み込み
            if (_configManager.RockbarSetting.RecFolderList != null)
            {
                foreach (RecFileSetInfo info in _configManager.RockbarSetting.RecFolderList)
                {
                    if (info == null) continue;

                    var item = new ListViewItem(new string[] {
                        "いいえ",
                        info.RecFolder ?? "",
                        info.WritePlugIn ?? "",
                        info.RecNamePlugIn ?? ""
                    });
                    _recFolderListView.Items.Add(item);
                }
            }

            // 部分受信フォルダ（部分受信：はい）の読み込み
            if (_configManager.RockbarSetting.PartialRecFolderList != null)
            {
                foreach (RecFileSetInfo info in _configManager.RockbarSetting.PartialRecFolderList)
                {
                    if (info == null) continue;

                    var item = new ListViewItem(new string[] {
                        "はい",
                        info.RecFolder ?? "",
                        info.WritePlugIn ?? "",
                        info.RecNamePlugIn ?? ""
                    });
                    _recFolderListView.Items.Add(item);
                }
            }
        }

        /// <summary>
        /// ListViewの内容を設定に保存する
        /// </summary>
        public void Save()
        {
            var recFolderList = new List<RecFileSetInfo>();
            var partialRecFolderList = new List<RecFileSetInfo>();

            foreach (ListViewItem item in _recFolderListView.Items)
            {
                bool isPartial = item.SubItems.Count > 0 && item.SubItems[0].Text == "はい";
                string folderPath = item.SubItems.Count > 1 ? item.SubItems[1].Text : "";
                string writePlugIn = item.SubItems.Count > 2 ? item.SubItems[2].Text : "";
                string recNamePlugIn = item.SubItems.Count > 3 ? item.SubItems[3].Text : "";

                var info = new RecFileSetInfo
                {
                    RecFolder = folderPath,
                    WritePlugIn = writePlugIn,
                    RecNamePlugIn = recNamePlugIn,
                    RecFileName = "" // 予約情報としては空文字列で保持
                };

                if (isPartial)
                {
                    partialRecFolderList.Add(info);
                }
                else
                {
                    recFolderList.Add(info);
                }
            }

            _configManager.RockbarSetting.RecFolderList = recFolderList;
            _configManager.RockbarSetting.PartialRecFolderList = partialRecFolderList;
        }

        /// <summary>
        /// 録画フォルダ追加ダイアログを表示
        /// </summary>
        public void AddFolder()
        {
            EditFolderWithDialog(null);
        }

        /// <summary>
        /// 選択されている録画フォルダ編集ダイアログを表示
        /// </summary>
        public void EditFolder()
        {
            if (_recFolderListView.SelectedItems.Count > 0)
            {
                EditFolderWithDialog(_recFolderListView.SelectedItems[0]);
            }
        }

        /// <summary>
        /// 録画フォルダの「新規追加」と「編集」処理。
        /// targetItem == null → 新規追加モード
        /// targetItem != null → 編集モード
        /// </summary>
        private void EditFolderWithDialog(ListViewItem targetItem)
        {
            // PlugIn 一覧の取得
            var writePlugIns = new List<string>();
            var fileNamePlugIns = new List<string>();
            if (_ctrlCmdUtil != null)
            {
                _ctrlCmdUtil.SendEnumPlugIn(2, ref writePlugIns);
                _ctrlCmdUtil.SendEnumPlugIn(1, ref fileNamePlugIns);
            }

            // 編集時は対象アイテムからデータを取り出す（新規の場合は初期値）
            bool isPartial = targetItem?.SubItems[0].Text == "はい";
            string folderPath = targetItem?.SubItems.Count > 1 ? targetItem.SubItems[1].Text : "";
            string writePlugIn = targetItem?.SubItems.Count > 2 ? targetItem.SubItems[2].Text : "";

            string combinedFileName = targetItem?.SubItems.Count > 3 ? targetItem.SubItems[3].Text : "";
            string[] nameParts = combinedFileName.Split(new[] { '?' }, 2);
            string fileNamePlugIn = nameParts.Length > 0 ? nameParts[0] : "";
            string fileNamePlugInOption = nameParts.Length > 1 ? nameParts[1] : "";

            using (var dialog = new RecFolderEditDialog(writePlugIns, fileNamePlugIns, targetItem, _configManager.RockbarSetting))
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;

                string[] subItemTexts = dialog.ResultSubItems;

                if (targetItem == null)
                {
                    // 新規追加
                    _recFolderListView.Items.Add(new ListViewItem(subItemTexts));
                }
                else
                {
                    // 既存更新
                    for (int i = 0; i < subItemTexts.Length; i++)
                    {
                        if (i < targetItem.SubItems.Count)
                            targetItem.SubItems[i].Text = subItemTexts[i];
                        else
                            targetItem.SubItems.Add(subItemTexts[i]);
                    }
                }
            }
        }

        /// <summary>
        /// 選択されている録画フォルダを複製
        /// </summary>
        public void CopySelectedFolder()
        {
            // リストで項目が選択されていない場合は何もしない
            if (_recFolderListView.SelectedItems.Count == 0) return;

            // 選択中の行のデータを取得
            ListViewItem selectedItem = _recFolderListView.SelectedItems[0];

            // 取得したデータで新しい ListViewItem を複製してリストに追加
            ListViewItem newItem = (ListViewItem)selectedItem.Clone();
            _recFolderListView.Items.Add(newItem);

            // コピーされた新しい項目を選択状態にする
            newItem.Selected = true;
            _recFolderListView.EnsureVisible(newItem.Index);
        }

        /// <summary>
        /// 選択されている録画フォルダを削除
        /// </summary>
        public void DeleteSelectedFolder()
        {
            // リストで項目が選択されていない場合は何もしない
            if (_recFolderListView.SelectedItems.Count == 0) return;

            // 選択中の行を削除
            ListViewItem selectedItem = _recFolderListView.SelectedItems[0];
            _recFolderListView.Items.Remove(selectedItem);
        }
    }

    /// <summary>
    /// チューナー選択ComboBoxの制御マネージャークラス
    /// </summary>
    public class TunerComboBoxManager
    {
        private readonly ConfigManager _configManager;
        private readonly List<TunerReserveInfo> _tunerReserveInfos;
        private readonly ComboBox _comboBox;

        /// <summary>
        /// チューナーコンボボックスに表示する要素項目を表す内部クラス
        /// </summary>
        private class Item
        {
            /// <summary>
            /// コンボボックスに表示するテキストを取得します。
            /// </summary>
            public string DisplayText { get; }

            /// <summary>
            /// チューナーを識別するIDを取得します。
            /// </summary>
            public uint TunerID { get; }

            /// <summary>
            /// <see cref="Item"/> クラスの新しいインスタンスを初期化します。
            /// </summary>
            /// <param name="displayText">表示用テキスト</param>
            /// <param name="tunerID">チューナーID</param>
            public Item(string displayText, uint tunerID)
            {
                DisplayText = displayText;
                TunerID = tunerID;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TunerComboBoxManager(ConfigManager configManager, List<TunerReserveInfo> tunerReserveInfos, ComboBox comboBox)
        {
            _configManager = configManager ?? throw new ArgumentNullException(nameof(configManager));
            _tunerReserveInfos = tunerReserveInfos ?? new List<TunerReserveInfo>();
            _comboBox = comboBox ?? throw new ArgumentNullException(nameof(comboBox));

            _comboBox.DisplayMember = "DisplayText";
            _comboBox.ValueMember = "TunerID";
        }

        /// <summary>
        /// 画面読み込み時の処理（リスト構築と選択状態の設定）
        /// </summary>
        public void Load()
        {
            _comboBox.Items.Clear();

            // 先頭に「自動」（TunerID = 0）を追加
            _comboBox.Items.Add(new Item("自動", 0));

            // チューナー一覧の追加
            foreach (var info in _tunerReserveInfos)
            {
                // 「チューナー不足 (0xFFFFFFFF)」は ComboBox の選択肢から除外
                if (info.tunerID != 0xFFFFFFFF)
                {
                    // 表示名: ID:00000001 (BonDriver_Proxy_T.dll)
                    string displayText = $"ID:{info.tunerID:x8} ({info.tunerName})";
                    _comboBox.Items.Add(new Item(displayText, info.tunerID));
                }
            }

            // 初期選択肢の設定
            uint targetTunerId = _configManager.RockbarSetting.RecTunerID;

            // recTunerIdComboBox の要素から、TunerID が一致するアイテムを検索
            var matchingItem = _comboBox.Items
                .OfType<Item>()
                .FirstOrDefault(item => item.TunerID == targetTunerId);

            if (matchingItem != null)
            {
                // 一致する項目があればそれを選択
                _comboBox.SelectedItem = matchingItem;
            }
            else if (_comboBox.Items.Count > 0)
            {
                // 一致するものがない場合（初期状態など）は先頭（自動）を設定
                _comboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 設定保存処理
        /// </summary>
        public void Save()
        {
            uint selectedTunerId = (_comboBox.SelectedItem as Item)?.TunerID ?? 0;
            _configManager.RockbarSetting.RecTunerID = selectedTunerId;
        }
    }

    /// <summary>
    /// 予約設定用ヘルパー関数クラス
    /// </summary>
    public static class SettingFormReservationHelper
    {
        /// <summary>
        /// バッチ/スクリプト選択ダイアログを表示し、選択結果をテキストボックスにセットする
        /// </summary>
        public static void BrowseScriptFile(TextBox targetTextBox)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "実行可能スクリプト (*.bat;*.ps1;*.lua)|*.bat;*.ps1;*.lua" +
                             "|Batchファイル (*.bat)|*.bat" +
                             "|PowerShellスクリプト (*.ps1)|*.ps1" +
                             "|Luaスクリプト (*.lua)|*.lua" +
                             "|すべてのファイル (*.*)|*.*";

                ofd.FilterIndex = 1; // デフォルトで最初のフィルタを選択
                ofd.Title = "バッチ/スクリプトファイルの選択";

                // すでにテキストボックスにパスが入力されていれば、そのフォルダを初期表示に設定
                string currentPath = targetTextBox.Text;
                if (!string.IsNullOrWhiteSpace(currentPath) && File.Exists(currentPath))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(currentPath);
                    ofd.FileName = Path.GetFileName(currentPath);
                }

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    targetTextBox.Text = ofd.FileName;
                }
            }
        }
    }
}
