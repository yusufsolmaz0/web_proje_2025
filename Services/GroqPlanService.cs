using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace FitnessCenterManagement.Services
{
    public class GroqPlanService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public GroqPlanService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<string> GeneratePlanAsync(int heightCm, int weightKg, string goal, string? note)
        {
            var apiKey = _config["Groq:ApiKey"];
            var model = _config["Groq:Model"] ?? "llama-3.1-8b-instant";

            if (string.IsNullOrWhiteSpace(apiKey))
                return "Groq ApiKey bulunamadı. appsettings.json içine ekleyin.";

            var prompt =
$@"Kullanıcı bilgileri:
- Boy: {heightCm} cm
- Kilo: {weightKg} kg
- Hedef: {goal}
- Ek not: {note ?? "Yok"}

Bana Türkçe, kısa ve uygulanabilir bir plan yaz:
1) Haftalık egzersiz planı (gün gün)
2) Beslenme önerileri
3) Güvenlik uyarıları (kısa)
Madde madde yaz, çok uzun olmasın.";

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var body = new
            {
                model,
                messages = new object[]
                {
                    new { role = "system", content = "Sen bir fitness koçu ve diyet öneri asistanısın." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(body);
            var res = await client.PostAsync(
                "https://api.groq.com/openai/v1/chat/completions",
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync();
                return $"Groq hata: {res.StatusCode}\n{err}";
            }

            var content = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);

            var text = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return text ?? "Öneri üretilemedi.";
        }
    }
}
