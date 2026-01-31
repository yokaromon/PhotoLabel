using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace PhotoLabel
{
    public enum NameHighlightStyle
    {
        BlueText,
        YellowHatch
    }

    /// <summary>
    /// Thumbnail card for the image list (uses designer controls).
    /// </summary>
    public class ThumbnailCard : UserControl
    {
        private PictureBox picBox = null!;
        private CheckBox chkSelect = null!;
        private Label lblDate = null!;
        private Label lblSize = null!;
        private HighlightLabel lblName = null!;
        private TextBox txtRename = null!;
        private const int CursorBorderWidth = 2;
        private static readonly Color CursorBorderColor = Color.DarkOrange;
        private bool _cursorHighlighted;
        private bool _selectionHighlighted;
        private Color _groupBackgroundColor = SystemColors.Window;

        public string FilePath { get; private set; } = string.Empty;
        public CheckBox SelectionCheckBox => chkSelect;
        public bool IsSelected { get; private set; }
        public event EventHandler<ThumbnailRenameEventArgs>? RenameRequested;
        public event EventHandler? ImageDoubleClick;

        public ThumbnailCard(string filePath)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            InitializeComponent();

            // runtime tweaks
            DoubleBuffered = true;
            Margin = new Padding(10);
            BorderStyle = BorderStyle.FixedSingle;
            BackColor = SystemColors.Window;

            picBox.SizeMode = PictureBoxSizeMode.Zoom;
            picBox.BorderStyle = BorderStyle.FixedSingle;
            picBox.BackColor = SystemColors.ControlLight;

            txtRename.KeyDown += TxtRename_KeyDown;
            txtRename.Leave += (_, _) => CancelRenameEdit();
            DoubleClick += (_, _) => BeginRenameEdit();
            picBox.DoubleClick += (_, e) => ImageDoubleClick?.Invoke(this, e);
            lblName.DoubleClick += (_, _) => BeginRenameEdit();
            lblDate.DoubleClick += (_, _) => BeginRenameEdit();
            lblSize.DoubleClick += (_, _) => BeginRenameEdit();

            // Bubble child clicks to parent so selection always triggers
            picBox.Click += BubbleClick;
            lblName.Click += BubbleClick;
            lblDate.Click += BubbleClick;
            lblSize.Click += BubbleClick;
            SetSelectionHighlight(chkSelect.Checked);
            // picBoxとラベルに対してはMouseイベント転送を行わない（DoubleClickを妨げるため）
            // AttachChildMouseForwarders(lblName);
            // AttachChildMouseForwarders(lblDate);
            // AttachChildMouseForwarders(lblSize);

            LoadMetadata(filePath);
        }

        public void UpdateFilePath(string newPath)
        {
            FilePath = newPath ?? throw new ArgumentNullException(nameof(newPath));
            LoadMetadata(newPath);
        }

        private void LoadMetadata(string filePath)
        {
            // ファイル名ラベルを更新し、強調表示の長さを補正する
            string fileName = Path.GetFileName(filePath);
            lblName.Text = fileName;
            lblName.AdjustHighlightLength();
            var modified = File.GetLastWriteTime(filePath);
            lblDate.Text = $"Modified: {modified:yyyy/MM/dd HH:mm:ss}";
            lblSize.Text = GetImageSizeText(filePath);

            try
            {
                var thumbPath = GetOrCreateThumbnail(filePath);
                if (File.Exists(thumbPath))
                {
                    using var fs = new FileStream(thumbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    picBox.Image = Image.FromStream(fs);
                }
            }
            catch
            {
                // ignore thumbnail errors
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

        private static string GetOrCreateThumbnail(string filePath, int width = 200, int height = 150)
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

        public void SetSelected(bool selected)
        {
            // 選択状態をチェックボックスと背景色に同期する
            bool isSame = chkSelect.Checked == selected;
            if (isSame)
            {
                SetSelectionHighlight(selected);
                return;
            }

            chkSelect.Checked = selected;
        }

        public void SetCursorHighlight(bool highlighted)
        {
            // カーソル枠線の表示を更新する
            bool isSame = _cursorHighlighted == highlighted;
            if (isSame)
            {
                return;
            }

            _cursorHighlighted = highlighted;
            Invalidate();
        }

        private void SetSelectionHighlight(bool selected)
        {
            // 背景色の同期は選択状態のみで行う
            bool isSame = _selectionHighlighted == selected;
            if (isSame)
            {
                return;
            }

            _selectionHighlighted = selected;
            ApplyHighlightState();
        }

        private void ApplyHighlightState()
        {
            bool isSelected = _selectionHighlighted;
            IsSelected = isSelected;
            BackColor = isSelected ? Color.LightSteelBlue : _groupBackgroundColor;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            bool hasCursor = _cursorHighlighted;
            if (!hasCursor)
            {
                return;
            }

            // カーソル位置を枠線で明示する
            Rectangle bounds = ClientRectangle;
            int borderWidth = CursorBorderWidth;
            bool canDraw = bounds.Width > borderWidth * 2 && bounds.Height > borderWidth * 2;
            if (!canDraw)
            {
                return;
            }

            int offset = borderWidth / 2;
            Rectangle drawRect = new Rectangle(
                bounds.X + offset,
                bounds.Y + offset,
                bounds.Width - borderWidth,
                bounds.Height - borderWidth);

            using (Pen pen = new Pen(CursorBorderColor, borderWidth))
            {
                e.Graphics.DrawRectangle(pen, drawRect);
            }
        }

        public void SetGroupBackgroundColor(Color color)
        {
            _groupBackgroundColor = color;
            ApplyHighlightState();
        }

        public void SetNameHighlight(int length, NameHighlightStyle style)
        {
            // ファイル名の共通部分を強調表示する
            lblName.SetHighlight(length, style);
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

        private void BubbleClick(object? sender, EventArgs e)
        {
            OnClick(e);
        }

        private void AttachChildMouseForwarders(Control control)
        {
            // 子コントロールでのドラッグ操作もカードのイベントとして扱う
            control.MouseDown += (_, e) => OnMouseDown(e);
            control.MouseMove += (_, e) => OnMouseMove(e);
            control.MouseUp += (_, e) => OnMouseUp(e);
        }

        private void BeginRenameEdit()
        {
            bool alreadyEditing = txtRename.Visible;
            if (alreadyEditing)
            {
                return;
            }

            Rectangle labelBounds = lblName.Bounds;
            int desiredWidth = Math.Max(labelBounds.Width, 200);
            int desiredHeight = Math.Max(labelBounds.Height + 4, 30);
            txtRename.SetBounds(labelBounds.X, labelBounds.Y, desiredWidth, desiredHeight);
            string currentName = Path.GetFileName(FilePath);
            txtRename.Text = currentName;
            txtRename.Visible = true;
            txtRename.BringToFront();
            txtRename.Focus();
            int extensionIndex = currentName.LastIndexOf(".", StringComparison.Ordinal);
            bool hasExtension = extensionIndex > 0;
            if (hasExtension)
            {
                txtRename.SelectionStart = 0;
                txtRename.SelectionLength = extensionIndex;
                return;
            }

            txtRename.SelectAll();
        }

        private void CancelRenameEdit()
        {
            bool editing = txtRename.Visible;
            if (!editing)
            {
                return;
            }

            txtRename.Visible = false;
            txtRename.Text = string.Empty;
        }

        private void CommitRenameEdit()
        {
            bool editing = txtRename.Visible;
            if (!editing)
            {
                return;
            }

            string rawText = txtRename.Text ?? string.Empty;
            string enteredText = rawText.Trim();
            bool hasText = !string.IsNullOrWhiteSpace(enteredText);
            if (!hasText)
            {
                CancelRenameEdit();
                return;
            }

            ThumbnailRenameEventArgs args = new ThumbnailRenameEventArgs(enteredText);
            EventHandler<ThumbnailRenameEventArgs>? handler = RenameRequested;
            if (handler == null)
            {
                CancelRenameEdit();
                return;
            }

            handler(this, args);
            bool succeeded = args.Success;
            if (succeeded)
            {
                CancelRenameEdit();
                return;
            }

            txtRename.Focus();
            txtRename.SelectAll();
        }

        private void TxtRename_KeyDown(object? sender, KeyEventArgs e)
        {
            bool isEnter = e.KeyCode == Keys.Enter;
            if (isEnter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                CommitRenameEdit();
                return;
            }

            bool isEscape = e.KeyCode == Keys.Escape;
            if (!isEscape)
            {
                return;
            }

            e.Handled = true;
            e.SuppressKeyPress = true;
            CancelRenameEdit();
        }

        private static string ComputePathHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash)[..16];
        }

        /// <summary>
        /// Designer-generated controls (kept in-code for simplicity).
        /// </summary>
        private void InitializeComponent()
        {
            picBox = new PictureBox();
            chkSelect = new CheckBox();
            lblDate = new Label();
            lblName = new HighlightLabel();
            lblSize = new Label();
            ((System.ComponentModel.ISupportInitialize)picBox).BeginInit();
            SuspendLayout();
            // 
            // picBox
            // 
            picBox.Location = new Point(65, 20);
            picBox.Margin = new Padding(4, 4, 4, 4);
            picBox.Name = "picBox";
            picBox.Size = new Size(260, 192);
            picBox.SizeMode = PictureBoxSizeMode.Zoom;
            picBox.BorderStyle = BorderStyle.FixedSingle;
            picBox.TabIndex = 0;
            picBox.TabStop = false;
            // 
            // chkSelect
            // 
            chkSelect.AutoSize = true;
            chkSelect.Location = new Point(16, 108);
            chkSelect.Margin = new Padding(4, 4, 4, 4);
            chkSelect.Name = "chkSelect";
            chkSelect.Size = new Size(28, 27);
            chkSelect.TabIndex = 1;
            chkSelect.UseVisualStyleBackColor = true;
            chkSelect.CheckedChanged += ChkSelect_CheckedChanged;
            // 
            // lblDate
            // 
            lblDate.AutoSize = true;
            lblDate.Font = new Font("Yu Gothic UI", 10F);
            lblDate.Location = new Point(351, 90);
            lblDate.Margin = new Padding(4, 0, 4, 0);
            lblDate.Name = "lblDate";
            lblDate.Size = new Size(145, 37);
            lblDate.TabIndex = 2;
            lblDate.Text = "Date / Size";
            // 
            // lblName
            // 
            lblName.AutoSize = true;
            lblName.Font = new Font("Yu Gothic UI", 11F, FontStyle.Bold);
            lblName.Location = new Point(351, 42);
            lblName.Margin = new Padding(4, 0, 4, 0);
            lblName.Name = "lblName";
            lblName.Size = new Size(100, 41);
            lblName.TabIndex = 3;
            lblName.Text = "Name";
            lblName.BackColor = Color.Transparent;
            // 
            // lblSize
            // 
            lblSize.AutoSize = true;
            lblSize.Font = new Font("Yu Gothic UI", 10F);
            lblSize.Location = new Point(351, 140);
            lblSize.Margin = new Padding(4, 0, 4, 0);
            lblSize.Name = "lblSize";
            lblSize.Size = new Size(145, 37);
            lblSize.TabIndex = 4;
            lblSize.Text = "Date / Size";
            //
            // txtRename
            //
            txtRename = new TextBox();
            txtRename.Visible = false;
            txtRename.Font = new Font("Yu Gothic UI", 11F, FontStyle.Bold);
            txtRename.Location = new Point(351, 42);
            txtRename.Margin = new Padding(4, 4, 4, 4);
            txtRename.Name = "txtRename";
            txtRename.Size = new Size(320, 47);
            txtRename.TabIndex = 5;
            // 
            // ThumbnailCard
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(lblSize);
            Controls.Add(txtRename);
            Controls.Add(lblName);
            Controls.Add(lblDate);
            Controls.Add(chkSelect);
            Controls.Add(picBox);
            Margin = new Padding(4, 4, 4, 4);
            Name = "ThumbnailCard";
            Size = new Size(1024, 243);
            ((System.ComponentModel.ISupportInitialize)picBox).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private void ChkSelect_CheckedChanged(object? sender, EventArgs e)
        {
            bool selected = chkSelect.Checked;
            SetSelectionHighlight(selected);
        }
    }

    public sealed class ThumbnailRenameEventArgs : EventArgs
    {
        public ThumbnailRenameEventArgs(string proposedName)
        {
            ProposedName = proposedName;
        }

        public string ProposedName { get; }
        public bool Success { get; set; }
    }

    internal sealed class HighlightLabel : Label
    {
        private const int PrefixPaddingAdjustment = 2;
        private int _highlightLength;
        private NameHighlightStyle _highlightStyle = NameHighlightStyle.BlueText;

        public void SetHighlight(int length, NameHighlightStyle style)
        {
            // 強調表示の長さとスタイルを更新する
            int safeLength = Math.Max(0, length);
            string text = Text ?? string.Empty;
            if (safeLength > text.Length)
            {
                safeLength = text.Length;
            }

            bool lengthChanged = _highlightLength != safeLength;
            bool styleChanged = _highlightStyle != style;
            if (!lengthChanged && !styleChanged)
            {
                return;
            }

            _highlightLength = safeLength;
            _highlightStyle = style;
            Invalidate();
        }

        public void AdjustHighlightLength()
        {
            // テキスト変更時に強調長を補正する
            int length = _highlightLength;
            string text = Text ?? string.Empty;
            if (length > text.Length)
            {
                _highlightLength = text.Length;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // ファイル名の共通部分だけを色分け描画する
            string text = Text ?? string.Empty;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            Rectangle bounds = ClientRectangle;
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
            int highlightLength = _highlightLength;
            bool hasHighlight = highlightLength > 0;
            if (!hasHighlight)
            {
                TextRenderer.DrawText(e.Graphics, text, Font, bounds, ForeColor, flags);
                return;
            }

            if (highlightLength >= text.Length)
            {
                Color highlightColor = GetHighlightTextColor();
                TextRenderer.DrawText(e.Graphics, text, Font, bounds, highlightColor, flags);
                return;
            }

            string prefix = text.Substring(0, highlightLength);
            string suffix = text.Substring(highlightLength);

            Color prefixColor = GetHighlightTextColor();
            TextRenderer.DrawText(e.Graphics, prefix, Font, bounds, prefixColor, flags);

            Size prefixSize = TextRenderer.MeasureText(e.Graphics, prefix, Font, bounds.Size, flags);
            int offsetX = Math.Max(0, prefixSize.Width - PrefixPaddingAdjustment);
            Rectangle suffixBounds = new Rectangle(bounds.X + offsetX, bounds.Y, Math.Max(0, bounds.Width - offsetX), bounds.Height);
            TextRenderer.DrawText(e.Graphics, suffix, Font, suffixBounds, ForeColor, flags);
        }

        private Color GetHighlightTextColor()
        {
            // 強調表示の文字色を返す
            if (_highlightStyle == NameHighlightStyle.BlueText)
            {
                return Color.DodgerBlue;
            }

            return ForeColor;
        }
    }
}
