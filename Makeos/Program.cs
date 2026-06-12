using Makeos.Services;
using Makeos.Utilities;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// El pool de motores Tesseract es singleton: reutiliza motores (caros de crear y no
// thread-safe) entre peticiones.
builder.Services.AddSingleton<ITesseractEnginePool, TesseractEnginePool>();
builder.Services.AddScoped<IPDFExtractorService, PDFExtractorService>();
builder.Services.AddScoped<IImageExtractorService, ImageExtractorService>();

// Límite de tamaño para la carga de archivos (configurable; por defecto 50 MB).
long maxFileSizeBytes = builder.Configuration.GetValue<long?>("Upload:MaxFileSizeBytes") ?? 50L * 1024 * 1024;
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

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
