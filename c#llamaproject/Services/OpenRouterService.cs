using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace c_llamaproject.Services
{
    /// <summary>
    /// Handles all OpenRouter API calls using Nano Banana 2 (Gemini 3.1 Flash Image)
    /// 1. Analyze a copyrighted image and extract a prompt
    /// 2. Generate a new copyright-free image from that prompt
    /// </summary>
    public class OpenRouterService
    {
        // HttpClient — standard .NET library for HTTP requests
        private readonly HttpClient _httpClient;

        // OpenRouter base URL
        private const string BASE_URL = "https://openrouter.ai/api/v1";

        // Nano Banana 2 model ID on OpenRouter
        private const string MODEL = "google/gemini-3.1-flash-image-preview";

        public OpenRouterService(string apiKey)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMinutes(5);

            // Bearer token auth — same format as OpenAI
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            // Required by OpenRouter
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost");
        }

        /// <summary>
        /// Step 1: Send image to Nano Banana 2 for analysis
        /// Returns analysis text + image generation prompt
        /// </summary>
        public async Task<string> AnalyzeImageAsync(string imagePath)
        {
            // Convert image to base64 data URI
            byte[] imageBytes = await File.ReadAllBytesAsync(imagePath);
            string base64 = Convert.ToBase64String(imageBytes);
            string extension = Path.GetExtension(imagePath).TrimStart('.').ToLower();
            string mimeType = extension == "jpg" ? "image/jpeg" : $"image/{extension}";
            string dataUri = $"data:{mimeType};base64,{base64}";

            var payload = new
            {
                model = MODEL,
                max_tokens = 1024,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "image_url", image_url = new { url = dataUri } },
                            new { type = "text", text =
                                "Analyze this image in detail. Describe the style, composition, " +
                                "colors, lighting, subjects, background, and mood. " +
                                "Then generate a detailed image generation prompt that would " +
                                "recreate a similar image without any copyrighted elements. " +
                                "Format your response as: ANALYSIS: ... PROMPT: ..." }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BASE_URL}/chat/completions", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine("AnalyzeImage raw: " + responseBody);

            using var doc = JsonDocument.Parse(responseBody);

            var messageContent = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content");

            // Gemini can return content as string or array
            if (messageContent.ValueKind == JsonValueKind.String)
                return messageContent.GetString() ?? "No response.";

            // If array, extract all text parts
            if (messageContent.ValueKind == JsonValueKind.Array)
            {
                var sb = new StringBuilder();
                foreach (var part in messageContent.EnumerateArray())
                {
                    if (part.TryGetProperty("type", out var typeEl) && typeEl.GetString() == "text")
                        sb.AppendLine(part.GetProperty("text").GetString());
                }
                return sb.ToString().Trim();
            }

            return "No response.";
        }

        public async Task<byte[]> GenerateImageAsync(string prompt)
        {
            var payload = new
            {
                model = MODEL,
                max_tokens = 2048,
                messages = new[]
                {
            new { role = "user", content = prompt }
        }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{BASE_URL}/chat/completions", content);
            var responseBody = await response.Content.ReadAsStringAsync();

            // Search raw response for base64 image data directly
            // More reliable than JSON parsing since Gemini nests it differently
            const string marker = "data:image/";
            int start = responseBody.IndexOf(marker);
            if (start == -1)
                throw new Exception("No image data found in response: " + responseBody);

            // Find closing quote
            int end = responseBody.IndexOf("\"", start);
            if (end == -1)
                throw new Exception("Could not find end of image data.");

            string dataUri = responseBody.Substring(start, end - start);

            // Extract base64 part after the comma
            int commaIndex = dataUri.IndexOf(",");
            if (commaIndex == -1)
                throw new Exception("Invalid data URI format.");

            string base64 = dataUri.Substring(commaIndex + 1);
            return Convert.FromBase64String(base64);
        }

    }
}