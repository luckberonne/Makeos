using System.Text;
using Makeos.Services;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Makeos.Tests
{
    public class PDFExtractorServiceTests
    {
        private readonly PDFExtractorService _service = new();

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
        public async Task ExtractTextAsync_UppercaseExtension_IsAccepted()
        {
            var file = TestFiles.Create(TestFiles.SamplePdfBytes(), "FACTURA.PDF");

            var result = await _service.ExtractTextAsync(file);

            Assert.Equal("FACTURA.PDF", result.PDFName);
        }
    }
}
