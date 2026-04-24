using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ATSklar.Services;

public class CompanyResearchService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CompanyResearchService> _logger;

    public class ResearchSource
    {
        public string Label { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }

    public class CompanyResearchResult
    {
        public string CompanyName { get; set; } = string.Empty;
        public string TargetRole { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public List<string> BusinessSignals { get; set; } = new();
        public List<string> ATSFocusAreas { get; set; } = new();
        public List<ResearchSource> Sources { get; set; } = new();
    }

    public CompanyResearchService(HttpClient httpClient, ILogger<CompanyResearchService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ATSklar/1.0");
        }

        _httpClient.Timeout = TimeSpan.FromSeconds(12);
    }

    public async Task<CompanyResearchResult> ResearchCompanyAsync(string companyName, string targetRole, string? companyWebsite = null)
    {
        var result = new CompanyResearchResult
        {
            CompanyName = companyName.Trim(),
            TargetRole = targetRole.Trim(),
            Website = companyWebsite?.Trim() ?? string.Empty
        };

        var overviewParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(companyWebsite))
        {
            var websiteSummary = await TryGetWebsiteSummaryAsync(companyWebsite);
            if (!string.IsNullOrWhiteSpace(websiteSummary))
            {
                overviewParts.Add(websiteSummary);
                result.Sources.Add(new ResearchSource { Label = "Company website", Url = NormalizeWebsite(companyWebsite) });
            }
        }

        var wikipediaSummary = await TryGetWikipediaSummaryAsync(companyName);
        if (!string.IsNullOrWhiteSpace(wikipediaSummary))
        {
            overviewParts.Add(wikipediaSummary);
            result.Sources.Add(new ResearchSource
            {
                Label = "Wikipedia summary",
                Url = $"https://en.wikipedia.org/wiki/{Uri.EscapeDataString(companyName.Replace(' ', '_'))}"
            });
        }

        var searchSummary = await TryGetSearchSummaryAsync(companyName, targetRole);
        if (!string.IsNullOrWhiteSpace(searchSummary))
        {
            overviewParts.Add(searchSummary);
            result.Sources.Add(new ResearchSource
            {
                Label = "DuckDuckGo instant answer",
                Url = $"https://duckduckgo.com/?q={Uri.EscapeDataString($"{companyName} {targetRole}")}"
            });
        }

        result.Overview = string.Join(" ", overviewParts.Where(static item => !string.IsNullOrWhiteSpace(item)).Distinct());
        if (string.IsNullOrWhiteSpace(result.Overview))
        {
            result.Overview = $"{companyName} research was limited, so the tailoring will lean more heavily on the role title, job description, and resume evidence.";
        }

        result.BusinessSignals = BuildBusinessSignals(result.Overview, companyName, targetRole);
        result.ATSFocusAreas = BuildATSFocusAreas(result.Overview, targetRole);

        return result;
    }

    private async Task<string> TryGetWikipediaSummaryAsync(string companyName)
    {
        try
        {
            var searchUrl = $"https://en.wikipedia.org/w/api.php?action=opensearch&search={Uri.EscapeDataString(companyName)}&limit=1&namespace=0&format=json";
            using var searchResponse = await _httpClient.GetAsync(searchUrl);
            if (!searchResponse.IsSuccessStatusCode)
            {
                return string.Empty;
            }

            var searchDoc = JsonDocument.Parse(await searchResponse.Content.ReadAsStringAsync());
            if (searchDoc.RootElement.GetArrayLength() < 2 || searchDoc.RootElement[1].GetArrayLength() == 0)
            {
                return string.Empty;
            }

            var title = searchDoc.RootElement[1][0].GetString();
            if (string.IsNullOrWhiteSpace(title))
            {
                return string.Empty;
            }

            var summaryUrl = $"https://en.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(title)}";
            using var summaryResponse = await _httpClient.GetAsync(summaryUrl);
            if (!summaryResponse.IsSuccessStatusCode)
            {
                return string.Empty;
            }

            var summaryDoc = JsonDocument.Parse(await summaryResponse.Content.ReadAsStringAsync());
            return summaryDoc.RootElement.TryGetProperty("extract", out var extract)
                ? extract.GetString() ?? string.Empty
                : string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Wikipedia lookup failed for {CompanyName}", companyName);
            return string.Empty;
        }
    }

    private async Task<string> TryGetSearchSummaryAsync(string companyName, string targetRole)
    {
        try
        {
            var url = $"https://api.duckduckgo.com/?q={Uri.EscapeDataString($"{companyName} {targetRole} company")}&format=json&no_html=1&skip_disambig=1";
            using var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }

            var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement.TryGetProperty("AbstractText", out var abstractText)
                ? abstractText.GetString() ?? string.Empty
                : string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Search lookup failed for {CompanyName}", companyName);
            return string.Empty;
        }
    }

    private async Task<string> TryGetWebsiteSummaryAsync(string companyWebsite)
    {
        try
        {
            using var response = await _httpClient.GetAsync(NormalizeWebsite(companyWebsite));
            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;
            }

            var html = await response.Content.ReadAsStringAsync();
            var title = Regex.Match(html, "<title>(.*?)</title>", RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;
            var description = Regex.Match(html, "<meta\\s+name=[\"']description[\"']\\s+content=[\"'](.*?)[\"']", RegexOptions.IgnoreCase | RegexOptions.Singleline).Groups[1].Value;

            return string.Join(" ", new[]
            {
                WebUtility.HtmlDecode(StripHtml(title)),
                WebUtility.HtmlDecode(StripHtml(description))
            }.Where(static item => !string.IsNullOrWhiteSpace(item)));
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Website lookup failed for {Website}", companyWebsite);
            return string.Empty;
        }
    }

    private static List<string> BuildBusinessSignals(string overview, string companyName, string targetRole)
    {
        var lower = overview.ToLowerInvariant();
        var signals = new List<string>();

        if (lower.Contains("cloud") || lower.Contains("saas"))
        {
            signals.Add("Public messaging suggests a cloud or SaaS business model.");
        }

        if (lower.Contains("artificial intelligence") || lower.Contains("ai") || lower.Contains("machine learning"))
        {
            signals.Add("AI or machine learning appears in the company background.");
        }

        if (lower.Contains("platform") || lower.Contains("product"))
        {
            signals.Add("Product or platform language is visible in the company overview.");
        }

        if (lower.Contains("global") || lower.Contains("international"))
        {
            signals.Add("The company presents itself as operating at broad scale.");
        }

        if (signals.Count == 0)
        {
            signals.Add($"Company research for {companyName} was limited, so ATS optimization should strongly mirror the {targetRole} role language.");
        }

        return signals;
    }

    private static List<string> BuildATSFocusAreas(string overview, string targetRole)
    {
        var lower = $"{overview} {targetRole}".ToLowerInvariant();
        var areas = new List<string>();

        if (lower.Contains("engineer") || lower.Contains("developer"))
        {
            areas.Add("Surface technical stack, shipped features, and measurable engineering outcomes.");
            areas.Add("Repeat core skill keywords in summary, skills, and experience sections.");
        }

        if (lower.Contains("manager") || lower.Contains("lead"))
        {
            areas.Add("Show leadership scope, stakeholder alignment, and delivery ownership.");
            areas.Add("Quantify team, project, timeline, or business impact where supported by the resume.");
        }

        if (areas.Count == 0)
        {
            areas.Add("Use standard ATS section headings and plain text formatting.");
            areas.Add("Prioritize direct verbs, outcome-driven bullets, and role-specific keywords.");
        }

        return areas.Distinct().ToList();
    }

    private static string NormalizeWebsite(string website)
    {
        return website.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? website
            : $"https://{website}";
    }

    private static string StripHtml(string input)
    {
        return Regex.Replace(input ?? string.Empty, "<.*?>", " ").Replace("\r", " ").Replace("\n", " ").Trim();
    }
}
