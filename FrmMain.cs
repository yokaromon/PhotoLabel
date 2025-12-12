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
                    var card = new ThumbnailCard(filePath);
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
    }
}
