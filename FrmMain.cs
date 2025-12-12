using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace PhotoLabel
{
    public partial class FrmMain : Form
    {
        private const string ConfigFileName = "Config.ini";
        private const string TargetDirKey = "TargetDir";

        public FrmMain()
        {
            InitializeComponent();
            Load += FrmMain_Load;
            treDir.BeforeExpand += TreDir_BeforeExpand;
            treDir.AfterSelect += TreDir_AfterSelect;
        }

        private void FrmMain_Load(object? sender, EventArgs e)
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

                if (!File.Exists(configPath))
                {
                    MessageBox.Show($"Config file not found: {configPath}", "Config Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var targetDir = ReadTargetDir(configPath);
                if (string.IsNullOrWhiteSpace(targetDir))
                {
                    MessageBox.Show("TargetDir is missing in Config.ini.", "Config Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!Directory.Exists(targetDir))
                {
                    MessageBox.Show($"Target directory not found: {targetDir}", "Config Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                PopulateTree(targetDir);
                LoadImagesForDirectory(targetDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load directory tree.{Environment.NewLine}{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string? ReadTargetDir(string configPath)
        {
            foreach (var line in File.ReadLines(configPath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith(";"))
                {
                    continue;
                }

                if (trimmed.StartsWith($"{TargetDirKey}=", StringComparison.OrdinalIgnoreCase))
                {
                    return trimmed.Substring(TargetDirKey.Length + 1).Trim();
                }
            }

            return null;
        }

        private void PopulateTree(string rootPath)
        {
            treDir.Nodes.Clear();
            var rootNode = CreateDirectoryNode(rootPath);
            treDir.Nodes.Add(rootNode);
            LoadChildDirectories(rootNode);
        }

        private void TreDir_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
        {
            LoadChildDirectories(e.Node);
        }

        private void TreDir_AfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is string path && Directory.Exists(path))
            {
                LoadImagesForDirectory(path);
            }
        }

        private void LoadChildDirectories(TreeNode node)
        {
            if (node.Tag is not string path || !Directory.Exists(path))
            {
                return;
            }

            if (node.Nodes.Count == 1 && node.Nodes[0].Tag == null)
            {
                node.Nodes.Clear();
            }

            if (node.Nodes.Count > 0)
            {
                return;
            }

            try
            {
                foreach (var directory in Directory.GetDirectories(path))
                {
                    var child = CreateDirectoryNode(directory);
                    node.Nodes.Add(child);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read directory: {path}{Environment.NewLine}{ex.Message}", "Directory Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static TreeNode CreateDirectoryNode(string path)
        {
            var directoryName = new DirectoryInfo(path).Name;
            if (string.IsNullOrEmpty(directoryName))
            {
                directoryName = path;
            }

            var node = new TreeNode(directoryName)
            {
                Tag = path
            };

            node.Nodes.Add(new TreeNode());
            return node;
        }

        private void LoadImagesForDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            {
                flowThumbs.Controls.Clear();
                return;
            }

            var extensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff", ".heic", ".heif" };
            string[] files;

            try
            {
                files = Directory
                    .GetFiles(directoryPath)
                    .Where(f => extensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read image list.{Environment.NewLine}{ex.Message}", "Read Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            flowThumbs.SuspendLayout();
            try
            {
                flowThumbs.Controls.Clear();

                foreach (var filePath in files)
                {
                    var card = CreateThumbnailCard(filePath);
                    flowThumbs.Controls.Add(card);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to render thumbnails.{Environment.NewLine}{ex.Message}", "Render Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                flowThumbs.ResumeLayout();
            }
        }

        private Control CreateThumbnailCard(string filePath)
        {
            var panel = new Panel
            {
                Width = 380,
                Height = 110,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(6),
                BackColor = SystemColors.Window
            };

            var chk = new CheckBox
            {
                Location = new Point(8, 8),
                Width = 18,
                Height = 18,
                Tag = filePath
            };

            var pic = new PictureBox
            {
                Location = new Point(30, 8),
                Size = new Size(96, 96),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = SystemColors.ControlLight
            };

            var lblName = new Label
            {
                Location = new Point(140, 12),
                AutoSize = false,
                Width = 220,
                Height = 20,
                Text = Path.GetFileName(filePath),
                Font = new Font(Font, FontStyle.Bold),
                AutoEllipsis = true
            };

            var lblDate = new Label
            {
                Location = new Point(140, 40),
                AutoSize = false,
                Width = 220,
                Height = 18,
                Text = $"更新: {File.GetLastWriteTime(filePath):yyyy/MM/dd HH:mm:ss}",
                AutoEllipsis = true
            };

            var lblSize = new Label
            {
                Location = new Point(140, 64),
                AutoSize = false,
                Width = 220,
                Height = 18,
                Text = GetImageSizeText(filePath),
                AutoEllipsis = true
            };

            try
            {
                var thumbPath = GetOrCreateThumbnail(filePath);
                if (File.Exists(thumbPath))
                {
                    using var fs = new FileStream(thumbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    pic.Image = Image.FromStream(fs);
                }
            }
            catch
            {
                // Fallback to blank when thumbnail generation fails
            }

            panel.Controls.Add(chk);
            panel.Controls.Add(pic);
            panel.Controls.Add(lblName);
            panel.Controls.Add(lblDate);
            panel.Controls.Add(lblSize);
            return panel;
        }

        private static string GetImageSizeText(string filePath)
        {
            try
            {
                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var img = Image.FromStream(fs, useEmbeddedColorManagement: false, validateImageData: false);
                return $"サイズ: {img.Width}x{img.Height}";
            }
            catch
            {
                return "サイズ: 取得不可";
            }
        }

        private static string GetOrCreateThumbnail(string filePath, int width = 96, int height = 96)
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
