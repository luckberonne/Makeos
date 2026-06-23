using System.ComponentModel.DataAnnotations;

namespace Makeos.Configuration
{
    /// <summary>
    /// Opciones de autenticación por clave de API (sección "ApiKey" de la configuración).
    /// </summary>
    public sealed class ApiKeyOptions
    {
        public const string SectionName = "ApiKey";

        /// <summary>
        /// Indica si se exige clave de API en los endpoints de extracción.
        /// Por defecto <c>false</c> para no romper despliegues existentes; al activarla
        /// hay que configurar al menos una clave en <see cref="Keys"/>.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Nombre del header HTTP donde el cliente envía la clave. Por defecto <c>X-Api-Key</c>.
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string HeaderName { get; set; } = "X-Api-Key";

        /// <summary>
        /// Claves válidas. Cualquiera de ellas autoriza la petición.
        /// </summary>
        public IList<string> Keys { get; set; } = new List<string>();
    }
}
