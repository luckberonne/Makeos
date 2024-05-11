using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Syncfusion.Pdf.Parsing;
using System;
using System.IO;
using Syncfusion.Pdf;

namespace Makeos.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public ActionResult GetTextFromPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No se proporcionó ningún archivo PDF.");
            }

            try
            {
                using (MemoryStream pdfStream = new MemoryStream())
                {
                    file.CopyTo(pdfStream);
                    pdfStream.Position = 0;

                    // Load the PDF document
                    PdfLoadedDocument lDoc = new PdfLoadedDocument(pdfStream);

                    string extractedText = ExtractTextFromPdf(lDoc);

                    return Ok(extractedText);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Se produjo un error al procesar el OCR: {ex.Message}");
            }
        }

        private string ExtractTextFromPdf(PdfLoadedDocument pdfDocument)
        {
            string extractedText = string.Empty;

            foreach (PdfLoadedPage loadedPage in pdfDocument.Pages)
            {
                extractedText += loadedPage.ExtractText();
            }

            return extractedText;
        }
    }
}
