using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace PhotoLabel.Ocr
{
    public class ReplaceRule
    {
        public string Pattern { get; set; } = string.Empty;
        public string Replacement { get; set; } = string.Empty;
        public bool IsRegex { get; set; }
    }

    public class ReplaceRuleStore
    {
        private readonly string _rulesPath;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public ReplaceRuleStore(string rulesPath)
        {
            _rulesPath = rulesPath;
        }

        public List<ReplaceRule> Load()
        {
            if (!File.Exists(_rulesPath))
            {
                return new List<ReplaceRule>();
            }

            var rules = new List<ReplaceRule>();
            foreach (var line in File.ReadLines(_rulesPath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    continue;
                }

                try
                {
                    var rule = JsonSerializer.Deserialize<ReplaceRule>(trimmed, _jsonOptions);
                    if (rule != null)
                    {
                        rules.Add(rule);
                    }
                }
                catch
                {
                    // skip invalid line
                }
            }

            return rules;
        }

        public void Save(IEnumerable<ReplaceRule> rules)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_rulesPath) ?? ".");

            using var writer = new StreamWriter(_rulesPath, false);
            foreach (var rule in rules)
            {
                var json = JsonSerializer.Serialize(rule, _jsonOptions);
                writer.WriteLine(json);
            }
        }
    }
}
