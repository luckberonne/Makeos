using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Makeos.Models;

namespace Makeos.Services
{
    public class AIInvoiceService : IAIInvoiceService
    {
        private readonly HttpClient _httpClient;
        private readonly string _geminiApiKey;

        public AIInvoiceService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _geminiApiKey = configuration["Gemini:ApiKey"];
        }

        public async Task<InvoiceInfo> ExtractInvoiceInfoAsync(PDFInfo pdfInfo)
        {
            if (pdfInfo == null || pdfInfo.Pages.Count == 0)
            {
                throw new ArgumentException("No valid PDF information provided.");
            }

            // Create payload for the Gemini API
            var requestPayload = new
            {
                model = "gemini-1.5-pro",
                inputs = new[]
                {
                    new
                    {
                        prompt = $"Extract relevant data from this invoice: {JsonSerializer.Serialize(pdfInfo)}",
                        context = "You are an expert in extracting information from invoices."
                    }
                }
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(requestPayload), Encoding.UTF8, "application/json");

            var requestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro:predict"),
                Headers =
                {
                    { "Authorization", $"Bearer {_geminiApiKey}" }
                },
                Content = requestContent
            };

            // Send request to Gemini API
            var response = await _httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException($"Error calling Gemini API: {errorContent}");
            }

            var responseJson = await response.Content.ReadFromJsonAsync<GeminiResponse>();

            if (responseJson?.Generations == null || responseJson.Generations.Length == 0)
            {
                throw new InvalidOperationException("Gemini API response does not contain valid data.");
            }

            // Parse the response from the API
            var invoiceInfo = JsonSerializer.Deserialize<InvoiceInfo>(responseJson.Generations[0].Content);

            return invoiceInfo ?? throw new InvalidOperationException("Failed to extract invoice information.");
        }
    }
}