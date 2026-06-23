using System.ComponentModel.DataAnnotations;

namespace Makeos.Configuration
{
    /// <summary>
    /// Opciones de limitación de tasa (sección "RateLimit" de la configuración).
    /// El OCR es intensivo en CPU, así que se limita la concurrencia para evitar que un
    /// pico de peticiones agote los recursos del servicio (DoS accidental o intencional).
    /// </summary>
    public sealed class RateLimitOptions
    {
        public const string SectionName = "RateLimit";

        /// <summary>
        /// Nombre de la política de limitación aplicada a los endpoints de extracción.
        /// </summary>
        public const string PolicyName = "extraction";

        /// <summary>
        /// Indica si la limitación de tasa está activa. Por defecto <c>true</c>.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Máximo de peticiones de extracción procesándose de forma concurrente.
        /// <c>0</c> usa el número de núcleos disponibles.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int PermitLimit { get; set; }

        /// <summary>
        /// Máximo de peticiones en cola cuando se alcanza el límite de concurrencia.
        /// Las que excedan la cola se rechazan con HTTP 429. <c>0</c> = sin cola.
        /// </summary>
        [Range(0, int.MaxValue)]
        public int QueueLimit { get; set; }
    }
}
