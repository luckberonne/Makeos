﻿namespace Makeos.Models
{
    public class PageInfo
    {
        public int PageNumber { get; set; }
        public List<WordInfo> Words { get; set; }
        public List<OCRTextInfo> OCRText { get; set; }
    }
}
