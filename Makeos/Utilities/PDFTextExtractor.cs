using Makeos.Models;
using UglyToad.PdfPig;

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
                    PageInfo pageInfo = new PageInfo
                    {
                        PageNumber = page.Number,
                        Words = new List<WordInfo>()
                    };

                    foreach (var word in page.GetWords())
                    {
                        WordInfo wordInfo = new WordInfo
                        {
                            Word = word.Text,
                            XMin = (int)word.BoundingBox.BottomLeft.X,
                            YMin = (int)word.BoundingBox.BottomLeft.Y,
                            XMax = (int)word.BoundingBox.TopRight.X,
                            YMax = (int)word.BoundingBox.TopRight.Y
                        };
                        pageInfo.Words.Add(wordInfo);
                    }
                    pdfInfo.Pages.Add(pageInfo);
                }
            }

            return pdfInfo;
        }
    }
}
