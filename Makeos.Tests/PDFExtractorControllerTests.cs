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
        private static PDFExtractorController CreateController(
            IPDFExtractorService? pdfService = null,
            IImageExtractorService? imageService = null)
            => new(
                NullLogger<PDFExtractorController>.Instance,
                pdfService ?? new StubService((_, _) => Task.FromResult(new PDFInfo())),
                imageService ?? new StubImageService((_, _) => Task.FromResult(new ImageInfo())));

        private static ProblemDetails AssertProblem(IActionResult? result, int expectedStatus)
        {
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(expectedStatus, objectResult.StatusCode);
            return Assert.IsType<ProblemDetails>(objectResult.Value);
        }

        [Fact]
        public async Task GetTextFromPdf_Success_ReturnsOkWithPdfInfo()
        {
            var pdfInfo = new PDFInfo { PDFName = "factura.pdf", TotalPages = 1 };
            var controller = CreateController(new StubService((_, _) => Task.FromResult(pdfInfo)));

            var result = await controller.GetTextFromPdf(TestFiles.Create(new byte[] { 1 }, "factura.pdf"), default);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(pdfInfo, ok.Value);
        }

        [Fact]
        public async Task GetTextFromPdf_ArgumentException_ReturnsBadRequestProblem()
        {
            var controller = CreateController(new StubService((_, _) => throw new ArgumentException("archivo inválido")));

            var result = await controller.GetTextFromPdf(TestFiles.Create(new byte[] { 1 }, "x.pdf"), default);

            var problem = AssertProblem(result.Result, StatusCodes.Status400BadRequest);
            Assert.Equal("archivo inválido", problem.Detail);
        }

        [Fact]
        public async Task GetTextFromPdf_UnexpectedException_Returns500WithoutLeakingDetails()
        {
            var controller = CreateController(new StubService((_, _) => throw new InvalidOperationException("detalle interno sensible")));

            var result = await controller.GetTextFromPdf(TestFiles.Create(new byte[] { 1 }, "x.pdf"), default);

            var problem = AssertProblem(result.Result, StatusCodes.Status500InternalServerError);
            Assert.DoesNotContain("detalle interno sensible", problem.Detail);
        }

        [Fact]
        public async Task GetTextFromImage_Success_ReturnsOkWithImageInfo()
        {
            var imageInfo = new ImageInfo { ImageName = "factura.png", OCRText = "Factura 1" };
            var controller = CreateController(imageService: new StubImageService((_, _) => Task.FromResult(imageInfo)));

            var result = await controller.GetTextFromImage(TestFiles.Create(new byte[] { 1 }, "factura.png"), default);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(imageInfo, ok.Value);
        }

        [Fact]
        public async Task GetTextFromImage_ArgumentException_ReturnsBadRequestProblem()
        {
            var controller = CreateController(imageService: new StubImageService((_, _) => throw new ArgumentException("imagen inválida")));

            var result = await controller.GetTextFromImage(TestFiles.Create(new byte[] { 1 }, "x.png"), default);

            var problem = AssertProblem(result.Result, StatusCodes.Status400BadRequest);
            Assert.Equal("imagen inválida", problem.Detail);
        }

        [Fact]
        public async Task GetTextFromImage_UnexpectedException_Returns500WithoutLeakingDetails()
        {
            var controller = CreateController(imageService: new StubImageService((_, _) => throw new InvalidOperationException("detalle interno sensible")));

            var result = await controller.GetTextFromImage(TestFiles.Create(new byte[] { 1 }, "x.png"), default);

            var problem = AssertProblem(result.Result, StatusCodes.Status500InternalServerError);
            Assert.DoesNotContain("detalle interno sensible", problem.Detail);
        }

        private sealed class StubService : IPDFExtractorService
        {
            private readonly Func<IFormFile, CancellationToken, Task<PDFInfo>> _handler;

            public StubService(Func<IFormFile, CancellationToken, Task<PDFInfo>> handler) => _handler = handler;

            public Task<PDFInfo> ExtractTextAsync(IFormFile file, CancellationToken cancellationToken = default)
                => _handler(file, cancellationToken);
        }

        private sealed class StubImageService : IImageExtractorService
        {
            private readonly Func<IFormFile, CancellationToken, Task<ImageInfo>> _handler;

            public StubImageService(Func<IFormFile, CancellationToken, Task<ImageInfo>> handler) => _handler = handler;

            public Task<ImageInfo> ExtractTextAsync(IFormFile file, CancellationToken cancellationToken = default)
                => _handler(file, cancellationToken);
        }
    }
}
