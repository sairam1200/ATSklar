using System.Text.RegularExpressions;

namespace ATSklar.Services;

public class ATSRepeatedWord
{
    public string Word { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<string> Alternatives { get; set; } = new();
}

public class ATSStepCheck
{
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = "Good";
    public int IssueCount { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<string> Details { get; set; } = new();
}

public class ATSDetailedReport
{
    public int TotalIssues { get; set; }
    public int ParseRate { get; set; }
    public List<string> ParsedContactFields { get; set; } = new();
    public List<string> EssentialSectionsFound { get; set; } = new();
    public List<string> EssentialSectionsMissing { get; set; } = new();
    public List<ATSRepeatedWord> RepeatedWords { get; set; } = new();
    public List<ATSStepCheck> Steps { get; set; } = new();
}

public class ATSIssue
{
    public string Severity { get; set; } = "Warning";
    public string Issue { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
}

public class ATSAnalysisResult
{
    public int ATSScore { get; set; }
    public List<ATSIssue> Issues { get; set; } = new();
    public Dictionary<string, int> KeywordFrequency { get; set; } = new();
    public List<string> MissingKeywords { get; set; } = new();
    public int PageCount { get; set; }
    public bool IsFormatted { get; set; }
    public string SummaryRecommendation { get; set; } = string.Empty;
    public ATSDetailedReport DetailedReport { get; set; } = new();
}

/// <summary>
/// ATS Resume Analyzer - rule-based evaluation for ATS readiness.
/// </summary>
public class ATSResumeAnalyzer
{
    private sealed class ResumeFacts
    {
        public string Text { get; set; } = string.Empty;
        public string LowerText { get; set; } = string.Empty;
        public List<string> Lines { get; set; } = new();
        public int WordCount { get; set; }
        public bool HasEmail { get; set; }
        public bool HasPhone { get; set; }
        public bool HasLinkedInOrPortfolio { get; set; }
        public bool HasFullUrl { get; set; }
        public bool HasSummary { get; set; }
        public bool HasEducation { get; set; }
        public bool HasExperience { get; set; }
        public bool HasProjects { get; set; }
        public bool HasSkills { get; set; }
        public string SummarySection { get; set; } = string.Empty;
        public string EducationSection { get; set; } = string.Empty;
        public string ExperienceSection { get; set; } = string.Empty;
        public string ProjectsSection { get; set; } = string.Empty;
        public string SkillsSection { get; set; } = string.Empty;
        public int BulletCount { get; set; }
        public int EvidenceBulletCount { get; set; }
        public int DateCount { get; set; }
        public int QuantifiedEvidenceCount { get; set; }
        public int ActionVerbCount { get; set; }
        public bool HasTableLikeLayout { get; set; }
        public bool HasMultiColumnSpacing { get; set; }
        public int UnusualCharacterCount { get; set; }
        public int LongLineCount { get; set; }
        public int PossibleGrammarFlags { get; set; }
        public List<string> ListedSkills { get; set; } = new();
        public List<string> SupportedSkills { get; set; } = new();
        public List<string> UnsupportedSkills { get; set; } = new();
        public int SkillAlignmentPercentage =>
            ListedSkills.Count == 0 ? 0 : (int)Math.Round(SupportedSkills.Count * 100.0 / ListedSkills.Count);
    }

    private sealed class ScoreCard
    {
        public int Score { get; private set; } = 100;

        public void Deduct(int points)
        {
            Score = Math.Max(0, Score - points);
        }
    }

    private static readonly string[] KnownSkills =
    {
        "python", "java", "c#", ".net", "asp.net", "javascript", "typescript", "react", "angular", "vue",
        "node.js", "sql", "mysql", "postgresql", "mongodb", "redis", "aws", "azure", "gcp", "docker",
        "kubernetes", "git", "ci/cd", "jenkins", "rest", "api", "microservices", "html", "css",
        "testing", "agile", "scrum", "leadership", "communication", "analysis", "excel", "power bi"
    };

    private static readonly string[] ActionVerbs =
    {
        "built", "developed", "designed", "implemented", "improved", "optimized", "created", "managed",
        "led", "delivered", "launched", "analyzed", "automated", "reduced", "increased", "migrated"
    };

    private static readonly Dictionary<string, string[]> RepetitionSuggestions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["developed"] = new[] { "enhanced", "expanded", "improved" },
        ["implemented"] = new[] { "executed", "applied", "delivered" },
        ["created"] = new[] { "established", "generated", "initiated" },
        ["enhanced"] = new[] { "improved", "upgraded", "fine-tuned" },
        ["managed"] = new[] { "coordinated", "oversaw", "led" },
        ["worked"] = new[] { "collaborated", "delivered", "contributed" }
    };

    private static readonly Dictionary<string, string[]> SectionAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["summary"] = new[] { "summary", "professional summary", "profile", "objective", "bio", "about" },
        ["education"] = new[] { "education", "study", "studies", "academic background", "qualifications" },
        ["experience"] = new[] { "experience", "work experience", "employment", "professional experience", "career history" },
        ["projects"] = new[] { "projects", "project experience", "key projects", "academic projects" },
        ["skills"] = new[] { "skills", "skill set", "technical skills", "core competencies", "technologies", "tools" }
    };

    public static ATSAnalysisResult AnalyzeResume(
        string resumeText,
        string? fileName = null,
        long? fileSizeBytes = null,
        string? contentType = null)
    {
        var result = new ATSAnalysisResult();
        var facts = BuildFacts(resumeText);

        result.PageCount = Math.Max(1, (int)Math.Ceiling(facts.WordCount / 450.0));
        result.IsFormatted = facts.HasTableLikeLayout || facts.HasMultiColumnSpacing || facts.UnusualCharacterCount > 0;

        PopulateKeywordData(result, facts);
        EvaluateRules(result, facts);

        result.ATSScore = CalculateScore(result, facts);
        result.SummaryRecommendation = GenerateSummary(result.ATSScore);
        result.DetailedReport = BuildDetailedReport(result, facts, fileName, fileSizeBytes, contentType);
        return result;
    }

    private static ResumeFacts BuildFacts(string resumeText)
    {
        var text = NormalizeText(resumeText);
        var lowerText = text.ToLowerInvariant();
        var lines = text.Split('\n', StringSplitOptions.None)
            .Select(static line => line.Trim())
            .Where(static line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var facts = new ResumeFacts
        {
            Text = text,
            LowerText = lowerText,
            Lines = lines,
            WordCount = Regex.Matches(text, @"\b[\w#+./-]+\b").Count,
            HasEmail = Regex.IsMatch(text, @"[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}", RegexOptions.IgnoreCase),
            HasPhone = Regex.IsMatch(text, @"(?:\+?\d{1,3}[\s.-]?)?(?:\(?\d{3}\)?[\s.-]?)\d{3}[\s.-]?\d{4}"),
            HasLinkedInOrPortfolio = Regex.IsMatch(text, @"linkedin\.com|github\.com|portfolio|behance\.net|dribbble\.com|https?://", RegexOptions.IgnoreCase),
            HasFullUrl = Regex.IsMatch(text, @"https?://\S+", RegexOptions.IgnoreCase),
            HasSummary = SectionAliases["summary"].Any(lowerText.Contains),
            HasEducation = SectionAliases["education"].Any(lowerText.Contains),
            HasExperience = SectionAliases["experience"].Any(lowerText.Contains),
            HasProjects = SectionAliases["projects"].Any(lowerText.Contains),
            HasSkills = SectionAliases["skills"].Any(lowerText.Contains),
            BulletCount = lines.Count(static line => Regex.IsMatch(line, @"^[-*]\s+|^[*]\s+|^[\u2022]\s+")),
            DateCount = Regex.Matches(text, @"\b(?:19|20)\d{2}\b|\b(?:jan|feb|mar|apr|may|jun|jul|aug|sep|sept|oct|nov|dec)[a-z]*\s+(?:19|20)\d{2}\b", RegexOptions.IgnoreCase).Count,
            QuantifiedEvidenceCount = Regex.Matches(text, @"\b\d+%|\b\d+\+|\$\d+|\b\d+\s*(users|customers|clients|projects|team|months|years|hours)\b", RegexOptions.IgnoreCase).Count,
            ActionVerbCount = ActionVerbs.Sum(verb => CountOccurrences(lowerText, verb)),
            HasTableLikeLayout = lines.Count(static line => (line.Contains('|') && line.Count(static c => c == '|') >= 2) || line.Contains('\t')) >= 3,
            HasMultiColumnSpacing = lines.Count(static line => line.Length > 45 && Regex.IsMatch(line, @"\S\s{3,}\S")) >= 5,
            UnusualCharacterCount = CountUnusualCharacters(text),
            LongLineCount = lines.Count(static line => line.Length > 140),
            PossibleGrammarFlags = CountGrammarFlags(text)
        };

        facts.SummarySection = ExtractSection(text, SectionAliases["summary"]);
        facts.EducationSection = ExtractSection(text, SectionAliases["education"]);
        facts.ExperienceSection = ExtractSection(text, SectionAliases["experience"]);
        facts.ProjectsSection = ExtractSection(text, SectionAliases["projects"]);
        facts.SkillsSection = ExtractSection(text, SectionAliases["skills"]);

        var evidenceSection = $"{facts.ExperienceSection}\n{facts.ProjectsSection}";
        facts.EvidenceBulletCount = evidenceSection.Split('\n', StringSplitOptions.None)
            .Count(static line => Regex.IsMatch(line.Trim(), @"^[-*]\s+|^[\u2022]\s+"));

        facts.ListedSkills = ExtractListedSkills(facts);
        foreach (var skill in facts.ListedSkills)
        {
            if (evidenceSection.Contains(skill, StringComparison.OrdinalIgnoreCase))
            {
                facts.SupportedSkills.Add(skill);
            }
            else
            {
                facts.UnsupportedSkills.Add(skill);
            }
        }

        return facts;
    }

    private static void PopulateKeywordData(ATSAnalysisResult result, ResumeFacts facts)
    {
        foreach (var skill in KnownSkills)
        {
            var count = CountOccurrences(facts.LowerText, skill);
            if (count > 0)
            {
                result.KeywordFrequency[skill] = count;
            }
        }

        result.MissingKeywords = facts.UnsupportedSkills
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(8)
            .ToList();
    }

    private static void EvaluateRules(ATSAnalysisResult result, ResumeFacts facts)
    {
        if (!facts.HasEmail || !facts.HasPhone)
        {
            result.Issues.Add(new ATSIssue
            {
                Severity = "Critical",
                Issue = "Contact information is incomplete or hard to detect",
                Recommendation = "Include both a visible email address and phone number near the top."
            });
        }

        if (!facts.HasSummary)
        {
            result.Issues.Add(new ATSIssue
            {
                Severity = "Info",
                Issue = "Summary or bio section is not clearly labeled",
                Recommendation = "Add a short Summary, Profile, or Bio section near the top."
            });
        }

        if (!facts.HasEducation)
        {
            result.Issues.Add(new ATSIssue
            {
                Severity = "Warning",
                Issue = "Education or study section is not clearly labeled",
                Recommendation = "Use a clear Education section heading even if the section is short."
            });
        }

        if (!facts.HasExperience && !facts.HasProjects)
        {
            result.Issues.Add(new ATSIssue
            {
                Severity = "Critical",
                Issue = "Experience or projects section is not clearly labeled",
                Recommendation = "Add a clear Experience or Projects heading to anchor your practical work."
            });
        }

        if (!facts.HasSkills)
        {
            result.Issues.Add(new ATSIssue
            {
                Severity = "Warning",
                Issue = "Skills section is not clearly labeled",
                Recommendation = "Add a Skills or Skill Set section so ATS systems can detect core tools quickly."
            });
        }

        if (facts.BulletCount < 4)
        {
            result.Issues.Add(new ATSIssue
            {
                Severity = "Info",
                Issue = "Resume uses very few bullet points",
                Recommendation = "Use bullet points for experience and projects to improve scanning."
            });
        }

        if (facts.DateCount < 2)
        {
            result.Issues.Add(new ATSIssue
            {
                Severity = "Warning",
                Issue = "Work or education timeline is not clearly visible",
                Recommendation = "Add date ranges for work, projects, and education."
            });
        }

        if (facts.QuantifiedEvidenceCount < 2)
        {
            result.Issues.Add(new ATSIssue
            {
                Severity = "Info",
                Issue = "Achievements are not strongly quantified",
                Recommendation = "Add measurable results such as percentages, counts, or business impact."
            });
        }

        if (facts.ActionVerbCount < 4)
        {
            result.Issues.Add(new ATSIssue
            {
                Severity = "Info",
                Issue = "Experience bullets use limited action-oriented language",
                Recommendation = "Start bullets with strong verbs like built, led, improved, or implemented."
            });
        }

        if (facts.ListedSkills.Count >= 4 && facts.SkillAlignmentPercentage < 60)
        {
            result.Issues.Add(new ATSIssue
            {
                Severity = "Warning",
                Issue = "Many listed skills are not supported by experience or project descriptions",
                Recommendation = "Reflect the listed skills inside experience or project bullets."
            });
        }

        if (facts.HasTableLikeLayout)
        {
            result.Issues.Add(new ATSIssue
            {
                Severity = "Critical",
                Issue = "Document appears to rely on tables or tabbed layout",
                Recommendation = "Use a simple single-column structure with plain section headings."
            });
        }

        if (facts.HasMultiColumnSpacing)
        {
            result.Issues.Add(new ATSIssue
            {
                Severity = "Warning",
                Issue = "Document may use multi-column spacing",
                Recommendation = "Keep the resume in a simple single-column reading order."
            });
        }

        if (facts.UnusualCharacterCount >= 4)
        {
            result.Issues.Add(new ATSIssue
            {
                Severity = "Info",
                Issue = "Decorative symbols may reduce parser accuracy",
                Recommendation = "Prefer standard bullets like '-' or '*'."
            });
        }

        if (Math.Max(1, (int)Math.Ceiling(facts.WordCount / 450.0)) > 2)
        {
            result.Issues.Add(new ATSIssue
            {
                Severity = "Info",
                Issue = "Resume may be longer than two pages",
                Recommendation = "Keep the strongest and most relevant content near the beginning."
            });
        }
    }

    private static int CalculateScore(ATSAnalysisResult result, ResumeFacts facts)
    {
        var score = new ScoreCard();

        ApplyContactRules(score, facts);
        ApplySectionRules(score, facts);
        ApplyExperienceRules(score, facts);
        ApplySkillsRules(score, facts);
        ApplyFormattingRules(score, facts);
        ApplyConsistencyRules(score, facts);

        // Avoid handing out perfect scores too easily.
        if (score.Score > 96)
        {
            score.Deduct(2);
        }

        return score.Score;
    }

    private static void ApplyContactRules(ScoreCard score, ResumeFacts facts)
    {
        if (!facts.HasEmail)
        {
            score.Deduct(8);
        }

        if (!facts.HasPhone)
        {
            score.Deduct(7);
        }

        if (!facts.HasLinkedInOrPortfolio)
        {
            score.Deduct(2);
        }
    }

    private static void ApplySectionRules(ScoreCard score, ResumeFacts facts)
    {
        if (!facts.HasSummary)
        {
            score.Deduct(6);
        }
        else if (WordCount(facts.SummarySection) < 20)
        {
            score.Deduct(2);
        }

        if (!facts.HasEducation)
        {
            score.Deduct(10);
        }

        if (!facts.HasExperience && !facts.HasProjects)
        {
            score.Deduct(16);
        }

        if (!facts.HasSkills)
        {
            score.Deduct(8);
        }
    }

    private static void ApplyExperienceRules(ScoreCard score, ResumeFacts facts)
    {
        var evidenceText = $"{facts.ExperienceSection}\n{facts.ProjectsSection}";
        if (string.IsNullOrWhiteSpace(evidenceText))
        {
            score.Deduct(18);
            return;
        }

        if (facts.EvidenceBulletCount < 3)
        {
            score.Deduct(6);
        }

        if (facts.DateCount < 2)
        {
            score.Deduct(6);
        }

        if (facts.ActionVerbCount < 4)
        {
            score.Deduct(5);
        }

        if (facts.QuantifiedEvidenceCount == 0)
        {
            score.Deduct(6);
        }
        else if (facts.QuantifiedEvidenceCount == 1)
        {
            score.Deduct(3);
        }

        if (WordCount(evidenceText) < 80)
        {
            score.Deduct(4);
        }
    }

    private static void ApplySkillsRules(ScoreCard score, ResumeFacts facts)
    {
        if (!facts.HasSkills)
        {
            return;
        }

        if (facts.ListedSkills.Count == 0)
        {
            score.Deduct(5);
            return;
        }

        if (facts.ListedSkills.Count < 4)
        {
            score.Deduct(2);
        }

        score.Deduct(facts.SkillAlignmentPercentage switch
        {
            >= 90 => 0,
            >= 75 => 2,
            >= 60 => 5,
            >= 45 => 8,
            _ => 12
        });
    }

    private static void ApplyFormattingRules(ScoreCard score, ResumeFacts facts)
    {
        if (facts.HasTableLikeLayout)
        {
            score.Deduct(14);
        }

        if (facts.HasMultiColumnSpacing)
        {
            score.Deduct(6);
        }

        if (facts.UnusualCharacterCount >= 4)
        {
            score.Deduct(2);
        }

        var pageCount = Math.Max(1, (int)Math.Ceiling(facts.WordCount / 450.0));
        if (pageCount > 2)
        {
            score.Deduct(pageCount == 3 ? 4 : 7);
        }
    }

    private static void ApplyConsistencyRules(ScoreCard score, ResumeFacts facts)
    {
        if (facts.WordCount < 180)
        {
            score.Deduct(10);
        }
        else if (facts.WordCount < 260)
        {
            score.Deduct(5);
        }

        if (facts.WordCount > 1100)
        {
            score.Deduct(6);
        }

        if (facts.HasSummary && facts.HasSkills && facts.ListedSkills.Count > 0 && facts.ActionVerbCount < 3)
        {
            score.Deduct(3);
        }

        if (facts.HasSkills && facts.ListedSkills.Count >= 6 && facts.UnsupportedSkills.Count >= facts.ListedSkills.Count / 2)
        {
            score.Deduct(5);
        }
    }

    private static string GenerateSummary(int score)
    {
        return score switch
        {
            >= 90 => "Excellent ATS compatibility with strong section coverage and supporting evidence.",
            >= 80 => "Strong ATS compatibility with a few refinements still possible.",
            >= 70 => "Good ATS compatibility, but some sections or evidence can be improved.",
            >= 60 => "Moderate ATS compatibility. Important structure or evidence gaps remain.",
            _ => "ATS compatibility needs work. Key sections, evidence, or formatting signals are missing."
        };
    }

    private static ATSDetailedReport BuildDetailedReport(
        ATSAnalysisResult result,
        ResumeFacts facts,
        string? fileName,
        long? fileSizeBytes,
        string? contentType)
    {
        var report = new ATSDetailedReport
        {
            TotalIssues = result.Issues.Count,
            ParseRate = CalculateParseRate(facts),
            ParsedContactFields = BuildContactFields(facts),
            EssentialSectionsFound = BuildEssentialSectionsFound(facts),
            EssentialSectionsMissing = BuildEssentialSectionsMissing(facts),
            RepeatedWords = BuildRepeatedWords(facts)
        };

        report.Steps.Add(BuildParseRateCheck(report.ParseRate, facts));
        report.Steps.Add(BuildRepetitionCheck(report.RepeatedWords));
        report.Steps.Add(BuildGrammarCheck(facts));
        report.Steps.Add(BuildEssentialSectionsCheck(report.EssentialSectionsFound, report.EssentialSectionsMissing));
        report.Steps.Add(BuildContactCheck(report.ParsedContactFields, facts));
        report.Steps.Add(BuildFileCheck(fileName, fileSizeBytes, contentType));
        report.Steps.Add(BuildDesignCheck(facts));
        report.Steps.Add(BuildImpactCheck(facts));

        return report;
    }

    private static ATSStepCheck BuildParseRateCheck(int parseRate, ResumeFacts facts)
    {
        var check = new ATSStepCheck
        {
            Category = "Content",
            Title = "ATS Parse Rate",
            Status = parseRate >= 95 ? "Great" : parseRate >= 80 ? "Watch" : "Risk",
            Summary = parseRate >= 95
                ? $"We parsed {parseRate}% of your resume successfully."
                : $"We parsed roughly {parseRate}% of your resume. Some structure may confuse ATS parsing."
        };

        if (!facts.HasExperience) check.Details.Add("Experience heading is not clearly recognized.");
        if (!facts.HasSkills) check.Details.Add("Skills heading is not clearly recognized.");
        if (facts.HasTableLikeLayout) check.Details.Add("Table or tab-like layout may reduce parser accuracy.");
        if (facts.HasMultiColumnSpacing) check.Details.Add("Multi-column spacing may disrupt reading order.");

        check.IssueCount = check.Details.Count;
        return check;
    }

    private static ATSStepCheck BuildRepetitionCheck(List<ATSRepeatedWord> repeatedWords)
    {
        var check = new ATSStepCheck
        {
            Category = "Content",
            Title = "Repetition",
            Status = repeatedWords.Count == 0 ? "Great" : repeatedWords.Count <= 2 ? "Watch" : "Risk",
            Summary = repeatedWords.Count == 0
                ? "No heavy repetition detected in your action verbs."
                : $"We found {repeatedWords.Count} frequently repeated words."
        };

        foreach (var repeatedWord in repeatedWords.Take(4))
        {
            var alternatives = repeatedWord.Alternatives.Count == 0
                ? string.Empty
                : $" Try: {string.Join(", ", repeatedWord.Alternatives)}.";
            check.Details.Add($"{repeatedWord.Count} times: {repeatedWord.Word}.{alternatives}".Trim());
        }

        check.IssueCount = repeatedWords.Count;
        return check;
    }

    private static ATSStepCheck BuildGrammarCheck(ResumeFacts facts)
    {
        var check = new ATSStepCheck
        {
            Category = "Content",
            Title = "Spelling & Grammar",
            Status = facts.PossibleGrammarFlags == 0 ? "Great" : facts.PossibleGrammarFlags <= 2 ? "Watch" : "Risk",
            Summary = facts.PossibleGrammarFlags == 0
                ? "No critical machine-detectable spelling or grammar issues were found."
                : $"We flagged {facts.PossibleGrammarFlags} possible readability or grammar issues."
        };

        if (facts.LongLineCount > 4)
        {
            check.Details.Add("Several lines are very long and may be hard to scan quickly.");
        }

        if (facts.UnusualCharacterCount > 0)
        {
            check.Details.Add("Some decorative symbols or encoding artifacts were detected.");
        }

        if (facts.PossibleGrammarFlags > 0)
        {
            check.Details.Add("Review sentence fragments and punctuation around summary and project bullets.");
        }

        check.IssueCount = facts.PossibleGrammarFlags;
        return check;
    }

    private static ATSStepCheck BuildEssentialSectionsCheck(List<string> found, List<string> missing)
    {
        var check = new ATSStepCheck
        {
            Category = "Sections",
            Title = "Essential Sections",
            Status = missing.Count == 0 ? "Great" : missing.Count == 1 ? "Watch" : "Risk",
            Summary = missing.Count == 0
                ? "All essential sections were found."
                : $"Missing or unclear essential sections: {string.Join(", ", missing)}."
        };

        if (found.Count > 0)
        {
            check.Details.Add($"Found: {string.Join(", ", found)}.");
        }

        check.IssueCount = missing.Count;
        return check;
    }

    private static ATSStepCheck BuildContactCheck(List<string> parsedFields, ResumeFacts facts)
    {
        var check = new ATSStepCheck
        {
            Category = "Sections",
            Title = "Contact Information",
            Status = facts.HasEmail && facts.HasPhone ? "Great" : "Risk",
            Summary = parsedFields.Count == 0
                ? "No contact details were clearly parsed."
                : $"Parsed contact fields: {string.Join(", ", parsedFields)}."
        };

        if (!facts.HasEmail) check.Details.Add("Email address not detected.");
        if (!facts.HasPhone) check.Details.Add("Phone number not detected.");
        if (!facts.HasLinkedInOrPortfolio) check.Details.Add("LinkedIn, portfolio, or public profile link not detected.");
        if (facts.HasLinkedInOrPortfolio && !facts.HasFullUrl) check.Details.Add("Use full visible URLs instead of hidden link text.");

        check.IssueCount = check.Details.Count;
        return check;
    }

    private static ATSStepCheck BuildFileCheck(string? fileName, long? fileSizeBytes, string? contentType)
    {
        var extension = Path.GetExtension(fileName ?? string.Empty);
        var isPdf = string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase)
            || string.Equals(contentType, "application/pdf", StringComparison.OrdinalIgnoreCase);
        var sizeKb = fileSizeBytes.HasValue ? (int)Math.Round(fileSizeBytes.Value / 1024.0) : 0;
        var fileIssues = 0;

        if (!isPdf) fileIssues++;
        if (fileSizeBytes.HasValue && fileSizeBytes.Value > 2 * 1024 * 1024) fileIssues++;

        var check = new ATSStepCheck
        {
            Category = "ATS Essentials",
            Title = "File Format & Size",
            Status = fileIssues == 0 ? "Great" : "Watch",
            Summary = fileSizeBytes.HasValue
                ? $"Your resume file is {sizeKb} KB and the detected type is {(string.IsNullOrWhiteSpace(extension) ? "unknown" : extension.TrimStart('.').ToUpperInvariant())}."
                : "File size or file type data is not available for this analysis."
        };

        if (!isPdf) check.Details.Add("PDF is preferred for ATS uploads.");
        if (fileSizeBytes.HasValue && fileSizeBytes.Value > 2 * 1024 * 1024) check.Details.Add("Keep the file below 2 MB when possible.");

        check.IssueCount = fileIssues;
        return check;
    }

    private static ATSStepCheck BuildDesignCheck(ResumeFacts facts)
    {
        var issues = 0;
        var check = new ATSStepCheck
        {
            Category = "ATS Essentials",
            Title = "Design",
            Summary = "The resume layout was checked for common ATS parsing risks."
        };

        if (facts.HasTableLikeLayout)
        {
            issues++;
            check.Details.Add("Table-based layout detected.");
        }

        if (facts.HasMultiColumnSpacing)
        {
            issues++;
            check.Details.Add("Multi-column spacing may confuse reading order.");
        }

        if (facts.UnusualCharacterCount > 0)
        {
            issues++;
            check.Details.Add("Decorative bullets or encoding artifacts were detected.");
        }

        check.Status = issues == 0 ? "Great" : issues == 1 ? "Watch" : "Risk";
        check.IssueCount = issues;
        if (issues == 0)
        {
            check.Summary = "The resume uses a parser-friendly structure.";
        }

        return check;
    }

    private static ATSStepCheck BuildImpactCheck(ResumeFacts facts)
    {
        var check = new ATSStepCheck
        {
            Category = "Content",
            Title = "Quantify Impact",
            Status = facts.QuantifiedEvidenceCount >= 3 ? "Great" : facts.QuantifiedEvidenceCount >= 1 ? "Watch" : "Risk",
            Summary = facts.QuantifiedEvidenceCount >= 3
                ? "Most of your bullets include measurable outcomes."
                : facts.QuantifiedEvidenceCount >= 1
                    ? "Some quantified impact is present, but more would strengthen the resume."
                    : "Very little quantified impact was detected."
        };

        if (facts.QuantifiedEvidenceCount == 0)
        {
            check.Details.Add("Add percentages, counts, volumes, timelines, or reliability gains to experience bullets.");
        }
        else if (facts.QuantifiedEvidenceCount < 3)
        {
            check.Details.Add("Add more measurable outcomes across recent roles.");
        }

        check.IssueCount = facts.QuantifiedEvidenceCount >= 3 ? 0 : Math.Max(1, 3 - facts.QuantifiedEvidenceCount);
        return check;
    }

    private static List<string> BuildContactFields(ResumeFacts facts)
    {
        var fields = new List<string>();
        if (facts.HasPhone) fields.Add("Phone");
        if (facts.HasEmail) fields.Add("Email");
        if (facts.HasLinkedInOrPortfolio) fields.Add("LinkedIn/Portfolio");
        return fields;
    }

    private static List<string> BuildEssentialSectionsFound(ResumeFacts facts)
    {
        var found = new List<string>();
        if (facts.HasExperience) found.Add("Experience");
        if (facts.HasEducation) found.Add("Education");
        if (facts.HasSummary) found.Add("Summary");
        if (facts.HasSkills) found.Add("Skills");
        return found;
    }

    private static List<string> BuildEssentialSectionsMissing(ResumeFacts facts)
    {
        var missing = new List<string>();
        if (!facts.HasExperience) missing.Add("Experience");
        if (!facts.HasEducation) missing.Add("Education");
        if (!facts.HasSummary) missing.Add("Summary");
        if (!facts.HasSkills) missing.Add("Skills");
        return missing;
    }

    private static List<ATSRepeatedWord> BuildRepeatedWords(ResumeFacts facts)
    {
        return RepetitionSuggestions
            .Select(pair => new ATSRepeatedWord
            {
                Word = pair.Key,
                Count = CountOccurrences(facts.LowerText, pair.Key),
                Alternatives = pair.Value.ToList()
            })
            .Where(item => item.Count >= 3)
            .OrderByDescending(item => item.Count)
            .ToList();
    }

    private static int CalculateParseRate(ResumeFacts facts)
    {
        var score = 100;
        if (!facts.HasExperience) score -= 20;
        if (!facts.HasSkills) score -= 12;
        if (!facts.HasEducation) score -= 10;
        if (!facts.HasPhone || !facts.HasEmail) score -= 10;
        if (facts.HasTableLikeLayout) score -= 20;
        if (facts.HasMultiColumnSpacing) score -= 10;
        if (facts.UnusualCharacterCount >= 4) score -= 5;
        return Math.Max(0, score);
    }

    private static List<string> ExtractListedSkills(ResumeFacts facts)
    {
        var fromSkillsSection = KnownSkills
            .Where(skill => facts.SkillsSection.Contains(skill, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (fromSkillsSection.Count > 0)
        {
            return fromSkillsSection;
        }

        return KnownSkills
            .Where(skill => CountOccurrences(facts.LowerText, skill) > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(10)
            .ToList();
    }

    private static string ExtractSection(string text, string[] aliases)
    {
        var lines = text.Split('\n', StringSplitOptions.None);
        var startIndex = -1;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim().ToLowerInvariant();
            if (aliases.Any(alias => line.Contains(alias)))
            {
                startIndex = i + 1;
                break;
            }
        }

        if (startIndex < 0)
        {
            return string.Empty;
        }

        var allAliases = SectionAliases.Values.SelectMany(static value => value).ToArray();
        var sectionLines = new List<string>();

        for (var i = startIndex; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            var lowerLine = line.ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(line) && allAliases.Any(lowerLine.Contains))
            {
                break;
            }

            sectionLines.Add(line);
        }

        return string.Join('\n', sectionLines);
    }

    private static string NormalizeText(string text)
    {
        return (text ?? string.Empty)
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Replace("\t", "    ")
            .Replace("•", "*")
            .Trim();
    }

    private static int CountUnusualCharacters(string text)
    {
        var unusualChars = new[] { '●', '◆', '■', '★', '✓', '▶', '◦' };
        return text.Count(c => unusualChars.Contains(c));
    }

    private static int CountGrammarFlags(string text)
    {
        var flags = 0;
        flags += Regex.Matches(text, @"\s{2,}").Count > 6 ? 1 : 0;
        flags += Regex.Matches(text, @"[a-z][A-Z]").Count > 2 ? 1 : 0;
        flags += text.Contains("..") ? 1 : 0;
        flags += text.Contains(" ,") || text.Contains(" .") ? 1 : 0;
        return flags;
    }

    private static int CountOccurrences(string text, string word)
    {
        var pattern = $@"(?<!\w){Regex.Escape(word)}(?!\w)";
        return Regex.Matches(text, pattern, RegexOptions.IgnoreCase).Count;
    }

    private static int WordCount(string text)
    {
        return Regex.Matches(text ?? string.Empty, @"\b[\w#+./-]+\b").Count;
    }
}
