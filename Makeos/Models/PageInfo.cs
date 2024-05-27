namespace Makeos.Models
{
    public class PageInfo
    {
        public int PageNumber { get; set; }
        public List<WordInfo> Words { get; set; }
        public OCRTextInfo OCRText { get; set; }
    }
}
