using System.Text;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;

namespace ATSklar.Services;

/// <summary>
/// ATS Export Manager - Exports resumes in ATS-friendly formats.
/// PDF export uses standard Helvetica fonts and a single-column, text-based
/// layout (no images, no tables, no columns) so applicant tracking systems
/// can parse it cleanly while still looking polished to human reviewers.
/// </summary>
public class ATSExportManager
{
    /// <summary>
    /// Export resume as plain text (most ATS-friendly).
    /// </summary>
    public static byte[] ExportAsPlainText(string resumeText)
    {
        var cleanedText = CleanTextForPlainText(resumeText);
        return Encoding.UTF8.GetBytes(cleanedText);
    }

    /// <summary>
    /// Export resume as ATS-safe PDF using iText7.
    /// - Single column, no tables, no images.
    /// - Standard Helvetica fonts (universally parsable).
    /// - Section headers detected and bolded slightly.
    /// - Bullet points normalized.
    /// </summary>
    public static byte[] ExportAsPdf(string resumeText)
    {
        using var memoryStream = new MemoryStream();

        using (var writer = new PdfWriter(memoryStream))
        using (var pdf = new PdfDocument(writer))
        using (var doc = new Document(pdf, PageSize.LETTER))
        {
            // Tight, professional margins (0.5 inch = 36pt).
            doc.SetMargins(40f, 48f, 40f, 48f);

            var body = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            var lines = resumeText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // First non-empty line is treated as the candidate name (title).
            var titleEmitted = false;

            foreach (var raw in lines)
            {
                var line = CleanLineForPdf(raw);

                if (string.IsNullOrWhiteSpace(line))
                {
                    // Preserve a small visual gap with an empty paragraph.
                    doc.Add(new Paragraph(" ").SetFontSize(4f).SetMarginTop(0).SetMarginBottom(0));
                    continue;
                }

                Paragraph p;

                if (!titleEmitted)
                {
                    p = new Paragraph(line)
                        .SetFont(bold)
                        .SetFontSize(18f)
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetMarginTop(0f)
                        .SetMarginBottom(2f);
                    titleEmitted = true;
                }
                else if (IsSectionHeader(line))
                {
                    p = new Paragraph(line.ToUpperInvariant())
                        .SetFont(bold)
                        .SetFontSize(11.5f)
                        .SetCharacterSpacing(0.6f)
                        .SetMarginTop(10f)
                        .SetMarginBottom(4f);
                }
                else if (IsJobTitle(line))
                {
                    p = new Paragraph(line)
                        .SetFont(bold)
                        .SetFontSize(10.5f)
                        .SetMarginTop(4f)
                        .SetMarginBottom(2f);
                }
                else if (IsBullet(line))
                {
                    // Normalize bullet character for ATS-friendliness.
                    var trimmed = line.TrimStart('-', '*', '\u2022', '\u2023', '\u25E6', '\u25C6', '\u25A0', ' ', '\t');
                    p = new Paragraph("\u2022  " + trimmed)
                        .SetFont(body)
                        .SetFontSize(10f)
                        .SetMarginLeft(12f)
                        .SetMarginTop(0f)
                        .SetMarginBottom(2f);
                }
                else
                {
                    p = new Paragraph(line)
                        .SetFont(body)
                        .SetFontSize(10f)
                        .SetMarginTop(0f)
                        .SetMarginBottom(2f);
                }

                p.SetMultipliedLeading(1.25f);
                doc.Add(p);
            }
        }

        return memoryStream.ToArray();
    }

    private static string CleanTextForPlainText(string text)
    {
        var sb = new StringBuilder();
        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            var cleanedLine = line
                .Replace("\u2022", "*")
                .Replace("\u25CB", "-")
                .Replace("\u25C6", "-")
                .Replace("\u25A0", "-")
                .Replace("\u2192", ">")
                .Replace("\u2013", "-")
                .Replace("\u201C", "\"")
                .Replace("\u201D", "\"")
                .Replace("\u2018", "'")
                .Replace("\u2019", "'");

            sb.AppendLine(cleanedLine.TrimEnd());
        }

        return sb.ToString();
    }

    private static string CleanLineForPdf(string line)
    {
        return line
            .Replace("\u201C", "\"")
            .Replace("\u201D", "\"")
            .Replace("\u2018", "'")
            .Replace("\u2019", "'")
            .Replace("\u2013", "-")
            .Replace("\u2014", "-")
            .TrimEnd();
    }

    private static bool IsBullet(string line)
    {
        var t = line.TrimStart();
        if (t.Length == 0) return false;
        var c = t[0];
        return c == '-' || c == '*' || c == '\u2022' || c == '\u2023' || c == '\u25E6' || c == '\u25C6' || c == '\u25A0';
    }

    private static bool IsSectionHeader(string line)
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.Length > 40) return false;

        var headers = new[]
        {
            "EXPERIENCE", "WORK EXPERIENCE", "PROFESSIONAL EXPERIENCE",
            "EDUCATION", "SKILLS", "TECHNICAL SKILLS", "SUMMARY",
            "PROFESSIONAL SUMMARY", "CONTACT", "PROJECTS",
            "CERTIFICATIONS", "LANGUAGES", "ACHIEVEMENTS", "AWARDS",
            "PUBLICATIONS", "INTERESTS", "OBJECTIVE", "PROFILE"
        };

        if (headers.Any(h => trimmed.Equals(h, StringComparison.OrdinalIgnoreCase)))
            return true;

        // All-caps short line is treated as a header.
        return trimmed.Length <= 30
               && trimmed.Any(char.IsLetter)
               && trimmed.Where(char.IsLetter).All(char.IsUpper);
    }

    private static bool IsJobTitle(string line)
    {
        var trimmed = line.Trim();
        var jobIndicators = new[]
        {
            "engineer", "developer", "analyst", "manager", "director",
            "consultant", "lead", "senior", "junior", "specialist",
            "architect", "designer", "intern"
        };
        return jobIndicators.Any(i => trimmed.Contains(i, StringComparison.OrdinalIgnoreCase))
               && trimmed.Length > 10
               && !trimmed.All(char.IsUpper);
    }
}

