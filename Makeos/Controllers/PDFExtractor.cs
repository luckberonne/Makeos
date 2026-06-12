using Microsoft.AspNetCore.Mvc;
using Makeos.Models;
using Makeos.Services;

namespace Makeos.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Produces("application/json")]
    public class PDFExtractorController : ControllerBase
    {
        private readonly ILogger<PDFExtractorController> _logger;
        private readonly IPDFExtractorService _pdfExtractorService;
        private readonly IImageExtractorService _imageExtractorService;

        public PDFExtractorController(
            ILogger<PDFExtractorController> logger,
            IPDFExtractorService pdfExtractorService,
            IImageExtractorService imageExtractorService)
        {
            _logger = logger;
            _pdfExtractorService = pdfExtractorService;
            _imageExtractorService = imageExtractorService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(PDFInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PDFInfo>> GetTextFromPdf(IFormFile file, CancellationToken cancellationToken)
        {
            try
            {
                var pdfInfo = await _pdfExtractorService.ExtractTextAsync(file, cancellationToken);
                return Ok(pdfInfo);
            }
            catch (ArgumentException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest, title: "Solicitud inválida");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el archivo PDF.");
                return Problem(detail: "Error al procesar el archivo PDF.", statusCode: StatusCodes.Status500InternalServerError, title: "Error interno");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ImageInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ImageInfo>> GetTextFromImage(IFormFile file, CancellationToken cancellationToken)
        {
            try
            {
                var imageInfo = await _imageExtractorService.ExtractTextAsync(file, cancellationToken);
                return Ok(imageInfo);
            }
            catch (ArgumentException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest, title: "Solicitud inválida");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la imagen.");
                return Problem(detail: "Error al procesar la imagen.", statusCode: StatusCodes.Status500InternalServerError, title: "Error interno");
            }
        }
    }
}
