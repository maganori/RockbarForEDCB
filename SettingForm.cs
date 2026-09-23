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
        private RecFolderListViewManager _recFolderListViewManager;
        private TunerNameListViewManager _tunerNameListViewManager;

        // 予約タブ TunerComboBoxManager
        private TunerComboBoxManager _tunerComboBoxManager;

        // CtrlCmdUtil
        private CtrlCmdUtil _ctrlCmdUtil;

        // CtrlCmdの結果格納用
        private List<EpgServiceInfo> _serviceInfos = new List<EpgServiceInfo>();
        private List<TunerReserveInfo> _tunerReserveInfos = new List<TunerReserveInfo>();

        // TypeConverterのキャッシュ
        private readonly TypeConverter _fontConverter = TypeDescriptor.GetConverter(typeof(Font));
        private readonly TypeConverter _colorConverter = TypeDescriptor.GetConverter(typeof(Color));

        /// <summary>
        /// 設定が適用されたときに発生するイベント
        /// </summary>
        public event EventHandler ApplyRequested;

        /// <summary>
        /// コンストラクタ
        /// 設定ファイルを読み込み画面表示する。
        /// </summary>
        /// <param name="ctrlCmdUtil"></param>
        /// <param name="canConnect"></param>
        public SettingForm(CtrlCmdUtil ctrlCmdUtil, bool canConnect)
        {
            InitializeComponent();

            // バージョン情報ラベルを設定
            versionLabel.Text =$"Version : {AppVersionAttribute.GetVersion()}";

            _ctrlCmdUtil = ctrlCmdUtil;

            // ConfigManagerで設定を読み込み
            _configManager = new ConfigManager();

            // サービス一覧取得
            _serviceInfos.Clear();
            _tunerReserveInfos.Clear();

            if (canConnect)
            {
                _ctrlCmdUtil.SendEnumTunerReserve(ref _tunerReserveInfos);
                _ctrlCmdUtil.SendEnumService(ref _serviceInfos);
            }

            // 選択チャンネルタブ用のオブジェクト作成
            _selectedServiceListViewManager = new SelectedServiceListViewManager
                (_configManager, _serviceInfos, allServiceListView, selectedServiceListView);

            // お気に入りチャンネルタブ用のオブジェクト作成
            _favoriteServiceListViewManager = new FavoriteServiceListViewManager
                (_configManager, selectedServiceListView, selectedServiceListView2, favoriteServiceListView);

            // 予約タブ RecFolderListViewManagerのオブジェクト作成
            _recFolderListViewManager = new RecFolderListViewManager(_configManager, recFolderListView, _ctrlCmdUtil);

            // 予約タブ TunerComboBoxManagerのオブジェクト作成
            _tunerComboBoxManager = new TunerComboBoxManager(_configManager, _tunerReserveInfos, recTunerIdComboBox);

            // チューナー名タブ用のオブジェクト作成
            _tunerNameListViewManager = new TunerNameListViewManager(_configManager, _tunerReserveInfos, tunerNameListView);

            // ConfigManagerの全ての設定を読み込み
            LoadAllSettings();

            // コントロールUIのフォント設定を適用
            ApplyUiFontSettings();
        }

        /// <summary>
        /// ConfigManagerの全ての設定を読み込むヘルパー
        /// </summary>
        private void LoadAllSettings()
        {
            // EDCB連携設定の読み込み
            LoadEdcbLinkageSettings();

            // 予約動作設定の読み込み
            LoadReserveSettings();

            // チューナー名設定の読み込み
            _tunerNameListViewManager.Load();

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
        }

        /// <summary>
        /// EDCB連携設定の読み込み
        /// </summary>
        private void LoadEdcbLinkageSettings()
        {
            useTcpIpCheckBox.Checked = _configManager.RockbarSetting.UseTcpIp;
            ipAddressTextBox.Text = _configManager.RockbarSetting.IpAddress;

            // port番号が異常の場合、下限にする
            if (_configManager.RockbarSetting.PortNumber > portNumberNumericUpDown.Maximum
                || _configManager.RockbarSetting.PortNumber < portNumberNumericUpDown.Minimum)
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

            // EDCB連携画面の初期表示時の有効/無効状態を反映
            ApplyUseTcpIpCheckBoxState();
            ApplyUseWebLinkCheckBoxState();
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

            // 優先度1のindexは0のためindexに反映時は-1が必要
            recPriorityComboBox.SelectedIndex = _configManager.RockbarSetting.RecPriority - 1;
            recTuijyuuCheckBox.Checked = _configManager.RockbarSetting.RecTuijyuu;
            recPittariCheckBox.Checked = _configManager.RockbarSetting.RecPittari;

            // 設定ファイルのUseCustomRecMarginは個別にStart,EndMargineを使用するかFlag
            useDefaultRecMarginCheckBox.Checked = !_configManager.RockbarSetting.UseCustomRecMargin;
            startRecMarginNumericUpDown.Value = _configManager.RockbarSetting.StartRecMargin;
            endRecMarginNumericUpDown.Value = _configManager.RockbarSetting.EndRecMargin;

            // recServiceMode
            // 1ビット目(デフォルトを使用(0:ON, 1:OFF))が 0 の場合 true、1 の場合 false
            useDefaultRecServiceDataCheckBox.Checked = (_configManager.RockbarSetting.RecServiceMode & 0b0000_0001) == 0;

            // 5ビット目(字幕を含める(0:OFF, 1:ON))が 1 の場合 true、0 の場合 false
            recServiceDataCaptionCheckBox.Checked = (_configManager.RockbarSetting.RecServiceMode & 0b0001_0000) != 0;

            // 6ビット目(データカルーセルを含める(0:OFF, 1:ON))が 1 の場合 true、0 の場合 false
            recServiceDataCarouselCheckBox.Checked = (_configManager.RockbarSetting.RecServiceMode & 0b0010_0000) != 0;

            // 録画フォルダ
            _recFolderListViewManager.Load();

            partialRecSeparateFileCheckBox.Checked = _configManager.RockbarSetting.PartialRecSeparateFile;
            continueRecSameFileCheckBox.Checked = _configManager.RockbarSetting.ContinueRecSameFile;

            // 使用チューナーの読み込み
            _tunerComboBoxManager.Load();

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
            ApplyUseRockbarReserveAddCheckBoxState();
            ApplyUseRockbarReserveDelCheckBoxState();
            ApplyDefaultMarginCheckBoxState();
            ApplyDefaultServiceDataCheckBoxState();
            ApplyDefaultPostRecActionCheckBoxState();
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
            if (_configManager.RockbarSetting.AutoOpenMargin > autoOpenMarginNumericUpDown.Maximum
                || _configManager.RockbarSetting.AutoOpenMargin < autoOpenMarginNumericUpDown.Minimum)
            {
                autoOpenMarginNumericUpDown.Value = autoOpenMarginNumericUpDown.Minimum;
            }
            else
            {
                autoOpenMarginNumericUpDown.Value = _configManager.RockbarSetting.AutoOpenMargin;
            }

            // 設定値異常の場合、下限にする
            if (_configManager.RockbarSetting.AutoCloseMargin > autoCloseMarginNumericUpDown.Maximum
                || _configManager.RockbarSetting.AutoCloseMargin < autoCloseMarginNumericUpDown.Minimum)
            {
                autoCloseMarginNumericUpDown.Value = autoCloseMarginNumericUpDown.Minimum;
            }
            else
            {
                autoCloseMarginNumericUpDown.Value = _configManager.RockbarSetting.AutoCloseMargin;
            }

            // TVTest連携画面の初期表示時の有効/無効状態を反映
            ApplyIsAutoOpenTvtestCheckBoxState();
        }

        /// <summary>
        /// フォント・色1(ListView)の設定の読み込みとプレビュー反映
        /// </summary>
        private void LoadListViewFontColor()
        {
            listFontTextBox.Text = _configManager.RockbarSetting.ListFont;
            formBackColorTextBox.Text = _configManager.RockbarSetting.FormBackColor;
            listForeColorTextBox.Text = _configManager.RockbarSetting.ListForeColor;
            listBackColorTextBox.Text = _configManager.RockbarSetting.ListBackColor;
            okReserveListBackColorTextBox.Text = _configManager.RockbarSetting.OkReserveListBackColor;
            partialReserveListBackColorTextBox.Text = _configManager.RockbarSetting.PartialReserveListBackColor;
            ngReserveListBackColorTextBox.Text = _configManager.RockbarSetting.NgReserveListBackColor;
            disabledReserveListBackColorTextBox.Text = _configManager.RockbarSetting.DisabledReserveListBackColor;
            listHeaderForeColorTextBox.Text = _configManager.RockbarSetting.ListHeaderForeColor;
            listHeaderBackColorTextBox.Text = _configManager.RockbarSetting.ListHeaderBackColor;

            // プレビューへ反映
            previewListView.Font = (Font)_fontConverter.ConvertFromString(_configManager.RockbarSetting.ListFont);

            previewFormPanel.BackColor = (Color)_colorConverter.ConvertFromString(_configManager.RockbarSetting.FormBackColor);
            previewListView.BackColor = (Color)_colorConverter.ConvertFromString(_configManager.RockbarSetting.ListBackColor);
            previewListView.Items[1].BackColor = (Color)_colorConverter.ConvertFromString(_configManager.RockbarSetting.OkReserveListBackColor);
            previewListView.Items[2].BackColor = (Color)_colorConverter.ConvertFromString(_configManager.RockbarSetting.PartialReserveListBackColor);
            previewListView.Items[3].BackColor = (Color)_colorConverter.ConvertFromString(_configManager.RockbarSetting.NgReserveListBackColor);
            previewListView.Items[4].BackColor = (Color)_colorConverter.ConvertFromString(_configManager.RockbarSetting.DisabledReserveListBackColor);
            previewListView.Items[5].BackColor = (Color)_colorConverter.ConvertFromString(_configManager.RockbarSetting.ListHeaderBackColor);
            previewListView.ForeColor = (Color)_colorConverter.ConvertFromString(_configManager.RockbarSetting.ListForeColor);
            previewListView.Items[5].ForeColor = (Color)_colorConverter.ConvertFromString(_configManager.RockbarSetting.ListHeaderForeColor);
        }

        /// <summary>
        /// フォント・色2(右クリックメニュー)の設定の読み込みとプレビュー反映
        /// </summary>
        private void LoadContextMenuFontColor()
        {
            menuFontTextBox.Text = _configManager.RockbarSetting.MenuFont;
            menuBackColorTextBox.Text = _configManager.RockbarSetting.MenuBackColor;
            okReserveMenuBackColorTextBox.Text = _configManager.RockbarSetting.OkReserveMenuBackColor;
            partialReserveMenuBackColorTextBox.Text = _configManager.RockbarSetting.PartialReserveMenuBackColor;
            ngReserveMenuBackColorTextBox.Text = _configManager.RockbarSetting.NgReserveMenuBackColor;
            disabledReserveMenuBackColorTextBox.Text = _configManager.RockbarSetting.DisabledReserveMenuBackColor;

            // プレビューへ反映
            previewMenuListView.Font = (Font)_fontConverter.ConvertFromString(_configManager.RockbarSetting.MenuFont);

            previewMenuListView.BackColor = (Color)_colorConverter.ConvertFromString(_configManager.RockbarSetting.MenuBackColor);
            previewMenuListView.Items[1].BackColor = (Color)_colorConverter.ConvertFromString(_configManager.RockbarSetting.OkReserveMenuBackColor);
            previewMenuListView.Items[2].BackColor = (Color)_colorConverter.ConvertFromString(_configManager.RockbarSetting.PartialReserveMenuBackColor);
            previewMenuListView.Items[3].BackColor = (Color)_colorConverter.ConvertFromString(_configManager.RockbarSetting.NgReserveMenuBackColor);
            previewMenuListView.Items[4].BackColor = (Color)_colorConverter.ConvertFromString(_configManager.RockbarSetting.DisabledReserveMenuBackColor);
        }

        /// <summary>
        /// フォント・色3(コントロールUI)の設定の読み込みとプレビュー反映
        /// </summary>
        private void LoadControlUiFontColor()
        {
            mainFormFontTextBox.Text = _configManager.RockbarSetting.MainFormFont;

            useMainFormFontForScalingCheckBox.Checked = _configManager.RockbarSetting.UseMainFormFontForScaling;
            useIndividualMainFormFontsCheckBox.Checked = _configManager.RockbarSetting.UseIndividualMainFormFonts;

            tabFontTextBox.Text = _configManager.RockbarSetting.TabFont;
            textBoxFontTextBox.Text = _configManager.RockbarSetting.TextBoxFont;
            buttonFontTextBox.Text = _configManager.RockbarSetting.ButtonFont;
            settingFormFontTextBox.Text = _configManager.RockbarSetting.SettingFormFont;

            // プレビューへ反映
            previewMainFormFontLabel.Font = (Font)_fontConverter.ConvertFromString(_configManager.RockbarSetting.MainFormFont);
            previewTabFontLabel.Font = (Font)_fontConverter.ConvertFromString(_configManager.RockbarSetting.TabFont);
            previewTextBoxFontLabel.Font = (Font)_fontConverter.ConvertFromString(_configManager.RockbarSetting.TextBoxFont);
            previewButtonFontLabel.Font = (Font)_fontConverter.ConvertFromString(_configManager.RockbarSetting.ButtonFont);
            previewSettingFormFontLabel.Font = (Font)_fontConverter.ConvertFromString(_configManager.RockbarSetting.SettingFormFont);

            // フォント・色3 画面の初期表示時の有効/無効状態を反映
            ApplyUseIndividualMainFormFontsCheckBoxState();
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
            if (_configManager.RockbarSetting.RecListMaxCount > recListMaxCountNumericUpDown.Maximum
                || _configManager.RockbarSetting.RecListMaxCount < recListMaxCountNumericUpDown.Minimum)
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
            Font settingFormFont = (Font)_fontConverter.ConvertFromString(settingFormFontTextBox.Text);
            this.Font = settingFormFont;

            // ListViewItemに対して明示的にFontが指定されている場合、親であるListView.Fontの変更を自動継承しないため、
            // ListViewItemに対して明示的にFontを再指定する必要がある
            // 現状selectedServiceListViewの時だけでよい(allServiceItem.Cloneをしているため必要と思われる)ため、ここで個別適用する
            foreach (ListViewItem item in selectedServiceListView.Items)
            {
                item.Font = settingFormFont;
            }
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
        /// 設定ファイルに保存し、フォームを閉じる
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void applyCloseButton_Click(object sender, EventArgs e)
        {
            // 各設定をConfigManagerに保存
            SaveAllSettings();

            // 設定ファイルに書き込み
            _configManager.SaveFromFile();

            // MainForm側に適用通知を出す
            ApplyRequested?.Invoke(this, EventArgs.Empty);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// 設定適用ボタン押下処理
        /// 設定ファイルに保存し、反映する（フォームは閉じない）
        /// </summary>
        private async void applyButton_Click(object sender, EventArgs e)
        {
            // ボタンを無効化して連続クリックを防止
            applyButton.Enabled = false;

            try
            {
                // 全ての設定をConfigManagerに保存する
                SaveAllSettings();

                // 設定ファイルに書き込み
                _configManager.SaveFromFile();

                // MainForm側に適用通知を出す
                ApplyRequested?.Invoke(this, EventArgs.Empty);

                // SettingForm自身最新状態を画面に反映
                ApplyUiFontSettings();

                // 1秒間（1000ミリ秒）待機
                await Task.Delay(1000);
            }
            finally
            {
                // 例外が発生しても必ずボタンを再度有効化する
                applyButton.Enabled = true;
            }
        }

        /// <summary>
        /// 全ての設定をConfigManagerに保存するヘルパー
        /// </summary>
        private void SaveAllSettings()
        {
            // EDCB連携設定の保存
            SaveEdcbLinkageSettings();

            // 予約動作設定の保存
            SaveReserveSettings();

            // チューナー名設定の保存
            _tunerNameListViewManager.Save();

            // 選択サービス設定の保存
            _selectedServiceListViewManager.Save();

            // お気に入りサービス設定の保存
            _favoriteServiceListViewManager.Save();

            // TVTest連携設定の保存
            SaveTvTestLinkageSettings();

            // フォント・色1(ListView)設定の保存
            SaveListViewFontColor();

            // フォント・色2(右クリックメニュー)設定の保存
            SaveContextMenuFontColor();

            // フォント・色3(コントロールUI)設定の保存
            SaveControlUiFontColor();

            // その他設定の保存
            SaveOtherSettings();
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

            // 優先度1のindexは0のため変数に反映時は+1が必要
            _configManager.RockbarSetting.RecPriority = (byte)(recPriorityComboBox.SelectedIndex + 1);
            _configManager.RockbarSetting.RecTuijyuu = recTuijyuuCheckBox.Checked;
            _configManager.RockbarSetting.RecPittari = recPittariCheckBox.Checked;

            // 設定ファイルのUseCustomRecMarginは個別にStart,EndMargineを使用するかFlag
            _configManager.RockbarSetting.UseCustomRecMargin = !useDefaultRecMarginCheckBox.Checked;
            _configManager.RockbarSetting.StartRecMargin = (int)startRecMarginNumericUpDown.Value;
            _configManager.RockbarSetting.EndRecMargin = (int)endRecMarginNumericUpDown.Value;

            // recServiceMode
            uint recServiceMode = _configManager.RockbarSetting.RecServiceMode;

            // 1ビット目(デフォルトを使用(0:ON, 1:OFF))
            // CheckBoxがChecked(True)ならビットを 0 に、Unchecked(False)ならビットを 1 にする
            recServiceMode = useDefaultRecServiceDataCheckBox.Checked
                ? (recServiceMode & ~0b0000_0001u) // 0にクリア
                : (recServiceMode | 0b0000_0001u); // 1をセット

            // 5ビット目(字幕を含める(0:OFF, 1:ON))
            // CheckBoxがChecked(True)ならビットを 1 に、Unchecked(False)ならビットを 0 にする
            recServiceMode = recServiceDataCaptionCheckBox.Checked
                ? (recServiceMode | 0b0001_0000u)   // 1をセット
                : (recServiceMode & ~0b0001_0000u); // 0にクリア

            // 6ビット目(データカルーセルを含める(0:OFF, 1:ON))
            // CheckBoxがChecked(True)ならビットを 1 に、Unchecked(False)ならビットを 0 にする
            recServiceMode = recServiceDataCarouselCheckBox.Checked
                ? (recServiceMode | 0b0010_0000u)   // 1をセット
                : (recServiceMode & ~0b0010_0000u); // 0にクリア

            _configManager.RockbarSetting.RecServiceMode = recServiceMode;
            // recServiceMode ここまで

            // 録画フォルダ
            _recFolderListViewManager.Save();

            _configManager.RockbarSetting.PartialRecSeparateFile = partialRecSeparateFileCheckBox.Checked;
            _configManager.RockbarSetting.ContinueRecSameFile = continueRecSameFileCheckBox.Checked;

            // 使用チューナーの保存
            _tunerComboBoxManager.Save();

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
        /// フォント・色1(ListView)の設定の保存
        /// </summary>
        private void SaveListViewFontColor()
        {
            _configManager.RockbarSetting.ListFont = listFontTextBox.Text;
            _configManager.RockbarSetting.FormBackColor = formBackColorTextBox.Text;
            _configManager.RockbarSetting.ListForeColor = listForeColorTextBox.Text;
            _configManager.RockbarSetting.ListBackColor = listBackColorTextBox.Text;
            _configManager.RockbarSetting.OkReserveListBackColor = okReserveListBackColorTextBox.Text;
            _configManager.RockbarSetting.PartialReserveListBackColor = partialReserveListBackColorTextBox.Text;
            _configManager.RockbarSetting.NgReserveListBackColor = ngReserveListBackColorTextBox.Text;
            _configManager.RockbarSetting.DisabledReserveListBackColor = disabledReserveListBackColorTextBox.Text;
            _configManager.RockbarSetting.ListHeaderForeColor = listHeaderForeColorTextBox.Text;
            _configManager.RockbarSetting.ListHeaderBackColor = listHeaderBackColorTextBox.Text;
        }

        /// <summary>
        /// フォント・色2(右クリックメニュー)の設定の保存
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
        /// フォント・色3(コントロールUI)の設定の保存
        /// </summary>
        private void SaveControlUiFontColor()
        {
            _configManager.RockbarSetting.MainFormFont = mainFormFontTextBox.Text;
            _configManager.RockbarSetting.UseMainFormFontForScaling = useMainFormFontForScalingCheckBox.Checked;
            _configManager.RockbarSetting.UseIndividualMainFormFonts = useIndividualMainFormFontsCheckBox.Checked;
            _configManager.RockbarSetting.TabFont = tabFontTextBox.Text;
            _configManager.RockbarSetting.TextBoxFont = textBoxFontTextBox.Text;
            _configManager.RockbarSetting.ButtonFont = buttonFontTextBox.Text;
            _configManager.RockbarSetting.SettingFormFont = settingFormFontTextBox.Text;
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

            _configManager.RockbarSetting.TaskTrayIconLeftClick = taskTrayIconLeftClickComboBox.SelectedItem.ToString() ?? "";
            _configManager.RockbarSetting.TaskTrayIconLeftDoubleClick = taskTrayIconLeftDoubleClickComboBox.SelectedItem.ToString() ?? "";
            _configManager.RockbarSetting.TaskTrayIconRightClick = taskTrayIconRightClickComboBox.SelectedItem.ToString() ?? "";
            _configManager.RockbarSetting.TaskTrayIconRightDoubleClick = taskTrayIconRightDoubleClickComboBox.SelectedItem.ToString() ?? "";
        }

        // 「EDCBとの通信にTCP/IPを使用する」チェックボックスの変更検知
        private void useTcpIpCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ApplyUseTcpIpCheckBoxState();
        }

        //// 「EDCBとの通信にTCP/IPを使用する」チェックボックスがOFFの場合は、予約内容全体が入力不可
        private void ApplyUseTcpIpCheckBoxState()
        {
            ipAddressTextBox.Enabled = useTcpIpCheckBox.Checked;
            portNumberNumericUpDown.Enabled = useTcpIpCheckBox.Checked;
        }

        // 「Web番組表機能(WebUI)を使用する」チェックボックスの変更検知
        private void useWebLinkCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ApplyUseWebLinkCheckBoxState();
        }

        //// 「Web番組表機能(WebUI)を使用する」チェックボックスがOFFの場合は、URL全体が入力不可
        private void ApplyUseWebLinkCheckBoxState()
        {
            webEpgUrlTextBox.Enabled = useWebLinkCheckBox.Checked;
            webLinkUrlTextBox.Enabled = useWebLinkCheckBox.Checked;
            recInfoWebLinkUrlTextBox.Enabled = useWebLinkCheckBox.Checked;
        }

        // 「予約追加」チェックボックスの変更検知
        private void useRockbarReserveAddCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ApplyUseRockbarReserveAddCheckBoxState();
        }

        // 「予約追加」チェックボックスがONの場合は、予約内容全体が入力不可
        private void ApplyUseRockbarReserveAddCheckBoxState()
        {
            // チェックが入っていない場合は GroupBox 全体を編集不可 (Enabled = false)
            edcbReserveSettingGroupBox.Enabled = useRockbarReserveAddCheckBox.Checked;
        }

        // 「予約削除」チェックボックスの変更検知
        private void useRockbarReserveDelCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ApplyUseRockbarReserveDelCheckBoxState();
        }

        // 「予約削除」チェックボックスがONの場合は、「削除前確認表示」が入力不可
        private void ApplyUseRockbarReserveDelCheckBoxState()
        {
            // チェックされている場合は入力不可 (Enabled = false)
            bool isInputEnabled = useRockbarReserveDelCheckBox.Checked;

            useRockbarReserveDelConfirmCheckBox.Enabled = isInputEnabled;
        }

        // 録画マージンのデフォルトチェックボックスの変更検知
        private void defaultMarginCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ApplyDefaultMarginCheckBoxState();
        }

        // 録画マージンのデフォルトチェックボックスがONの場合は開始、終了の入力は不可
        private void ApplyDefaultMarginCheckBoxState()
        {
            // チェックされている場合は入力不可 (Enabled = false)
            bool isInputEnabled = !useDefaultRecMarginCheckBox.Checked;

            startRecMarginNumericUpDown.Enabled = isInputEnabled;
            endRecMarginNumericUpDown.Enabled = isInputEnabled;
        }

        // サービス対象データのデフォルトチェックボックスの変更検知
        private void defaultServiceDataCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ApplyDefaultServiceDataCheckBoxState();
        }

        // サービス対象データの「デフォルト」チェックボックスがONの場合は、
        // 「字幕を含める」と「データカルーセルを含める」は入力不可
        private void ApplyDefaultServiceDataCheckBoxState()
        {
            // チェックされている場合は入力不可 (Enabled = false)
            bool isInputEnabled = !useDefaultRecServiceDataCheckBox.Checked;

            recServiceDataCaptionCheckBox.Enabled = isInputEnabled;
            recServiceDataCarouselCheckBox.Enabled = isInputEnabled;
        }

        // 録画後動作のデフォルトチェックボックスの変更検知
        private void defaultPostRecActionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ApplyDefaultPostRecActionCheckBoxState();
        }

        // 録画後動作の「デフォルト」チェックボックスがONの場合は、
        // 各ラジオボタンと「復帰後再起動する」チェックボックスは入力不可
        private void ApplyDefaultPostRecActionCheckBoxState()
        {
            // チェックが入っている場合は編集不可 (Enabled = false)
            bool isInputEnabled = !defaultSuspendModeAfterRecCheckBox.Checked;

            afterRecNoActionRadioButton.Enabled = isInputEnabled;
            afterRecStandbyRadioButton.Enabled = isInputEnabled;
            afterRecSuspendRadioButton.Enabled = isInputEnabled;
            afterRecShutdownRadioButton.Enabled = isInputEnabled;
            rebootAfterReturnCheckBox.Enabled = isInputEnabled;
        }

        // 「予約時間に合わせてTVTestを自動起動／終了する」チェックボックスの変更検知
        private void isAutoOpenTvtestCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ApplyIsAutoOpenTvtestCheckBoxState();
        }

        //// 「予約時間に合わせてTVTestを自動起動／終了する」チェックボックスがOFFの場合は設定不可
        private void ApplyIsAutoOpenTvtestCheckBoxState()
        {
            autoStartTargetGroupBox.Enabled = isAutoOpenTvtestCheckBox.Checked;
            autoOpenMarginNumericUpDown.Enabled = isAutoOpenTvtestCheckBox.Checked;
            autoCloseMarginNumericUpDown.Enabled = isAutoOpenTvtestCheckBox.Checked;
        }

        // 「メイン画面のフォントを個別に指定する」チェックボックスの変更検知
        private void useIndividualMainFormFontsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ApplyUseIndividualMainFormFontsCheckBoxState();
        }

        // 「メイン画面のフォントを個別に指定する」チェックボックスがONの場合は、全体が設定不可
        private void ApplyUseIndividualMainFormFontsCheckBoxState()
        {
            // チェックが入っていない場合は GroupBox 全体を編集不可
            individualMainFormFontsGroupBox.Enabled = useIndividualMainFormFontsCheckBox.Checked;
        }

        /// <summary>
        /// 録画フォルダ追加処理
        /// </summary>
        private void addRecFolderButton_Click(object sender, EventArgs e)
        {
            _recFolderListViewManager.AddFolder();
        }

        /// <summary>
        /// 録画フォルダ変更処理
        /// </summary>
        private void editRecFolderButton_Click(object sender, EventArgs e)
        {
            _recFolderListViewManager.EditFolder();
        }

        // 録画フォルダListViewダブルクリック動作
        private void recFolderListView_DoubleClick(object sender, EventArgs e)
        {
            _recFolderListViewManager.EditFolder();
        }

        /// <summary>
        /// 録画フォルダコピー処理
        /// </summary>
        private void copyRecFolderButton_Click(object sender, EventArgs e)
        {
            _recFolderListViewManager.CopySelectedFolder();
        }

        /// <summary>
        /// 録画フォルダ削除処理
        /// </summary>
        private void delRecFolderButton_Click(object sender, EventArgs e)
        {
            _recFolderListViewManager.DeleteSelectedFolder();
        }

        /// <summary>
        /// バッチファイル参照ボタン処理
        /// </summary>
        private void browseBatFileButton_Click(object sender, EventArgs e)
        {
            SettingFormReservationHelper.BrowseScriptFile(recBatFilePathTextBox);
        }

        /// <summary>
        /// 選択サービス追加処理
        /// </summary>
        private void addSelectedServiceButton_Click(object sender, EventArgs e)
        {
            _selectedServiceListViewManager.AddService();
        }

        /// <summary>
        /// お気に入りサービス追加処理
        /// </summary>
        private void addFavoriteServiceButton_Click(object sender, EventArgs e)
        {
            _favoriteServiceListViewManager.AddService();
        }

        /// <summary>
        /// 選択サービス削除処理
        /// </summary>
        private void removeServiceButton_Click(object sender, EventArgs e)
        {
            _selectedServiceListViewManager.RemoveService();
        }

        /// <summary>
        /// お気に入りサービス削除処理
        /// </summary>
        private void removeFavoriteServiceButton_Click(object sender, EventArgs e)
        {
            _favoriteServiceListViewManager.RemoveService();
        }

        /// <summary>
        /// 選択サービス上移動処理
        /// </summary>
        private void moveUpSelectedServiceButton_Click(object sender, EventArgs e)
        {
            _selectedServiceListViewManager.MoveUp();
        }

        /// <summary>
        /// お気に入りサービス上移動処理
        /// </summary>
        private void moveUpFavoriteServiceButton_Click(object sender, EventArgs e)
        {
            _favoriteServiceListViewManager.MoveUp();
        }

        /// <summary>
        /// 選択サービス下移動処理
        /// </summary>
        private void moveDownSelectedServiceButton_Click(object sender, EventArgs e)
        {
            _selectedServiceListViewManager.MoveDown();
        }

        /// <summary>
        /// お気に入りサービス下移動処理
        /// </summary>
        private void moveDownFavoriteServiceButton_Click(object sender, EventArgs e)
        {
            _favoriteServiceListViewManager.MoveDown();
        }

        /// <summary>
        /// 選択サービス新規追加処理
        /// </summary>
        private void addNewServiceButton_Click(object sender, EventArgs e)
        {
            _selectedServiceListViewManager.AddNewService();
        }

        /// <summary>
        /// 選択サービス編集処理
        /// </summary>
        private void editServiceButton_Click(object sender, EventArgs e)
        {
            _selectedServiceListViewManager.EditService();
        }

        /// <summary>
        /// 選択サービス編集処理(ダブルクリック)
        /// </summary>
        private void selectedServiceListView_DoubleClick(object sender, EventArgs e)
        {
            _selectedServiceListViewManager.EditService();
        }

        /// <summary>
        /// 設定タブ切り替え処理
        /// </summary>
        private void settingTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (settingTabControl.SelectedTab == favoriteServiceTabPage)
            {
                _favoriteServiceListViewManager.SyncAndRefresh();
            }
        }

        /// <summary>
        /// 全サービス一覧のカラムヘッダクリック処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void allServiceListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {

            _selectedServiceListViewManager.SortAllServiceList(e.Column);
        }

        /// <summary>
        /// 選択サービス一覧のカラムヘッダクリック処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectedServiceListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            _selectedServiceListViewManager.SortSelectedServiceList(e.Column);
        }

        /// <summary>
        /// 選択サービス一覧(お気に入りサービスタブ)のカラムヘッダクリック処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void selectedServiceListView2_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            _favoriteServiceListViewManager.SortSelectedService2List(e.Column);
        }

        /// <summary>
        /// お気に入りサービス一覧のカラムヘッダクリック処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void favoriteServiceListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            _favoriteServiceListViewManager.SortFavoriteServiceList(e.Column);
        }

        /// <summary>
        /// チューナー名一覧のカラムヘッダクリック処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントパラメータ</param>
        private void tunerNameListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            _tunerNameListViewManager.Sort(e.Column);
        }

        /// <summary>
        /// チューナー名選択項目変更処理
        /// </summary>
        private void tunerNameListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            // テキストボックスに選択したチューナー名を表示
            _tunerNameListViewManager.DisplaySelectedNameTo(tunerNameTextBox);
        }

        /// <summary>
        /// チューナー名更新ボタン押下処理
        /// </summary>
        private void updateTunerNameButton_Click(object sender, EventArgs e)
        {
            // ListViewに反映
            _tunerNameListViewManager.UpdateSelectedNameFrom(tunerNameTextBox);
        }

        /// <summary>
        /// BonDriver削除処理
        /// </summary>
        private void deleteBonDriverButton_Click(object sender, EventArgs e)
        {
            _tunerNameListViewManager.DeleteSelectedBonDriver();
        }

        /// <summary>
        /// TVTest参照ボタン押下処理
        /// ファイル選択ダイアログを開く。
        /// </summary>
        private void tvtestOpenButton_Click(object sender, EventArgs e)
        {
            tvtestOpenFileDialog.ShowDialog();
        }

        /// <summary>
        /// ファイル選択ダイアログ選択完了処理
        /// TVTest.exeパスを設定する。
        /// </summary>
        private void tvtestOpenFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            tvtestPathTextBox.Text = tvtestOpenFileDialog.FileName;
        }

        /// <summary>
        /// リストフォント選択ボタン押下処理
        /// </summary>
        private void selectListFontButton_Click(object sender, EventArgs e)
        {
            SelectFontFor(listFontTextBox,
                font => previewListView.Font = font);
        }

        /// <summary>
        /// フォーム背景色選択ボタン押下処理
        /// </summary>
        private void selectFormBackColorButton_Click(object sender, EventArgs e)
        {
            SelectColorFor(formBackColorTextBox,
                color => previewFormPanel.BackColor = color);
        }

        /// <summary>
        /// リスト文字色選択ボタン押下処理
        /// </summary>
        private void selectListForeColorButton_Click(object sender, EventArgs e)
        {
            SelectColorFor(listForeColorTextBox,
                color => previewListView.ForeColor = color);
        }

        /// <summary>
        /// リスト背景色選択ボタン押下処理
        /// </summary>
        private void selectListBackColorButton_Click(object sender, EventArgs e)
        {
            SelectColorFor(listBackColorTextBox,
                color => previewListView.BackColor = color);
        }

        /// <summary>
        /// リスト背景色(正常予約)選択ボタン押下処理
        /// </summary>
        private void selectOkReserveListBackColorButton_Click(object sender, EventArgs e)
        {
            SelectColorFor(okReserveListBackColorTextBox,
                color => previewListView.Items[1].BackColor = color);
        }

        /// <summary>
        /// リスト背景色(部分予約)選択ボタン押下処理
        /// </summary>
        private void selectPartialReserveListBackColorButton_Click(object sender, EventArgs e)
        {
            SelectColorFor(partialReserveListBackColorTextBox,
                color => previewListView.Items[2].BackColor = color);
        }

        /// <summary>
        /// リスト背景色(予約不可)選択ボタン押下処理
        /// </summary>
        private void selectNgReserveListBackColorButton_Click(object sender, EventArgs e)
        {
            SelectColorFor(ngReserveListBackColorTextBox,
                color => previewListView.Items[3].BackColor = color);
        }

        /// <summary>
        /// リスト背景色(無効予約)選択ボタン押下処理
        /// </summary>
        private void selectDisabledReserveListBackColorButton_Click(object sender, EventArgs e)
        {
            SelectColorFor(disabledReserveListBackColorTextBox,
                color => previewListView.Items[4].BackColor = color);
        }

        /// <summary>
        /// 予約一覧ヘッダ文字色選択ボタン押下処理
        /// </summary>
        private void selectListHeaderForeColorButton_Click(object sender, EventArgs e)
        {
            SelectColorFor(listHeaderForeColorTextBox,
                color => previewListView.Items[5].ForeColor = color);
        }

        /// <summary>
        /// 予約一覧ヘッダ背景色選択ボタン押下処理
        /// </summary>
        private void selectListHeaderBackColorButton_Click(object sender, EventArgs e)
        {
            SelectColorFor(listHeaderBackColorTextBox,
                color => previewListView.Items[5].BackColor = color);
        }

        /// <summary>
        /// メニューフォント選択ボタン押下処理
        /// </summary>
        private void selectMenuFontButton_Click(object sender, EventArgs e)
        {
            SelectFontFor(menuFontTextBox,
                font => previewMenuListView.Font = font);
        }

        /// <summary>
        /// メニュー背景色選択ボタン押下処理
        /// </summary>
        private void selectMenuBackColorButton_Click(object sender, EventArgs e)
        {
            SelectColorFor(menuBackColorTextBox,
                color => previewMenuListView.BackColor = color);
        }

        /// <summary>
        /// メニュー背景色(正常予約)選択ボタン押下処理
        /// </summary>
        private void selectOkReserveMenuBackColorButton_Click(object sender, EventArgs e)
        {
            SelectColorFor(okReserveMenuBackColorTextBox,
                color => previewMenuListView.Items[1].BackColor = color);
        }

        /// <summary>
        /// メニュー背景色(部分予約)選択ボタン押下処理
        /// </summary>
        private void selectPartialReserveMenuBackColorButton_Click(object sender, EventArgs e)
        {
            SelectColorFor(partialReserveMenuBackColorTextBox,
                color => previewMenuListView.Items[2].BackColor = color);
        }

        /// <summary>
        /// メニュー背景色(予約不可)選択ボタン押下処理
        /// </summary>
        private void selectNgReserveMenuBackColorButton_Click(object sender, EventArgs e)
        {
            SelectColorFor(ngReserveMenuBackColorTextBox,
                color => previewMenuListView.Items[3].BackColor = color);
        }

        /// <summary>
        /// メニュー背景色(無効予約)選択ボタン押下処理
        /// </summary>
        private void selectDisabledReserveMenuBackColorButton_Click(object sender, EventArgs e)
        {
            SelectColorFor(disabledReserveMenuBackColorTextBox,
                color => previewMenuListView.Items[4].BackColor = color);
        }

        /// <summary>
        /// メイン画面フォント選択ボタン押下処理
        /// </summary>
        private void selectMainFormFontButton_Click(object sender, EventArgs e)
        {
            SelectFontFor(mainFormFontTextBox,
                font => previewMainFormFontLabel.Font = font);
        }

        /// <summary>
        /// タブのフォント選択ボタン押下処理
        /// </summary>
        private void selectTabFontButton_Click(object sender, EventArgs e)
        {
            SelectFontFor(tabFontTextBox,
                font => previewTabFontLabel.Font = font);
        }

        /// <summary>
        /// テキストボックスのフォント選択ボタン押下処理
        /// </summary>
        private void selectTextBoxFontButton_Click(object sender, EventArgs e)
        {
            SelectFontFor(textBoxFontTextBox,
                font => previewTextBoxFontLabel.Font = font);
        }

        /// <summary>
        /// ボタンのフォント選択ボタン押下処理
        /// </summary>
        private void selectButtonFontButton_Click(object sender, EventArgs e)
        {
            SelectFontFor(buttonFontTextBox,
                font => previewButtonFontLabel.Font = font);
        }

        /// <summary>
        /// 設定画面フォント選択ボタン押下処理
        /// </summary>
        private void selectSettingFormFontButton_Click(object sender, EventArgs e)
        {
            SelectFontFor(settingFormFontTextBox,
                font => previewSettingFormFontLabel.Font = font);
        }

        /// <summary>
        /// フォント選択ダイアログを表示し、テキストボックスとプレビューに反映するヘルパー
        /// </summary>
        /// <param name="targetTextBox">設定文字列を保持するTextBox</param>
        /// <param name="applyPreviewAction">プレビューコントロール等への適用アクション</param>
        private void SelectFontFor(TextBox targetTextBox, Action<Font> applyPreviewAction = null)
        {
            if (!string.IsNullOrEmpty(targetTextBox.Text))
            {
                try
                {
                    fontDialog.Font = (Font)_fontConverter.ConvertFromString(targetTextBox.Text);
                }
                catch { }
            }

            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                targetTextBox.Text = _fontConverter.ConvertToString(fontDialog.Font);
                applyPreviewAction?.Invoke(fontDialog.Font);
            }
        }

        /// <summary>
        /// 色選択ダイアログを表示し、テキストボックスとプレビューに反映するヘルパー
        /// </summary>
        /// <param name="targetTextBox">設定文字列を保持するTextBox</param>
        /// <param name="applyPreviewAction">プレビューコントロール等への適用アクション</param>
        private void SelectColorFor(TextBox targetTextBox, Action<Color> applyPreviewAction = null)
        {
            if (!string.IsNullOrEmpty(targetTextBox.Text))
            {
                try
                {
                    colorDialog.Color = (Color)_colorConverter.ConvertFromString(targetTextBox.Text);
                }
                catch { }
            }

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                targetTextBox.Text = _colorConverter.ConvertToString(colorDialog.Color);
                applyPreviewAction?.Invoke(colorDialog.Color);
            }
        }
    }
}
