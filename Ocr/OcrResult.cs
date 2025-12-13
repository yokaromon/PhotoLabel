using System.Collections.Generic;

namespace PhotoLabel.Ocr
{
    public class Coordinate
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class OcrResult
    {
        public string Text { get; set; } = string.Empty;
        public List<Coordinate> Coordinates { get; set; } = new();
        public bool IsFullText { get; set; }
    }
}
