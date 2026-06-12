using Makeos.Models;
using Makeos.Utilities;

namespace Makeos.Services
{
    public class ImageExtractorService : IImageExtractorService
    {
        private static readonly string[] AllowedExtensions =
            { ".png", ".jpg", ".jpeg", ".tif", ".tiff", ".bmp" };

        public async Task<ImageInfo> ExtractTextAsync(IFormFile file)
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
            await file.CopyToAsync(memoryStream);
            byte[] imageBytes = memoryStream.ToArray();

            if (!ImageSignatures.IsSupportedImage(imageBytes))
            {
                throw new ArgumentException("El contenido del archivo no corresponde a una imagen válida.");
            }

            try
            {
                using var ocr = new OCRTextExtractor();
                var (text, width, height) = ocr.Recognize(imageBytes);

                return new ImageInfo
                {
                    ImageName = file.FileName,
                    Width = width,
                    Height = height,
                    OCRText = text
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al procesar la imagen.", ex);
            }
        }
    }
}
