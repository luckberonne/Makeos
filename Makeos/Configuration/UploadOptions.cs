using System.ComponentModel.DataAnnotations;

namespace Makeos.Configuration
{
    /// <summary>
    /// Opciones de carga de archivos (sección "Upload" de la configuración).
    /// </summary>
    public sealed class UploadOptions
    {
        public const string SectionName = "Upload";

        /// <summary>
        /// Tamaño máximo de archivo subido, en bytes. Por defecto 50 MB.
        /// </summary>
        [Range(1, long.MaxValue)]
        public long MaxFileSizeBytes { get; set; } = 50L * 1024 * 1024;
    }
}
