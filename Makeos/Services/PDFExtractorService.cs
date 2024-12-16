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
                throw new ArgumentException("El archivo proporcionado está vacío o es nulo.");
            }

            if (!file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("El archivo proporcionado no tiene un formato PDF válido.");
            }

            await using var memoryStream = new MemoryStream();
            try
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                PDFInfo pdfInfo = PDFTextExtractor.ExtractText(memoryStream);
                return pdfInfo;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al procesar el archivo PDF.", ex);
            }
        }
    }
}
