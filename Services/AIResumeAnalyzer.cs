using System.Net.Http.Json;
using System.Text.Json;

namespace ATSklar.Services;

/// <summary>
/// AI-powered resume analysis using local LLM (Ollama/LMStudio)
/// </summary>
public class AIResumeAnalyzer
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _model;
    private readonly ILogger<AIResumeAnalyzer> _logger;

    public class TailoredResumeRequest
    {
        public string CompanyName { get; set; } = string.Empty;
        public string TargetRole { get; set; } = string.Empty;
        public string JobDescription { get; set; } = string.Empty;
        public string CompanyResearch { get; set; } = string.Empty;
    }

    public class TailoredResumeResult
    {
        public string Headline { get; set; } = string.Empty;
        public string ProfessionalSummary { get; set; } = string.Empty;
        public List<string> ATSKeywords { get; set; } = new();
        public List<string> CompanyFitHighlights { get; set; } = new();
        public List<string> RewrittenBullets { get; set; } = new();
        public string FullResumeDraft { get; set; } = string.Empty;
    }

    public AIResumeAnalyzer(HttpClient httpClient, IConfiguration config, ILogger<AIResumeAnalyzer> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = config["AI:OllamaUrl"] ?? "http://localhost:11434";
        _model = config["AI:Model"] ?? "phi4";
    }

    public async Task<bool> IsAIAvailableAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<TailoredResumeResult> TailorResumeAsync(string resumeText, TailoredResumeRequest request)
    {
        var prompt = $@"You are an ATS resume strategist.
Use only facts already present in the current resume, target role, and company research.
Do not invent employers, dates, degrees, certifications, or metrics.
Make the resume easier for ATS systems, recruiters, and AI screeners to parse.

TARGET ROLE:
{request.TargetRole}

TARGET COMPANY:
{request.CompanyName}

COMPANY RESEARCH:
{request.CompanyResearch}

JOB DESCRIPTION:
{request.JobDescription}

CURRENT RESUME:
{resumeText}

Return the answer using exactly these sections:
HEADLINE:
[one line]

SUMMARY:
[2-4 sentence summary]

ATS_KEYWORDS:
- keyword 1
- keyword 2
- keyword 3

COMPANY_FIT:
- point 1
- point 2
- point 3

REWRITTEN_BULLETS:
- bullet 1
- bullet 2
- bullet 3
- bullet 4

FULL_RESUME:
[plain text ATS-friendly resume draft with standard sections like SUMMARY, SKILLS, EXPERIENCE, EDUCATION]";

        var response = await CallOllamaAsync(prompt, 1400);
        return ParseTailoredResume(response);
    }

    private async Task<string> CallOllamaAsync(string prompt, int maxTokens = 500)
    {
        try
        {
            var requestBody = new
            {
                model = _model,
                prompt,
                stream = false,
                options = new
                {
                    temperature = 0.4,
                    top_p = 0.9,
                    top_k = 40,
                    num_predict = maxTokens
                }
            };

            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/api/generate", requestBody);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ollama returned status: {StatusCode}", response.StatusCode);
                return string.Empty;
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(content);

            if (jsonResponse.TryGetProperty("response", out var responseText))
            {
                return responseText.GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to Ollama. Make sure it's running on {BaseUrl}", _baseUrl);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Ollama API");
            return string.Empty;
        }
    }

    private TailoredResumeResult ParseTailoredResume(string response)
    {
        return new TailoredResumeResult
        {
            Headline = ExtractSingleLineSection(response, "HEADLINE:"),
            ProfessionalSummary = ExtractMultilineSection(response, "SUMMARY:", new[] { "ATS_KEYWORDS:", "COMPANY_FIT:", "REWRITTEN_BULLETS:", "FULL_RESUME:" }),
            ATSKeywords = ExtractList(response, "ATS_KEYWORDS:"),
            CompanyFitHighlights = ExtractList(response, "COMPANY_FIT:"),
            RewrittenBullets = ExtractList(response, "REWRITTEN_BULLETS:"),
            FullResumeDraft = ExtractMultilineSection(response, "FULL_RESUME:", Array.Empty<string>())
        };
    }

    private List<string> ExtractList(string text, string section)
    {
        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var result = new List<string>();
        var foundSection = false;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (line.Equals(section, StringComparison.OrdinalIgnoreCase))
            {
                foundSection = true;
                continue;
            }

            if (!foundSection)
            {
                continue;
            }

            if (line.EndsWith(":", StringComparison.OrdinalIgnoreCase) && !line.Equals(section, StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            if (line.StartsWith("-") || line.StartsWith("*"))
            {
                var item = line.TrimStart('-', '*', ' ');
                if (!string.IsNullOrWhiteSpace(item))
                {
                    result.Add(item);
                }
            }
        }

        return result;
    }

    private static string ExtractSingleLineSection(string text, string section)
    {
        var value = ExtractMultilineSection(text, section, new[] { "SUMMARY:", "ATS_KEYWORDS:", "COMPANY_FIT:", "REWRITTEN_BULLETS:", "FULL_RESUME:" });
        return value.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
    }

    private static string ExtractMultilineSection(string text, string section, string[] nextSections)
    {
        var startIndex = text.IndexOf(section, StringComparison.OrdinalIgnoreCase);
        if (startIndex < 0)
        {
            return string.Empty;
        }

        startIndex += section.Length;
        var remaining = text[startIndex..].Trim();
        var endIndex = remaining.Length;

        foreach (var nextSection in nextSections)
        {
            if (string.IsNullOrWhiteSpace(nextSection))
            {
                continue;
            }

            var nextIndex = remaining.IndexOf(nextSection, StringComparison.OrdinalIgnoreCase);
            if (nextIndex >= 0 && nextIndex < endIndex)
            {
                endIndex = nextIndex;
            }
        }

        return remaining[..endIndex].Trim();
    }
}
