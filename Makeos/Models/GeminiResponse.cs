namespace Makeos.Models
{
    public class GeminiResponse
    {
        public GeminiGeneration[] Generations { get; set; }
    }

    public class GeminiGeneration
    {
        public string Content { get; set; }
    }

    public class InvoiceInfo
    {
        public string InvoiceNumber { get; set; }
        public string Vendor { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public Dictionary<string, decimal> Items { get; set; }
    }
}