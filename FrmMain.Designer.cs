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
            tabPage2 = new TabPage();
            splitContainer1 = new SplitContainer();
            treDir = new TreeView();
            splitContainer2 = new SplitContainer();
            flowThumbs = new FlowLayoutPanel();
            pnlAction = new Panel();
            textBox1 = new TextBox();
            comboBox4 = new ComboBox();
            comboBox3 = new ComboBox();
            comboBox2 = new ComboBox();
            splitContainer3 = new SplitContainer();
            webViewPreview = new WebView2();
            txtOcr = new TextBox();
            pnlControl = new Panel();
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
            tabPage1.Text = "tabPage1";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(8, 46);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(384, 146);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
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
            pnlAction.Controls.Add(textBox1);
            pnlAction.Controls.Add(comboBox4);
            pnlAction.Controls.Add(comboBox3);
            pnlAction.Controls.Add(comboBox2);
            pnlAction.Dock = DockStyle.Top;
            pnlAction.Location = new Point(0, 0);
            pnlAction.Name = "pnlAction";
            pnlAction.Size = new Size(934, 127);
            pnlAction.TabIndex = 0;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(66, 19);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(327, 39);
            textBox1.TabIndex = 4;
            // 
            // comboBox4
            // 
            comboBox4.FormattingEnabled = true;
            comboBox4.Location = new Point(433, 78);
            comboBox4.Name = "comboBox4";
            comboBox4.Size = new Size(324, 40);
            comboBox4.TabIndex = 3;
            // 
            // comboBox3
            // 
            comboBox3.FormattingEnabled = true;
            comboBox3.Location = new Point(69, 78);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(324, 40);
            comboBox3.TabIndex = 2;
            // 
            // comboBox2
            // 
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(433, 21);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(332, 40);
            comboBox2.TabIndex = 1;
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
            splitContainer3.SplitterDistance = 331;
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
            webViewPreview.Size = new Size(348, 331);
            webViewPreview.TabIndex = 0;
            webViewPreview.ZoomFactor = 1D;
            // 
            // txtOcr
            // 
            txtOcr.Dock = DockStyle.Fill;
            txtOcr.Location = new Point(0, 0);
            txtOcr.Multiline = true;
            txtOcr.Name = "txtOcr";
            txtOcr.Size = new Size(348, 350);
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
        private TextBox textBox1;
        private ComboBox comboBox4;
        private ComboBox comboBox3;
        private ComboBox comboBox2;
        private SplitContainer splitContainer3;
        private WebView2 webViewPreview;
        private TextBox txtOcr;
        private Panel pnlControl;
    }
}
