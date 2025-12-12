using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace PhotoLabel
{
    /// <summary>
    /// Thumbnail card for the image list (thumb, name, modified date, size, checkbox).
    /// </summary>
    public class ThumbnailCard : UserControl
    {
        private readonly PictureBox _pic;
        private readonly Label _lblName;
        private readonly Label _lblDate;
        private readonly Label _lblSize;
        private readonly CheckBox _chk;

        public string FilePath { get; }
        public CheckBox SelectionCheckBox => _chk;

        public ThumbnailCard(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            DoubleBuffered = true;
            Margin = new Padding(10);
            Size = new Size(520, 150);
            BackColor = SystemColors.Window;
            BorderStyle = BorderStyle.FixedSingle;

            _chk = new CheckBox
            {
                Dock = DockStyle.Fill,
                Width = 20,
                Height = 20,
                Tag = filePath,
                Margin = new Padding(8, 8, 4, 4)
            };

            _pic = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = SystemColors.ControlLight
            };

            _lblName = CreateLabel(FontStyle.Bold);
            _lblDate = CreateLabel(FontStyle.Regular);
            _lblSize = CreateLabel(FontStyle.Regular);

            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(4, 6, 8, 6)
            };
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
            rightLayout.Controls.Add(_lblName, 0, 0);
            rightLayout.Controls.Add(_lblDate, 0, 1);
            rightLayout.Controls.Add(_lblSize, 0, 2);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(6),
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 32));   // checkbox
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 128));  // thumb
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));   // text

            mainLayout.Controls.Add(_chk, 0, 0);
            mainLayout.Controls.Add(_pic, 1, 0);
            mainLayout.Controls.Add(rightLayout, 2, 0);

            Controls.Add(mainLayout);

            LoadMetadata(filePath);
        }

        private Label CreateLabel(FontStyle style)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                Font = new Font(Font, style)
            };
        }

        private void LoadMetadata(string filePath)
        {
            _lblName.Text = Path.GetFileName(filePath);
            _lblDate.Text = $"Modified: {File.GetLastWriteTime(filePath):yyyy/MM/dd HH:mm:ss}";
            _lblSize.Text = GetImageSizeText(filePath);

            try
            {
                var thumbPath = GetOrCreateThumbnail(filePath);
                if (File.Exists(thumbPath))
                {
                    using var fs = new FileStream(thumbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    _pic.Image = Image.FromStream(fs);
                }
            }
            catch
            {
                // Ignore thumbnail errors; leave blank.
            }
        }

        private static string GetImageSizeText(string filePath)
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var img = Image.FromStream(fs, useEmbeddedColorManagement: false, validateImageData: false);
                return $"Size: {img.Width}x{img.Height}";
            }
            catch
            {
                return "Size: (unavailable)";
            }
        }

        private static string GetOrCreateThumbnail(string filePath, int width = 120, int height = 120)
        {
            var cacheDir = Path.Combine(Path.GetTempPath(), "PhotoLabel", "Thumbnails");
            Directory.CreateDirectory(cacheDir);

            var thumbName = $"thumb_{ComputePathHash(filePath)}_{width}x{height}.jpg";
            var thumbPath = Path.Combine(cacheDir, thumbName);

            var sourceTime = File.GetLastWriteTimeUtc(filePath);
            var needRegen = !File.Exists(thumbPath) || File.GetLastWriteTimeUtc(thumbPath) < sourceTime;

            if (needRegen)
            {
                GenerateThumbnail(filePath, thumbPath, width, height);
            }

            return thumbPath;
        }

        private static void GenerateThumbnail(string sourcePath, string thumbPath, int width, int height)
        {
            using var fs = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var img = Image.FromStream(fs, useEmbeddedColorManagement: false, validateImageData: false);

            var ratio = Math.Min((double)width / img.Width, (double)height / img.Height);
            var w = Math.Max(1, (int)Math.Round(img.Width * ratio));
            var h = Math.Max(1, (int)Math.Round(img.Height * ratio));

            using var bmp = new Bitmap(w, h);
            using var g = Graphics.FromImage(bmp);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.Clear(Color.Transparent);
            g.DrawImage(img, 0, 0, w, h);

            bmp.Save(thumbPath, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        private static string ComputePathHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash)[..16];
        }
    }
}
