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
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            splitContainer1 = new SplitContainer();
            treDir = new TreeView();
            splitContainer2 = new SplitContainer();
            flowThumbs = new FlowLayoutPanel();
            pnlAction = new Panel();
            Item1 = new TextBox();
            Item4 = new ComboBox();
            Item3 = new ComboBox();
            Item2 = new ComboBox();
            splitContainer3 = new SplitContainer();
            webViewPreview = new WebView2();
            txtOcr = new TextBox();
            pnlControl = new Panel();
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
            splitContainer1.Location = new Point(3, 201);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(treDir);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(1743, 685);
            splitContainer1.SplitterDistance = 453;
            splitContainer1.TabIndex = 4;
            // 
            // treDir
            // 
            treDir.Dock = DockStyle.Fill;
            treDir.Location = new Point(0, 0);
            treDir.Name = "treDir";
            treDir.Size = new Size(453, 685);
            treDir.TabIndex = 0;
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
            splitContainer2.Size = new Size(1286, 685);
            splitContainer2.SplitterDistance = 934;
            splitContainer2.TabIndex = 0;
            // 
            // flowThumbs
            // 
            flowThumbs.AutoScroll = true;
            flowThumbs.Dock = DockStyle.Fill;
            flowThumbs.FlowDirection = FlowDirection.TopDown;
            flowThumbs.Location = new Point(0, 127);
            flowThumbs.Margin = new Padding(6);
            flowThumbs.Name = "flowThumbs";
            flowThumbs.Padding = new Padding(12);
            flowThumbs.Size = new Size(934, 558);
            flowThumbs.TabIndex = 0;
            flowThumbs.WrapContents = false;
            // 
            // pnlAction
            // 
            pnlAction.Controls.Add(Item1);
            pnlAction.Controls.Add(Item4);
            pnlAction.Controls.Add(Item3);
            pnlAction.Controls.Add(Item2);
            pnlAction.Dock = DockStyle.Top;
            pnlAction.Location = new Point(0, 0);
            pnlAction.Name = "pnlAction";
            pnlAction.Size = new Size(934, 127);
            pnlAction.TabIndex = 0;
            // 
            // Item1
            // 
            Item1.Location = new Point(66, 19);
            Item1.Name = "Item1";
            Item1.Size = new Size(327, 39);
            Item1.TabIndex = 4;
            // 
            // Item4
            // 
            Item4.FormattingEnabled = true;
            Item4.Location = new Point(433, 78);
            Item4.Name = "Item4";
            Item4.Size = new Size(324, 40);
            Item4.TabIndex = 3;
            // 
            // Item3
            // 
            Item3.FormattingEnabled = true;
            Item3.Location = new Point(69, 78);
            Item3.Name = "Item3";
            Item3.Size = new Size(324, 40);
            Item3.TabIndex = 2;
            // 
            // Item2
            // 
            Item2.FormattingEnabled = true;
            Item2.Location = new Point(433, 21);
            Item2.Name = "Item2";
            Item2.Size = new Size(332, 40);
            Item2.TabIndex = 1;
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
            splitContainer3.Size = new Size(348, 685);
            splitContainer3.SplitterDistance = 330;
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
            webViewPreview.Size = new Size(348, 330);
            webViewPreview.TabIndex = 0;
            webViewPreview.ZoomFactor = 1D;
            // 
            // txtOcr
            // 
            txtOcr.Dock = DockStyle.Fill;
            txtOcr.Location = new Point(0, 0);
            txtOcr.Multiline = true;
            txtOcr.Name = "txtOcr";
            txtOcr.Size = new Size(348, 351);
            txtOcr.TabIndex = 0;
            // 
            // pnlControl
            // 
            pnlControl.Dock = DockStyle.Top;
            pnlControl.Location = new Point(3, 3);
            pnlControl.Name = "pnlControl";
            pnlControl.Size = new Size(1743, 198);
            pnlControl.TabIndex = 5;
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
    }
}
