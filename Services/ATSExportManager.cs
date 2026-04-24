using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace ATSklar.Services;

/// <summary>
/// ATS Export Manager - Exports resumes in ATS-friendly formats
/// </summary>
public class ATSExportManager
{
    /// <summary>
    /// Export resume as plain text (most ATS-friendly)
    /// </summary>
    public static byte[] ExportAsPlainText(string resumeText)
    {
        // Clean up text for plain text export
        var cleanedText = CleanTextForPlainText(resumeText);
        return System.Text.Encoding.UTF8.GetBytes(cleanedText);
    }

    /// <summary>
    /// Export resume as DOCX (Word format)
    /// </summary>
    public static byte[] ExportAsDocx(string resumeText)
    {
        using var memoryStream = new MemoryStream();
        using (var wordprocessingDocument = WordprocessingDocument.Create(
            memoryStream, WordprocessingDocumentType.Document))
        {
            // Create main document part
            var mainPart = wordprocessingDocument.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            // Process text and add paragraphs
            var lines = resumeText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var paragraph = body.AppendChild(new Paragraph());
                var run = paragraph.AppendChild(new Run());

                // Set formatting based on line content
                var runProperties = run.AppendChild(new RunProperties());
                
                // Bold for section headers (all caps or known headers)
                if (IsSectionHeader(line))
                {
                    runProperties.AppendChild(new Bold());
                    runProperties.AppendChild(new FontSize { Val = "24" }); // 12pt
                }
                // Slightly larger for job titles/positions
                else if (IsJobTitle(line))
                {
                    runProperties.AppendChild(new Bold());
                    runProperties.AppendChild(new FontSize { Val = "22" }); // 11pt
                }
                else
                {
                    runProperties.AppendChild(new FontSize { Val = "20" }); // 10pt
                }

                var cleanedLine = CleanLineForDocx(line);
                run.AppendChild(new Text { Text = cleanedLine });
                
                var pPr = paragraph.AppendChild(new ParagraphProperties());
                pPr.AppendChild(new SpacingBetweenLines { After = "60" }); // 6pt spacing
            }

            wordprocessingDocument.Save();
        }

        return memoryStream.ToArray();
    }

    private static string CleanTextForPlainText(string text)
    {
        var sb = new StringBuilder();
        var lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            // Replace special characters with plain equivalents
            var cleanedLine = line
                .Replace("•", "*")
                .Replace("○", "-")
                .Replace("◆", "-")
                .Replace("■", "-")
                .Replace("→", ">")
                .Replace("✓", "✓")
                .Replace("–", "-")
                .Replace("\"", "\"")
                .Replace("\"", "\"")
                .Replace("'", "'")
                .Replace("'", "'");

            sb.AppendLine(cleanedLine.TrimEnd());
        }

        return sb.ToString();
    }

    private static string CleanLineForDocx(string line)
    {
        return line
            .Replace("•", "•")
            .Replace("○", "◦")
            .Replace("–", "-")
            .Replace("\"", "\"")
            .Replace("\"", "\"");
    }

    private static bool IsSectionHeader(string line)
    {
        var trimmed = line.Trim();
        var headers = new[] { "EXPERIENCE", "EDUCATION", "SKILLS", "SUMMARY", "CONTACT", 
                             "PROJECTS", "CERTIFICATIONS", "LANGUAGES", "ACHIEVEMENTS" };
        return headers.Any(h => trimmed.Equals(h, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsJobTitle(string line)
    {
        // Job titles usually contain company names and job positions
        var trimmed = line.Trim();
        var jobIndicators = new[] { "engineer", "developer", "analyst", "manager", "director", 
                                   "consultant", "lead", "senior", "junior", "specialist" };
        return jobIndicators.Any(indicator => 
            trimmed.Contains(indicator, StringComparison.OrdinalIgnoreCase)) 
            && trimmed.Length > 10 && !trimmed.All(c => char.IsUpper(c));
    }
}
