using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Drawing;

namespace PhotoLabel.Ocr
{
    public class GoogleVisionClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _visionApiUrl;

        public GoogleVisionClient(string visionApiUrl, string? apiKey = null)
        {
            if (string.IsNullOrWhiteSpace(visionApiUrl))
            {
                throw new ArgumentNullException(nameof(visionApiUrl));
            }

            _visionApiUrl = visionApiUrl;
            _httpClient = new HttpClient();

            // Append API key only when it is not already present in the URL.
            var hasKeyParam = _visionApiUrl.IndexOf("key=", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!hasKeyParam && !string.IsNullOrWhiteSpace(apiKey) &&
                _visionApiUrl.Contains("googleapis.com", StringComparison.OrdinalIgnoreCase))
            {
                _visionApiUrl = _visionApiUrl.Contains("?")
                    ? $"{_visionApiUrl}&key={apiKey}"
                    : $"{_visionApiUrl}?key={apiKey}";
            }
        }

        public async Task<List<OcrResult>> ExtractTextAsync(string imagePath)
        {
            var base64 = await ConvertImageToBase64Async(imagePath);
            var body = CreateRequest(base64);
            var response = await _httpClient.PostAsync(_visionApiUrl, new StringContent(body, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return ParseResponse(json);
        }

        private async Task<string> ConvertImageToBase64Async(string imagePath)
        {
            await using var ms = new MemoryStream();
            using (var img = Image.FromFile(imagePath))
            {
                var copy = ResizeForVision(img, 4096, 640);
                copy.Save(ms, ImageFormat.Jpeg);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        private static Image ResizeForVision(Image src, int maxDim, int minDim)
        {
            var width = src.Width;
            var height = src.Height;

            if (width > maxDim || height > maxDim)
            {
                var scale = Math.Min((double)maxDim / width, (double)maxDim / height);
                width = (int)(width * scale);
                height = (int)(height * scale);
            }
            if (width < minDim && height < minDim)
            {
                var scale = Math.Max((double)minDim / width, (double)minDim / height);
                width = (int)(width * scale);
                height = (int)(height * scale);
            }

            var bmp = new Bitmap(width, height);
            using var g = Graphics.FromImage(bmp);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(src, 0, 0, width, height);
            return bmp;
        }

        private static string CreateRequest(string base64)
        {
            var payload = new
            {
                requests = new[]
                {
                    new
                    {
                        image = new { content = base64 },
                        features = new[] { new { type = "TEXT_DETECTION", maxResults = 50 } }
                    }
                }
            };

            return JsonSerializer.Serialize(payload);
        }

        private static List<OcrResult> ParseResponse(string json)
        {
            var results = new List<OcrResult>();

            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("responses", out var responses) || responses.GetArrayLength() == 0)
            {
                return results;
            }

            var first = responses[0];
            if (!first.TryGetProperty("textAnnotations", out var annotations))
            {
                return results;
            }

            var index = 0;
            foreach (var ann in annotations.EnumerateArray())
            {
                if (!ann.TryGetProperty("description", out var desc)) continue;
                var text = desc.GetString();
                if (string.IsNullOrWhiteSpace(text)) continue;

                var coords = new List<Coordinate>();
                if (ann.TryGetProperty("boundingPoly", out var poly) &&
                    poly.TryGetProperty("vertices", out var verts))
                {
                    foreach (var v in verts.EnumerateArray())
                    {
                        int x = v.TryGetProperty("x", out var xProp) && xProp.ValueKind == JsonValueKind.Number ? xProp.GetInt32() : 0;
                        int y = v.TryGetProperty("y", out var yProp) && yProp.ValueKind == JsonValueKind.Number ? yProp.GetInt32() : 0;
                        coords.Add(new Coordinate { X = x, Y = y });
                    }
                }

                if (index == 0)
                {
                    var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var trimmed = line.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                        {
                            results.Add(new OcrResult { Text = trimmed, Coordinates = coords, IsFullText = true });
                        }
                    }
                }
                else
                {
                    results.Add(new OcrResult { Text = text.Trim(), Coordinates = coords, IsFullText = false });
                }
                index++;
            }

            return results;
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
