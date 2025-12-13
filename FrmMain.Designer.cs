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
            splitContainer1 = new SplitContainer();
            treDir = new TreeView();
            splitContainer2 = new SplitContainer();
            flowThumbs = new FlowLayoutPanel();
            pnlAction = new Panel();
            comboBox2 = new ComboBox();
            splitContainer3 = new SplitContainer();
            webViewPreview = new WebView2();
            txtOcr = new TextBox();
            pnlControl = new Panel();
            comboBox3 = new ComboBox();
            comboBox4 = new ComboBox();
            textBox1 = new TextBox();
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
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 198);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(treDir);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(1765, 745);
            splitContainer1.SplitterDistance = 460;
            splitContainer1.TabIndex = 0;
            // 
            // treDir
            // 
            treDir.Dock = DockStyle.Fill;
            treDir.Location = new Point(0, 0);
            treDir.Name = "treDir";
            treDir.Size = new Size(460, 745);
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
            splitContainer2.Size = new Size(1301, 745);
            splitContainer2.SplitterDistance = 946;
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
            flowThumbs.Size = new Size(946, 618);
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
            pnlAction.Size = new Size(946, 127);
            pnlAction.TabIndex = 0;
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
            splitContainer3.Size = new Size(351, 745);
            splitContainer3.SplitterDistance = 361;
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
            webViewPreview.Size = new Size(351, 361);
            webViewPreview.TabIndex = 0;
            webViewPreview.ZoomFactor = 1D;
            // 
            // txtOcr
            // 
            txtOcr.Dock = DockStyle.Fill;
            txtOcr.Location = new Point(0, 0);
            txtOcr.Multiline = true;
            txtOcr.Name = "txtOcr";
            txtOcr.Size = new Size(351, 380);
            txtOcr.TabIndex = 0;
            // 
            // pnlControl
            // 
            pnlControl.Dock = DockStyle.Top;
            pnlControl.Location = new Point(0, 0);
            pnlControl.Name = "pnlControl";
            pnlControl.Size = new Size(1765, 198);
            pnlControl.TabIndex = 1;
            // 
            // comboBox3
            // 
            comboBox3.FormattingEnabled = true;
            comboBox3.Location = new Point(69, 78);
            comboBox3.Name = "comboBox3";
            comboBox3.Size = new Size(324, 40);
            comboBox3.TabIndex = 2;
            // 
            // comboBox4
            // 
            comboBox4.FormattingEnabled = true;
            comboBox4.Location = new Point(433, 78);
            comboBox4.Name = "comboBox4";
            comboBox4.Size = new Size(324, 40);
            comboBox4.TabIndex = 3;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(66, 19);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(327, 39);
            textBox1.TabIndex = 4;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1765, 943);
            Controls.Add(splitContainer1);
            Controls.Add(pnlControl);
            Name = "FrmMain";
            Text = "Form1";
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

        private SplitContainer splitContainer1;
        private TreeView treDir;
        private SplitContainer splitContainer2;
        private FlowLayoutPanel flowThumbs;
        private SplitContainer splitContainer3;
        private Panel pnlControl;
        private WebView2 webViewPreview;
        private TextBox txtOcr;
        private Panel pnlAction;
        private ComboBox comboBox2;
        private TextBox textBox1;
        private ComboBox comboBox4;
        private ComboBox comboBox3;
    }
}
