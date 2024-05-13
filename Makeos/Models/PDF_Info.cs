namespace Makeos.Models
{
    public class PDF_Info
    {
        public string PDF_Name { get; set; }
        public int Total_Pages { get; set; }
        public List<PAGE_Info> Pages { get; set; }
    }
}
