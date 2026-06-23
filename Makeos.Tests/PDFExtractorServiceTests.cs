using System.Text;
using Makeos.Configuration;
using Makeos.Services;
using Makeos.Utilities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Makeos.Tests
{
    public class PDFExtractorServiceTests
    {
        private static readonly IOptions<OcrOptions> OcrOpts = Options.Create(new OcrOptions());
        private static readonly IOptions<PdfOptions> PdfOpts = Options.Create(new PdfOptions());
        private static readonly IOptions<UploadOptions> UploadOpts = Options.Create(new UploadOptions());
        private readonly PDFExtractorService _service = new(
            OcrOpts,
            PdfOpts,
            UploadOpts,
            new TesseractEnginePool(OcrOpts),
            NullLogger<PDFExtractorService>.Instance);

        [Fact]
        public async Task ExtractTextAsync_NullFile_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.ExtractTextAsync(null!));
        }

        [Fact]
        public async Task ExtractTextAsync_EmptyFile_ThrowsArgumentException()
        {
            var file = TestFiles.Create(Array.Empty<byte>(), "vacio.pdf");

            await Assert.ThrowsAsync<ArgumentException>(() => _service.ExtractTextAsync(file));
        }

        [Fact]
        public async Task ExtractTextAsync_NonPdfExtension_ThrowsArgumentException()
        {
            var file = TestFiles.Create(Encoding.ASCII.GetBytes("hola"), "documento.txt", "text/plain");

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.ExtractTextAsync(file));
            Assert.Contains("formato PDF", ex.Message);
        }

        [Fact]
        public async Task ExtractTextAsync_PdfExtensionButInvalidContent_ThrowsArgumentException()
        {
            // Extensión .pdf pero el contenido no empieza por la firma "%PDF".
            var file = TestFiles.Create(Encoding.ASCII.GetBytes("esto no es un pdf"), "falso.pdf");

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.ExtractTextAsync(file));
            Assert.Contains("contenido", ex.Message);
        }

        [Fact]
        public async Task ExtractTextAsync_ValidPdf_SetsPdfNameAndExtractsWords()
        {
            var file = TestFiles.Create(TestFiles.SamplePdfBytes(), "factura.pdf");

            var result = await _service.ExtractTextAsync(file);

            Assert.Equal("factura.pdf", result.PDFName);
            Assert.Equal(1, result.TotalPages);
            var page = Assert.Single(result.Pages);
            Assert.Equal(1, page.PageNumber);
            Assert.Equal(new[] { "Hola", "factura", "123" }, page.Words.Select(w => w.Word));
        }

        [Fact]
        public async Task ExtractTextAsync_FileExceedsSizeLimit_ThrowsArgumentException()
        {
            var smallLimit = Options.Create(new UploadOptions { MaxFileSizeBytes = 10 });
            var service = new PDFExtractorService(
                OcrOpts, PdfOpts, smallLimit, new TesseractEnginePool(OcrOpts), NullLogger<PDFExtractorService>.Instance);
            var file = TestFiles.Create(TestFiles.SamplePdfBytes(), "factura.pdf");

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.ExtractTextAsync(file));
            Assert.Contains("tamaño máximo", ex.Message);
        }

        [Fact]
        public async Task ExtractTextAsync_UppercaseExtension_IsAccepted()
        {
            var file = TestFiles.Create(TestFiles.SamplePdfBytes(), "FACTURA.PDF");

            var result = await _service.ExtractTextAsync(file);

            Assert.Equal("FACTURA.PDF", result.PDFName);
        }
    }
}
