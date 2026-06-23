using Makeos.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Makeos.Middleware
{
    /// <summary>
    /// Exige una clave de API válida (en el header configurado) para acceder a los endpoints
    /// de extracción. Las rutas de salud (<c>/health</c>) y de Swagger quedan exentas para no
    /// interferir con monitoreo ni con la exploración de la API.
    /// </summary>
    public sealed class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _headerName;
        private readonly HashSet<string> _keys;

        public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiKeyOptions> options)
        {
            _next = next;
            _headerName = options.Value.HeaderName;
            _keys = new HashSet<string>(
                options.Value.Keys.Where(k => !string.IsNullOrWhiteSpace(k)),
                StringComparer.Ordinal);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;
            if (path.StartsWithSegments("/health") || path.StartsWithSegments("/swagger"))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(_headerName, out var provided)
                || !_keys.Contains(provided.ToString()))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "No autorizado",
                    Detail = "Falta o es inválida la clave de API."
                });
                return;
            }

            await _next(context);
        }
    }
}
