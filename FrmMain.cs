using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using PhotoLabel.Ocr;

namespace PhotoLabel
{
    public partial class FrmMain : Form
    {
        private const string ConfigFileName = "Config.ini";
        private const string TargetDirKey = "TargetDir";
        private string _currentPreviewPath = string.Empty;
        private ThumbnailCard? _selectedCard;
        private OcrService? _ocrService;
        private const int WIDTH = 800;
        private Task? _webViewInitTask;

        public FrmMain()
        {
            InitializeComponent();
            Load += FrmMain_Load;
            treDir.BeforeExpand += TreDir_BeforeExpand;
            treDir.AfterSelect += TreDir_AfterSelect;
            splitContainer2.SizeChanged += (_, _) => AdjustPaneWidthToCard();
            Resize += (_, _) => AdjustPaneWidthToCard();
            flowThumbs.ControlAdded += (_, _) => AdjustPaneWidthToCard();
            flowThumbs.ControlRemoved += (_, _) => AdjustPaneWidthToCard();
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

                InitializeOcrServices(configPath);
                LoadItemsFromConfig(configPath);
                if (cmbItems.Items.Count > 0)
                {
                    cmbItems.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load directory tree.{Environment.NewLine}{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadItemsFromConfig(string configPath)
        {
            try
            {
                var dict = new Tools.ParameterDict(configPath);
                LoadItemList(dict);
                LoadItemCombos(dict);
            }
            catch
            {
                // ignore load errors
            }
        }

        private void LoadItemList(Tools.ParameterDict dict)
        {
            var keys = dict.GetKeyArray("Items");
            if (keys == null || keys.Length == 0)
            {
                return;
            }

            cmbItems.Items.Clear();
            foreach (var key in keys)
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    cmbItems.Items.Add(key);
                }
            }
        }

        private void LoadItemCombos(Tools.ParameterDict dict)
        {
            PopulateComboFromItems(Item2, "Item2", dict);
            PopulateComboFromItems(Item3, "Item3", dict);
            PopulateComboFromItems(Item4, "Item4", dict);
        }

        private static void PopulateComboFromItems(ComboBox combo, string itemKey, Tools.ParameterDict dict)
        {
            var values = dict.GetValueArray("Items", itemKey);
            combo.Items.Clear();
            if (values == null)
            {
                return;
            }

            foreach (var value in values)
            {
                var trimmed = (value ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    combo.Items.Add(trimmed);
                }
            }

            if (combo.Items.Count > 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private void UpdateItemPreview()
        {
            var key = cmbItems.SelectedItem as string ?? cmbItems.Text;
            if (string.IsNullOrWhiteSpace(key))
            {
                txtItems.Text = string.Empty;
                return;
            }
            try
            {
                var dict = new Tools.ParameterDict(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName));
                txtItems.Text = dict.GetValue("Items", key, string.Empty) ?? string.Empty;
            }
            catch
            {
                txtItems.Text = string.Empty;
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
                    WireCardEvents(card, filePath);
                    flowThumbs.Controls.Add(card);
                }
                AdjustPaneWidthToCard();
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

        private void WireCardEvents(ThumbnailCard card, string filePath)
        {
            card.Click += (_, _) => SelectAndPreview(card, filePath);
            card.SelectionCheckBox.Click += (_, _) => SelectAndPreview(card, filePath);
            card.SelectionCheckBox.CheckedChanged += (_, _) => { _ = RunOcrAsync(filePath); };
        }

        private void SelectAndPreview(ThumbnailCard card, string filePath)
        {
            if (_selectedCard != null && _selectedCard != card)
            {
                _selectedCard.SetSelected(false);
            }

            _selectedCard = card;
            _selectedCard.SetSelected(true);
            _ = ShowPreviewAsync(filePath);
        }

        private int GetCardWidth()
        {
            var card = flowThumbs.Controls.OfType<ThumbnailCard>().FirstOrDefault();
            return card?.Width ?? WIDTH;
        }

        private void AdjustPaneWidthToCard()
        {
            try
            {
                var cardWidth = GetCardWidth();
                var padding = flowThumbs.Padding.Horizontal;
                var scrollbar = SystemInformation.VerticalScrollBarWidth;
                var margin = 24;
                var desired = cardWidth + padding + scrollbar + margin;
                var minRight = 200;
                var total = splitContainer2.Width;
                var splitter = Math.Min(desired, Math.Max(100, total - minRight));
                splitContainer2.SplitterDistance = splitter;

                foreach (var card in flowThumbs.Controls.OfType<ThumbnailCard>())
                {
                    card.Width = cardWidth;
                }
            }
            catch
            {
                // ignore layout errors
            }
        }

        private async Task EnsureWebViewAsync()
        {
            if (webViewPreview.CoreWebView2 != null)
            {
                return;
            }

            _webViewInitTask ??= webViewPreview.EnsureCoreWebView2Async();
            await _webViewInitTask;
        }

        private async Task ShowPreviewAsync(string filePath)
        {
            _currentPreviewPath = filePath;
            try
            {
                await EnsureWebViewAsync();
                var uri = new Uri(filePath);
                webViewPreview.Source = uri; // navigate directly to file:// image
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load preview.{Environment.NewLine}{ex.Message}", "Preview Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            _ = RunOcrAsync(filePath);
        }

        private async Task RunOcrAsync(string filePath)
        {
            try
            {
                if (_ocrService == null)
                {
                    txtOcr.Text = "OCR service not configured.";
                    return;
                }

                txtOcr.Text = "Running OCR...";
                var result = await _ocrService.ExtractTextAsync(filePath);

                if (result.ExtractedTexts.Count == 0)
                {
                    txtOcr.Text = "No text detected.";
                    return;
                }

                var cacheNote = result.FromCache ? " (cache)" : string.Empty;
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Detected {result.ExtractedTexts.Count} items{cacheNote}");
                foreach (var item in result.ExtractedTexts)
                {
                    sb.AppendLine(item.Text);
                }
                txtOcr.Text = sb.ToString();
            }
            catch (Exception ex)
            {
                txtOcr.Text = $"OCR failed: {ex.Message}";
            }
        }

        private void InitializeOcrServices(string configPath)
        {
            try
            {
                var ini = File.ReadAllLines(configPath);
                string? visionUrl = null;
                string? apiKey = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_API_KEY");
                string? replacePath = null;

                foreach (var line in ini)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#") || trimmed.StartsWith(";")) continue;
                    var parts = trimmed.Split('=', 2);
                    if (parts.Length != 2) continue;
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    if (key.Equals("VisionApiUrl", StringComparison.OrdinalIgnoreCase))
                    {
                        visionUrl = value;
                    }
                    if (key.Equals("ReplaceRulesPath", StringComparison.OrdinalIgnoreCase))
                    {
                        replacePath = value;
                    }
                }

                visionUrl ??= "https://vision.googleapis.com/v1/images:annotate";
                replacePath ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ReplaceRules.dat");

                var rules = new ReplaceRuleStore(replacePath).Load();
                var replace = new ReplaceService(rules);
                var cache = new OcrCacheService("PhotoLabel");
                var client = new GoogleVisionClient(visionUrl, apiKey);
                _ocrService = new OcrService(client, cache, replace);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize OCR service.{Environment.NewLine}{ex.Message}", "OCR Init Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cmbItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateItemPreview();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var key = cmbItems.SelectedItem as string ?? cmbItems.Text;
            if (string.IsNullOrWhiteSpace(key))
            {
                MessageBox.Show("保存する項目を選択してください。", "Save", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
            try
            {
                var itemValues = new Dictionary<string, string>
                {
                    { key, txtItems.Text ?? string.Empty }
                };
                Tools.ParameterDict.SaveValues("Items", itemValues, configPath);

                if (!string.IsNullOrWhiteSpace(txtTargetDir.Text))
                {
                    var targetDirValue = new Dictionary<string, string>
                    {
                        { TargetDirKey, txtTargetDir.Text }
                    };
                    Tools.ParameterDict.SaveValues("Config", targetDirValue, configPath);
                }

                MessageBox.Show("保存しました。", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存に失敗しました。{Environment.NewLine}{ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
