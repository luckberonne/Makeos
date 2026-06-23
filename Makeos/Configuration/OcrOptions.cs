using System.ComponentModel.DataAnnotations;

namespace Makeos.Configuration
{
    /// <summary>
    /// Opciones de OCR (sección "Ocr" de la configuración).
    /// </summary>
    public sealed class OcrOptions
    {
        public const string SectionName = "Ocr";

        /// <summary>
        /// Idiomas de Tesseract separados por "+". Cada idioma requiere su archivo
        /// <c>Data/tessdata/&lt;idioma&gt;.traineddata</c>.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Languages { get; set; } = "spa+eng";

        /// <summary>
        /// Máximo de motores Tesseract ociosos retenidos por idioma. <c>0</c> usa el número
        /// de núcleos disponibles.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int MaxPoolSize { get; set; }
    }
}
