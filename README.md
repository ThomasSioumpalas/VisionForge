# 🦙 LLaVA + LLamaSharp Image Chat — C# Console App

A multimodal AI console application built in C# that:
1. **Describes images** using LLaVA (vision model)
2. **Answers follow-up questions** about the image using LLamaSharp (text chat)

Supports two backends:
- **Ollama** (recommended) — local HTTP server, easier setup
- **CLI + LLamaSharp** (fallback) — direct process + in-memory inference

---

## 🏗️ Architecture

```
Your C# Console App
│
├── LlavaService       ← handles image description (vision)
│   ├── Ollama mode    → HTTP POST to localhost:11434 with base64 image
│   └── CLI mode       → spawns llama-llava-cli.exe as child process
│
└── ChatService        ← handles text conversation (language)
    ├── Ollama mode    → HTTP POST to localhost:11434/api/chat
    └── LLamaSharp     → loads GGUF model directly in memory via llama.cpp
```

---

## 📋 Requirements

### Runtime
- [.NET 8 or 9](https://dotnet.microsoft.com/download)
- Visual Studio 2022

### NuGet Packages
Install via Package Manager Console:
```powershell
Install-Package LLamaSharp -Version 0.25.0
Install-Package LLamaSharp.Backend.Cpu -Version 0.25.0
```

---

## 🔽 Downloads

### Option A: Ollama Backend (recommended)

**1. Install Ollama**
```powershell
winget install Ollama.Ollama
```

**2. Pull the LLaVA model**
```powershell
ollama pull llava
```

That's it — Ollama manages the model files internally.

---

### Option B: CLI + LLamaSharp Backend (fallback)

You need to download three things manually:

**1. llama.cpp CLI binaries (build b3621)**

Go to:
```
https://github.com/ggerganov/llama.cpp/releases/tag/b3621
```
Download: `llama-b3621-bin-win-avx2-x64.zip`

> ⚠️ Use b3621 specifically — newer builds dropped LLaVA v1.5 support.
> Use avx2 if your CPU supports AVX2 (most modern CPUs do).

Extract to: `C:\Users\<you>\Downloads\llama-b3621-bin-win-avx2-x64\`

---

**2. LLaVA main model (GGUF)**

Go to:
```
https://huggingface.co/cjpais/llava-1.6-mistral-7b-gguf
```
Download: `llava-v1.6-mistral-7b.Q4_K_M.gguf` (~4.37 GB)

> Q4_K_M = good quality/size balance. Q5_K_M is higher quality but larger.

---

**3. Vision encoder / mmproj (GGUF)**

From the same repo:
```
https://huggingface.co/cjpais/llava-1.6-mistral-7b-gguf
```
Download: `mmproj-model-f16.gguf` (~624 MB)

> ⚠️ The main model and mmproj MUST come from the same repo.
> Mixing files from different repos causes an `n_embd mismatch` error.

---

## ⚙️ Configuration

Open `Program.cs` and set your backend at the top:

```csharp
// true  = Ollama (needs ollama serve running)
// false = CLI + LLamaSharp (needs GGUF files downloaded)
const bool USE_OLLAMA = false;

// Ollama settings (only used when USE_OLLAMA = true)
const string OLLAMA_URL = "http://localhost:11434";
const string OLLAMA_MODEL = "llava";

// CLI / LLamaSharp settings (only used when USE_OLLAMA = false)
const string CLI_PATH   = @"C:\Users\thomas\Downloads\llama-b3621-bin-win-avx2-x64\llama-llava-cli.exe";
const string MODEL_PATH = @"C:\Users\thomas\Downloads\llava-v1.6-mistral-7b.Q4_K_M.gguf";
const string MMPROJ_PATH = @"C:\Users\thomas\Downloads\mmproj-model-f16.gguf";
```

---

## 🚀 Running the App

### If using Ollama (`USE_OLLAMA = true`)

**1. Start Ollama server** (keep this PowerShell window open):
```powershell
ollama serve
```

> If you get `bind: Only one usage of each socket address` — Ollama is already running. Skip this step.

**2. Verify Ollama is running** — open browser and go to:
```
http://localhost:11434
```
You should see: `Ollama is running`

**3. Run the project** in Visual Studio — press **F5**

---

### If using CLI + LLamaSharp (`USE_OLLAMA = false`)

Just press **F5** in Visual Studio — no server needed.

---

## 💬 Usage

```
=== LLaVA + LLaMA Chat ===
Backend: CLI + LLamaSharp

Image path (or press Enter to skip): C:\Users\thomas\Pictures\dogs.jpg

Analyzing image...

Image Description:
The image features a diverse group of dogs sitting and standing
together in a grassy field...

Image loaded! Ask questions about it.
Type your message or 'exit' to quit.

You: what breeds are in the image?
AI: Based on the description, I can identify...

You: exit
Goodbye!
```

---

## 📁 Project Structure

```
c#llamaproject/
├── Program.cs              ← entry point, backend config, main loop
├── Services/
│   ├── LlavaService.cs     ← image description (Ollama HTTP or CLI process)
│   └── ChatService.cs      ← text chat (Ollama HTTP or LLamaSharp in-memory)
└── Inputs/                 ← put your test images here
```

---

## 🔧 Tech Stack

| Component | What it is | Used for |
|---|---|---|
| **C#/.NET 9** | Application language | App logic, UI, HTTP calls |
| **LLamaSharp 0.25.0** | C# wrapper for llama.cpp | In-process text inference |
| **llama.cpp b3621** | C++ inference engine | CLI binary for LLaVA vision |
| **LLaVA v1.6** | Multimodal vision-language model | Image description |
| **GGUF** | Model file format | Stores quantized model weights |
| **Ollama** | Local model runtime/server | Optional HTTP backend |
| **System.Diagnostics** | Built-in .NET | Spawning CLI child process |
| **System.Text.Json** | Built-in .NET | Parsing Ollama HTTP responses |

---

## ❓ Troubleshooting

**`Unable to load DLL 'llava_shared'`**
→ LLamaSharp's CPU backend doesn't include this DLL in newer versions.
→ Use `USE_OLLAMA = false` with the CLI approach, or switch to `USE_OLLAMA = true`.

**`model 'llava' not found`**
→ Run `ollama pull llava` to download the model into Ollama.

**`No connection could be made (localhost:11434)`**
→ Ollama server isn't running. Run `ollama serve` in PowerShell.

**`TaskCanceledException` (timeout)**
→ LLaVA is slow on CPU. The HttpClient timeout has been increased to 10 minutes in `LlavaService.cs`.

**`n_embd mismatch` error**
→ Your main model and mmproj files are from different repos. Download both from the same HuggingFace repo.

**Image file not found**
→ Don't include quotes around the path when typing it in the console.

---

## 📝 Notes

- LLaVA **describes** images — it cannot **generate** images. For image generation you'd need Stable Diffusion.
- LLamaSharp handles text chat only — it never sees the actual image pixels, only the text description produced by LLaVA.
- Inference on CPU is slow (~3-4 tokens/second for a 7B model). GPU acceleration requires CUDA drivers and `LLamaSharp.Backend.Cuda12`.
