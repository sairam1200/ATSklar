using System.Text.RegularExpressions;

namespace ATSklar.Services;

public class ATSPlacementSuggestion
{
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
}

/// <summary>
/// ATS Keyword Optimizer - Extracts keywords from job descriptions and optimizes resume content
/// </summary>
public class ATSKeywordOptimizer
{
    public class KeywordAnalysis
    {
        public List<string> ExtractedKeywords { get; set; } = new();
        public Dictionary<string, int> KeywordFrequency { get; set; } = new();
        public List<string> TechnicalSkills { get; set; } = new();
        public List<string> SoftSkills { get; set; } = new();
        public List<string> KeywordsInResume { get; set; } = new();
        public List<string> MissingKeywordsFromJob { get; set; } = new();
        public int MatchPercentage { get; set; }
        public List<string> KeywordVariations { get; set; } = new();
    }

    private static readonly Dictionary<string, List<string>> KeywordVariations = new()
    {
        { "python", new() { "python", "py", "python3" } },
        { "csharp", new() { "csharp", "c#", ".net" } },
        { "dotnet", new() { ".net", "dotnet", "asp.net", "aspnet" } },
        { "javascript", new() { "javascript", "js", "node.js", "nodejs", "react", "angular", "vue" } },
        { "database", new() { "database", "db", "sql", "mongodb", "postgresql", "mysql" } },
        { "aws", new() { "aws", "amazon web services", "s3", "ec2", "lambda" } },
        { "azure", new() { "azure", "microsoft azure", "cosmos", "sql azure" } },
        { "devops", new() { "devops", "ci/cd", "jenkins", "docker", "kubernetes" } },
        { "agile", new() { "agile", "scrum", "sprint", "kanban" } },
        { "leadership", new() { "leadership", "led", "managed", "directed", "supervised" } }
    };

    /// <summary>
    /// Extract and analyze keywords from job description
    /// </summary>
    public static KeywordAnalysis AnalyzeJobDescription(string jobDescription, string? resumeText = null)
    {
        var analysis = new KeywordAnalysis();
        var cleanedJob = NormalizeText(jobDescription);
        var cleanedResume = string.IsNullOrEmpty(resumeText) ? string.Empty : NormalizeText(resumeText);

        analysis.ExtractedKeywords = ExtractKeywords(cleanedJob);
        analysis.KeywordFrequency = CalculateFrequency(analysis.ExtractedKeywords);

        SeparateSkills(analysis.ExtractedKeywords, analysis);

        if (!string.IsNullOrEmpty(cleanedResume))
        {
            var resumeKeywords = ExtractKeywords(cleanedResume);
            analysis.KeywordsInResume = analysis.ExtractedKeywords
                .Where(keyword => resumeKeywords.Any(resumeKeyword => resumeKeyword.Equals(keyword, StringComparison.OrdinalIgnoreCase)))
                .Distinct()
                .ToList();

            analysis.MissingKeywordsFromJob = analysis.ExtractedKeywords
                .Except(analysis.KeywordsInResume, StringComparer.OrdinalIgnoreCase)
                .Distinct()
                .ToList();

            analysis.MatchPercentage = analysis.ExtractedKeywords.Count > 0
                ? (analysis.KeywordsInResume.Count * 100) / analysis.ExtractedKeywords.Count
                : 0;
        }

        analysis.KeywordVariations = GenerateVariations(analysis.MissingKeywordsFromJob);
        return analysis;
    }

    private static List<string> ExtractKeywords(string text)
    {
        var keywords = new List<string>();
        var textLower = text.ToLower();

        var technicalTerms = new[]
        {
            "python", "java", "csharp", "c#", "javascript", "js", "typescript",
            "react", "angular", "vue", "node.js", "nodejs", ".net", "aspnet", "asp.net",
            "sql", "mysql", "postgresql", "mongodb", "nosql", "redis",
            "docker", "kubernetes", "aws", "azure", "gcp", "cloud",
            "git", "github", "gitlab", "devops", "ci/cd", "jenkins", "gitlab-ci",
            "rest", "restful", "api", "graphql", "soap",
            "agile", "scrum", "kanban", "sprint", "jira",
            "html", "css", "sass", "less", "webpack",
            "testing", "unit test", "integration test", "pytest", "jest",
            "linux", "windows", "unix", "macos",
            "microservices", "monolithic", "architecture", "design patterns",
            "message queue", "rabbitmq", "kafka", "activemq"
        };

        var softSkills = new[]
        {
            "leadership", "communication", "collaboration", "teamwork",
            "problem-solving", "critical thinking", "analytical",
            "time management", "organization", "project management",
            "customer service", "attention to detail", "adaptability"
        };

        foreach (var term in technicalTerms)
        {
            if (textLower.Contains(term))
            {
                keywords.Add(term);
            }
        }

        foreach (var skill in softSkills)
        {
            if (textLower.Contains(skill))
            {
                keywords.Add(skill);
            }
        }

        var yearsMatch = Regex.Match(text, @"(\d+)\+?\s*years?");
        if (yearsMatch.Success)
        {
            keywords.Add($"{yearsMatch.Groups[1].Value} years experience");
        }

        return keywords.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static Dictionary<string, int> CalculateFrequency(List<string> keywords)
    {
        return keywords
            .GroupBy(keyword => keyword, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Count());
    }

    private static void SeparateSkills(List<string> keywords, KeywordAnalysis analysis)
    {
        var softSkillsList = new[]
        {
            "leadership", "communication", "collaboration", "teamwork",
            "problem-solving", "critical thinking", "analytical",
            "time management", "organization", "customer service"
        };

        foreach (var keyword in keywords)
        {
            if (softSkillsList.Any(skill => keyword.Contains(skill, StringComparison.OrdinalIgnoreCase)))
            {
                analysis.SoftSkills.Add(keyword);
            }
            else
            {
                analysis.TechnicalSkills.Add(keyword);
            }
        }
    }

    private static List<string> GenerateVariations(List<string> keywords)
    {
        var variations = new List<string>();

        foreach (var keyword in keywords)
        {
            var keyLower = keyword.ToLower();
            foreach (var variationGroup in KeywordVariations)
            {
                if (variationGroup.Value.Any(variation => keyLower.Contains(variation)))
                {
                    variations.AddRange(variationGroup.Value);
                }
            }
        }

        return variations.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string NormalizeText(string text)
    {
        return text.Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
    }

    public static List<ATSPlacementSuggestion> GeneratePlacementSuggestions(
        KeywordAnalysis analysis,
        string resumeText,
        string? targetRole = null,
        string? companyName = null)
    {
        var suggestions = new List<ATSPlacementSuggestion>();
        var missingKeywords = analysis.MissingKeywordsFromJob
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (missingKeywords.Count == 0)
        {
            return suggestions;
        }

        var technicalKeywords = missingKeywords
            .Where(keyword => analysis.TechnicalSkills.Contains(keyword, StringComparer.OrdinalIgnoreCase))
            .Take(6)
            .ToList();

        var softKeywords = missingKeywords
            .Where(keyword => analysis.SoftSkills.Contains(keyword, StringComparer.OrdinalIgnoreCase))
            .Take(4)
            .ToList();

        var summaryKeywords = missingKeywords.Take(3).ToList();

        suggestions.Add(new ATSPlacementSuggestion
        {
            Title = "Strengthen the top summary",
            Location = ResolveSectionLocation(resumeText, "summary"),
            Reason = BuildSummaryReason(targetRole, companyName, summaryKeywords),
            Impact = "Adding the most relevant terms near the top can improve first-pass ATS matching.",
            Keywords = summaryKeywords
        });

        if (technicalKeywords.Count > 0)
        {
            suggestions.Add(new ATSPlacementSuggestion
            {
                Title = "Expand the skills section",
                Location = ResolveSectionLocation(resumeText, "skills"),
                Reason = "Place missing technical keywords in a dedicated skills block if you genuinely have them.",
                Impact = "A clearer skills section usually lifts keyword coverage faster than rewriting every bullet.",
                Keywords = technicalKeywords
            });
        }

        suggestions.Add(new ATSPlacementSuggestion
        {
            Title = "Rewrite one experience bullet",
            Location = ResolveSectionLocation(resumeText, "experience"),
            Reason = "Update the most relevant recent bullet with exact tools, methods, or outcomes from the job description.",
            Impact = "Experience bullets carry more weight because they show evidence, not just keyword presence.",
            Keywords = missingKeywords.Take(4).ToList()
        });

        if (softKeywords.Count > 0)
        {
            suggestions.Add(new ATSPlacementSuggestion
            {
                Title = "Show soft skills through results",
                Location = ResolveSectionLocation(resumeText, "experience"),
                Reason = "Convert soft skills into proof points such as leading, collaborating, or communicating in a measurable outcome.",
                Impact = "Demonstrated soft skills can help match screening criteria without sounding generic.",
                Keywords = softKeywords
            });
        }

        return suggestions
            .Where(suggestion => suggestion.Keywords.Count > 0)
            .ToList();
    }

    private static string ResolveSectionLocation(string resumeText, string sectionType)
    {
        var hasSummary = Regex.IsMatch(resumeText, @"(?im)^\s*(professional\s+summary|summary|profile)\s*$");
        var hasSkills = Regex.IsMatch(resumeText, @"(?im)^\s*(technical\s+skills|skills|core\s+competencies)\s*$");
        var hasExperience = Regex.IsMatch(resumeText, @"(?im)^\s*(experience|work\s+experience|professional\s+experience|employment)\s*$");

        return sectionType switch
        {
            "summary" when hasSummary => "Professional Summary section",
            "summary" => "Add a Professional Summary section below your name",
            "skills" when hasSkills => "Skills or Core Competencies section",
            "skills" => "Add a Skills section near the top of the resume",
            "experience" when hasExperience => "Most relevant bullet in Work Experience",
            "experience" => "Most relevant achievement bullet under Work Experience",
            _ => "Top third of the resume"
        };
    }

    private static string BuildSummaryReason(string? targetRole, string? companyName, List<string> keywords)
    {
        var contextParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(targetRole))
        {
            contextParts.Add(targetRole);
        }

        if (!string.IsNullOrWhiteSpace(companyName))
        {
            contextParts.Add(companyName);
        }

        var context = contextParts.Count > 0
            ? $"Align the opening with {string.Join(" at ", contextParts)}"
            : "Align the opening with the job description";

        return keywords.Count == 0
            ? context + "."
            : $"{context} and naturally mention {string.Join(", ", keywords)}.";
    }
}
