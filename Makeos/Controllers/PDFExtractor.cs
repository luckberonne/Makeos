using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
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
        private readonly IAIInvoiceService _aiInvoiceService;

        public PDFExtractorController(ILogger<PDFExtractorController> logger, IPDFExtractorService pdfExtractorService, IAIInvoiceService aIInvoiceService)
        {
            _logger = logger;
            _pdfExtractorService = pdfExtractorService;
            _aiInvoiceService = aIInvoiceService;
        }

        [HttpPost]
        public async Task<ActionResult<PDFInfo>> GetTextFromPdf(IFormFile file)
        {
            try
            {
                var pdfInfo = await _pdfExtractorService.ExtractTextAsync(file);
                var invoiceInfo = await _aiInvoiceService.ExtractInvoiceInfoAsync(pdfInfo);

                return Ok(pdfInfo);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el archivo PDF.");
                return StatusCode(500, $"Error al procesar el archivo PDF: {ex.Message}");
            }
        }
    }
}

