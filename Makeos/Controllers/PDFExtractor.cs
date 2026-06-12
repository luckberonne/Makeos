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

        public PDFExtractorController(ILogger<PDFExtractorController> logger, IPDFExtractorService pdfExtractorService)
        {
            _logger = logger;
            _pdfExtractorService = pdfExtractorService;
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
    }
}
