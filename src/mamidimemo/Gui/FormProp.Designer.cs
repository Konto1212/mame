namespace zanac.MAmidiMEmo.Gui
{
    partial class FormProp
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
            this.components = new System.ComponentModel.Container();
            this.propertyGrid = new System.Windows.Forms.PropertyGrid();
            this.toolStrip3 = new zanac.MAmidiMEmo.ComponentModel.ToolStripBase();
            this.toolStripButtonCat = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonA2Z = new System.Windows.Forms.ToolStripButton();
            this.toolStripButtonPopup = new System.Windows.Forms.ToolStripButton();
            this.contextMenuStripProp = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.resetToDefaultThisPropertyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip3.SuspendLayout();
            this.contextMenuStripProp.SuspendLayout();
            this.SuspendLayout();
            // 
            // propertyGrid
            // 
            this.propertyGrid.ContextMenuStrip = this.contextMenuStripProp;
            this.propertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid.Location = new System.Drawing.Point(0, 25);
            this.propertyGrid.Name = "propertyGrid";
            this.propertyGrid.Size = new System.Drawing.Size(487, 367);
            this.propertyGrid.TabIndex = 2;
            this.propertyGrid.ToolbarVisible = false;
            // 
            // toolStrip3
            // 
            this.toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButtonCat,
            this.toolStripButtonA2Z,
            this.toolStripButtonPopup});
            this.toolStrip3.Location = new System.Drawing.Point(0, 0);
            this.toolStrip3.Name = "toolStrip3";
            this.toolStrip3.Size = new System.Drawing.Size(487, 25);
            this.toolStrip3.TabIndex = 3;
            this.toolStrip3.Text = "toolStrip3";
            // 
            // toolStripButtonCat
            // 
            this.toolStripButtonCat.Checked = true;
            this.toolStripButtonCat.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripButtonCat.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonCat.Image = global::zanac.MAmidiMEmo.Properties.Resources.Cat;
            this.toolStripButtonCat.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonCat.Name = "toolStripButtonCat";
            this.toolStripButtonCat.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonCat.Text = "Categorized sort";
            this.toolStripButtonCat.Click += new System.EventHandler(this.toolStripButtonCat_Click);
            // 
            // toolStripButtonA2Z
            // 
            this.toolStripButtonA2Z.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonA2Z.Image = global::zanac.MAmidiMEmo.Properties.Resources.AtoZ;
            this.toolStripButtonA2Z.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonA2Z.Name = "toolStripButtonA2Z";
            this.toolStripButtonA2Z.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonA2Z.Text = "Alphabetical sort";
            this.toolStripButtonA2Z.Click += new System.EventHandler(this.toolStripButtonA2Z_Click);
            // 
            // toolStripButtonPopup
            // 
            this.toolStripButtonPopup.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButtonPopup.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButtonPopup.Image = global::zanac.MAmidiMEmo.Properties.Resources.Popup;
            this.toolStripButtonPopup.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButtonPopup.Name = "toolStripButtonPopup";
            this.toolStripButtonPopup.Size = new System.Drawing.Size(23, 22);
            this.toolStripButtonPopup.Text = "Popup the property";
            this.toolStripButtonPopup.Click += new System.EventHandler(this.toolStripButtonPopup_Click);
            // 
            // contextMenuStripProp
            // 
            this.contextMenuStripProp.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.resetToDefaultThisPropertyToolStripMenuItem});
            this.contextMenuStripProp.Name = "contextMenuStripProp";
            this.contextMenuStripProp.Size = new System.Drawing.Size(227, 26);
            this.contextMenuStripProp.Click += new System.EventHandler(this.contextMenuStripProp_Click);
            // 
            // resetToDefaultThisPropertyToolStripMenuItem
            // 
            this.resetToDefaultThisPropertyToolStripMenuItem.Name = "resetToDefaultThisPropertyToolStripMenuItem";
            this.resetToDefaultThisPropertyToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.resetToDefaultThisPropertyToolStripMenuItem.Text = "&Reset to default this property";
            // 
            // FormProp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(487, 392);
            this.Controls.Add(this.propertyGrid);
            this.Controls.Add(this.toolStrip3);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormProp";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "FormProp";
            this.toolStrip3.ResumeLayout(false);
            this.toolStrip3.PerformLayout();
            this.contextMenuStripProp.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGrid;
        private ComponentModel.ToolStripBase toolStrip3;
        private System.Windows.Forms.ToolStripButton toolStripButtonCat;
        private System.Windows.Forms.ToolStripButton toolStripButtonA2Z;
        private System.Windows.Forms.ToolStripButton toolStripButtonPopup;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripProp;
        private System.Windows.Forms.ToolStripMenuItem resetToDefaultThisPropertyToolStripMenuItem;
    }
}