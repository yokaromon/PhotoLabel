using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PhotoLabel.Ocr
{
    public class ReplaceService
    {
        private readonly List<ReplaceRule> _rules;

        public ReplaceService(List<ReplaceRule> rules)
        {
            _rules = rules ?? new List<ReplaceRule>();
        }

        public List<OcrResult> Apply(List<OcrResult> source)
        {
            if (_rules.Count == 0)
            {
                return new List<OcrResult>(source);
            }

            var results = new List<OcrResult>(source.Count);

            foreach (var item in source)
            {
                var text = item.Text;
                foreach (var rule in _rules)
                {
                    try
                    {
                        text = rule.IsRegex
                            ? Regex.Replace(text, rule.Pattern, rule.Replacement)
                            : text.Replace(rule.Pattern, rule.Replacement);
                    }
                    catch
                    {
                        // ignore bad regex
                    }
                }

                results.Add(new OcrResult
                {
                    Text = text,
                    Coordinates = item.Coordinates,
                    IsFullText = item.IsFullText
                });
            }

            return results;
        }
    }
}
