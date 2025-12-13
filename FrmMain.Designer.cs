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
            splitContainer3 = new SplitContainer();
            picFullSize = new PictureBox();
            txtOcr = new TextBox();
            pnlControl = new Panel();
            pnlAction = new Panel();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picFullSize).BeginInit();
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
            splitContainer1.Size = new Size(1151, 555);
            splitContainer1.SplitterDistance = 300;
            splitContainer1.TabIndex = 0;
            // 
            // treDir
            // 
            treDir.Dock = DockStyle.Fill;
            treDir.Location = new Point(0, 0);
            treDir.Name = "treDir";
            treDir.Size = new Size(300, 555);
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
            splitContainer2.Size = new Size(847, 555);
            splitContainer2.SplitterDistance = 616;
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
            flowThumbs.Size = new Size(616, 428);
            flowThumbs.TabIndex = 0;
            flowThumbs.WrapContents = false;
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
            splitContainer3.Panel1.Controls.Add(picFullSize);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(txtOcr);
            splitContainer3.Size = new Size(227, 555);
            splitContainer3.SplitterDistance = 269;
            splitContainer3.TabIndex = 0;
            // 
            // picFullSize
            // 
            picFullSize.BorderStyle = BorderStyle.FixedSingle;
            picFullSize.Dock = DockStyle.Fill;
            picFullSize.Location = new Point(0, 0);
            picFullSize.Name = "picFullSize";
            picFullSize.Size = new Size(227, 269);
            picFullSize.SizeMode = PictureBoxSizeMode.Zoom;
            picFullSize.TabIndex = 0;
            picFullSize.TabStop = false;
            // 
            // txtOcr
            // 
            txtOcr.Dock = DockStyle.Fill;
            txtOcr.Location = new Point(0, 0);
            txtOcr.Multiline = true;
            txtOcr.Name = "txtOcr";
            txtOcr.Size = new Size(227, 282);
            txtOcr.TabIndex = 0;
            // 
            // pnlControl
            // 
            pnlControl.Dock = DockStyle.Top;
            pnlControl.Location = new Point(0, 0);
            pnlControl.Name = "pnlControl";
            pnlControl.Size = new Size(1151, 198);
            pnlControl.TabIndex = 1;
            // 
            // pnlAction
            // 
            pnlAction.Dock = DockStyle.Top;
            pnlAction.Location = new Point(0, 0);
            pnlAction.Name = "pnlAction";
            pnlAction.Size = new Size(616, 127);
            pnlAction.TabIndex = 0;
            // 
            // FrmMain
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1151, 753);
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
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picFullSize).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private TreeView treDir;
        private SplitContainer splitContainer2;
        private FlowLayoutPanel flowThumbs;
        private SplitContainer splitContainer3;
        private Panel pnlControl;
        private PictureBox picFullSize;
        private TextBox txtOcr;
        private Panel pnlAction;
    }
}
