using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoLabel.Ocr
{
    public class OcrService
    {
        private readonly GoogleVisionClient _client;
        private readonly OcrCacheService _cache;
        private readonly ReplaceService _replace;

        public OcrService(GoogleVisionClient client, OcrCacheService cache, ReplaceService replace)
        {
            _client = client;
            _cache = cache;
            _replace = replace;
        }

        public async Task<OcrResultWithCacheInfo> ExtractTextAsync(string imagePath)
        {
            var cached = await _cache.GetCachedResultAsync(imagePath);
            if (cached != null && cached.ExtractedTexts.Count > 0)
            {
                var processed = _replace.Apply(cached.ExtractedTexts);
                return new OcrResultWithCacheInfo
                {
                    ExtractedTexts = processed,
                    FromCache = true,
                    ProcessedAt = cached.ProcessedAt
                };
            }

            var results = await _client.ExtractTextAsync(imagePath);
            await _cache.CacheResultAsync(imagePath, results);
            var processedTexts = _replace.Apply(results);

            return new OcrResultWithCacheInfo
            {
                ExtractedTexts = processedTexts,
                FromCache = false,
                ProcessedAt = System.DateTime.UtcNow
            };
        }
    }

    public class OcrResultWithCacheInfo
    {
        public List<OcrResult> ExtractedTexts { get; set; } = new();
        public bool FromCache { get; set; }
        public System.DateTime ProcessedAt { get; set; }
    }
}
