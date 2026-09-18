using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EpgTimer;
using Nett;

namespace RockbarForEDCB
{
    /// <summary>
    /// 設定フォームクラス
    /// </summary>
    public partial class SettingForm : Form
    {
        // 設定とサービスリスト
        private ConfigManager _configManager;

        // 各ListViewManager
        private SelectedServiceListViewManager _selectedServiceListViewManager;
        private FavoriteServiceListViewManager _favoriteServiceListViewManager;

        // ListViewのソーター。ListViewにセットすると自動整列解除できないため、適宜ListViewにセットする。
        private ListViewItemComparer tunerNameListViewSorter = new ListViewItemComparer();

        // CtrlCmdUtil
        private CtrlCmdUtil ctrlCmdUtil = null;

        // CtrlCmdの結果格納用
        private List<EpgServiceInfo> serviceInfos = new List<EpgServiceInfo>();
        private List<TunerReserveInfo> tunerReserveInfos = new List<TunerReserveInfo>();

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

                okButton.Click += okButton_Click;

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

            private void okButton_Click(object sender, EventArgs e)
            {
                //if (string.IsNullOrWhiteSpace(recFolderTextBox.Text))
                //{
                //    MessageBox.Show("録画フォルダを指定してください。", "入力エラー", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //    this.DialogResult = DialogResult.None;
                //}
            }
        }

        /// <summary>
        /// チューナーComboBox表示用アイテムクラス
        /// </summary>
        public class TunerSelectItem
        {
            public string DisplayText { get; set; }
            public uint TunerID { get; set; }

            public TunerSelectItem(string displayText, uint tunerID)
            {
                DisplayText = displayText;
                TunerID = tunerID;
            }

            public override string ToString()
            {
                return DisplayText;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// 設定ファイルを読み込み画面表示する。
        /// サービス一覧を取得し、全サービスとしてリストに表示する。
        /// </summary>
        /// <param name="ctrlCmdUtil"></param>
        /// <param name="canConnect"></param>
        public SettingForm(CtrlCmdUtil ctrlCmdUtil, bool canConnect)
        {
            InitializeComponent();

            this.ctrlCmdUtil = ctrlCmdUtil;

            // セッティングを読み込んで画面表示
            _configManager = new ConfigManager();

            // サービス一覧取得
            serviceInfos.Clear();
            tunerReserveInfos.Clear();

            if (canConnect)
            {
                ctrlCmdUtil.SendEnumTunerReserve(ref tunerReserveInfos);
                ctrlCmdUtil.SendEnumService(ref serviceInfos);
            }

            // 選択チャンネルタブ用のオブジェクト作成
            _selectedServiceListViewManager = new SelectedServiceListViewManager
                (_configManager, serviceInfos, allServiceListView, selectedServiceListView);

            // お気に入りチャンネルタブ用のオブジェクト作成
            _favoriteServiceListViewManager = new FavoriteServiceListViewManager
                (_configManager, selectedServiceListView, selectedServiceListView2, favoriteServiceListView);

            // EDCB連携設定の読み込み
            LoadEdcbLinkageSettings();

            // 予約動作設定の読み込み
            LoadReserveSettings();

            // チューナー名設定の読み込み
            LoadTunerNameSettings();

            // 選択サービス設定の読み込み
            _selectedServiceListViewManager.Load();

            // お気に入りサービス設定の読み込み
            _favoriteServiceListViewManager.Load();

            // TVTest連携設定の読み込み
            LoadTvTestLinkageSettings();

            // フォント・色(ListView)設定の読み込み
            LoadListViewFontColor();

            // フォント・色(右クリック)設定の読み込み
            LoadContextMenuFontColor();

            // フォント(コントロールUI)設定の読み込み
            LoadControlUiFontColor();

            // その他設定の読み込み
            LoadOtherSettings();

            // コントロールUIのフォント設定を適用
            ApplyUiFontSettings();
        }

        /// <summary>
        /// EDCB連携設定の読み込み
        /// </summary>
        private void LoadEdcbLinkageSettings()
        {
            useTcpIpCheckBox.Checked = _configManager.RockbarSetting.UseTcpIp;
            ipAddressTextBox.Text = _configManager.RockbarSetting.IpAddress;

            // port番号が異常の場合、下限にする
            if (_configManager.RockbarSetting.PortNumber > portNumberNumericUpDown.Maximum || _configManager.RockbarSetting.PortNumber < portNumberNumericUpDown.Minimum)
            {
                portNumberNumericUpDown.Value = portNumberNumericUpDown.Minimum;
            }
            else
            {
                portNumberNumericUpDown.Value = _configManager.RockbarSetting.PortNumber;
            }

            useWebLinkCheckBox.Checked = _configManager.RockbarSetting.UseWebLink;
            webEpgUrlTextBox.Text = _configManager.RockbarSetting.WebEpgUrl;
            webLinkUrlTextBox.Text = _configManager.RockbarSetting.WebLinkUrl;
            recInfoWebLinkUrlTextBox.Text = _configManager.RockbarSetting.RecInfoWebLinkUrl;
        }

        /// <summary>
        /// 予約動作設定の読み込み
        /// </summary>
        private void LoadReserveSettings()
        {
            // Rockbar機能
            useRockbarReserveAddCheckBox.Checked = _configManager.RockbarSetting.UseRockbarReserveAdd;
            useRockbarReserveModCheckBox.Checked = _configManager.RockbarSetting.UseRockbarReserveMod;
            useRockbarReserveDelCheckBox.Checked = _configManager.RockbarSetting.UseRockbarReserveDel;
            useRockbarReserveDelConfirmCheckBox.Checked = _configManager.RockbarSetting.UseRockbarReserveDelConfirm;

            // EDCB予約追加内容
            enableReserveCheckBox.Checked = _configManager.RockbarSetting.EnableReserve;
            prioritizeViewRadioButton.Checked = _configManager.RockbarSetting.PrioritizeView;
            recModeComboBox.SelectedIndex = _configManager.RockbarSetting.RecMode;

            recPriorityComboBox.SelectedIndex = _configManager.RockbarSetting.RecPriority - 1; // 優先度1のindexは0のため-1が必要
            recTuijyuuCheckBox.Checked = _configManager.RockbarSetting.RecTuijyuu;
            recPittariCheckBox.Checked = _configManager.RockbarSetting.RecPittari;

            useDefaultRecMarginCheckBox.Checked = !_configManager.RockbarSetting.UseCustomRecMargin; // 設定ファイルのUseCustomRecMarginは個別にStart,EndMargineを使用するかFlag
            startRecMarginNumericUpDown.Value = _configManager.RockbarSetting.StartRecMargin;
            endRecMarginNumericUpDown.Value = _configManager.RockbarSetting.EndRecMargin;

            // recServiceMode
            useDefaultRecServiceDataCheckBox.Checked = (_configManager.RockbarSetting.RecServiceMode & 0b0000_0001) == 0; // 1ビット目(デフォルトを使用(0:ON, 1:OFF))が 0 の場合 true、1 の場合 false
            recServiceDataCaptionCheckBox.Checked = (_configManager.RockbarSetting.RecServiceMode & 0b0001_0000) != 0; // 5ビット目(字幕を含める(0:OFF, 1:ON))が 1 の場合 true、0 の場合 false
            recServiceDataCarouselCheckBox.Checked = (_configManager.RockbarSetting.RecServiceMode & 0b0010_0000) != 0; // 6ビット目(データカルーセルを含める(0:OFF, 1:ON))が 1 の場合 true、0 の場合 false

            // 録画フォルダ
            recFolderListView.Items.Clear();

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
                    recFolderListView.Items.Add(item);
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
                    recFolderListView.Items.Add(item);
                }
            }

            partialRecSeparateFileCheckBox.Checked = _configManager.RockbarSetting.PartialRecSeparateFile;
            continueRecSameFileCheckBox.Checked = _configManager.RockbarSetting.ContinueRecSameFile;

            // チューナーComboBox の初期化
            recTunerIdComboBox.Items.Clear();
            recTunerIdComboBox.DisplayMember = "DisplayText";
            recTunerIdComboBox.ValueMember = "TunerID";
            recTunerIdComboBox.Items.Add(new TunerSelectItem("自動", 0)); // 先頭に「自動」（TunerID = 0）を追加

            // チューナーComboBoxの作成
            foreach (TunerReserveInfo tunerReserveInfo in tunerReserveInfos)
            {
                // 「チューナー不足 (0xFFFFFFFF)」は ComboBox の選択肢から除外
                if (tunerReserveInfo.tunerID != 0xFFFFFFFF)
                {
                    // 表示名: ID:00000001 (BonDriver_Proxy_T.dll)
                    string displayText = $"ID:{tunerReserveInfo.tunerID:x8} ({tunerReserveInfo.tunerName})";
                    recTunerIdComboBox.Items.Add(new TunerSelectItem(displayText, tunerReserveInfo.tunerID));
                }
            }

            // チューナーComboBox の初期選択肢設定
            uint targetTunerId = _configManager.RockbarSetting.RecTunerID;

            // recTunerIdComboBox の要素から、TunerID が一致するアイテムを検索
            var matchingItem = recTunerIdComboBox.Items
                .OfType<TunerSelectItem>()
                .FirstOrDefault(item => item.TunerID == targetTunerId);

            if (matchingItem != null)
            {
                // 一致する項目があればそれを選択
                recTunerIdComboBox.SelectedItem = matchingItem;
            }
            else if (recTunerIdComboBox.Items.Count > 0)
            {
                // 一致するものがない場合（初期状態など）は先頭（自動）を設定
                recTunerIdComboBox.SelectedIndex = 0;
            }

            // 録画後動作
            byte suspendMode = _configManager.RockbarSetting.SuspendModeAfterRec;

            // 0 なら デフォルトCheckBox を True、それ以外なら False
            defaultSuspendModeAfterRecCheckBox.Checked = (suspendMode == 0);

            // Panel内のラジオボタンを一括処理して Tag が一致するものを True にする
            foreach (RadioButton rb in suspendModeAfterRecPanel.Controls.OfType<RadioButton>())
            {
                if (rb.Tag != null && Convert.ToByte(rb.Tag) == suspendMode)
                {
                    rb.Checked = true;
                    break;
                }
            }

            rebootAfterReturnCheckBox.Checked = _configManager.RockbarSetting.RebootAfterReturn;

            // 録画後実行batと録画タグ
            string val = _configManager.RockbarSetting.RecBatFilePath;
            int pos = val.IndexOf('*');
            if (pos < 0)
            {
                recBatFilePathTextBox.Text = val;
                recTagTextBox.Text = "";
            }
            else
            {
                recBatFilePathTextBox.Text = val.Substring(0, pos);
                recTagTextBox.Text = val.Substring(pos + 1);
            }

            // 録画コメント
            recCommentTextBox.Text = _configManager.RockbarSetting.RecComment;

            // 予約画面の初期表示時の有効/無効状態を反映
            UpdateEdcbReserveSettingControlsEnableState();
            UpdateUseRockbarReserveDelConfirmControlsEnableState();
            UpdateMarginControlsEnableState();
            UpdateServiceDataControlsEnableState();
            UpdatePostRecActionControlsEnableState();
        }

        /// <summary>
        /// チューナー名設定の読み込み
        /// </summary>
        private void LoadTunerNameSettings()
        {
            // チューナー名タブのListView作成
            foreach (TunerReserveInfo tunerReserveInfo in tunerReserveInfos)
            {
                // ListView作成
                if (!tunerNameListView.Items.ContainsKey(tunerReserveInfo.tunerName))
                {
                    String[] data = null;

                    if (_configManager.RockbarSetting.BonDriverNameToTunerName.ContainsKey(tunerReserveInfo.tunerName))
                    {
                        data = new[]{
                            "",
                            (tunerReserveInfo.tunerID & 0xffff0000).ToString("x8").Substring(0, 4),
                            tunerReserveInfo.tunerName,
                            _configManager.RockbarSetting.BonDriverNameToTunerName[tunerReserveInfo.tunerName]
                        };

                    }
                    else
                    {
                        data = new[]{
                            "",
                            (tunerReserveInfo.tunerID & 0xffff0000).ToString("x8").Substring(0, 4),
                            tunerReserveInfo.tunerName,
                            RockbarUtility.GetDefaultTunerName(tunerReserveInfo.tunerName)
                        };
                    }

                    ListViewItem item = new ListViewItem(data);
                    item.Name = tunerReserveInfo.tunerName;
                    tunerNameListView.Items.Add(item);
                }
            }

            // 取得したチューナに含まれず、設定にだけあるものを一応表示
            foreach (var kv in _configManager.RockbarSetting.BonDriverNameToTunerName)
            {
                // ListView上になければ追加しておく
                if (!tunerNameListView.Items.ContainsKey(kv.Key))
                {
                    String[] data = {
                        "！",
                        "",
                        kv.Key,
                        kv.Value
                    };

                    ListViewItem item = new ListViewItem(data);
                    item.Name = kv.Key;
                    tunerNameListView.Items.Add(item);
                }
            }
        }

        /// <summary>
        /// TVTest連携設定の読み込み
        /// </summary>
        private void LoadTvTestLinkageSettings()
        {
            tvtestPathTextBox.Text = _configManager.RockbarSetting.TvtestPath;
            tvtestDttvOptionTextBox.Text = _configManager.RockbarSetting.TvtestDttvOption;
            tvtestBscsOptionTextBox.Text = _configManager.RockbarSetting.TvtestBscsOption;
            tvtestCatvOptionTextBox.Text = _configManager.RockbarSetting.TvtestCatvOption;
            tvtestSphdOptionTextBox.Text = _configManager.RockbarSetting.TvtestSphdOption;
            tvtestBs4kOptionTextBox.Text = _configManager.RockbarSetting.TvtestBs4kOption;
            tvtestTsFileOptionTextBox.Text = _configManager.RockbarSetting.TvtestTsFileOption;

            useDoubleClickTvtestCheckBox.Checked = _configManager.RockbarSetting.UseDoubleClickTvtest;

            isAutoOpenTvtestCheckBox.Checked = _configManager.RockbarSetting.IsAutoOpenTvtest;

            isAutoOpenDttvCheckBox.Checked = _configManager.RockbarSetting.IsAutoOpenTvtestDttv;
            isAutoOpenBsCheckBox.Checked = _configManager.RockbarSetting.IsAutoOpenTvtestBs;
            isAutoOpenCsCheckBox.Checked = _configManager.RockbarSetting.IsAutoOpenTvtestCs;
            isAutoOpenCatvCheckBox.Checked = _configManager.RockbarSetting.IsAutoOpenTvtestCatv;
            isAutoOpenBs4kCheckBox.Checked = _configManager.RockbarSetting.IsAutoOpenTvtestBs4k;
            isAutoOpenSphdCheckBox.Checked = _configManager.RockbarSetting.IsAutoOpenTvtestSphd;
            isAutoOpenFavoriteServiceCheckBox.Checked = _configManager.RockbarSetting.IsAutoOpenTvtestFavoriteService;

            // 設定値異常の場合、下限にする
            if (_configManager.RockbarSetting.AutoOpenMargin > autoOpenMarginNumericUpDown.Maximum || _configManager.RockbarSetting.AutoOpenMargin < autoOpenMarginNumericUpDown.Minimum)
            {
                autoOpenMarginNumericUpDown.Value = autoOpenMarginNumericUpDown.Minimum;
            }
            else
            {
                autoOpenMarginNumericUpDown.Value = _configManager.RockbarSetting.AutoOpenMargin;
            }

            // 設定値異常の場合、下限にする
            if (_configManager.RockbarSetting.AutoCloseMargin > autoCloseMarginNumericUpDown.Maximum || _configManager.RockbarSetting.AutoCloseMargin < autoCloseMarginNumericUpDown.Minimum)
            {
                autoCloseMarginNumericUpDown.Value = autoCloseMarginNumericUpDown.Minimum;
            }
            else
            {
                autoCloseMarginNumericUpDown.Value = _configManager.RockbarSetting.AutoCloseMargin;
            }
        }

        /// <summary>
        /// フォント・色(ListView)の設定の読み込み
        /// </summary>
        private void LoadListViewFontColor()
        {
            fontTextBox.Text = _configManager.RockbarSetting.Font;
            formBackColorTextBox.Text = _configManager.RockbarSetting.FormBackColor;
            foreColorTextBox.Text = _configManager.RockbarSetting.ForeColor;
            listBackColorTextBox.Text = _configManager.RockbarSetting.ListBackColor;
            okReserveListBackColorTextBox.Text = _configManager.RockbarSetting.OkReserveListBackColor;
            partialReserveListBackColorTextBox.Text = _configManager.RockbarSetting.PartialReserveListBackColor;
            ngReserveListBackColorTextBox.Text = _configManager.RockbarSetting.NgReserveListBackColor;
            disabledReserveListBackColorTextBox.Text = _configManager.RockbarSetting.DisabledReserveListBackColor;
            listHeaderForeColorTextBox.Text = _configManager.RockbarSetting.ListHeaderForeColor;
            listHeaderBackColorTextBox.Text = _configManager.RockbarSetting.ListHeaderBackColor;

            TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));
            previewListView.Font = (Font)fontConverter.ConvertFromString(_configManager.RockbarSetting.Font);

            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
            previewFormPanel.BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.FormBackColor);
            previewListView.BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.ListBackColor);
            previewListView.Items[1].BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.OkReserveListBackColor);
            previewListView.Items[2].BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.PartialReserveListBackColor);
            previewListView.Items[3].BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.NgReserveListBackColor);
            previewListView.Items[4].BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.DisabledReserveListBackColor);
            previewListView.Items[5].BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.ListHeaderBackColor);
            previewListView.ForeColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.ForeColor);
            previewListView.Items[5].ForeColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.ListHeaderForeColor);
        }

        /// <summary>
        /// フォント・色(右クリック)の設定の読み込み
        /// </summary>
        private void LoadContextMenuFontColor()
        {
            menuFontTextBox.Text = _configManager.RockbarSetting.MenuFont;
            menuBackColorTextBox.Text = _configManager.RockbarSetting.MenuBackColor;
            okReserveMenuBackColorTextBox.Text = _configManager.RockbarSetting.OkReserveMenuBackColor;
            partialReserveMenuBackColorTextBox.Text = _configManager.RockbarSetting.PartialReserveMenuBackColor;
            ngReserveMenuBackColorTextBox.Text = _configManager.RockbarSetting.NgReserveMenuBackColor;
            disabledReserveMenuBackColorTextBox.Text = _configManager.RockbarSetting.DisabledReserveMenuBackColor;

            TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));
            previewMenuListView.Font = (Font)fontConverter.ConvertFromString(_configManager.RockbarSetting.MenuFont);

            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
            previewMenuListView.BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.MenuBackColor);
            previewMenuListView.Items[1].BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.OkReserveMenuBackColor);
            previewMenuListView.Items[2].BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.PartialReserveMenuBackColor);
            previewMenuListView.Items[3].BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.NgReserveMenuBackColor);
            previewMenuListView.Items[4].BackColor = (Color)colorConverter.ConvertFromString(_configManager.RockbarSetting.DisabledReserveMenuBackColor);

        }

        /// <summary>
        /// フォント(コントロールUI)の設定の読み込み
        /// </summary>
        private void LoadControlUiFontColor()
        {
            tabFontTextBox.Text = _configManager.RockbarSetting.TabFont;
            buttonFontTextBox.Text = _configManager.RockbarSetting.ButtonFont;
            labelFontTextBox.Text = _configManager.RockbarSetting.LabelFont;
            textBoxFontTextBox.Text = _configManager.RockbarSetting.TextBoxFont;
        }

        /// <summary>
        /// その他の設定の読み込み
        /// </summary>
        private void LoadOtherSettings()
        {
            showTaskTrayIconCheckBox.Checked = _configManager.RockbarSetting.ShowTaskTrayIcon;
            storeTaskTrayByClosingCheckBox.Checked = _configManager.RockbarSetting.StoreTaskTrayByClosing;
            toggleVisibleTaskTrayIconClickCheckBox.Checked = _configManager.RockbarSetting.ToggleVisibleTaskTrayIconClick;
            isHorizontalSplitCheckBox.Checked = _configManager.RockbarSetting.IsHorizontalSplit;
            fixNoRecToServiceOnlyCheckBox.Checked = _configManager.RockbarSetting.FixNoRecToServiceOnly;

            // 設定値異常の場合、デフォルトにする
            if (_configManager.RockbarSetting.RecListMaxCount > recListMaxCountNumericUpDown.Maximum || _configManager.RockbarSetting.RecListMaxCount < recListMaxCountNumericUpDown.Minimum)
            {
                recListMaxCountNumericUpDown.Value = RockBarSetting.DEFAULT_REC_LIST_MAX_COUNT;
            }
            else
            {
                recListMaxCountNumericUpDown.Value = _configManager.RockbarSetting.RecListMaxCount;
            }

            // タスクトレイアイコンマウス操作
            taskTrayIconLeftClickComboBox.SelectedItem = _configManager.RockbarSetting.TaskTrayIconLeftClick;
            taskTrayIconLeftDoubleClickComboBox.SelectedItem = _configManager.RockbarSetting.TaskTrayIconLeftDoubleClick;
            taskTrayIconRightClickComboBox.SelectedItem = _configManager.RockbarSetting.TaskTrayIconRightClick;
            taskTrayIconRightDoubleClickComboBox.SelectedItem = _configManager.RockbarSetting.TaskTrayIconRightDoubleClick;
        }

        /// <summary>
        /// コントロールUIのフォント設定を適用
        /// </summary>
        private void ApplyUiFontSettings()
        {
            try
            {
                TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));
                Font tabFont = (Font)fontConverter.ConvertFromString(tabFontTextBox.Text);
                Font buttonFont = (Font)fontConverter.ConvertFromString(buttonFontTextBox.Text);
                Font labelFont = (Font)fontConverter.ConvertFromString(labelFontTextBox.Text);
                Font textBoxFont = (Font)fontConverter.ConvertFromString(textBoxFontTextBox.Text);

                Type settingType = this.GetType();
                FieldInfo[] fieldInfos = settingType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (FieldInfo fieldInfo in fieldInfos)
                {
                    string typeName = fieldInfo.FieldType.Name;
                    if (typeName == "TabControl")
                    {
                        TabControl obj = fieldInfo.GetValue(this) as TabControl;
                        obj.Font = tabFont;
                    }
                    else if (typeName == "Button")
                    {
                        Button obj = fieldInfo.GetValue(this) as Button;
                        obj.Font = buttonFont;
                    }
                    else if (typeName == "Label" || typeName == "CheckBox" || typeName == "GroupBox" || typeName == "ListView")
                    {
                        Control obj = fieldInfo.GetValue(this) as Control;
                        obj.Font = labelFont;
                    }
                    else if (typeName == "TextBox" || typeName == "NumericUpDown")
                    {
                        Control obj = fieldInfo.GetValue(this) as Control;
                        obj.Font = textBoxFont;
                    }
                }
                foreach (ListViewItem item in selectedServiceListView.Items)
                {
                    item.Font = labelFont;
                }
            }
            catch
            { }
        }

        /// <summary>
        /// キャンセルボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// 設定保存ボタン押下処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void applyButton_Click(object sender, EventArgs e)
        {
            // EDCB連携設定の保存
            SaveEdcbLinkageSettings();

            // 予約動作設定の保存
            SaveReserveSettings();

            // チューナー名設定の保存
            SaveTunerNameSettings();

            // 選択サービス設定の保存
            _selectedServiceListViewManager.Save();

            // お気に入りサービス設定の保存
            _favoriteServiceListViewManager.Save();

            // TVTest連携設定の保存
            SaveTvTestLinkageSettings();

            // フォント・色(ListView)設定の保存
            SaveListViewFontColor();

            // フォント・色(右クリック)設定の保存
            SaveContextMenuFontColor();

            // フォント(コントロールUI)設定の保存
            SaveControlUiFontColor();

            // その他設定の保存
            SaveOtherSettings();

            // 設定ファイルに書き込み
            _configManager.SaveFromFile();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// EDCB連携設定の保存
        /// </summary>
        private void SaveEdcbLinkageSettings()
        {
            _configManager.RockbarSetting.UseTcpIp = useTcpIpCheckBox.Checked;
            _configManager.RockbarSetting.IpAddress = ipAddressTextBox.Text;
            _configManager.RockbarSetting.PortNumber = (uint)portNumberNumericUpDown.Value;
            _configManager.RockbarSetting.UseWebLink = useWebLinkCheckBox.Checked;
            _configManager.RockbarSetting.WebEpgUrl = webEpgUrlTextBox.Text;
            _configManager.RockbarSetting.WebLinkUrl = webLinkUrlTextBox.Text;
            _configManager.RockbarSetting.RecInfoWebLinkUrl = recInfoWebLinkUrlTextBox.Text;
        }

        /// <summary>
        /// 予約動作設定の保存
        /// </summary>
        private void SaveReserveSettings()
        {
            // Rockbar機能
            _configManager.RockbarSetting.UseRockbarReserveAdd = useRockbarReserveAddCheckBox.Checked;
            _configManager.RockbarSetting.UseRockbarReserveMod = useRockbarReserveModCheckBox.Checked;
            _configManager.RockbarSetting.UseRockbarReserveDel = useRockbarReserveDelCheckBox.Checked;
            _configManager.RockbarSetting.UseRockbarReserveDelConfirm = useRockbarReserveDelConfirmCheckBox.Checked;

            // EDCB予約追加内容
            _configManager.RockbarSetting.EnableReserve = enableReserveCheckBox.Checked;
            _configManager.RockbarSetting.PrioritizeView = prioritizeViewRadioButton.Checked;
            _configManager.RockbarSetting.RecMode = (byte)recModeComboBox.SelectedIndex;

            _configManager.RockbarSetting.RecPriority = (byte)(recPriorityComboBox.SelectedIndex + 1); // 優先度1のindexは0のため+1が必要
            _configManager.RockbarSetting.RecTuijyuu = recTuijyuuCheckBox.Checked;
            _configManager.RockbarSetting.RecPittari = recPittariCheckBox.Checked;

            _configManager.RockbarSetting.UseCustomRecMargin = !useDefaultRecMarginCheckBox.Checked;  // 設定ファイルのUseCustomRecMarginは個別にStart,EndMargineを使用するかFlag
            _configManager.RockbarSetting.StartRecMargin = (int)startRecMarginNumericUpDown.Value;
            _configManager.RockbarSetting.EndRecMargin = (int)endRecMarginNumericUpDown.Value;

            // recServiceMode
            uint recServiceMode = _configManager.RockbarSetting.RecServiceMode;

            // 1ビット目(デフォルトを使用(0:ON, 1:OFF))
            // CheckBoxがChecked(True)ならビットを 0 に、Unchecked(False)ならビットを 1 にする
            if (useDefaultRecServiceDataCheckBox.Checked)
            {
                recServiceMode &= ~0b0000_0001u; // 0にクリア
            }
            else
            {
                recServiceMode |= 0b0000_0001;  // 1をセット
            }

            // 5ビット目(字幕を含める(0:OFF, 1:ON))
            // CheckBoxがChecked(True)ならビットを 1 に、Unchecked(False)ならビットを 0 にする
            if (recServiceDataCaptionCheckBox.Checked)
            {
                recServiceMode |= 0b0001_0000;   // 1をセット
            }
            else
            {
                recServiceMode &= ~0b0001_0000u; // 0にクリア
            }

            // 6ビット目(データカルーセルを含める(0:OFF, 1:ON))
            // CheckBoxがChecked(True)ならビットを 1 に、Unchecked(False)ならビットを 0 にする
            if (recServiceDataCarouselCheckBox.Checked)
            {
                recServiceMode |= 0b0010_0000;   // 1をセット
            }
            else
            {
                recServiceMode &= ~0b0010_0000u; // 0にクリア
            }

            _configManager.RockbarSetting.RecServiceMode = recServiceMode;
            // recServiceMode ここまで

            // 録画フォルダ関連
            var recFolderList = new List<RecFileSetInfo>();
            var partialRecFolderList = new List<RecFileSetInfo>();

            foreach (ListViewItem item in recFolderListView.Items)
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
            // 録画フォルダ関連 ここまで

            _configManager.RockbarSetting.PartialRecSeparateFile = partialRecSeparateFileCheckBox.Checked;
            _configManager.RockbarSetting.ContinueRecSameFile = continueRecSameFileCheckBox.Checked;

            _configManager.RockbarSetting.RecTunerID = ((TunerSelectItem)recTunerIdComboBox.SelectedItem)?.TunerID ?? 0;

            // 録画後動作
            byte suspendMode = 0;

            if (!defaultSuspendModeAfterRecCheckBox.Checked)
            {
                // チェックボックスが False の場合、選択中のラジオボタンの Tag を取得
                RadioButton selectedRb = suspendModeAfterRecPanel.Controls
                    .OfType<RadioButton>()
                    .FirstOrDefault(rb => rb.Checked);

                if (selectedRb?.Tag != null)
                {
                    suspendMode = Convert.ToByte(selectedRb.Tag);
                }
            }

            _configManager.RockbarSetting.SuspendModeAfterRec = suspendMode;
            _configManager.RockbarSetting.RebootAfterReturn = rebootAfterReturnCheckBox.Checked;

            // 録画後実行batと録画タグ
            _configManager.RockbarSetting.RecBatFilePath =
                string.IsNullOrEmpty(recTagTextBox.Text)
                    ? recBatFilePathTextBox.Text
                    : $"{recBatFilePathTextBox.Text}*{recTagTextBox.Text}";

            // 録画コメント
            _configManager.RockbarSetting.RecComment = recCommentTextBox.Text;
        }

        /// <summary>
        /// チューナー名設定の保存
        /// </summary>
        private void SaveTunerNameSettings()
        {
            _configManager.RockbarSetting.BonDriverNameToTunerName = new Dictionary<string, string>();

            foreach (ListViewItem item in tunerNameListView.Items)
            {
                _configManager.RockbarSetting.BonDriverNameToTunerName.Add(
                    item.SubItems[tunerNameBonDriverNameColumnHeader.Index].Text,
                    item.SubItems[tunerNameTunerNameColumnHeader.Index].Text
                );
            }
        }

        /// <summary>
        /// TVTest連携設定の保存
        /// </summary>
        private void SaveTvTestLinkageSettings()
        {
            _configManager.RockbarSetting.TvtestPath = tvtestPathTextBox.Text;
            _configManager.RockbarSetting.TvtestDttvOption = tvtestDttvOptionTextBox.Text;
            _configManager.RockbarSetting.TvtestBscsOption = tvtestBscsOptionTextBox.Text;
            _configManager.RockbarSetting.TvtestCatvOption = tvtestCatvOptionTextBox.Text;
            _configManager.RockbarSetting.TvtestSphdOption = tvtestSphdOptionTextBox.Text;
            _configManager.RockbarSetting.TvtestBs4kOption = tvtestBs4kOptionTextBox.Text;
            _configManager.RockbarSetting.TvtestTsFileOption = tvtestTsFileOptionTextBox.Text;

            _configManager.RockbarSetting.UseDoubleClickTvtest = useDoubleClickTvtestCheckBox.Checked;

            _configManager.RockbarSetting.IsAutoOpenTvtest = isAutoOpenTvtestCheckBox.Checked;

            _configManager.RockbarSetting.IsAutoOpenTvtestDttv = isAutoOpenDttvCheckBox.Checked;
            _configManager.RockbarSetting.IsAutoOpenTvtestBs = isAutoOpenBsCheckBox.Checked;
            _configManager.RockbarSetting.IsAutoOpenTvtestCs = isAutoOpenCsCheckBox.Checked;
            _configManager.RockbarSetting.IsAutoOpenTvtestCatv = isAutoOpenCatvCheckBox.Checked;
            _configManager.RockbarSetting.IsAutoOpenTvtestBs4k = isAutoOpenBs4kCheckBox.Checked;
            _configManager.RockbarSetting.IsAutoOpenTvtestSphd = isAutoOpenSphdCheckBox.Checked;
            _configManager.RockbarSetting.IsAutoOpenTvtestFavoriteService = isAutoOpenFavoriteServiceCheckBox.Checked;
            _configManager.RockbarSetting.AutoOpenMargin = (uint)autoOpenMarginNumericUpDown.Value;
            _configManager.RockbarSetting.AutoCloseMargin = (uint)autoCloseMarginNumericUpDown.Value;
        }

        /// <summary>
        /// フォント・色(ListView)の設定の保存
        /// </summary>
        private void SaveListViewFontColor()
        {
            _configManager.RockbarSetting.Font = fontTextBox.Text;
            _configManager.RockbarSetting.FormBackColor = formBackColorTextBox.Text;
            _configManager.RockbarSetting.ForeColor = foreColorTextBox.Text;
            _configManager.RockbarSetting.ListBackColor = listBackColorTextBox.Text;
            _configManager.RockbarSetting.OkReserveListBackColor = okReserveListBackColorTextBox.Text;
            _configManager.RockbarSetting.PartialReserveListBackColor = partialReserveListBackColorTextBox.Text;
            _configManager.RockbarSetting.NgReserveListBackColor = ngReserveListBackColorTextBox.Text;
            _configManager.RockbarSetting.DisabledReserveListBackColor = disabledReserveListBackColorTextBox.Text;
            _configManager.RockbarSetting.ListHeaderForeColor = listHeaderForeColorTextBox.Text;
            _configManager.RockbarSetting.ListHeaderBackColor = listHeaderBackColorTextBox.Text;
        }

        /// <summary>
        /// フォント・色(右クリック)の設定の保存
        /// </summary>
        private void SaveContextMenuFontColor()
        {
            _configManager.RockbarSetting.MenuFont = menuFontTextBox.Text;
            _configManager.RockbarSetting.MenuBackColor = menuBackColorTextBox.Text;
            _configManager.RockbarSetting.OkReserveMenuBackColor = okReserveMenuBackColorTextBox.Text;
            _configManager.RockbarSetting.PartialReserveMenuBackColor = partialReserveMenuBackColorTextBox.Text;
            _configManager.RockbarSetting.NgReserveMenuBackColor = ngReserveMenuBackColorTextBox.Text;
            _configManager.RockbarSetting.DisabledReserveMenuBackColor = disabledReserveMenuBackColorTextBox.Text;
        }

        /// <summary>
        /// フォント(コントロールUI)の設定の保存
        /// </summary>
        private void SaveControlUiFontColor()
        {
            _configManager.RockbarSetting.TabFont = tabFontTextBox.Text;
            _configManager.RockbarSetting.ButtonFont = buttonFontTextBox.Text;
            _configManager.RockbarSetting.LabelFont = labelFontTextBox.Text;
            _configManager.RockbarSetting.TextBoxFont = textBoxFontTextBox.Text;
        }

        /// <summary>
        /// その他の設定の保存
        /// </summary>
        private void SaveOtherSettings()
        {
            _configManager.RockbarSetting.ShowTaskTrayIcon = showTaskTrayIconCheckBox.Checked;
            _configManager.RockbarSetting.StoreTaskTrayByClosing = storeTaskTrayByClosingCheckBox.Checked;
            _configManager.RockbarSetting.ToggleVisibleTaskTrayIconClick = toggleVisibleTaskTrayIconClickCheckBox.Checked;
            _configManager.RockbarSetting.IsHorizontalSplit = isHorizontalSplitCheckBox.Checked;
            _configManager.RockbarSetting.FixNoRecToServiceOnly = fixNoRecToServiceOnlyCheckBox.Checked;

            _configManager.RockbarSetting.RecListMaxCount = (int)recListMaxCountNumericUpDown.Value;

            _configManager.RockbarSetting.TaskTrayIconLeftClick = taskTrayIconLeftClickComboBox.SelectedItem.ToString();
            _configManager.RockbarSetting.TaskTrayIconLeftDoubleClick = taskTrayIconLeftDoubleClickComboBox.SelectedItem.ToString();
            _configManager.RockbarSetting.TaskTrayIconRightClick = taskTrayIconRightClickComboBox.SelectedItem.ToString();
            _configManager.RockbarSetting.TaskTrayIconRightDoubleClick = taskTrayIconRightDoubleClickComboBox.SelectedItem.ToString();
        }

        // 「予約追加」チェックボックスの制御
        private void useRockbarReserveAddCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEdcbReserveSettingControlsEnableState();
        }

        // 「予約追加」チェックボックスがONの場合は、予約内容全体が入力不可
        private void UpdateEdcbReserveSettingControlsEnableState()
        {
            // チェックが入っていない場合は GroupBox 全体を編集不可 (Enabled = false)
            edcbReserveSettingGroupBox.Enabled = useRockbarReserveAddCheckBox.Checked;
        }

        // 「予約削除」チェックボックスの制御
        private void useRockbarReserveDelCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateUseRockbarReserveDelConfirmControlsEnableState();
        }

        // 「予約削除」チェックボックスがONの場合は、「削除前確認表示」が入力不可
        private void UpdateUseRockbarReserveDelConfirmControlsEnableState()
        {
            // チェックされている場合は入力不可 (Enabled = false)
            bool isInputEnabled = useRockbarReserveDelCheckBox.Checked;

            useRockbarReserveDelConfirmCheckBox.Enabled = isInputEnabled;
        }

        // 録画マージンのデフォルトチェックボックスの制御
        private void defaultMarginCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateMarginControlsEnableState();
        }

        // 録画マージンのデフォルトチェックボックスがONの場合は開始、終了の入力は不可
        private void UpdateMarginControlsEnableState()
        {
            // チェックされている場合は入力不可 (Enabled = false)
            bool isInputEnabled = !useDefaultRecMarginCheckBox.Checked;

            startRecMarginNumericUpDown.Enabled = isInputEnabled;
            endRecMarginNumericUpDown.Enabled = isInputEnabled;
        }

        // サービス対象データのデフォルトチェックボックスの制御
        private void defaultServiceDataCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateServiceDataControlsEnableState();
        }

        // サービス対象データのデフォルトチェックボックスがONの場合は、字幕を含めるとデータカルーセルを含めるは入力不可
        private void UpdateServiceDataControlsEnableState()
        {
            // チェックされている場合は入力不可 (Enabled = false)
            bool isInputEnabled = !useDefaultRecServiceDataCheckBox.Checked;

            recServiceDataCaptionCheckBox.Enabled = isInputEnabled;
            recServiceDataCarouselCheckBox.Enabled = isInputEnabled;
        }

        // 録画後動作のデフォルトチェックボックスの制御
        private void defaultPostRecActionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdatePostRecActionControlsEnableState();
        }

        // 録画後動作のデフォルトチェックボックスがONの場合は、各ラジオボタンと復帰後再起動するチェックボックスは入力不可
        private void UpdatePostRecActionControlsEnableState()
        {
            // チェックが入っている場合は編集不可 (Enabled = false)
            bool isInputEnabled = !defaultSuspendModeAfterRecCheckBox.Checked;

            afterRecNoActionRadioButton.Enabled = isInputEnabled;
            afterRecStandbyRadioButton.Enabled = isInputEnabled;
            afterRecSuspendRadioButton.Enabled = isInputEnabled;
            afterRecShutdownRadioButton.Enabled = isInputEnabled;
            rebootAfterReturnCheckBox.Enabled = isInputEnabled;
        }

        /// <summary>
        /// 録画フォルダ追加処理
        /// </summary>
        private void addRecFolderButton_Click(object sender, EventArgs e)
        {
            ShowRecFolderEditDialog(null);
        }

        /// <summary>
        /// 録画フォルダ変更処理
        /// </summary>
        private void editRecFolderButton_Click(object sender, EventArgs e)
        {
            if (recFolderListView.SelectedItems.Count > 0)
            {
                ShowRecFolderEditDialog(recFolderListView.SelectedItems[0]);
            }
        }

        // 録画フォルダListViewダブルクリック動作
        private void recFolderListView_DoubleClick(object sender, EventArgs e)
        {
            if (recFolderListView.SelectedItems.Count > 0)
            {
                ShowRecFolderEditDialog(recFolderListView.SelectedItems[0]);
            }
        }

        /// <summary>
        /// 録画フォルダの追加・編集ダイアログ表示共通処理
        /// </summary>
        private void ShowRecFolderEditDialog(ListViewItem targetItem)
        {
            // PlugIn 一覧の取得
            var writePlugIns = new List<string>();
            var fileNamePlugIns = new List<string>();
            if (ctrlCmdUtil != null)
            {
                ctrlCmdUtil.SendEnumPlugIn(2, ref writePlugIns);
                ctrlCmdUtil.SendEnumPlugIn(1, ref fileNamePlugIns);
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
                if (dialog.ShowDialog(this) != DialogResult.OK) return;

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
                    recFolderListView.Items.Add(new ListViewItem(subItemTexts));
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
        /// 録画フォルダコピー処理
        /// </summary>
        private void copyRecFolderButton_Click(object sender, EventArgs e)
        {
            // リストで項目が選択されていない場合は何もしない
            if (recFolderListView.SelectedItems.Count == 0)
            {
                return;
            }

            // 選択中の行のデータを取得
            ListViewItem selectedItem = recFolderListView.SelectedItems[0];

            // 取得したデータで新しい ListViewItem を複製してリストに追加
            ListViewItem newItem = (ListViewItem)selectedItem.Clone();
            recFolderListView.Items.Add(newItem);

            // コピーされた新しい項目を選択状態にする（任意）
            newItem.Selected = true;
            recFolderListView.EnsureVisible(newItem.Index);
        }

        /// <summary>
        /// 録画フォルダ削除処理
        /// </summary>
        private void delRecFolderButton_Click(object sender, EventArgs e)
        {
            // リストで項目が選択されていない場合は何もしない
            if (recFolderListView.SelectedItems.Count == 0)
            {
                return;
            }

            // 選択中の行を削除
            ListViewItem selectedItem = recFolderListView.SelectedItems[0];
            recFolderListView.Items.Remove(selectedItem);
        }

        /// <summary>
        /// バッチファイル参照ボタン処理
        /// </summary>
        private void browseBatFileButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                // 選択可能な拡張子のフィルタを設定
                ofd.Filter = "実行可能スクリプト (*.bat;*.ps1;*.lua)|*.bat;*.ps1;*.lua" +
                             "|Batchファイル (*.bat)|*.bat" +
                             "|PowerShellスクリプト (*.ps1)|*.ps1" +
                             "|Luaスクリプト (*.lua)|*.lua" +
                             "|すべてのファイル (*.*)|*.*";

                ofd.FilterIndex = 1; // デフォルトで最初のフィルタを選択
                ofd.Title = "バッチ/スクリプトファイルの選択";

                // すでにテキストボックスにパスが入力されていれば、そのフォルダを初期表示に設定
                if (!string.IsNullOrWhiteSpace(recBatFilePathTextBox.Text) && System.IO.File.Exists(recBatFilePathTextBox.Text))
                {
                    ofd.InitialDirectory = System.IO.Path.GetDirectoryName(recBatFilePathTextBox.Text);
                    ofd.FileName = System.IO.Path.GetFileName(recBatFilePathTextBox.Text);
                }

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    recBatFilePathTextBox.Text = ofd.FileName;
                }
            }
        }

        /// <summary>
        /// 選択サービス追加処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void addSelectedServiceButton_Click(object sender, EventArgs e)
        {
            _selectedServiceListViewManager.AddService();
        }

        /// <summary>
        /// お気に入りサービス追加処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void addFavoriteServiceButton_Click(object sender, EventArgs e)
        {
            _favoriteServiceListViewManager.AddService();
        }

        /// <summary>
        /// 選択サービス削除処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void removeServiceButton_Click(object sender, EventArgs e)
        {
            _selectedServiceListViewManager.RemoveService();
        }

        /// <summary>
        /// お気に入りサービス削除処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void removeFavoriteServiceButton_Click(object sender, EventArgs e)
        {
            _favoriteServiceListViewManager.RemoveService();
        }

        /// <summary>
        /// 選択サービス上移動処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void moveUpSelectedServiceButton_Click(object sender, EventArgs e)
        {
            _selectedServiceListViewManager.MoveUp();
        }

        /// <summary>
        /// お気に入りサービス上移動処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void moveUpFavoriteServiceButton_Click(object sender, EventArgs e)
        {
            _favoriteServiceListViewManager.MoveUp();
        }

        /// <summary>
        /// 選択サービス下移動処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void moveDownSelectedServiceButton_Click(object sender, EventArgs e)
        {
            _selectedServiceListViewManager.MoveDown();
        }

        /// <summary>
        /// お気に入りサービス下移動処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void moveDownFavoriteServiceButton_Click(object sender, EventArgs e)
        {
            _favoriteServiceListViewManager.MoveDown();
        }

        /// <summary>
        /// 選択サービス新規追加処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void addNewServiceButton_Click(object sender, EventArgs e)
        {
            _selectedServiceListViewManager.EditService(null);
        }

        /// <summary>
        /// 選択サービス編集処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void editServiceButton_Click(object sender, EventArgs e)
        {
            if (selectedServiceListView.SelectedItems.Count == 0)
            {
                return;
            }

            _selectedServiceListViewManager.EditService(selectedServiceListView.SelectedItems[0]);
        }

        /// <summary>
        /// 選択サービス編集処理(ダブルクリック)
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectedServiceListView_DoubleClick(object sender, EventArgs e)
        {
            editServiceButton_Click(sender, e);
        }

        /// <summary>
        /// 設定タブ切り替え処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void settingTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (settingTabControl.SelectedTab == favoriteServiceTabPage)
            {
                _favoriteServiceListViewManager.RefreshRefreshFrom();
            }
        }

        /// <summary>
        /// 全サービス一覧のカラムヘッダクリック処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void allServiceListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // ソートする
            _selectedServiceListViewManager.SortAllServiceList(e.Column);
        }

        /// <summary>
        /// 選択サービス一覧のカラムヘッダクリック処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectedServiceListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // ソートする
            _selectedServiceListViewManager.SortSelectedServiceList(e.Column);
        }

        /// <summary>
        /// 選択サービス一覧(お気に入りサービスタブ)のカラムヘッダクリック処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectedServiceListView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // ソートする
            _favoriteServiceListViewManager.SortSelectedService2List(e.Column);
        }

        /// <summary>
        /// お気に入りサービス一覧のカラムヘッダクリック処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void favoriteServiceListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // ソートする
            _favoriteServiceListViewManager.SortFavoriteServiceList(e.Column);
        }

        /// <summary>
        /// チューナー名一覧のカラムヘッダクリック処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void tunerNameListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // ソートする
            tunerNameListViewSorter.SetColumn(e.Column);
            tunerNameListView.ListViewItemSorter = tunerNameListViewSorter;
            tunerNameListView.Sort();
        }

        /// <summary>
        /// チューナー名選択項目変更処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void tunerNameListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            // テキストボックスに選択したチューナー名を表示
            if (tunerNameListView.SelectedItems.Count > 0)
            {
                tunerNameTextBox.Text = tunerNameListView.SelectedItems[0].SubItems[tunerNameTunerNameColumnHeader.Index].Text;
            }
        }

        /// <summary>
        /// チューナー名更新ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void updateTunerNameButton_Click(object sender, EventArgs e)
        {
            // ListViewに反映
            if (tunerNameListView.SelectedItems.Count > 0)
            {
                tunerNameListView.SelectedItems[0].SubItems[tunerNameTunerNameColumnHeader.Index].Text = tunerNameTextBox.Text;
            }
        }

        /// <summary>
        /// TVTest参照ボタン押下処理
        /// ファイル選択ダイアログを開く。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void tvtestOpenButton_Click(object sender, EventArgs e)
        {
            tvtestOpenFileDialog.ShowDialog();
        }

        /// <summary>
        /// ファイル選択ダイアログ選択完了処理
        /// TVTest.exeパスを設定する。
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void tvtestOpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            tvtestPathTextBox.Text = tvtestOpenFileDialog.FileName;
        }

        /// <summary>
        /// フォント選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectFontButton_Click(object sender, EventArgs e)
        {
            // フォント選択ダイアログを開いて設定値を反映
            TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));

            if (fontTextBox.Text != null)
            {
                fontDialog.Font = (Font)fontConverter.ConvertFromString(fontTextBox.Text);
            }

            DialogResult result = fontDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                fontTextBox.Text = fontConverter.ConvertToString(fontDialog.Font);
                previewListView.Font = fontDialog.Font;
            }
        }

        /// <summary>
        /// フォーム背景色選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectFormBackColorButton_Click(object sender, EventArgs e)
        {
            // カラー選択ダイアログを開いて設定値を反映
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            if (formBackColorTextBox.Text != null)
            {
                colorDialog.Color = (Color)colorConverter.ConvertFromString(formBackColorTextBox.Text);
            }

            DialogResult result = colorDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                formBackColorTextBox.Text = colorConverter.ConvertToString(colorDialog.Color);
                previewFormPanel.BackColor = colorDialog.Color;
            }
        }

        /// <summary>
        /// 文字色選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectForeColorButton_Click(object sender, EventArgs e)
        {
            // カラー選択ダイアログを開いて設定値を反映
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            if (foreColorTextBox.Text != null)
            {
                colorDialog.Color = (Color)colorConverter.ConvertFromString(foreColorTextBox.Text);
            }

            DialogResult result = colorDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                foreColorTextBox.Text = colorConverter.ConvertToString(colorDialog.Color);
                previewListView.ForeColor = colorDialog.Color;
            }
        }

        /// <summary>
        /// リスト背景色選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectListBackColorButton_Click(object sender, EventArgs e)
        {
            // カラー選択ダイアログを開いて設定値を反映
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            if (listBackColorTextBox.Text != null)
            {
                colorDialog.Color = (Color)colorConverter.ConvertFromString(listBackColorTextBox.Text);
            }

            DialogResult result = colorDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                listBackColorTextBox.Text = colorConverter.ConvertToString(colorDialog.Color);
                previewListView.BackColor = colorDialog.Color;
            }
        }

        /// <summary>
        /// リスト背景色(正常予約)選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectOkReserveListBackColorButton_Click(object sender, EventArgs e)
        {
            // カラー選択ダイアログを開いて設定値を反映
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            if (okReserveListBackColorTextBox.Text != null)
            {
                colorDialog.Color = (Color)colorConverter.ConvertFromString(okReserveListBackColorTextBox.Text);
            }

            DialogResult result = colorDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                okReserveListBackColorTextBox.Text = colorConverter.ConvertToString(colorDialog.Color);
                previewListView.Items[1].BackColor = colorDialog.Color;
            }
        }

        /// <summary>
        /// リスト背景色(部分予約)選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectPartialReserveListBackColorButton_Click(object sender, EventArgs e)
        {
            // カラー選択ダイアログを開いて設定値を反映
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            if (partialReserveListBackColorTextBox.Text != null)
            {
                colorDialog.Color = (Color)colorConverter.ConvertFromString(partialReserveListBackColorTextBox.Text);
            }

            DialogResult result = colorDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                partialReserveListBackColorTextBox.Text = colorConverter.ConvertToString(colorDialog.Color);
                previewListView.Items[2].BackColor = colorDialog.Color;
            }
        }

        /// <summary>
        /// リスト背景色(予約不可)選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectNgReserveListBackColorButton_Click(object sender, EventArgs e)
        {
            // カラー選択ダイアログを開いて設定値を反映
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            if (ngReserveListBackColorTextBox.Text != null)
            {
                colorDialog.Color = (Color)colorConverter.ConvertFromString(ngReserveListBackColorTextBox.Text);
            }

            DialogResult result = colorDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                ngReserveListBackColorTextBox.Text = colorConverter.ConvertToString(colorDialog.Color);
                previewListView.Items[3].BackColor = colorDialog.Color;
            }
        }

        /// <summary>
        /// リスト背景色(無効予約)選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectDisabledReserveListBackColorButton_Click(object sender, EventArgs e)
        {
            // カラー選択ダイアログを開いて設定値を反映
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            if (disabledReserveListBackColorTextBox.Text != null)
            {
                colorDialog.Color = (Color)colorConverter.ConvertFromString(disabledReserveListBackColorTextBox.Text);
            }

            DialogResult result = colorDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                disabledReserveListBackColorTextBox.Text = colorConverter.ConvertToString(colorDialog.Color);
                previewListView.Items[4].BackColor = colorDialog.Color;
            }
        }

        /// <summary>
        /// 予約一覧ヘッダ文字色選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectListHeaderForeColorButton_Click(object sender, EventArgs e)
        {
            // カラー選択ダイアログを開いて設定値を反映
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            if (listHeaderForeColorTextBox.Text != null)
            {
                colorDialog.Color = (Color)colorConverter.ConvertFromString(listHeaderForeColorTextBox.Text);
            }

            DialogResult result = colorDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                listHeaderForeColorTextBox.Text = colorConverter.ConvertToString(colorDialog.Color);
                previewListView.Items[5].ForeColor = colorDialog.Color;
            }
        }

        /// <summary>
        /// 予約一覧ヘッダ背景色選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectListHeaderBackColorButton_Click(object sender, EventArgs e)
        {
            // カラー選択ダイアログを開いて設定値を反映
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            if (listHeaderBackColorTextBox.Text != null)
            {
                colorDialog.Color = (Color)colorConverter.ConvertFromString(listHeaderBackColorTextBox.Text);
            }

            DialogResult result = colorDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                listHeaderBackColorTextBox.Text = colorConverter.ConvertToString(colorDialog.Color);
                previewListView.Items[5].BackColor = colorDialog.Color;
            }
        }

        /// <summary>
        /// メニューフォント選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectMenuFontButton_Click(object sender, EventArgs e)
        {
            // フォント選択ダイアログを開いて設定値を反映
            TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));

            if (menuFontTextBox.Text != null)
            {
                fontDialog.Font = (Font)fontConverter.ConvertFromString(menuFontTextBox.Text);
            }

            DialogResult result = fontDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                menuFontTextBox.Text = fontConverter.ConvertToString(fontDialog.Font);
                previewMenuListView.Font = fontDialog.Font;
            }
        }

        /// <summary>
        /// メニュー背景色選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectMenuBackColorButton_Click(object sender, EventArgs e)
        {
            // カラー選択ダイアログを開いて設定値を反映
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            if (menuBackColorTextBox.Text != null)
            {
                colorDialog.Color = (Color)colorConverter.ConvertFromString(menuBackColorTextBox.Text);
            }

            DialogResult result = colorDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                menuBackColorTextBox.Text = colorConverter.ConvertToString(colorDialog.Color);
                previewMenuListView.BackColor = colorDialog.Color;
            }
        }

        /// <summary>
        /// メニュー背景色(正常予約)選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectOkReserveMenuBackColorButton_Click(object sender, EventArgs e)
        {
            // カラー選択ダイアログを開いて設定値を反映
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            if (okReserveMenuBackColorTextBox.Text != null)
            {
                colorDialog.Color = (Color)colorConverter.ConvertFromString(okReserveMenuBackColorTextBox.Text);
            }

            DialogResult result = colorDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                okReserveMenuBackColorTextBox.Text = colorConverter.ConvertToString(colorDialog.Color);
                previewMenuListView.Items[1].BackColor = colorDialog.Color;
            }
        }

        /// <summary>
        /// メニュー背景色(部分予約)選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectPartialReserveMenuBackColorButton_Click(object sender, EventArgs e)
        {
            // カラー選択ダイアログを開いて設定値を反映
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            if (partialReserveMenuBackColorTextBox.Text != null)
            {
                colorDialog.Color = (Color)colorConverter.ConvertFromString(partialReserveMenuBackColorTextBox.Text);
            }

            DialogResult result = colorDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                partialReserveMenuBackColorTextBox.Text = colorConverter.ConvertToString(colorDialog.Color);
                previewMenuListView.Items[2].BackColor = colorDialog.Color;
            }
        }

        /// <summary>
        /// メニュー背景色(予約不可)選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectNgReserveMenuBackColorButton_Click(object sender, EventArgs e)
        {
            // カラー選択ダイアログを開いて設定値を反映
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            if (ngReserveMenuBackColorTextBox.Text != null)
            {
                colorDialog.Color = (Color)colorConverter.ConvertFromString(ngReserveMenuBackColorTextBox.Text);
            }

            DialogResult result = colorDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                ngReserveMenuBackColorTextBox.Text = colorConverter.ConvertToString(colorDialog.Color);
                previewMenuListView.Items[3].BackColor = colorDialog.Color;
            }
        }

        /// <summary>
        /// メニュー背景色(無効予約)選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectDisabledReserveMenuBackColorButton_Click(object sender, EventArgs e)
        {
            // カラー選択ダイアログを開いて設定値を反映
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            if (disabledReserveMenuBackColorTextBox.Text != null)
            {
                colorDialog.Color = (Color)colorConverter.ConvertFromString(disabledReserveMenuBackColorTextBox.Text);
            }

            DialogResult result = colorDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                disabledReserveMenuBackColorTextBox.Text = colorConverter.ConvertToString(colorDialog.Color);
                previewMenuListView.Items[4].BackColor = colorDialog.Color;
            }
        }

        /// <summary>
        /// タブのフォント選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectTabFontButton_Click(object sender, EventArgs e)
        {
            // フォント選択ダイアログを開いて設定値を反映
            TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));

            if (tabFontTextBox.Text != null)
            {
                fontDialog.Font = (Font)fontConverter.ConvertFromString(tabFontTextBox.Text);
            }

            DialogResult result = fontDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                tabFontTextBox.Text = fontConverter.ConvertToString(fontDialog.Font);
            }
        }

        /// <summary>
        /// ボタンのフォント選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectButtonFontButton_Click(object sender, EventArgs e)
        {
            // フォント選択ダイアログを開いて設定値を反映
            TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));

            if (buttonFontTextBox.Text != null)
            {
                fontDialog.Font = (Font)fontConverter.ConvertFromString(buttonFontTextBox.Text);
            }

            DialogResult result = fontDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                buttonFontTextBox.Text = fontConverter.ConvertToString(fontDialog.Font);
            }
        }

        /// <summary>
        /// ラベルのフォント選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectLabelFontButton_Click(object sender, EventArgs e)
        {
            // フォント選択ダイアログを開いて設定値を反映
            TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));

            if (labelFontTextBox.Text != null)
            {
                fontDialog.Font = (Font)fontConverter.ConvertFromString(labelFontTextBox.Text);
            }

            DialogResult result = fontDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                labelFontTextBox.Text = fontConverter.ConvertToString(fontDialog.Font);
            }
        }

        /// <summary>
        /// テキストボックスのフォント選択ボタン押下処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectTextBoxFontButton_Click(object sender, EventArgs e)
        {
            // フォント選択ダイアログを開いて設定値を反映
            TypeConverter fontConverter = TypeDescriptor.GetConverter(typeof(Font));

            if (textBoxFontTextBox.Text != null)
            {
                fontDialog.Font = (Font)fontConverter.ConvertFromString(textBoxFontTextBox.Text);
            }

            DialogResult result = fontDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                textBoxFontTextBox.Text = fontConverter.ConvertToString(fontDialog.Font);
            }
        }
    }
}
