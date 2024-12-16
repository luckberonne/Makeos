using Makeos.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Makeos.Utilities
{
    public class PDFTextExtractor
    {
        public static PDFInfo ExtractText(Stream pdfStream)
        {
            PDFInfo pdfInfo = new PDFInfo();

            using (PdfDocument document = PdfDocument.Open(pdfStream))
            {
                pdfInfo.TotalPages = document.NumberOfPages;
                pdfInfo.Pages = new List<PageInfo>();

                for (var i = 0; i < document.NumberOfPages; i++)
                {
                    var page = document.GetPage(i + 1);
                    var pageInfo = ExtractPageInfo(page);
                    pdfInfo.Pages.Add(pageInfo);
                }
            }

            return pdfInfo;
        }

        private static PageInfo ExtractPageInfo(Page page)
        {
            var pageInfo = new PageInfo
            {
                PageNumber = page.Number,
                Words = ExtractWords(page),
                OCRText = ExtractOCRText(page)
            };

            return pageInfo;
        }

        private static List<WordInfo> ExtractWords(Page page)
        {
            var words = new List<WordInfo>();

            foreach (var word in page.GetWords())
            {
                words.Add(new WordInfo
                {
                    Word = word.Text,
                    XMin = (int)word.BoundingBox.BottomLeft.X,
                    YMin = (int)word.BoundingBox.BottomLeft.Y,
                    XMax = (int)word.BoundingBox.TopRight.X,
                    YMax = (int)word.BoundingBox.TopRight.Y
                });
            }

            return words;
        }

        private static List<OCRTextInfo> ExtractOCRText(Page page)
        {
            var ocrTextList = new List<OCRTextInfo>();

            var images = page.GetImages();
            if (images != null)
            {
                foreach (var image in images)
                {
                    using (var imageStream = new MemoryStream(image.RawBytes.ToArray()))
                    {
                        string ocrText = OCRTextExtractor.ExtractTextFromImage(imageStream);

                        ocrTextList.Add(new OCRTextInfo
                        {
                            OCRText = ocrText,
                            XMin = (int)image.Bounds.BottomLeft.X,
                            YMin = (int)image.Bounds.BottomLeft.Y,
                            XMax = (int)image.Bounds.TopRight.X,
                            YMax = (int)image.Bounds.TopRight.Y
                        });
                    }
                }
            }

            return ocrTextList;
        }
    }
}
