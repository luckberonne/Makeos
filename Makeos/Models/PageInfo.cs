namespace Makeos.Models
{
    public class PageInfo
    {
        public int PageNumber { get; set; }
        public List<WordInfo> Words { get; set; } = new();
        public List<OCRTextInfo> OCRText { get; set; } = new();
    }
}
