namespace Makeos.Models
{
    public class OCRTextInfo
    {
        public string OCRText { get; set; } = string.Empty;
        public int XMin { get; set; }
        public int YMin { get; set; }
        public int XMax { get; set; }
        public int YMax { get; set; }
    }
}
