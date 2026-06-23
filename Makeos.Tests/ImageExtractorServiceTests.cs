using System.Text;
using Makeos.Configuration;
using Makeos.Services;
using Makeos.Utilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Makeos.Tests
{
    // Nota: el camino "feliz" (OCR real) requiere las librerías nativas de Tesseract y se
    // verifica manualmente contra la API; aquí se cubre solo la validación de entrada, que
    // no toca código nativo y por tanto corre en cualquier CI.
    public class ImageExtractorServiceTests
    {
        private static readonly IOptions<OcrOptions> OcrOpts = Options.Create(new OcrOptions());
        private static readonly IOptions<UploadOptions> UploadOpts = Options.Create(new UploadOptions());
        private readonly ImageExtractorService _service = new(
            OcrOpts, UploadOpts, new TesseractEnginePool(OcrOpts), NullLogger<ImageExtractorService>.Instance);

        [Fact]
        public async Task ExtractTextAsync_NullFile_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ExtractTextAsync(null!));
        }

        [Fact]
        public async Task ExtractTextAsync_EmptyFile_ThrowsArgumentException()
        {
            var file = TestFiles.Create(Array.Empty<byte>(), "vacia.png", "image/png");

            await Assert.ThrowsAsync<ArgumentException>(() => _service.ExtractTextAsync(file));
        }

        [Fact]
        public async Task ExtractTextAsync_UnsupportedExtension_ThrowsArgumentException()
        {
            var file = TestFiles.Create(Encoding.ASCII.GetBytes("hola"), "documento.txt", "text/plain");

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.ExtractTextAsync(file));
            Assert.Contains("formato de imagen", ex.Message);
        }

        [Fact]
        public async Task ExtractTextAsync_ImageExtensionButInvalidContent_ThrowsArgumentException()
        {
            // Extensión .png pero el contenido no tiene la firma de ninguna imagen soportada.
            var file = TestFiles.Create(Encoding.ASCII.GetBytes("esto no es una imagen"), "falsa.png", "image/png");

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.ExtractTextAsync(file));
            Assert.Contains("contenido", ex.Message);
        }

        [Fact]
        public async Task ExtractTextAsync_FileExceedsSizeLimit_ThrowsArgumentException()
        {
            var smallLimit = Options.Create(new UploadOptions { MaxFileSizeBytes = 5 });
            var service = new ImageExtractorService(
                OcrOpts, smallLimit, new TesseractEnginePool(OcrOpts), NullLogger<ImageExtractorService>.Instance);
            var file = TestFiles.Create(Encoding.ASCII.GetBytes("contenido mas largo que el limite"), "imagen.png", "image/png");

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.ExtractTextAsync(file));
            Assert.Contains("tamaño máximo", ex.Message);
        }
    }
}
