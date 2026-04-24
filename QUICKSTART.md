# 🚀 Quick Start Guide - ATSklar AI Resume Optimizer

## 5-Minute Setup

### Prerequisites
- .NET 10.0 SDK installed
- Windows/Mac/Linux

### Step 1: Build
```bash
cd e:\ATSklar
dotnet build
```

### Step 2: Run
```bash
dotnet run
```

### Step 3: Open Browser
Visit: **https://localhost:5001** or **http://localhost:5000**

---

## First Use (Without AI)

1. **Upload**: Select any resume (PDF, DOCX, TXT)
2. **View Score**: Get instant ATS compatibility score (0-100)
3. **Review Issues**: See what ATS systems detect
4. **Fix & Re-upload**: Improve and verify
5. **Export**: Download optimized resume

⏱️ **Time**: 2-3 minutes

---

## Enable AI Features (Optional)

### 1. Install Ollama
Download from: **https://ollama.ai**

### 2. Download Model
```bash
ollama pull llama2
```
(Other options: mistral, neural-chat - see README)

### 3. Start Service
```bash
ollama serve
```

### 4. Refresh Page
The app will detect Ollama automatically.

⏱️ **Total Time**: 5-10 minutes (first run)

---

## Using AI Analysis

### With AI Ready:

1. **Upload resume** → Step 1-2 complete
2. **Click "🤖 Analyze with AI"** → Step 4
3. **See AI insights**:
   - Resume assessment
   - Strengths & weaknesses
   - Suggested keywords
   - Content improvements
   - Interview tips

### With Job Description (Bonus):

1. **Paste job posting** in Step 4
2. **Click "Analyze with AI"**
3. See:
   - Keyword match %
   - Gap analysis vs requirements
   - Specific recommendations

⏱️ **AI Analysis Time**: 5-15 seconds

---

## Essential Files to Know

| File | Purpose |
|------|---------|
| `README.md` | Full documentation |
| `OLLAMA-SETUP.md` | AI setup details |
| `AI-FEATURES.md` | AI capabilities guide |
| `appsettings.json` | Configuration |
| `Pages/ResumeModifier.cshtml` | Main UI |
| `Services/AIResumeAnalyzer.cs` | AI engine |

---

## Troubleshooting

### "AI Not Connected"
```bash
# Check if Ollama is running
ollama list

# If not, start it
ollama serve
```

### Build Errors
```bash
# Clean and rebuild
dotnet clean
dotnet build
```

### Port Already in Use
```bash
# Use different port
dotnet run --urls="http://localhost:5002"
```

---

## Features Summary

✅ **No AI Needed:**
- ATS Score (0-100)
- Issue detection
- Keyword analysis
- Export formats

✨ **With Ollama:**
- AI Assessment
- Strengths/Weaknesses
- Content tips
- Gap analysis
- Interview tips

---

## Example Workflow

```
1. Upload resume.pdf (2 seconds)
   ↓
2. Get ATS Score: 72/100 (1 second)
   ↓
3. See Issues: "Contains tables" (instant)
   ↓
4. Paste job description (5 seconds)
   ↓
5. Keyword match: 65% (1 second)
   ↓
6. Click AI Analyze (10 seconds - needs Ollama)
   ↓
7. Review AI insights (2 minutes reading)
   ↓
8. Export optimized resume (1 second)
   ↓
✅ DONE - Ready to submit!
```

---

## Pro Tips

### Maximize ATS Score
- Remove tables/graphics
- Use standard fonts
- Include relevant keywords
- Target: 85+/100

### Prepare for Interviews
1. Upload resume
2. Paste job posting
3. Review AI interview tips
4. Practice talking points

### Update for Multiple Jobs
- Don't change one resume
- Add relevant keywords per job
- Re-upload to verify score
- Export fresh version

---

## Configuration Quick Reference

### Change AI Model (faster)
File: `appsettings.json`
```json
"Model": "mistral"  // instead of "llama2"
```

### Use Remote Ollama
```json
"OllamaUrl": "http://192.168.1.100:11434"
```

### Use Different Port
```bash
dotnet run --urls="http://localhost:5002"
```

---

## What Happens Behind the Scenes

### Traditional ATS Analysis (Instant)
```
Resume → Extract Text → Scan for:
  ✓ Tables/graphics
  ✓ Special characters
  ✓ Missing sections
  ✓ Formatting issues
  → Score: 0-100
```

### AI Analysis (5-15 seconds)
```
Resume + AI Model → Generate:
  ✓ Professional assessment
  ✓ Strengths identified
  ✓ Weaknesses highlighted
  ✓ Keywords suggested
  ✓ Improvement tips
  ✓ Interview guidance
```

---

## Next Steps

### Choice 1: Use Now (No AI)
✅ Build → Run → Upload resume → Done!

### Choice 2: Add AI (Recommended)
1. Install Ollama: https://ollama.ai
2. `ollama pull llama2`
3. `ollama serve`
4. Refresh app → AI features ready!

### Choice 3: Learn More
- Read `README.md` for full docs
- Read `OLLAMA-SETUP.md` for AI details
- Read `AI-FEATURES.md` for capabilities

---

## Performance Expectations

### ATS Analysis
- Upload: Instant
- Score: <1 second
- Review: 2-3 minutes
- Export: <1 second

### AI Analysis (with Ollama)
- Assessment: 10-15 seconds
- Strengths: 10-15 seconds
- Keywords: 5-10 seconds
- Content tips: 10-15 seconds
- Gap analysis: 15-20 seconds

### Full Workflow
- Without AI: 5-10 minutes
- With AI: 10-20 minutes (includes AI processing)

---

## Recommended Models

### Best For: Beginners
```bash
ollama pull mistral
# ✓ Fastest (2-5s)
# ✓ Small download (4GB)
# ✓ Great quality
```

### Best For: Accuracy
```bash
ollama pull llama2
# ✓ Most reliable
# ✓ Best reasoning
# ✓ Recommended default
```

### Best For: Low Resources
```bash
ollama pull orca-mini
# ✓ Tiny (1.5GB)
# ✓ Very fast
# ✓ Basic quality
```

---

## Common Questions

**Q: Do I need AI?**
A: No! Basic ATS features work great alone. AI adds deeper insights.

**Q: Is it secure?**
A: Yes! Everything runs locally. No data sent to servers.

**Q: How much disk space?**
A: App: 2GB. AI model: 4-8GB depending on choice.

**Q: Can I use on different machine?**
A: Yes! Point appsettings.json to remote Ollama instance.

**Q: What if Ollama crashes?**
A: App still works! Just basic ATS features without AI.

---

## Getting Help

1. **Setup issues**: See `OLLAMA-SETUP.md`
2. **Feature questions**: See `AI-FEATURES.md`
3. **Full docs**: See `README.md`
4. **Error messages**: Check browser console or app logs

---

**Ready?** 🚀

```bash
cd e:\ATSklar
dotnet run
```

Then visit: **http://localhost:5000**

Enjoy optimizing your resume!
