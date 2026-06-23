using Makeos.Utilities;
using Xunit;

namespace Makeos.Tests
{
    public class ImageSignaturesTests
    {
        public static IEnumerable<object[]> ValidSignatures()
        {
            yield return new object[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, "PNG" };
            yield return new object[] { new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, "JPEG" };
            yield return new object[] { new byte[] { 0x49, 0x49, 0x2A, 0x00 }, "TIFF-LE" };
            yield return new object[] { new byte[] { 0x4D, 0x4D, 0x00, 0x2A }, "TIFF-BE" };
            yield return new object[] { new byte[] { 0x42, 0x4D, 0x00, 0x00 }, "BMP" };
        }

        [Theory]
        [MemberData(nameof(ValidSignatures))]
        public void IsSupportedImage_KnownSignature_ReturnsTrue(byte[] header, string _)
        {
            // Se añade carga útil tras la firma para simular un archivo real.
            var content = header.Concat(new byte[64]).ToArray();

            Assert.True(ImageSignatures.IsSupportedImage(content));
        }

        [Theory]
        [MemberData(nameof(ValidSignatures))]
        public void IsSupportedImage_ExactSignatureLength_ReturnsTrue(byte[] header, string _)
        {
            Assert.True(ImageSignatures.IsSupportedImage(header));
        }

        [Fact]
        public void IsSupportedImage_UnknownBytes_ReturnsFalse()
        {
            var content = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07 };

            Assert.False(ImageSignatures.IsSupportedImage(content));
        }

        [Fact]
        public void IsSupportedImage_EmptyContent_ReturnsFalse()
        {
            Assert.False(ImageSignatures.IsSupportedImage(Array.Empty<byte>()));
        }

        [Fact]
        public void IsSupportedImage_TruncatedSignature_ReturnsFalse()
        {
            // Solo el primer byte de PNG: no debe considerarse una firma válida.
            var content = new byte[] { 0x89 };

            Assert.False(ImageSignatures.IsSupportedImage(content));
        }
    }
}
