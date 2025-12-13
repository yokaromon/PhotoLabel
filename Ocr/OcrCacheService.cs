using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace PhotoLabel.Ocr
{
    public class CachedOcrResult
    {
        public List<OcrResult> ExtractedTexts { get; set; } = new();
        public DateTime ProcessedAt { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileHash { get; set; } = string.Empty;
    }

    public class OcrCacheService
    {
        private readonly Dictionary<string, CachedOcrResult> _memoryCache = new();
        private readonly string _cacheDirectory;
        private readonly object _lock = new();
        private readonly TimeSpan _maxAge;

        public OcrCacheService(string appName = "PhotoLabel", TimeSpan? maxAge = null)
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _cacheDirectory = Path.Combine(appData, appName, "OcrCache");
            Directory.CreateDirectory(_cacheDirectory);

            _maxAge = maxAge ?? TimeSpan.FromHours(24);
            LoadCacheFromDisk();
        }

        public async Task<string> GenerateCacheKeyAsync(string imagePath)
        {
            var fileInfo = new FileInfo(imagePath);
            var hash = await GenerateImageHashAsync(imagePath);
            return $"{hash}_{fileInfo.Length}";
        }

        public async Task<CachedOcrResult?> GetCachedResultAsync(string imagePath)
        {
            var key = await GenerateCacheKeyAsync(imagePath);
            var cutoff = DateTime.UtcNow - _maxAge;

            lock (_lock)
            {
                if (_memoryCache.TryGetValue(key, out var mem))
                {
                    if (mem.ProcessedAt < cutoff)
                    {
                        _memoryCache.Remove(key);
                    }
                    else
                    {
                        return mem;
                    }
                }
            }

            var path = Path.Combine(_cacheDirectory, $"{key}.json");
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                var json = await File.ReadAllTextAsync(path);
                var disk = JsonSerializer.Deserialize<CachedOcrResult>(json);
                if (disk == null || disk.ProcessedAt < cutoff)
                {
                    File.Delete(path);
                    return null;
                }

                lock (_lock)
                {
                    _memoryCache[key] = disk;
                }
                return disk;
            }
            catch
            {
                return null;
            }
        }

        public async Task CacheResultAsync(string imagePath, List<OcrResult> texts)
        {
            var key = await GenerateCacheKeyAsync(imagePath);
            var info = new FileInfo(imagePath);
            var hash = await GenerateImageHashAsync(imagePath);

            var result = new CachedOcrResult
            {
                ExtractedTexts = texts,
                ProcessedAt = DateTime.UtcNow,
                ImagePath = imagePath,
                FileSize = info.Length,
                FileHash = hash
            };

            lock (_lock)
            {
                _memoryCache[key] = result;
            }

            var path = Path.Combine(_cacheDirectory, $"{key}.json");
            var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await File.WriteAllTextAsync(path, json);
            CleanupExpired();
        }

        private async Task<string> GenerateImageHashAsync(string imagePath)
        {
            using var sha = SHA256.Create();
            await using var fs = File.OpenRead(imagePath);
            var hash = await sha.ComputeHashAsync(fs);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private void CleanupExpired()
        {
            var cutoff = DateTime.UtcNow - _maxAge;

            lock (_lock)
            {
                var keys = new List<string>(_memoryCache.Keys);
                foreach (var k in keys)
                {
                    if (_memoryCache[k].ProcessedAt < cutoff)
                    {
                        _memoryCache.Remove(k);
                    }
                }
            }

            foreach (var file in Directory.GetFiles(_cacheDirectory, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var item = JsonSerializer.Deserialize<CachedOcrResult>(json);
                    if (item == null || item.ProcessedAt < cutoff)
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }

        private void LoadCacheFromDisk()
        {
            var cutoff = DateTime.UtcNow - _maxAge;
            foreach (var file in Directory.GetFiles(_cacheDirectory, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var item = JsonSerializer.Deserialize<CachedOcrResult>(json);
                    if (item != null && item.ProcessedAt >= cutoff)
                    {
                        _memoryCache[Path.GetFileNameWithoutExtension(file)] = item;
                    }
                    else
                    {
                        File.Delete(file);
                    }
                }
                catch
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }
    }
}
