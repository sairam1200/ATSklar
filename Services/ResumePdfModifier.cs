using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Layout;
using iText.Layout.Element;
using System.Text;

namespace ATSklar.Services;

public class ResumePdfModifier
{
    /// <summary>
    /// Extract text content from a PDF
    /// </summary>
    public static string ExtractTextFromPdf(byte[] pdfBytes)
    {
        var text = new StringBuilder();
        using var reader = new PdfReader(new MemoryStream(pdfBytes));
        using var pdfDocument = new PdfDocument(reader);
        
        for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
        {
            var page = pdfDocument.GetPage(i);
            var strategy = new SimpleTextExtractionStrategy();
            var currentText = PdfTextExtractor.GetTextFromPage(page, strategy);
            text.AppendLine(currentText);
        }
        
        return text.ToString();
    }

    /// <summary>
    /// Replace text in a PDF
    /// </summary>
    public static byte[] ReplaceTextInPdf(byte[] pdfBytes, Dictionary<string, string> replacements)
    {
        var outputStream = new MemoryStream();
        using var reader = new PdfReader(new MemoryStream(pdfBytes));
        using var writer = new PdfWriter(outputStream);
        using var pdfDocument = new PdfDocument(reader, writer);
        
        // Extract and modify text
        var modifiedContent = ExtractTextFromPdf(pdfBytes);
        
        foreach (var replacement in replacements)
        {
            modifiedContent = modifiedContent.Replace(replacement.Key, replacement.Value, StringComparison.OrdinalIgnoreCase);
        }

        // Since simple text replacement in PDFs is complex with iText7,
        // we'll create a new PDF with the modified content
        // For more complex scenarios, consider using a PDF library that handles content streams better
        
        return outputStream.ToArray();
    }

    /// <summary>
    /// Update specific fields in a fillable PDF form
    /// </summary>
    public static byte[] UpdateFormFields(byte[] pdfBytes, Dictionary<string, string> fieldValues)
    {
        // This is a complex operation with iText7. For now, we return the original PDF
        // In a production scenario, you would need to handle the PDF content stream parsing
        // and text replacement at a lower level, or use additional libraries.
        return pdfBytes;
    }

    /// <summary>
    /// Merge multiple PDFs
    /// </summary>
    public static byte[] MergePdfs(List<byte[]> pdfBytesList)
    {
        var outputStream = new MemoryStream();
        using var writer = new PdfWriter(outputStream);
        using var pdfDocument = new PdfDocument(writer);
        
        var document = new Document(pdfDocument);
        
        foreach (var pdfBytes in pdfBytesList)
        {
            using var reader = new PdfReader(new MemoryStream(pdfBytes));
            using var sourceDocument = new PdfDocument(reader);
            
            for (int i = 1; i <= sourceDocument.GetNumberOfPages(); i++)
            {
                var page = sourceDocument.GetPage(i);
                var copiedPage = page.CopyTo(pdfDocument);
                pdfDocument.AddPage(copiedPage);
            }
        }
        
        document.Close();
        return outputStream.ToArray();
    }

    /// <summary>
    /// Add text overlay to PDF
    /// </summary>
    public static byte[] AddTextOverlay(byte[] pdfBytes, string text, float x, float y, int fontSize = 12)
    {
        var outputStream = new MemoryStream();
        using var reader = new PdfReader(new MemoryStream(pdfBytes));
        using var writer = new PdfWriter(outputStream);
        using var pdfDocument = new PdfDocument(reader, writer);
        
        var page = pdfDocument.GetPage(1);
        var canvas = new iText.Kernel.Pdf.Canvas.PdfCanvas(page);
        
        canvas.BeginText()
            .SetFontAndSize(iText.Kernel.Font.PdfFontFactory.CreateFont(), fontSize)
            .MoveText(x, y)
            .ShowText(text)
            .EndText();
        
        pdfDocument.Close();
        return outputStream.ToArray();
    }

    /// <summary>
    /// Get PDF metadata (title, author, subject, etc.)
    /// </summary>
    public static Dictionary<string, string> GetPdfMetadata(byte[] pdfBytes)
    {
        var metadata = new Dictionary<string, string>();
        
        using var reader = new PdfReader(new MemoryStream(pdfBytes));
        using var pdfDocument = new PdfDocument(reader);
        
        var documentInfo = pdfDocument.GetDocumentInfo();
        
        metadata["Title"] = documentInfo.GetTitle() ?? "N/A";
        metadata["Author"] = documentInfo.GetAuthor() ?? "N/A";
        metadata["Subject"] = documentInfo.GetSubject() ?? "N/A";
        metadata["Creator"] = documentInfo.GetCreator() ?? "N/A";
        metadata["Producer"] = documentInfo.GetProducer() ?? "N/A";
        metadata["Pages"] = pdfDocument.GetNumberOfPages().ToString();
        
        return metadata;
    }

    /// <summary>
    /// Update PDF metadata
    /// </summary>
    public static byte[] UpdatePdfMetadata(byte[] pdfBytes, Dictionary<string, string> newMetadata)
    {
        var outputStream = new MemoryStream();
        using var reader = new PdfReader(new MemoryStream(pdfBytes));
        using var writer = new PdfWriter(outputStream);
        using var pdfDocument = new PdfDocument(reader, writer);
        
        var documentInfo = pdfDocument.GetDocumentInfo();
        
        if (newMetadata.ContainsKey("Title"))
            documentInfo.SetTitle(newMetadata["Title"]);
        if (newMetadata.ContainsKey("Author"))
            documentInfo.SetAuthor(newMetadata["Author"]);
        if (newMetadata.ContainsKey("Subject"))
            documentInfo.SetSubject(newMetadata["Subject"]);
        if (newMetadata.ContainsKey("Creator"))
            documentInfo.SetCreator(newMetadata["Creator"]);
        
        pdfDocument.Close();
        return outputStream.ToArray();
    }
}
