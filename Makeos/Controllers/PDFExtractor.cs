using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace Makeos.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class PDFExtractorController : ControllerBase
    {
        private readonly ILogger<PDFExtractorController> _logger;

        public PDFExtractorController(ILogger<PDFExtractorController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public ActionResult<string> GetTextFromPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No se proporcionó ningún archivo PDF.");
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    using (PdfDocument document = PdfDocument.Open(memoryStream))
                    {
                        string text = "";

                        for (var i = 0; i < document.NumberOfPages; i++)
                        {
                            var page = document.GetPage(i + 1);

                            foreach (var word in page.GetWords())
                            {
                                text += word.Text + " ";
                            }
                        }

                        return Ok(text);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al procesar el archivo PDF: {ex.Message}");
            }
        }
    }
}
