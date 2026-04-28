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

    // Navigation State
    public int CurrentStep { get; set; } = 1;
    public bool ResumeAnalyzed { get; set; }
    public bool JobAnalyzed { get; set; }

    // Analysis Results
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
        CurrentStep = 1; // Default to upload page
    }

    public async Task<IActionResult> OnPostAsync(string? action)
    {
        try
        {
            return action switch
            {
                "analyze" => await HandleAnalyzeResumeAsync(),
                "researchOptimize" => await HandleResearchOptimizeAsync(),
                "exportText" => HandleExportText(),
                "exportasPdf" => HandleExportPdf(),
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

        ResumeText = await ExtractTextFromFileAsync(ResumeFile);

        // Store in Session
        HttpContext.Session.SetString("CurrentResume", ResumeText);
        HttpContext.Session.SetString("ResumeFileName", ResumeFile.FileName ?? string.Empty);
        HttpContext.Session.SetString("ResumeContentType", ResumeFile.ContentType ?? string.Empty);
        HttpContext.Session.SetString("ResumeFileSize", ResumeFile.Length.ToString());

        ApplyResumeAnalysis(ResumeText, ResumeFile.FileName, ResumeFile.Length, ResumeFile.ContentType);

        ResumeAnalyzed = true;
        CurrentStep = 2; // MOVE TO ANALYSIS PAGE
        IsSuccess = true;

        return Page();
    }

    // =============================
    // STEP 2: OPTIMIZE FOR JOB
    // =============================
    private async Task<IActionResult> HandleResearchOptimizeAsync()
    {
        Mode = NormalizeMode(Mode);
        ResumeText = HttpContext.Session.GetString("CurrentResume") ?? string.Empty;

        if (string.IsNullOrEmpty(ResumeText))
        {
            Message = "Resume session expired. Please upload again.";
            CurrentStep = 1;
            return Page();
        }

        // Re-run baseline analysis to keep UI populated
        ApplyResumeAnalysis(ResumeText, GetStoredFileName(), GetStoredFileSize(), GetStoredContentType());
        ResumeAnalyzed = true;

        if (string.IsNullOrWhiteSpace(CompanyName) || string.IsNullOrWhiteSpace(TargetRole))
        {
            Message = "Please enter both a target company and target role.";
            CurrentStep = 2;
            return Page();
        }

        var companyResearch = await _companyResearchService.ResearchCompanyAsync(CompanyName, TargetRole, CompanyWebsite);

        TailoredResume = await _aiAnalyzer.TailorResumeAsync(ResumeText, new AIResumeAnalyzer.TailoredResumeRequest {
            CompanyName = CompanyName,
            TargetRole = TargetRole,
            JobDescription = JobDescription,
            CompanyResearch = companyResearch.Overview
        });

        if (!string.IsNullOrWhiteSpace(TailoredResume.FullResumeDraft))
        {
            HttpContext.Session.SetString("OptimizedResume", TailoredResume.FullResumeDraft);
            OptimizedATSScore = ATSResumeAnalyzer.AnalyzeResume(TailoredResume.FullResumeDraft).ATSScore;
            JobAnalyzed = true;
            CurrentStep = 3; // MOVE TO FINAL PAGE
            IsSuccess = true;
        }

        return Page();
    }

    // =============================
    // EXPORT & HELPERS
    // =============================
    private IActionResult HandleExportText()
    {
        var resumeText = HttpContext.Session.GetString("OptimizedResume") ?? HttpContext.Session.GetString("CurrentResume") ?? "";
        return File(ATSExportManager.ExportAsPlainText(resumeText), "text/plain", $"resume-{DateTime.Now:yyyyMMdd}.txt");
    }

    private IActionResult HandleExportPdf
    ()
    {
        var resumeText = HttpContext.Session.GetString("OptimizedResume") ?? HttpContext.Session.GetString("CurrentResume") ?? "";
        return File(ATSExportManager.ExportAsPdf(resumeText), "application/pdf", $"resume-{DateTime.Now:yyyyMMdd}.pdf");
    }

    private async Task<string> ExtractTextFromFileAsync(IFormFile file)
    {
        if (file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            return ResumePdfModifier.ExtractTextFromPdf(await ToBytes(file));

        if (file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            return ExtractDocxText(file);

        using var reader = new StreamReader(file.OpenReadStream());
        return await reader.ReadToEndAsync();
    }

    private async Task<byte[]> ToBytes(IFormFile file) { using var ms = new MemoryStream(); await file.CopyToAsync(ms); return ms.ToArray(); }

    private string ExtractDocxText(IFormFile file)
    {
        using var stream = new MemoryStream();
        file.CopyTo(stream);
        stream.Position = 0;
        var text = new StringBuilder();
        using var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(stream, false);
        foreach (var p in doc.MainDocumentPart.Document.Body.Elements<Paragraph>()) text.AppendLine(p.InnerText);
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

    private static string NormalizeMode(string? mode) => string.Equals(mode, "focused", StringComparison.OrdinalIgnoreCase) ? "focused" : "quick";
    private string GetStoredFileName() => HttpContext.Session.GetString("ResumeFileName") ?? "";
    private string GetStoredContentType() => HttpContext.Session.GetString("ResumeContentType") ?? "";
    private long GetStoredFileSize() => long.TryParse(HttpContext.Session.GetString("ResumeFileSize"), out var s) ? s : 0;
}