namespace Makeos.Utilities
{
    /// <summary>
    /// Validaciones comunes a las cargas de archivos (PDF e imágenes).
    /// </summary>
    public static class UploadValidation
    {
        /// <summary>
        /// Verifica que el archivo no sea nulo, vacío ni supere el tamaño máximo permitido.
        /// Lanza <see cref="ArgumentException"/> (mapeada a HTTP 400) si no es válido.
        /// </summary>
        /// <remarks>
        /// El límite de Kestrel rechaza la conexión a nivel de transporte; esta comprobación
        /// adicional permite devolver un mensaje claro y acotar la memoria que se reserva
        /// para el archivo antes de procesarlo.
        /// </remarks>
        public static void EnsureWithinSizeLimit(IFormFile? file, long maxFileSizeBytes)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("El archivo proporcionado está vacío o es nulo.");
            }

            if (maxFileSizeBytes > 0 && file.Length > maxFileSizeBytes)
            {
                throw new ArgumentException(
                    $"El archivo supera el tamaño máximo permitido de {maxFileSizeBytes} bytes.");
            }
        }
    }
}
