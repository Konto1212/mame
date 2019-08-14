namespace zanac.MAmidiMEmo.Gui
{
    partial class FormMain
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                Instruments.InstrumentManager.InstrumentAdded -= InstrumentManager_InstrumentAdded;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.listViewIntruments = new System.Windows.Forms.ListView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.decreaseThisKindOfChipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxMidiIf = new System.Windows.Forms.ToolStripComboBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.addYM2151ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addYM2612ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSN76496ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNAMCOCUS30ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendGBAPUToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendNESAPUToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendSCC1kToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendYM3812ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            resources.ApplyResources(this.splitContainer1.Panel1, "splitContainer1.Panel1");
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.listView1);
            resources.ApplyResources(this.splitContainer1.Panel2, "splitContainer1.Panel2");
            // 
            // splitContainer2
            // 
            resources.ApplyResources(this.splitContainer2, "splitContainer2");
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.listViewIntruments);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.propertyGrid);
            // 
            // listViewIntruments
            // 
            this.listViewIntruments.ContextMenuStrip = this.contextMenuStrip1;
            resources.ApplyResources(this.listViewIntruments, "listViewIntruments");
            this.listViewIntruments.HideSelection = false;
            this.listViewIntruments.LargeImageList = this.imageList1;
            this.listViewIntruments.Name = "listViewIntruments";
            this.listViewIntruments.SmallImageList = this.imageList1;
            this.listViewIntruments.UseCompatibleStateImageBehavior = false;
            this.listViewIntruments.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewIntruments_ItemSelectionChanged);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.decreaseThisKindOfChipToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenuStrip1, "contextMenuStrip1");
            // 
            // decreaseThisKindOfChipToolStripMenuItem
            // 
            this.decreaseThisKindOfChipToolStripMenuItem.Name = "decreaseThisKindOfChipToolStripMenuItem";
            resources.ApplyResources(this.decreaseThisKindOfChipToolStripMenuItem, "decreaseThisKindOfChipToolStripMenuItem");
            this.decreaseThisKindOfChipToolStripMenuItem.Click += new System.EventHandler(this.decreaseThisKindOfChipToolStripMenuItem_Click);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            resources.ApplyResources(this.imageList1, "imageList1");
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // propertyGrid
            // 
            resources.ApplyResources(this.propertyGrid, "propertyGrid");
            this.propertyGrid.Name = "propertyGrid";
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            resources.ApplyResources(this.listView1, "listView1");
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.Name = "listView1";
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            resources.ApplyResources(this.statusStrip1, "statusStrip1");
            this.statusStrip1.Name = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            resources.ApplyResources(this.toolStripStatusLabel1, "toolStripStatusLabel1");
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            resources.ApplyResources(this.menuStrip1, "menuStrip1");
            this.menuStrip1.Name = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripMenuItemExit});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            resources.ApplyResources(this.fileToolStripMenuItem, "fileToolStripMenuItem");
            // 
            // loadToolStripMenuItem
            // 
            this.loadToolStripMenuItem.Name = "loadToolStripMenuItem";
            resources.ApplyResources(this.loadToolStripMenuItem, "loadToolStripMenuItem");
            this.loadToolStripMenuItem.Click += new System.EventHandler(this.loadToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            resources.ApplyResources(this.saveToolStripMenuItem, "saveToolStripMenuItem");
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            resources.ApplyResources(this.saveAsToolStripMenuItem, "saveAsToolStripMenuItem");
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripMenuItemExit
            // 
            this.toolStripMenuItemExit.Name = "toolStripMenuItemExit";
            resources.ApplyResources(this.toolStripMenuItemExit, "toolStripMenuItemExit");
            this.toolStripMenuItemExit.Click += new System.EventHandler(this.toolStripMenuItemExit_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemAbout});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            resources.ApplyResources(this.helpToolStripMenuItem, "helpToolStripMenuItem");
            // 
            // toolStripMenuItemAbout
            // 
            this.toolStripMenuItemAbout.Name = "toolStripMenuItemAbout";
            resources.ApplyResources(this.toolStripMenuItemAbout, "toolStripMenuItemAbout");
            this.toolStripMenuItemAbout.Click += new System.EventHandler(this.toolStripMenuItemAbout_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripComboBoxMidiIf,
            this.toolStripDropDownButton1});
            resources.ApplyResources(this.toolStrip1, "toolStrip1");
            this.toolStrip1.Name = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Name = "toolStripLabel1";
            resources.ApplyResources(this.toolStripLabel1, "toolStripLabel1");
            // 
            // toolStripComboBoxMidiIf
            // 
            this.toolStripComboBoxMidiIf.Name = "toolStripComboBoxMidiIf";
            resources.ApplyResources(this.toolStripComboBoxMidiIf, "toolStripComboBoxMidiIf");
            this.toolStripComboBoxMidiIf.DropDown += new System.EventHandler(this.toolStripComboBox1_DropDown);
            this.toolStripComboBoxMidiIf.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBox1_SelectedIndexChanged);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "MAmi";
            this.saveFileDialog1.FileName = "MyEnvironment";
            resources.ApplyResources(this.saveFileDialog1, "saveFileDialog1");
            this.saveFileDialog1.SupportMultiDottedExtensions = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "*.MAmi";
            this.openFileDialog1.FileName = "openFileDialog1";
            resources.ApplyResources(this.openFileDialog1, "openFileDialog1");
            this.openFileDialog1.SupportMultiDottedExtensions = true;
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addYM2151ToolStripMenuItem,
            this.addYM2612ToolStripMenuItem,
            this.extendYM3812ToolStripMenuItem,
            this.addSN76496ToolStripMenuItem,
            this.addNAMCOCUS30ToolStripMenuItem,
            this.extendGBAPUToolStripMenuItem,
            this.extendNESAPUToolStripMenuItem,
            this.extendSCC1kToolStripMenuItem});
            resources.ApplyResources(this.toolStripDropDownButton1, "toolStripDropDownButton1");
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            // 
            // addYM2151ToolStripMenuItem
            // 
            this.addYM2151ToolStripMenuItem.Name = "addYM2151ToolStripMenuItem";
            resources.ApplyResources(this.addYM2151ToolStripMenuItem, "addYM2151ToolStripMenuItem");
            this.addYM2151ToolStripMenuItem.Click += new System.EventHandler(this.addYM2151ToolStripMenuItem_Click);
            // 
            // addYM2612ToolStripMenuItem
            // 
            this.addYM2612ToolStripMenuItem.Name = "addYM2612ToolStripMenuItem";
            resources.ApplyResources(this.addYM2612ToolStripMenuItem, "addYM2612ToolStripMenuItem");
            this.addYM2612ToolStripMenuItem.Click += new System.EventHandler(this.addYM2612ToolStripMenuItem_Click);
            // 
            // addSN76496ToolStripMenuItem
            // 
            this.addSN76496ToolStripMenuItem.Name = "addSN76496ToolStripMenuItem";
            resources.ApplyResources(this.addSN76496ToolStripMenuItem, "addSN76496ToolStripMenuItem");
            this.addSN76496ToolStripMenuItem.Click += new System.EventHandler(this.addSN76496ToolStripMenuItem_Click);
            // 
            // addNAMCOCUS30ToolStripMenuItem
            // 
            this.addNAMCOCUS30ToolStripMenuItem.Name = "addNAMCOCUS30ToolStripMenuItem";
            resources.ApplyResources(this.addNAMCOCUS30ToolStripMenuItem, "addNAMCOCUS30ToolStripMenuItem");
            this.addNAMCOCUS30ToolStripMenuItem.Click += new System.EventHandler(this.addNAMCOCUS30ToolStripMenuItem_Click);
            // 
            // extendGBAPUToolStripMenuItem
            // 
            this.extendGBAPUToolStripMenuItem.Name = "extendGBAPUToolStripMenuItem";
            resources.ApplyResources(this.extendGBAPUToolStripMenuItem, "extendGBAPUToolStripMenuItem");
            this.extendGBAPUToolStripMenuItem.Click += new System.EventHandler(this.extendGBAPUToolStripMenuItem_Click);
            // 
            // extendNESAPUToolStripMenuItem
            // 
            this.extendNESAPUToolStripMenuItem.Name = "extendNESAPUToolStripMenuItem";
            resources.ApplyResources(this.extendNESAPUToolStripMenuItem, "extendNESAPUToolStripMenuItem");
            this.extendNESAPUToolStripMenuItem.Click += new System.EventHandler(this.extendNESAPUToolStripMenuItem_Click);
            // 
            // extendSCC1kToolStripMenuItem
            // 
            this.extendSCC1kToolStripMenuItem.Name = "extendSCC1kToolStripMenuItem";
            resources.ApplyResources(this.extendSCC1kToolStripMenuItem, "extendSCC1kToolStripMenuItem");
            this.extendSCC1kToolStripMenuItem.Click += new System.EventHandler(this.extendSCC1kToolStripMenuItem_Click);
            // 
            // extendYM3812ToolStripMenuItem
            // 
            this.extendYM3812ToolStripMenuItem.Name = "extendYM3812ToolStripMenuItem";
            resources.ApplyResources(this.extendYM3812ToolStripMenuItem, "extendYM3812ToolStripMenuItem");
            this.extendYM3812ToolStripMenuItem.Click += new System.EventHandler(this.extendYM3812ToolStripMenuItem_Click);
            // 
            // FormMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemExit;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemAbout;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListView listViewIntruments;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBoxMidiIf;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem addYM2612ToolStripMenuItem;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripMenuItem addSN76496ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNAMCOCUS30ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addYM2151ToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem decreaseThisKindOfChipToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendGBAPUToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripMenuItem extendNESAPUToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem extendSCC1kToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendYM3812ToolStripMenuItem;
    }
}

