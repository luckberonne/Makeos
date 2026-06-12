using Tesseract;

namespace Makeos.Utilities
{
    /// <summary>
    /// Envuelve un <see cref="TesseractEngine"/> reutilizable. El motor usa recursos
    /// nativos, por lo que debe crearse una sola vez por documento y liberarse con Dispose.
    /// </summary>
    public sealed class OCRTextExtractor : IDisposable
    {
        private const string TessDataPath = "./Data/tessdata";
        private const string DefaultLanguage = "eng";

        private readonly TesseractEngine _engine;

        public OCRTextExtractor(string language = DefaultLanguage)
        {
            _engine = new TesseractEngine(TessDataPath, language, EngineMode.Default);
        }

        public string ExtractTextFromImage(byte[] imageBytes)
        {
            using var img = Pix.LoadFromMemory(imageBytes);
            using var page = _engine.Process(img);
            return page.GetText();
        }

        public void Dispose()
        {
            _engine.Dispose();
        }
    }
}
