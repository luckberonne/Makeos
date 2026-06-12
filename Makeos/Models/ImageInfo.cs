namespace Makeos.Models
{
    public class ImageInfo
    {
        public string ImageName { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public string OCRText { get; set; } = string.Empty;
    }
}
