using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PhotoLabel
{
    public partial class FrmPicture : Form
    {
        private const string ConfigFileName = "Config.ini";
        private string _currentImagePath = string.Empty;
        private bool _settingsLoaded;

        public FrmPicture()
        {
            InitializeComponent();
            Load += FrmPicture_Load;
            FormClosing += FrmPicture_FormClosing;
        }

        private void FrmPicture_Load(object? sender, EventArgs e)
        {
            LoadWindowSettings();
            _settingsLoaded = true;
        }

        public void ShowImage(string imagePath)
        {
            bool hasPath = !string.IsNullOrWhiteSpace(imagePath);
            if (!hasPath)
            {
                return;
            }

            bool fileExists = File.Exists(imagePath);
            if (!fileExists)
            {
                return;
            }

            try
            {
                // 既存の画像を解放
                if (picPreview.Image != null)
                {
                    var oldImage = picPreview.Image;
                    picPreview.Image = null;
                    oldImage.Dispose();
                }

                // 新しい画像を読み込み
                using (var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    picPreview.Image = Image.FromStream(fileStream);
                }

                _currentImagePath = imagePath;
                Text = $"Picture Viewer - {Path.GetFileName(imagePath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"画像の読み込みに失敗しました。\n{ex.Message}", "画像読み込みエラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FrmPicture_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // ウィンドウ設定を保存
            if (_settingsLoaded)
            {
                SaveWindowSettings();
            }

            // フォームを閉じる際に画像を解放
            if (picPreview.Image != null)
            {
                var oldImage = picPreview.Image;
                picPreview.Image = null;
                oldImage.Dispose();
            }
        }

        private void LoadWindowSettings()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
                if (!File.Exists(configPath))
                {
                    return;
                }

                var dict = new Tools.ParameterDict(configPath);

                // FrmPictureの位置とサイズ
                int x = int.TryParse(dict.GetValue("Window", "PictureX", null), out var px) ? px : -1;
                int y = int.TryParse(dict.GetValue("Window", "PictureY", null), out var py) ? py : -1;
                int width = int.TryParse(dict.GetValue("Window", "PictureWidth", null), out var pw) ? pw : -1;
                int height = int.TryParse(dict.GetValue("Window", "PictureHeight", null), out var ph) ? ph : -1;

                bool hasPosition = x >= 0 && y >= 0;
                bool hasSize = width > 0 && height > 0;

                if (hasPosition && hasSize)
                {
                    StartPosition = FormStartPosition.Manual;
                    Location = new Point(x, y);
                    Size = new Size(width, height);
                }
            }
            catch
            {
                // 設定読み込みエラーは無視
            }
        }

        private void SaveWindowSettings()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
                var settings = new Dictionary<string, string>
                {
                    { "PictureX", Location.X.ToString() },
                    { "PictureY", Location.Y.ToString() },
                    { "PictureWidth", Width.ToString() },
                    { "PictureHeight", Height.ToString() }
                };

                Tools.ParameterDict.SaveValues("Window", settings, configPath);
            }
            catch
            {
                // 設定保存エラーは無視
            }
        }
    }
}
