using System.ComponentModel.DataAnnotations;

namespace Makeos.Configuration
{
    /// <summary>
    /// Opciones de procesamiento de PDF (sección "Pdf" de la configuración).
    /// </summary>
    public sealed class PdfOptions
    {
        public const string SectionName = "Pdf";

        /// <summary>
        /// Máximo de páginas permitidas por PDF. <c>0</c> = sin límite.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int MaxPages { get; set; }
    }
}
