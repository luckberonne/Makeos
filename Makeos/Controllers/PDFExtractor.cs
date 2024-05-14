using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UglyToad.PdfPig;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using Makeos.Models;

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
                PDF_Info pdfInfo = new PDF_Info();
                using (var memoryStream = new MemoryStream())
                {
                    file.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    using (PdfDocument document = PdfDocument.Open(memoryStream))
                    {
                        pdfInfo.PDF_Name = file.FileName;
                        pdfInfo.Total_Pages = document.NumberOfPages;
                        pdfInfo.Pages = new List<PAGE_Info>();

                        for (var i = 0; i < document.NumberOfPages; i++)
                        {
                            var page = document.GetPage(i + 1);
                            PAGE_Info page_info = new PAGE_Info();
                            page_info.Page_number = page.Number;
                            page_info.Words = new List<WORD_Info>();
                            foreach (var word in page.GetWords())
                            {
                                WORD_Info word_info = new WORD_Info();
                                word_info.Word = word.Text;
                                word_info.xmin = (int)word.BoundingBox.BottomLeft.X;
                                word_info.ymin = (int)word.BoundingBox.BottomLeft.Y;
                                word_info.xmax = (int)word.BoundingBox.TopRight.X;
                                word_info.ymax = (int)word.BoundingBox.TopRight.Y;
                                page_info.Words.Add(word_info);
                            }
                            pdfInfo.Pages.Add(page_info);
                        }

                        return Ok(pdfInfo);
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
