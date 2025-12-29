using Microsoft.Web.WebView2.WinForms;
namespace PhotoLabel
{
    partial class FrmMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            splitContainer1 = new SplitContainer();
            panel2 = new Panel();
            treDir = new TreeView();
            treeContextMenu = new ContextMenuStrip(components);
            menuDeleteDirectory = new ToolStripMenuItem();
            panel1 = new Panel();
            txtFind = new TextBox();
            splitContainer2 = new SplitContainer();
            flowThumbs = new FlowLayoutPanel();
            pnlAction = new Panel();
            cmbSort = new ComboBox();
            cbxSelect = new CheckBox();
            splitContainer3 = new SplitContainer();
            webViewPreview = new WebView2();
            txtOcr = new TextBox();
            pnlControl = new Panel();
            btnDelete = new Button();
            btnRename = new Button();
            btnMove = new Button();
            Item1 = new TextBox();
            Item4 = new ComboBox();
            Item3 = new ComboBox();
            Item2 = new ComboBox();
            tabPage2 = new TabPage();
            label1 = new Label();
            txtTargetDir = new TextBox();
            btnSave = new Button();
            txtItems = new TextBox();
            cmbItems = new ComboBox();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel2.SuspendLayout();
            treeContextMenu.SuspendLayout();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            pnlAction.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webViewPreview).BeginInit();
            pnlControl.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1765, 943);
            tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(splitContainer1);
            tabPage1.Controls.Add(pnlControl);
            tabPage1.Location = new Point(8, 46);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1749, 889);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "変換";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(3, 80);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(panel2);
            splitContainer1.Panel1.Controls.Add(panel1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(1743, 806);
            splitContainer1.SplitterDistance = 453;
            splitContainer1.TabIndex = 4;
            // 
            // panel2
            // 
            panel2.Controls.Add(treDir);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(0, 49);
            panel2.Name = "panel2";
            panel2.Size = new Size(453, 757);
            panel2.TabIndex = 4;
            // 
            // treDir
            // 
            treDir.ContextMenuStrip = treeContextMenu;
            treDir.Dock = DockStyle.Fill;
            treDir.Location = new Point(0, 0);
            treDir.Name = "treDir";
            treDir.Size = new Size(453, 757);
            treDir.TabIndex = 0;
            // 
            // treeContextMenu
            // 
            treeContextMenu.ImageScalingSize = new Size(32, 32);
            treeContextMenu.Items.AddRange(new ToolStripItem[] { menuDeleteDirectory });
            treeContextMenu.Name = "treeContextMenu";
            treeContextMenu.Size = new Size(137, 42);
            // 
            // menuDeleteDirectory
            // 
            menuDeleteDirectory.Name = "menuDeleteDirectory";
            menuDeleteDirectory.Size = new Size(136, 38);
            menuDeleteDirectory.Text = "削除";
            menuDeleteDirectory.Click += menuDeleteDirectory_Click;
            // 
            // panel1
            // 
            panel1.Controls.Add(txtFind);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(453, 49);
            panel1.TabIndex = 3;
            // 
            // txtFind
            // 
            txtFind.Dock = DockStyle.Top;
            txtFind.Location = new Point(0, 0);
            txtFind.Name = "txtFind";
            txtFind.Size = new Size(453, 39);
            txtFind.TabIndex = 1;
            txtFind.TextChanged += txtFind_TextChanged;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(flowThumbs);
            splitContainer2.Panel1.Controls.Add(pnlAction);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(splitContainer3);
            splitContainer2.Size = new Size(1286, 806);
            splitContainer2.SplitterDistance = 934;
            splitContainer2.TabIndex = 0;
            // 
            // flowThumbs
            // 
            flowThumbs.AutoScroll = true;
            flowThumbs.Dock = DockStyle.Fill;
            flowThumbs.FlowDirection = FlowDirection.TopDown;
            flowThumbs.Location = new Point(0, 63);
            flowThumbs.Margin = new Padding(6);
            flowThumbs.Name = "flowThumbs";
            flowThumbs.Padding = new Padding(12);
            flowThumbs.Size = new Size(934, 743);
            flowThumbs.TabIndex = 0;
            flowThumbs.WrapContents = false;
            // 
            // pnlAction
            // 
            pnlAction.Controls.Add(cmbSort);
            pnlAction.Controls.Add(cbxSelect);
            pnlAction.Dock = DockStyle.Top;
            pnlAction.Location = new Point(0, 0);
            pnlAction.Name = "pnlAction";
            pnlAction.Size = new Size(934, 63);
            pnlAction.TabIndex = 0;
            // 
            // cmbSort
            // 
            cmbSort.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSort.FormattingEnabled = true;
            cmbSort.Items.AddRange(new object[] { "日時順", "名前順" });
            cmbSort.Location = new Point(91, 14);
            cmbSort.Name = "cmbSort";
            cmbSort.Size = new Size(242, 40);
            cmbSort.TabIndex = 6;
            cmbSort.SelectedIndexChanged += cmbSort_SelectedIndexChanged;
            // 
            // cbxSelect
            // 
            cbxSelect.AutoSize = true;
            cbxSelect.Location = new Point(15, 17);
            cbxSelect.Name = "cbxSelect";
            cbxSelect.Size = new Size(28, 27);
            cbxSelect.TabIndex = 5;
            cbxSelect.UseVisualStyleBackColor = true;
            cbxSelect.CheckedChanged += cbxSelect_CheckedChanged;
            // 
            // splitContainer3
            // 
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.Location = new Point(0, 0);
            splitContainer3.Name = "splitContainer3";
            splitContainer3.Orientation = Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(webViewPreview);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(txtOcr);
            splitContainer3.Size = new Size(348, 806);
            splitContainer3.SplitterDistance = 387;
            splitContainer3.TabIndex = 0;
            // 
            // webViewPreview
            // 
            webViewPreview.AllowExternalDrop = true;
            webViewPreview.CreationProperties = null;
            webViewPreview.DefaultBackgroundColor = Color.Black;
            webViewPreview.Dock = DockStyle.Fill;
            webViewPreview.Location = new Point(0, 0);
            webViewPreview.Name = "webViewPreview";
            webViewPreview.Size = new Size(348, 387);
            webViewPreview.TabIndex = 0;
            webViewPreview.ZoomFactor = 1D;
            // 
            // txtOcr
            // 
            txtOcr.Dock = DockStyle.Fill;
            txtOcr.Location = new Point(0, 0);
            txtOcr.Multiline = true;
            txtOcr.Name = "txtOcr";
            txtOcr.Size = new Size(348, 415);
            txtOcr.TabIndex = 0;
            // 
            // pnlControl
            // 
            pnlControl.Controls.Add(btnDelete);
            pnlControl.Controls.Add(btnRename);
            pnlControl.Controls.Add(btnMove);
            pnlControl.Controls.Add(Item1);
            pnlControl.Controls.Add(Item4);
            pnlControl.Controls.Add(Item3);
            pnlControl.Controls.Add(Item2);
            pnlControl.Dock = DockStyle.Top;
            pnlControl.Location = new Point(3, 3);
            pnlControl.Name = "pnlControl";
            pnlControl.Size = new Size(1743, 77);
            pnlControl.TabIndex = 5;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(1618, 14);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(113, 46);
            btnDelete.TabIndex = 8;
            btnDelete.Text = "削除";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnRename
            // 
            btnRename.Location = new Point(1352, 14);
            btnRename.Name = "btnRename";
            btnRename.Size = new Size(141, 46);
            btnRename.TabIndex = 6;
            btnRename.Text = "名前変更";
            btnRename.UseVisualStyleBackColor = true;
            btnRename.Click += btnRename_Click;
            // 
            // btnMove
            // 
            btnMove.Location = new Point(1499, 14);
            btnMove.Name = "btnMove";
            btnMove.Size = new Size(113, 46);
            btnMove.TabIndex = 7;
            btnMove.Text = "移動";
            btnMove.UseVisualStyleBackColor = true;
            btnMove.Click += btnMove_Click;
            // 
            // Item1
            // 
            Item1.Location = new Point(13, 15);
            Item1.Name = "Item1";
            Item1.Size = new Size(327, 39);
            Item1.TabIndex = 4;
            // 
            // Item4
            // 
            Item4.FormattingEnabled = true;
            Item4.Location = new Point(1014, 15);
            Item4.Name = "Item4";
            Item4.Size = new Size(332, 40);
            Item4.TabIndex = 3;
            // 
            // Item3
            // 
            Item3.FormattingEnabled = true;
            Item3.Location = new Point(346, 14);
            Item3.Name = "Item3";
            Item3.Size = new Size(324, 40);
            Item3.TabIndex = 2;
            // 
            // Item2
            // 
            Item2.FormattingEnabled = true;
            Item2.Location = new Point(676, 14);
            Item2.Name = "Item2";
            Item2.Size = new Size(332, 40);
            Item2.TabIndex = 1;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(label1);
            tabPage2.Controls.Add(txtTargetDir);
            tabPage2.Controls.Add(btnSave);
            tabPage2.Controls.Add(txtItems);
            tabPage2.Controls.Add(cmbItems);
            tabPage2.Location = new Point(8, 46);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1749, 889);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "設定";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(62, 275);
            label1.Name = "label1";
            label1.Size = new Size(133, 32);
            label1.TabIndex = 4;
            label1.Text = "参照フォルダ";
            // 
            // txtTargetDir
            // 
            txtTargetDir.Location = new Point(253, 272);
            txtTargetDir.Name = "txtTargetDir";
            txtTargetDir.Size = new Size(1164, 39);
            txtTargetDir.TabIndex = 3;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(1267, 359);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(150, 46);
            btnSave.TabIndex = 2;
            btnSave.Text = "保存";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // txtItems
            // 
            txtItems.Location = new Point(62, 116);
            txtItems.Multiline = true;
            txtItems.Name = "txtItems";
            txtItems.Size = new Size(1355, 102);
            txtItems.TabIndex = 1;
            // 
            // cmbItems
            // 
            cmbItems.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbItems.FormattingEnabled = true;
            cmbItems.Items.AddRange(new object[] { "Item1", "Item2", "Item3", "Item4" });
            cmbItems.Location = new Point(61, 50);
            cmbItems.Name = "cmbItems";
            cmbItems.Size = new Size(242, 40);
            cmbItems.TabIndex = 0;
            cmbItems.SelectedIndexChanged += cmbItems_SelectedIndexChanged;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1765, 943);
            Controls.Add(tabControl1);
            Name = "FrmMain";
            Text = "Form1";
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            treeContextMenu.ResumeLayout(false);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            pnlAction.ResumeLayout(false);
            pnlAction.PerformLayout();
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)webViewPreview).EndInit();
            pnlControl.ResumeLayout(false);
            pnlControl.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private SplitContainer splitContainer1;
        private TreeView treDir;
        private SplitContainer splitContainer2;
        private FlowLayoutPanel flowThumbs;
        private Panel pnlAction;
        private TextBox Item1;
        private ComboBox Item4;
        private ComboBox Item3;
        private ComboBox Item2;
        private SplitContainer splitContainer3;
        private WebView2 webViewPreview;
        private TextBox txtOcr;
        private Panel pnlControl;
        private TextBox txtItems;
        private ComboBox comboBox1;
        private Button btnSave;
        private ComboBox cmbItems;
        private Label label1;
        private TextBox txtTargetDir;
        private Button btnRename;
        private CheckBox cbxSelect;
        private Button btnMove;
        private ComboBox comboBox2;
        private ComboBox cmbSort;
        private TextBox txtFind;
        private Panel panel2;
        private Panel panel1;
        private Button btnDelete;
        private ContextMenuStrip treeContextMenu;
        private ToolStripMenuItem menuDeleteDirectory;
    }
}
