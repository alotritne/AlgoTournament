using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace algotournament.Services
{
    public interface ITranslationService
    {
        Task<string> TranslateTextAsync(string text, string fromLanguage = "vi", string toLanguage = "en");
        Task<Dictionary<string, string>> TranslateProblemContentAsync(Dictionary<string, string> vietnameseContent);
    }

    public class TranslationService : ITranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TranslationService> _logger;
        private readonly string? _apiKey;

        public TranslationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<TranslationService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _apiKey = _configuration["Translation:ApiKey"];
        }

        public async Task<string> TranslateTextAsync(string text, string fromLanguage = "vi", string toLanguage = "en")
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            try
            {
                _logger.LogInformation("Starting translation using Google Translate free API");

                // Google Translate free API endpoint
                var encodedText = Uri.EscapeDataString(text);
                var url = $"https://clients5.google.com/translate_a/t?client=dict-chrome-ex&sl={fromLanguage}&tl={toLanguage}&q={encodedText}";

                _logger.LogInformation("Request URL: {Url}", url);

                var response = await _httpClient.GetAsync(url);
                _logger.LogInformation("Response status: {StatusCode}", response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Response content: {Content}", responseContent);
                    
                    // Google Translate returns nested array: [["translated text", "detected_language"]]
                    try
                    {
                        var result = JsonSerializer.Deserialize<List<List<string>>>(responseContent);
                        
                        if (result != null && result.Count > 0 && result[0].Count > 0)
                        {
                            var translation = result[0][0];
                            _logger.LogInformation("Translation successful: {Translation}", translation);
                            return translation;
                        }
                        
                        _logger.LogWarning("Parsed result is null or empty");
                    }
                    catch (JsonException jsonEx)
                    {
                        _logger.LogError(jsonEx, "Failed to parse JSON response");
                        
                        // Try alternative parsing - response might be just a string
                        try
                        {
                            var simpleResult = JsonSerializer.Deserialize<List<string>>(responseContent);
                            if (simpleResult != null && simpleResult.Count > 0)
                            {
                                _logger.LogInformation("Translation successful (simple format): {Translation}", simpleResult[0]);
                                return simpleResult[0];
                            }
                        }
                        catch
                        {
                            _logger.LogWarning("Alternative parsing also failed");
                        }
                    }
                    
                    _logger.LogWarning("Could not parse translation from response");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Google Translate API failed: {StatusCode}, Content: {Content}", 
                        response.StatusCode, errorContent);
                }

                // Fallback to original
                _logger.LogWarning("Translation failed, returning original text");
                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error using Google Translate: {Message}", ex.Message);
                return text;
            }
        }

        public async Task<Dictionary<string, string>> TranslateProblemContentAsync(Dictionary<string, string> vietnameseContent)
        {
            var translatedContent = new Dictionary<string, string>();

            foreach (var kvp in vietnameseContent)
            {
                var fieldName = kvp.Key;
                var vietnameseText = kvp.Value;

                if (string.IsNullOrWhiteSpace(vietnameseText))
                {
                    translatedContent[fieldName] = "";
                    continue;
                }

                // Translate each field
                var englishText = await TranslateTextAsync(vietnameseText, "vi", "en");
                translatedContent[fieldName] = englishText;
            }

            return translatedContent;
        }
    }
}
