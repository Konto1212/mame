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
            this.contextMenuStripInst = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.decreaseThisKindOfChipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuStripProp = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.resetToDefaultThisPropertyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripComboBoxMidiIf = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.pCMToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendC140ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fMSynthesisToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addYM2151ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addYM2612ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendYM3812ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendYM2413ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wSGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendSCC1kToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addNAMCOCUS30ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pSGToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendMOS8580ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendMOS6581ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendNESAPUToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendGBAPUToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addSN76496ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendAY38910ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendMSM5232ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extendBeepToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.listViewIntruments = new System.Windows.Forms.ListView();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.pianoControl1 = new zanac.MAmidiMEmo.Gui.PianoControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStripInst.SuspendLayout();
            this.contextMenuStripProp.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStripInst
            // 
            this.contextMenuStripInst.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.decreaseThisKindOfChipToolStripMenuItem});
            this.contextMenuStripInst.Name = "contextMenuStrip1";
            resources.ApplyResources(this.contextMenuStripInst, "contextMenuStripInst");
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
            // contextMenuStripProp
            // 
            this.contextMenuStripProp.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetToDefaultThisPropertyToolStripMenuItem});
            this.contextMenuStripProp.Name = "contextMenuStripProp";
            resources.ApplyResources(this.contextMenuStripProp, "contextMenuStripProp");
            // 
            // resetToDefaultThisPropertyToolStripMenuItem
            // 
            this.resetToDefaultThisPropertyToolStripMenuItem.Name = "resetToDefaultThisPropertyToolStripMenuItem";
            resources.ApplyResources(this.resetToDefaultThisPropertyToolStripMenuItem, "resetToDefaultThisPropertyToolStripMenuItem");
            this.resetToDefaultThisPropertyToolStripMenuItem.Click += new System.EventHandler(this.resetToDefaultThisPropertyToolStripMenuItem_Click);
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
            this.toolToolStripMenuItem,
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
            // toolToolStripMenuItem
            // 
            this.toolToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingsToolStripMenuItem});
            this.toolToolStripMenuItem.Name = "toolToolStripMenuItem";
            resources.ApplyResources(this.toolToolStripMenuItem, "toolToolStripMenuItem");
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            resources.ApplyResources(this.settingsToolStripMenuItem, "settingsToolStripMenuItem");
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
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
            this.toolStripDropDownButton1,
            this.toolStripButton1,
            this.toolStripButton2});
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
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pCMToolStripMenuItem,
            this.fMSynthesisToolStripMenuItem,
            this.wSGToolStripMenuItem,
            this.pSGToolStripMenuItem});
            resources.ApplyResources(this.toolStripDropDownButton1, "toolStripDropDownButton1");
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            // 
            // pCMToolStripMenuItem
            // 
            this.pCMToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extendC140ToolStripMenuItem});
            this.pCMToolStripMenuItem.Name = "pCMToolStripMenuItem";
            resources.ApplyResources(this.pCMToolStripMenuItem, "pCMToolStripMenuItem");
            // 
            // extendC140ToolStripMenuItem
            // 
            this.extendC140ToolStripMenuItem.Name = "extendC140ToolStripMenuItem";
            resources.ApplyResources(this.extendC140ToolStripMenuItem, "extendC140ToolStripMenuItem");
            this.extendC140ToolStripMenuItem.Click += new System.EventHandler(this.extendC140ToolStripMenuItem_Click);
            // 
            // fMSynthesisToolStripMenuItem
            // 
            this.fMSynthesisToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addYM2151ToolStripMenuItem,
            this.addYM2612ToolStripMenuItem,
            this.extendYM3812ToolStripMenuItem,
            this.extendYM2413ToolStripMenuItem});
            this.fMSynthesisToolStripMenuItem.Name = "fMSynthesisToolStripMenuItem";
            resources.ApplyResources(this.fMSynthesisToolStripMenuItem, "fMSynthesisToolStripMenuItem");
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
            // extendYM3812ToolStripMenuItem
            // 
            this.extendYM3812ToolStripMenuItem.Name = "extendYM3812ToolStripMenuItem";
            resources.ApplyResources(this.extendYM3812ToolStripMenuItem, "extendYM3812ToolStripMenuItem");
            this.extendYM3812ToolStripMenuItem.Click += new System.EventHandler(this.extendYM3812ToolStripMenuItem_Click);
            // 
            // extendYM2413ToolStripMenuItem
            // 
            this.extendYM2413ToolStripMenuItem.Name = "extendYM2413ToolStripMenuItem";
            resources.ApplyResources(this.extendYM2413ToolStripMenuItem, "extendYM2413ToolStripMenuItem");
            this.extendYM2413ToolStripMenuItem.Click += new System.EventHandler(this.extendYM2413ToolStripMenuItem_Click);
            // 
            // wSGToolStripMenuItem
            // 
            this.wSGToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extendSCC1kToolStripMenuItem,
            this.addNAMCOCUS30ToolStripMenuItem});
            this.wSGToolStripMenuItem.Name = "wSGToolStripMenuItem";
            resources.ApplyResources(this.wSGToolStripMenuItem, "wSGToolStripMenuItem");
            // 
            // extendSCC1kToolStripMenuItem
            // 
            this.extendSCC1kToolStripMenuItem.Name = "extendSCC1kToolStripMenuItem";
            resources.ApplyResources(this.extendSCC1kToolStripMenuItem, "extendSCC1kToolStripMenuItem");
            this.extendSCC1kToolStripMenuItem.Click += new System.EventHandler(this.extendSCC1kToolStripMenuItem_Click);
            // 
            // addNAMCOCUS30ToolStripMenuItem
            // 
            this.addNAMCOCUS30ToolStripMenuItem.Name = "addNAMCOCUS30ToolStripMenuItem";
            resources.ApplyResources(this.addNAMCOCUS30ToolStripMenuItem, "addNAMCOCUS30ToolStripMenuItem");
            this.addNAMCOCUS30ToolStripMenuItem.Click += new System.EventHandler(this.addNAMCOCUS30ToolStripMenuItem_Click);
            // 
            // pSGToolStripMenuItem
            // 
            this.pSGToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extendMOS8580ToolStripMenuItem,
            this.extendMOS6581ToolStripMenuItem,
            this.extendNESAPUToolStripMenuItem,
            this.extendGBAPUToolStripMenuItem,
            this.addSN76496ToolStripMenuItem,
            this.extendAY38910ToolStripMenuItem,
            this.extendMSM5232ToolStripMenuItem,
            this.extendBeepToolStripMenuItem});
            this.pSGToolStripMenuItem.Name = "pSGToolStripMenuItem";
            resources.ApplyResources(this.pSGToolStripMenuItem, "pSGToolStripMenuItem");
            // 
            // extendMOS8580ToolStripMenuItem
            // 
            this.extendMOS8580ToolStripMenuItem.Name = "extendMOS8580ToolStripMenuItem";
            resources.ApplyResources(this.extendMOS8580ToolStripMenuItem, "extendMOS8580ToolStripMenuItem");
            this.extendMOS8580ToolStripMenuItem.Click += new System.EventHandler(this.extendMOS8580ToolStripMenuItem_Click);
            // 
            // extendMOS6581ToolStripMenuItem
            // 
            this.extendMOS6581ToolStripMenuItem.Name = "extendMOS6581ToolStripMenuItem";
            resources.ApplyResources(this.extendMOS6581ToolStripMenuItem, "extendMOS6581ToolStripMenuItem");
            this.extendMOS6581ToolStripMenuItem.Click += new System.EventHandler(this.extendMOS6581ToolStripMenuItem_Click);
            // 
            // extendNESAPUToolStripMenuItem
            // 
            this.extendNESAPUToolStripMenuItem.Name = "extendNESAPUToolStripMenuItem";
            resources.ApplyResources(this.extendNESAPUToolStripMenuItem, "extendNESAPUToolStripMenuItem");
            this.extendNESAPUToolStripMenuItem.Click += new System.EventHandler(this.extendNESAPUToolStripMenuItem_Click);
            // 
            // extendGBAPUToolStripMenuItem
            // 
            this.extendGBAPUToolStripMenuItem.Name = "extendGBAPUToolStripMenuItem";
            resources.ApplyResources(this.extendGBAPUToolStripMenuItem, "extendGBAPUToolStripMenuItem");
            this.extendGBAPUToolStripMenuItem.Click += new System.EventHandler(this.extendGBAPUToolStripMenuItem_Click);
            // 
            // addSN76496ToolStripMenuItem
            // 
            this.addSN76496ToolStripMenuItem.Name = "addSN76496ToolStripMenuItem";
            resources.ApplyResources(this.addSN76496ToolStripMenuItem, "addSN76496ToolStripMenuItem");
            this.addSN76496ToolStripMenuItem.Click += new System.EventHandler(this.addSN76496ToolStripMenuItem_Click);
            // 
            // extendAY38910ToolStripMenuItem
            // 
            this.extendAY38910ToolStripMenuItem.Name = "extendAY38910ToolStripMenuItem";
            resources.ApplyResources(this.extendAY38910ToolStripMenuItem, "extendAY38910ToolStripMenuItem");
            this.extendAY38910ToolStripMenuItem.Click += new System.EventHandler(this.extendAY38910ToolStripMenuItem_Click);
            // 
            // extendMSM5232ToolStripMenuItem
            // 
            this.extendMSM5232ToolStripMenuItem.Name = "extendMSM5232ToolStripMenuItem";
            resources.ApplyResources(this.extendMSM5232ToolStripMenuItem, "extendMSM5232ToolStripMenuItem");
            this.extendMSM5232ToolStripMenuItem.Click += new System.EventHandler(this.extendMSM5232ToolStripMenuItem_Click);
            // 
            // extendBeepToolStripMenuItem
            // 
            this.extendBeepToolStripMenuItem.Name = "extendBeepToolStripMenuItem";
            resources.ApplyResources(this.extendBeepToolStripMenuItem, "extendBeepToolStripMenuItem");
            this.extendBeepToolStripMenuItem.Click += new System.EventHandler(this.extendBeepToolStripMenuItem_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton1.Image = global::zanac.MAmidiMEmo.Properties.Resources.Panic;
            resources.ApplyResources(this.toolStripButton1, "toolStripButton1");
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton2.Image = global::zanac.MAmidiMEmo.Properties.Resources.Rst;
            resources.ApplyResources(this.toolStripButton2, "toolStripButton2");
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
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
            // timer1
            // 
            this.timer1.Interval = 50;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
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
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
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
            this.listViewIntruments.ContextMenuStrip = this.contextMenuStripInst;
            resources.ApplyResources(this.listViewIntruments, "listViewIntruments");
            this.listViewIntruments.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewIntruments.Groups"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewIntruments.Groups1"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewIntruments.Groups2"))),
            ((System.Windows.Forms.ListViewGroup)(resources.GetObject("listViewIntruments.Groups3")))});
            this.listViewIntruments.HideSelection = false;
            this.listViewIntruments.LargeImageList = this.imageList1;
            this.listViewIntruments.Name = "listViewIntruments";
            this.listViewIntruments.ShowItemToolTips = true;
            this.listViewIntruments.SmallImageList = this.imageList1;
            this.listViewIntruments.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listViewIntruments.UseCompatibleStateImageBehavior = false;
            this.listViewIntruments.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listViewIntruments_ItemSelectionChanged);
            // 
            // propertyGrid
            // 
            this.propertyGrid.ContextMenuStrip = this.contextMenuStripProp;
            resources.ApplyResources(this.propertyGrid, "propertyGrid");
            this.propertyGrid.Name = "propertyGrid";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage2);
            resources.ApplyResources(this.tabControl1, "tabControl1");
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Black;
            resources.ApplyResources(this.tabPage1, "tabPage1");
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Paint += new System.Windows.Forms.PaintEventHandler(this.tabPage1_Paint);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.pianoControl1);
            resources.ApplyResources(this.tabPage3, "tabPage3");
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // pianoControl1
            // 
            resources.ApplyResources(this.pianoControl1, "pianoControl1");
            this.pianoControl1.Name = "pianoControl1";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.listView1);
            resources.ApplyResources(this.tabPage2, "tabPage2");
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            resources.ApplyResources(this.listView1, "listView1");
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView1.Name = "listView1";
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            resources.ApplyResources(this.columnHeader1, "columnHeader1");
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
            this.contextMenuStripInst.ResumeLayout(false);
            this.contextMenuStripProp.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
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
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripInst;
        private System.Windows.Forms.ToolStripMenuItem decreaseThisKindOfChipToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripMenuItem toolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripProp;
        private System.Windows.Forms.ToolStripMenuItem resetToDefaultThisPropertyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fMSynthesisToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addYM2151ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addYM2612ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendYM3812ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendYM2413ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wSGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendSCC1kToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNAMCOCUS30ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pSGToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendNESAPUToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addSN76496ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendMSM5232ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendAY38910ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendMOS8580ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendMOS6581ToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendBeepToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendGBAPUToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pCMToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extendC140ToolStripMenuItem;
        private System.Windows.Forms.TabPage tabPage3;
        private PianoControl pianoControl1;
    }
}

