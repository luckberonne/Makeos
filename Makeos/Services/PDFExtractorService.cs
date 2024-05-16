using Makeos.Models;
using Makeos.Utilities;

namespace Makeos.Services
{
    public class PDFExtractorService : IPDFExtractorService
    {
        public async Task<PDFInfo> ExtractTextAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No se proporcionó ningún archivo PDF.");
            }

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                PDFInfo PDF_Info = PDFTextExtractor.ExtractText(memoryStream);
                return PDF_Info;
            }
        }
    }
}
