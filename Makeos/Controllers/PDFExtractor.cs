using Microsoft.AspNetCore.Mvc;
using Makeos.Models;
using Makeos.Services;

namespace Makeos.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
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
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PDFInfo>> GetTextFromPdf(IFormFile file)
        {
            try
            {
                var pdfInfo = await _pdfExtractorService.ExtractTextAsync(file);
                return Ok(pdfInfo);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el archivo PDF.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar el archivo PDF.");
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(ImageInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ImageInfo>> GetTextFromImage(IFormFile file)
        {
            try
            {
                var imageInfo = await _imageExtractorService.ExtractTextAsync(file);
                return Ok(imageInfo);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la imagen.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error al procesar la imagen.");
            }
        }
    }
}
