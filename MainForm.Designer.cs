namespace RockbarForEDCB
{
    partial class MainForm
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "",
            "test",
            "test",
            "test"}, -1);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("");
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.aaaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.mainFormTabControl = new System.Windows.Forms.TabControl();
            this.allTabPage = new System.Windows.Forms.TabPage();
            this.dttvTabPage = new System.Windows.Forms.TabPage();
            this.bsTabPage = new System.Windows.Forms.TabPage();
            this.csTabPage = new System.Windows.Forms.TabPage();
            this.favoriteTabPage = new System.Windows.Forms.TabPage();
            this.newProgramTabPage = new System.Windows.Forms.TabPage();
            this.reserveTabPage = new System.Windows.Forms.TabPage();
            this.recTabPage = new System.Windows.Forms.TabPage();
            this.subListView = new System.Windows.Forms.ListView();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.filterTextBox = new System.Windows.Forms.TextBox();
            this.resetButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.settingButton = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.taskTrayContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openWebEpgTopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSettingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.listContextMenuStrip.SuspendLayout();
            this.mainFormTabControl.SuspendLayout();
            this.taskTrayContextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainListView
            // 
            this.mainListView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.mainListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.mainListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainListView.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(250)))), ((int)(((byte)(140)))));
            this.mainListView.FullRowSelect = true;
            this.mainListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.mainListView.HideSelection = false;
            this.mainListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3});
            this.mainListView.Location = new System.Drawing.Point(0, 0);
            this.mainListView.MultiSelect = false;
            this.mainListView.Name = "mainListView";
            this.mainListView.ShowItemToolTips = true;
            this.mainListView.Size = new System.Drawing.Size(467, 194);
            this.mainListView.TabIndex = 0;
            this.mainListView.UseCompatibleStateImageBehavior = false;
            this.mainListView.View = System.Windows.Forms.View.Details;
            this.mainListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.mainListView_ItemSelectionChanged);
            this.mainListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.mainListView_MouseClick);
            this.mainListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.mainListView_MouseDoubleClick);
            this.mainListView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.mainListView_MouseUp);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 27;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Width = 70;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Width = 24;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Width = 300;
            // 
            // listContextMenuStrip
            // 
            this.listContextMenuStrip.BackColor = System.Drawing.SystemColors.ControlLight;
            this.listContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aaaToolStripMenuItem});
            this.listContextMenuStrip.Name = "contextMenuStrip1";
            this.listContextMenuStrip.Size = new System.Drawing.Size(93, 26);
            // 
            // aaaToolStripMenuItem
            // 
            this.aaaToolStripMenuItem.Name = "aaaToolStripMenuItem";
            this.aaaToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.aaaToolStripMenuItem.Text = "aaa";
            // 
            // timer
            // 
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // mainFormTabControl
            // 
            this.mainFormTabControl.Controls.Add(this.allTabPage);
            this.mainFormTabControl.Controls.Add(this.dttvTabPage);
            this.mainFormTabControl.Controls.Add(this.bsTabPage);
            this.mainFormTabControl.Controls.Add(this.csTabPage);
            this.mainFormTabControl.Controls.Add(this.favoriteTabPage);
            this.mainFormTabControl.Controls.Add(this.newProgramTabPage);
            this.mainFormTabControl.Controls.Add(this.reserveTabPage);
            this.mainFormTabControl.Controls.Add(this.recTabPage);
            this.mainFormTabControl.Location = new System.Drawing.Point(8, 10);
            this.mainFormTabControl.Name = "mainFormTabControl";
            this.mainFormTabControl.SelectedIndex = 0;
            this.mainFormTabControl.Size = new System.Drawing.Size(388, 20);
            this.mainFormTabControl.TabIndex = 1;
            this.mainFormTabControl.SelectedIndexChanged += new System.EventHandler(this.mainFormTabControl_SelectedIndexChanged);
            // 
            // allTabPage
            // 
            this.allTabPage.Location = new System.Drawing.Point(4, 22);
            this.allTabPage.Name = "allTabPage";
            this.allTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.allTabPage.Size = new System.Drawing.Size(380, 0);
            this.allTabPage.TabIndex = 0;
            this.allTabPage.Text = "全て";
            this.allTabPage.UseVisualStyleBackColor = true;
            // 
            // dttvTabPage
            // 
            this.dttvTabPage.Location = new System.Drawing.Point(4, 22);
            this.dttvTabPage.Name = "dttvTabPage";
            this.dttvTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.dttvTabPage.Size = new System.Drawing.Size(380, 0);
            this.dttvTabPage.TabIndex = 1;
            this.dttvTabPage.Text = "地デジ";
            this.dttvTabPage.UseVisualStyleBackColor = true;
            // 
            // bsTabPage
            // 
            this.bsTabPage.Location = new System.Drawing.Point(4, 22);
            this.bsTabPage.Name = "bsTabPage";
            this.bsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.bsTabPage.Size = new System.Drawing.Size(380, 0);
            this.bsTabPage.TabIndex = 2;
            this.bsTabPage.Text = "BS";
            this.bsTabPage.UseVisualStyleBackColor = true;
            // 
            // csTabPage
            // 
            this.csTabPage.Location = new System.Drawing.Point(4, 22);
            this.csTabPage.Name = "csTabPage";
            this.csTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.csTabPage.Size = new System.Drawing.Size(380, 0);
            this.csTabPage.TabIndex = 3;
            this.csTabPage.Text = "CS";
            this.csTabPage.UseVisualStyleBackColor = true;
            // 
            // favoriteTabPage
            // 
            this.favoriteTabPage.Font = new System.Drawing.Font("MS UI Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.favoriteTabPage.Location = new System.Drawing.Point(4, 22);
            this.favoriteTabPage.Name = "favoriteTabPage";
            this.favoriteTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.favoriteTabPage.Size = new System.Drawing.Size(380, 0);
            this.favoriteTabPage.TabIndex = 4;
            this.favoriteTabPage.Text = "お気に入り";
            this.favoriteTabPage.UseVisualStyleBackColor = true;
            // 
            // newProgramTabPage
            // 
            this.newProgramTabPage.Location = new System.Drawing.Point(4, 22);
            this.newProgramTabPage.Name = "newProgramTabPage";
            this.newProgramTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.newProgramTabPage.Size = new System.Drawing.Size(380, 0);
            this.newProgramTabPage.TabIndex = 7;
            this.newProgramTabPage.Text = "新番組";
            this.newProgramTabPage.UseVisualStyleBackColor = true;
            // 
            // reserveTabPage
            // 
            this.reserveTabPage.Location = new System.Drawing.Point(4, 22);
            this.reserveTabPage.Name = "reserveTabPage";
            this.reserveTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.reserveTabPage.Size = new System.Drawing.Size(380, 0);
            this.reserveTabPage.TabIndex = 5;
            this.reserveTabPage.Text = "予約";
            this.reserveTabPage.UseVisualStyleBackColor = true;
            // 
            // recTabPage
            // 
            this.recTabPage.Location = new System.Drawing.Point(4, 22);
            this.recTabPage.Name = "recTabPage";
            this.recTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.recTabPage.Size = new System.Drawing.Size(380, 0);
            this.recTabPage.TabIndex = 6;
            this.recTabPage.Text = "録画";
            this.recTabPage.UseVisualStyleBackColor = true;
            // 
            // subListView
            // 
            this.subListView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.subListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8});
            this.subListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.subListView.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(250)))), ((int)(((byte)(140)))));
            this.subListView.FullRowSelect = true;
            this.subListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.subListView.HideSelection = false;
            this.subListView.Location = new System.Drawing.Point(0, 0);
            this.subListView.MultiSelect = false;
            this.subListView.Name = "subListView";
            this.subListView.ShowItemToolTips = true;
            this.subListView.Size = new System.Drawing.Size(260, 194);
            this.subListView.TabIndex = 2;
            this.subListView.UseCompatibleStateImageBehavior = false;
            this.subListView.View = System.Windows.Forms.View.Details;
            this.subListView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.subListView_MouseClick);
            this.subListView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.subListView_MouseUp);
            // 
            // columnHeader6
            // 
            this.columnHeader6.Width = 130;
            // 
            // filterTextBox
            // 
            this.filterTextBox.Location = new System.Drawing.Point(440, 9);
            this.filterTextBox.Name = "filterTextBox";
            this.filterTextBox.Size = new System.Drawing.Size(128, 19);
            this.filterTextBox.TabIndex = 4;
            this.filterTextBox.TextChanged += new System.EventHandler(this.filterTextBox_TextChanged);
            this.filterTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.filterTextBox_KeyDown);
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(570, 7);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(47, 23);
            this.resetButton.TabIndex = 5;
            this.resetButton.Text = "Reset";
            this.resetButton.UseVisualStyleBackColor = true;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.Location = new System.Drawing.Point(715, 7);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(24, 23);
            this.closeButton.TabIndex = 6;
            this.closeButton.Text = "×";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // settingButton
            // 
            this.settingButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.settingButton.Location = new System.Drawing.Point(689, 7);
            this.settingButton.Name = "settingButton";
            this.settingButton.Size = new System.Drawing.Size(24, 23);
            this.settingButton.TabIndex = 6;
            this.settingButton.Text = "設";
            this.settingButton.UseVisualStyleBackColor = true;
            this.settingButton.Click += new System.EventHandler(this.settingButton_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "Rockbar for EDCB";
            this.notifyIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDown);
            // 
            // taskTrayContextMenuStrip
            // 
            this.taskTrayContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openWebEpgTopToolStripMenuItem,
            this.openSettingToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.taskTrayContextMenuStrip.Name = "taskTrayContextMenuStrip";
            this.taskTrayContextMenuStrip.Size = new System.Drawing.Size(152, 70);
            // 
            // openWebEpgTopToolStripMenuItem
            // 
            this.openWebEpgTopToolStripMenuItem.Name = "openWebEpgTopToolStripMenuItem";
            this.openWebEpgTopToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.openWebEpgTopToolStripMenuItem.Text = "テレビ番組表";
            this.openWebEpgTopToolStripMenuItem.Click += new System.EventHandler(this.openWebEpgTopToolStripMenuItem_Click);
            // 
            // openSettingToolStripMenuItem
            // 
            this.openSettingToolStripMenuItem.Name = "openSettingToolStripMenuItem";
            this.openSettingToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.openSettingToolStripMenuItem.Text = "設定";
            this.openSettingToolStripMenuItem.Click += new System.EventHandler(this.settingButton_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(151, 22);
            this.exitToolStripMenuItem.Text = "Rockbarの終了";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(8, 32);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.mainListView);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.subListView);
            this.splitContainer.Size = new System.Drawing.Size(731, 194);
            this.splitContainer.SplitterDistance = 467;
            this.splitContainer.TabIndex = 11;
            this.splitContainer.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer_SplitterMoved);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(163)))), ((int)(((byte)(216)))), ((int)(((byte)(232)))));
            this.ClientSize = new System.Drawing.Size(746, 233);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.settingButton);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.resetButton);
            this.Controls.Add(this.filterTextBox);
            this.Controls.Add(this.mainFormTabControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(630, 100);
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.Text = "RockbarForEDCB";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseMove);
            this.listContextMenuStrip.ResumeLayout(false);
            this.mainFormTabControl.ResumeLayout(false);
            this.taskTrayContextMenuStrip.ResumeLayout(false);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView mainListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ContextMenuStrip listContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem aaaToolStripMenuItem;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.TabControl mainFormTabControl;
        private System.Windows.Forms.TabPage dttvTabPage;
        private System.Windows.Forms.TabPage bsTabPage;
        private System.Windows.Forms.TabPage csTabPage;
        private System.Windows.Forms.TabPage favoriteTabPage;
        private System.Windows.Forms.TabPage reserveTabPage;
        private System.Windows.Forms.TabPage recTabPage;
        private System.Windows.Forms.ListView subListView;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.TabPage allTabPage;
        private System.Windows.Forms.TextBox filterTextBox;
        private System.Windows.Forms.Button resetButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button settingButton;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip taskTrayContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openWebEpgTopToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSettingToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.TabPage newProgramTabPage;
    }
}

