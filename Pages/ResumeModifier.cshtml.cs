using System.Text;
using ATSklar.Services;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ATSklar.Pages;

public class ResumeModifierModel : PageModel
{
private readonly ILogger<ResumeModifierModel> _logger;
private readonly AIResumeAnalyzer _aiAnalyzer;
private readonly CompanyResearchService _companyResearchService;
private const long MaxFileSize = 10 * 1024 * 1024;

[BindProperty]
public IFormFile? ResumeFile { get; set; }

[BindProperty]
public string CompanyName { get; set; } = string.Empty;

[BindProperty]
public string TargetRole { get; set; } = string.Empty;

[BindProperty]
public string CompanyWebsite { get; set; } = string.Empty;

[BindProperty]
public string JobDescription { get; set; } = string.Empty;

[BindProperty(SupportsGet = true)]
public string Mode { get; set; } = "quick";

public bool ResumeAnalyzed { get; set; }
public bool JobAnalyzed { get; set; }
public bool ShowJobForm { get; set; }

public string ResumeText { get; set; } = string.Empty;
public int ATSScore { get; set; }
public int OptimizedATSScore { get; set; }
public List<ATSIssue> ATSIssues { get; set; } = new();
public ATSDetailedReport ATSDetailedReport { get; set; } = new();
public Dictionary<string, int> KeywordFrequency { get; set; } = new();
public List<string> MissingKeywords { get; set; } = new();

public ATSKeywordOptimizer.KeywordAnalysis? JobKeywordAnalysis { get; set; }
public List<ATSPlacementSuggestion> PlacementSuggestions { get; set; } = new();

public AIResumeAnalyzer.TailoredResumeResult? TailoredResume { get; set; }

public string Message { get; set; } = string.Empty; 
public bool IsSuccess { get; set; }

public ResumeModifierModel(
    ILogger<ResumeModifierModel> logger,
    AIResumeAnalyzer aiAnalyzer,
    CompanyResearchService companyResearchService)
{
    _logger = logger;
    _aiAnalyzer = aiAnalyzer;
    _companyResearchService = companyResearchService;
}

public void OnGet()
{
    Mode = NormalizeMode(Mode);
}

public async Task<IActionResult> OnPostAsync(string? action)
{
    try
    {
        return action switch
        {
            "analyze" => await HandleAnalyzeResumeAsync(),
            "showJobForm" => HandleShowJobForm(), // ✅ NEW
            "researchOptimize" => await HandleResearchOptimizeAsync(),
            "exportText" => HandleExportText(),
            "exportDocx" => HandleExportDocx(),
            _ => Page()
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing resume");
        Message = $"Error: {ex.Message}";
        IsSuccess = false;
        return Page();
    }
}

// =============================
// STEP 1: ANALYZE RESUME
// =============================
private async Task<IActionResult> HandleAnalyzeResumeAsync()
{
    Mode = NormalizeMode(Mode);

    if (ResumeFile == null || ResumeFile.Length == 0)
    {
        Message = "Please select a resume file.";
        IsSuccess = false;
        return Page();
    }

    if (ResumeFile.Length > MaxFileSize)
    {
        Message = "File size exceeds the 10MB limit.";
        IsSuccess = false;
        return Page();
    }

    ResumeText = await ExtractTextFromFileAsync(ResumeFile);

    HttpContext.Session.SetString("CurrentResume", ResumeText);
    HttpContext.Session.Remove("OptimizedResume");
    HttpContext.Session.SetString("ResumeFileName", ResumeFile.FileName ?? string.Empty);
    HttpContext.Session.SetString("ResumeContentType", ResumeFile.ContentType ?? string.Empty);
    HttpContext.Session.SetString("ResumeFileSize", ResumeFile.Length.ToString());

    ApplyResumeAnalysis(ResumeText, ResumeFile.FileName, ResumeFile.Length, ResumeFile.ContentType);

    ResumeAnalyzed = true;
    ShowJobForm = false; // ✅ ensures clean step flow

    Message = "Resume analyzed successfully. You can now tailor it for a job.";
    IsSuccess = true;

    return Page();
}

// =============================
// STEP 2: SHOW JOB FORM
// =============================
private IActionResult HandleShowJobForm()
{
    if (!HttpContext.Session.TryGetValue("CurrentResume", out _))
    {
        Message = "Please analyze a resume first.";
        IsSuccess = false;
        return Page();
    }

    ResumeText = HttpContext.Session.GetString("CurrentResume") ?? string.Empty;

    ApplyResumeAnalysis(
        ResumeText,
        GetStoredFileName(),
        GetStoredFileSize(),
        GetStoredContentType()
    );

    ResumeAnalyzed = true;
    ShowJobForm = true; // ✅ KEY FIX

    return Page();
}

// =============================
// STEP 3: OPTIMIZE FOR JOB
// =============================
private async Task<IActionResult> HandleResearchOptimizeAsync()
{
    Mode = NormalizeMode(Mode);

    if (!HttpContext.Session.TryGetValue("CurrentResume", out _))
    {
        Message = "Please upload and analyze a resume first.";
        IsSuccess = false;
        return Page();
    }

    if (string.IsNullOrWhiteSpace(CompanyName) || string.IsNullOrWhiteSpace(TargetRole))
    {
        Message = "Please enter both a target company and target role.";
        IsSuccess = false;
        return Page();
    }

    ResumeText = HttpContext.Session.GetString("CurrentResume") ?? string.Empty;

    ApplyResumeAnalysis(
        ResumeText,
        GetStoredFileName(),
        GetStoredFileSize(),
        GetStoredContentType()
    );

    ResumeAnalyzed = true;
    ShowJobForm = true; // ✅ KEEP FORM VISIBLE

    if (!await _aiAnalyzer.IsAIAvailableAsync())
    {
        Message = "The rewrite service is not available right now.";
        IsSuccess = false;
        return Page();
    }

    var companyResearch = await _companyResearchService.ResearchCompanyAsync(
        CompanyName,
        TargetRole,
        CompanyWebsite
    );

    if (!string.IsNullOrWhiteSpace(JobDescription))
    {
        JobKeywordAnalysis = ATSKeywordOptimizer.AnalyzeJobDescription(JobDescription, ResumeText);
        PlacementSuggestions = ATSKeywordOptimizer.GeneratePlacementSuggestions(
            JobKeywordAnalysis,
            ResumeText,
            TargetRole,
            CompanyName
        );
        JobAnalyzed = true;
    }

    TailoredResume = await _aiAnalyzer.TailorResumeAsync(
        ResumeText,
        new AIResumeAnalyzer.TailoredResumeRequest
        {
            CompanyName = CompanyName,
            TargetRole = TargetRole,
            JobDescription = JobDescription,
            CompanyResearch = companyResearch.Overview
        });

    if (!string.IsNullOrWhiteSpace(TailoredResume.FullResumeDraft))
    {
        HttpContext.Session.SetString("OptimizedResume", TailoredResume.FullResumeDraft);
        OptimizedATSScore = ATSResumeAnalyzer.AnalyzeResume(TailoredResume.FullResumeDraft).ATSScore;
    }

    Message = $"Tailored resume generated for {CompanyName}.";
    IsSuccess = true;

    return Page();
}

// =============================
// EXPORT
// =============================
private IActionResult HandleExportText()
{
    if (!HttpContext.Session.TryGetValue("CurrentResume", out _))
    {
        Message = "No resume available.";
        IsSuccess = false;
        return Page();
    }

    var resumeText = HttpContext.Session.GetString("OptimizedResume")
        ?? HttpContext.Session.GetString("CurrentResume")
        ?? string.Empty;

    return File(
        ATSExportManager.ExportAsPlainText(resumeText),
        "text/plain",
        $"resume-{DateTime.Now:yyyyMMdd-HHmmss}.txt");
}

private IActionResult HandleExportDocx()
{
    if (!HttpContext.Session.TryGetValue("CurrentResume", out _))
    {
        Message = "No resume available.";
        IsSuccess = false;
        return Page();
    }

    var resumeText = HttpContext.Session.GetString("OptimizedResume")
        ?? HttpContext.Session.GetString("CurrentResume")
        ?? string.Empty;

    return File(
        ATSExportManager.ExportAsDocx(resumeText),
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        $"resume-{DateTime.Now:yyyyMMdd-HHmmss}.docx");
}

// =============================
// HELPERS
// =============================
private async Task<string> ExtractTextFromFileAsync(IFormFile file)
{
    if (file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        return ResumePdfModifier.ExtractTextFromPdf(await ToBytes(file));

    if (file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
        return ExtractDocxText(file);

    using var reader = new StreamReader(file.OpenReadStream());
    return await reader.ReadToEndAsync();
}

private async Task<byte[]> ToBytes(IFormFile file)
{
    using var ms = new MemoryStream();
    await file.CopyToAsync(ms);
    return ms.ToArray();
}

private string ExtractDocxText(IFormFile file)
{
    using var stream = new MemoryStream();
    file.CopyTo(stream);
    stream.Position = 0;

    var text = new StringBuilder();

    using var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(stream, false);
    foreach (var paragraph in doc.MainDocumentPart.Document.Body.Elements<Paragraph>())
    {
        text.AppendLine(paragraph.InnerText);
    }

    return text.ToString();
}

private void ApplyResumeAnalysis(string text, string? fileName, long? size, string? type)
{
    var analysis = ATSResumeAnalyzer.AnalyzeResume(text, fileName, size, type);
    ATSScore = analysis.ATSScore;
    ATSIssues = analysis.Issues;
    ATSDetailedReport = analysis.DetailedReport;
    KeywordFrequency = analysis.KeywordFrequency;
    MissingKeywords = analysis.MissingKeywords;
}

private static string NormalizeMode(string? mode)
{
    return string.Equals(mode, "focused", StringComparison.OrdinalIgnoreCase)
        ? "focused"
        : "quick";
}

private string GetStoredFileName() => HttpContext.Session.GetString("ResumeFileName") ?? "";
private string GetStoredContentType() => HttpContext.Session.GetString("ResumeContentType") ?? "";
private long GetStoredFileSize() => long.TryParse(HttpContext.Session.GetString("ResumeFileSize"), out var s) ? s : 0;


}
