using System.Diagnostics;
using Makeos.Configuration;
using Makeos.Models;
using Makeos.Utilities;
using Microsoft.Extensions.Options;

namespace Makeos.Services
{
    public class ImageExtractorService : IImageExtractorService
    {
        private static readonly string[] AllowedExtensions =
            { ".png", ".jpg", ".jpeg", ".tif", ".tiff", ".bmp" };

        private readonly ITesseractEnginePool _enginePool;
        private readonly ILogger<ImageExtractorService> _logger;
        private readonly string _ocrLanguages;
        private readonly long _maxFileSizeBytes;

        public ImageExtractorService(
            IOptions<OcrOptions> ocrOptions,
            IOptions<UploadOptions> uploadOptions,
            ITesseractEnginePool enginePool,
            ILogger<ImageExtractorService> logger)
        {
            _enginePool = enginePool;
            _logger = logger;
            _ocrLanguages = string.IsNullOrWhiteSpace(ocrOptions.Value.Languages)
                ? OcrProcessor.DefaultLanguage
                : ocrOptions.Value.Languages;
            _maxFileSizeBytes = uploadOptions.Value.MaxFileSizeBytes;
        }

        public async Task<ImageInfo> ExtractTextAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            UploadValidation.EnsureWithinSizeLimit(file, _maxFileSizeBytes);

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
                var stopwatch = Stopwatch.StartNew();
                using var engine = _enginePool.Rent(_ocrLanguages);
                var result = OcrProcessor.Recognize(engine.Engine, imageBytes);
                stopwatch.Stop();

                _logger.LogInformation(
                    "Imagen procesada: {FileName} ({Width}x{Height}, {Bytes} bytes, {Words} palabras) en {ElapsedMs} ms.",
                    file.FileName, result.Width, result.Height, imageBytes.Length, result.Words.Count, stopwatch.ElapsedMilliseconds);

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
