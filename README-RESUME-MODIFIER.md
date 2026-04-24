# Resume PDF Modifier - Quick Start Guide

## Overview
Your ATSklar project now includes a full-featured Resume PDF Modifier accessible at `/ResumeModifier` page.

## Features

### 1. **Upload & Extract**
   - Upload any PDF resume (max 5MB)
   - Automatically extracts all text content
   - Displays PDF metadata (title, author, subject, pages, etc.)

### 2. **Find & Replace**
   - Replace text throughout your PDF
   - Add multiple find/replace pairs
   - Perfect for updating dates, contact info, addresses, etc.
   - Example: Replace "2025" with "2026", "New York" with "Remote"

### 3. **Update Metadata**
   - Change PDF title, author, subject
   - Essential for ensuring professional document properties
   - Helpful for ATS (Applicant Tracking Systems)

### 4. **Form Fields** (fillable PDFs)
   - Fill form fields if your PDF has interactive fields
   - Enter field names and values
   - Auto-populated if PDF has form structure

### 5. **Download Modified**
   - All modifications are applied and downloaded as new PDF
   - Original file remains unchanged
   - Timestamped filenames for organization

## How to Use

1. Navigate to **http://localhost:5000/ResumeModifier** (or your configured URL)
2. Click "Choose PDF File" and select your resume
3. Click "Upload PDF"
4. Review extracted text and metadata
5. In "Modify Content" section:
   - Use **Find & Replace** tab to update text
   - Use **Update Metadata** tab to change document properties
   - Use **Form Fields** tab for fillable PDFs
6. Click "Apply Changes & Download"
7. Your modified resume will download as `resume-modified-[timestamp].pdf`

## Project Structure

```
ATSklar/
├── Services/
│   └── ResumePdfModifier.cs          # PDF manipulation logic
├── Pages/
│   ├── ResumeModifier.cshtml         # UI form
│   ├── ResumeModifier.cshtml.cs      # Page handler
│   └── Shared/_Layout.cshtml         # Updated with nav link
├── Program.cs                        # Updated with session support
└── ATSklar.csproj                    # Added iText7 NuGet package
```

## Key Technologies

- **iText7 (v9.0.0)**: Industry-leading PDF manipulation library
- **ASP.NET Core Razor Pages**: Web framework
- **Bootstrap 5**: Responsive UI
- **Session State**: Temporary PDF storage during modifications

## API Methods in ResumePdfModifier Service

- `ExtractTextFromPdf(byte[])` - Extract all text from PDF
- `ReplaceTextInPdf(byte[], Dictionary<string,string>)` - Find & replace
- `UpdatePdfMetadata(byte[], Dictionary<string,string>)` - Update document properties
- `UpdateFormFields(byte[], Dictionary<string,string>)` - Fill form fields
- `GetPdfMetadata(byte[])` - Retrieve document metadata
- `MergePdfs(List<byte[]>)` - Combine multiple PDFs (bonus feature)
- `AddTextOverlay(byte[], string, float, float, int)` - Add text to PDF (bonus feature)

## Running the Application

```bash
cd e:\ATSklar
dotnet run
```

Then visit: `https://localhost:5001` or `http://localhost:5000`

## Examples

**Example 1: Update Contact Info**
- Find: "john@oldmail.com" → Replace: "john@newmail.com"
- Find: "(555) 123-4567" → Replace: "(555) 987-6543"

**Example 2: Update Dates**
- Find: "Jan 2024 - Present" → Replace: "Jan 2024 - April 2026"
- Find: "Graduate: 2020" → Replace: "Graduate: 2020"

**Example 3: Update Location**
- Find: "New York, NY" → Replace: "Remote, USA"

## Notes

- Sessions expire after 20 minutes of inactivity
- Maximum file size: 5MB
- Supports standard PDF format
- Complex form fields may require additional processing
- Text extraction works best with standard PDF text (not scanned images)

## Troubleshooting

- **"Please upload a valid PDF file"** - Ensure the file is a PDF format
- **"File size exceeds 5MB limit"** - Reduce PDF file size
- **Text not extracted** - PDF may be image-based (scanned document)
- **Metadata not updating** - Some PDFs have restricted permissions

## Future Enhancements

- OCR support for scanned PDFs
- Advanced form field handling
- PDF compression options
- Batch processing multiple files
- Custom watermarks
- PDF signature support
