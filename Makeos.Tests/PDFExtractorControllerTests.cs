using Makeos.Controllers;
using Makeos.Models;
using Makeos.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Makeos.Tests
{
    public class PDFExtractorControllerTests
    {
        private static PDFExtractorController CreateController(IPDFExtractorService service)
            => new(NullLogger<PDFExtractorController>.Instance, service);

        [Fact]
        public async Task GetTextFromPdf_Success_ReturnsOkWithPdfInfo()
        {
            var pdfInfo = new PDFInfo { PDFName = "factura.pdf", TotalPages = 1 };
            var controller = CreateController(new StubService(_ => Task.FromResult(pdfInfo)));

            var result = await controller.GetTextFromPdf(TestFiles.Create(new byte[] { 1 }, "factura.pdf"));

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(pdfInfo, ok.Value);
        }

        [Fact]
        public async Task GetTextFromPdf_ArgumentException_ReturnsBadRequest()
        {
            var controller = CreateController(new StubService(_ => throw new ArgumentException("archivo inválido")));

            var result = await controller.GetTextFromPdf(TestFiles.Create(new byte[] { 1 }, "x.pdf"));

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("archivo inválido", bad.Value);
        }

        [Fact]
        public async Task GetTextFromPdf_UnexpectedException_Returns500WithoutLeakingDetails()
        {
            var controller = CreateController(new StubService(_ => throw new InvalidOperationException("detalle interno sensible")));

            var result = await controller.GetTextFromPdf(TestFiles.Create(new byte[] { 1 }, "x.pdf"));

            var error = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status500InternalServerError, error.StatusCode);
            Assert.DoesNotContain("detalle interno sensible", error.Value?.ToString());
        }

        private sealed class StubService : IPDFExtractorService
        {
            private readonly Func<IFormFile, Task<PDFInfo>> _handler;

            public StubService(Func<IFormFile, Task<PDFInfo>> handler) => _handler = handler;

            public Task<PDFInfo> ExtractTextAsync(IFormFile file) => _handler(file);
        }
    }
}
