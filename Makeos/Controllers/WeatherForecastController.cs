using Microsoft.AspNetCore.Mvc;
using Tesseract;

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
        public ActionResult GetTextFromImage(IFormFile image)
        {
            try
            {
                // Check if the image is not null and it has content
                if (image != null && image.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {
                        // Copy the image stream to a memory stream
                        image.CopyTo(stream);

                        // Reset the stream position to start
                        stream.Position = 0;

                        // Initialize Tesseract engine
                        using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                        {
                            using (var img = Pix.LoadFromFile(image.FileName))
                            {
                                using (var page = engine.Process(img))
                                {
                                    // Get the extracted text
                                    var extractedText = page.GetText();

                                    // You can do further processing with the extracted text here
                                    // For example, return it as JSON
                                    return Ok(new { Text = extractedText });
                                }
                            }
                        }
                    }
                }
                else
                {
                    return BadRequest("Image file is empty.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing the image.");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
