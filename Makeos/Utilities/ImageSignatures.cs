namespace Makeos.Utilities
{
    /// <summary>
    /// Valida que un archivo sea realmente una imagen en un formato que Leptonica/Tesseract
    /// pueden decodificar, comprobando sus "magic bytes" (no solo la extensión).
    /// </summary>
    public static class ImageSignatures
    {
        private static readonly byte[][] Signatures =
        {
            new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, // PNG
            new byte[] { 0xFF, 0xD8, 0xFF },                               // JPEG
            new byte[] { 0x49, 0x49, 0x2A, 0x00 },                         // TIFF (little-endian)
            new byte[] { 0x4D, 0x4D, 0x00, 0x2A },                         // TIFF (big-endian)
            new byte[] { 0x42, 0x4D }                                      // BMP
        };

        public static bool IsSupportedImage(byte[] content)
        {
            foreach (var signature in Signatures)
            {
                if (StartsWith(content, signature))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool StartsWith(byte[] content, byte[] signature)
        {
            if (content.Length < signature.Length)
            {
                return false;
            }

            for (int i = 0; i < signature.Length; i++)
            {
                if (content[i] != signature[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
