using Makeos.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Images;

namespace Makeos.Utilities
{
    public static class PDFTextExtractor
    {
        public static PDFInfo ExtractText(Stream pdfStream, string ocrLanguages = OCRTextExtractor.DefaultLanguage)
        {
            PDFInfo pdfInfo = new PDFInfo();

            // El motor OCR se crea una sola vez por documento (y solo si hay imágenes),
            // ya que su inicialización es costosa y usa recursos nativos.
            OCRTextExtractor? ocrExtractor = null;

            try
            {
                using PdfDocument document = PdfDocument.Open(pdfStream);

                pdfInfo.TotalPages = document.NumberOfPages;

                for (var i = 0; i < document.NumberOfPages; i++)
                {
                    var page = document.GetPage(i + 1);
                    pdfInfo.Pages.Add(new PageInfo
                    {
                        PageNumber = page.Number,
                        Words = ExtractWords(page),
                        OCRText = ExtractOCRText(page, ocrLanguages, ref ocrExtractor)
                    });
                }
            }
            finally
            {
                ocrExtractor?.Dispose();
            }

            return pdfInfo;
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

        private static List<OCRTextInfo> ExtractOCRText(Page page, string ocrLanguages, ref OCRTextExtractor? ocrExtractor)
        {
            var ocrTextList = new List<OCRTextInfo>();

            foreach (var image in page.GetImages())
            {
                ocrExtractor ??= new OCRTextExtractor(ocrLanguages);

                // El OCR es "best effort": una imagen ilegible no debe invalidar el resto del documento.
                try
                {
                    string ocrText = ocrExtractor.ExtractTextFromImage(GetImageBytes(image));

                    ocrTextList.Add(new OCRTextInfo
                    {
                        OCRText = ocrText,
                        XMin = (int)image.Bounds.BottomLeft.X,
                        YMin = (int)image.Bounds.BottomLeft.Y,
                        XMax = (int)image.Bounds.TopRight.X,
                        YMax = (int)image.Bounds.TopRight.Y
                    });
                }
                catch (Exception)
                {
                    // Imagen en un formato que Tesseract no puede procesar: se omite.
                }
            }

            return ocrTextList;
        }

        private static byte[] GetImageBytes(IPdfImage image)
        {
            // Los bytes crudos del PDF pueden estar comprimidos (Flate, etc.) y no ser
            // legibles por Tesseract; se prefiere la imagen decodificada como PNG.
            if (image.TryGetPng(out var pngBytes))
            {
                return pngBytes;
            }

            return image.RawBytes.ToArray();
        }
    }
}
