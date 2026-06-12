using Microsoft.AspNetCore.Http;

namespace Makeos.Tests
{
    /// <summary>
    /// Utilidades para construir <see cref="IFormFile"/> en memoria a partir de bytes,
    /// evitando dependencias del pipeline HTTP en las pruebas de los servicios.
    /// </summary>
    internal static class TestFiles
    {
        public static IFormFile Create(byte[] content, string fileName, string contentType = "application/pdf")
        {
            var stream = new MemoryStream(content);
            return new FormFile(stream, 0, content.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        public static byte[] SamplePdfBytes()
            => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", "sample.pdf"));
    }
}
