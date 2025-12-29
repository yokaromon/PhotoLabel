namespace PhotoLabel
{
    partial class FrmPicture
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
            components = new System.ComponentModel.Container();
            imageList1 = new ImageList(components);
            picPreview = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
            SuspendLayout();
            // 
            // imageList1
            // 
            imageList1.ColorDepth = ColorDepth.Depth32Bit;
            imageList1.ImageSize = new Size(16, 16);
            imageList1.TransparentColor = Color.Transparent;
            // 
            // picPreview
            //
            picPreview.Dock = DockStyle.Fill;
            picPreview.Location = new Point(0, 0);
            picPreview.Name = "picPreview";
            picPreview.Size = new Size(1051, 690);
            picPreview.SizeMode = PictureBoxSizeMode.Zoom;
            picPreview.TabIndex = 0;
            picPreview.TabStop = false;
            // 
            // FrmPicture
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1051, 690);
            Controls.Add(picPreview);
            Name = "FrmPicture";
            Text = "FrmPicture";
            ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private ImageList imageList1;
        private PictureBox picPreview;
    }
}