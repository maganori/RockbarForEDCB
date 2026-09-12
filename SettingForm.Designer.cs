
namespace RockbarForEDCB
{
    partial class SettingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingForm));
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "チャンネル　00:00-00:00　　　通常番組",
            "test",
            "test",
            "test"}, -1);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("チャンネル　00:00-00:00　◎　正常予約番組");
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("チャンネル　00:00-00:00　欠　部分予約番組");
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("チャンネル　00:00-00:00　×　予約不可番組");
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("チャンネル　00:00-00:00　無　無効予約番組");
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("チャンネル　00:00-00:00　　　予約一覧ヘッダ");
            System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem(new string[] {
            "01/01 00:00～00:00  　  通常番組",
            "test",
            "test",
            "test"}, -1);
            System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("01/01 00:00～00:00  ◎  正常予約番組");
            System.Windows.Forms.ListViewItem listViewItem9 = new System.Windows.Forms.ListViewItem("01/01 00:00～00:00  欠  部分予約番組");
            System.Windows.Forms.ListViewItem listViewItem10 = new System.Windows.Forms.ListViewItem("01/01 00:00～00:00  ×  予約不可番組");
            System.Windows.Forms.ListViewItem listViewItem11 = new System.Windows.Forms.ListViewItem("01/01 00:00～00:00  無  無効予約番組");
            this.cancelButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.allServiceListView = new System.Windows.Forms.ListView();
            this.allServiceMarkColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.allServiceNetworkTypeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.allServiceNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.allServiceTsidColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.allServiceSidColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.addSelectedServiceButton = new System.Windows.Forms.Button();
            this.addNewServiceButton = new System.Windows.Forms.Button();
            this.editServiceButton = new System.Windows.Forms.Button();
            this.removeSelectedServiceButton = new System.Windows.Forms.Button();
            this.moveDownSelectedServiceButton = new System.Windows.Forms.Button();
            this.moveUpSelectedServiceButton = new System.Windows.Forms.Button();
            this.selectedServiceListView = new System.Windows.Forms.ListView();
            this.selectedServiceMarkColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.selectedServiceNetworkTypeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.selectedServiceNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.selectedServiceTsidColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.selectedServiceSidColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.selectedServiceTvtestOptionColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.settingTabControl = new System.Windows.Forms.TabControl();
            this.edcbLinkageTabPage = new System.Windows.Forms.TabPage();
            this.webEPGLabel = new System.Windows.Forms.Label();
            this.webEpgUrlTextBox = new System.Windows.Forms.TextBox();
            this.portNumberNoteLabel = new System.Windows.Forms.Label();
            this.portNumberNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.portNumberLabel = new System.Windows.Forms.Label();
            this.webLinkUrlExampleLabel = new System.Windows.Forms.Label();
            this.useTcpIpCheckBox = new System.Windows.Forms.CheckBox();
            this.useWebLinkCheckBox = new System.Windows.Forms.CheckBox();
            this.ipAddressLabel = new System.Windows.Forms.Label();
            this.webLinkUrlLabel = new System.Windows.Forms.Label();
            this.recInfoWebLinkUrlLabel = new System.Windows.Forms.Label();
            this.ipAddressTextBox = new System.Windows.Forms.TextBox();
            this.webLinkUrlTextBox = new System.Windows.Forms.TextBox();
            this.recInfoWebLinkUrlTextBox = new System.Windows.Forms.TextBox();
            this.reserveTabPage = new System.Windows.Forms.TabPage();
            this.useRockbarReserveLabel = new System.Windows.Forms.Label();
            this.useRockbarReserveDelConfirmCheckBox = new System.Windows.Forms.CheckBox();
            this.useRockbarReserveDelCheckBox = new System.Windows.Forms.CheckBox();
            this.useRockbarReserveModCheckBox = new System.Windows.Forms.CheckBox();
            this.edcbReserveSettingGroupBox = new System.Windows.Forms.GroupBox();
            this.recCommentTextBox = new System.Windows.Forms.TextBox();
            this.recCommentLabel = new System.Windows.Forms.Label();
            this.prioritizeViewLabel = new System.Windows.Forms.Label();
            this.prioritizeViewPanel = new System.Windows.Forms.Panel();
            this.prioritizeRecRadioButton = new System.Windows.Forms.RadioButton();
            this.prioritizeViewRadioButton = new System.Windows.Forms.RadioButton();
            this.suspendModeAfterRecPanel = new System.Windows.Forms.Panel();
            this.afterRecNoActionRadioButton = new System.Windows.Forms.RadioButton();
            this.afterRecStandbyRadioButton = new System.Windows.Forms.RadioButton();
            this.afterRecSuspendRadioButton = new System.Windows.Forms.RadioButton();
            this.afterRecShutdownRadioButton = new System.Windows.Forms.RadioButton();
            this.endRecMarginNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.startRecMarginNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.browseBatFileButton = new System.Windows.Forms.Button();
            this.deleteRecFolderButton = new System.Windows.Forms.Button();
            this.copyRecFolderButton = new System.Windows.Forms.Button();
            this.editRecFolderButton = new System.Windows.Forms.Button();
            this.addRecFolderButton = new System.Windows.Forms.Button();
            this.enableReserveCheckBox = new System.Windows.Forms.CheckBox();
            this.recTagTextBox = new System.Windows.Forms.TextBox();
            this.recPriorityComboBox = new System.Windows.Forms.ComboBox();
            this.recBatFilePathTextBox = new System.Windows.Forms.TextBox();
            this.recModeComboBox = new System.Windows.Forms.ComboBox();
            this.recTagLabel = new System.Windows.Forms.Label();
            this.recModeLabel = new System.Windows.Forms.Label();
            this.recBatFilePathLabel = new System.Windows.Forms.Label();
            this.recTuijyuuCheckBox = new System.Windows.Forms.CheckBox();
            this.rebootAfterReturnCheckBox = new System.Windows.Forms.CheckBox();
            this.recPittariCheckBox = new System.Windows.Forms.CheckBox();
            this.recPriorityLabel = new System.Windows.Forms.Label();
            this.recMarginLabel = new System.Windows.Forms.Label();
            this.useDefaultRecMarginCheckBox = new System.Windows.Forms.CheckBox();
            this.startRecMarginLabel = new System.Windows.Forms.Label();
            this.defaultSuspendModeAfterRecCheckBox = new System.Windows.Forms.CheckBox();
            this.suspendModeAfterRecLabel = new System.Windows.Forms.Label();
            this.endRecMarginLabel = new System.Windows.Forms.Label();
            this.recTunerIdComboBox = new System.Windows.Forms.ComboBox();
            this.recTunerIdLabel = new System.Windows.Forms.Label();
            this.recServiceDataCarouselCheckBox = new System.Windows.Forms.CheckBox();
            this.continueRecSameFileCheckBox = new System.Windows.Forms.CheckBox();
            this.recServiceDataLabel = new System.Windows.Forms.Label();
            this.partialRecSeparateFileCheckBox = new System.Windows.Forms.CheckBox();
            this.useDefaultRecServiceDataCheckBox = new System.Windows.Forms.CheckBox();
            this.recFolderListView = new System.Windows.Forms.ListView();
            this.partialRecColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.recFolderColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.writePlugInColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.recNamePlugInColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.recServiceDataCaptionCheckBox = new System.Windows.Forms.CheckBox();
            this.useRockbarReserveAddCheckBox = new System.Windows.Forms.CheckBox();
            this.tunerTabPage = new System.Windows.Forms.TabPage();
            this.tunerNameLabel = new System.Windows.Forms.Label();
            this.tunerNameNoteLabel = new System.Windows.Forms.Label();
            this.updateTunerNameButton = new System.Windows.Forms.Button();
            this.tunerNameTextBox = new System.Windows.Forms.TextBox();
            this.tunerNameListView = new System.Windows.Forms.ListView();
            this.tunerNameMarkColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tunerNameTunerIdColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tunerNameBonDriverNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tunerNameTunerNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.allServiceTabPage = new System.Windows.Forms.TabPage();
            this.selectedServiceListLabel = new System.Windows.Forms.Label();
            this.allServiceListLabel = new System.Windows.Forms.Label();
            this.favoriteServiceTabPage = new System.Windows.Forms.TabPage();
            this.favoriteServiceListLabel = new System.Windows.Forms.Label();
            this.selectedServiceList2Label = new System.Windows.Forms.Label();
            this.selectedServiceListView2 = new System.Windows.Forms.ListView();
            this.selectedService2MarkColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.selectedService2NetworkTypeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.selectedService2NameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.selectedService2TsidColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.selectedService2SidColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.selectedService2TvtestOptionColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.moveUpFavoriteServiceButton = new System.Windows.Forms.Button();
            this.moveDownFavoriteServiceButton = new System.Windows.Forms.Button();
            this.removeFavoriteServiceButton = new System.Windows.Forms.Button();
            this.favoriteServiceListView = new System.Windows.Forms.ListView();
            this.favoriteServiceMarkColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.favoriteServiceNetworkTypeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.favoriteServiceNameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.favoriteServiceTsidColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.favoriteServiceSidColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.favoriteServiceTvtestOptionColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.addFavoriteServiceButton = new System.Windows.Forms.Button();
            this.tvtestLinkageTabPage = new System.Windows.Forms.TabPage();
            this.tvtestBs4kOptionLabel = new System.Windows.Forms.Label();
            this.tvtestSphdOptionLabel = new System.Windows.Forms.Label();
            this.tvtestBs4kOptionTextBox = new System.Windows.Forms.TextBox();
            this.tvtestSphdOptionTextBox = new System.Windows.Forms.TextBox();
            this.tvtestCatvOptionTextBox = new System.Windows.Forms.TextBox();
            this.tvtestCatvOptionLabel = new System.Windows.Forms.Label();
            this.tvtestOptionExampleLabel = new System.Windows.Forms.Label();
            this.tvtestTsFileOptionExampleLabel = new System.Windows.Forms.Label();
            this.autoStartTargetGroupBox = new System.Windows.Forms.GroupBox();
            this.isAutoOpenSphdCheckBox = new System.Windows.Forms.CheckBox();
            this.isAutoOpenBs4kCheckBox = new System.Windows.Forms.CheckBox();
            this.isAutoOpenCatvCheckBox = new System.Windows.Forms.CheckBox();
            this.isAutoOpenFavoriteServiceCheckBox = new System.Windows.Forms.CheckBox();
            this.isAutoOpenDttvCheckBox = new System.Windows.Forms.CheckBox();
            this.isAutoOpenCsCheckBox = new System.Windows.Forms.CheckBox();
            this.isAutoOpenBsCheckBox = new System.Windows.Forms.CheckBox();
            this.autoCloseMarginNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.autoCloseMarginLabel = new System.Windows.Forms.Label();
            this.autoOpenMarginNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.autoOpenMarginLabel = new System.Windows.Forms.Label();
            this.isAutoOpenTvtestCheckBox = new System.Windows.Forms.CheckBox();
            this.tvTestNoteLabel = new System.Windows.Forms.Label();
            this.useDoubleClickTvtestCheckBox = new System.Windows.Forms.CheckBox();
            this.tvtestDttvOptionLabel = new System.Windows.Forms.Label();
            this.tvtestDttvOptionTextBox = new System.Windows.Forms.TextBox();
            this.tvtestBscsOptionLabel = new System.Windows.Forms.Label();
            this.tvtestBscsOptionTextBox = new System.Windows.Forms.TextBox();
            this.tvtestTsFileOptionLabel = new System.Windows.Forms.Label();
            this.tvtestTsFileOptionTextBox = new System.Windows.Forms.TextBox();
            this.tvtestPathLabel = new System.Windows.Forms.Label();
            this.tvtestOpenButton = new System.Windows.Forms.Button();
            this.tvtestPathTextBox = new System.Windows.Forms.TextBox();
            this.listViewContColorTabPage = new System.Windows.Forms.TabPage();
            this.ngReserveListBackColorLabel = new System.Windows.Forms.Label();
            this.ngReserveListBackColorTextBox = new System.Windows.Forms.TextBox();
            this.selectNgReserveListBackColorButton = new System.Windows.Forms.Button();
            this.partialReserveListBackColorLabel = new System.Windows.Forms.Label();
            this.partialReserveListBackColorTextBox = new System.Windows.Forms.TextBox();
            this.selectPartialReserveListBackColorButton = new System.Windows.Forms.Button();
            this.okReserveListBackColorLabel = new System.Windows.Forms.Label();
            this.okReserveListBackColorTextBox = new System.Windows.Forms.TextBox();
            this.selectOkReserveListBackColorButton = new System.Windows.Forms.Button();
            this.disabledReserveListBackColorLabel = new System.Windows.Forms.Label();
            this.disabledReserveListBackColorTextBox = new System.Windows.Forms.TextBox();
            this.selectDisabledReserveListBackColorButton = new System.Windows.Forms.Button();
            this.listHeaderForeColorLabel = new System.Windows.Forms.Label();
            this.listHeaderForeColorTextBox = new System.Windows.Forms.TextBox();
            this.selectListHeaderForeColorButton = new System.Windows.Forms.Button();
            this.listHeaderBackColorLabel = new System.Windows.Forms.Label();
            this.listHeaderBackColorTextBox = new System.Windows.Forms.TextBox();
            this.selectListHeaderBackColorButton = new System.Windows.Forms.Button();
            this.previewListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.previewLabel = new System.Windows.Forms.Label();
            this.foreColorLabel = new System.Windows.Forms.Label();
            this.foreColorTextBox = new System.Windows.Forms.TextBox();
            this.selectForeColorButton = new System.Windows.Forms.Button();
            this.listBackColorLabel = new System.Windows.Forms.Label();
            this.listBackColorTextBox = new System.Windows.Forms.TextBox();
            this.selectListBackColorButton = new System.Windows.Forms.Button();
            this.previewFormPanel = new System.Windows.Forms.Panel();
            this.formBackColorLabel = new System.Windows.Forms.Label();
            this.formBackColorTextBox = new System.Windows.Forms.TextBox();
            this.selectFormBackColorButton = new System.Windows.Forms.Button();
            this.fontLabel = new System.Windows.Forms.Label();
            this.fontTextBox = new System.Windows.Forms.TextBox();
            this.selectFontButton = new System.Windows.Forms.Button();
            this.contextMenuFontColorTabPage = new System.Windows.Forms.TabPage();
            this.ngReserveMenuBackColorLabel = new System.Windows.Forms.Label();
            this.ngReserveMenuBackColorTextBox = new System.Windows.Forms.TextBox();
            this.selectNgReserveMenuBackColorButton = new System.Windows.Forms.Button();
            this.partialReserveMenuBackColorLabel = new System.Windows.Forms.Label();
            this.partialReserveMenuBackColorTextBox = new System.Windows.Forms.TextBox();
            this.selectPartialReserveMenuBackColorButton = new System.Windows.Forms.Button();
            this.okReserveMenuBackColorLabel = new System.Windows.Forms.Label();
            this.okReserveMenuBackColorTextBox = new System.Windows.Forms.TextBox();
            this.selectOkReserveMenuBackColorButton = new System.Windows.Forms.Button();
            this.disabledReserveMenuBackColorLabel = new System.Windows.Forms.Label();
            this.disabledReserveMenuBackColorTextBox = new System.Windows.Forms.TextBox();
            this.selectDisabledReserveMenuBackColorButton = new System.Windows.Forms.Button();
            this.previewMenuListView = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label5 = new System.Windows.Forms.Label();
            this.menuBackColorLabel = new System.Windows.Forms.Label();
            this.menuBackColorTextBox = new System.Windows.Forms.TextBox();
            this.selectMenuBackColorButton = new System.Windows.Forms.Button();
            this.menuFontLabel = new System.Windows.Forms.Label();
            this.menuFontTextBox = new System.Windows.Forms.TextBox();
            this.selectMenuFontButton = new System.Windows.Forms.Button();
            this.controlUiFontColorTabPage = new System.Windows.Forms.TabPage();
            this.tabFontLabel = new System.Windows.Forms.Label();
            this.tabFontTextBox = new System.Windows.Forms.TextBox();
            this.selectTabFontButton = new System.Windows.Forms.Button();
            this.buttonFontLabel = new System.Windows.Forms.Label();
            this.buttonFontTextBox = new System.Windows.Forms.TextBox();
            this.selectButtonFontButton = new System.Windows.Forms.Button();
            this.labelFontLabel = new System.Windows.Forms.Label();
            this.labelFontTextBox = new System.Windows.Forms.TextBox();
            this.selectLabelFontButton = new System.Windows.Forms.Button();
            this.textBoxFontLabel = new System.Windows.Forms.Label();
            this.textBoxFontTextBox = new System.Windows.Forms.TextBox();
            this.selectTextBoxFontButton = new System.Windows.Forms.Button();
            this.otherTabPage = new System.Windows.Forms.TabPage();
            this.isHorizontalSplitCheckBox = new System.Windows.Forms.CheckBox();
            this.toggleVisibleTaskTrayIconClickCheckBox = new System.Windows.Forms.CheckBox();
            this.storeTaskTrayByClosingCheckBox = new System.Windows.Forms.CheckBox();
            this.showTaskTrayIconCheckBox = new System.Windows.Forms.CheckBox();
            this.fixNoRecToServiceOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.recListMaxCountLabel = new System.Windows.Forms.Label();
            this.recListMaxCountNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.taskTrayIconClickGroupBox = new System.Windows.Forms.GroupBox();
            this.taskTrayIconRightDoubleClickLabel = new System.Windows.Forms.Label();
            this.taskTrayIconRightClickLabel = new System.Windows.Forms.Label();
            this.taskTrayIconLeftDoubleClickLabel = new System.Windows.Forms.Label();
            this.taskTrayIconLeftClickLabel = new System.Windows.Forms.Label();
            this.taskTrayIconRightDoubleClickComboBox = new System.Windows.Forms.ComboBox();
            this.taskTrayIconRightClickComboBox = new System.Windows.Forms.ComboBox();
            this.taskTrayIconLeftDoubleClickComboBox = new System.Windows.Forms.ComboBox();
            this.taskTrayIconLeftClickComboBox = new System.Windows.Forms.ComboBox();
            this.tvtestOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.fontDialog = new System.Windows.Forms.FontDialog();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.settingTabControl.SuspendLayout();
            this.edcbLinkageTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.portNumberNumericUpDown)).BeginInit();
            this.reserveTabPage.SuspendLayout();
            this.edcbReserveSettingGroupBox.SuspendLayout();
            this.prioritizeViewPanel.SuspendLayout();
            this.suspendModeAfterRecPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.endRecMarginNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.startRecMarginNumericUpDown)).BeginInit();
            this.tunerTabPage.SuspendLayout();
            this.allServiceTabPage.SuspendLayout();
            this.favoriteServiceTabPage.SuspendLayout();
            this.tvtestLinkageTabPage.SuspendLayout();
            this.autoStartTargetGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.autoCloseMarginNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.autoOpenMarginNumericUpDown)).BeginInit();
            this.listViewContColorTabPage.SuspendLayout();
            this.contextMenuFontColorTabPage.SuspendLayout();
            this.controlUiFontColorTabPage.SuspendLayout();
            this.otherTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.recListMaxCountNumericUpDown)).BeginInit();
            this.taskTrayIconClickGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(721, 419);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "キャンセル";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Location = new System.Drawing.Point(630, 419);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(75, 23);
            this.applyButton.TabIndex = 1;
            this.applyButton.Text = "設定保存";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // allServiceListView
            // 
            this.allServiceListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.allServiceMarkColumnHeader,
            this.allServiceNetworkTypeColumnHeader,
            this.allServiceNameColumnHeader,
            this.allServiceTsidColumnHeader,
            this.allServiceSidColumnHeader});
            this.allServiceListView.FullRowSelect = true;
            this.allServiceListView.HideSelection = false;
            this.allServiceListView.Location = new System.Drawing.Point(8, 26);
            this.allServiceListView.Name = "allServiceListView";
            this.allServiceListView.Size = new System.Drawing.Size(333, 353);
            this.allServiceListView.TabIndex = 0;
            this.allServiceListView.UseCompatibleStateImageBehavior = false;
            this.allServiceListView.View = System.Windows.Forms.View.Details;
            this.allServiceListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.allServiceListView_ColumnClick);
            // 
            // allServiceMarkColumnHeader
            // 
            this.allServiceMarkColumnHeader.Text = "";
            this.allServiceMarkColumnHeader.Width = 20;
            // 
            // allServiceNetworkTypeColumnHeader
            // 
            this.allServiceNetworkTypeColumnHeader.Text = "種類";
            this.allServiceNetworkTypeColumnHeader.Width = 48;
            // 
            // allServiceNameColumnHeader
            // 
            this.allServiceNameColumnHeader.Text = "名前";
            this.allServiceNameColumnHeader.Width = 130;
            // 
            // allServiceTsidColumnHeader
            // 
            this.allServiceTsidColumnHeader.Text = "TSID";
            this.allServiceTsidColumnHeader.Width = 57;
            // 
            // allServiceSidColumnHeader
            // 
            this.allServiceSidColumnHeader.Text = "SID";
            this.allServiceSidColumnHeader.Width = 57;
            // 
            // addSelectedServiceButton
            // 
            this.addSelectedServiceButton.Location = new System.Drawing.Point(347, 139);
            this.addSelectedServiceButton.Name = "addSelectedServiceButton";
            this.addSelectedServiceButton.Size = new System.Drawing.Size(51, 23);
            this.addSelectedServiceButton.TabIndex = 2;
            this.addSelectedServiceButton.Text = ">>";
            this.addSelectedServiceButton.UseVisualStyleBackColor = true;
            this.addSelectedServiceButton.Click += new System.EventHandler(this.addSelectedServiceButton_Click);
            // 
            // addNewServiceButton
            // 
            this.addNewServiceButton.Location = new System.Drawing.Point(744, 240);
            this.addNewServiceButton.Name = "addNewServiceButton";
            this.addNewServiceButton.Size = new System.Drawing.Size(40, 23);
            this.addNewServiceButton.TabIndex = 6;
            this.addNewServiceButton.Text = "追加";
            this.addNewServiceButton.UseVisualStyleBackColor = true;
            this.addNewServiceButton.Click += new System.EventHandler(this.addNewServiceButton_Click);
            // 
            // editServiceButton
            // 
            this.editServiceButton.Location = new System.Drawing.Point(744, 269);
            this.editServiceButton.Name = "editServiceButton";
            this.editServiceButton.Size = new System.Drawing.Size(40, 23);
            this.editServiceButton.TabIndex = 7;
            this.editServiceButton.Text = "編集";
            this.editServiceButton.UseVisualStyleBackColor = true;
            this.editServiceButton.Click += new System.EventHandler(this.editServiceButton_Click);
            // 
            // removeSelectedServiceButton
            // 
            this.removeSelectedServiceButton.Location = new System.Drawing.Point(347, 243);
            this.removeSelectedServiceButton.Name = "removeSelectedServiceButton";
            this.removeSelectedServiceButton.Size = new System.Drawing.Size(51, 23);
            this.removeSelectedServiceButton.TabIndex = 3;
            this.removeSelectedServiceButton.Text = "<<";
            this.removeSelectedServiceButton.UseVisualStyleBackColor = true;
            this.removeSelectedServiceButton.Click += new System.EventHandler(this.removeServiceButton_Click);
            // 
            // moveDownSelectedServiceButton
            // 
            this.moveDownSelectedServiceButton.Location = new System.Drawing.Point(744, 208);
            this.moveDownSelectedServiceButton.Name = "moveDownSelectedServiceButton";
            this.moveDownSelectedServiceButton.Size = new System.Drawing.Size(40, 23);
            this.moveDownSelectedServiceButton.TabIndex = 5;
            this.moveDownSelectedServiceButton.Text = "↓";
            this.moveDownSelectedServiceButton.UseVisualStyleBackColor = true;
            this.moveDownSelectedServiceButton.Click += new System.EventHandler(this.moveDownSelectedServiceButton_Click);
            // 
            // moveUpSelectedServiceButton
            // 
            this.moveUpSelectedServiceButton.Location = new System.Drawing.Point(744, 169);
            this.moveUpSelectedServiceButton.Name = "moveUpSelectedServiceButton";
            this.moveUpSelectedServiceButton.Size = new System.Drawing.Size(40, 23);
            this.moveUpSelectedServiceButton.TabIndex = 4;
            this.moveUpSelectedServiceButton.Text = "↑";
            this.moveUpSelectedServiceButton.UseVisualStyleBackColor = true;
            this.moveUpSelectedServiceButton.Click += new System.EventHandler(this.moveUpSelectedServiceButton_Click);
            // 
            // selectedServiceListView
            // 
            this.selectedServiceListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.selectedServiceMarkColumnHeader,
            this.selectedServiceNetworkTypeColumnHeader,
            this.selectedServiceNameColumnHeader,
            this.selectedServiceTsidColumnHeader,
            this.selectedServiceSidColumnHeader,
            this.selectedServiceTvtestOptionColumnHeader});
            this.selectedServiceListView.FullRowSelect = true;
            this.selectedServiceListView.HideSelection = false;
            this.selectedServiceListView.Location = new System.Drawing.Point(405, 26);
            this.selectedServiceListView.Name = "selectedServiceListView";
            this.selectedServiceListView.Size = new System.Drawing.Size(333, 353);
            this.selectedServiceListView.TabIndex = 1;
            this.selectedServiceListView.UseCompatibleStateImageBehavior = false;
            this.selectedServiceListView.View = System.Windows.Forms.View.Details;
            this.selectedServiceListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.selectedServiceListView_ColumnClick);
            this.selectedServiceListView.DoubleClick += new System.EventHandler(this.selectedServiceListView_DoubleClick);
            // 
            // selectedServiceMarkColumnHeader
            // 
            this.selectedServiceMarkColumnHeader.Text = "";
            this.selectedServiceMarkColumnHeader.Width = 20;
            // 
            // selectedServiceNetworkTypeColumnHeader
            // 
            this.selectedServiceNetworkTypeColumnHeader.Text = "種類";
            this.selectedServiceNetworkTypeColumnHeader.Width = 48;
            // 
            // selectedServiceNameColumnHeader
            // 
            this.selectedServiceNameColumnHeader.Text = "名前";
            this.selectedServiceNameColumnHeader.Width = 130;
            // 
            // selectedServiceTsidColumnHeader
            // 
            this.selectedServiceTsidColumnHeader.Text = "TSID";
            this.selectedServiceTsidColumnHeader.Width = 50;
            // 
            // selectedServiceSidColumnHeader
            // 
            this.selectedServiceSidColumnHeader.Text = "SID";
            this.selectedServiceSidColumnHeader.Width = 50;
            // 
            // selectedServiceTvtestOptionColumnHeader
            // 
            this.selectedServiceTvtestOptionColumnHeader.Text = "TVTestオプション";
            this.selectedServiceTvtestOptionColumnHeader.Width = 180;
            // 
            // settingTabControl
            // 
            this.settingTabControl.Controls.Add(this.edcbLinkageTabPage);
            this.settingTabControl.Controls.Add(this.reserveTabPage);
            this.settingTabControl.Controls.Add(this.tunerTabPage);
            this.settingTabControl.Controls.Add(this.allServiceTabPage);
            this.settingTabControl.Controls.Add(this.favoriteServiceTabPage);
            this.settingTabControl.Controls.Add(this.tvtestLinkageTabPage);
            this.settingTabControl.Controls.Add(this.listViewContColorTabPage);
            this.settingTabControl.Controls.Add(this.contextMenuFontColorTabPage);
            this.settingTabControl.Controls.Add(this.controlUiFontColorTabPage);
            this.settingTabControl.Controls.Add(this.otherTabPage);
            this.settingTabControl.Location = new System.Drawing.Point(1, 3);
            this.settingTabControl.Name = "settingTabControl";
            this.settingTabControl.SelectedIndex = 0;
            this.settingTabControl.Size = new System.Drawing.Size(799, 410);
            this.settingTabControl.TabIndex = 0;
            this.settingTabControl.SelectedIndexChanged += new System.EventHandler(this.settingTabControl_SelectedIndexChanged);
            // 
            // edcbLinkageTabPage
            // 
            this.edcbLinkageTabPage.Controls.Add(this.webEPGLabel);
            this.edcbLinkageTabPage.Controls.Add(this.webEpgUrlTextBox);
            this.edcbLinkageTabPage.Controls.Add(this.portNumberNoteLabel);
            this.edcbLinkageTabPage.Controls.Add(this.portNumberNumericUpDown);
            this.edcbLinkageTabPage.Controls.Add(this.label3);
            this.edcbLinkageTabPage.Controls.Add(this.portNumberLabel);
            this.edcbLinkageTabPage.Controls.Add(this.webLinkUrlExampleLabel);
            this.edcbLinkageTabPage.Controls.Add(this.useTcpIpCheckBox);
            this.edcbLinkageTabPage.Controls.Add(this.useWebLinkCheckBox);
            this.edcbLinkageTabPage.Controls.Add(this.ipAddressLabel);
            this.edcbLinkageTabPage.Controls.Add(this.webLinkUrlLabel);
            this.edcbLinkageTabPage.Controls.Add(this.recInfoWebLinkUrlLabel);
            this.edcbLinkageTabPage.Controls.Add(this.ipAddressTextBox);
            this.edcbLinkageTabPage.Controls.Add(this.webLinkUrlTextBox);
            this.edcbLinkageTabPage.Controls.Add(this.recInfoWebLinkUrlTextBox);
            this.edcbLinkageTabPage.Location = new System.Drawing.Point(4, 22);
            this.edcbLinkageTabPage.Name = "edcbLinkageTabPage";
            this.edcbLinkageTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.edcbLinkageTabPage.Size = new System.Drawing.Size(791, 384);
            this.edcbLinkageTabPage.TabIndex = 0;
            this.edcbLinkageTabPage.Text = "EDCB連携";
            this.edcbLinkageTabPage.UseVisualStyleBackColor = true;
            // 
            // webEPGLabel
            // 
            this.webEPGLabel.AutoSize = true;
            this.webEPGLabel.Location = new System.Drawing.Point(35, 178);
            this.webEPGLabel.Name = "webEPGLabel";
            this.webEPGLabel.Size = new System.Drawing.Size(63, 12);
            this.webEPGLabel.TabIndex = 18;
            this.webEPGLabel.Text = "番組表URL";
            // 
            // webEpgUrlTextBox
            // 
            this.webEpgUrlTextBox.Location = new System.Drawing.Point(122, 175);
            this.webEpgUrlTextBox.Name = "webEpgUrlTextBox";
            this.webEpgUrlTextBox.Size = new System.Drawing.Size(478, 19);
            this.webEpgUrlTextBox.TabIndex = 17;
            // 
            // portNumberNoteLabel
            // 
            this.portNumberNoteLabel.AutoSize = true;
            this.portNumberNoteLabel.Location = new System.Drawing.Point(303, 77);
            this.portNumberNoteLabel.Name = "portNumberNoteLabel";
            this.portNumberNoteLabel.Size = new System.Drawing.Size(209, 12);
            this.portNumberNoteLabel.TabIndex = 16;
            this.portNumberNoteLabel.Text = "※localhost, ホスト名はNG。127.0.0.1はOK";
            // 
            // portNumberNumericUpDown
            // 
            this.portNumberNumericUpDown.Location = new System.Drawing.Point(178, 111);
            this.portNumberNumericUpDown.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.portNumberNumericUpDown.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.portNumberNumericUpDown.Name = "portNumberNumericUpDown";
            this.portNumberNumericUpDown.Size = new System.Drawing.Size(70, 19);
            this.portNumberNumericUpDown.TabIndex = 2;
            this.portNumberNumericUpDown.Value = new decimal(new int[] {
            5510,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(37, 45);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(264, 12);
            this.label3.TabIndex = 14;
            this.label3.Text = "※非チェック時はローカルパイプ通信。次回起動時反映";
            // 
            // portNumberLabel
            // 
            this.portNumberLabel.AutoSize = true;
            this.portNumberLabel.Location = new System.Drawing.Point(59, 113);
            this.portNumberLabel.Name = "portNumberLabel";
            this.portNumberLabel.Size = new System.Drawing.Size(113, 12);
            this.portNumberLabel.TabIndex = 13;
            this.portNumberLabel.Text = "ポート番号(1～65535)";
            // 
            // webLinkUrlExampleLabel
            // 
            this.webLinkUrlExampleLabel.AutoSize = true;
            this.webLinkUrlExampleLabel.Location = new System.Drawing.Point(120, 282);
            this.webLinkUrlExampleLabel.Name = "webLinkUrlExampleLabel";
            this.webLinkUrlExampleLabel.Size = new System.Drawing.Size(484, 84);
            this.webLinkUrlExampleLabel.TabIndex = 11;
            this.webLinkUrlExampleLabel.Text = resources.GetString("webLinkUrlExampleLabel.Text");
            // 
            // useTcpIpCheckBox
            // 
            this.useTcpIpCheckBox.AutoSize = true;
            this.useTcpIpCheckBox.Location = new System.Drawing.Point(22, 20);
            this.useTcpIpCheckBox.Name = "useTcpIpCheckBox";
            this.useTcpIpCheckBox.Size = new System.Drawing.Size(196, 16);
            this.useTcpIpCheckBox.TabIndex = 0;
            this.useTcpIpCheckBox.Text = "EDCBとの通信にTCP/IPを使用する";
            this.useTcpIpCheckBox.UseVisualStyleBackColor = true;
            // 
            // useWebLinkCheckBox
            // 
            this.useWebLinkCheckBox.AutoSize = true;
            this.useWebLinkCheckBox.Location = new System.Drawing.Point(22, 153);
            this.useWebLinkCheckBox.Name = "useWebLinkCheckBox";
            this.useWebLinkCheckBox.Size = new System.Drawing.Size(197, 16);
            this.useWebLinkCheckBox.TabIndex = 3;
            this.useWebLinkCheckBox.Text = "Web番組表機能(WebUI)を使用する";
            this.useWebLinkCheckBox.UseVisualStyleBackColor = true;
            // 
            // ipAddressLabel
            // 
            this.ipAddressLabel.AutoSize = true;
            this.ipAddressLabel.Location = new System.Drawing.Point(65, 77);
            this.ipAddressLabel.Name = "ipAddressLabel";
            this.ipAddressLabel.Size = new System.Drawing.Size(51, 12);
            this.ipAddressLabel.TabIndex = 8;
            this.ipAddressLabel.Text = "IPアドレス";
            // 
            // webLinkUrlLabel
            // 
            this.webLinkUrlLabel.AutoSize = true;
            this.webLinkUrlLabel.Location = new System.Drawing.Point(35, 212);
            this.webLinkUrlLabel.Name = "webLinkUrlLabel";
            this.webLinkUrlLabel.Size = new System.Drawing.Size(75, 12);
            this.webLinkUrlLabel.TabIndex = 8;
            this.webLinkUrlLabel.Text = "番組詳細URL";
            // 
            // recInfoWebLinkUrlLabel
            // 
            this.recInfoWebLinkUrlLabel.AutoSize = true;
            this.recInfoWebLinkUrlLabel.Location = new System.Drawing.Point(35, 247);
            this.recInfoWebLinkUrlLabel.Name = "recInfoWebLinkUrlLabel";
            this.recInfoWebLinkUrlLabel.Size = new System.Drawing.Size(75, 12);
            this.recInfoWebLinkUrlLabel.TabIndex = 9;
            this.recInfoWebLinkUrlLabel.Text = "録画詳細URL";
            // 
            // ipAddressTextBox
            // 
            this.ipAddressTextBox.Location = new System.Drawing.Point(122, 74);
            this.ipAddressTextBox.Name = "ipAddressTextBox";
            this.ipAddressTextBox.Size = new System.Drawing.Size(166, 19);
            this.ipAddressTextBox.TabIndex = 1;
            // 
            // webLinkUrlTextBox
            // 
            this.webLinkUrlTextBox.Location = new System.Drawing.Point(122, 209);
            this.webLinkUrlTextBox.Name = "webLinkUrlTextBox";
            this.webLinkUrlTextBox.Size = new System.Drawing.Size(478, 19);
            this.webLinkUrlTextBox.TabIndex = 4;
            // 
            // recInfoWebLinkUrlTextBox
            // 
            this.recInfoWebLinkUrlTextBox.Location = new System.Drawing.Point(122, 244);
            this.recInfoWebLinkUrlTextBox.Name = "recInfoWebLinkUrlTextBox";
            this.recInfoWebLinkUrlTextBox.Size = new System.Drawing.Size(478, 19);
            this.recInfoWebLinkUrlTextBox.TabIndex = 5;
            // 
            // reserveTabPage
            // 
            this.reserveTabPage.Controls.Add(this.useRockbarReserveLabel);
            this.reserveTabPage.Controls.Add(this.useRockbarReserveDelConfirmCheckBox);
            this.reserveTabPage.Controls.Add(this.useRockbarReserveDelCheckBox);
            this.reserveTabPage.Controls.Add(this.useRockbarReserveModCheckBox);
            this.reserveTabPage.Controls.Add(this.edcbReserveSettingGroupBox);
            this.reserveTabPage.Controls.Add(this.useRockbarReserveAddCheckBox);
            this.reserveTabPage.Location = new System.Drawing.Point(4, 22);
            this.reserveTabPage.Name = "reserveTabPage";
            this.reserveTabPage.Size = new System.Drawing.Size(791, 384);
            this.reserveTabPage.TabIndex = 9;
            this.reserveTabPage.Text = "予約";
            this.reserveTabPage.UseVisualStyleBackColor = true;
            // 
            // useRockbarReserveLabel
            // 
            this.useRockbarReserveLabel.AutoSize = true;
            this.useRockbarReserveLabel.Location = new System.Drawing.Point(21, 17);
            this.useRockbarReserveLabel.Name = "useRockbarReserveLabel";
            this.useRockbarReserveLabel.Size = new System.Drawing.Size(143, 12);
            this.useRockbarReserveLabel.TabIndex = 60;
            this.useRockbarReserveLabel.Text = "RockbarForEDCB予約機能";
            // 
            // useRockbarReserveDelConfirmCheckBox
            // 
            this.useRockbarReserveDelConfirmCheckBox.AutoSize = true;
            this.useRockbarReserveDelConfirmCheckBox.Checked = true;
            this.useRockbarReserveDelConfirmCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useRockbarReserveDelConfirmCheckBox.Location = new System.Drawing.Point(488, 16);
            this.useRockbarReserveDelConfirmCheckBox.Name = "useRockbarReserveDelConfirmCheckBox";
            this.useRockbarReserveDelConfirmCheckBox.Size = new System.Drawing.Size(108, 16);
            this.useRockbarReserveDelConfirmCheckBox.TabIndex = 59;
            this.useRockbarReserveDelConfirmCheckBox.Text = "削除前確認表示";
            this.useRockbarReserveDelConfirmCheckBox.UseVisualStyleBackColor = true;
            // 
            // useRockbarReserveDelCheckBox
            // 
            this.useRockbarReserveDelCheckBox.AutoSize = true;
            this.useRockbarReserveDelCheckBox.Location = new System.Drawing.Point(346, 16);
            this.useRockbarReserveDelCheckBox.Name = "useRockbarReserveDelCheckBox";
            this.useRockbarReserveDelCheckBox.Size = new System.Drawing.Size(126, 16);
            this.useRockbarReserveDelCheckBox.TabIndex = 58;
            this.useRockbarReserveDelCheckBox.Text = "予約・録画情報削除";
            this.useRockbarReserveDelCheckBox.UseVisualStyleBackColor = true;
            this.useRockbarReserveDelCheckBox.CheckedChanged += new System.EventHandler(this.useRockbarReserveDelCheckBox_CheckedChanged);
            // 
            // useRockbarReserveModCheckBox
            // 
            this.useRockbarReserveModCheckBox.AutoSize = true;
            this.useRockbarReserveModCheckBox.Location = new System.Drawing.Point(260, 16);
            this.useRockbarReserveModCheckBox.Name = "useRockbarReserveModCheckBox";
            this.useRockbarReserveModCheckBox.Size = new System.Drawing.Size(72, 16);
            this.useRockbarReserveModCheckBox.TabIndex = 57;
            this.useRockbarReserveModCheckBox.Text = "予約変更";
            this.useRockbarReserveModCheckBox.UseVisualStyleBackColor = true;
            // 
            // edcbReserveSettingGroupBox
            // 
            this.edcbReserveSettingGroupBox.Controls.Add(this.recCommentTextBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recCommentLabel);
            this.edcbReserveSettingGroupBox.Controls.Add(this.prioritizeViewLabel);
            this.edcbReserveSettingGroupBox.Controls.Add(this.prioritizeViewPanel);
            this.edcbReserveSettingGroupBox.Controls.Add(this.suspendModeAfterRecPanel);
            this.edcbReserveSettingGroupBox.Controls.Add(this.endRecMarginNumericUpDown);
            this.edcbReserveSettingGroupBox.Controls.Add(this.startRecMarginNumericUpDown);
            this.edcbReserveSettingGroupBox.Controls.Add(this.browseBatFileButton);
            this.edcbReserveSettingGroupBox.Controls.Add(this.deleteRecFolderButton);
            this.edcbReserveSettingGroupBox.Controls.Add(this.copyRecFolderButton);
            this.edcbReserveSettingGroupBox.Controls.Add(this.editRecFolderButton);
            this.edcbReserveSettingGroupBox.Controls.Add(this.addRecFolderButton);
            this.edcbReserveSettingGroupBox.Controls.Add(this.enableReserveCheckBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recTagTextBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recPriorityComboBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recBatFilePathTextBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recModeComboBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recTagLabel);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recModeLabel);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recBatFilePathLabel);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recTuijyuuCheckBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.rebootAfterReturnCheckBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recPittariCheckBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recPriorityLabel);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recMarginLabel);
            this.edcbReserveSettingGroupBox.Controls.Add(this.useDefaultRecMarginCheckBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.startRecMarginLabel);
            this.edcbReserveSettingGroupBox.Controls.Add(this.defaultSuspendModeAfterRecCheckBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.suspendModeAfterRecLabel);
            this.edcbReserveSettingGroupBox.Controls.Add(this.endRecMarginLabel);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recTunerIdComboBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recTunerIdLabel);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recServiceDataCarouselCheckBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.continueRecSameFileCheckBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recServiceDataLabel);
            this.edcbReserveSettingGroupBox.Controls.Add(this.partialRecSeparateFileCheckBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.useDefaultRecServiceDataCheckBox);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recFolderListView);
            this.edcbReserveSettingGroupBox.Controls.Add(this.recServiceDataCaptionCheckBox);
            this.edcbReserveSettingGroupBox.Location = new System.Drawing.Point(23, 40);
            this.edcbReserveSettingGroupBox.Name = "edcbReserveSettingGroupBox";
            this.edcbReserveSettingGroupBox.Size = new System.Drawing.Size(655, 333);
            this.edcbReserveSettingGroupBox.TabIndex = 56;
            this.edcbReserveSettingGroupBox.TabStop = false;
            this.edcbReserveSettingGroupBox.Text = "EDCB予約追加内容";
            // 
            // recCommentTextBox
            // 
            this.recCommentTextBox.Location = new System.Drawing.Point(113, 303);
            this.recCommentTextBox.Name = "recCommentTextBox";
            this.recCommentTextBox.Size = new System.Drawing.Size(420, 19);
            this.recCommentTextBox.TabIndex = 68;
            // 
            // recCommentLabel
            // 
            this.recCommentLabel.AutoSize = true;
            this.recCommentLabel.Location = new System.Drawing.Point(14, 306);
            this.recCommentLabel.Name = "recCommentLabel";
            this.recCommentLabel.Size = new System.Drawing.Size(62, 12);
            this.recCommentLabel.TabIndex = 67;
            this.recCommentLabel.Text = "録画コメント";
            // 
            // prioritizeViewLabel
            // 
            this.prioritizeViewLabel.AutoSize = true;
            this.prioritizeViewLabel.Location = new System.Drawing.Point(80, 24);
            this.prioritizeViewLabel.Name = "prioritizeViewLabel";
            this.prioritizeViewLabel.Size = new System.Drawing.Size(57, 12);
            this.prioritizeViewLabel.TabIndex = 66;
            this.prioritizeViewLabel.Text = "優先モード";
            // 
            // prioritizeViewPanel
            // 
            this.prioritizeViewPanel.Controls.Add(this.prioritizeRecRadioButton);
            this.prioritizeViewPanel.Controls.Add(this.prioritizeViewRadioButton);
            this.prioritizeViewPanel.Location = new System.Drawing.Point(149, 16);
            this.prioritizeViewPanel.Name = "prioritizeViewPanel";
            this.prioritizeViewPanel.Size = new System.Drawing.Size(116, 27);
            this.prioritizeViewPanel.TabIndex = 65;
            // 
            // prioritizeRecRadioButton
            // 
            this.prioritizeRecRadioButton.AutoSize = true;
            this.prioritizeRecRadioButton.Checked = true;
            this.prioritizeRecRadioButton.Location = new System.Drawing.Point(60, 6);
            this.prioritizeRecRadioButton.Name = "prioritizeRecRadioButton";
            this.prioritizeRecRadioButton.Size = new System.Drawing.Size(47, 16);
            this.prioritizeRecRadioButton.TabIndex = 63;
            this.prioritizeRecRadioButton.TabStop = true;
            this.prioritizeRecRadioButton.Text = "録画";
            this.prioritizeRecRadioButton.UseVisualStyleBackColor = true;
            // 
            // prioritizeViewRadioButton
            // 
            this.prioritizeViewRadioButton.AutoSize = true;
            this.prioritizeViewRadioButton.Location = new System.Drawing.Point(8, 6);
            this.prioritizeViewRadioButton.Name = "prioritizeViewRadioButton";
            this.prioritizeViewRadioButton.Size = new System.Drawing.Size(47, 16);
            this.prioritizeViewRadioButton.TabIndex = 62;
            this.prioritizeViewRadioButton.Text = "視聴";
            this.prioritizeViewRadioButton.UseVisualStyleBackColor = true;
            // 
            // suspendModeAfterRecPanel
            // 
            this.suspendModeAfterRecPanel.Controls.Add(this.afterRecNoActionRadioButton);
            this.suspendModeAfterRecPanel.Controls.Add(this.afterRecStandbyRadioButton);
            this.suspendModeAfterRecPanel.Controls.Add(this.afterRecSuspendRadioButton);
            this.suspendModeAfterRecPanel.Controls.Add(this.afterRecShutdownRadioButton);
            this.suspendModeAfterRecPanel.Location = new System.Drawing.Point(168, 225);
            this.suspendModeAfterRecPanel.Name = "suspendModeAfterRecPanel";
            this.suspendModeAfterRecPanel.Size = new System.Drawing.Size(304, 25);
            this.suspendModeAfterRecPanel.TabIndex = 64;
            // 
            // afterRecNoActionRadioButton
            // 
            this.afterRecNoActionRadioButton.AutoSize = true;
            this.afterRecNoActionRadioButton.Location = new System.Drawing.Point(6, 5);
            this.afterRecNoActionRadioButton.Name = "afterRecNoActionRadioButton";
            this.afterRecNoActionRadioButton.Size = new System.Drawing.Size(73, 16);
            this.afterRecNoActionRadioButton.TabIndex = 47;
            this.afterRecNoActionRadioButton.Tag = "4";
            this.afterRecNoActionRadioButton.Text = "何もしない";
            this.afterRecNoActionRadioButton.UseVisualStyleBackColor = true;
            // 
            // afterRecStandbyRadioButton
            // 
            this.afterRecStandbyRadioButton.AutoSize = true;
            this.afterRecStandbyRadioButton.Checked = true;
            this.afterRecStandbyRadioButton.Location = new System.Drawing.Point(89, 5);
            this.afterRecStandbyRadioButton.Name = "afterRecStandbyRadioButton";
            this.afterRecStandbyRadioButton.Size = new System.Drawing.Size(68, 16);
            this.afterRecStandbyRadioButton.TabIndex = 48;
            this.afterRecStandbyRadioButton.TabStop = true;
            this.afterRecStandbyRadioButton.Tag = "1";
            this.afterRecStandbyRadioButton.Text = "スタンバイ";
            this.afterRecStandbyRadioButton.UseVisualStyleBackColor = true;
            // 
            // afterRecSuspendRadioButton
            // 
            this.afterRecSuspendRadioButton.AutoSize = true;
            this.afterRecSuspendRadioButton.Location = new System.Drawing.Point(163, 5);
            this.afterRecSuspendRadioButton.Name = "afterRecSuspendRadioButton";
            this.afterRecSuspendRadioButton.Size = new System.Drawing.Size(47, 16);
            this.afterRecSuspendRadioButton.TabIndex = 49;
            this.afterRecSuspendRadioButton.Tag = "2";
            this.afterRecSuspendRadioButton.Text = "休止";
            this.afterRecSuspendRadioButton.UseVisualStyleBackColor = true;
            // 
            // afterRecShutdownRadioButton
            // 
            this.afterRecShutdownRadioButton.AutoSize = true;
            this.afterRecShutdownRadioButton.Location = new System.Drawing.Point(216, 5);
            this.afterRecShutdownRadioButton.Name = "afterRecShutdownRadioButton";
            this.afterRecShutdownRadioButton.Size = new System.Drawing.Size(83, 16);
            this.afterRecShutdownRadioButton.TabIndex = 50;
            this.afterRecShutdownRadioButton.Tag = "3";
            this.afterRecShutdownRadioButton.Text = "シャットダウン";
            this.afterRecShutdownRadioButton.UseVisualStyleBackColor = true;
            // 
            // endRecMarginNumericUpDown
            // 
            this.endRecMarginNumericUpDown.Location = new System.Drawing.Point(432, 72);
            this.endRecMarginNumericUpDown.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.endRecMarginNumericUpDown.Minimum = new decimal(new int[] {
            9999,
            0,
            0,
            -2147483648});
            this.endRecMarginNumericUpDown.Name = "endRecMarginNumericUpDown";
            this.endRecMarginNumericUpDown.Size = new System.Drawing.Size(70, 19);
            this.endRecMarginNumericUpDown.TabIndex = 61;
            this.endRecMarginNumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // startRecMarginNumericUpDown
            // 
            this.startRecMarginNumericUpDown.Location = new System.Drawing.Point(297, 72);
            this.startRecMarginNumericUpDown.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.startRecMarginNumericUpDown.Minimum = new decimal(new int[] {
            9999,
            0,
            0,
            -2147483648});
            this.startRecMarginNumericUpDown.Name = "startRecMarginNumericUpDown";
            this.startRecMarginNumericUpDown.Size = new System.Drawing.Size(70, 19);
            this.startRecMarginNumericUpDown.TabIndex = 18;
            this.startRecMarginNumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // browseBatFileButton
            // 
            this.browseBatFileButton.Location = new System.Drawing.Point(539, 252);
            this.browseBatFileButton.Name = "browseBatFileButton";
            this.browseBatFileButton.Size = new System.Drawing.Size(40, 23);
            this.browseBatFileButton.TabIndex = 60;
            this.browseBatFileButton.Text = "開く";
            this.browseBatFileButton.UseVisualStyleBackColor = true;
            this.browseBatFileButton.Click += new System.EventHandler(this.browseBatFileButton_Click);
            // 
            // deleteRecFolderButton
            // 
            this.deleteRecFolderButton.Location = new System.Drawing.Point(589, 197);
            this.deleteRecFolderButton.Name = "deleteRecFolderButton";
            this.deleteRecFolderButton.Size = new System.Drawing.Size(40, 23);
            this.deleteRecFolderButton.TabIndex = 59;
            this.deleteRecFolderButton.Text = "削除";
            this.deleteRecFolderButton.UseVisualStyleBackColor = true;
            this.deleteRecFolderButton.Click += new System.EventHandler(this.delRecFolderButton_Click);
            // 
            // copyRecFolderButton
            // 
            this.copyRecFolderButton.Location = new System.Drawing.Point(589, 172);
            this.copyRecFolderButton.Name = "copyRecFolderButton";
            this.copyRecFolderButton.Size = new System.Drawing.Size(40, 23);
            this.copyRecFolderButton.TabIndex = 58;
            this.copyRecFolderButton.Text = "コピー";
            this.copyRecFolderButton.UseVisualStyleBackColor = true;
            this.copyRecFolderButton.Click += new System.EventHandler(this.copyRecFolderButton_Click);
            // 
            // editRecFolderButton
            // 
            this.editRecFolderButton.Location = new System.Drawing.Point(589, 147);
            this.editRecFolderButton.Name = "editRecFolderButton";
            this.editRecFolderButton.Size = new System.Drawing.Size(40, 23);
            this.editRecFolderButton.TabIndex = 57;
            this.editRecFolderButton.Text = "変更";
            this.editRecFolderButton.UseVisualStyleBackColor = true;
            this.editRecFolderButton.Click += new System.EventHandler(this.editRecFolderButton_Click);
            // 
            // addRecFolderButton
            // 
            this.addRecFolderButton.Location = new System.Drawing.Point(589, 122);
            this.addRecFolderButton.Name = "addRecFolderButton";
            this.addRecFolderButton.Size = new System.Drawing.Size(40, 23);
            this.addRecFolderButton.TabIndex = 56;
            this.addRecFolderButton.Text = "追加";
            this.addRecFolderButton.UseVisualStyleBackColor = true;
            this.addRecFolderButton.Click += new System.EventHandler(this.addRecFolderButton_Click);
            // 
            // enableReserveCheckBox
            // 
            this.enableReserveCheckBox.AutoSize = true;
            this.enableReserveCheckBox.Checked = true;
            this.enableReserveCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.enableReserveCheckBox.Location = new System.Drawing.Point(16, 23);
            this.enableReserveCheckBox.Name = "enableReserveCheckBox";
            this.enableReserveCheckBox.Size = new System.Drawing.Size(48, 16);
            this.enableReserveCheckBox.TabIndex = 2;
            this.enableReserveCheckBox.Text = "有効";
            this.enableReserveCheckBox.UseVisualStyleBackColor = true;
            // 
            // recTagTextBox
            // 
            this.recTagTextBox.Location = new System.Drawing.Point(113, 279);
            this.recTagTextBox.Name = "recTagTextBox";
            this.recTagTextBox.Size = new System.Drawing.Size(420, 19);
            this.recTagTextBox.TabIndex = 55;
            // 
            // recPriorityComboBox
            // 
            this.recPriorityComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.recPriorityComboBox.FormattingEnabled = true;
            this.recPriorityComboBox.Items.AddRange(new object[] {
            "1(低)",
            "2",
            "3",
            "4",
            "5(高)"});
            this.recPriorityComboBox.Location = new System.Drawing.Point(155, 47);
            this.recPriorityComboBox.Name = "recPriorityComboBox";
            this.recPriorityComboBox.Size = new System.Drawing.Size(101, 20);
            this.recPriorityComboBox.TabIndex = 28;
            // 
            // recBatFilePathTextBox
            // 
            this.recBatFilePathTextBox.Location = new System.Drawing.Point(113, 254);
            this.recBatFilePathTextBox.Name = "recBatFilePathTextBox";
            this.recBatFilePathTextBox.Size = new System.Drawing.Size(420, 19);
            this.recBatFilePathTextBox.TabIndex = 54;
            // 
            // recModeComboBox
            // 
            this.recModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.recModeComboBox.FormattingEnabled = true;
            this.recModeComboBox.Items.AddRange(new object[] {
            "全サービス",
            "指定サービス",
            "全サービス(デコード処理なし)",
            "指定サービス(デコード処理なし)"});
            this.recModeComboBox.Location = new System.Drawing.Point(349, 21);
            this.recModeComboBox.Name = "recModeComboBox";
            this.recModeComboBox.Size = new System.Drawing.Size(175, 20);
            this.recModeComboBox.TabIndex = 24;
            // 
            // recTagLabel
            // 
            this.recTagLabel.AutoSize = true;
            this.recTagLabel.Location = new System.Drawing.Point(14, 282);
            this.recTagLabel.Name = "recTagLabel";
            this.recTagLabel.Size = new System.Drawing.Size(46, 12);
            this.recTagLabel.TabIndex = 53;
            this.recTagLabel.Text = "録画タグ";
            // 
            // recModeLabel
            // 
            this.recModeLabel.AutoSize = true;
            this.recModeLabel.Location = new System.Drawing.Point(287, 24);
            this.recModeLabel.Name = "recModeLabel";
            this.recModeLabel.Size = new System.Drawing.Size(57, 12);
            this.recModeLabel.TabIndex = 25;
            this.recModeLabel.Text = "録画モード";
            // 
            // recBatFilePathLabel
            // 
            this.recBatFilePathLabel.AutoSize = true;
            this.recBatFilePathLabel.Location = new System.Drawing.Point(14, 257);
            this.recBatFilePathLabel.Name = "recBatFilePathLabel";
            this.recBatFilePathLabel.Size = new System.Drawing.Size(81, 12);
            this.recBatFilePathLabel.TabIndex = 52;
            this.recBatFilePathLabel.Text = "録画後実行bat";
            // 
            // recTuijyuuCheckBox
            // 
            this.recTuijyuuCheckBox.AutoSize = true;
            this.recTuijyuuCheckBox.Checked = true;
            this.recTuijyuuCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.recTuijyuuCheckBox.Location = new System.Drawing.Point(289, 49);
            this.recTuijyuuCheckBox.Name = "recTuijyuuCheckBox";
            this.recTuijyuuCheckBox.Size = new System.Drawing.Size(110, 16);
            this.recTuijyuuCheckBox.TabIndex = 26;
            this.recTuijyuuCheckBox.Text = "イベントリレー追従";
            this.recTuijyuuCheckBox.UseVisualStyleBackColor = true;
            // 
            // rebootAfterReturnCheckBox
            // 
            this.rebootAfterReturnCheckBox.AutoSize = true;
            this.rebootAfterReturnCheckBox.Location = new System.Drawing.Point(480, 230);
            this.rebootAfterReturnCheckBox.Name = "rebootAfterReturnCheckBox";
            this.rebootAfterReturnCheckBox.Size = new System.Drawing.Size(115, 16);
            this.rebootAfterReturnCheckBox.TabIndex = 51;
            this.rebootAfterReturnCheckBox.Text = "復帰後再起動する";
            this.rebootAfterReturnCheckBox.UseVisualStyleBackColor = true;
            // 
            // recPittariCheckBox
            // 
            this.recPittariCheckBox.AutoSize = true;
            this.recPittariCheckBox.Location = new System.Drawing.Point(418, 49);
            this.recPittariCheckBox.Name = "recPittariCheckBox";
            this.recPittariCheckBox.Size = new System.Drawing.Size(96, 16);
            this.recPittariCheckBox.TabIndex = 27;
            this.recPittariCheckBox.Text = "ぴったり(?)録画";
            this.recPittariCheckBox.UseVisualStyleBackColor = true;
            // 
            // recPriorityLabel
            // 
            this.recPriorityLabel.AutoSize = true;
            this.recPriorityLabel.Location = new System.Drawing.Point(78, 50);
            this.recPriorityLabel.Name = "recPriorityLabel";
            this.recPriorityLabel.Size = new System.Drawing.Size(41, 12);
            this.recPriorityLabel.TabIndex = 29;
            this.recPriorityLabel.Text = "優先度";
            // 
            // recMarginLabel
            // 
            this.recMarginLabel.AutoSize = true;
            this.recMarginLabel.Location = new System.Drawing.Point(14, 75);
            this.recMarginLabel.Name = "recMarginLabel";
            this.recMarginLabel.Size = new System.Drawing.Size(67, 12);
            this.recMarginLabel.TabIndex = 30;
            this.recMarginLabel.Text = "録画マージン";
            // 
            // useDefaultRecMarginCheckBox
            // 
            this.useDefaultRecMarginCheckBox.AutoSize = true;
            this.useDefaultRecMarginCheckBox.Checked = true;
            this.useDefaultRecMarginCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useDefaultRecMarginCheckBox.Location = new System.Drawing.Point(156, 74);
            this.useDefaultRecMarginCheckBox.Name = "useDefaultRecMarginCheckBox";
            this.useDefaultRecMarginCheckBox.Size = new System.Drawing.Size(68, 16);
            this.useDefaultRecMarginCheckBox.TabIndex = 32;
            this.useDefaultRecMarginCheckBox.Text = "デフォルト";
            this.useDefaultRecMarginCheckBox.UseVisualStyleBackColor = true;
            this.useDefaultRecMarginCheckBox.CheckedChanged += new System.EventHandler(this.defaultMarginCheckBox_CheckedChanged);
            // 
            // startRecMarginLabel
            // 
            this.startRecMarginLabel.AutoSize = true;
            this.startRecMarginLabel.Location = new System.Drawing.Point(253, 75);
            this.startRecMarginLabel.Name = "startRecMarginLabel";
            this.startRecMarginLabel.Size = new System.Drawing.Size(29, 12);
            this.startRecMarginLabel.TabIndex = 34;
            this.startRecMarginLabel.Text = "開始";
            // 
            // defaultSuspendModeAfterRecCheckBox
            // 
            this.defaultSuspendModeAfterRecCheckBox.AutoSize = true;
            this.defaultSuspendModeAfterRecCheckBox.Checked = true;
            this.defaultSuspendModeAfterRecCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.defaultSuspendModeAfterRecCheckBox.Location = new System.Drawing.Point(101, 230);
            this.defaultSuspendModeAfterRecCheckBox.Name = "defaultSuspendModeAfterRecCheckBox";
            this.defaultSuspendModeAfterRecCheckBox.Size = new System.Drawing.Size(68, 16);
            this.defaultSuspendModeAfterRecCheckBox.TabIndex = 46;
            this.defaultSuspendModeAfterRecCheckBox.Text = "デフォルト";
            this.defaultSuspendModeAfterRecCheckBox.UseVisualStyleBackColor = true;
            this.defaultSuspendModeAfterRecCheckBox.CheckedChanged += new System.EventHandler(this.defaultPostRecActionCheckBox_CheckedChanged);
            // 
            // suspendModeAfterRecLabel
            // 
            this.suspendModeAfterRecLabel.AutoSize = true;
            this.suspendModeAfterRecLabel.Location = new System.Drawing.Point(14, 231);
            this.suspendModeAfterRecLabel.Name = "suspendModeAfterRecLabel";
            this.suspendModeAfterRecLabel.Size = new System.Drawing.Size(65, 12);
            this.suspendModeAfterRecLabel.TabIndex = 45;
            this.suspendModeAfterRecLabel.Text = "録画後動作";
            // 
            // endRecMarginLabel
            // 
            this.endRecMarginLabel.AutoSize = true;
            this.endRecMarginLabel.Location = new System.Drawing.Point(388, 75);
            this.endRecMarginLabel.Name = "endRecMarginLabel";
            this.endRecMarginLabel.Size = new System.Drawing.Size(29, 12);
            this.endRecMarginLabel.TabIndex = 36;
            this.endRecMarginLabel.Text = "終了";
            // 
            // recTunerIdComboBox
            // 
            this.recTunerIdComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.recTunerIdComboBox.FormattingEnabled = true;
            this.recTunerIdComboBox.Location = new System.Drawing.Point(143, 203);
            this.recTunerIdComboBox.Name = "recTunerIdComboBox";
            this.recTunerIdComboBox.Size = new System.Drawing.Size(356, 20);
            this.recTunerIdComboBox.TabIndex = 44;
            // 
            // recTunerIdLabel
            // 
            this.recTunerIdLabel.AutoSize = true;
            this.recTunerIdLabel.Location = new System.Drawing.Point(13, 206);
            this.recTunerIdLabel.Name = "recTunerIdLabel";
            this.recTunerIdLabel.Size = new System.Drawing.Size(124, 12);
            this.recTunerIdLabel.TabIndex = 43;
            this.recTunerIdLabel.Text = "使用チューナー強制指定";
            // 
            // recServiceDataCarouselCheckBox
            // 
            this.recServiceDataCarouselCheckBox.AutoSize = true;
            this.recServiceDataCarouselCheckBox.Location = new System.Drawing.Point(359, 98);
            this.recServiceDataCarouselCheckBox.Name = "recServiceDataCarouselCheckBox";
            this.recServiceDataCarouselCheckBox.Size = new System.Drawing.Size(141, 16);
            this.recServiceDataCarouselCheckBox.TabIndex = 39;
            this.recServiceDataCarouselCheckBox.Text = "データカルーセルを含める";
            this.recServiceDataCarouselCheckBox.UseVisualStyleBackColor = true;
            // 
            // continueRecSameFileCheckBox
            // 
            this.continueRecSameFileCheckBox.AutoSize = true;
            this.continueRecSameFileCheckBox.Location = new System.Drawing.Point(309, 182);
            this.continueRecSameFileCheckBox.Name = "continueRecSameFileCheckBox";
            this.continueRecSameFileCheckBox.Size = new System.Drawing.Size(199, 16);
            this.continueRecSameFileCheckBox.TabIndex = 42;
            this.continueRecSameFileCheckBox.Text = "後ろの予約を同一ファイルで出力する";
            this.continueRecSameFileCheckBox.UseVisualStyleBackColor = true;
            // 
            // recServiceDataLabel
            // 
            this.recServiceDataLabel.AutoSize = true;
            this.recServiceDataLabel.Location = new System.Drawing.Point(14, 99);
            this.recServiceDataLabel.Name = "recServiceDataLabel";
            this.recServiceDataLabel.Size = new System.Drawing.Size(118, 12);
            this.recServiceDataLabel.TabIndex = 31;
            this.recServiceDataLabel.Text = "指定サービス対象データ";
            // 
            // partialRecSeparateFileCheckBox
            // 
            this.partialRecSeparateFileCheckBox.AutoSize = true;
            this.partialRecSeparateFileCheckBox.Location = new System.Drawing.Point(34, 182);
            this.partialRecSeparateFileCheckBox.Name = "partialRecSeparateFileCheckBox";
            this.partialRecSeparateFileCheckBox.Size = new System.Drawing.Size(248, 16);
            this.partialRecSeparateFileCheckBox.TabIndex = 41;
            this.partialRecSeparateFileCheckBox.Text = "部分受信(ワンセグ)を別ファイルに同時出力する";
            this.partialRecSeparateFileCheckBox.UseVisualStyleBackColor = true;
            // 
            // useDefaultRecServiceDataCheckBox
            // 
            this.useDefaultRecServiceDataCheckBox.AutoSize = true;
            this.useDefaultRecServiceDataCheckBox.Checked = true;
            this.useDefaultRecServiceDataCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.useDefaultRecServiceDataCheckBox.Location = new System.Drawing.Point(156, 98);
            this.useDefaultRecServiceDataCheckBox.Name = "useDefaultRecServiceDataCheckBox";
            this.useDefaultRecServiceDataCheckBox.Size = new System.Drawing.Size(68, 16);
            this.useDefaultRecServiceDataCheckBox.TabIndex = 33;
            this.useDefaultRecServiceDataCheckBox.Text = "デフォルト";
            this.useDefaultRecServiceDataCheckBox.UseVisualStyleBackColor = true;
            this.useDefaultRecServiceDataCheckBox.CheckedChanged += new System.EventHandler(this.defaultServiceDataCheckBox_CheckedChanged);
            // 
            // recFolderListView
            // 
            this.recFolderListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.partialRecColumnHeader,
            this.recFolderColumnHeader,
            this.writePlugInColumnHeader,
            this.recNamePlugInColumnHeader});
            this.recFolderListView.FullRowSelect = true;
            this.recFolderListView.HideSelection = false;
            this.recFolderListView.Location = new System.Drawing.Point(16, 122);
            this.recFolderListView.Name = "recFolderListView";
            this.recFolderListView.Size = new System.Drawing.Size(554, 54);
            this.recFolderListView.TabIndex = 40;
            this.recFolderListView.UseCompatibleStateImageBehavior = false;
            this.recFolderListView.View = System.Windows.Forms.View.Details;
            this.recFolderListView.DoubleClick += new System.EventHandler(this.recFolderListView_DoubleClick);
            // 
            // partialRecColumnHeader
            // 
            this.partialRecColumnHeader.Text = "部分受信";
            this.partialRecColumnHeader.Width = 63;
            // 
            // recFolderColumnHeader
            // 
            this.recFolderColumnHeader.Text = "録画フォルダ";
            this.recFolderColumnHeader.Width = 142;
            // 
            // writePlugInColumnHeader
            // 
            this.writePlugInColumnHeader.Text = "出力PlugIn";
            this.writePlugInColumnHeader.Width = 130;
            // 
            // recNamePlugInColumnHeader
            // 
            this.recNamePlugInColumnHeader.Text = "ファイル名PlugIn";
            this.recNamePlugInColumnHeader.Width = 163;
            // 
            // recServiceDataCaptionCheckBox
            // 
            this.recServiceDataCaptionCheckBox.AutoSize = true;
            this.recServiceDataCaptionCheckBox.Checked = true;
            this.recServiceDataCaptionCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.recServiceDataCaptionCheckBox.Location = new System.Drawing.Point(255, 98);
            this.recServiceDataCaptionCheckBox.Name = "recServiceDataCaptionCheckBox";
            this.recServiceDataCaptionCheckBox.Size = new System.Drawing.Size(88, 16);
            this.recServiceDataCaptionCheckBox.TabIndex = 38;
            this.recServiceDataCaptionCheckBox.Text = "字幕を含める";
            this.recServiceDataCaptionCheckBox.UseVisualStyleBackColor = true;
            // 
            // useRockbarReserveAddCheckBox
            // 
            this.useRockbarReserveAddCheckBox.AutoSize = true;
            this.useRockbarReserveAddCheckBox.Location = new System.Drawing.Point(174, 16);
            this.useRockbarReserveAddCheckBox.Name = "useRockbarReserveAddCheckBox";
            this.useRockbarReserveAddCheckBox.Size = new System.Drawing.Size(72, 16);
            this.useRockbarReserveAddCheckBox.TabIndex = 1;
            this.useRockbarReserveAddCheckBox.Text = "予約追加";
            this.useRockbarReserveAddCheckBox.UseVisualStyleBackColor = true;
            this.useRockbarReserveAddCheckBox.CheckedChanged += new System.EventHandler(this.useRockbarReserveAddCheckBox_CheckedChanged);
            // 
            // tunerTabPage
            // 
            this.tunerTabPage.Controls.Add(this.tunerNameLabel);
            this.tunerTabPage.Controls.Add(this.tunerNameNoteLabel);
            this.tunerTabPage.Controls.Add(this.updateTunerNameButton);
            this.tunerTabPage.Controls.Add(this.tunerNameTextBox);
            this.tunerTabPage.Controls.Add(this.tunerNameListView);
            this.tunerTabPage.Location = new System.Drawing.Point(4, 22);
            this.tunerTabPage.Name = "tunerTabPage";
            this.tunerTabPage.Size = new System.Drawing.Size(791, 384);
            this.tunerTabPage.TabIndex = 5;
            this.tunerTabPage.Text = "チューナー名";
            this.tunerTabPage.UseVisualStyleBackColor = true;
            // 
            // tunerNameLabel
            // 
            this.tunerNameLabel.AutoSize = true;
            this.tunerNameLabel.Location = new System.Drawing.Point(478, 26);
            this.tunerNameLabel.Name = "tunerNameLabel";
            this.tunerNameLabel.Size = new System.Drawing.Size(41, 12);
            this.tunerNameLabel.TabIndex = 27;
            this.tunerNameLabel.Text = "表示名";
            // 
            // tunerNameNoteLabel
            // 
            this.tunerNameNoteLabel.AutoSize = true;
            this.tunerNameNoteLabel.Location = new System.Drawing.Point(475, 60);
            this.tunerNameNoteLabel.Name = "tunerNameNoteLabel";
            this.tunerNameNoteLabel.Size = new System.Drawing.Size(317, 48);
            this.tunerNameNoteLabel.TabIndex = 26;
            this.tunerNameNoteLabel.Text = "※チューナー一覧に表示名+連番(チューナーID下位2byte)で表示\r\n\r\nデフォルトでBonDriver名から推測した表示名を当て込んでいます。\r\nあっていない" +
    "場合もあるので適宜更新してください。";
            // 
            // updateTunerNameButton
            // 
            this.updateTunerNameButton.Location = new System.Drawing.Point(658, 21);
            this.updateTunerNameButton.Name = "updateTunerNameButton";
            this.updateTunerNameButton.Size = new System.Drawing.Size(75, 23);
            this.updateTunerNameButton.TabIndex = 3;
            this.updateTunerNameButton.Text = "更新";
            this.updateTunerNameButton.UseVisualStyleBackColor = true;
            this.updateTunerNameButton.Click += new System.EventHandler(this.updateTunerNameButton_Click);
            // 
            // tunerNameTextBox
            // 
            this.tunerNameTextBox.Location = new System.Drawing.Point(525, 23);
            this.tunerNameTextBox.Name = "tunerNameTextBox";
            this.tunerNameTextBox.Size = new System.Drawing.Size(127, 19);
            this.tunerNameTextBox.TabIndex = 2;
            // 
            // tunerNameListView
            // 
            this.tunerNameListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.tunerNameMarkColumnHeader,
            this.tunerNameTunerIdColumnHeader,
            this.tunerNameBonDriverNameColumnHeader,
            this.tunerNameTunerNameColumnHeader});
            this.tunerNameListView.FullRowSelect = true;
            this.tunerNameListView.HideSelection = false;
            this.tunerNameListView.Location = new System.Drawing.Point(7, 12);
            this.tunerNameListView.MultiSelect = false;
            this.tunerNameListView.Name = "tunerNameListView";
            this.tunerNameListView.Size = new System.Drawing.Size(463, 353);
            this.tunerNameListView.TabIndex = 1;
            this.tunerNameListView.UseCompatibleStateImageBehavior = false;
            this.tunerNameListView.View = System.Windows.Forms.View.Details;
            this.tunerNameListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.tunerNameListView_ColumnClick);
            this.tunerNameListView.SelectedIndexChanged += new System.EventHandler(this.tunerNameListView_SelectedIndexChanged);
            // 
            // tunerNameMarkColumnHeader
            // 
            this.tunerNameMarkColumnHeader.Text = "";
            this.tunerNameMarkColumnHeader.Width = 20;
            // 
            // tunerNameTunerIdColumnHeader
            // 
            this.tunerNameTunerIdColumnHeader.Text = "チューナーID(上位2byte)";
            this.tunerNameTunerIdColumnHeader.Width = 130;
            // 
            // tunerNameBonDriverNameColumnHeader
            // 
            this.tunerNameBonDriverNameColumnHeader.Text = "BonDriver名";
            this.tunerNameBonDriverNameColumnHeader.Width = 160;
            // 
            // tunerNameTunerNameColumnHeader
            // 
            this.tunerNameTunerNameColumnHeader.Text = "表示名";
            this.tunerNameTunerNameColumnHeader.Width = 140;
            // 
            // allServiceTabPage
            // 
            this.allServiceTabPage.Controls.Add(this.selectedServiceListLabel);
            this.allServiceTabPage.Controls.Add(this.allServiceListLabel);
            this.allServiceTabPage.Controls.Add(this.allServiceListView);
            this.allServiceTabPage.Controls.Add(this.moveUpSelectedServiceButton);
            this.allServiceTabPage.Controls.Add(this.moveDownSelectedServiceButton);
            this.allServiceTabPage.Controls.Add(this.removeSelectedServiceButton);
            this.allServiceTabPage.Controls.Add(this.selectedServiceListView);
            this.allServiceTabPage.Controls.Add(this.addSelectedServiceButton);
            this.allServiceTabPage.Controls.Add(this.addNewServiceButton);
            this.allServiceTabPage.Controls.Add(this.editServiceButton);
            this.allServiceTabPage.Location = new System.Drawing.Point(4, 22);
            this.allServiceTabPage.Name = "allServiceTabPage";
            this.allServiceTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.allServiceTabPage.Size = new System.Drawing.Size(791, 384);
            this.allServiceTabPage.TabIndex = 2;
            this.allServiceTabPage.Text = "選択チャンネル";
            this.allServiceTabPage.UseVisualStyleBackColor = true;
            // 
            // selectedServiceListLabel
            // 
            this.selectedServiceListLabel.AutoSize = true;
            this.selectedServiceListLabel.Location = new System.Drawing.Point(403, 10);
            this.selectedServiceListLabel.Name = "selectedServiceListLabel";
            this.selectedServiceListLabel.Size = new System.Drawing.Size(346, 12);
            this.selectedServiceListLabel.TabIndex = 9;
            this.selectedServiceListLabel.Text = "選択チャンネル(\"種類\"が表示タブの決定などに使用されます。編集可。)";
            // 
            // allServiceListLabel
            // 
            this.allServiceListLabel.AutoSize = true;
            this.allServiceListLabel.Location = new System.Drawing.Point(6, 10);
            this.allServiceListLabel.Name = "allServiceListLabel";
            this.allServiceListLabel.Size = new System.Drawing.Size(63, 12);
            this.allServiceListLabel.TabIndex = 8;
            this.allServiceListLabel.Text = "全チャンネル";
            // 
            // favoriteServiceTabPage
            // 
            this.favoriteServiceTabPage.Controls.Add(this.favoriteServiceListLabel);
            this.favoriteServiceTabPage.Controls.Add(this.selectedServiceList2Label);
            this.favoriteServiceTabPage.Controls.Add(this.selectedServiceListView2);
            this.favoriteServiceTabPage.Controls.Add(this.moveUpFavoriteServiceButton);
            this.favoriteServiceTabPage.Controls.Add(this.moveDownFavoriteServiceButton);
            this.favoriteServiceTabPage.Controls.Add(this.removeFavoriteServiceButton);
            this.favoriteServiceTabPage.Controls.Add(this.favoriteServiceListView);
            this.favoriteServiceTabPage.Controls.Add(this.addFavoriteServiceButton);
            this.favoriteServiceTabPage.Location = new System.Drawing.Point(4, 22);
            this.favoriteServiceTabPage.Name = "favoriteServiceTabPage";
            this.favoriteServiceTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.favoriteServiceTabPage.Size = new System.Drawing.Size(791, 384);
            this.favoriteServiceTabPage.TabIndex = 1;
            this.favoriteServiceTabPage.Text = "お気に入りチャンネル";
            this.favoriteServiceTabPage.UseVisualStyleBackColor = true;
            // 
            // favoriteServiceListLabel
            // 
            this.favoriteServiceListLabel.AutoSize = true;
            this.favoriteServiceListLabel.Location = new System.Drawing.Point(403, 11);
            this.favoriteServiceListLabel.Name = "favoriteServiceListLabel";
            this.favoriteServiceListLabel.Size = new System.Drawing.Size(102, 12);
            this.favoriteServiceListLabel.TabIndex = 15;
            this.favoriteServiceListLabel.Text = "お気に入りチャンネル";
            // 
            // selectedServiceList2Label
            // 
            this.selectedServiceList2Label.AutoSize = true;
            this.selectedServiceList2Label.Location = new System.Drawing.Point(7, 11);
            this.selectedServiceList2Label.Name = "selectedServiceList2Label";
            this.selectedServiceList2Label.Size = new System.Drawing.Size(75, 12);
            this.selectedServiceList2Label.TabIndex = 14;
            this.selectedServiceList2Label.Text = "選択チャンネル";
            // 
            // selectedServiceListView2
            // 
            this.selectedServiceListView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.selectedService2MarkColumnHeader,
            this.selectedService2NetworkTypeColumnHeader,
            this.selectedService2NameColumnHeader,
            this.selectedService2TsidColumnHeader,
            this.selectedService2SidColumnHeader,
            this.selectedService2TvtestOptionColumnHeader});
            this.selectedServiceListView2.FullRowSelect = true;
            this.selectedServiceListView2.HideSelection = false;
            this.selectedServiceListView2.Location = new System.Drawing.Point(8, 26);
            this.selectedServiceListView2.Name = "selectedServiceListView2";
            this.selectedServiceListView2.Size = new System.Drawing.Size(333, 353);
            this.selectedServiceListView2.TabIndex = 0;
            this.selectedServiceListView2.UseCompatibleStateImageBehavior = false;
            this.selectedServiceListView2.View = System.Windows.Forms.View.Details;
            this.selectedServiceListView2.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.selectedServiceListView2_ColumnClick);
            // 
            // selectedService2MarkColumnHeader
            // 
            this.selectedService2MarkColumnHeader.Text = "";
            this.selectedService2MarkColumnHeader.Width = 20;
            // 
            // selectedService2NetworkTypeColumnHeader
            // 
            this.selectedService2NetworkTypeColumnHeader.Text = "種類";
            this.selectedService2NetworkTypeColumnHeader.Width = 48;
            // 
            // selectedService2NameColumnHeader
            // 
            this.selectedService2NameColumnHeader.Text = "名前";
            this.selectedService2NameColumnHeader.Width = 130;
            // 
            // selectedService2TsidColumnHeader
            // 
            this.selectedService2TsidColumnHeader.Text = "TSID";
            this.selectedService2TsidColumnHeader.Width = 57;
            // 
            // selectedService2SidColumnHeader
            // 
            this.selectedService2SidColumnHeader.Text = "SID";
            this.selectedService2SidColumnHeader.Width = 57;
            // 
            // selectedService2TvtestOptionColumnHeader
            // 
            this.selectedService2TvtestOptionColumnHeader.Text = "TVTestオプション";
            this.selectedService2TvtestOptionColumnHeader.Width = 0;
            // 
            // moveUpFavoriteServiceButton
            // 
            this.moveUpFavoriteServiceButton.Location = new System.Drawing.Point(744, 169);
            this.moveUpFavoriteServiceButton.Name = "moveUpFavoriteServiceButton";
            this.moveUpFavoriteServiceButton.Size = new System.Drawing.Size(40, 23);
            this.moveUpFavoriteServiceButton.TabIndex = 4;
            this.moveUpFavoriteServiceButton.Text = "↑";
            this.moveUpFavoriteServiceButton.UseVisualStyleBackColor = true;
            this.moveUpFavoriteServiceButton.Click += new System.EventHandler(this.moveUpFavoriteServiceButton_Click);
            // 
            // moveDownFavoriteServiceButton
            // 
            this.moveDownFavoriteServiceButton.Location = new System.Drawing.Point(744, 208);
            this.moveDownFavoriteServiceButton.Name = "moveDownFavoriteServiceButton";
            this.moveDownFavoriteServiceButton.Size = new System.Drawing.Size(40, 23);
            this.moveDownFavoriteServiceButton.TabIndex = 5;
            this.moveDownFavoriteServiceButton.Text = "↓";
            this.moveDownFavoriteServiceButton.UseVisualStyleBackColor = true;
            this.moveDownFavoriteServiceButton.Click += new System.EventHandler(this.moveDownFavoriteServiceButton_Click);
            // 
            // removeFavoriteServiceButton
            // 
            this.removeFavoriteServiceButton.Location = new System.Drawing.Point(347, 243);
            this.removeFavoriteServiceButton.Name = "removeFavoriteServiceButton";
            this.removeFavoriteServiceButton.Size = new System.Drawing.Size(51, 23);
            this.removeFavoriteServiceButton.TabIndex = 3;
            this.removeFavoriteServiceButton.Text = "<<";
            this.removeFavoriteServiceButton.UseVisualStyleBackColor = true;
            this.removeFavoriteServiceButton.Click += new System.EventHandler(this.removeFavoriteServiceButton_Click);
            // 
            // favoriteServiceListView
            // 
            this.favoriteServiceListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.favoriteServiceMarkColumnHeader,
            this.favoriteServiceNetworkTypeColumnHeader,
            this.favoriteServiceNameColumnHeader,
            this.favoriteServiceTsidColumnHeader,
            this.favoriteServiceSidColumnHeader,
            this.favoriteServiceTvtestOptionColumnHeader});
            this.favoriteServiceListView.FullRowSelect = true;
            this.favoriteServiceListView.HideSelection = false;
            this.favoriteServiceListView.Location = new System.Drawing.Point(405, 26);
            this.favoriteServiceListView.Name = "favoriteServiceListView";
            this.favoriteServiceListView.Size = new System.Drawing.Size(333, 353);
            this.favoriteServiceListView.TabIndex = 1;
            this.favoriteServiceListView.UseCompatibleStateImageBehavior = false;
            this.favoriteServiceListView.View = System.Windows.Forms.View.Details;
            this.favoriteServiceListView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.favoriteServiceListView_ColumnClick);
            // 
            // favoriteServiceMarkColumnHeader
            // 
            this.favoriteServiceMarkColumnHeader.Text = "";
            this.favoriteServiceMarkColumnHeader.Width = 20;
            // 
            // favoriteServiceNetworkTypeColumnHeader
            // 
            this.favoriteServiceNetworkTypeColumnHeader.Text = "種類";
            this.favoriteServiceNetworkTypeColumnHeader.Width = 48;
            // 
            // favoriteServiceNameColumnHeader
            // 
            this.favoriteServiceNameColumnHeader.Text = "名前";
            this.favoriteServiceNameColumnHeader.Width = 130;
            // 
            // favoriteServiceTsidColumnHeader
            // 
            this.favoriteServiceTsidColumnHeader.Text = "TSID";
            this.favoriteServiceTsidColumnHeader.Width = 50;
            // 
            // favoriteServiceSidColumnHeader
            // 
            this.favoriteServiceSidColumnHeader.Text = "SID";
            this.favoriteServiceSidColumnHeader.Width = 50;
            // 
            // favoriteServiceTvtestOptionColumnHeader
            // 
            this.favoriteServiceTvtestOptionColumnHeader.Text = "TVTestオプション";
            this.favoriteServiceTvtestOptionColumnHeader.Width = 180;
            // 
            // addFavoriteServiceButton
            // 
            this.addFavoriteServiceButton.Location = new System.Drawing.Point(347, 139);
            this.addFavoriteServiceButton.Name = "addFavoriteServiceButton";
            this.addFavoriteServiceButton.Size = new System.Drawing.Size(51, 23);
            this.addFavoriteServiceButton.TabIndex = 2;
            this.addFavoriteServiceButton.Text = ">>";
            this.addFavoriteServiceButton.UseVisualStyleBackColor = true;
            this.addFavoriteServiceButton.Click += new System.EventHandler(this.addFavoriteServiceButton_Click);
            // 
            // tvtestLinkageTabPage
            // 
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestBs4kOptionLabel);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestSphdOptionLabel);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestBs4kOptionTextBox);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestSphdOptionTextBox);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestCatvOptionTextBox);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestCatvOptionLabel);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestOptionExampleLabel);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestTsFileOptionExampleLabel);
            this.tvtestLinkageTabPage.Controls.Add(this.autoStartTargetGroupBox);
            this.tvtestLinkageTabPage.Controls.Add(this.autoCloseMarginNumericUpDown);
            this.tvtestLinkageTabPage.Controls.Add(this.autoCloseMarginLabel);
            this.tvtestLinkageTabPage.Controls.Add(this.autoOpenMarginNumericUpDown);
            this.tvtestLinkageTabPage.Controls.Add(this.autoOpenMarginLabel);
            this.tvtestLinkageTabPage.Controls.Add(this.isAutoOpenTvtestCheckBox);
            this.tvtestLinkageTabPage.Controls.Add(this.tvTestNoteLabel);
            this.tvtestLinkageTabPage.Controls.Add(this.useDoubleClickTvtestCheckBox);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestDttvOptionLabel);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestDttvOptionTextBox);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestBscsOptionLabel);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestBscsOptionTextBox);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestTsFileOptionLabel);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestTsFileOptionTextBox);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestPathLabel);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestOpenButton);
            this.tvtestLinkageTabPage.Controls.Add(this.tvtestPathTextBox);
            this.tvtestLinkageTabPage.Location = new System.Drawing.Point(4, 22);
            this.tvtestLinkageTabPage.Name = "tvtestLinkageTabPage";
            this.tvtestLinkageTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.tvtestLinkageTabPage.Size = new System.Drawing.Size(791, 384);
            this.tvtestLinkageTabPage.TabIndex = 3;
            this.tvtestLinkageTabPage.Text = "TVTest連携";
            this.tvtestLinkageTabPage.UseVisualStyleBackColor = true;
            // 
            // tvtestBs4kOptionLabel
            // 
            this.tvtestBs4kOptionLabel.AutoSize = true;
            this.tvtestBs4kOptionLabel.Location = new System.Drawing.Point(14, 147);
            this.tvtestBs4kOptionLabel.Name = "tvtestBs4kOptionLabel";
            this.tvtestBs4kOptionLabel.Size = new System.Drawing.Size(76, 12);
            this.tvtestBs4kOptionLabel.TabIndex = 32;
            this.tvtestBs4kOptionLabel.Text = "BS4Kオプション";
            // 
            // tvtestSphdOptionLabel
            // 
            this.tvtestSphdOptionLabel.AutoSize = true;
            this.tvtestSphdOptionLabel.Location = new System.Drawing.Point(14, 122);
            this.tvtestSphdOptionLabel.Name = "tvtestSphdOptionLabel";
            this.tvtestSphdOptionLabel.Size = new System.Drawing.Size(78, 12);
            this.tvtestSphdOptionLabel.TabIndex = 31;
            this.tvtestSphdOptionLabel.Text = "SPHDオプション";
            // 
            // tvtestBs4kOptionTextBox
            // 
            this.tvtestBs4kOptionTextBox.Location = new System.Drawing.Point(102, 144);
            this.tvtestBs4kOptionTextBox.Name = "tvtestBs4kOptionTextBox";
            this.tvtestBs4kOptionTextBox.Size = new System.Drawing.Size(312, 19);
            this.tvtestBs4kOptionTextBox.TabIndex = 30;
            // 
            // tvtestSphdOptionTextBox
            // 
            this.tvtestSphdOptionTextBox.Location = new System.Drawing.Point(102, 119);
            this.tvtestSphdOptionTextBox.Name = "tvtestSphdOptionTextBox";
            this.tvtestSphdOptionTextBox.Size = new System.Drawing.Size(312, 19);
            this.tvtestSphdOptionTextBox.TabIndex = 29;
            // 
            // tvtestCatvOptionTextBox
            // 
            this.tvtestCatvOptionTextBox.Location = new System.Drawing.Point(102, 94);
            this.tvtestCatvOptionTextBox.Name = "tvtestCatvOptionTextBox";
            this.tvtestCatvOptionTextBox.Size = new System.Drawing.Size(312, 19);
            this.tvtestCatvOptionTextBox.TabIndex = 28;
            // 
            // tvtestCatvOptionLabel
            // 
            this.tvtestCatvOptionLabel.AutoSize = true;
            this.tvtestCatvOptionLabel.Location = new System.Drawing.Point(14, 97);
            this.tvtestCatvOptionLabel.Name = "tvtestCatvOptionLabel";
            this.tvtestCatvOptionLabel.Size = new System.Drawing.Size(79, 12);
            this.tvtestCatvOptionLabel.TabIndex = 27;
            this.tvtestCatvOptionLabel.Text = "CATVオプション";
            // 
            // tvtestOptionExampleLabel
            // 
            this.tvtestOptionExampleLabel.AutoSize = true;
            this.tvtestOptionExampleLabel.Location = new System.Drawing.Point(424, 47);
            this.tvtestOptionExampleLabel.Name = "tvtestOptionExampleLabel";
            this.tvtestOptionExampleLabel.Size = new System.Drawing.Size(266, 96);
            this.tvtestOptionExampleLabel.TabIndex = 25;
            this.tvtestOptionExampleLabel.Text = resources.GetString("tvtestOptionExampleLabel.Text");
            // 
            // tvtestTsFileOptionExampleLabel
            // 
            this.tvtestTsFileOptionExampleLabel.AutoSize = true;
            this.tvtestTsFileOptionExampleLabel.Location = new System.Drawing.Point(424, 151);
            this.tvtestTsFileOptionExampleLabel.Name = "tvtestTsFileOptionExampleLabel";
            this.tvtestTsFileOptionExampleLabel.Size = new System.Drawing.Size(215, 36);
            this.tvtestTsFileOptionExampleLabel.TabIndex = 26;
            this.tvtestTsFileOptionExampleLabel.Text = "(例: TvtPlayでTS再生)\r\n/d BonDriver_Pipe.dll\r\n※TvtPlayの仕様上 /s は付けない方がよい";
            // 
            // autoStartTargetGroupBox
            // 
            this.autoStartTargetGroupBox.Controls.Add(this.isAutoOpenSphdCheckBox);
            this.autoStartTargetGroupBox.Controls.Add(this.isAutoOpenBs4kCheckBox);
            this.autoStartTargetGroupBox.Controls.Add(this.isAutoOpenCatvCheckBox);
            this.autoStartTargetGroupBox.Controls.Add(this.isAutoOpenFavoriteServiceCheckBox);
            this.autoStartTargetGroupBox.Controls.Add(this.isAutoOpenDttvCheckBox);
            this.autoStartTargetGroupBox.Controls.Add(this.isAutoOpenCsCheckBox);
            this.autoStartTargetGroupBox.Controls.Add(this.isAutoOpenBsCheckBox);
            this.autoStartTargetGroupBox.Location = new System.Drawing.Point(36, 285);
            this.autoStartTargetGroupBox.Name = "autoStartTargetGroupBox";
            this.autoStartTargetGroupBox.Size = new System.Drawing.Size(229, 88);
            this.autoStartTargetGroupBox.TabIndex = 7;
            this.autoStartTargetGroupBox.TabStop = false;
            this.autoStartTargetGroupBox.Text = "対象種別";
            // 
            // isAutoOpenSphdCheckBox
            // 
            this.isAutoOpenSphdCheckBox.AutoSize = true;
            this.isAutoOpenSphdCheckBox.Location = new System.Drawing.Point(141, 36);
            this.isAutoOpenSphdCheckBox.Name = "isAutoOpenSphdCheckBox";
            this.isAutoOpenSphdCheckBox.Size = new System.Drawing.Size(54, 16);
            this.isAutoOpenSphdCheckBox.TabIndex = 13;
            this.isAutoOpenSphdCheckBox.Text = "SPHD";
            this.isAutoOpenSphdCheckBox.UseVisualStyleBackColor = true;
            // 
            // isAutoOpenBs4kCheckBox
            // 
            this.isAutoOpenBs4kCheckBox.AutoSize = true;
            this.isAutoOpenBs4kCheckBox.Location = new System.Drawing.Point(79, 36);
            this.isAutoOpenBs4kCheckBox.Name = "isAutoOpenBs4kCheckBox";
            this.isAutoOpenBs4kCheckBox.Size = new System.Drawing.Size(52, 16);
            this.isAutoOpenBs4kCheckBox.TabIndex = 12;
            this.isAutoOpenBs4kCheckBox.Text = "BS4K";
            this.isAutoOpenBs4kCheckBox.UseVisualStyleBackColor = true;
            // 
            // isAutoOpenCatvCheckBox
            // 
            this.isAutoOpenCatvCheckBox.AutoSize = true;
            this.isAutoOpenCatvCheckBox.Location = new System.Drawing.Point(7, 36);
            this.isAutoOpenCatvCheckBox.Name = "isAutoOpenCatvCheckBox";
            this.isAutoOpenCatvCheckBox.Size = new System.Drawing.Size(55, 16);
            this.isAutoOpenCatvCheckBox.TabIndex = 11;
            this.isAutoOpenCatvCheckBox.Text = "CATV";
            this.isAutoOpenCatvCheckBox.UseVisualStyleBackColor = true;
            // 
            // isAutoOpenFavoriteServiceCheckBox
            // 
            this.isAutoOpenFavoriteServiceCheckBox.AutoSize = true;
            this.isAutoOpenFavoriteServiceCheckBox.Location = new System.Drawing.Point(7, 56);
            this.isAutoOpenFavoriteServiceCheckBox.Name = "isAutoOpenFavoriteServiceCheckBox";
            this.isAutoOpenFavoriteServiceCheckBox.Size = new System.Drawing.Size(158, 28);
            this.isAutoOpenFavoriteServiceCheckBox.TabIndex = 10;
            this.isAutoOpenFavoriteServiceCheckBox.Text = "お気に入りチャンネルのみ\r\n(上記対象種別との積集合)";
            this.isAutoOpenFavoriteServiceCheckBox.UseVisualStyleBackColor = true;
            // 
            // isAutoOpenDttvCheckBox
            // 
            this.isAutoOpenDttvCheckBox.AutoSize = true;
            this.isAutoOpenDttvCheckBox.Location = new System.Drawing.Point(7, 16);
            this.isAutoOpenDttvCheckBox.Name = "isAutoOpenDttvCheckBox";
            this.isAutoOpenDttvCheckBox.Size = new System.Drawing.Size(56, 16);
            this.isAutoOpenDttvCheckBox.TabIndex = 7;
            this.isAutoOpenDttvCheckBox.Text = "地デジ";
            this.isAutoOpenDttvCheckBox.UseVisualStyleBackColor = true;
            // 
            // isAutoOpenCsCheckBox
            // 
            this.isAutoOpenCsCheckBox.AutoSize = true;
            this.isAutoOpenCsCheckBox.Location = new System.Drawing.Point(141, 16);
            this.isAutoOpenCsCheckBox.Name = "isAutoOpenCsCheckBox";
            this.isAutoOpenCsCheckBox.Size = new System.Drawing.Size(39, 16);
            this.isAutoOpenCsCheckBox.TabIndex = 9;
            this.isAutoOpenCsCheckBox.Text = "CS";
            this.isAutoOpenCsCheckBox.UseVisualStyleBackColor = true;
            // 
            // isAutoOpenBsCheckBox
            // 
            this.isAutoOpenBsCheckBox.AutoSize = true;
            this.isAutoOpenBsCheckBox.Location = new System.Drawing.Point(79, 16);
            this.isAutoOpenBsCheckBox.Name = "isAutoOpenBsCheckBox";
            this.isAutoOpenBsCheckBox.Size = new System.Drawing.Size(39, 16);
            this.isAutoOpenBsCheckBox.TabIndex = 8;
            this.isAutoOpenBsCheckBox.Text = "BS";
            this.isAutoOpenBsCheckBox.UseVisualStyleBackColor = true;
            // 
            // autoCloseMarginNumericUpDown
            // 
            this.autoCloseMarginNumericUpDown.Location = new System.Drawing.Point(445, 320);
            this.autoCloseMarginNumericUpDown.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.autoCloseMarginNumericUpDown.Name = "autoCloseMarginNumericUpDown";
            this.autoCloseMarginNumericUpDown.Size = new System.Drawing.Size(70, 19);
            this.autoCloseMarginNumericUpDown.TabIndex = 12;
            this.autoCloseMarginNumericUpDown.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // autoCloseMarginLabel
            // 
            this.autoCloseMarginLabel.AutoSize = true;
            this.autoCloseMarginLabel.Location = new System.Drawing.Point(285, 323);
            this.autoCloseMarginLabel.Name = "autoCloseMarginLabel";
            this.autoCloseMarginLabel.Size = new System.Drawing.Size(153, 12);
            this.autoCloseMarginLabel.TabIndex = 23;
            this.autoCloseMarginLabel.Text = "終了マージン(0～59秒後終了)";
            // 
            // autoOpenMarginNumericUpDown
            // 
            this.autoOpenMarginNumericUpDown.Location = new System.Drawing.Point(444, 291);
            this.autoOpenMarginNumericUpDown.Maximum = new decimal(new int[] {
            59,
            0,
            0,
            0});
            this.autoOpenMarginNumericUpDown.Name = "autoOpenMarginNumericUpDown";
            this.autoOpenMarginNumericUpDown.Size = new System.Drawing.Size(70, 19);
            this.autoOpenMarginNumericUpDown.TabIndex = 11;
            this.autoOpenMarginNumericUpDown.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            // 
            // autoOpenMarginLabel
            // 
            this.autoOpenMarginLabel.AutoSize = true;
            this.autoOpenMarginLabel.Location = new System.Drawing.Point(285, 294);
            this.autoOpenMarginLabel.Name = "autoOpenMarginLabel";
            this.autoOpenMarginLabel.Size = new System.Drawing.Size(153, 12);
            this.autoOpenMarginLabel.TabIndex = 21;
            this.autoOpenMarginLabel.Text = "開始マージン(0～59秒前起動)";
            // 
            // isAutoOpenTvtestCheckBox
            // 
            this.isAutoOpenTvtestCheckBox.AutoSize = true;
            this.isAutoOpenTvtestCheckBox.Location = new System.Drawing.Point(16, 261);
            this.isAutoOpenTvtestCheckBox.Name = "isAutoOpenTvtestCheckBox";
            this.isAutoOpenTvtestCheckBox.Size = new System.Drawing.Size(272, 16);
            this.isAutoOpenTvtestCheckBox.TabIndex = 6;
            this.isAutoOpenTvtestCheckBox.Text = "予約時間に合わせてTVTestを自動起動／終了する";
            this.isAutoOpenTvtestCheckBox.UseVisualStyleBackColor = true;
            // 
            // tvTestNoteLabel
            // 
            this.tvTestNoteLabel.AutoSize = true;
            this.tvTestNoteLabel.ForeColor = System.Drawing.Color.Red;
            this.tvTestNoteLabel.Location = new System.Drawing.Point(100, 192);
            this.tvTestNoteLabel.Name = "tvTestNoteLabel";
            this.tvTestNoteLabel.Size = new System.Drawing.Size(545, 36);
            this.tvTestNoteLabel.TabIndex = 19;
            this.tvTestNoteLabel.Text = "/TSID /SIDオプションは自動付与します。\r\n！！注意！！ チューナー共有前提です。\r\nチューナー共有していない場合、チューナーを占有してしまうことにより録" +
    "画が失敗することがあるため注意してください。";
            // 
            // useDoubleClickTvtestCheckBox
            // 
            this.useDoubleClickTvtestCheckBox.AutoSize = true;
            this.useDoubleClickTvtestCheckBox.Location = new System.Drawing.Point(16, 236);
            this.useDoubleClickTvtestCheckBox.Name = "useDoubleClickTvtestCheckBox";
            this.useDoubleClickTvtestCheckBox.Size = new System.Drawing.Size(209, 16);
            this.useDoubleClickTvtestCheckBox.TabIndex = 5;
            this.useDoubleClickTvtestCheckBox.Text = "チャンネルダブルクリックでTVTestを起動";
            this.useDoubleClickTvtestCheckBox.UseVisualStyleBackColor = true;
            // 
            // tvtestDttvOptionLabel
            // 
            this.tvtestDttvOptionLabel.AutoSize = true;
            this.tvtestDttvOptionLabel.Location = new System.Drawing.Point(14, 47);
            this.tvtestDttvOptionLabel.Name = "tvtestDttvOptionLabel";
            this.tvtestDttvOptionLabel.Size = new System.Drawing.Size(80, 12);
            this.tvtestDttvOptionLabel.TabIndex = 17;
            this.tvtestDttvOptionLabel.Text = "地デジオプション";
            // 
            // tvtestDttvOptionTextBox
            // 
            this.tvtestDttvOptionTextBox.Location = new System.Drawing.Point(102, 44);
            this.tvtestDttvOptionTextBox.Name = "tvtestDttvOptionTextBox";
            this.tvtestDttvOptionTextBox.Size = new System.Drawing.Size(312, 19);
            this.tvtestDttvOptionTextBox.TabIndex = 3;
            // 
            // tvtestBscsOptionLabel
            // 
            this.tvtestBscsOptionLabel.AutoSize = true;
            this.tvtestBscsOptionLabel.Location = new System.Drawing.Point(14, 72);
            this.tvtestBscsOptionLabel.Name = "tvtestBscsOptionLabel";
            this.tvtestBscsOptionLabel.Size = new System.Drawing.Size(84, 12);
            this.tvtestBscsOptionLabel.TabIndex = 15;
            this.tvtestBscsOptionLabel.Text = "BS/CSオプション";
            // 
            // tvtestBscsOptionTextBox
            // 
            this.tvtestBscsOptionTextBox.Location = new System.Drawing.Point(102, 69);
            this.tvtestBscsOptionTextBox.Name = "tvtestBscsOptionTextBox";
            this.tvtestBscsOptionTextBox.Size = new System.Drawing.Size(312, 19);
            this.tvtestBscsOptionTextBox.TabIndex = 2;
            // 
            // tvtestTsFileOptionLabel
            // 
            this.tvtestTsFileOptionLabel.AutoSize = true;
            this.tvtestTsFileOptionLabel.Location = new System.Drawing.Point(14, 172);
            this.tvtestTsFileOptionLabel.Name = "tvtestTsFileOptionLabel";
            this.tvtestTsFileOptionLabel.Size = new System.Drawing.Size(86, 12);
            this.tvtestTsFileOptionLabel.TabIndex = 18;
            this.tvtestTsFileOptionLabel.Text = "TS再生オプション";
            // 
            // tvtestTsFileOptionTextBox
            // 
            this.tvtestTsFileOptionTextBox.Location = new System.Drawing.Point(102, 169);
            this.tvtestTsFileOptionTextBox.Name = "tvtestTsFileOptionTextBox";
            this.tvtestTsFileOptionTextBox.Size = new System.Drawing.Size(312, 19);
            this.tvtestTsFileOptionTextBox.TabIndex = 4;
            // 
            // tvtestPathLabel
            // 
            this.tvtestPathLabel.AutoSize = true;
            this.tvtestPathLabel.Location = new System.Drawing.Point(14, 22);
            this.tvtestPathLabel.Name = "tvtestPathLabel";
            this.tvtestPathLabel.Size = new System.Drawing.Size(82, 12);
            this.tvtestPathLabel.TabIndex = 13;
            this.tvtestPathLabel.Text = "TVTest.exeパス";
            // 
            // tvtestOpenButton
            // 
            this.tvtestOpenButton.Location = new System.Drawing.Point(420, 17);
            this.tvtestOpenButton.Name = "tvtestOpenButton";
            this.tvtestOpenButton.Size = new System.Drawing.Size(75, 23);
            this.tvtestOpenButton.TabIndex = 1;
            this.tvtestOpenButton.Text = "参照...";
            this.tvtestOpenButton.UseVisualStyleBackColor = true;
            this.tvtestOpenButton.Click += new System.EventHandler(this.tvtestOpenButton_Click);
            // 
            // tvtestPathTextBox
            // 
            this.tvtestPathTextBox.Location = new System.Drawing.Point(102, 19);
            this.tvtestPathTextBox.Name = "tvtestPathTextBox";
            this.tvtestPathTextBox.Size = new System.Drawing.Size(312, 19);
            this.tvtestPathTextBox.TabIndex = 0;
            // 
            // listViewContColorTabPage
            // 
            this.listViewContColorTabPage.Controls.Add(this.ngReserveListBackColorLabel);
            this.listViewContColorTabPage.Controls.Add(this.ngReserveListBackColorTextBox);
            this.listViewContColorTabPage.Controls.Add(this.selectNgReserveListBackColorButton);
            this.listViewContColorTabPage.Controls.Add(this.partialReserveListBackColorLabel);
            this.listViewContColorTabPage.Controls.Add(this.partialReserveListBackColorTextBox);
            this.listViewContColorTabPage.Controls.Add(this.selectPartialReserveListBackColorButton);
            this.listViewContColorTabPage.Controls.Add(this.okReserveListBackColorLabel);
            this.listViewContColorTabPage.Controls.Add(this.okReserveListBackColorTextBox);
            this.listViewContColorTabPage.Controls.Add(this.selectOkReserveListBackColorButton);
            this.listViewContColorTabPage.Controls.Add(this.disabledReserveListBackColorLabel);
            this.listViewContColorTabPage.Controls.Add(this.disabledReserveListBackColorTextBox);
            this.listViewContColorTabPage.Controls.Add(this.selectDisabledReserveListBackColorButton);
            this.listViewContColorTabPage.Controls.Add(this.listHeaderForeColorLabel);
            this.listViewContColorTabPage.Controls.Add(this.listHeaderForeColorTextBox);
            this.listViewContColorTabPage.Controls.Add(this.selectListHeaderForeColorButton);
            this.listViewContColorTabPage.Controls.Add(this.listHeaderBackColorLabel);
            this.listViewContColorTabPage.Controls.Add(this.listHeaderBackColorTextBox);
            this.listViewContColorTabPage.Controls.Add(this.selectListHeaderBackColorButton);
            this.listViewContColorTabPage.Controls.Add(this.previewListView);
            this.listViewContColorTabPage.Controls.Add(this.previewLabel);
            this.listViewContColorTabPage.Controls.Add(this.foreColorLabel);
            this.listViewContColorTabPage.Controls.Add(this.foreColorTextBox);
            this.listViewContColorTabPage.Controls.Add(this.selectForeColorButton);
            this.listViewContColorTabPage.Controls.Add(this.listBackColorLabel);
            this.listViewContColorTabPage.Controls.Add(this.listBackColorTextBox);
            this.listViewContColorTabPage.Controls.Add(this.selectListBackColorButton);
            this.listViewContColorTabPage.Controls.Add(this.previewFormPanel);
            this.listViewContColorTabPage.Controls.Add(this.formBackColorLabel);
            this.listViewContColorTabPage.Controls.Add(this.formBackColorTextBox);
            this.listViewContColorTabPage.Controls.Add(this.selectFormBackColorButton);
            this.listViewContColorTabPage.Controls.Add(this.fontLabel);
            this.listViewContColorTabPage.Controls.Add(this.fontTextBox);
            this.listViewContColorTabPage.Controls.Add(this.selectFontButton);
            this.listViewContColorTabPage.Location = new System.Drawing.Point(4, 22);
            this.listViewContColorTabPage.Name = "listViewContColorTabPage";
            this.listViewContColorTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.listViewContColorTabPage.Size = new System.Drawing.Size(791, 384);
            this.listViewContColorTabPage.TabIndex = 6;
            this.listViewContColorTabPage.Text = "フォント・色";
            this.listViewContColorTabPage.UseVisualStyleBackColor = true;
            // 
            // ngReserveListBackColorLabel
            // 
            this.ngReserveListBackColorLabel.AutoSize = true;
            this.ngReserveListBackColorLabel.Location = new System.Drawing.Point(9, 243);
            this.ngReserveListBackColorLabel.Name = "ngReserveListBackColorLabel";
            this.ngReserveListBackColorLabel.Size = new System.Drawing.Size(133, 12);
            this.ngReserveListBackColorLabel.TabIndex = 61;
            this.ngReserveListBackColorLabel.Text = "リスト背景色(予約不可等)";
            // 
            // ngReserveListBackColorTextBox
            // 
            this.ngReserveListBackColorTextBox.Location = new System.Drawing.Point(148, 240);
            this.ngReserveListBackColorTextBox.Name = "ngReserveListBackColorTextBox";
            this.ngReserveListBackColorTextBox.ReadOnly = true;
            this.ngReserveListBackColorTextBox.Size = new System.Drawing.Size(136, 19);
            this.ngReserveListBackColorTextBox.TabIndex = 60;
            // 
            // selectNgReserveListBackColorButton
            // 
            this.selectNgReserveListBackColorButton.Location = new System.Drawing.Point(290, 238);
            this.selectNgReserveListBackColorButton.Name = "selectNgReserveListBackColorButton";
            this.selectNgReserveListBackColorButton.Size = new System.Drawing.Size(75, 23);
            this.selectNgReserveListBackColorButton.TabIndex = 59;
            this.selectNgReserveListBackColorButton.Text = "選択";
            this.selectNgReserveListBackColorButton.UseVisualStyleBackColor = true;
            this.selectNgReserveListBackColorButton.Click += new System.EventHandler(this.selectNgReserveListBackColorButton_Click);
            // 
            // partialReserveListBackColorLabel
            // 
            this.partialReserveListBackColorLabel.AutoSize = true;
            this.partialReserveListBackColorLabel.Location = new System.Drawing.Point(9, 205);
            this.partialReserveListBackColorLabel.Name = "partialReserveListBackColorLabel";
            this.partialReserveListBackColorLabel.Size = new System.Drawing.Size(133, 12);
            this.partialReserveListBackColorLabel.TabIndex = 58;
            this.partialReserveListBackColorLabel.Text = "リスト背景色(部分予約等)";
            // 
            // partialReserveListBackColorTextBox
            // 
            this.partialReserveListBackColorTextBox.Location = new System.Drawing.Point(148, 202);
            this.partialReserveListBackColorTextBox.Name = "partialReserveListBackColorTextBox";
            this.partialReserveListBackColorTextBox.ReadOnly = true;
            this.partialReserveListBackColorTextBox.Size = new System.Drawing.Size(136, 19);
            this.partialReserveListBackColorTextBox.TabIndex = 57;
            // 
            // selectPartialReserveListBackColorButton
            // 
            this.selectPartialReserveListBackColorButton.Location = new System.Drawing.Point(290, 200);
            this.selectPartialReserveListBackColorButton.Name = "selectPartialReserveListBackColorButton";
            this.selectPartialReserveListBackColorButton.Size = new System.Drawing.Size(75, 23);
            this.selectPartialReserveListBackColorButton.TabIndex = 56;
            this.selectPartialReserveListBackColorButton.Text = "選択";
            this.selectPartialReserveListBackColorButton.UseVisualStyleBackColor = true;
            this.selectPartialReserveListBackColorButton.Click += new System.EventHandler(this.selectPartialReserveListBackColorButton_Click);
            // 
            // okReserveListBackColorLabel
            // 
            this.okReserveListBackColorLabel.AutoSize = true;
            this.okReserveListBackColorLabel.Location = new System.Drawing.Point(9, 167);
            this.okReserveListBackColorLabel.Name = "okReserveListBackColorLabel";
            this.okReserveListBackColorLabel.Size = new System.Drawing.Size(133, 12);
            this.okReserveListBackColorLabel.TabIndex = 55;
            this.okReserveListBackColorLabel.Text = "リスト背景色(正常予約等)";
            // 
            // okReserveListBackColorTextBox
            // 
            this.okReserveListBackColorTextBox.Location = new System.Drawing.Point(148, 164);
            this.okReserveListBackColorTextBox.Name = "okReserveListBackColorTextBox";
            this.okReserveListBackColorTextBox.ReadOnly = true;
            this.okReserveListBackColorTextBox.Size = new System.Drawing.Size(136, 19);
            this.okReserveListBackColorTextBox.TabIndex = 54;
            // 
            // selectOkReserveListBackColorButton
            // 
            this.selectOkReserveListBackColorButton.Location = new System.Drawing.Point(290, 162);
            this.selectOkReserveListBackColorButton.Name = "selectOkReserveListBackColorButton";
            this.selectOkReserveListBackColorButton.Size = new System.Drawing.Size(75, 23);
            this.selectOkReserveListBackColorButton.TabIndex = 53;
            this.selectOkReserveListBackColorButton.Text = "選択";
            this.selectOkReserveListBackColorButton.UseVisualStyleBackColor = true;
            this.selectOkReserveListBackColorButton.Click += new System.EventHandler(this.selectOkReserveListBackColorButton_Click);
            // 
            // disabledReserveListBackColorLabel
            // 
            this.disabledReserveListBackColorLabel.AutoSize = true;
            this.disabledReserveListBackColorLabel.Location = new System.Drawing.Point(9, 281);
            this.disabledReserveListBackColorLabel.Name = "disabledReserveListBackColorLabel";
            this.disabledReserveListBackColorLabel.Size = new System.Drawing.Size(133, 12);
            this.disabledReserveListBackColorLabel.TabIndex = 61;
            this.disabledReserveListBackColorLabel.Text = "リスト背景色(無効予約等)";
            // 
            // disabledReserveListBackColorTextBox
            // 
            this.disabledReserveListBackColorTextBox.Location = new System.Drawing.Point(148, 278);
            this.disabledReserveListBackColorTextBox.Name = "disabledReserveListBackColorTextBox";
            this.disabledReserveListBackColorTextBox.ReadOnly = true;
            this.disabledReserveListBackColorTextBox.Size = new System.Drawing.Size(136, 19);
            this.disabledReserveListBackColorTextBox.TabIndex = 60;
            // 
            // selectDisabledReserveListBackColorButton
            // 
            this.selectDisabledReserveListBackColorButton.Location = new System.Drawing.Point(290, 276);
            this.selectDisabledReserveListBackColorButton.Name = "selectDisabledReserveListBackColorButton";
            this.selectDisabledReserveListBackColorButton.Size = new System.Drawing.Size(75, 23);
            this.selectDisabledReserveListBackColorButton.TabIndex = 59;
            this.selectDisabledReserveListBackColorButton.Text = "選択";
            this.selectDisabledReserveListBackColorButton.UseVisualStyleBackColor = true;
            this.selectDisabledReserveListBackColorButton.Click += new System.EventHandler(this.selectDisabledReserveListBackColorButton_Click);
            // 
            // listHeaderForeColorLabel
            // 
            this.listHeaderForeColorLabel.AutoSize = true;
            this.listHeaderForeColorLabel.Location = new System.Drawing.Point(44, 319);
            this.listHeaderForeColorLabel.Name = "listHeaderForeColorLabel";
            this.listHeaderForeColorLabel.Size = new System.Drawing.Size(91, 12);
            this.listHeaderForeColorLabel.TabIndex = 61;
            this.listHeaderForeColorLabel.Text = "リストヘッダ文字色";
            // 
            // listHeaderForeColorTextBox
            // 
            this.listHeaderForeColorTextBox.Location = new System.Drawing.Point(148, 316);
            this.listHeaderForeColorTextBox.Name = "listHeaderForeColorTextBox";
            this.listHeaderForeColorTextBox.ReadOnly = true;
            this.listHeaderForeColorTextBox.Size = new System.Drawing.Size(136, 19);
            this.listHeaderForeColorTextBox.TabIndex = 60;
            // 
            // selectListHeaderForeColorButton
            // 
            this.selectListHeaderForeColorButton.Location = new System.Drawing.Point(290, 314);
            this.selectListHeaderForeColorButton.Name = "selectListHeaderForeColorButton";
            this.selectListHeaderForeColorButton.Size = new System.Drawing.Size(75, 23);
            this.selectListHeaderForeColorButton.TabIndex = 59;
            this.selectListHeaderForeColorButton.Text = "選択";
            this.selectListHeaderForeColorButton.UseVisualStyleBackColor = true;
            this.selectListHeaderForeColorButton.Click += new System.EventHandler(this.selectListHeaderForeColorButton_Click);
            // 
            // listHeaderBackColorLabel
            // 
            this.listHeaderBackColorLabel.AutoSize = true;
            this.listHeaderBackColorLabel.Location = new System.Drawing.Point(44, 357);
            this.listHeaderBackColorLabel.Name = "listHeaderBackColorLabel";
            this.listHeaderBackColorLabel.Size = new System.Drawing.Size(91, 12);
            this.listHeaderBackColorLabel.TabIndex = 61;
            this.listHeaderBackColorLabel.Text = "リストヘッダ背景色";
            // 
            // listHeaderBackColorTextBox
            // 
            this.listHeaderBackColorTextBox.Location = new System.Drawing.Point(148, 354);
            this.listHeaderBackColorTextBox.Name = "listHeaderBackColorTextBox";
            this.listHeaderBackColorTextBox.ReadOnly = true;
            this.listHeaderBackColorTextBox.Size = new System.Drawing.Size(136, 19);
            this.listHeaderBackColorTextBox.TabIndex = 60;
            // 
            // selectListHeaderBackColorButton
            // 
            this.selectListHeaderBackColorButton.Location = new System.Drawing.Point(290, 352);
            this.selectListHeaderBackColorButton.Name = "selectListHeaderBackColorButton";
            this.selectListHeaderBackColorButton.Size = new System.Drawing.Size(75, 23);
            this.selectListHeaderBackColorButton.TabIndex = 59;
            this.selectListHeaderBackColorButton.Text = "選択";
            this.selectListHeaderBackColorButton.UseVisualStyleBackColor = true;
            this.selectListHeaderBackColorButton.Click += new System.EventHandler(this.selectListHeaderBackColorButton_Click);
            // 
            // previewListView
            // 
            this.previewListView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.previewListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.previewListView.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(250)))), ((int)(((byte)(140)))));
            this.previewListView.FullRowSelect = true;
            this.previewListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.previewListView.HideSelection = false;
            this.previewListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5,
            listViewItem6});
            this.previewListView.Location = new System.Drawing.Point(423, 90);
            this.previewListView.MultiSelect = false;
            this.previewListView.Name = "previewListView";
            this.previewListView.ShowItemToolTips = true;
            this.previewListView.Size = new System.Drawing.Size(323, 158);
            this.previewListView.TabIndex = 52;
            this.previewListView.UseCompatibleStateImageBehavior = false;
            this.previewListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 319;
            // 
            // previewLabel
            // 
            this.previewLabel.AutoSize = true;
            this.previewLabel.Location = new System.Drawing.Point(407, 60);
            this.previewLabel.Name = "previewLabel";
            this.previewLabel.Size = new System.Drawing.Size(49, 12);
            this.previewLabel.TabIndex = 51;
            this.previewLabel.Text = "プレビュー";
            // 
            // foreColorLabel
            // 
            this.foreColorLabel.AutoSize = true;
            this.foreColorLabel.Location = new System.Drawing.Point(99, 91);
            this.foreColorLabel.Name = "foreColorLabel";
            this.foreColorLabel.Size = new System.Drawing.Size(41, 12);
            this.foreColorLabel.TabIndex = 50;
            this.foreColorLabel.Text = "文字色";
            // 
            // foreColorTextBox
            // 
            this.foreColorTextBox.Location = new System.Drawing.Point(148, 88);
            this.foreColorTextBox.Name = "foreColorTextBox";
            this.foreColorTextBox.ReadOnly = true;
            this.foreColorTextBox.Size = new System.Drawing.Size(136, 19);
            this.foreColorTextBox.TabIndex = 49;
            // 
            // selectForeColorButton
            // 
            this.selectForeColorButton.Location = new System.Drawing.Point(290, 86);
            this.selectForeColorButton.Name = "selectForeColorButton";
            this.selectForeColorButton.Size = new System.Drawing.Size(75, 23);
            this.selectForeColorButton.TabIndex = 48;
            this.selectForeColorButton.Text = "選択";
            this.selectForeColorButton.UseVisualStyleBackColor = true;
            this.selectForeColorButton.Click += new System.EventHandler(this.selectForeColorButton_Click);
            // 
            // listBackColorLabel
            // 
            this.listBackColorLabel.AutoSize = true;
            this.listBackColorLabel.Location = new System.Drawing.Point(75, 129);
            this.listBackColorLabel.Name = "listBackColorLabel";
            this.listBackColorLabel.Size = new System.Drawing.Size(65, 12);
            this.listBackColorLabel.TabIndex = 47;
            this.listBackColorLabel.Text = "リスト背景色";
            // 
            // listBackColorTextBox
            // 
            this.listBackColorTextBox.Location = new System.Drawing.Point(148, 126);
            this.listBackColorTextBox.Name = "listBackColorTextBox";
            this.listBackColorTextBox.ReadOnly = true;
            this.listBackColorTextBox.Size = new System.Drawing.Size(136, 19);
            this.listBackColorTextBox.TabIndex = 46;
            // 
            // selectListBackColorButton
            // 
            this.selectListBackColorButton.Location = new System.Drawing.Point(290, 124);
            this.selectListBackColorButton.Name = "selectListBackColorButton";
            this.selectListBackColorButton.Size = new System.Drawing.Size(75, 23);
            this.selectListBackColorButton.TabIndex = 45;
            this.selectListBackColorButton.Text = "選択";
            this.selectListBackColorButton.UseVisualStyleBackColor = true;
            this.selectListBackColorButton.Click += new System.EventHandler(this.selectListBackColorButton_Click);
            // 
            // previewFormPanel
            // 
            this.previewFormPanel.AutoSize = true;
            this.previewFormPanel.Location = new System.Drawing.Point(409, 77);
            this.previewFormPanel.Name = "previewFormPanel";
            this.previewFormPanel.Size = new System.Drawing.Size(337, 171);
            this.previewFormPanel.TabIndex = 44;
            // 
            // formBackColorLabel
            // 
            this.formBackColorLabel.AutoSize = true;
            this.formBackColorLabel.Location = new System.Drawing.Point(63, 53);
            this.formBackColorLabel.Name = "formBackColorLabel";
            this.formBackColorLabel.Size = new System.Drawing.Size(77, 12);
            this.formBackColorLabel.TabIndex = 43;
            this.formBackColorLabel.Text = "フォーム背景色";
            // 
            // formBackColorTextBox
            // 
            this.formBackColorTextBox.Location = new System.Drawing.Point(148, 50);
            this.formBackColorTextBox.Name = "formBackColorTextBox";
            this.formBackColorTextBox.ReadOnly = true;
            this.formBackColorTextBox.Size = new System.Drawing.Size(136, 19);
            this.formBackColorTextBox.TabIndex = 42;
            // 
            // selectFormBackColorButton
            // 
            this.selectFormBackColorButton.Location = new System.Drawing.Point(290, 48);
            this.selectFormBackColorButton.Name = "selectFormBackColorButton";
            this.selectFormBackColorButton.Size = new System.Drawing.Size(75, 23);
            this.selectFormBackColorButton.TabIndex = 41;
            this.selectFormBackColorButton.Text = "選択";
            this.selectFormBackColorButton.UseVisualStyleBackColor = true;
            this.selectFormBackColorButton.Click += new System.EventHandler(this.selectFormBackColorButton_Click);
            // 
            // fontLabel
            // 
            this.fontLabel.AutoSize = true;
            this.fontLabel.Location = new System.Drawing.Point(102, 15);
            this.fontLabel.Name = "fontLabel";
            this.fontLabel.Size = new System.Drawing.Size(38, 12);
            this.fontLabel.TabIndex = 40;
            this.fontLabel.Text = "フォント";
            // 
            // fontTextBox
            // 
            this.fontTextBox.Location = new System.Drawing.Point(148, 12);
            this.fontTextBox.Name = "fontTextBox";
            this.fontTextBox.ReadOnly = true;
            this.fontTextBox.Size = new System.Drawing.Size(302, 19);
            this.fontTextBox.TabIndex = 39;
            // 
            // selectFontButton
            // 
            this.selectFontButton.Location = new System.Drawing.Point(456, 10);
            this.selectFontButton.Name = "selectFontButton";
            this.selectFontButton.Size = new System.Drawing.Size(75, 23);
            this.selectFontButton.TabIndex = 38;
            this.selectFontButton.Text = "選択";
            this.selectFontButton.UseVisualStyleBackColor = true;
            this.selectFontButton.Click += new System.EventHandler(this.selectFontButton_Click);
            // 
            // contextMenuFontColorTabPage
            // 
            this.contextMenuFontColorTabPage.Controls.Add(this.ngReserveMenuBackColorLabel);
            this.contextMenuFontColorTabPage.Controls.Add(this.ngReserveMenuBackColorTextBox);
            this.contextMenuFontColorTabPage.Controls.Add(this.selectNgReserveMenuBackColorButton);
            this.contextMenuFontColorTabPage.Controls.Add(this.partialReserveMenuBackColorLabel);
            this.contextMenuFontColorTabPage.Controls.Add(this.partialReserveMenuBackColorTextBox);
            this.contextMenuFontColorTabPage.Controls.Add(this.selectPartialReserveMenuBackColorButton);
            this.contextMenuFontColorTabPage.Controls.Add(this.okReserveMenuBackColorLabel);
            this.contextMenuFontColorTabPage.Controls.Add(this.okReserveMenuBackColorTextBox);
            this.contextMenuFontColorTabPage.Controls.Add(this.selectOkReserveMenuBackColorButton);
            this.contextMenuFontColorTabPage.Controls.Add(this.disabledReserveMenuBackColorLabel);
            this.contextMenuFontColorTabPage.Controls.Add(this.disabledReserveMenuBackColorTextBox);
            this.contextMenuFontColorTabPage.Controls.Add(this.selectDisabledReserveMenuBackColorButton);
            this.contextMenuFontColorTabPage.Controls.Add(this.previewMenuListView);
            this.contextMenuFontColorTabPage.Controls.Add(this.label5);
            this.contextMenuFontColorTabPage.Controls.Add(this.menuBackColorLabel);
            this.contextMenuFontColorTabPage.Controls.Add(this.menuBackColorTextBox);
            this.contextMenuFontColorTabPage.Controls.Add(this.selectMenuBackColorButton);
            this.contextMenuFontColorTabPage.Controls.Add(this.menuFontLabel);
            this.contextMenuFontColorTabPage.Controls.Add(this.menuFontTextBox);
            this.contextMenuFontColorTabPage.Controls.Add(this.selectMenuFontButton);
            this.contextMenuFontColorTabPage.Location = new System.Drawing.Point(4, 22);
            this.contextMenuFontColorTabPage.Name = "contextMenuFontColorTabPage";
            this.contextMenuFontColorTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.contextMenuFontColorTabPage.Size = new System.Drawing.Size(791, 384);
            this.contextMenuFontColorTabPage.TabIndex = 7;
            this.contextMenuFontColorTabPage.Text = "フォント・色(右クリックメニュー)";
            this.contextMenuFontColorTabPage.UseVisualStyleBackColor = true;
            // 
            // ngReserveMenuBackColorLabel
            // 
            this.ngReserveMenuBackColorLabel.AutoSize = true;
            this.ngReserveMenuBackColorLabel.Location = new System.Drawing.Point(7, 167);
            this.ngReserveMenuBackColorLabel.Name = "ngReserveMenuBackColorLabel";
            this.ngReserveMenuBackColorLabel.Size = new System.Drawing.Size(132, 12);
            this.ngReserveMenuBackColorLabel.TabIndex = 85;
            this.ngReserveMenuBackColorLabel.Text = "メニュー背景色(予約不可)";
            // 
            // ngReserveMenuBackColorTextBox
            // 
            this.ngReserveMenuBackColorTextBox.Location = new System.Drawing.Point(148, 164);
            this.ngReserveMenuBackColorTextBox.Name = "ngReserveMenuBackColorTextBox";
            this.ngReserveMenuBackColorTextBox.ReadOnly = true;
            this.ngReserveMenuBackColorTextBox.Size = new System.Drawing.Size(136, 19);
            this.ngReserveMenuBackColorTextBox.TabIndex = 84;
            // 
            // selectNgReserveMenuBackColorButton
            // 
            this.selectNgReserveMenuBackColorButton.Location = new System.Drawing.Point(290, 162);
            this.selectNgReserveMenuBackColorButton.Name = "selectNgReserveMenuBackColorButton";
            this.selectNgReserveMenuBackColorButton.Size = new System.Drawing.Size(75, 23);
            this.selectNgReserveMenuBackColorButton.TabIndex = 83;
            this.selectNgReserveMenuBackColorButton.Text = "選択";
            this.selectNgReserveMenuBackColorButton.UseVisualStyleBackColor = true;
            this.selectNgReserveMenuBackColorButton.Click += new System.EventHandler(this.selectNgReserveMenuBackColorButton_Click);
            // 
            // partialReserveMenuBackColorLabel
            // 
            this.partialReserveMenuBackColorLabel.AutoSize = true;
            this.partialReserveMenuBackColorLabel.Location = new System.Drawing.Point(7, 129);
            this.partialReserveMenuBackColorLabel.Name = "partialReserveMenuBackColorLabel";
            this.partialReserveMenuBackColorLabel.Size = new System.Drawing.Size(132, 12);
            this.partialReserveMenuBackColorLabel.TabIndex = 82;
            this.partialReserveMenuBackColorLabel.Text = "メニュー背景色(部分予約)";
            // 
            // partialReserveMenuBackColorTextBox
            // 
            this.partialReserveMenuBackColorTextBox.Location = new System.Drawing.Point(148, 126);
            this.partialReserveMenuBackColorTextBox.Name = "partialReserveMenuBackColorTextBox";
            this.partialReserveMenuBackColorTextBox.ReadOnly = true;
            this.partialReserveMenuBackColorTextBox.Size = new System.Drawing.Size(136, 19);
            this.partialReserveMenuBackColorTextBox.TabIndex = 81;
            // 
            // selectPartialReserveMenuBackColorButton
            // 
            this.selectPartialReserveMenuBackColorButton.Location = new System.Drawing.Point(290, 124);
            this.selectPartialReserveMenuBackColorButton.Name = "selectPartialReserveMenuBackColorButton";
            this.selectPartialReserveMenuBackColorButton.Size = new System.Drawing.Size(75, 23);
            this.selectPartialReserveMenuBackColorButton.TabIndex = 80;
            this.selectPartialReserveMenuBackColorButton.Text = "選択";
            this.selectPartialReserveMenuBackColorButton.UseVisualStyleBackColor = true;
            this.selectPartialReserveMenuBackColorButton.Click += new System.EventHandler(this.selectPartialReserveMenuBackColorButton_Click);
            // 
            // okReserveMenuBackColorLabel
            // 
            this.okReserveMenuBackColorLabel.AutoSize = true;
            this.okReserveMenuBackColorLabel.Location = new System.Drawing.Point(7, 91);
            this.okReserveMenuBackColorLabel.Name = "okReserveMenuBackColorLabel";
            this.okReserveMenuBackColorLabel.Size = new System.Drawing.Size(132, 12);
            this.okReserveMenuBackColorLabel.TabIndex = 79;
            this.okReserveMenuBackColorLabel.Text = "メニュー背景色(正常予約)";
            // 
            // okReserveMenuBackColorTextBox
            // 
            this.okReserveMenuBackColorTextBox.Location = new System.Drawing.Point(148, 88);
            this.okReserveMenuBackColorTextBox.Name = "okReserveMenuBackColorTextBox";
            this.okReserveMenuBackColorTextBox.ReadOnly = true;
            this.okReserveMenuBackColorTextBox.Size = new System.Drawing.Size(136, 19);
            this.okReserveMenuBackColorTextBox.TabIndex = 78;
            // 
            // selectOkReserveMenuBackColorButton
            // 
            this.selectOkReserveMenuBackColorButton.Location = new System.Drawing.Point(290, 86);
            this.selectOkReserveMenuBackColorButton.Name = "selectOkReserveMenuBackColorButton";
            this.selectOkReserveMenuBackColorButton.Size = new System.Drawing.Size(75, 23);
            this.selectOkReserveMenuBackColorButton.TabIndex = 77;
            this.selectOkReserveMenuBackColorButton.Text = "選択";
            this.selectOkReserveMenuBackColorButton.UseVisualStyleBackColor = true;
            this.selectOkReserveMenuBackColorButton.Click += new System.EventHandler(this.selectOkReserveMenuBackColorButton_Click);
            // 
            // disabledReserveMenuBackColorLabel
            // 
            this.disabledReserveMenuBackColorLabel.AutoSize = true;
            this.disabledReserveMenuBackColorLabel.Location = new System.Drawing.Point(7, 205);
            this.disabledReserveMenuBackColorLabel.Name = "disabledReserveMenuBackColorLabel";
            this.disabledReserveMenuBackColorLabel.Size = new System.Drawing.Size(132, 12);
            this.disabledReserveMenuBackColorLabel.TabIndex = 79;
            this.disabledReserveMenuBackColorLabel.Text = "メニュー背景色(無効予約)";
            // 
            // disabledReserveMenuBackColorTextBox
            // 
            this.disabledReserveMenuBackColorTextBox.Location = new System.Drawing.Point(148, 202);
            this.disabledReserveMenuBackColorTextBox.Name = "disabledReserveMenuBackColorTextBox";
            this.disabledReserveMenuBackColorTextBox.ReadOnly = true;
            this.disabledReserveMenuBackColorTextBox.Size = new System.Drawing.Size(136, 19);
            this.disabledReserveMenuBackColorTextBox.TabIndex = 78;
            // 
            // selectDisabledReserveMenuBackColorButton
            // 
            this.selectDisabledReserveMenuBackColorButton.Location = new System.Drawing.Point(290, 200);
            this.selectDisabledReserveMenuBackColorButton.Name = "selectDisabledReserveMenuBackColorButton";
            this.selectDisabledReserveMenuBackColorButton.Size = new System.Drawing.Size(75, 23);
            this.selectDisabledReserveMenuBackColorButton.TabIndex = 77;
            this.selectDisabledReserveMenuBackColorButton.Text = "選択";
            this.selectDisabledReserveMenuBackColorButton.UseVisualStyleBackColor = true;
            this.selectDisabledReserveMenuBackColorButton.Click += new System.EventHandler(this.selectDisabledReserveMenuBackColorButton_Click);
            // 
            // previewMenuListView
            // 
            this.previewMenuListView.BackColor = System.Drawing.SystemColors.ControlLight;
            this.previewMenuListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.previewMenuListView.ForeColor = System.Drawing.Color.Black;
            this.previewMenuListView.FullRowSelect = true;
            this.previewMenuListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.previewMenuListView.HideSelection = false;
            this.previewMenuListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem7,
            listViewItem8,
            listViewItem9,
            listViewItem10,
            listViewItem11});
            this.previewMenuListView.Location = new System.Drawing.Point(409, 75);
            this.previewMenuListView.MultiSelect = false;
            this.previewMenuListView.Name = "previewMenuListView";
            this.previewMenuListView.ShowItemToolTips = true;
            this.previewMenuListView.Size = new System.Drawing.Size(323, 158);
            this.previewMenuListView.TabIndex = 76;
            this.previewMenuListView.UseCompatibleStateImageBehavior = false;
            this.previewMenuListView.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Width = 319;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(407, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(49, 12);
            this.label5.TabIndex = 75;
            this.label5.Text = "プレビュー";
            // 
            // menuBackColorLabel
            // 
            this.menuBackColorLabel.AutoSize = true;
            this.menuBackColorLabel.Location = new System.Drawing.Point(63, 53);
            this.menuBackColorLabel.Name = "menuBackColorLabel";
            this.menuBackColorLabel.Size = new System.Drawing.Size(76, 12);
            this.menuBackColorLabel.TabIndex = 74;
            this.menuBackColorLabel.Text = "メニュー背景色";
            // 
            // menuBackColorTextBox
            // 
            this.menuBackColorTextBox.Location = new System.Drawing.Point(148, 50);
            this.menuBackColorTextBox.Name = "menuBackColorTextBox";
            this.menuBackColorTextBox.ReadOnly = true;
            this.menuBackColorTextBox.Size = new System.Drawing.Size(136, 19);
            this.menuBackColorTextBox.TabIndex = 73;
            // 
            // selectMenuBackColorButton
            // 
            this.selectMenuBackColorButton.Location = new System.Drawing.Point(290, 48);
            this.selectMenuBackColorButton.Name = "selectMenuBackColorButton";
            this.selectMenuBackColorButton.Size = new System.Drawing.Size(75, 23);
            this.selectMenuBackColorButton.TabIndex = 72;
            this.selectMenuBackColorButton.Text = "選択";
            this.selectMenuBackColorButton.UseVisualStyleBackColor = true;
            this.selectMenuBackColorButton.Click += new System.EventHandler(this.selectMenuBackColorButton_Click);
            // 
            // menuFontLabel
            // 
            this.menuFontLabel.AutoSize = true;
            this.menuFontLabel.Location = new System.Drawing.Point(102, 15);
            this.menuFontLabel.Name = "menuFontLabel";
            this.menuFontLabel.Size = new System.Drawing.Size(38, 12);
            this.menuFontLabel.TabIndex = 64;
            this.menuFontLabel.Text = "フォント";
            // 
            // menuFontTextBox
            // 
            this.menuFontTextBox.Location = new System.Drawing.Point(148, 12);
            this.menuFontTextBox.Name = "menuFontTextBox";
            this.menuFontTextBox.ReadOnly = true;
            this.menuFontTextBox.Size = new System.Drawing.Size(302, 19);
            this.menuFontTextBox.TabIndex = 63;
            // 
            // selectMenuFontButton
            // 
            this.selectMenuFontButton.Location = new System.Drawing.Point(456, 10);
            this.selectMenuFontButton.Name = "selectMenuFontButton";
            this.selectMenuFontButton.Size = new System.Drawing.Size(75, 23);
            this.selectMenuFontButton.TabIndex = 62;
            this.selectMenuFontButton.Text = "選択";
            this.selectMenuFontButton.UseVisualStyleBackColor = true;
            this.selectMenuFontButton.Click += new System.EventHandler(this.selectMenuFontButton_Click);
            // 
            // controlUiFontColorTabPage
            // 
            this.controlUiFontColorTabPage.Controls.Add(this.tabFontLabel);
            this.controlUiFontColorTabPage.Controls.Add(this.tabFontTextBox);
            this.controlUiFontColorTabPage.Controls.Add(this.selectTabFontButton);
            this.controlUiFontColorTabPage.Controls.Add(this.buttonFontLabel);
            this.controlUiFontColorTabPage.Controls.Add(this.buttonFontTextBox);
            this.controlUiFontColorTabPage.Controls.Add(this.selectButtonFontButton);
            this.controlUiFontColorTabPage.Controls.Add(this.labelFontLabel);
            this.controlUiFontColorTabPage.Controls.Add(this.labelFontTextBox);
            this.controlUiFontColorTabPage.Controls.Add(this.selectLabelFontButton);
            this.controlUiFontColorTabPage.Controls.Add(this.textBoxFontLabel);
            this.controlUiFontColorTabPage.Controls.Add(this.textBoxFontTextBox);
            this.controlUiFontColorTabPage.Controls.Add(this.selectTextBoxFontButton);
            this.controlUiFontColorTabPage.Location = new System.Drawing.Point(4, 22);
            this.controlUiFontColorTabPage.Name = "controlUiFontColorTabPage";
            this.controlUiFontColorTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.controlUiFontColorTabPage.Size = new System.Drawing.Size(791, 384);
            this.controlUiFontColorTabPage.TabIndex = 8;
            this.controlUiFontColorTabPage.Text = "フォント(コントロールUI)";
            this.controlUiFontColorTabPage.UseVisualStyleBackColor = true;
            // 
            // tabFontLabel
            // 
            this.tabFontLabel.AutoSize = true;
            this.tabFontLabel.Location = new System.Drawing.Point(78, 15);
            this.tabFontLabel.Name = "tabFontLabel";
            this.tabFontLabel.Size = new System.Drawing.Size(65, 12);
            this.tabFontLabel.TabIndex = 64;
            this.tabFontLabel.Text = "タブのフォント";
            // 
            // tabFontTextBox
            // 
            this.tabFontTextBox.Location = new System.Drawing.Point(148, 12);
            this.tabFontTextBox.Name = "tabFontTextBox";
            this.tabFontTextBox.ReadOnly = true;
            this.tabFontTextBox.Size = new System.Drawing.Size(302, 19);
            this.tabFontTextBox.TabIndex = 63;
            // 
            // selectTabFontButton
            // 
            this.selectTabFontButton.Location = new System.Drawing.Point(456, 10);
            this.selectTabFontButton.Name = "selectTabFontButton";
            this.selectTabFontButton.Size = new System.Drawing.Size(75, 23);
            this.selectTabFontButton.TabIndex = 62;
            this.selectTabFontButton.Text = "選択";
            this.selectTabFontButton.UseVisualStyleBackColor = true;
            this.selectTabFontButton.Click += new System.EventHandler(this.selectTabFontButton_Click);
            // 
            // buttonFontLabel
            // 
            this.buttonFontLabel.AutoSize = true;
            this.buttonFontLabel.Location = new System.Drawing.Point(69, 53);
            this.buttonFontLabel.Name = "buttonFontLabel";
            this.buttonFontLabel.Size = new System.Drawing.Size(75, 12);
            this.buttonFontLabel.TabIndex = 64;
            this.buttonFontLabel.Text = "ボタンのフォント";
            // 
            // buttonFontTextBox
            // 
            this.buttonFontTextBox.Location = new System.Drawing.Point(148, 50);
            this.buttonFontTextBox.Name = "buttonFontTextBox";
            this.buttonFontTextBox.ReadOnly = true;
            this.buttonFontTextBox.Size = new System.Drawing.Size(302, 19);
            this.buttonFontTextBox.TabIndex = 63;
            // 
            // selectButtonFontButton
            // 
            this.selectButtonFontButton.Location = new System.Drawing.Point(456, 48);
            this.selectButtonFontButton.Name = "selectButtonFontButton";
            this.selectButtonFontButton.Size = new System.Drawing.Size(75, 23);
            this.selectButtonFontButton.TabIndex = 62;
            this.selectButtonFontButton.Text = "選択";
            this.selectButtonFontButton.UseVisualStyleBackColor = true;
            this.selectButtonFontButton.Click += new System.EventHandler(this.selectButtonFontButton_Click);
            // 
            // labelFontLabel
            // 
            this.labelFontLabel.AutoSize = true;
            this.labelFontLabel.Location = new System.Drawing.Point(68, 91);
            this.labelFontLabel.Name = "labelFontLabel";
            this.labelFontLabel.Size = new System.Drawing.Size(76, 12);
            this.labelFontLabel.TabIndex = 64;
            this.labelFontLabel.Text = "ラベルのフォント";
            // 
            // labelFontTextBox
            // 
            this.labelFontTextBox.Location = new System.Drawing.Point(148, 88);
            this.labelFontTextBox.Name = "labelFontTextBox";
            this.labelFontTextBox.ReadOnly = true;
            this.labelFontTextBox.Size = new System.Drawing.Size(302, 19);
            this.labelFontTextBox.TabIndex = 63;
            // 
            // selectLabelFontButton
            // 
            this.selectLabelFontButton.Location = new System.Drawing.Point(456, 86);
            this.selectLabelFontButton.Name = "selectLabelFontButton";
            this.selectLabelFontButton.Size = new System.Drawing.Size(75, 23);
            this.selectLabelFontButton.TabIndex = 62;
            this.selectLabelFontButton.Text = "選択";
            this.selectLabelFontButton.UseVisualStyleBackColor = true;
            this.selectLabelFontButton.Click += new System.EventHandler(this.selectLabelFontButton_Click);
            // 
            // textBoxFontLabel
            // 
            this.textBoxFontLabel.AutoSize = true;
            this.textBoxFontLabel.Location = new System.Drawing.Point(29, 129);
            this.textBoxFontLabel.Name = "textBoxFontLabel";
            this.textBoxFontLabel.Size = new System.Drawing.Size(118, 12);
            this.textBoxFontLabel.TabIndex = 64;
            this.textBoxFontLabel.Text = "テキストボックスのフォント";
            // 
            // textBoxFontTextBox
            // 
            this.textBoxFontTextBox.Location = new System.Drawing.Point(148, 126);
            this.textBoxFontTextBox.Name = "textBoxFontTextBox";
            this.textBoxFontTextBox.ReadOnly = true;
            this.textBoxFontTextBox.Size = new System.Drawing.Size(302, 19);
            this.textBoxFontTextBox.TabIndex = 63;
            // 
            // selectTextBoxFontButton
            // 
            this.selectTextBoxFontButton.Location = new System.Drawing.Point(456, 124);
            this.selectTextBoxFontButton.Name = "selectTextBoxFontButton";
            this.selectTextBoxFontButton.Size = new System.Drawing.Size(75, 23);
            this.selectTextBoxFontButton.TabIndex = 62;
            this.selectTextBoxFontButton.Text = "選択";
            this.selectTextBoxFontButton.UseVisualStyleBackColor = true;
            this.selectTextBoxFontButton.Click += new System.EventHandler(this.selectTextBoxFontButton_Click);
            // 
            // otherTabPage
            // 
            this.otherTabPage.Controls.Add(this.isHorizontalSplitCheckBox);
            this.otherTabPage.Controls.Add(this.toggleVisibleTaskTrayIconClickCheckBox);
            this.otherTabPage.Controls.Add(this.storeTaskTrayByClosingCheckBox);
            this.otherTabPage.Controls.Add(this.showTaskTrayIconCheckBox);
            this.otherTabPage.Controls.Add(this.fixNoRecToServiceOnlyCheckBox);
            this.otherTabPage.Controls.Add(this.recListMaxCountLabel);
            this.otherTabPage.Controls.Add(this.recListMaxCountNumericUpDown);
            this.otherTabPage.Controls.Add(this.taskTrayIconClickGroupBox);
            this.otherTabPage.Location = new System.Drawing.Point(4, 22);
            this.otherTabPage.Name = "otherTabPage";
            this.otherTabPage.Size = new System.Drawing.Size(791, 384);
            this.otherTabPage.TabIndex = 4;
            this.otherTabPage.Text = "その他";
            this.otherTabPage.UseVisualStyleBackColor = true;
            // 
            // isHorizontalSplitCheckBox
            // 
            this.isHorizontalSplitCheckBox.AutoSize = true;
            this.isHorizontalSplitCheckBox.Location = new System.Drawing.Point(17, 88);
            this.isHorizontalSplitCheckBox.Name = "isHorizontalSplitCheckBox";
            this.isHorizontalSplitCheckBox.Size = new System.Drawing.Size(133, 16);
            this.isHorizontalSplitCheckBox.TabIndex = 15;
            this.isHorizontalSplitCheckBox.Text = "一覧を縦に並べて表示";
            this.isHorizontalSplitCheckBox.UseVisualStyleBackColor = true;
            // 
            // toggleVisibleTaskTrayIconClickCheckBox
            // 
            this.toggleVisibleTaskTrayIconClickCheckBox.AutoSize = true;
            this.toggleVisibleTaskTrayIconClickCheckBox.Location = new System.Drawing.Point(17, 66);
            this.toggleVisibleTaskTrayIconClickCheckBox.Name = "toggleVisibleTaskTrayIconClickCheckBox";
            this.toggleVisibleTaskTrayIconClickCheckBox.Size = new System.Drawing.Size(275, 16);
            this.toggleVisibleTaskTrayIconClickCheckBox.TabIndex = 12;
            this.toggleVisibleTaskTrayIconClickCheckBox.Text = "タスクトレイアイコンクリックで表示・非表示を切り替える";
            this.toggleVisibleTaskTrayIconClickCheckBox.UseVisualStyleBackColor = true;
            // 
            // storeTaskTrayByClosingCheckBox
            // 
            this.storeTaskTrayByClosingCheckBox.AutoSize = true;
            this.storeTaskTrayByClosingCheckBox.Location = new System.Drawing.Point(17, 44);
            this.storeTaskTrayByClosingCheckBox.Name = "storeTaskTrayByClosingCheckBox";
            this.storeTaskTrayByClosingCheckBox.Size = new System.Drawing.Size(176, 16);
            this.storeTaskTrayByClosingCheckBox.TabIndex = 11;
            this.storeTaskTrayByClosingCheckBox.Text = "×ボタンでタスクトレイに格納する";
            this.storeTaskTrayByClosingCheckBox.UseVisualStyleBackColor = true;
            // 
            // showTaskTrayIconCheckBox
            // 
            this.showTaskTrayIconCheckBox.AutoSize = true;
            this.showTaskTrayIconCheckBox.Location = new System.Drawing.Point(17, 22);
            this.showTaskTrayIconCheckBox.Name = "showTaskTrayIconCheckBox";
            this.showTaskTrayIconCheckBox.Size = new System.Drawing.Size(195, 16);
            this.showTaskTrayIconCheckBox.TabIndex = 10;
            this.showTaskTrayIconCheckBox.Text = "タスクトレイに常時アイコンを表示する";
            this.showTaskTrayIconCheckBox.UseVisualStyleBackColor = true;
            // 
            // fixNoRecToServiceOnlyCheckBox
            // 
            this.fixNoRecToServiceOnlyCheckBox.AutoSize = true;
            this.fixNoRecToServiceOnlyCheckBox.Location = new System.Drawing.Point(17, 110);
            this.fixNoRecToServiceOnlyCheckBox.Name = "fixNoRecToServiceOnlyCheckBox";
            this.fixNoRecToServiceOnlyCheckBox.Size = new System.Drawing.Size(296, 16);
            this.fixNoRecToServiceOnlyCheckBox.TabIndex = 16;
            this.fixNoRecToServiceOnlyCheckBox.Text = "予約を無効にするとき、録画モードを「指定サービス」にする";
            this.fixNoRecToServiceOnlyCheckBox.UseVisualStyleBackColor = true;
            // 
            // recListMaxCountLabel
            // 
            this.recListMaxCountLabel.AutoSize = true;
            this.recListMaxCountLabel.Location = new System.Drawing.Point(14, 144);
            this.recListMaxCountLabel.Name = "recListMaxCountLabel";
            this.recListMaxCountLabel.Size = new System.Drawing.Size(256, 12);
            this.recListMaxCountLabel.TabIndex = 18;
            this.recListMaxCountLabel.Text = "録画済み一覧の最大表示数(0～99999, 0=無制限)";
            // 
            // recListMaxCountNumericUpDown
            // 
            this.recListMaxCountNumericUpDown.Location = new System.Drawing.Point(270, 141);
            this.recListMaxCountNumericUpDown.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.recListMaxCountNumericUpDown.Name = "recListMaxCountNumericUpDown";
            this.recListMaxCountNumericUpDown.Size = new System.Drawing.Size(70, 19);
            this.recListMaxCountNumericUpDown.TabIndex = 17;
            this.recListMaxCountNumericUpDown.Value = new decimal(new int[] {
            1500,
            0,
            0,
            0});
            // 
            // taskTrayIconClickGroupBox
            // 
            this.taskTrayIconClickGroupBox.Controls.Add(this.taskTrayIconRightDoubleClickLabel);
            this.taskTrayIconClickGroupBox.Controls.Add(this.taskTrayIconRightClickLabel);
            this.taskTrayIconClickGroupBox.Controls.Add(this.taskTrayIconLeftDoubleClickLabel);
            this.taskTrayIconClickGroupBox.Controls.Add(this.taskTrayIconLeftClickLabel);
            this.taskTrayIconClickGroupBox.Controls.Add(this.taskTrayIconRightDoubleClickComboBox);
            this.taskTrayIconClickGroupBox.Controls.Add(this.taskTrayIconRightClickComboBox);
            this.taskTrayIconClickGroupBox.Controls.Add(this.taskTrayIconLeftDoubleClickComboBox);
            this.taskTrayIconClickGroupBox.Controls.Add(this.taskTrayIconLeftClickComboBox);
            this.taskTrayIconClickGroupBox.Location = new System.Drawing.Point(419, 30);
            this.taskTrayIconClickGroupBox.Name = "taskTrayIconClickGroupBox";
            this.taskTrayIconClickGroupBox.Size = new System.Drawing.Size(244, 128);
            this.taskTrayIconClickGroupBox.TabIndex = 27;
            this.taskTrayIconClickGroupBox.TabStop = false;
            this.taskTrayIconClickGroupBox.Text = "タスクトレイアイコン・マウス定義";
            // 
            // taskTrayIconRightDoubleClickLabel
            // 
            this.taskTrayIconRightDoubleClickLabel.AutoSize = true;
            this.taskTrayIconRightDoubleClickLabel.Location = new System.Drawing.Point(19, 103);
            this.taskTrayIconRightDoubleClickLabel.Name = "taskTrayIconRightDoubleClickLabel";
            this.taskTrayIconRightDoubleClickLabel.Size = new System.Drawing.Size(75, 12);
            this.taskTrayIconRightDoubleClickLabel.TabIndex = 26;
            this.taskTrayIconRightDoubleClickLabel.Text = "右ダブルクリック";
            // 
            // taskTrayIconRightClickLabel
            // 
            this.taskTrayIconRightClickLabel.AutoSize = true;
            this.taskTrayIconRightClickLabel.Location = new System.Drawing.Point(19, 75);
            this.taskTrayIconRightClickLabel.Name = "taskTrayIconRightClickLabel";
            this.taskTrayIconRightClickLabel.Size = new System.Drawing.Size(47, 12);
            this.taskTrayIconRightClickLabel.TabIndex = 25;
            this.taskTrayIconRightClickLabel.Text = "右クリック";
            // 
            // taskTrayIconLeftDoubleClickLabel
            // 
            this.taskTrayIconLeftDoubleClickLabel.AutoSize = true;
            this.taskTrayIconLeftDoubleClickLabel.Location = new System.Drawing.Point(19, 48);
            this.taskTrayIconLeftDoubleClickLabel.Name = "taskTrayIconLeftDoubleClickLabel";
            this.taskTrayIconLeftDoubleClickLabel.Size = new System.Drawing.Size(75, 12);
            this.taskTrayIconLeftDoubleClickLabel.TabIndex = 24;
            this.taskTrayIconLeftDoubleClickLabel.Text = "左ダブルクリック";
            // 
            // taskTrayIconLeftClickLabel
            // 
            this.taskTrayIconLeftClickLabel.AutoSize = true;
            this.taskTrayIconLeftClickLabel.Location = new System.Drawing.Point(19, 21);
            this.taskTrayIconLeftClickLabel.Name = "taskTrayIconLeftClickLabel";
            this.taskTrayIconLeftClickLabel.Size = new System.Drawing.Size(47, 12);
            this.taskTrayIconLeftClickLabel.TabIndex = 23;
            this.taskTrayIconLeftClickLabel.Text = "左クリック";
            // 
            // taskTrayIconRightDoubleClickComboBox
            // 
            this.taskTrayIconRightDoubleClickComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.taskTrayIconRightDoubleClickComboBox.FormattingEnabled = true;
            this.taskTrayIconRightDoubleClickComboBox.Items.AddRange(new object[] {
            "",
            "メニュー",
            "テレビ番組表",
            "Rockバー表示"});
            this.taskTrayIconRightDoubleClickComboBox.Location = new System.Drawing.Point(102, 99);
            this.taskTrayIconRightDoubleClickComboBox.Name = "taskTrayIconRightDoubleClickComboBox";
            this.taskTrayIconRightDoubleClickComboBox.Size = new System.Drawing.Size(121, 20);
            this.taskTrayIconRightDoubleClickComboBox.TabIndex = 22;
            // 
            // taskTrayIconRightClickComboBox
            // 
            this.taskTrayIconRightClickComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.taskTrayIconRightClickComboBox.FormattingEnabled = true;
            this.taskTrayIconRightClickComboBox.Items.AddRange(new object[] {
            "",
            "メニュー",
            "テレビ番組表",
            "Rockバー表示"});
            this.taskTrayIconRightClickComboBox.Location = new System.Drawing.Point(102, 72);
            this.taskTrayIconRightClickComboBox.Name = "taskTrayIconRightClickComboBox";
            this.taskTrayIconRightClickComboBox.Size = new System.Drawing.Size(121, 20);
            this.taskTrayIconRightClickComboBox.TabIndex = 21;
            // 
            // taskTrayIconLeftDoubleClickComboBox
            // 
            this.taskTrayIconLeftDoubleClickComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.taskTrayIconLeftDoubleClickComboBox.FormattingEnabled = true;
            this.taskTrayIconLeftDoubleClickComboBox.Items.AddRange(new object[] {
            "",
            "メニュー",
            "テレビ番組表",
            "Rockバー表示"});
            this.taskTrayIconLeftDoubleClickComboBox.Location = new System.Drawing.Point(102, 45);
            this.taskTrayIconLeftDoubleClickComboBox.Name = "taskTrayIconLeftDoubleClickComboBox";
            this.taskTrayIconLeftDoubleClickComboBox.Size = new System.Drawing.Size(121, 20);
            this.taskTrayIconLeftDoubleClickComboBox.TabIndex = 20;
            // 
            // taskTrayIconLeftClickComboBox
            // 
            this.taskTrayIconLeftClickComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.taskTrayIconLeftClickComboBox.FormattingEnabled = true;
            this.taskTrayIconLeftClickComboBox.Items.AddRange(new object[] {
            "",
            "メニュー",
            "テレビ番組表",
            "Rockバー表示"});
            this.taskTrayIconLeftClickComboBox.Location = new System.Drawing.Point(102, 18);
            this.taskTrayIconLeftClickComboBox.Name = "taskTrayIconLeftClickComboBox";
            this.taskTrayIconLeftClickComboBox.Size = new System.Drawing.Size(121, 20);
            this.taskTrayIconLeftClickComboBox.TabIndex = 19;
            // 
            // tvtestOpenFileDialog
            // 
            this.tvtestOpenFileDialog.FileName = "openFileDialog1";
            this.tvtestOpenFileDialog.Filter = "exe Files (*.exe)|*.exe";
            this.tvtestOpenFileDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.tvtestOpenFileDialog_FileOk);
            // 
            // fontDialog
            // 
            this.fontDialog.AllowVerticalFonts = false;
            this.fontDialog.MaxSize = 28;
            this.fontDialog.MinSize = 6;
            this.fontDialog.ShowEffects = false;
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.settingTabControl);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.applyButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingForm";
            this.Text = "設定";
            this.settingTabControl.ResumeLayout(false);
            this.edcbLinkageTabPage.ResumeLayout(false);
            this.edcbLinkageTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.portNumberNumericUpDown)).EndInit();
            this.reserveTabPage.ResumeLayout(false);
            this.reserveTabPage.PerformLayout();
            this.edcbReserveSettingGroupBox.ResumeLayout(false);
            this.edcbReserveSettingGroupBox.PerformLayout();
            this.prioritizeViewPanel.ResumeLayout(false);
            this.prioritizeViewPanel.PerformLayout();
            this.suspendModeAfterRecPanel.ResumeLayout(false);
            this.suspendModeAfterRecPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.endRecMarginNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.startRecMarginNumericUpDown)).EndInit();
            this.tunerTabPage.ResumeLayout(false);
            this.tunerTabPage.PerformLayout();
            this.allServiceTabPage.ResumeLayout(false);
            this.allServiceTabPage.PerformLayout();
            this.favoriteServiceTabPage.ResumeLayout(false);
            this.favoriteServiceTabPage.PerformLayout();
            this.tvtestLinkageTabPage.ResumeLayout(false);
            this.tvtestLinkageTabPage.PerformLayout();
            this.autoStartTargetGroupBox.ResumeLayout(false);
            this.autoStartTargetGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.autoCloseMarginNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.autoOpenMarginNumericUpDown)).EndInit();
            this.listViewContColorTabPage.ResumeLayout(false);
            this.listViewContColorTabPage.PerformLayout();
            this.contextMenuFontColorTabPage.ResumeLayout(false);
            this.contextMenuFontColorTabPage.PerformLayout();
            this.controlUiFontColorTabPage.ResumeLayout(false);
            this.controlUiFontColorTabPage.PerformLayout();
            this.otherTabPage.ResumeLayout(false);
            this.otherTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.recListMaxCountNumericUpDown)).EndInit();
            this.taskTrayIconClickGroupBox.ResumeLayout(false);
            this.taskTrayIconClickGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.ListView allServiceListView;
        private System.Windows.Forms.Button addSelectedServiceButton;
        private System.Windows.Forms.Button addNewServiceButton;
        private System.Windows.Forms.Button editServiceButton;
        private System.Windows.Forms.Button removeSelectedServiceButton;
        private System.Windows.Forms.Button moveDownSelectedServiceButton;
        private System.Windows.Forms.Button moveUpSelectedServiceButton;
        private System.Windows.Forms.ColumnHeader allServiceNetworkTypeColumnHeader;
        private System.Windows.Forms.ColumnHeader allServiceNameColumnHeader;
        private System.Windows.Forms.ColumnHeader allServiceTsidColumnHeader;
        private System.Windows.Forms.ColumnHeader allServiceSidColumnHeader;
        private System.Windows.Forms.ColumnHeader allServiceMarkColumnHeader;
        private System.Windows.Forms.ListView selectedServiceListView;
        private System.Windows.Forms.ColumnHeader selectedServiceMarkColumnHeader;
        private System.Windows.Forms.ColumnHeader selectedServiceNetworkTypeColumnHeader;
        private System.Windows.Forms.ColumnHeader selectedServiceNameColumnHeader;
        private System.Windows.Forms.ColumnHeader selectedServiceTsidColumnHeader;
        private System.Windows.Forms.ColumnHeader selectedServiceSidColumnHeader;
        private System.Windows.Forms.ColumnHeader selectedServiceTvtestOptionColumnHeader;
        private System.Windows.Forms.TabControl settingTabControl;
        private System.Windows.Forms.TabPage allServiceTabPage;
        private System.Windows.Forms.TabPage edcbLinkageTabPage;
        private System.Windows.Forms.OpenFileDialog tvtestOpenFileDialog;
        private System.Windows.Forms.Label webLinkUrlExampleLabel;
        private System.Windows.Forms.CheckBox useWebLinkCheckBox;
        private System.Windows.Forms.Label webEPGLabel;
        private System.Windows.Forms.TextBox webEpgUrlTextBox;
        private System.Windows.Forms.Label webLinkUrlLabel;
        private System.Windows.Forms.TextBox webLinkUrlTextBox;
        private System.Windows.Forms.Label recInfoWebLinkUrlLabel;
        private System.Windows.Forms.TextBox recInfoWebLinkUrlTextBox;
        private System.Windows.Forms.TabPage favoriteServiceTabPage;
        private System.Windows.Forms.ListView selectedServiceListView2;
        private System.Windows.Forms.ColumnHeader selectedService2MarkColumnHeader;
        private System.Windows.Forms.ColumnHeader selectedService2NetworkTypeColumnHeader;
        private System.Windows.Forms.ColumnHeader selectedService2NameColumnHeader;
        private System.Windows.Forms.ColumnHeader selectedService2TsidColumnHeader;
        private System.Windows.Forms.ColumnHeader selectedService2SidColumnHeader;
        private System.Windows.Forms.ColumnHeader selectedService2TvtestOptionColumnHeader;
        private System.Windows.Forms.Button moveUpFavoriteServiceButton;
        private System.Windows.Forms.Button moveDownFavoriteServiceButton;
        private System.Windows.Forms.Button removeFavoriteServiceButton;
        private System.Windows.Forms.ListView favoriteServiceListView;
        private System.Windows.Forms.ColumnHeader favoriteServiceMarkColumnHeader;
        private System.Windows.Forms.ColumnHeader favoriteServiceNetworkTypeColumnHeader;
        private System.Windows.Forms.ColumnHeader favoriteServiceNameColumnHeader;
        private System.Windows.Forms.ColumnHeader favoriteServiceTsidColumnHeader;
        private System.Windows.Forms.ColumnHeader favoriteServiceSidColumnHeader;
        private System.Windows.Forms.ColumnHeader favoriteServiceTvtestOptionColumnHeader;
        private System.Windows.Forms.Button addFavoriteServiceButton;
        private System.Windows.Forms.TabPage tvtestLinkageTabPage;
        private System.Windows.Forms.Label portNumberLabel;
        private System.Windows.Forms.CheckBox useTcpIpCheckBox;
        private System.Windows.Forms.Label ipAddressLabel;
        private System.Windows.Forms.TextBox ipAddressTextBox;
        private System.Windows.Forms.Label tvTestNoteLabel;
        private System.Windows.Forms.CheckBox useDoubleClickTvtestCheckBox;
        private System.Windows.Forms.Label tvtestDttvOptionLabel;
        private System.Windows.Forms.TextBox tvtestDttvOptionTextBox;
        private System.Windows.Forms.Label tvtestBscsOptionLabel;
        private System.Windows.Forms.TextBox tvtestBscsOptionTextBox;
        private System.Windows.Forms.Label tvtestTsFileOptionLabel;
        private System.Windows.Forms.TextBox tvtestTsFileOptionTextBox;
        private System.Windows.Forms.Label tvtestPathLabel;
        private System.Windows.Forms.Button tvtestOpenButton;
        private System.Windows.Forms.TextBox tvtestPathTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown portNumberNumericUpDown;
        private System.Windows.Forms.Label selectedServiceListLabel;
        private System.Windows.Forms.Label allServiceListLabel;
        private System.Windows.Forms.Label favoriteServiceListLabel;
        private System.Windows.Forms.Label selectedServiceList2Label;
        private System.Windows.Forms.NumericUpDown autoCloseMarginNumericUpDown;
        private System.Windows.Forms.Label autoCloseMarginLabel;
        private System.Windows.Forms.NumericUpDown autoOpenMarginNumericUpDown;
        private System.Windows.Forms.Label autoOpenMarginLabel;
        private System.Windows.Forms.CheckBox isAutoOpenTvtestCheckBox;
        private System.Windows.Forms.GroupBox autoStartTargetGroupBox;
        private System.Windows.Forms.CheckBox isAutoOpenDttvCheckBox;
        private System.Windows.Forms.CheckBox isAutoOpenCsCheckBox;
        private System.Windows.Forms.CheckBox isAutoOpenBsCheckBox;
        private System.Windows.Forms.Label portNumberNoteLabel;
        private System.Windows.Forms.TabPage otherTabPage;
        private System.Windows.Forms.CheckBox showTaskTrayIconCheckBox;
        private System.Windows.Forms.CheckBox fixNoRecToServiceOnlyCheckBox;
        private System.Windows.Forms.CheckBox isAutoOpenFavoriteServiceCheckBox;
        private System.Windows.Forms.Label recListMaxCountLabel;
        private System.Windows.Forms.NumericUpDown recListMaxCountNumericUpDown;
        private System.Windows.Forms.Label tvtestOptionExampleLabel;
        private System.Windows.Forms.Label tvtestTsFileOptionExampleLabel;
        private System.Windows.Forms.CheckBox toggleVisibleTaskTrayIconClickCheckBox;
        private System.Windows.Forms.CheckBox storeTaskTrayByClosingCheckBox;
        private System.Windows.Forms.CheckBox isHorizontalSplitCheckBox;
        private System.Windows.Forms.FontDialog fontDialog;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.TabPage tunerTabPage;
        private System.Windows.Forms.ListView tunerNameListView;
        private System.Windows.Forms.ColumnHeader tunerNameTunerIdColumnHeader;
        private System.Windows.Forms.ColumnHeader tunerNameBonDriverNameColumnHeader;
        private System.Windows.Forms.ColumnHeader tunerNameTunerNameColumnHeader;
        private System.Windows.Forms.Button updateTunerNameButton;
        private System.Windows.Forms.TextBox tunerNameTextBox;
        private System.Windows.Forms.Label tunerNameNoteLabel;
        private System.Windows.Forms.Label tunerNameLabel;
        private System.Windows.Forms.ColumnHeader tunerNameMarkColumnHeader;
        private System.Windows.Forms.TabPage listViewContColorTabPage;
        private System.Windows.Forms.Label previewLabel;
        private System.Windows.Forms.Label fontLabel;
        private System.Windows.Forms.TextBox fontTextBox;
        private System.Windows.Forms.Button selectFontButton;
        private System.Windows.Forms.Label ngReserveListBackColorLabel;
        private System.Windows.Forms.TextBox ngReserveListBackColorTextBox;
        private System.Windows.Forms.Button selectNgReserveListBackColorButton;
        private System.Windows.Forms.Label partialReserveListBackColorLabel;
        private System.Windows.Forms.TextBox partialReserveListBackColorTextBox;
        private System.Windows.Forms.Button selectPartialReserveListBackColorButton;
        private System.Windows.Forms.Label okReserveListBackColorLabel;
        private System.Windows.Forms.TextBox okReserveListBackColorTextBox;
        private System.Windows.Forms.Button selectOkReserveListBackColorButton;
        private System.Windows.Forms.Label disabledReserveListBackColorLabel;
        private System.Windows.Forms.TextBox disabledReserveListBackColorTextBox;
        private System.Windows.Forms.Button selectDisabledReserveListBackColorButton;
        private System.Windows.Forms.Label listHeaderForeColorLabel;
        private System.Windows.Forms.TextBox listHeaderForeColorTextBox;
        private System.Windows.Forms.Button selectListHeaderForeColorButton;
        private System.Windows.Forms.Label listHeaderBackColorLabel;
        private System.Windows.Forms.TextBox listHeaderBackColorTextBox;
        private System.Windows.Forms.Button selectListHeaderBackColorButton;
        private System.Windows.Forms.ListView previewListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label foreColorLabel;
        private System.Windows.Forms.TextBox foreColorTextBox;
        private System.Windows.Forms.Button selectForeColorButton;
        private System.Windows.Forms.Label listBackColorLabel;
        private System.Windows.Forms.TextBox listBackColorTextBox;
        private System.Windows.Forms.Button selectListBackColorButton;
        private System.Windows.Forms.Panel previewFormPanel;
        private System.Windows.Forms.Label formBackColorLabel;
        private System.Windows.Forms.TextBox formBackColorTextBox;
        private System.Windows.Forms.Button selectFormBackColorButton;
        private System.Windows.Forms.TabPage contextMenuFontColorTabPage;
        private System.Windows.Forms.TabPage controlUiFontColorTabPage;
        private System.Windows.Forms.Label ngReserveMenuBackColorLabel;
        private System.Windows.Forms.TextBox ngReserveMenuBackColorTextBox;
        private System.Windows.Forms.Button selectNgReserveMenuBackColorButton;
        private System.Windows.Forms.Label partialReserveMenuBackColorLabel;
        private System.Windows.Forms.TextBox partialReserveMenuBackColorTextBox;
        private System.Windows.Forms.Button selectPartialReserveMenuBackColorButton;
        private System.Windows.Forms.Label okReserveMenuBackColorLabel;
        private System.Windows.Forms.TextBox okReserveMenuBackColorTextBox;
        private System.Windows.Forms.Button selectOkReserveMenuBackColorButton;
        private System.Windows.Forms.Label disabledReserveMenuBackColorLabel;
        private System.Windows.Forms.TextBox disabledReserveMenuBackColorTextBox;
        private System.Windows.Forms.Button selectDisabledReserveMenuBackColorButton;
        private System.Windows.Forms.ListView previewMenuListView;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label menuBackColorLabel;
        private System.Windows.Forms.TextBox menuBackColorTextBox;
        private System.Windows.Forms.Button selectMenuBackColorButton;
        private System.Windows.Forms.Label menuFontLabel;
        private System.Windows.Forms.TextBox menuFontTextBox;
        private System.Windows.Forms.Button selectMenuFontButton;
        private System.Windows.Forms.Label tabFontLabel;
        private System.Windows.Forms.TextBox tabFontTextBox;
        private System.Windows.Forms.Button selectTabFontButton;
        private System.Windows.Forms.Label buttonFontLabel;
        private System.Windows.Forms.TextBox buttonFontTextBox;
        private System.Windows.Forms.Button selectButtonFontButton;
        private System.Windows.Forms.Label labelFontLabel;
        private System.Windows.Forms.TextBox labelFontTextBox;
        private System.Windows.Forms.Button selectLabelFontButton;
        private System.Windows.Forms.Label textBoxFontLabel;
        private System.Windows.Forms.TextBox textBoxFontTextBox;
        private System.Windows.Forms.Button selectTextBoxFontButton;
        private System.Windows.Forms.ComboBox taskTrayIconLeftClickComboBox;
        private System.Windows.Forms.ComboBox taskTrayIconRightDoubleClickComboBox;
        private System.Windows.Forms.ComboBox taskTrayIconRightClickComboBox;
        private System.Windows.Forms.ComboBox taskTrayIconLeftDoubleClickComboBox;
        private System.Windows.Forms.Label taskTrayIconRightDoubleClickLabel;
        private System.Windows.Forms.Label taskTrayIconRightClickLabel;
        private System.Windows.Forms.Label taskTrayIconLeftDoubleClickLabel;
        private System.Windows.Forms.Label taskTrayIconLeftClickLabel;
        private System.Windows.Forms.GroupBox taskTrayIconClickGroupBox;
        private System.Windows.Forms.Label tvtestCatvOptionLabel;
        private System.Windows.Forms.TextBox tvtestCatvOptionTextBox;
        private System.Windows.Forms.Label tvtestSphdOptionLabel;
        private System.Windows.Forms.TextBox tvtestBs4kOptionTextBox;
        private System.Windows.Forms.TextBox tvtestSphdOptionTextBox;
        private System.Windows.Forms.Label tvtestBs4kOptionLabel;
        private System.Windows.Forms.CheckBox isAutoOpenCatvCheckBox;
        private System.Windows.Forms.CheckBox isAutoOpenSphdCheckBox;
        private System.Windows.Forms.CheckBox isAutoOpenBs4kCheckBox;
        private System.Windows.Forms.TabPage reserveTabPage;
        private System.Windows.Forms.CheckBox useRockbarReserveAddCheckBox;
        private System.Windows.Forms.CheckBox enableReserveCheckBox;
        private System.Windows.Forms.Label recServiceDataLabel;
        private System.Windows.Forms.Label recMarginLabel;
        private System.Windows.Forms.Label recPriorityLabel;
        private System.Windows.Forms.ComboBox recPriorityComboBox;
        private System.Windows.Forms.CheckBox recPittariCheckBox;
        private System.Windows.Forms.CheckBox recTuijyuuCheckBox;
        private System.Windows.Forms.Label recModeLabel;
        private System.Windows.Forms.ComboBox recModeComboBox;
        private System.Windows.Forms.ListView recFolderListView;
        private System.Windows.Forms.ColumnHeader partialRecColumnHeader;
        private System.Windows.Forms.ColumnHeader recFolderColumnHeader;
        private System.Windows.Forms.ColumnHeader writePlugInColumnHeader;
        private System.Windows.Forms.ColumnHeader recNamePlugInColumnHeader;
        private System.Windows.Forms.CheckBox recServiceDataCarouselCheckBox;
        private System.Windows.Forms.CheckBox recServiceDataCaptionCheckBox;
        private System.Windows.Forms.Label endRecMarginLabel;
        private System.Windows.Forms.Label startRecMarginLabel;
        private System.Windows.Forms.CheckBox useDefaultRecServiceDataCheckBox;
        private System.Windows.Forms.CheckBox useDefaultRecMarginCheckBox;
        private System.Windows.Forms.CheckBox partialRecSeparateFileCheckBox;
        private System.Windows.Forms.Label recBatFilePathLabel;
        private System.Windows.Forms.CheckBox rebootAfterReturnCheckBox;
        private System.Windows.Forms.RadioButton afterRecShutdownRadioButton;
        private System.Windows.Forms.RadioButton afterRecSuspendRadioButton;
        private System.Windows.Forms.RadioButton afterRecStandbyRadioButton;
        private System.Windows.Forms.RadioButton afterRecNoActionRadioButton;
        private System.Windows.Forms.CheckBox defaultSuspendModeAfterRecCheckBox;
        private System.Windows.Forms.Label suspendModeAfterRecLabel;
        private System.Windows.Forms.ComboBox recTunerIdComboBox;
        private System.Windows.Forms.Label recTunerIdLabel;
        private System.Windows.Forms.CheckBox continueRecSameFileCheckBox;
        private System.Windows.Forms.TextBox recTagTextBox;
        private System.Windows.Forms.TextBox recBatFilePathTextBox;
        private System.Windows.Forms.Label recTagLabel;
        private System.Windows.Forms.GroupBox edcbReserveSettingGroupBox;
        private System.Windows.Forms.Button browseBatFileButton;
        private System.Windows.Forms.Button deleteRecFolderButton;
        private System.Windows.Forms.Button copyRecFolderButton;
        private System.Windows.Forms.Button editRecFolderButton;
        private System.Windows.Forms.Button addRecFolderButton;
        private System.Windows.Forms.NumericUpDown startRecMarginNumericUpDown;
        private System.Windows.Forms.NumericUpDown endRecMarginNumericUpDown;
        private System.Windows.Forms.RadioButton prioritizeRecRadioButton;
        private System.Windows.Forms.RadioButton prioritizeViewRadioButton;
        private System.Windows.Forms.Panel suspendModeAfterRecPanel;
        private System.Windows.Forms.Panel prioritizeViewPanel;
        private System.Windows.Forms.Label prioritizeViewLabel;
        private System.Windows.Forms.TextBox recCommentTextBox;
        private System.Windows.Forms.Label recCommentLabel;
        private System.Windows.Forms.CheckBox useRockbarReserveDelCheckBox;
        private System.Windows.Forms.CheckBox useRockbarReserveModCheckBox;
        private System.Windows.Forms.CheckBox useRockbarReserveDelConfirmCheckBox;
        private System.Windows.Forms.Label useRockbarReserveLabel;
    }
}