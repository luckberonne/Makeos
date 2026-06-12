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
- `POST /PDFExtractor/GetTextFromImage` (form-data, campo `file`): recibe una imagen (PNG/JPG/TIFF/BMP) y devuelve su texto reconocido por OCR junto con sus dimensiones.


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
  - OCRTextExtractor.cs
- Models
  - PDFInfo.cs
  - PageInfo.cs
  - WordInfo.cs
- Program.cs

## Requisitos Previos
- .NET 8
- Docker

