namespace Makeos.Models
{
    public class PDFInfo
    {
        public string PDFName { get; set; }
        public int TotalPages { get; set; }
        public List<PageInfo> Pages { get; set; }
    }
}
