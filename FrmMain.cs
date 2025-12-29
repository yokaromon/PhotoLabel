using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private const string InternalDragFormat = "PhotoLabel.InternalDrag";
        private string _currentPreviewPath = string.Empty;
        private readonly HashSet<ThumbnailCard> _selectedCards = new();
        private ThumbnailCard? _lastSelectedCard;
        private OcrService? _ocrService;
        private const int WIDTH = 800;
        private Task? _webViewInitTask;
        private bool _suppressBulkOcr;
        private string _currentDirectory = string.Empty;
        private const string DefaultRenamePattern = "{Item1}_{Item2}_{Item3}_{Item4}{Ext}";
        private readonly Dictionary<string, ItemSnapshot> _itemSnapshots = new();
        private string _treeRootPath = string.Empty;
        private readonly List<string> _allDirectories = new List<string>();
        private bool _isDragging;
        private Point _dragStartPoint;
        private Point _dragCurrentPoint;
        private bool _isCardDragPending;
        private Point _cardDragStartScreenPoint;
        private ThumbnailCard? _cardDragSource;
        private readonly System.Windows.Forms.Timer _treeFilterTimer = new System.Windows.Forms.Timer();
        private string _pendingTreeFilter = string.Empty;
        private bool _isTreeFiltered;
        private enum SortOrder { Date, Name }
        private SortOrder _currentSortOrder = SortOrder.Date;
        private FrmPicture? _pictureForm;

        public FrmMain()
        {
            InitializeComponent();
            Load += FrmMain_Load;
            FormClosing += FrmMain_FormClosing;
            treDir.BeforeExpand += TreDir_BeforeExpand;
            treDir.AfterSelect += TreDir_AfterSelect;
            splitContainer2.SizeChanged += (_, _) => AdjustPaneWidthToCard();
            Resize += (_, _) => AdjustPaneWidthToCard();
            flowThumbs.ControlAdded += (_, _) => AdjustPaneWidthToCard();
            flowThumbs.ControlRemoved += (_, _) => AdjustPaneWidthToCard();
            flowThumbs.AllowDrop = true;
            flowThumbs.DragEnter += FlowThumbs_DragEnter;
            flowThumbs.DragDrop += FlowThumbs_DragDrop;
            flowThumbs.MouseDown += FlowThumbs_MouseDown;
            flowThumbs.MouseMove += FlowThumbs_MouseMove;
            flowThumbs.MouseUp += FlowThumbs_MouseUp;
            flowThumbs.Paint += FlowThumbs_Paint;
            _treeFilterTimer.Interval = 300;
            _treeFilterTimer.Tick += TreeFilterTimer_Tick;
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

                txtTargetDir.Text = targetDir;

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

                if (cmbSort.Items.Count > 0)
                {
                    cmbSort.SelectedIndex = 0;
                }

                // ウィンドウとスプリッタの位置を復元
                LoadWindowSettings(configPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load directory tree.{Environment.NewLine}{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmMain_FormClosing(object? sender, FormClosingEventArgs e)
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
                SaveWindowSettings(configPath);
            }
            catch
            {
                // 設定保存エラーは無視
            }
        }

        private void LoadWindowSettings(string configPath)
        {
            try
            {
                var dict = new Tools.ParameterDict(configPath);

                // FrmMainの位置とサイズ
                int x = int.TryParse(dict.GetValue("Window", "MainX", null), out var mx) ? mx : -1;
                int y = int.TryParse(dict.GetValue("Window", "MainY", null), out var my) ? my : -1;
                int width = int.TryParse(dict.GetValue("Window", "MainWidth", null), out var mw) ? mw : -1;
                int height = int.TryParse(dict.GetValue("Window", "MainHeight", null), out var mh) ? mh : -1;

                bool hasPosition = x >= 0 && y >= 0;
                bool hasSize = width > 0 && height > 0;

                if (hasPosition && hasSize)
                {
                    StartPosition = FormStartPosition.Manual;
                    Location = new Point(x, y);
                    Size = new Size(width, height);
                }

                // スプリッタの位置
                int split1 = int.TryParse(dict.GetValue("Window", "Split1", null), out var s1) ? s1 : -1;
                int split2 = int.TryParse(dict.GetValue("Window", "Split2", null), out var s2) ? s2 : -1;
                int split3 = int.TryParse(dict.GetValue("Window", "Split3", null), out var s3) ? s3 : -1;

                if (split1 > 0 && split1 < splitContainer1.Width - splitContainer1.Panel2MinSize)
                {
                    splitContainer1.SplitterDistance = split1;
                }

                if (split2 > 0 && split2 < splitContainer2.Width - splitContainer2.Panel2MinSize)
                {
                    splitContainer2.SplitterDistance = split2;
                }

                if (split3 > 0 && split3 < splitContainer3.Height - splitContainer3.Panel2MinSize)
                {
                    splitContainer3.SplitterDistance = split3;
                }
            }
            catch
            {
                // 設定読み込みエラーは無視
            }
        }

        private void SaveWindowSettings(string configPath)
        {
            try
            {
                var settings = new Dictionary<string, string>
                {
                    { "MainX", Location.X.ToString() },
                    { "MainY", Location.Y.ToString() },
                    { "MainWidth", Width.ToString() },
                    { "MainHeight", Height.ToString() },
                    { "Split1", splitContainer1.SplitterDistance.ToString() },
                    { "Split2", splitContainer2.SplitterDistance.ToString() },
                    { "Split3", splitContainer3.SplitterDistance.ToString() }
                };

                Tools.ParameterDict.SaveValues("Window", settings, configPath);
            }
            catch
            {
                // 設定保存エラーは無視
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
            combo.Tag = null;
            if (values == null)
            {
                return;
            }

            var entries = new List<ComboPatternEntry>();

            foreach (var value in values)
            {
                var trimmed = (value ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    var parts = trimmed.Split('=', 2);
                    var pattern = parts[0];
                    var replacement = parts.Length == 2 ? parts[1] : null;
                    var allowDisplay = replacement == null || !replacement.Contains("$");
                    var display = replacement ?? pattern;

                    entries.Add(new ComboPatternEntry(pattern, replacement, display, allowDisplay));
                    if (allowDisplay)
                    {
                        combo.Items.Add(display);
                    }
                }
            }

            combo.Tag = entries;
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
            // 現在の展開状態と選択状態を保存
            HashSet<string> expandedPaths = SaveExpandedNodePaths();
            string? selectedPath = treDir.SelectedNode?.Tag as string;

            treDir.Nodes.Clear();
            _treeRootPath = rootPath ?? string.Empty;
            _isTreeFiltered = false;
            _pendingTreeFilter = string.Empty;

            bool hasRoot = !string.IsNullOrWhiteSpace(_treeRootPath) && Directory.Exists(_treeRootPath);
            if (!hasRoot)
            {
                _allDirectories.Clear();
                return;
            }

            TreeNode rootNode = CreateDirectoryNode(_treeRootPath);
            treDir.Nodes.Add(rootNode);
            LoadChildDirectories(rootNode);
            CacheDirectoryList(_treeRootPath);

            // 展開状態と選択状態を復元
            RestoreExpandedNodePaths(expandedPaths);
            RestoreSelectedNode(selectedPath);
        }

        private void CacheDirectoryList(string rootPath)
        {
            _allDirectories.Clear();
            bool hasRoot = !string.IsNullOrWhiteSpace(rootPath) && Directory.Exists(rootPath);
            if (!hasRoot)
            {
                return;
            }

            // ルート配下のディレクトリ情報をあらかじめキャッシュしてフィルタ速度を確保
            _allDirectories.Add(rootPath);
            try
            {
                foreach (string directory in Directory.EnumerateDirectories(rootPath, "*", SearchOption.AllDirectories))
                {
                    _allDirectories.Add(directory);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"フォルダ一覧の取得に失敗しました。{Environment.NewLine}{ex.Message}", "Tree Filter", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void TreDir_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
        {
            if (_isTreeFiltered)
            {
                return;
            }

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

        private static TreeNode CreateDirectoryNode(string path, bool includePlaceholder = true)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            string directoryName = info.Name;
            if (string.IsNullOrEmpty(directoryName))
            {
                directoryName = path;
            }

            TreeNode node = new TreeNode(directoryName)
            {
                Tag = path
            };

            if (includePlaceholder)
            {
                node.Nodes.Add(new TreeNode());
            }

            return node;
        }

        private void ApplyTreeFilter(string keyword)
        {
            bool hasRoot = !string.IsNullOrWhiteSpace(_treeRootPath) && Directory.Exists(_treeRootPath);
            if (!hasRoot)
            {
                return;
            }

            string baseKeyword = keyword ?? string.Empty;
            string trimmedKeyword = baseKeyword.Trim();
            bool hasKeyword = trimmedKeyword.Length > 0;

            treDir.BeginUpdate();
            try
            {
                treDir.Nodes.Clear();
                TreeNode rootNode = CreateDirectoryNode(_treeRootPath, false);
                treDir.Nodes.Add(rootNode);

                List<string> matchedDirectories = new List<string>();
                foreach (string directory in _allDirectories)
                {
                    bool shouldInclude = !hasKeyword || directory.IndexOf(trimmedKeyword, StringComparison.OrdinalIgnoreCase) >= 0;
                    if (shouldInclude)
                    {
                        matchedDirectories.Add(directory);
                    }
                }

                if (matchedDirectories.Count == 0)
                {
                    _isTreeFiltered = hasKeyword;
                    return;
                }

                matchedDirectories.Sort(StringComparer.OrdinalIgnoreCase);
                foreach (string directory in matchedDirectories)
                {
                    AddPathNodes(rootNode, directory);
                }

                rootNode.Expand();
                rootNode.ExpandAll();
                _isTreeFiltered = hasKeyword;
            }
            finally
            {
                treDir.EndUpdate();
            }
        }

        private void AddPathNodes(TreeNode rootNode, string fullPath)
        {
            bool hasRoot = !string.IsNullOrWhiteSpace(_treeRootPath);
            if (!hasRoot)
            {
                return;
            }

            string relativePath = Path.GetRelativePath(_treeRootPath, fullPath);
            bool isRootPath = string.IsNullOrWhiteSpace(relativePath) || relativePath == ".";
            if (isRootPath)
            {
                return;
            }

            char[] separators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            string[] segments = relativePath.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            TreeNode currentNode = rootNode;
            string currentPath = _treeRootPath;

            foreach (string segment in segments)
            {
                currentPath = Path.Combine(currentPath, segment);
                currentNode = FindOrCreateChildNode(currentNode, currentPath);
            }

            currentNode.EnsureVisible();
        }

        private TreeNode FindOrCreateChildNode(TreeNode parent, string path)
        {
            foreach (TreeNode child in parent.Nodes)
            {
                bool samePath = child.Tag is string childPath && string.Equals(childPath, path, StringComparison.OrdinalIgnoreCase);
                if (samePath)
                {
                    return child;
                }
            }

            TreeNode node = CreateDirectoryNode(path, false);
            parent.Nodes.Add(node);
            return node;
        }

        private void LoadImagesForDirectory(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath) || !Directory.Exists(directoryPath))
            {
                flowThumbs.Controls.Clear();
                _currentDirectory = string.Empty;
                return;
            }

            _currentDirectory = directoryPath;
            var extensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff", ".heic", ".heif" };
            string[] files;

            try
            {
                var query = Directory
                    .GetFiles(directoryPath)
                    .Where(f => extensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

                files = _currentSortOrder switch
                {
                    SortOrder.Name => query.OrderBy(f => f, StringComparer.OrdinalIgnoreCase).ToArray(),
                    SortOrder.Date => query.OrderBy(f => File.GetLastWriteTime(f)).ToArray(),
                    _ => query.ToArray()
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read image list.{Environment.NewLine}{ex.Message}", "Read Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            flowThumbs.SuspendLayout();
            try
            {
                ClearAllSelections();
                _lastSelectedCard = null;
                flowThumbs.Controls.Clear();

                string? previousGroup = null;
                bool useAlternateColor = false;
                Color defaultColor = SystemColors.Window;
                Color alternateColor = Color.FromArgb(245, 250, 255);

                foreach (var filePath in files)
                {
                    string currentGroup = _currentSortOrder switch
                    {
                        SortOrder.Name => GetFileNamePrefix(filePath),
                        SortOrder.Date => GetFileDate(filePath).ToString("yyyy-MM-dd"),
                        _ => string.Empty
                    };

                    bool groupChanged = previousGroup != null && previousGroup != currentGroup;
                    if (groupChanged)
                    {
                        useAlternateColor = !useAlternateColor;
                    }

                    var card = new ThumbnailCard(filePath);
                    Color backgroundColor = useAlternateColor ? alternateColor : defaultColor;
                    card.SetGroupBackgroundColor(backgroundColor);
                    WireCardEvents(card);
                    flowThumbs.Controls.Add(card);

                    previousGroup = currentGroup;
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

        private void WireCardEvents(ThumbnailCard card)
        {
            card.Click += (_, _) => HandleCardClick(card, card.FilePath);
            card.ImageDoubleClick += (sender, _) =>
            {
                if (sender is ThumbnailCard c)
                {
                    HandleCardDoubleClick(c.FilePath);
                }
            };
            card.SelectionCheckBox.Click += (_, _) => SelectAndPreview(card, card.FilePath);
            card.SelectionCheckBox.CheckedChanged += (_, _) =>
            {
                if (_suppressBulkOcr)
                {
                    return;
                }

                string path = card.FilePath;
                _ = RunOcrAsync(path);
            };
            card.RenameRequested += Card_RenameRequested;
            card.MouseDown += Card_MouseDown;
            card.MouseMove += Card_MouseMove;
            card.MouseUp += Card_MouseUp;
        }

        private void Card_RenameRequested(object? sender, ThumbnailRenameEventArgs e)
        {
            if (sender is not ThumbnailCard card)
            {
                return;
            }

            string sourcePath = card.FilePath;
            bool hasSource = !string.IsNullOrWhiteSpace(sourcePath);
            if (!hasSource)
            {
                return;
            }

            string rawName = e.ProposedName ?? string.Empty;
            string trimmedName = rawName.Trim();
            bool hasName = !string.IsNullOrWhiteSpace(trimmedName);
            if (!hasName)
            {
                MessageBox.Show("ファイル名が空です。", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sanitized = SanitizeFileName(trimmedName);
            string extension = Path.GetExtension(sourcePath);
            bool hasExtension = Path.HasExtension(sanitized);
            if (!hasExtension && !string.IsNullOrEmpty(extension))
            {
                sanitized += extension;
            }

            bool emptyAfterSanitize = string.IsNullOrWhiteSpace(sanitized);
            if (emptyAfterSanitize)
            {
                MessageBox.Show("有効なファイル名を入力してください。", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string? directory = Path.GetDirectoryName(sourcePath);
            bool hasDirectory = !string.IsNullOrWhiteSpace(directory);
            if (!hasDirectory)
            {
                MessageBox.Show("フォルダが存在しません。", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string safeDirectory = directory ?? string.Empty;
            string destinationPath = Path.Combine(safeDirectory, sanitized);
            bool samePath = string.Equals(sourcePath, destinationPath, StringComparison.OrdinalIgnoreCase);
            if (samePath)
            {
                e.Success = true;
                return;
            }

            bool destinationExists = File.Exists(destinationPath);
            if (destinationExists)
            {
                MessageBox.Show("同名のファイルが既に存在します。", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                File.Move(sourcePath, destinationPath);
                card.UpdateFilePath(destinationPath);
                MoveSnapshot(sourcePath, destinationPath);
                bool previewSelected = string.Equals(_currentPreviewPath, sourcePath, StringComparison.OrdinalIgnoreCase);
                if (previewSelected)
                {
                    _currentPreviewPath = destinationPath;
                    try
                    {
                        webViewPreview.Source = new Uri(destinationPath);
                    }
                    catch
                    {
                        // ignore preview update errors
                    }
                }

                e.Success = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"リネームに失敗しました。{Environment.NewLine}{ex.Message}", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectAndPreview(ThumbnailCard card, string filePath)
        {
            bool isAlreadySelected = _selectedCards.Contains(card);
            if (!isAlreadySelected)
            {
                ClearAllSelections();
                AddToSelection(card);
            }

            _lastSelectedCard = card;
            _ = ShowPreviewAsync(filePath);
        }

        private void AddToSelection(ThumbnailCard card)
        {
            bool wasAdded = _selectedCards.Add(card);
            if (wasAdded)
            {
                card.SetSelected(true);
            }
        }

        private void RemoveFromSelection(ThumbnailCard card)
        {
            bool wasRemoved = _selectedCards.Remove(card);
            if (wasRemoved)
            {
                card.SetSelected(false);
            }
        }

        private void ToggleSelection(ThumbnailCard card)
        {
            bool isCurrentlySelected = _selectedCards.Contains(card);
            if (isCurrentlySelected)
            {
                RemoveFromSelection(card);
                bool noCardsLeft = _selectedCards.Count == 0;
                if (noCardsLeft)
                {
                    _lastSelectedCard = null;
                }
            }
            else
            {
                AddToSelection(card);
            }
        }

        private void ClearAllSelections()
        {
            foreach (var card in _selectedCards.ToList())
            {
                card.SetSelected(false);
            }
            _selectedCards.Clear();
        }

        private int GetCardIndex(ThumbnailCard card)
        {
            var controls = flowThumbs.Controls;
            for (int i = 0; i < controls.Count; i++)
            {
                bool isMatch = controls[i] == card;
                if (isMatch)
                {
                    return i;
                }
            }
            return -1;
        }

        private void SelectRange(ThumbnailCard startCard, ThumbnailCard endCard)
        {
            int startIndex = GetCardIndex(startCard);
            int endIndex = GetCardIndex(endCard);

            bool invalidIndices = startIndex == -1 || endIndex == -1;
            if (invalidIndices)
            {
                return;
            }

            int minIndex = Math.Min(startIndex, endIndex);
            int maxIndex = Math.Max(startIndex, endIndex);

            bool ctrlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
            if (!ctrlPressed)
            {
                ClearAllSelections();
            }

            for (int i = minIndex; i <= maxIndex; i++)
            {
                if (flowThumbs.Controls[i] is ThumbnailCard card)
                {
                    AddToSelection(card);
                }
            }
        }

        private void HandleCardClick(ThumbnailCard card, string filePath)
        {
            bool ctrlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
            bool shiftPressed = (Control.ModifierKeys & Keys.Shift) == Keys.Shift;

            if (shiftPressed && _lastSelectedCard != null)
            {
                SelectRange(_lastSelectedCard, card);
            }
            else if (ctrlPressed)
            {
                ToggleSelection(card);
            }
            else
            {
                ClearAllSelections();
                AddToSelection(card);
            }

            _lastSelectedCard = card;
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

            // FrmPictureが開いている場合は画像を更新
            UpdatePictureForm(filePath);
        }

        private void HandleCardDoubleClick(string filePath)
        {
            bool hasPath = !string.IsNullOrWhiteSpace(filePath);
            if (!hasPath)
            {
                return;
            }

            bool fileExists = File.Exists(filePath);
            if (!fileExists)
            {
                return;
            }

            // FrmPictureが既に開いている場合
            bool formIsOpen = _pictureForm != null && !_pictureForm.IsDisposed;
            if (formIsOpen && _pictureForm != null)
            {
                _pictureForm.ShowImage(filePath);
                _pictureForm.BringToFront();
                _pictureForm.Activate();
                return;
            }

            // 新しくFrmPictureを開く
            _pictureForm = new FrmPicture();
            _pictureForm.Owner = this;
            _pictureForm.FormClosed += (_, _) => _pictureForm = null;
            _pictureForm.ShowImage(filePath);
            _pictureForm.Show();
            _pictureForm.BringToFront();
        }

        private void UpdatePictureForm(string filePath)
        {
            bool formIsOpen = _pictureForm != null && !_pictureForm.IsDisposed;
            if (!formIsOpen)
            {
                return;
            }

            bool hasPath = !string.IsNullOrWhiteSpace(filePath);
            if (!hasPath)
            {
                return;
            }

            bool fileExists = File.Exists(filePath);
            if (!fileExists)
            {
                return;
            }

            _pictureForm?.ShowImage(filePath);
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

                ApplyOcrMatches(filePath, result.ExtractedTexts);
            }
            catch (Exception ex)
            {
                txtOcr.Text = $"OCR failed: {ex.Message}";
            }
        }

        private async Task<bool> EnsureSnapshotAsync(string filePath)
        {
            bool hasPath = !string.IsNullOrWhiteSpace(filePath);
            if (!hasPath)
            {
                return false;
            }

            bool alreadyCached = _itemSnapshots.ContainsKey(filePath);
            if (alreadyCached)
            {
                return true;
            }

            bool hasOcrService = _ocrService != null;
            if (hasOcrService)
            {
                await RunOcrAsync(filePath);
                bool cachedByOcr = _itemSnapshots.ContainsKey(filePath);
                if (cachedByOcr)
                {
                    return true;
                }
            }

            ThumbnailCard? selectedCard = _lastSelectedCard;
            bool hasSelectedCard = selectedCard != null;
            if (!hasSelectedCard)
            {
                return false;
            }

            bool sameFile = string.Equals(selectedCard.FilePath, filePath, StringComparison.OrdinalIgnoreCase);
            if (!sameFile)
            {
                return false;
            }

            SaveItemSnapshot(filePath, Item1.Text, Item2.Text, Item3.Text, Item4.Text);
            bool cachedAfterManual = _itemSnapshots.ContainsKey(filePath);
            return cachedAfterManual;
        }

        private void ApplyOcrMatches(string filePath, List<OcrResult> texts)
        {
            if (texts == null || texts.Count == 0)
            {
                return;
            }

            var joined = string.Join("\n", texts.Select(t => t.Text).Where(t => !string.IsNullOrWhiteSpace(t)));
            if (string.IsNullOrWhiteSpace(joined))
            {
                return;
            }

            ApplyItem1Match(joined);
            ApplyComboMatch(Item2, joined);
            ApplyComboMatch(Item3, joined);
            ApplyComboMatch(Item4, joined);

            SaveItemSnapshot(filePath, Item1.Text, Item2.Text, Item3.Text, Item4.Text);
        }

        private void ApplyItem1Match(string text)
        {
            Item1.Text = string.Empty;
            try
            {
                var dict = new Tools.ParameterDict(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName));
                var raw = dict.GetValue("Items", "Item1", string.Empty) ?? string.Empty;
                if (string.IsNullOrWhiteSpace(raw))
                {
                    return;
                }

                var parts = raw.Split('=', 2);
                var pattern = parts[0];
                var replacement = parts.Length > 1 ? parts[1] : "$0";
                if (string.IsNullOrWhiteSpace(pattern))
                {
                    return;
                }

                var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                var match = regex.Match(text);
                if (!match.Success)
                {
                    return;
                }

                var value = match.Result(replacement);
                Item1.Text = value;
            }
            catch
            {
                // ignore invalid regex or read errors
            }
        }

        private static void ApplyComboMatch(ComboBox combo, string text)
        {
            combo.SelectedIndex = -1;
            combo.Text = string.Empty;
            var entries = combo.Tag as List<ComboPatternEntry>;
            if (entries == null || entries.Count == 0 || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            foreach (var entry in entries)
            {
                var pattern = entry.Pattern;
                var replacement = entry.Replacement;
                if (string.IsNullOrWhiteSpace(pattern))
                {
                    continue;
                }

                bool matched = false;
                string? display = null;

                if (!string.IsNullOrWhiteSpace(pattern))
                {
                    try
                    {
                        var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                        var match = regex.Match(text);
                        if (match.Success)
                        {
                            matched = true;
                            display = replacement != null ? match.Result(replacement) : match.Value;
                        }
                    }
                    catch
                    {
                        // ignore invalid regex and fallback to substring match
                    }

                    if (!matched && text.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matched = true;
                        display = replacement ?? pattern;
                    }
                }

                if (matched)
                {
                    if (replacement == null && entry.AllowDisplay)
                    {
                        combo.SelectedItem = entry.Display;
                    }
                    else
                    {
                        combo.Text = display ?? replacement ?? string.Empty;
                    }
                    return;
                }
            }
        }

        private record ComboPatternEntry(string Pattern, string? Replacement, string Display, bool AllowDisplay);

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

                // reload combos and preview after save
                LoadItemsFromConfig(configPath);
                UpdateItemPreview();

                MessageBox.Show("保存しました。", "Save", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存に失敗しました。{Environment.NewLine}{ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnRename_Click(object sender, EventArgs e)
        {
            var targets = flowThumbs.Controls.OfType<ThumbnailCard>()
                .Where(c => c.SelectionCheckBox.Checked)
                .ToList();

            if (targets.Count == 0)
            {
                MessageBox.Show("チェックされているカードがありません。", "Rename", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
            var pattern = DefaultRenamePattern;
            try
            {
                var dict = new Tools.ParameterDict(configPath);
                var configured = dict.GetValue("Config", "Rename", pattern) ?? pattern;
                if (!string.IsNullOrWhiteSpace(configured))
                {
                    pattern = configured;
                }
            }
            catch
            {
                // use default pattern
            }

            var renamed = 0;
            var errors = new List<string>();
            var renamedPaths = new List<string>();

            foreach (var card in targets)
            {
                try
                {
                    var src = card.FilePath;
                    if (!_itemSnapshots.ContainsKey(src))
                    {
                        var ok = await EnsureSnapshotAsync(src);
                        if (!ok || !_itemSnapshots.ContainsKey(src))
                        {
                            errors.Add($"{Path.GetFileName(src)}: OCR情報がありません。");
                            continue;
                        }
                    }

                    var ext = Path.GetExtension(src);
                    var map = GetItemMapForFile(src);
                    var rawName = BuildRename(pattern, map, ext);
                    if (string.IsNullOrWhiteSpace(rawName))
                    {
                        errors.Add(Path.GetFileName(src));
                        continue;
                    }

                    var safeName = SanitizeFileName(rawName);
                    if (!Path.HasExtension(safeName) && !string.IsNullOrEmpty(ext))
                    {
                        safeName += ext;
                    }

                    var destDir = Path.GetDirectoryName(src) ?? string.Empty;
                    var destPath = GetUniquePath(destDir, safeName);

                    File.Move(src, destPath);
                    card.UpdateFilePath(destPath);
                    MoveSnapshot(src, destPath);
                    if (_currentPreviewPath == src)
                    {
                        _currentPreviewPath = destPath;
                        try
                        {
                            webViewPreview.Source = new Uri(destPath);
                        }
                        catch
                        {
                            // ignore preview update errors
                        }
                    }
                    renamed++;
                    renamedPaths.Add(destPath);
                }
                catch (Exception ex)
                {
                    errors.Add($"{Path.GetFileName(card.FilePath)}: {ex.Message}");
                }
            }

            if (renamedPaths.Count > 0 && Directory.Exists(_currentDirectory))
            {
                try
                {
                    _suppressBulkOcr = true;
                    LoadImagesForDirectory(_currentDirectory);

                    var set = new HashSet<string>(renamedPaths, StringComparer.OrdinalIgnoreCase);
                    ThumbnailCard? first = null;
                    foreach (var card in flowThumbs.Controls.OfType<ThumbnailCard>())
                    {
                        if (set.Contains(card.FilePath))
                        {
                            card.SelectionCheckBox.Checked = true;
                            first ??= card;
                        }
                    }

                    if (first != null)
                    {
                        ClearAllSelections();
                        AddToSelection(first);
                        _lastSelectedCard = first;
                        _ = ShowPreviewAsync(first.FilePath);
                    }
                }
                finally
                {
                    _suppressBulkOcr = false;
                }
            }

            var message = $"リネーム完了: {renamed} 件";
            if (errors.Count > 0)
            {
                message += $"\n失敗: {errors.Count} 件\n" + string.Join("\n", errors.Take(10));
            }
            MessageBox.Show(message, "Rename", MessageBoxButtons.OK, errors.Count == 0 ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void cbxSelect_CheckedChanged(object sender, EventArgs e)
        {
            var checkAll = cbxSelect.Checked;
            bool hasMultipleSelection = _selectedCards.Count > 1;

            try
            {
                _suppressBulkOcr = true;

                if (hasMultipleSelection)
                {
                    foreach (var card in _selectedCards)
                    {
                        var chk = card.SelectionCheckBox;
                        if (chk.Checked != checkAll)
                        {
                            chk.Checked = checkAll;
                        }
                    }
                }
                else
                {
                    foreach (var card in flowThumbs.Controls.OfType<ThumbnailCard>())
                    {
                        var chk = card.SelectionCheckBox;
                        if (chk.Checked != checkAll)
                        {
                            chk.Checked = checkAll;
                        }
                    }
                }
            }
            finally
            {
                _suppressBulkOcr = false;
            }
        }

        private static string BuildRename(string pattern, Dictionary<string, string> map, string extension)
        {
            var result = string.IsNullOrWhiteSpace(pattern) ? DefaultRenamePattern : pattern;
            foreach (var kv in map)
            {
                result = result.Replace($"{{{kv.Key}}}", kv.Value ?? string.Empty);
            }

            extension ??= string.Empty;
            result = result.Replace("{Ext}", extension);
            result = result.Replace(".*", extension);
            return result.Trim();
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var chars = name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray();
            return new string(chars).Trim();
        }

        private static string GetUniquePath(string directory, string fileName)
        {
            var dir = string.IsNullOrWhiteSpace(directory) ? Directory.GetCurrentDirectory() : directory;
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var ext = Path.GetExtension(fileName);
            var candidate = Path.Combine(dir, $"{baseName}{ext}");
            var index = 1;
            while (File.Exists(candidate))
            {
                candidate = Path.Combine(dir, $"{baseName}_{index}{ext}");
                index++;
            }
            return candidate;
        }

        private void MoveSnapshot(string oldPath, string newPath)
        {
            if (string.IsNullOrWhiteSpace(oldPath) || string.IsNullOrWhiteSpace(newPath))
            {
                return;
            }

            if (_itemSnapshots.TryGetValue(oldPath, out var snap))
            {
                _itemSnapshots.Remove(oldPath);
                _itemSnapshots[newPath] = snap;
            }
        }

        private static string ReplacePlaceholders(string template, Dictionary<string, string> values)
        {
            var result = template ?? string.Empty;

            foreach (var kvp in values)
            {
                var key = kvp.Key;
                var value = kvp.Value ?? string.Empty;
                var pattern = $@"\{{{Regex.Escape(key)}(?::(\d+)-(\d+))?\}}";
                var regex = new Regex(pattern);

                result = regex.Replace(result, match =>
                {
                    if (!match.Groups[1].Success)
                    {
                        return value;
                    }

                    var start = int.Parse(match.Groups[1].Value);
                    var len = int.Parse(match.Groups[2].Value);
                    return ExtractSubstring(value, start, len);
                });
            }

            result = Regex.Replace(result, @"\{[^}]+\}", string.Empty);
            return result;
        }

        private static string ExtractSubstring(string value, int start, int length)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new string('-', length);
            }

            if (start >= value.Length)
            {
                return new string('-', length);
            }

            var available = Math.Min(length, value.Length - start);
            var slice = value.Substring(start, available);
            if (available < length)
            {
                slice += new string('-', length - available);
            }
            return slice;
        }

        private static string NormalizePathTemplate(string pathTemplate)
        {
            if (string.IsNullOrWhiteSpace(pathTemplate))
            {
                return string.Empty;
            }

            var separator = Path.DirectorySeparatorChar;
            var alt = separator == '/' ? '\\' : '/';
            return pathTemplate.Replace(alt, separator);
        }

        private record ItemSnapshot(string Item1, string Item2, string Item3, string Item4);

        private void FlowThumbs_DragEnter(object? sender, DragEventArgs e)
        {
            // 内部ドラッグの場合は拒否
            bool isInternalDrag = e.Data != null && e.Data.GetDataPresent(InternalDragFormat);
            if (isInternalDrag)
            {
                e.Effect = DragDropEffects.None;
                return;
            }

            // 外部からのファイルドロップは受け入れ
            bool hasFileDrop = e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop);
            if (hasFileDrop)
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void FlowThumbs_DragDrop(object? sender, DragEventArgs e)
        {
            // 内部ドラッグの場合は何もしない
            bool isInternalDrag = e.Data != null && e.Data.GetDataPresent(InternalDragFormat);
            if (isInternalDrag)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_currentDirectory) || !Directory.Exists(_currentDirectory))
            {
                MessageBox.Show("フォルダが選択されていません。", "Drag & Drop", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (e.Data?.GetData(DataFormats.FileDrop) is not string[] dropped || dropped.Length == 0)
            {
                return;
            }

            var extensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tif", ".tiff", ".heic", ".heif" };
            var copied = 0;
            var errors = new List<string>();

            foreach (var path in dropped)
            {
                try
                {
                    var ext = Path.GetExtension(path);
                    if (string.IsNullOrWhiteSpace(ext) || !extensions.Any(x => ext.Equals(x, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue; // skip non-image
                    }

                    // 同じディレクトリにあるファイルは無視（同一ファイルのコピーを防ぐ）
                    string? sourceDir = Path.GetDirectoryName(path);
                    bool isSameDirectory = !string.IsNullOrWhiteSpace(sourceDir) &&
                                          string.Equals(sourceDir, _currentDirectory, StringComparison.OrdinalIgnoreCase);
                    if (isSameDirectory)
                    {
                        continue;
                    }

                    var destName = Path.GetFileName(path);
                    var destPath = GetUniquePath(_currentDirectory, destName);
                    File.Copy(path, destPath, overwrite: false);
                    copied++;
                }
                catch (Exception ex)
                {
                    errors.Add($"{Path.GetFileName(path)}: {ex.Message}");
                }
            }

            if (copied > 0)
            {
                LoadImagesForDirectory(_currentDirectory);
            }

            if (errors.Count > 0)
            {
                MessageBox.Show($"一部コピーに失敗しました。\n{string.Join("\n", errors.Take(10))}", "Drag & Drop", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private Dictionary<string, string> GetItemMapForFile(string filePath)
        {
            if (_itemSnapshots.TryGetValue(filePath, out var snap))
            {
                return new Dictionary<string, string>
                {
                    { "Item1", snap.Item1 },
                    { "Item2", snap.Item2 },
                    { "Item3", snap.Item3 },
                    { "Item4", snap.Item4 }
                };
            }

            throw new InvalidOperationException("OCR情報が未取得のためリネームできません。");
        }

        private void SaveItemSnapshot(string filePath, string item1, string item2, string item3, string item4)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            _itemSnapshots[filePath] = new ItemSnapshot(
                item1 ?? string.Empty,
                item2 ?? string.Empty,
                item3 ?? string.Empty,
                item4 ?? string.Empty);
        }

        private async void btnMove_Click(object sender, EventArgs e)
        {
            // チェック処理
            var validationResult = ValidateMoveOperation();
            if (!validationResult.IsValid)
            {
                return;
            }

            // 移動処理
            var moveResult = await ExecuteFileMoves(validationResult.Targets, validationResult.MovePattern, validationResult.TargetRoot);

            // 移動終了後の処理
            RefreshAfterMove();
            ShowMoveResult(moveResult.MovedCount, moveResult.Errors);
        }

        private (bool IsValid, List<ThumbnailCard> Targets, string MovePattern, string TargetRoot) ValidateMoveOperation()
        {
            var targets = flowThumbs.Controls.OfType<ThumbnailCard>()
                .Where(c => c.SelectionCheckBox.Checked)
                .ToList();

            var hasTargets = targets.Count > 0;
            if (!hasTargets)
            {
                MessageBox.Show("移動するファイルが選択されていません。", "Move", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return (false, null, null, null);
            }

            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
            string movePattern;
            string targetRoot = string.Empty;
            try
            {
                var dict = new Tools.ParameterDict(configPath);
                movePattern = dict.GetValue("Config", "MoveDir", string.Empty) ?? string.Empty;
                targetRoot = ReadTargetDir(configPath) ?? string.Empty;
            }
            catch
            {
                movePattern = string.Empty;
            }

            var hasMovePattern = !string.IsNullOrWhiteSpace(movePattern);
            if (!hasMovePattern)
            {
                MessageBox.Show("Config.ini の MoveDir が設定されていません。", "Move", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return (false, null, null, null);
            }

            return (true, targets, movePattern, targetRoot);
        }

        private async Task<(int MovedCount, List<string> Errors)> ExecuteFileMoves(List<ThumbnailCard> targets, string movePattern, string targetRoot)
        {
            var today = DateTime.Now.ToString("yyyyMMdd");
            var moved = 0;
            var errors = new List<string>();

            foreach (var card in targets)
            {
                try
                {
                    var src = card.FilePath;
                    if (!_itemSnapshots.ContainsKey(src))
                    {
                        var ok = await EnsureSnapshotAsync(src);
                        if (!ok || !_itemSnapshots.ContainsKey(src))
                        {
                            errors.Add($"{Path.GetFileName(src)}: OCR情報がありません。");
                            continue;
                        }
                    }

                    var map = GetItemMapForFile(src);
                    map["TargetDir"] = targetRoot;
                    map["日付"] = today;
                    map["FileName"] = Path.GetFileName(src);

                    var replaced = ReplacePlaceholders(movePattern, map);
                    var hasReplacedPath = !string.IsNullOrWhiteSpace(replaced);
                    if (!hasReplacedPath)
                    {
                        errors.Add($"{Path.GetFileName(src)}: MoveDir テンプレートが空です。");
                        continue;
                    }

                    var normalized = NormalizePathTemplate(replaced);
                    var destDir = Path.GetDirectoryName(normalized);
                    var destName = Path.GetFileName(normalized);

                    var hasDestDir = !string.IsNullOrWhiteSpace(destDir);
                    if (!hasDestDir)
                    {
                        destDir = Path.GetDirectoryName(src) ?? string.Empty;
                    }

                    var hasDestName = !string.IsNullOrWhiteSpace(destName);
                    if (!hasDestName)
                    {
                        destName = Path.GetFileName(src);
                    }

                    Directory.CreateDirectory(destDir);
                    var destPath = GetUniquePath(destDir, destName);
                    File.Move(src, destPath);
                    card.UpdateFilePath(destPath);
                    MoveSnapshot(src, destPath);

                    var isCurrentPreview = _currentPreviewPath == src;
                    if (isCurrentPreview)
                    {
                        _currentPreviewPath = destPath;
                        try
                        {
                            webViewPreview.Source = new Uri(destPath);
                        }
                        catch
                        {
                        }
                    }

                    moved++;
                }
                catch (Exception ex)
                {
                    errors.Add($"{Path.GetFileName(card.FilePath)}: {ex.Message}");
                }
            }

            return (moved, errors);
        }

        private void RefreshAfterMove()
        {
            var directoryExists = Directory.Exists(_currentDirectory);
            if (!directoryExists)
            {
                return;
            }

            try
            {
                _suppressBulkOcr = true;
                ClearAllSelections();
                _lastSelectedCard = null;
                LoadImagesForDirectory(_currentDirectory);

                bool hasRoot = !string.IsNullOrWhiteSpace(_treeRootPath) && Directory.Exists(_treeRootPath);
                if (hasRoot)
                {
                    PopulateTree(_treeRootPath);
                }
            }
            finally
            {
                _suppressBulkOcr = false;
            }
        }

        private static void ShowMoveResult(int movedCount, List<string> errors)
        {
            var message = $"移動完了: {movedCount} 件";
            var hasErrors = errors.Count > 0;
            if (hasErrors)
            {
                message += $"\n失敗: {errors.Count} 件\n" + string.Join("\n", errors.Take(10));
            }

            MessageBox.Show(message, "Move", MessageBoxButtons.OK, hasErrors ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        private void FlowThumbs_MouseDown(object? sender, MouseEventArgs e)
        {
            bool isLeftButton = e.Button == MouseButtons.Left;
            if (!isLeftButton)
            {
                return;
            }

            Control? controlAtPoint = flowThumbs.GetChildAtPoint(e.Location);
            ThumbnailCard? cardAtPoint = controlAtPoint as ThumbnailCard;
            bool clickedOnCard = cardAtPoint != null;
            if (clickedOnCard)
            {
                Point screenPoint = flowThumbs.PointToScreen(e.Location);
                HandleCardMouseDown(cardAtPoint!, screenPoint);
                return;
            }

            _isDragging = true;
            _dragStartPoint = e.Location;
            _dragCurrentPoint = e.Location;
        }

        private void Card_MouseDown(object? sender, MouseEventArgs e)
        {
            bool isLeftButton = e.Button == MouseButtons.Left;
            if (!isLeftButton)
            {
                return;
            }

            ThumbnailCard? card = sender as ThumbnailCard;
            if (card == null)
            {
                return;
            }

            Point screenPoint = Cursor.Position;
            HandleCardMouseDown(card, screenPoint);
        }

        private void Card_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_isCardDragPending)
            {
                return;
            }

            Point currentScreenPoint = Cursor.Position;
            TryStartCardDrag(currentScreenPoint);
        }

        private void Card_MouseUp(object? sender, MouseEventArgs e)
        {
            bool isLeftButton = e.Button == MouseButtons.Left;
            if (!isLeftButton)
            {
                return;
            }

            if (_isCardDragPending)
            {
                ThumbnailCard? card = sender as ThumbnailCard;
                bool wasClick = card != null && !HasExceededDragThreshold(_cardDragStartScreenPoint, Cursor.Position);
                if (wasClick && card != null)
                {
                    _ = ShowPreviewAsync(card.FilePath);
                }

                ResetCardDragState();
            }
        }

        private void HandleCardMouseDown(ThumbnailCard card, Point screenPoint)
        {
            // カードをドラッグ対象にする前に選択状態を整える
            bool alreadySelected = _selectedCards.Contains(card);
            if (!alreadySelected)
            {
                bool ctrlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
                if (!ctrlPressed)
                {
                    ClearAllSelections();
                }

                AddToSelection(card);
            }

            _lastSelectedCard = card;
            _isCardDragPending = true;
            _cardDragStartScreenPoint = screenPoint;
            _cardDragSource = card;
            if (_cardDragSource != null)
            {
                _cardDragSource.Capture = true;
            }
        }

        private void FlowThumbs_MouseMove(object? sender, MouseEventArgs e)
        {
            if (!_isDragging)
            {
                return;
            }

            _dragCurrentPoint = e.Location;
            flowThumbs.Invalidate();
        }

        private void FlowThumbs_MouseUp(object? sender, MouseEventArgs e)
        {
            if (_isCardDragPending)
            {
                ResetCardDragState();
            }

            if (!_isDragging)
            {
                return;
            }

            _isDragging = false;
            flowThumbs.Invalidate();

            Rectangle selectionRect = GetNormalizedRectangle(_dragStartPoint, _dragCurrentPoint);
            bool hasArea = selectionRect.Width > 5 && selectionRect.Height > 5;
            if (!hasArea)
            {
                return;
            }

            bool ctrlPressed = (Control.ModifierKeys & Keys.Control) == Keys.Control;
            if (!ctrlPressed)
            {
                ClearAllSelections();
            }

            foreach (var card in flowThumbs.Controls.OfType<ThumbnailCard>())
            {
                Rectangle cardBounds = card.Bounds;
                bool intersects = selectionRect.IntersectsWith(cardBounds);
                if (intersects)
                {
                    AddToSelection(card);
                }
            }

            bool hasSelection = _selectedCards.Count > 0;
            if (hasSelection)
            {
                _lastSelectedCard = _selectedCards.Last();
            }
        }

        private void FlowThumbs_Paint(object? sender, PaintEventArgs e)
        {
            if (!_isDragging)
            {
                return;
            }

            Rectangle selectionRect = GetNormalizedRectangle(_dragStartPoint, _dragCurrentPoint);
            using var pen = new Pen(Color.DodgerBlue, 2);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            e.Graphics.DrawRectangle(pen, selectionRect);

            using var brush = new SolidBrush(Color.FromArgb(50, Color.DodgerBlue));
            e.Graphics.FillRectangle(brush, selectionRect);
        }

        private void TryStartCardDrag(Point currentScreenPoint)
        {
            // ドラッグ距離を監視し、閾値を超えたら外部アプリへのD&Dを実施
            bool pendingDrag = _isCardDragPending;
            if (!pendingDrag)
            {
                return;
            }

            bool hasSource = _cardDragSource != null;
            if (!hasSource)
            {
                ResetCardDragState();
                return;
            }

            bool leftPressed = (Control.MouseButtons & MouseButtons.Left) == MouseButtons.Left;
            if (!leftPressed)
            {
                ResetCardDragState();
                return;
            }

            bool movedEnough = HasExceededDragThreshold(_cardDragStartScreenPoint, currentScreenPoint);
            if (!movedEnough)
            {
                return;
            }

            List<string> selectedFiles = GetCheckedFilePathsInDisplayOrder();
            bool hasFiles = selectedFiles.Count > 0;
            if (!hasFiles)
            {
                ResetCardDragState();
                return;
            }

            BeginExternalCardDrag(selectedFiles);
        }

        private void BeginExternalCardDrag(List<string> filePaths)
        {
            // 選択ファイルのパスをFileDrop形式で外部アプリに渡す
            DataObject dataObject = new DataObject();
            StringCollection dropList = new StringCollection();
            string[] fileArray = filePaths.ToArray();
            dropList.AddRange(fileArray);
            dataObject.SetFileDropList(dropList);
            // 内部ドラッグであることを示すマーカーを追加
            dataObject.SetData(InternalDragFormat, true);

            DragDropEffects allowedEffects = DragDropEffects.Copy | DragDropEffects.Move;
            DragDropEffects dragResult = flowThumbs.DoDragDrop(dataObject, allowedEffects);
            HandleExternalDragCompleted(dragResult, filePaths);
            ResetCardDragState();
        }

        private void HandleExternalDragCompleted(DragDropEffects dragResult, List<string> draggedFiles)
        {
            // 移動として完了した場合は一覧を最新化する
            bool moveRequested = (dragResult & DragDropEffects.Move) == DragDropEffects.Move;
            if (!moveRequested)
            {
                return;
            }

            bool requiresRefresh = false;
            foreach (string filePath in draggedFiles)
            {
                bool fileStillExists = File.Exists(filePath);
                if (fileStillExists)
                {
                    continue;
                }

                requiresRefresh = true;
                break;
            }

            if (!requiresRefresh)
            {
                return;
            }

            RefreshAfterMove();
        }

        private List<string> GetCheckedFilePathsInDisplayOrder()
        {
            // FlowLayoutPanel上の表示順序でチェック済ファイルを収集
            List<string> filePaths = new List<string>();
            foreach (Control control in flowThumbs.Controls)
            {
                ThumbnailCard? card = control as ThumbnailCard;
                bool isCard = card != null;
                if (!isCard)
                {
                    continue;
                }

                bool isChecked = card.SelectionCheckBox.Checked;
                if (!isChecked)
                {
                    continue;
                }

                bool hasPath = !string.IsNullOrWhiteSpace(card.FilePath);
                if (!hasPath)
                {
                    continue;
                }

                filePaths.Add(card.FilePath);
            }

            bool hasCheckedFiles = filePaths.Count > 0;
            if (hasCheckedFiles)
            {
                return filePaths;
            }

            ThumbnailCard? dragSource = _cardDragSource;
            bool hasDragSource = dragSource != null && !string.IsNullOrWhiteSpace(dragSource.FilePath);
            if (hasDragSource && dragSource != null)
            {
                List<string> fallbackList = new List<string>
                {
                    dragSource.FilePath
                };
                return fallbackList;
            }

            return filePaths;
        }

        private static bool HasExceededDragThreshold(Point startPoint, Point currentPoint)
        {
            // Windows標準のドラッグ閾値を利用してドラッグ開始を判定
            int deltaX = Math.Abs(currentPoint.X - startPoint.X);
            int deltaY = Math.Abs(currentPoint.Y - startPoint.Y);
            Size dragSize = SystemInformation.DragSize;
            bool exceedX = deltaX >= dragSize.Width;
            if (exceedX)
            {
                return true;
            }

            bool exceedY = deltaY >= dragSize.Height;
            return exceedY;
        }

        private void ResetCardDragState()
        {
            // カードドラッグ用の一時状態を初期化
            _isCardDragPending = false;
            if (_cardDragSource != null && _cardDragSource.Capture)
            {
                _cardDragSource.Capture = false;
            }
            _cardDragSource = null;
        }

        private static Rectangle GetNormalizedRectangle(Point start, Point end)
        {
            int x = Math.Min(start.X, end.X);
            int y = Math.Min(start.Y, end.Y);
            int width = Math.Abs(end.X - start.X);
            int height = Math.Abs(end.Y - start.Y);
            return new Rectangle(x, y, width, height);
        }

        private void cmbSort_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int selectedIndex = cmbSort.SelectedIndex;
            _currentSortOrder = selectedIndex switch
            {
                0 => SortOrder.Date,
                1 => SortOrder.Name,
                _ => SortOrder.Date
            };

            bool hasDirectory = !string.IsNullOrWhiteSpace(_currentDirectory);
            if (hasDirectory)
            {
                LoadImagesForDirectory(_currentDirectory);
            }
        }

        private static string GetFileNamePrefix(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            int underscoreIndex = fileName.IndexOf('_');
            bool hasUnderscore = underscoreIndex >= 0;
            if (hasUnderscore)
            {
                return fileName.Substring(0, underscoreIndex);
            }
            return fileName;
        }

        private static DateTime GetFileDate(string filePath)
        {
            return File.GetLastWriteTime(filePath).Date;
        }

        private void txtFind_TextChanged(object sender, EventArgs e)
        {
            _pendingTreeFilter = txtFind.Text ?? string.Empty;
            _treeFilterTimer.Stop();
            _treeFilterTimer.Start();
        }

        private void TreeFilterTimer_Tick(object? sender, EventArgs e)
        {
            _treeFilterTimer.Stop();
            ApplyTreeFilter(_pendingTreeFilter);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // 削除対象のカードを抽出
            List<ThumbnailCard> targets = flowThumbs.Controls.OfType<ThumbnailCard>()
                .Where(card => card.SelectionCheckBox.Checked)
                .ToList();
            bool hasTargets = targets.Count > 0;
            if (!hasTargets)
            {
                MessageBox.Show("削除するファイルが選択されていません。", "削除", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 削除確認ダイアログ
            string message = $"選択された {targets.Count} 件のファイルを削除します。よろしいですか？";
            DialogResult confirmResult = MessageBox.Show(message, "削除確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            bool canceled = confirmResult != DialogResult.Yes;
            if (canceled)
            {
                return;
            }

            // ファイル削除処理
            int deletedCount = 0;
            List<string> errors = new List<string>();
            foreach (ThumbnailCard card in targets)
            {
                string filePath = card.FilePath;
                bool hasPath = !string.IsNullOrWhiteSpace(filePath);
                if (!hasPath)
                {
                    continue;
                }

                try
                {
                    bool exists = File.Exists(filePath);
                    if (exists)
                    {
                        File.Delete(filePath);
                    }

                    deletedCount++;
                    _itemSnapshots.Remove(filePath);
                    _selectedCards.Remove(card);
                    flowThumbs.Controls.Remove(card);

                    bool previewMatches = string.Equals(_currentPreviewPath, filePath, StringComparison.OrdinalIgnoreCase);
                    if (previewMatches)
                    {
                        _currentPreviewPath = string.Empty;
                        _lastSelectedCard = null;
                        try
                        {
                            webViewPreview.Source = new Uri("about:blank");
                        }
                        catch
                        {
                            // プレビューリセット失敗時は無視
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"{Path.GetFileName(filePath)}: {ex.Message}");
                }
            }

            bool needRefresh = deletedCount > 0;
            bool directoryDeleted = false;
            if (needRefresh && !string.IsNullOrWhiteSpace(_currentDirectory))
            {
                LoadImagesForDirectory(_currentDirectory);

                // ディレクトリが空になったかチェック
                bool directoryStillExists = Directory.Exists(_currentDirectory);
                if (directoryStillExists)
                {
                    bool isEmpty = IsDirectoryEmpty(_currentDirectory);
                    if (isEmpty)
                    {
                        try
                        {
                            Directory.Delete(_currentDirectory);
                            directoryDeleted = true;

                            // 親ディレクトリに移動
                            string parentDir = Path.GetDirectoryName(_currentDirectory) ?? string.Empty;
                            bool hasParent = !string.IsNullOrWhiteSpace(parentDir) && Directory.Exists(parentDir);
                            if (hasParent)
                            {
                                LoadImagesForDirectory(parentDir);
                                _currentDirectory = parentDir;
                            }
                            else
                            {
                                _currentDirectory = string.Empty;
                            }

                            // ツリーを更新
                            bool hasRoot = !string.IsNullOrWhiteSpace(_treeRootPath) && Directory.Exists(_treeRootPath);
                            if (hasRoot)
                            {
                                PopulateTree(_treeRootPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"ディレクトリ削除失敗: {ex.Message}");
                        }
                    }
                }
            }

            // 結果を通知
            string resultMessage = $"削除完了: {deletedCount} 件";
            if (directoryDeleted)
            {
                resultMessage += "\nディレクトリも削除しました。";
            }

            bool hasErrors = errors.Count > 0;
            if (hasErrors)
            {
                resultMessage += $"\n失敗: {errors.Count} 件\n" + string.Join("\n", errors.Take(10));
            }

            MessageBoxIcon icon = hasErrors ? MessageBoxIcon.Warning : MessageBoxIcon.Information;
            MessageBox.Show(resultMessage, "削除", MessageBoxButtons.OK, icon);
        }

        private static bool IsDirectoryEmpty(string directoryPath)
        {
            bool hasPath = !string.IsNullOrWhiteSpace(directoryPath);
            if (!hasPath)
            {
                return false;
            }

            bool exists = Directory.Exists(directoryPath);
            if (!exists)
            {
                return false;
            }

            bool hasFiles = Directory.GetFiles(directoryPath).Length > 0;
            if (hasFiles)
            {
                return false;
            }

            bool hasDirectories = Directory.GetDirectories(directoryPath).Length > 0;
            if (hasDirectories)
            {
                return false;
            }

            return true;
        }

        private void menuDeleteDirectory_Click(object? sender, EventArgs e)
        {
            TreeNode? selectedNode = treDir.SelectedNode;
            bool hasSelection = selectedNode != null;
            if (!hasSelection)
            {
                MessageBox.Show("削除するディレクトリを選択してください。", "ディレクトリ削除", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string? directoryPath = selectedNode?.Tag as string;
            bool hasPath = !string.IsNullOrWhiteSpace(directoryPath);
            if (!hasPath)
            {
                MessageBox.Show("ディレクトリパスが取得できません。", "ディレクトリ削除", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool exists = Directory.Exists(directoryPath);
            if (!exists)
            {
                MessageBox.Show("ディレクトリが存在しません。", "ディレクトリ削除", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 確認メッセージを表示
            string directoryName = Path.GetFileName(directoryPath);
            if (string.IsNullOrWhiteSpace(directoryName))
            {
                directoryName = directoryPath;
            }

            string message = $"ディレクトリ「{directoryName}」とその中のすべてのファイル・フォルダを削除します。\nよろしいですか？";
            DialogResult confirmResult = MessageBox.Show(message, "ディレクトリ削除確認", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            bool canceled = confirmResult != DialogResult.Yes;
            if (canceled)
            {
                return;
            }

            try
            {
                // ディレクトリを削除（再帰的）
                Directory.Delete(directoryPath, recursive: true);

                // 親ディレクトリに移動
                string? parentDir = Path.GetDirectoryName(directoryPath);
                bool hasParent = !string.IsNullOrWhiteSpace(parentDir) && Directory.Exists(parentDir);
                if (hasParent)
                {
                    LoadImagesForDirectory(parentDir);
                    _currentDirectory = parentDir;
                }
                else
                {
                    _currentDirectory = string.Empty;
                    flowThumbs.Controls.Clear();
                }

                // ツリーを更新
                bool hasRoot = !string.IsNullOrWhiteSpace(_treeRootPath) && Directory.Exists(_treeRootPath);
                if (hasRoot)
                {
                    PopulateTree(_treeRootPath);
                }

                MessageBox.Show("ディレクトリを削除しました。", "ディレクトリ削除", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ディレクトリの削除に失敗しました。\n{ex.Message}", "ディレクトリ削除エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private HashSet<string> SaveExpandedNodePaths()
        {
            HashSet<string> expandedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CollectExpandedPaths(treDir.Nodes, expandedPaths);
            return expandedPaths;
        }

        private void CollectExpandedPaths(TreeNodeCollection nodes, HashSet<string> expandedPaths)
        {
            foreach (TreeNode node in nodes)
            {
                bool isExpanded = node.IsExpanded;
                if (isExpanded)
                {
                    string? path = node.Tag as string;
                    bool hasPath = !string.IsNullOrWhiteSpace(path);
                    if (hasPath && path != null)
                    {
                        expandedPaths.Add(path);
                    }

                    CollectExpandedPaths(node.Nodes, expandedPaths);
                }
            }
        }

        private void RestoreExpandedNodePaths(HashSet<string> expandedPaths)
        {
            bool hasPaths = expandedPaths.Count > 0;
            if (!hasPaths)
            {
                return;
            }

            treDir.BeginUpdate();
            try
            {
                ExpandNodesByPaths(treDir.Nodes, expandedPaths);
            }
            finally
            {
                treDir.EndUpdate();
            }
        }

        private void ExpandNodesByPaths(TreeNodeCollection nodes, HashSet<string> expandedPaths)
        {
            foreach (TreeNode node in nodes)
            {
                string? path = node.Tag as string;
                bool shouldExpand = !string.IsNullOrWhiteSpace(path) && expandedPaths.Contains(path);
                if (shouldExpand)
                {
                    LoadChildDirectories(node);
                    node.Expand();
                    ExpandNodesByPaths(node.Nodes, expandedPaths);
                }
            }
        }

        private void RestoreSelectedNode(string? selectedPath)
        {
            bool hasPath = !string.IsNullOrWhiteSpace(selectedPath);
            if (!hasPath)
            {
                return;
            }

            TreeNode? nodeToSelect = FindNodeByPath(treDir.Nodes, selectedPath);
            if (nodeToSelect != null)
            {
                treDir.SelectedNode = nodeToSelect;
                nodeToSelect.EnsureVisible();
            }
        }

        private TreeNode? FindNodeByPath(TreeNodeCollection nodes, string targetPath)
        {
            foreach (TreeNode node in nodes)
            {
                string? nodePath = node.Tag as string;
                bool isMatch = !string.IsNullOrWhiteSpace(nodePath) && string.Equals(nodePath, targetPath, StringComparison.OrdinalIgnoreCase);
                if (isMatch)
                {
                    return node;
                }

                TreeNode? found = FindNodeByPath(node.Nodes, targetPath);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private void cbxPreview_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
