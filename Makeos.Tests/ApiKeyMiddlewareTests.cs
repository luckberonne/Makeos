using Makeos.Configuration;
using Makeos.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Xunit;

namespace Makeos.Tests
{
    public class ApiKeyMiddlewareTests
    {
        private static ApiKeyMiddleware Create(ApiKeyOptions options, RequestDelegate next)
            => new(next, Options.Create(options));

        private static ApiKeyOptions EnabledWithKey() =>
            new() { Enabled = true, HeaderName = "X-Api-Key", Keys = { "secreta" } };

        [Fact]
        public async Task MissingKey_Returns401AndDoesNotCallNext()
        {
            var called = false;
            var middleware = Create(EnabledWithKey(), _ => { called = true; return Task.CompletedTask; });
            var context = new DefaultHttpContext();
            context.Request.Path = "/PDFExtractor/GetTextFromPdf";
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            Assert.False(called);
            Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvalidKey_Returns401()
        {
            var called = false;
            var middleware = Create(EnabledWithKey(), _ => { called = true; return Task.CompletedTask; });
            var context = new DefaultHttpContext();
            context.Request.Path = "/PDFExtractor/GetTextFromPdf";
            context.Request.Headers["X-Api-Key"] = "incorrecta";
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            Assert.False(called);
            Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        }

        [Fact]
        public async Task ValidKey_CallsNext()
        {
            var called = false;
            var middleware = Create(EnabledWithKey(), _ => { called = true; return Task.CompletedTask; });
            var context = new DefaultHttpContext();
            context.Request.Path = "/PDFExtractor/GetTextFromPdf";
            context.Request.Headers["X-Api-Key"] = "secreta";

            await middleware.InvokeAsync(context);

            Assert.True(called);
        }

        [Theory]
        [InlineData("/health")]
        [InlineData("/swagger/index.html")]
        public async Task ExemptPaths_BypassAuth(string path)
        {
            var called = false;
            var middleware = Create(EnabledWithKey(), _ => { called = true; return Task.CompletedTask; });
            var context = new DefaultHttpContext();
            context.Request.Path = path;

            await middleware.InvokeAsync(context);

            Assert.True(called);
        }
    }
}
