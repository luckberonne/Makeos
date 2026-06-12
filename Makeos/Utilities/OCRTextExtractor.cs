using Tesseract;

namespace Makeos.Utilities
{
    /// <summary>
    /// Envuelve un <see cref="TesseractEngine"/> reutilizable. El motor usa recursos
    /// nativos, por lo que debe crearse una sola vez por documento y liberarse con Dispose.
    /// </summary>
    public sealed class OCRTextExtractor : IDisposable
    {
        // Las facturas suelen estar en español; se mantiene inglés como respaldo.
        private const string DefaultLanguage = "spa+eng";

        // La ruta se resuelve respecto al directorio del ejecutable (no al directorio de
        // trabajo) para que el OCR funcione sin importar desde dónde se lance el proceso.
        private static readonly string TessDataPath =
            Path.Combine(AppContext.BaseDirectory, "Data", "tessdata");

        private readonly TesseractEngine _engine;

        public OCRTextExtractor(string language = DefaultLanguage)
        {
            _engine = new TesseractEngine(TessDataPath, language, EngineMode.Default);
        }

        public string ExtractTextFromImage(byte[] imageBytes)
            => Recognize(imageBytes).Text;

        /// <summary>
        /// Ejecuta OCR sobre una imagen y devuelve el texto reconocido junto con las
        /// dimensiones de la imagen analizada.
        /// </summary>
        public (string Text, int Width, int Height) Recognize(byte[] imageBytes)
        {
            using var img = Pix.LoadFromMemory(imageBytes);
            using var page = _engine.Process(img);
            return (page.GetText(), img.Width, img.Height);
        }

        public void Dispose()
        {
            _engine.Dispose();
        }
    }
}
