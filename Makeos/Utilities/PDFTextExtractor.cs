using Makeos.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Images;

namespace Makeos.Utilities
{
    public static class PDFTextExtractor
    {
        public static PDFInfo ExtractText(
            Stream pdfStream,
            ITesseractEnginePool enginePool,
            string ocrLanguages,
            ILogger logger,
            int maxPages = 0,
            CancellationToken cancellationToken = default)
        {
            PDFInfo pdfInfo = new PDFInfo();

            // El motor OCR se toma del pool una sola vez por documento (y solo si hay imágenes),
            // ya que su inicialización es costosa y no es thread-safe.
            PooledEngine? engine = null;

            try
            {
                using PdfDocument document = PdfDocument.Open(pdfStream);

                if (maxPages > 0 && document.NumberOfPages > maxPages)
                {
                    throw new ArgumentException($"El PDF tiene {document.NumberOfPages} páginas y supera el máximo permitido de {maxPages}.");
                }


                pdfInfo.TotalPages = document.NumberOfPages;

                for (var i = 0; i < document.NumberOfPages; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var page = document.GetPage(i + 1);
                    pdfInfo.Pages.Add(new PageInfo
                    {
                        PageNumber = page.Number,
                        Words = ExtractWords(page),
                        OCRText = ExtractOCRText(page, enginePool, ocrLanguages, logger, ref engine)
                    });
                }
            }
            finally
            {
                engine?.Dispose();
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

        private static List<OCRTextInfo> ExtractOCRText(
            Page page,
            ITesseractEnginePool enginePool,
            string ocrLanguages,
            ILogger logger,
            ref PooledEngine? engine)
        {
            var ocrTextList = new List<OCRTextInfo>();
            int imageIndex = 0;

            foreach (var image in page.GetImages())
            {
                imageIndex++;
                engine ??= enginePool.Rent(ocrLanguages);

                // El OCR es "best effort": una imagen ilegible no debe invalidar el resto del documento.
                try
                {
                    var result = OcrProcessor.Recognize(engine.Engine, GetImageBytes(image));

                    ocrTextList.Add(new OCRTextInfo
                    {
                        OCRText = result.Text,
                        XMin = (int)image.Bounds.BottomLeft.X,
                        YMin = (int)image.Bounds.BottomLeft.Y,
                        XMax = (int)image.Bounds.TopRight.X,
                        YMax = (int)image.Bounds.TopRight.Y
                    });
                }
                catch (Exception ex)
                {
                    // Imagen en un formato que Tesseract no puede procesar: se omite, pero se
                    // registra para poder diagnosticar por qué no produjo texto.
                    logger.LogWarning(ex,
                        "No se pudo aplicar OCR a la imagen {ImageIndex} de la página {PageNumber}; se omite.",
                        imageIndex, page.Number);
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
