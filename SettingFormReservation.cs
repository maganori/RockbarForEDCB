using EpgTimer;
using System;
using System.Collections.Generic;
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
            private readonly CheckBox partialCheckBox = new CheckBox();
            private readonly TextBox recFolderTextBox = new TextBox();
            private readonly ComboBox writePlugInComboBox = new ComboBox();
            private readonly ComboBox fileNamePlugInComboBox = new ComboBox();
            private readonly TextBox fileNamePlugInOptionTextBox = new TextBox();

            public bool IsPartialRec => partialCheckBox.Checked;
            public string RecFolderPath => recFolderTextBox.Text.Trim();
            public string WritePlugIn => writePlugInComboBox.Text.Trim();
            public string FileNamePlugIn => fileNamePlugInComboBox.Text.Trim();
            public string FileNamePlugInOption => fileNamePlugInOptionTextBox.Text.Trim();

            /// <summary>
            /// コンストラクタ（プラグイン一覧を受け取る）
            /// </summary>
            public RecFolderEditDialog(List<string> writePlugIns, List<string> fileNamePlugIns)
            {
                this.Text = "録画フォルダ、使用PlugIn設定";
                this.FormBorderStyle = FormBorderStyle.FixedDialog;
                this.StartPosition = FormStartPosition.CenterParent;
                this.MinimizeBox = false;
                this.MaximizeBox = false;
                this.ShowInTaskbar = false;
                this.ClientSize = new Size(450, 210);

                partialCheckBox.Text = "部分受信";
                partialCheckBox.Location = new Point(16, 12);
                partialCheckBox.AutoSize = true;

                Label folderLabel = new Label { Left = 16, Top = 40, Width = 110, Text = "録画フォルダ" };
                recFolderTextBox.Left = 130;
                recFolderTextBox.Top = 36;
                recFolderTextBox.Width = 230;

                Button browseButton = new Button { Left = 365, Top = 34, Width = 70, Text = "開く" };
                browseButton.Click += (s, e) =>
                {
                    using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                    {
                        if (fbd.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                        {
                            recFolderTextBox.Text = fbd.SelectedPath;
                        }
                    }
                };

                Label writePlugInLabel = new Label { Left = 16, Top = 70, Width = 110, Text = "出力PlugIn" };
                writePlugInComboBox.Left = 130;
                writePlugInComboBox.Top = 66;
                writePlugInComboBox.Width = 230;
                writePlugInComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

                // 出力PlugIn の選択肢設定
                if (writePlugIns != null && writePlugIns.Count > 0)
                {
                    writePlugInComboBox.Items.AddRange(writePlugIns.ToArray());
                }
                int defaultWriteIndex = writePlugInComboBox.FindStringExact("Write_Default.dll");
                writePlugInComboBox.SelectedIndex = defaultWriteIndex >= 0 ? defaultWriteIndex : (writePlugInComboBox.Items.Count > 0 ? 0 : -1);

                Label fileNamePlugInLabel = new Label { Left = 16, Top = 100, Width = 110, Text = "ファイル名PlugIn" };
                fileNamePlugInComboBox.Left = 130;
                fileNamePlugInComboBox.Top = 96;
                fileNamePlugInComboBox.Width = 230;
                fileNamePlugInComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

                // ファイル名PlugIn の選択肢設定（未指定 "" を選択可能にする）
                fileNamePlugInComboBox.Items.Add("");
                if (fileNamePlugIns != null && fileNamePlugIns.Count > 0)
                {
                    fileNamePlugInComboBox.Items.AddRange(fileNamePlugIns.ToArray());
                }
                fileNamePlugInComboBox.SelectedIndex = 0; // デフォルトは未指定 ("")

                Label fileNameOptionLabel = new Label { Left = 16, Top = 130, Width = 130, Text = "ファイル名PlugInオプション" };
                fileNamePlugInOptionTextBox.Left = 150;
                fileNamePlugInOptionTextBox.Top = 126;
                fileNamePlugInOptionTextBox.Width = 210;

                Button okButton = new Button { Left = 235, Top = 170, Width = 80, Text = "OK", DialogResult = DialogResult.OK };
                Button cancelButton = new Button { Left = 330, Top = 170, Width = 80, Text = "キャンセル", DialogResult = DialogResult.Cancel };

                this.Controls.AddRange(new Control[] {
                    partialCheckBox, folderLabel, recFolderTextBox, browseButton,
                    writePlugInLabel, writePlugInComboBox,
                    fileNamePlugInLabel, fileNamePlugInComboBox,
                    fileNameOptionLabel, fileNamePlugInOptionTextBox,
                    okButton, cancelButton
                });

                this.AcceptButton = okButton;
                this.CancelButton = cancelButton;
            }

            /// <summary>
            /// 編集用コンストラクタ（既存の設定値を初期値としてセット）
            /// </summary>
            public RecFolderEditDialog(List<string> writePlugIns, List<string> fileNamePlugIns, bool isPartial, string folderPath, string writePlugIn, string fileNamePlugIn, string fileNamePlugInOption)
                : this(writePlugIns, fileNamePlugIns)
            {
                this.Text = "録画フォルダの変更";
                this.partialCheckBox.Checked = isPartial;
                this.recFolderTextBox.Text = folderPath;

                // 出力PlugIn の選択肢合わせ
                if (!string.IsNullOrEmpty(writePlugIn))
                {
                    int writeIndex = this.writePlugInComboBox.FindStringExact(writePlugIn);
                    if (writeIndex >= 0)
                    {
                        this.writePlugInComboBox.SelectedIndex = writeIndex;
                    }
                    else
                    {
                        this.writePlugInComboBox.Items.Add(writePlugIn);
                        this.writePlugInComboBox.SelectedItem = writePlugIn;
                    }
                }
                else
                {
                    int defaultWriteIndex = this.writePlugInComboBox.FindStringExact("Write_Default.dll");
                    this.writePlugInComboBox.SelectedIndex = defaultWriteIndex >= 0 ? defaultWriteIndex : (this.writePlugInComboBox.Items.Count > 0 ? 0 : -1);
                }

                // ファイル名PlugIn の選択肢合わせ
                int fileNameIndex = this.fileNamePlugInComboBox.FindStringExact(fileNamePlugIn ?? "");
                if (fileNameIndex >= 0)
                {
                    this.fileNamePlugInComboBox.SelectedIndex = fileNameIndex;
                }
                else
                {
                    this.fileNamePlugInComboBox.Items.Add(fileNamePlugIn);
                    this.fileNamePlugInComboBox.SelectedItem = fileNamePlugIn;
                }

                this.fileNamePlugInOptionTextBox.Text = fileNamePlugInOption;
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
            ShowEditDialog(null);
        }

        /// <summary>
        /// 選択されている録画フォルダ編集ダイアログを表示
        /// </summary>
        public void EditSelectedFolder()
        {
            if (_recFolderListView.SelectedItems.Count > 0)
            {
                ShowEditDialog(_recFolderListView.SelectedItems[0]);
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

        /// <summary>
        /// 編集ダイアログ表示の内部処理
        /// </summary>
        private void ShowEditDialog(ListViewItem targetItem)
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

            using (var dialog = targetItem == null
                ? new RecFolderEditDialog(writePlugIns, fileNamePlugIns)
                : new RecFolderEditDialog(writePlugIns, fileNamePlugIns, isPartial, folderPath, writePlugIn, fileNamePlugIn, fileNamePlugInOption))
            {
                if (dialog.ShowDialog() != DialogResult.OK) return;

                // FileNamePlugIn や FileNamePlugInOption の有無で結合方法を制御
                string fileNamePlugInCombined;
                if (string.IsNullOrWhiteSpace(dialog.FileNamePlugIn))
                {
                    fileNamePlugInCombined = "";
                }
                else if (string.IsNullOrWhiteSpace(dialog.FileNamePlugInOption))
                {
                    fileNamePlugInCombined = dialog.FileNamePlugIn;
                }
                else
                {
                    fileNamePlugInCombined = $"{dialog.FileNamePlugIn}?{dialog.FileNamePlugInOption}";
                }

                string[] subItemTexts = {
                    dialog.IsPartialRec ? "はい" : "いいえ",
                    dialog.RecFolderPath,
                    dialog.WritePlugIn,
                    fileNamePlugInCombined
                };

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
    }

    /// <summary>
    /// チューナー選択ComboBoxの制御マネージャークラス
    /// </summary>
    public class TunerComboBoxManager
    {
        private readonly ConfigManager _configManager;
        private readonly List<TunerReserveInfo> _tunerReserveInfos;
        private readonly ComboBox _comboBox;

        private class Item
        {
            public string DisplayText { get; }
            public uint TunerID { get; }

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
