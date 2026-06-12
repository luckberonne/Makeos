using Makeos.Models;
using Makeos.Utilities;

namespace Makeos.Services
{
    public class ImageExtractorService : IImageExtractorService
    {
        private static readonly string[] AllowedExtensions =
            { ".png", ".jpg", ".jpeg", ".tif", ".tiff", ".bmp" };

        private readonly ITesseractEnginePool _enginePool;
        private readonly string _ocrLanguages;

        public ImageExtractorService(IConfiguration configuration, ITesseractEnginePool enginePool)
        {
            _enginePool = enginePool;
            _ocrLanguages = configuration["Ocr:Languages"] ?? OcrProcessor.DefaultLanguage;
        }

        public async Task<ImageInfo> ExtractTextAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("El archivo proporcionado está vacío o es nulo.");
            }

            if (!AllowedExtensions.Any(ext => file.FileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("El archivo proporcionado no tiene un formato de imagen válido (PNG, JPG, TIFF o BMP).");
            }

            await using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream, cancellationToken);
            byte[] imageBytes = memoryStream.ToArray();

            if (!ImageSignatures.IsSupportedImage(imageBytes))
            {
                throw new ArgumentException("El contenido del archivo no corresponde a una imagen válida.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var engine = _enginePool.Rent(_ocrLanguages);
                var result = OcrProcessor.Recognize(engine.Engine, imageBytes);

                return new ImageInfo
                {
                    ImageName = file.FileName,
                    Width = result.Width,
                    Height = result.Height,
                    OCRText = result.Text,
                    Words = result.Words
                };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al procesar la imagen.", ex);
            }
        }
    }
}
