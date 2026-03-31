using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PhotoLabel.Ocr
{
    /// <summary>
    /// OCRエンドポイント・Item候補・置換ルール・画像処理パラメータを保持し、
    /// 画像からItem1〜4の値を抽出するクラス。
    /// </summary>
    public class PictureToText : IDisposable
    {
        // ---- OCR endpoint ----
        public string VisionApiUrl { get; }
        public string? ApiKey { get; }

        // ---- Item matching config ----
        /// <summary>Item1マッチパターン。"regex=replacement" または "regex" 形式。</summary>
        public string Item1Pattern { get; set; } = string.Empty;
        public List<ItemCandidate> Item2Candidates { get; set; } = new();
        public List<ItemCandidate> Item3Candidates { get; set; } = new();
        public List<ItemCandidate> Item4Candidates { get; set; } = new();

        // ---- Replace rules ----
        /// <summary>ReplaceRules.dat 由来のルール（正規表現 or 固定文字列）</summary>
        public List<ReplaceRule> FileReplaceRules { get; set; } = new();
        /// <summary>UIのtxtReplace由来のインラインルール行（"pattern=replacement" 形式）</summary>
        public List<string> InlineReplaceRules { get; set; } = new();

        // ---- Image processing options ----
        /// <summary>true の場合、OCR前に画像から看板領域を自動検出して切り抜く</summary>
        public bool EnableSignboardCrop { get; set; }

        private readonly GoogleVisionClient _client;
        private readonly OcrCacheService _cache;

        public PictureToText(string visionApiUrl, string? apiKey = null)
        {
            VisionApiUrl = visionApiUrl;
            ApiKey = apiKey;
            _client = new GoogleVisionClient(visionApiUrl, apiKey);
            _cache = new OcrCacheService("PhotoLabel");
        }

        /// <summary>
        /// ファイルパス指定でOCRを実行。キャッシュを使用する。
        /// </summary>
        public async Task<OcrMatchResult> ExecuteOcrAsync(string imagePath)
        {
            var cached = await _cache.GetCachedResultAsync(imagePath);
            List<OcrResult> rawTexts;
            bool fromCache;
            string? cropPath = null;

            if (cached != null && cached.ExtractedTexts.Count > 0)
            {
                rawTexts = cached.ExtractedTexts;
                fromCache = true;
            }
            else
            {
                string cropNote;
                if (EnableSignboardCrop)
                {
                    using var img = Image.FromFile(imagePath);
                    var (cropped, savedPath, note) = CropSignboard(img);
                    cropNote = note;
                    using (cropped)
                    {
                        bool signboardDetected = savedPath != null;
                        if (signboardDetected)
                        {
                            // 看板検出 → 切り抜き画像でOCR
                            rawTexts = await _client.ExtractTextFromImageAsync(cropped);
                            cropPath = savedPath;
                        }
                        else
                        {
                            // 看板未検出 → 通常OCR（フルパス・キャッシュ対応）
                            rawTexts = await _client.ExtractTextAsync(imagePath);
                        }
                    }
                }
                else
                {
                    rawTexts = await _client.ExtractTextAsync(imagePath);
                    cropNote = "EnableSignboardCrop=false";
                }
                await _cache.CacheResultAsync(imagePath, rawTexts);
                fromCache = false;

                return new OcrMatchResult
                {
                    Items = MatchItems(rawTexts),
                    RawTexts = rawTexts,
                    FromCache = fromCache,
                    SignboardCropPath = cropPath,
                    SignboardNote = cropNote,
                };
            }

            return new OcrMatchResult
            {
                Items = MatchItems(rawTexts),
                RawTexts = rawTexts,
                FromCache = true,
                SignboardCropPath = null,
                SignboardNote = "キャッシュヒット（看板検出スキップ）",
            };
        }

        /// <summary>
        /// Image直接指定でOCRを実行。キャッシュなし。看板検出オプション適用。
        /// </summary>
        public async Task<OcrMatchResult> ExecuteOcrAsync(Image image)
        {
            string? cropPath = null;
            Image target = image;
            bool needDispose = false;

            string imageNote = "EnableSignboardCrop=false";
            if (EnableSignboardCrop)
            {
                var (cropped, savedPath, note) = CropSignboard(image);
                target = cropped;
                cropPath = savedPath;
                imageNote = note;
                needDispose = true;
            }

            try
            {
                var rawTexts = await _client.ExtractTextFromImageAsync(target);
                return new OcrMatchResult
                {
                    Items = MatchItems(rawTexts),
                    RawTexts = rawTexts,
                    FromCache = false,
                    SignboardCropPath = cropPath,
                    SignboardNote = imageNote,
                };
            }
            finally
            {
                if (needDispose) target.Dispose();
            }
        }

        // -------------------------------------------------------------------------
        // Item matching
        // -------------------------------------------------------------------------

        private Dictionary<string, string> MatchItems(List<OcrResult> texts)
        {
            var result = new Dictionary<string, string>
            {
                ["Item1"] = string.Empty,
                ["Item2"] = string.Empty,
                ["Item3"] = string.Empty,
                ["Item4"] = string.Empty,
            };

            bool hasText = texts.Count > 0;
            if (!hasText)
            {
                return result;
            }

            var joined = string.Join("\n", texts
                .Select(t => t.Text)
                .Where(t => !string.IsNullOrWhiteSpace(t)));
            if (string.IsNullOrWhiteSpace(joined))
            {
                return result;
            }

            joined = ApplyAllReplaceRules(joined);

            result["Item1"] = MatchItem1(joined);
            result["Item2"] = MatchCandidate(Item2Candidates, joined);
            result["Item3"] = MatchCandidate(Item3Candidates, joined);
            result["Item4"] = MatchCandidate(Item4Candidates, joined);

            return result;
        }

        private string ApplyAllReplaceRules(string text)
        {
            foreach (var rule in FileReplaceRules)
            {
                try
                {
                    text = rule.IsRegex
                        ? Regex.Replace(text, rule.Pattern, rule.Replacement)
                        : text.Replace(rule.Pattern, rule.Replacement);
                }
                catch { }
            }

            foreach (var line in InlineReplaceRules)
            {
                var parts = line.Split(',');
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    int eqIndex = trimmed.IndexOf('=');
                    bool hasEquals = eqIndex > 0;
                    if (!hasEquals) continue;

                    var pattern = trimmed.Substring(0, eqIndex);
                    var replacement = trimmed.Substring(eqIndex + 1);
                    try { text = Regex.Replace(text, pattern, replacement); } catch { }
                }
            }

            return text;
        }

        private string MatchItem1(string text)
        {
            bool hasPattern = !string.IsNullOrWhiteSpace(Item1Pattern);
            if (!hasPattern) return string.Empty;

            try
            {
                var parts = Item1Pattern.Split('=', 2);
                var pattern = parts[0];
                var replacement = parts.Length > 1 ? parts[1] : "$0";
                if (string.IsNullOrWhiteSpace(pattern)) return string.Empty;

                var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                var match = regex.Match(text);
                return match.Success ? match.Result(replacement) : string.Empty;
            }
            catch { return string.Empty; }
        }

        private static string MatchCandidate(List<ItemCandidate> candidates, string text)
        {
            if (candidates.Count == 0 || string.IsNullOrWhiteSpace(text)) return string.Empty;

            // 長いパターンを優先（"塗装上塗り" が "塗装" より先にマッチするように）
            foreach (var candidate in candidates.OrderByDescending(c => c.Pattern.Length))
            {
                try
                {
                    var regex = new Regex(candidate.Pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    var match = regex.Match(text);
                    if (!match.Success) continue;

                    return candidate.Replacement != null
                        ? match.Result(candidate.Replacement)
                        : match.Value;
                }
                catch { }
            }
            return string.Empty;
        }

        // -------------------------------------------------------------------------
        // Signboard detection
        // -------------------------------------------------------------------------

        /// <summary>
        /// 画像から看板（黒背景・白枠の矩形）を検出して切り抜く。
        /// 検出できた場合はテンポラリに保存してパスを返す。
        /// 検出できなかった場合は元画像のコピーと null を返す。
        /// NOTE: 呼び出し元で戻り値の Image を Dispose すること。
        /// </summary>
        private static (Image Cropped, string? CropPath, string Note) CropSignboard(Image source)
        {
            // 高速化のため小さいサイズで検出する
            const int detectionLongSide = 512;
            double scale = Math.Min(
                (double)detectionLongSide / source.Width,
                (double)detectionLongSide / source.Height);
            int dw = Math.Max(1, (int)(source.Width * scale));
            int dh = Math.Max(1, (int)(source.Height * scale));

            using var small = new Bitmap(dw, dh);
            using (var g = Graphics.FromImage(small))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(source, 0, 0, dw, dh);
            }

            int[,] gray = ToGrayscaleArray(small);
            long[,] prefix = BuildPrefixSum(gray, dw, dh);
            var (smallRect, bestScore) = FindSignboardRect(prefix, dw, dh);

            Image cropped;
            string? cropPath = null;

            if (smallRect == Rectangle.Empty)
            {
                // 検出できなかった場合は元画像のコピーを返す
                cropped = new Bitmap(source.Width, source.Height);
                using var gCopy = Graphics.FromImage(cropped);
                gCopy.DrawImage(source, 0, 0);
                return (cropped, null, $"看板未検出 bestScore={bestScore:F1}");
            }

            // 検出スケールから元サイズへ戻す
            double invScale = 1.0 / scale;
            const int padding = 20;
            int origX1 = Math.Max(0, (int)(smallRect.Left * invScale) - padding);
            int origY1 = Math.Max(0, (int)(smallRect.Top * invScale) - padding);
            int origX2 = Math.Min(source.Width, (int)(smallRect.Right * invScale) + padding);
            int origY2 = Math.Min(source.Height, (int)(smallRect.Bottom * invScale) + padding);
            var origRect = Rectangle.FromLTRB(origX1, origY1, origX2, origY2);

            // 元解像度で切り抜き
            cropped = new Bitmap(origRect.Width, origRect.Height);
            using (var gCrop = Graphics.FromImage(cropped))
            {
                gCrop.DrawImage(source,
                    new Rectangle(0, 0, origRect.Width, origRect.Height),
                    origRect,
                    GraphicsUnit.Pixel);
            }

            // テンポラリディレクトリに保存
            try
            {
                var tempDir = Path.Combine(Path.GetTempPath(), "PhotoLabel", "SignboardCrops");
                Directory.CreateDirectory(tempDir);
                cropPath = Path.Combine(tempDir, $"crop_{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.jpg");
                cropped.Save(cropPath, ImageFormat.Jpeg);
            }
            catch
            {
                // 保存失敗は無視（画像は使い続ける）
                cropPath = null;
            }

            return (cropped, cropPath, $"看板検出 score={bestScore:F1} rect={smallRect}");
        }

        /// <summary>
        /// グレースケール配列の2次元プレフィックスサムを構築する。
        /// prefix[y, x] = gray[0..y-1, 0..x-1] の総和。
        /// </summary>
        private static long[,] BuildPrefixSum(int[,] gray, int w, int h)
        {
            var prefix = new long[h + 1, w + 1];
            for (int y = 1; y <= h; y++)
            {
                for (int x = 1; x <= w; x++)
                {
                    prefix[y, x] = gray[y - 1, x - 1]
                        + prefix[y - 1, x]
                        + prefix[y, x - 1]
                        - prefix[y - 1, x - 1];
                }
            }
            return prefix;
        }

        /// <summary>
        /// プレフィックスサムを使って矩形領域のピクセル平均輝度を O(1) で求める。
        /// </summary>
        private static double RectAvg(long[,] prefix, int x1, int y1, int x2, int y2)
        {
            if (x2 <= x1 || y2 <= y1) return 0;
            long sum = prefix[y2, x2] - prefix[y1, x2] - prefix[y2, x1] + prefix[y1, x1];
            long area = (long)(x2 - x1) * (y2 - y1);
            return area > 0 ? (double)sum / area : 0;
        }

        /// <summary>
        /// 看板矩形を探す。スコア = ボーダー平均輝度 - 内側平均輝度 が最大の矩形を返す。
        /// 閾値未満の場合は Rectangle.Empty を返す。
        /// </summary>
        private static (Rectangle Rect, double BestScore) FindSignboardRect(long[,] prefix, int w, int h)
        {
            // 検出スケール(~512px)での各パラメータ
            const int borderT = 4;       // ボーダー厚さ（px）
            const int step = 8;          // 探索ステップ（px）
            const double minScore = 50;  // ボーダー平均 - 内側平均 の最小スコア

            // 看板は通常画像の下部にあるため、上1/3はスキップ
            int yStart = h / 3;
            int minW = w / 10;
            int maxW = w * 6 / 10;
            int minH = h / 12;
            int maxH = h * 6 / 10;

            Rectangle best = Rectangle.Empty;
            double bestScore = 0;

            for (int y = yStart; y < h - minH; y += step)
            {
                for (int x = 0; x < w - minW; x += step)
                {
                    for (int rh = minH; rh <= maxH && y + rh <= h; rh += step)
                    {
                        for (int rw = minW; rw <= maxW && x + rw <= w; rw += step)
                        {
                            // 内側が狭すぎる場合はスキップ
                            bool validInterior = (rw - 2 * borderT) > 10 && (rh - 2 * borderT) > 10;
                            if (!validInterior) continue;

                            double topAvg    = RectAvg(prefix, x,              y,              x + rw,         y + borderT);
                            double bottomAvg = RectAvg(prefix, x,              y + rh - borderT, x + rw,       y + rh);
                            double leftAvg   = RectAvg(prefix, x,              y + borderT,    x + borderT,    y + rh - borderT);
                            double rightAvg  = RectAvg(prefix, x + rw - borderT, y + borderT, x + rw,         y + rh - borderT);
                            double borderAvg = (topAvg + bottomAvg + leftAvg + rightAvg) / 4.0;

                            double interiorAvg = RectAvg(prefix,
                                x + borderT, y + borderT,
                                x + rw - borderT, y + rh - borderT);

                            double score = borderAvg - interiorAvg;
                            if (score > bestScore)
                            {
                                bestScore = score;
                                best = new Rectangle(x, y, rw, rh);
                            }
                        }
                    }
                }
            }

            bool detected = bestScore >= minScore;
            return (detected ? best : Rectangle.Empty, bestScore);
        }

        /// <summary>
        /// Bitmap をグレースケールの2次元整数配列に変換する（0〜255）。
        /// LockBits を使って高速に処理する。
        /// </summary>
        private static int[,] ToGrayscaleArray(Bitmap bmp)
        {
            int w = bmp.Width, h = bmp.Height;
            var gray = new int[h, w];

            var data = bmp.LockBits(
                new Rectangle(0, 0, w, h),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            try
            {
                int stride = Math.Abs(data.Stride);
                var pixels = new byte[stride * h];
                Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int offset = y * stride + x * 4;
                        int b = pixels[offset];
                        int g = pixels[offset + 1];
                        int r = pixels[offset + 2];
                        // ITU-R BT.601 輝度計算
                        gray[y, x] = (int)(0.299 * r + 0.587 * g + 0.114 * b);
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            return gray;
        }

        /// <summary>OCRキャッシュをすべて削除する。</summary>
        public void ClearCache()
        {
            _cache.ClearAll();
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }

    /// <summary>Item候補のパターンと置換文字列。</summary>
    public record ItemCandidate(string Pattern, string? Replacement);

    /// <summary>ExecuteOcrAsync の戻り値。</summary>
    public class OcrMatchResult
    {
        /// <summary>マッチしたItem値。キーは "Item1"〜"Item4"。</summary>
        public Dictionary<string, string> Items { get; set; } = new();
        /// <summary>Vision APIの生テキスト結果。</summary>
        public List<OcrResult> RawTexts { get; set; } = new();
        public bool FromCache { get; set; }
        /// <summary>EnableSignboardCrop=true で看板が検出・切り抜きされたときの保存パス。未検出時は null。</summary>
        public string? SignboardCropPath { get; set; }
        /// <summary>看板検出の診断メモ（スコア、未検出理由、キャッシュヒット等）。</summary>
        public string? SignboardNote { get; set; }
    }
}
