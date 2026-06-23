using Makeos.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Images;

namespace Makeos.Utilities
{
    public static class PDFTextExtractor
    {
        public static async Task<PDFInfo> ExtractTextAsync(
            Stream pdfStream,
            ITesseractEnginePool enginePool,
            string ocrLanguages,
            ILogger logger,
            int maxPages = 0,
            CancellationToken cancellationToken = default)
        {
            using PdfDocument document = PdfDocument.Open(pdfStream);

            if (maxPages > 0 && document.NumberOfPages > maxPages)
            {
                throw new ArgumentException($"El PDF tiene {document.NumberOfPages} páginas y supera el máximo permitido de {maxPages}.");
            }

            // Preparar datos de cada página (lectura del PDF es rápida, no OCR).
            // PdfDocument no es thread-safe, así que se hace de forma secuencial.
            var pageData = new List<(int pageNumber, List<WordInfo> words, List<ImageData> images)>();
            for (var i = 0; i < document.NumberOfPages; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var page = document.GetPage(i + 1);
                var words = ExtractWords(page);
                var images = page.GetImages().Select((img, idx) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return new ImageData
                    {
                        Index = idx,
                        Bytes = GetImageBytes(img),
                        XMin = (int)img.Bounds.BottomLeft.X,
                        YMin = (int)img.Bounds.BottomLeft.Y,
                        XMax = (int)img.Bounds.TopRight.X,
                        YMax = (int)img.Bounds.TopRight.Y
                    };
                }).ToList();

                pageData.Add((page.Number, words, images));
            }

            // Procesar OCR de cada página en paralelo. Cada página renta su propio motor del pool
            // para que no haya contención: con 4 núcleos, hasta 4 páginas se procesan en paralelo.
            var pageInfos = new PageInfo[pageData.Count];
            await Parallel.ForEachAsync(
                Enumerable.Range(0, pageData.Count),
                new ParallelOptions { CancellationToken = cancellationToken, MaxDegreeOfParallelism = Environment.ProcessorCount },
                async (pageIdx, ct) =>
                {
                    var (pageNumber, words, images) = pageData[pageIdx];
                    pageInfos[pageIdx] = await ProcessPageOcrAsync(
                        pageNumber, words, images, enginePool, ocrLanguages, logger, ct);
                });

            var pdfInfo = new PDFInfo
            {
                TotalPages = document.NumberOfPages,
            };
            pdfInfo.Pages.AddRange(pageInfos);
            return pdfInfo;
        }

        private static async Task<PageInfo> ProcessPageOcrAsync(
            int pageNumber,
            List<WordInfo> words,
            List<ImageData> images,
            ITesseractEnginePool enginePool,
            string ocrLanguages,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            PooledEngine? engine = null;

            try
            {
                var ocrTextList = new List<OCRTextInfo>();

                foreach (var image in images)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    engine ??= enginePool.Rent(ocrLanguages);

                    // El OCR es "best effort": una imagen ilegible no debe invalidar el resto del documento.
                    try
                    {
                        var result = OcrProcessor.Recognize(engine.Engine, image.Bytes);

                        ocrTextList.Add(new OCRTextInfo
                        {
                            OCRText = result.Text,
                            XMin = image.XMin,
                            YMin = image.YMin,
                            XMax = image.XMax,
                            YMax = image.YMax
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex,
                            "No se pudo aplicar OCR a la imagen {ImageIndex} de la página {PageNumber}; se omite.",
                            image.Index, pageNumber);
                    }
                }

                return new PageInfo
                {
                    PageNumber = pageNumber,
                    Words = words,
                    OCRText = ocrTextList
                };
            }
            finally
            {
                engine?.Dispose();
            }
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

        private static byte[] GetImageBytes(IPdfImage image)
        {
            if (image.TryGetPng(out var pngBytes))
            {
                return pngBytes;
            }

            return image.RawBytes.ToArray();
        }

        private sealed class ImageData
        {
            public int Index { get; init; }
            public byte[] Bytes { get; init; } = Array.Empty<byte>();
            public int XMin { get; init; }
            public int YMin { get; init; }
            public int XMax { get; init; }
            public int YMax { get; init; }
        }
    }
}
