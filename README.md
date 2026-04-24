# ATSklar - 🎯 AI-Powered ATS Resume Optimizer

> Transform your resume for Applicant Tracking Systems with intelligent AI analysis and scoring.

## ✨ What is ATSklar?

ATSklar is an advanced **ATS (Applicant Tracking System) Resume Optimizer** that helps you:
- ✅ Score your resume against ATS standards (0-100)
- ✅ Fix ATS-breaking issues (tables, graphics, special characters)
- ✅ Match keywords to job descriptions
- ✅ Get AI-powered resume insights
- ✅ Export optimized formats

**Now with AI**: Real AI analysis powered by local language models (Ollama).

---

## 🎯 Key Features

### 1. 📊 ATS Scoring (0-100)
- Instant compatibility score
- Color-coded feedback
- Issue breakdown with fixes
- See exactly what ATS systems detect

### 2. 🔍 Detailed Issue Detection
- Tables and graphics (ATS can't parse)
- Special characters
- Missing key sections
- Poor formatting
- Page count issues

### 3. 🔑 Keyword Analysis
- Extract keywords from job descriptions
- Compare with your resume
- See match percentage
- Get suggestions for missing keywords

### 4. 🤖 AI-Powered Analysis ✨ (NEW!)
- Resume assessment
- Strengths & weaknesses
- Suggested keywords
- Content improvement tips
- Gap analysis vs job requirements
- Interview preparation tips

### 5. 📄 Multi-Format Export
- Plain text (most ATS-friendly)
- DOCX (Word format)
- Auto-optimized selection

### 6. 📁 Multiple File Format Support
- PDF (with text extraction)
- DOCX (Word documents)
- TXT (plain text)

---

## 🚀 Quick Start

### Prerequisites
- .NET 10.0 SDK
- ASP.NET Core 10.0 runtime

### Installation
```bash
git clone [repo-url]
cd ATSklar
dotnet build
dotnet run
```

Browse to: `https://localhost:5001` or `http://localhost:5000`

### For AI Features
1. **Install Ollama**: https://ollama.ai
2. **Download a model**: `ollama pull llama2`
3. **Start service**: `ollama serve`
4. See [OLLAMA-SETUP.md](OLLAMA-SETUP.md) for detailed setup

---

## 📖 Usage Guide

### Step 1: Upload Resume
- Select PDF, DOCX, or TXT file
- Auto-extracts text
- Max 10MB

### Step 2: View ATS Analysis
- Get instant score (0-100)
- Review detected issues
- Check keyword frequency

### Step 3: Compare Job Description (Optional)
- Paste job posting
- See keyword match %
- Get suggestions

### Step 4: AI Analysis (Optional)
- Requires Ollama running
- Get AI insights
- Review strengths/weaknesses

### Step 5: Export
- Download optimized resume
- Choose format
- Ready to submit

---

## 📚 Documentation

- **[OLLAMA-SETUP.md](OLLAMA-SETUP.md)** - Complete AI setup guide
- **[AI-FEATURES.md](AI-FEATURES.md)** - Detailed AI feature guide
- **[Configuration](#configuration)** - Config options

---

## 🎨 UI Overview

The application presents a 5-step workflow:

```
┌─ Step 1: Upload Resume
├─ Step 2: ATS Analysis (Automatic)
├─ Step 3: Job Description Matching (Optional)
├─ Step 4: AI Analysis (Optional - requires Ollama)
└─ Step 5: Export
```

**Visual Feedback:**
- 🟢 Green: Excellent ATS score (90+)
- 🔵 Blue: Good score (75-89)
- 🟡 Yellow: Fair score (60-74)
- 🔴 Red: Poor score (<60)

---

## ⚙️ Configuration

### appsettings.json
```json
{
  "AI": {
    "OllamaUrl": "http://localhost:11434",
    "Model": "llama2"
  }
}
```

### Available Models
- **llama2** (default): Best quality, 10-15s per query
- **mistral**: Fastest (2-5s), excellent quality
- **neural-chat**: Balanced, optimized for dialogue
- **orca-mini**: Smallest, fastest, basic quality

### Change Configuration
```json
{
  "AI": {
    "OllamaUrl": "http://192.168.1.100:11434",  // Remote server
    "Model": "mistral"  // Use Mistral instead
  }
}
```

---

## 🏗️ Architecture

### Project Structure
```
ATSklar/
├── Pages/
│   ├── ResumeModifier.cshtml       # Main UI
│   ├── ResumeModifier.cshtml.cs    # Page handler
│   └── Index.cshtml                # Home page
├── Services/
│   ├── ResumePdfModifier.cs        # PDF extraction
│   ├── ATSResumeAnalyzer.cs        # ATS scoring
│   ├── ATSKeywordOptimizer.cs      # Keyword analysis
│   ├── ATSExportManager.cs         # Export formats
│   └── AIResumeAnalyzer.cs         # AI analysis ✨
├── Program.cs                       # DI & middleware
├── appsettings.json                # Configuration
└── OLLAMA-SETUP.md                 # AI setup guide
```

### Technology Stack
- **Framework**: ASP.NET Core 10.0 Razor Pages
- **PDF**: iText7 9.0.0
- **DOCX**: DocumentFormat.OpenXml 3.0.0
- **AI**: Ollama (Local LLM)
- **Language**: C#

### Service Dependencies
```
ILogger -> AIResumeAnalyzer -> Ollama API
         -> ATSResumeAnalyzer
         -> ATSKeywordOptimizer
         -> ATSExportManager
         -> ResumePdfModifier
```

---

## 🔧 Services Guide

### ResumePdfModifier
Handles PDF text extraction and metadata.
```csharp
var text = ResumePdfModifier.ExtractTextFromPdf(stream);
var metadata = ResumePdfModifier.GetPdfMetadata(stream);
```

### ATSResumeAnalyzer
Scores resume for ATS compatibility (0-100).
```csharp
var analysis = ATSResumeAnalyzer.AnalyzeResume(text);
// Returns: Score, Issues, Keywords, SummaryRecommendation
```

### ATSKeywordOptimizer
Extracts keywords and matches job descriptions.
```csharp
var jobAnalysis = ATSKeywordOptimizer.AnalyzeJobDescription(jobText, resumeText);
// Returns: MatchPercentage, ExtractedKeywords, MissingKeywords
```

### ATSExportManager
Exports resume in optimized formats.
```csharp
var plainText = ATSExportManager.ExportAsPlainText(text);
var docx = ATSExportManager.ExportAsDocx(text);
```

### AIResumeAnalyzer ✨
AI-powered resume analysis using Ollama.
```csharp
var result = await _aiAnalyzer.AnalyzeResumeWithAIAsync(resumeText, jobDescription);
// Returns: Assessment, Strengths, Weaknesses, Keywords, etc.
```

---

## 📊 ATS Score Breakdown

### Score Calculation
```
Base Score: 100
Critical Issues (-20 each):
  - Contains tables
  - Contains graphics
  - Contains special characters
  - Missing name/contact info
  - Missing work experience
  - Missing education

Warnings (-5 each):
  - More than 3 pages
  - Inconsistent formatting
  - Missing dates

Bonuses (+5 each):
  - 10+ ATS keywords found

Final: ATS Score (0-100)
```

### Score Interpretation
| Score | Status | ATS Chance |
|-------|--------|-----------|
| 90+ | ✅ Excellent | 95%+ |
| 75-89 | ✅ Good | 80-95% |
| 60-74 | 🔶 Fair | 50-80% |
| <60 | ❌ Poor | <50% |

---

## 🤖 AI Features

### Available AI Analyses
- 📋 **Assessment**: Overall evaluation
- ✅ **Strengths**: What's working
- ❌ **Weaknesses**: Areas to improve
- 🔑 **Keywords**: Suggested keywords to add
- ✏️ **Content Tips**: Stronger phrasing
- 📊 **Gap Analysis**: Skill gaps vs job (requires job description)
- 💡 **Interview Tips**: Talking points

### Performance
- Most AI queries: 5-15 seconds
- Gap Analysis: 15-20 seconds
- Mistral model: 2-5 seconds (faster)
- Llama2 model: 10-15 seconds (better quality)

### Privacy
✅ All analysis happens locally
✅ No data sent to external servers
✅ No API keys or subscriptions required
✅ Works offline (after model download)

---

## 🐛 Troubleshooting

### Build Issues
```bash
# Clean and rebuild
dotnet clean
dotnet build

# Restore packages
dotnet restore
```

### Runtime Issues
```bash
# Check .NET version
dotnet --version

# Run in debug
dotnet run --configuration Debug
```

### AI Not Connecting
1. Install Ollama: https://ollama.ai
2. Start service: `ollama serve`
3. Verify: curl http://localhost:11434
4. Download model: `ollama pull llama2`

### HTTPS Certificate Issues
First run, accept the certificate or set:
```bash
export ASPNETCORE_ENVIRONMENT=Development
```

---

## 📋 Requirements

### System Requirements
- **CPU**: Intel i5+ / AMD equivalent
- **RAM**: 
  - Basic (no AI): 2GB
  - With AI: 8GB+ (for 7B models)
  - With AI: 16GB+ (for 13B models)
- **Disk**: 2GB for app + 4-8GB for AI model

### Software Requirements
- .NET 10.0 SDK (or later)
- Windows 10+, macOS 10.15+, or Linux

### For AI Features
- Ollama (https://ollama.ai)
- 8GB RAM minimum
- 4GB disk for model

---

## 📝 Usage Examples

### Get ATS Score
```
1. Upload resume.pdf
2. See instant score (e.g., 78/100)
3. Review issues in "Issues" tab
```

### Fix ATS Issues
```
1. See "Issue: Contains tables"
2. Remove tables from resume
3. Upload fixed version
4. Score should improve
```

### Optimize for Job
```
1. Upload resume
2. Paste job description in Step 3
3. See "78% keyword match"
4. Add suggested keywords
5. Re-upload to verify improvement
```

### Get AI Insights
```
1. Upload resume
2. Click "Analyze with AI"
3. Review strengths/weaknesses
4. Read interview tips
5. Follow content improvements
```

---

## 🔐 Security & Privacy

- ✅ **Local Processing**: Everything on your machine
- ✅ **No Cloud**: No internet required (after setup)
- ✅ **No Tracking**: Zero telemetry or analytics
- ✅ **Open Source**: Code is transparent
- ✅ **HTTPS**: Secure by default

---

## 📄 File Support

### Input Formats
- ✅ PDF (text extraction)
- ✅ DOCX (Word documents)
- ✅ TXT (plain text)
- ❌ Images (not supported)
- ❌ RTF (not supported)

### Output Formats
- ✅ Plain Text (best for ATS)
- ✅ DOCX (Word format)
- ✅ Auto-optimized (system chooses)

### Max File Size
- 10MB per file

---

## 🎓 Learning Resources

### Understanding ATS
- ATS systems parse resumes as text, not design
- They look for specific keywords
- They can't read tables, graphics, or special formatting
- Plain text is safest option

### Resume Best Practices
- Use standard fonts (Arial, Calibri, Times New Roman)
- Standard section names (Work Experience, Education, Skills)
- Quantified achievements (increased by 25%, saved $50k)
- Industry keywords
- Consistent date formatting

### Keywords to Include
- Soft skills: Leadership, Communication, Problem-solving
- Technical: Depends on role
- Certifications: Relevant to industry
- Tools/Software: Used in job description

---

## 🚀 Future Enhancements

Potential future features:
- [ ] CV (European format) support
- [ ] Multi-language support
- [ ] Cover letter optimization
- [ ] Interview simulation
- [ ] LinkedIn integration
- [ ] Resume templates
- [ ] Batch processing
- [ ] Analytics dashboard

---

## 📞 Support

### Getting Help
1. Check [OLLAMA-SETUP.md](OLLAMA-SETUP.md) for AI setup
2. Review [AI-FEATURES.md](AI-FEATURES.md) for feature details
3. Check troubleshooting sections above

### Reporting Issues
- Save error message
- Note resume type (PDF/DOCX/TXT)
- Provide steps to reproduce

---

## 📄 License

Check LICENSE file for details.

---

## ✅ Testing

### What to Test
1. Upload different file formats (PDF, DOCX, TXT)
2. Verify ATS scores are reasonable
3. Check export formats look correct
4. Test AI analysis with various resumes
5. Test job description matching

### Expected Results
- ATS Score should be 0-100
- Issues should be realistic
- Keywords should be relevant
- AI analysis should be professional
- Exports should be readable

---

## 🎯 Next Steps

1. **Build**: `dotnet build`
2. **Setup AI**: Follow [OLLAMA-SETUP.md](OLLAMA-SETUP.md)
3. **Run**: `dotnet run`
4. **Test**: Upload a resume
5. **Optimize**: Add keywords and re-analyze

---

## 📈 Performance

### Processing Times
- Upload: Instant
- ATS Analysis: <1 second
- Keyword Analysis: <1 second
- AI Analysis: 5-15 seconds (model dependent)
- Export: <1 second

### Memory Usage
- Idle: ~50MB
- Analyzing: ~100-200MB
- AI Analysis: +300MB (Ollama overhead)

---

**Ready to optimize your resume?** 🚀

See [OLLAMA-SETUP.md](OLLAMA-SETUP.md) for AI setup or start using the ATS features immediately!
