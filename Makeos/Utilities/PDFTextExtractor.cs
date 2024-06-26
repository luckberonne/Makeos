﻿using Makeos.Models;
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
                // segmentar el código en métodos más pequeños
                for (var i = 0; i < document.NumberOfPages; i++)
                {
                    var page = document.GetPage(i + 1);
                    PageInfo pageInfo = new PageInfo
                    {
                        PageNumber = page.Number,
                        Words = new List<WordInfo>(),
                        OCRText = new List<OCRTextInfo>()
                    };

                    IEnumerable<IPdfImage> images = page.GetImages();
                    if (images != null)
                    {
                        foreach (var image in images)
                        {
                            Stream imageStream = new MemoryStream(image.RawBytes.ToArray());

                            string ocrText = OCRTextExtractor.ExtractTextFromImage(imageStream);
                            
                            OCRTextInfo ocrTextInfo = new OCRTextInfo
                            {
                                OCRText = ocrText,
                                XMin = (int)image.Bounds.BottomLeft.X,
                                YMin = (int)image.Bounds.BottomLeft.Y,
                                XMax = (int)image.Bounds.TopRight.X,
                                YMax = (int)image.Bounds.TopRight.Y
                            };
                            pageInfo.OCRText.Add(ocrTextInfo);
                        }
                    }
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
