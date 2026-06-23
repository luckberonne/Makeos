using System.Threading.RateLimiting;
using Makeos.Configuration;
using Makeos.Middleware;
using Makeos.Services;
using Makeos.Utilities;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Configuración tipada con validación al arranque: un valor inválido en appsettings
// (idioma vacío, números negativos, etc.) hace fallar el arranque en vez de pasar
// desapercibido en tiempo de ejecución.
builder.Services.AddOptions<OcrOptions>()
    .Bind(builder.Configuration.GetSection(OcrOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<PdfOptions>()
    .Bind(builder.Configuration.GetSection(PdfOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<UploadOptions>()
    .Bind(builder.Configuration.GetSection(UploadOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<RateLimitOptions>()
    .Bind(builder.Configuration.GetSection(RateLimitOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddOptions<ApiKeyOptions>()
    .Bind(builder.Configuration.GetSection(ApiKeyOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(o => !o.Enabled || o.Keys.Any(k => !string.IsNullOrWhiteSpace(k)),
        "ApiKey:Enabled es true pero no se configuró ninguna clave en ApiKey:Keys.")
    .ValidateOnStart();

// Limitación de concurrencia para los endpoints de extracción: el OCR es intensivo en
// CPU, así que se acota cuántas peticiones se procesan a la vez para que un pico no
// agote el servicio. Las que exceden cola se rechazan con HTTP 429.
var apiKey = builder.Configuration.GetSection(ApiKeyOptions.SectionName)
    .Get<ApiKeyOptions>() ?? new ApiKeyOptions();

var rateLimit = builder.Configuration.GetSection(RateLimitOptions.SectionName)
    .Get<RateLimitOptions>() ?? new RateLimitOptions();
if (rateLimit.Enabled)
{
    int permitLimit = rateLimit.PermitLimit > 0 ? rateLimit.PermitLimit : Environment.ProcessorCount;
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddConcurrencyLimiter(RateLimitOptions.PolicyName, limiterOptions =>
        {
            limiterOptions.PermitLimit = permitLimit;
            limiterOptions.QueueLimit = rateLimit.QueueLimit;
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        });
    });
}

// El pool de motores Tesseract es singleton: reutiliza motores (caros de crear y no
// thread-safe) entre peticiones.
builder.Services.AddSingleton<ITesseractEnginePool, TesseractEnginePool>();
builder.Services.AddScoped<IPDFExtractorService, PDFExtractorService>();
builder.Services.AddScoped<IImageExtractorService, ImageExtractorService>();

// Límite de tamaño para la carga de archivos (configurable; por defecto 50 MB).
long maxFileSizeBytes = builder.Configuration.GetSection(UploadOptions.SectionName)
    .Get<UploadOptions>()?.MaxFileSizeBytes ?? new UploadOptions().MaxFileSizeBytes;
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = maxFileSizeBytes;
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = maxFileSizeBytes;
});

builder.Services.AddHealthChecks();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

// La autenticación se valida antes del rate limiter para que una petición no autorizada
// no consuma un cupo de concurrencia.
if (apiKey.Enabled)
{
    app.UseMiddleware<ApiKeyMiddleware>();
}

if (rateLimit.Enabled)
{
    app.UseRateLimiter();
}

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
