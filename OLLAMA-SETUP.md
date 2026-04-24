# 🤖 AI Resume Analyzer Setup Guide

## Overview
Your ATS Resume Optimizer now includes **AI-powered analysis** using **Ollama** - a local LLM that runs on your machine with complete privacy and zero API costs.

## What is Ollama?
Ollama is a simple way to run large language models locally. It's:
- ✅ **Privacy-First**: Everything stays on your computer
- ✅ **Free**: No API costs or subscriptions
- ✅ **Fast**: Local processing is instant
- ✅ **Easy**: One-command setup

## Installation Steps

### 1. Download Ollama
Visit **https://ollama.ai** and download Ollama for your OS:
- Windows
- macOS
- Linux

### 2. Install Ollama
Run the installer and follow the setup wizard. Ollama will start automatically.

### 3. Download a Model
Open a terminal and pull a model. For best results, use one of these:

**Option 1: Llama 2 (7B - Recommended)**
```bash
ollama pull llama2
```

**Option 2: Mistral (Faster, lighter)**
```bash
ollama pull mistral
```

**Option 3: Neural Chat (Optimized for chat)**
```bash
ollama pull neural-chat
```

Model sizes:
- **7B models** (llama2, mistral): ~4-5GB, runs on most computers
- **13B models** (neural-chat): ~8GB, needs more RAM
- **70B models** (llama2:70b): ~40GB, requires high-end GPU

### 4. Verify Installation
Run this command to list installed models:
```bash
ollama list
```

### 5. Start Ollama (if not auto-running)
```bash
ollama serve
```

The service will run on `http://localhost:11434`

### 6. Test in ATSklar
1. Refresh the ATS Resume Optimizer page
2. Upload a resume
3. Click "🤖 Analyze with AI"
4. If Ollama is running, you'll see ✨ AI-Powered Analysis section

## Configuration

### Change Ollama URL or Model
Edit `appsettings.json`:
```json
{
  "AI": {
    "OllamaUrl": "http://localhost:11434",
    "Model": "llama2"
  }
}
```

### Use Different Model
Simply change the model name in config:
```json
{
  "AI": {
    "Model": "mistral"
  }
}
```

### Remote Ollama Instance
If running Ollama on a different machine:
```json
{
  "AI": {
    "OllamaUrl": "http://192.168.1.100:11434",
    "Model": "llama2"
  }
}
```

## AI Features

### 1. 📋 Resume Assessment
- Overall quality evaluation
- Completeness check
- Professional effectiveness rating

### 2. ✅ Strengths & Weaknesses
- 3-4 key strengths identified
- 2-3 areas for improvement highlighted
- Specific, actionable feedback

### 3. 🔑 Keyword Suggestions
- Top missing keywords for your industry
- AI-suggested keywords to add
- Keyword variations based on job description

### 4. ✏️ Content Improvement
- Suggestions for stronger bullet points
- Achievement rewrite recommendations
- Action-oriented language improvements

### 5. 📊 Gap Analysis (with Job Description)
- Specific skill gaps vs job requirements
- Experience mismatches identified
- Targeted recommendations to close gaps

### 6. 💡 Interview Preparation Tips
- How to discuss your experience
- Frame your strengths effectively
- Talking points based on your resume

## Performance Tips

### For Slower Machines
- Use smaller models (7B instead of 13B)
- Close other applications
- Allocate more RAM to Ollama

### For Faster Response
- Use quantized models (4-bit/5-bit versions)
- Enable GPU if available (requires CUDA/Metal)
- Keep resume text concise (under 2000 words)

### Faster Models by Rank
1. **orca-mini** (fastest, 1.5GB)
2. **mistral** (2-5s response, 4GB)
3. **neural-chat** (5-10s response, 8GB)
4. **llama2** (10-15s response, 4-5GB)

## Troubleshooting

### "AI Not Connected" Error
- Ensure Ollama is running: `ollama serve`
- Verify URL is correct in appsettings.json
- Check firewall isn't blocking port 11434

### "Model not found"
- Download the model: `ollama pull llama2`
- Verify with `ollama list`

### Slow Responses
- Use a smaller model
- Close other applications
- Reduce resume length

### Out of Memory Error
- Use a smaller model (7B instead of 13B)
- Close other apps
- Add more RAM to system

## Command Reference

```bash
# List installed models
ollama list

# Pull/download a model
ollama pull llama2
ollama pull mistral

# Start Ollama service
ollama serve

# Remove a model (frees space)
ollama rm llama2

# Show model info
ollama show llama2

# Update Ollama
ollama update
```

## Model Recommendations by Use Case

### Best All-Rounder
**Llama 2 (7B)**
- Balanced speed & quality
- Great at reasoning
- Good resume analysis
```bash
ollama pull llama2
```

### Fastest Option
**Mistral (7B)**
- 2-5 seconds per response
- Very good quality
- Lowest latency
```bash
ollama pull mistral
```

### Most Accurate
**Neural Chat (7B)**
- Optimized for conversation
- Best for detailed analysis
- Slightly slower (~5s)
```bash
ollama pull neural-chat
```

### Budget Option
**Orca Mini (1.5B)**
- Tiny (1.5GB download)
- Very fast (<1s)
- Works for simple analysis
```bash
ollama pull orca-mini
```

## Advanced: GPU Acceleration

### On NVIDIA (CUDA)
Ollama automatically uses CUDA if detected. Ensure you have:
- NVIDIA driver installed
- CUDA toolkit
- 6GB+ VRAM recommended

### On Apple Silicon (Metal)
Automatic on M1/M2/M3 Macs.

### On AMD (ROCm)
Additional setup required - see Ollama docs.

## Privacy & Security

✅ **Your data stays local** - No data sent to external servers
✅ **No API keys needed** - No account required
✅ **Open source** - Community verified
✅ **Fully offline** - Works without internet after model download

## Resource Requirements

| Model | Size | RAM | Speed | Quality |
|-------|------|-----|-------|---------|
| Orca Mini | 1.5GB | 4GB | <1s | Basic |
| Mistral | 4GB | 8GB | 2-5s | Good |
| Llama 2 | 4GB | 8GB | 10-15s | Excellent |
| Neural Chat | 8GB | 16GB | 5-10s | Excellent |

## Next Steps

1. **Install Ollama** from ollama.ai
2. **Pull a model**: `ollama pull llama2`
3. **Start service**: `ollama serve`
4. **Upload resume** and click "🤖 Analyze with AI"
5. **Enjoy AI-powered resume insights!**

## Support & Resources

- **Ollama Website**: https://ollama.ai
- **Model Library**: https://ollama.ai/library
- **Community**: https://discord.gg/ollama
- **GitHub**: https://github.com/jmorganca/ollama

## FAQ

**Q: Is this secure?**
A: Yes, everything runs locally. No data leaves your computer.

**Q: Will it slow down my computer?**
A: Only while analyzing (5-15 seconds). Ollama doesn't run in background after analysis completes.

**Q: Can I use remote Ollama?**
A: Yes, update appsettings.json with remote server IP/port.

**Q: Which model should I choose?**
A: Start with Mistral or Llama2. They're fast and accurate.

**Q: Do I need GPU?**
A: No, CPU works fine. GPU is optional for faster processing.

**Q: How much internet is required?**
A: Only for initial model download. No internet needed after that.

---

**Ready?** Install Ollama and start getting AI-powered resume insights! 🚀
