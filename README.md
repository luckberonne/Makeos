# Proyecto: Servicio Web de Extracción de Texto de PDF
## Descripción
Este proyecto consiste en la creación de un servicio web en ASP.NET Core que permite la extracción de texto de archivos PDF y devuelve la información en formato JSON. El servicio puede procesar PDFs para extraer palabras, junto con sus posiciones en la página, y devolver esta información estructurada. Además, puede realizar OCR (Reconocimiento Óptico de Caracteres) en imágenes dentro de los PDFs utilizando Tesseract si fuera necesario.


## Características Clave
Carga de Archivos PDF: Permite la carga de archivos PDF a través de una solicitud HTTP POST.
Extracción de Texto: Utiliza la biblioteca UglyToad.PdfPig para extraer texto y posiciones de las palabras dentro del PDF.
OCR con Tesseract: Realiza OCR en imágenes contenidas en los PDFs utilizando la biblioteca Tesseract si es necesario.
Carga de Imágenes: Permite también enviar imágenes sueltas (PNG, JPG, TIFF o BMP) y obtener su texto por OCR.
Respuesta en Formato JSON: La información extraída se devuelve en un formato JSON estructurado.
Manejo de Errores: El servicio maneja errores y devuelve mensajes de error adecuados.

## Endpoints
- `POST /PDFExtractor/GetTextFromPdf` (form-data, campo `file`): recibe un PDF y devuelve, por página, las palabras de la capa de texto y el texto OCR de las imágenes embebidas.
- `POST /PDFExtractor/GetTextFromImage` (form-data, campo `file`): recibe una imagen (PNG/JPG/TIFF/BMP) y devuelve su texto OCR, sus dimensiones y las palabras reconocidas con sus coordenadas (en píxeles, origen arriba-izquierda).
- `GET /health`: comprobación de estado del servicio.

Los errores se devuelven en formato `ProblemDetails` (RFC 7807) como JSON.

Si la autenticación por clave de API está activada (`ApiKey:Enabled`), las peticiones a los endpoints de extracción deben incluir el header configurado (por defecto `X-Api-Key`) con una clave válida; en caso contrario se devuelve HTTP 401.

## Configuración
Vía `appsettings.json` o variables de entorno (reemplazando `:` por `__`):
- `Ocr:Languages`: idiomas de Tesseract separados por `+`. Por defecto `spa+eng`. Cada idioma requiere su archivo `Data/tessdata/<idioma>.traineddata`.
- `Ocr:MaxPoolSize`: máximo de motores Tesseract ociosos retenidos por idioma. `0` (por defecto) usa el número de núcleos disponibles.
- `Pdf:MaxPages`: máximo de páginas permitidas por PDF. `0` (por defecto) = sin límite.
- `Upload:MaxFileSizeBytes`: tamaño máximo de archivo subido. Por defecto `52428800` (50 MB). Se valida tanto a nivel de Kestrel como en el servicio, devolviendo HTTP 400 con un mensaje claro si se supera.
- `RateLimit:Enabled`: activa la limitación de concurrencia en los endpoints de extracción. Por defecto `true`.
- `RateLimit:PermitLimit`: máximo de peticiones de extracción procesándose a la vez. `0` (por defecto) usa el número de núcleos. Las que exceden cola se rechazan con HTTP 429.
- `RateLimit:QueueLimit`: peticiones que pueden esperar en cola al alcanzar el límite. `0` (por defecto) = sin cola.
- `ApiKey:Enabled`: exige clave de API en los endpoints de extracción. Por defecto `false`. Si se activa sin configurar claves, el arranque falla con un mensaje claro.
- `ApiKey:HeaderName`: header donde el cliente envía la clave. Por defecto `X-Api-Key`.
- `ApiKey:Keys`: lista de claves válidas (cualquiera autoriza). `/health` y `/swagger` quedan exentos.


## Arquitectura del Proyecto
El proyecto sigue una arquitectura de capas que separa las responsabilidades en diferentes componentes:
Copiar código
- Controllers
  - PDFExtractorController.cs
- Services
  - IPDFExtractorService.cs
  - PDFExtractorService.cs
- Utilities
  - PDFTextExtractor.cs
  - OcrProcessor.cs
  - TesseractEnginePool.cs
  - ImageSignatures.cs
  - UploadValidation.cs
- Models
  - PDFInfo.cs
  - PageInfo.cs
  - WordInfo.cs
- Configuration
  - OcrOptions.cs
  - PdfOptions.cs
  - UploadOptions.cs
  - RateLimitOptions.cs
  - ApiKeyOptions.cs
- Middleware
  - ApiKeyMiddleware.cs
- Program.cs

Las secciones de `appsettings.json` (`Ocr`, `Pdf`, `Upload`, `RateLimit`, `ApiKey`) se enlazan a clases de opciones tipadas (`IOptions<T>`) y se validan al arranque: un valor inválido (idioma vacío, número negativo, o `ApiKey:Enabled` sin claves) impide que el servicio inicie en lugar de fallar silenciosamente en tiempo de ejecución.

El OCR del PDF se procesa en paralelo por página (`Parallel.ForEachAsync`, limitado al número de núcleos), y los endpoints de extracción están protegidos por un limitador de concurrencia que rechaza con HTTP 429 cuando se satura, y opcionalmente por autenticación con clave de API.

## Requisitos Previos
- .NET 8
- Docker

