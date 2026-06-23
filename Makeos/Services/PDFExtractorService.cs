using Makeos.Configuration;
using Makeos.Models;
using Makeos.Utilities;
using Microsoft.Extensions.Options;

namespace Makeos.Services
{
    public class PDFExtractorService : IPDFExtractorService
    {
        private readonly ITesseractEnginePool _enginePool;
        private readonly ILogger<PDFExtractorService> _logger;
        private readonly string _ocrLanguages;
        private readonly int _maxPages;

        public PDFExtractorService(
            IOptions<OcrOptions> ocrOptions,
            IOptions<PdfOptions> pdfOptions,
            ITesseractEnginePool enginePool,
            ILogger<PDFExtractorService> logger)
        {
            _enginePool = enginePool;
            _logger = logger;
            _ocrLanguages = string.IsNullOrWhiteSpace(ocrOptions.Value.Languages)
                ? OcrProcessor.DefaultLanguage
                : ocrOptions.Value.Languages;
            _maxPages = pdfOptions.Value.MaxPages;
        }

        public async Task<PDFInfo> ExtractTextAsync(IFormFile file, CancellationToken cancellationToken = default)
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
            await file.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            if (!IsPdf(memoryStream))
            {
                throw new ArgumentException("El contenido del archivo no corresponde a un PDF válido.");
            }

            try
            {
                PDFInfo pdfInfo = await PDFTextExtractor.ExtractTextAsync(memoryStream, _enginePool, _ocrLanguages, _logger, _maxPages, cancellationToken);
                pdfInfo.PDFName = file.FileName;
                return pdfInfo;
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                throw;
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
