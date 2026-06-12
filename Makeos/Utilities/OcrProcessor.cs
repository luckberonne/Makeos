using Makeos.Models;
using Tesseract;

namespace Makeos.Utilities
{
    /// <summary>
    /// Ejecuta OCR sobre una imagen usando un motor ya inicializado (tomado del pool).
    /// Es un componente sin estado: toda la información de cada motor vive en el pool.
    /// </summary>
    public static class OcrProcessor
    {
        // Las facturas suelen estar en español; se mantiene inglés como respaldo.
        public const string DefaultLanguage = "spa+eng";

        public static OcrResult Recognize(TesseractEngine engine, byte[] imageBytes)
        {
            using var img = Pix.LoadFromMemory(imageBytes);
            using var page = engine.Process(img);

            return new OcrResult
            {
                Text = page.GetText(),
                Width = img.Width,
                Height = img.Height,
                Words = ExtractWordBoxes(page)
            };
        }

        private static List<WordInfo> ExtractWordBoxes(Tesseract.Page page)
        {
            var words = new List<WordInfo>();

            using var iter = page.GetIterator();
            iter.Begin();
            do
            {
                if (!iter.TryGetBoundingBox(PageIteratorLevel.Word, out var rect))
                {
                    continue;
                }

                var text = iter.GetText(PageIteratorLevel.Word);
                if (string.IsNullOrWhiteSpace(text))
                {
                    continue;
                }

                // Coordenadas en píxeles de la imagen, con origen en la esquina superior izquierda.
                words.Add(new WordInfo
                {
                    Word = text.Trim(),
                    XMin = rect.X1,
                    YMin = rect.Y1,
                    XMax = rect.X2,
                    YMax = rect.Y2
                });
            }
            while (iter.Next(PageIteratorLevel.Word));

            return words;
        }
    }

    public sealed class OcrResult
    {
        public string Text { get; init; } = string.Empty;
        public int Width { get; init; }
        public int Height { get; init; }
        public List<WordInfo> Words { get; init; } = new();
    }
}
