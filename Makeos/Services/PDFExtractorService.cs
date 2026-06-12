using Makeos.Models;
using Makeos.Utilities;

namespace Makeos.Services
{
    public class PDFExtractorService : IPDFExtractorService
    {
        private readonly string _ocrLanguages;

        public PDFExtractorService(IConfiguration configuration)
        {
            _ocrLanguages = configuration["Ocr:Languages"] ?? OCRTextExtractor.DefaultLanguage;
        }

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
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            if (!IsPdf(memoryStream))
            {
                throw new ArgumentException("El contenido del archivo no corresponde a un PDF válido.");
            }

            try
            {
                PDFInfo pdfInfo = PDFTextExtractor.ExtractText(memoryStream, _ocrLanguages);
                pdfInfo.PDFName = file.FileName;
                return pdfInfo;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al procesar el archivo PDF.", ex);
            }
        }

        private static bool IsPdf(Stream stream)
        {
            // Un PDF válido comienza con la firma "%PDF".
            Span<byte> header = stackalloc byte[4];
            int bytesRead = stream.Read(header);
            stream.Position = 0;

            return bytesRead == 4 &&
                   header[0] == (byte)'%' &&
                   header[1] == (byte)'P' &&
                   header[2] == (byte)'D' &&
                   header[3] == (byte)'F';
        }
    }
}
