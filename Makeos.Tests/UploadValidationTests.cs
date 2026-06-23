using System.Text;
using Makeos.Utilities;
using Xunit;

namespace Makeos.Tests
{
    public class UploadValidationTests
    {
        [Fact]
        public void EnsureWithinSizeLimit_NullFile_Throws()
        {
            var ex = Assert.Throws<ArgumentException>(() => UploadValidation.EnsureWithinSizeLimit(null, 1000));
            Assert.Contains("vacío o es nulo", ex.Message);
        }

        [Fact]
        public void EnsureWithinSizeLimit_EmptyFile_Throws()
        {
            var file = TestFiles.Create(Array.Empty<byte>(), "vacio.png", "image/png");

            var ex = Assert.Throws<ArgumentException>(() => UploadValidation.EnsureWithinSizeLimit(file, 1000));
            Assert.Contains("vacío o es nulo", ex.Message);
        }

        [Fact]
        public void EnsureWithinSizeLimit_ExceedsLimit_Throws()
        {
            var file = TestFiles.Create(Encoding.ASCII.GetBytes("contenido bastante mas largo que el limite"), "grande.png", "image/png");

            var ex = Assert.Throws<ArgumentException>(() => UploadValidation.EnsureWithinSizeLimit(file, 5));
            Assert.Contains("tamaño máximo", ex.Message);
        }

        [Fact]
        public void EnsureWithinSizeLimit_WithinLimit_DoesNotThrow()
        {
            var file = TestFiles.Create(Encoding.ASCII.GetBytes("ok"), "chico.png", "image/png");

            // No debe lanzar.
            UploadValidation.EnsureWithinSizeLimit(file, 1000);
        }

        [Fact]
        public void EnsureWithinSizeLimit_AtExactLimit_DoesNotThrow()
        {
            var bytes = Encoding.ASCII.GetBytes("12345");
            var file = TestFiles.Create(bytes, "exacto.png", "image/png");

            UploadValidation.EnsureWithinSizeLimit(file, bytes.Length);
        }

        [Fact]
        public void EnsureWithinSizeLimit_ZeroLimit_MeansNoLimit()
        {
            var file = TestFiles.Create(Encoding.ASCII.GetBytes("contenido largo sin limite"), "sinlimite.png", "image/png");

            // maxFileSizeBytes = 0 significa sin límite de tamaño (solo se valida nulo/vacío).
            UploadValidation.EnsureWithinSizeLimit(file, 0);
        }
    }
}
