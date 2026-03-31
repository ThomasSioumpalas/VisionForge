ImageReforge

A C# console application that analyzes a copyrighted image, generates a copyright-free version of it, and allows follow-up conversation about the result.

# What it does

1. You provide an image path
2. Gemini 3.1 Flash (via OpenRouter) analyzes the image and extracts a detailed description
3. The same model generates a new copyright-free image based on that description
4. The generated image opens automatically in Windows Photos
5. You can type modifications to regenerate, or ask text questions answered by a local LLM
6. Type "save" to save the current image to the Outputs/ folder

# Architecture

```
User provides image path
        |
OpenRouterService (HTTP)
        |-- AnalyzeImageAsync  --> Gemini 3.1 Flash reads image, returns ANALYSIS + PROMPT
        |-- GenerateImageAsync --> Gemini 3.1 Flash generates new image from PROMPT
        |
ImageService
        |-- SaveImageAsync     --> saves .png to Outputs/ folder
        |-- OpenImage          --> opens image in Windows default photo viewer
        |
ChatService (LLamaSharp)
        |-- ChatAsync          --> answers follow-up text questions using local GGUF model
```

# Tech Stack

| Component                              | What it is               | Used for                             |
| -------------------------------------- | ------------------------ | ------------------------------------ |
| C# / .NET 9                            | Application language     | App logic, HTTP calls, file handling |
| OpenRouter API                         | Cloud AI gateway         | Routes requests to Gemini 3.1 Flash  |
| Gemini 3.1 Flash (Nano Banana 2)       | Google multimodal model  | Image analysis + image generation    |
| LLamaSharp 0.25.0                      | C# wrapper for llama.cpp | Local text chat                      |
| llama.cpp (via LLamaSharp.Backend.Cpu) | C++ inference engine     | Runs GGUF model in memory            |
| DotNetEnv                              | NuGet package            | Reads API key from .env file         |
| System.Net.Http                        | Built-in .NET            | HTTP requests to OpenRouter          |
| System.Diagnostics                     | Built-in .NET            | Opens image in Windows Photos        |

# Project Structure

```
ImageReforge/
├── .env                      <- API key (never commit this)
├── .gitignore
├── Program.cs                <- entry point and main loop
├── Services/
│   ├── OpenRouterService.cs  <- Gemini image analysis + generation via OpenRouter
│   ├── ImageService.cs       <- saves and opens generated images
│   └── ChatService.cs        <- local text chat via LLamaSharp
├── Inputs/                   <- put your source images here
└── Outputs/                  <- generated images are saved here
```

# Requirements

- .NET 9 SDK
- Visual Studio 2022
- An OpenRouter account with credits
- A local GGUF model file for text chat (LLamaSharp)

# Downloads

### 1. OpenRouter API Key

1. Go to https://openrouter.ai
2. Sign up and go to Settings > Keys
3. Create a new key
4. Add credits at https://openrouter.ai/settings/credits (minimum $5 recommended)

### 2. Local GGUF model (for text chat via LLamaSharp)

Go to:

```
https://huggingface.co/cjpais/llava-1.6-mistral-7b-gguf
```

Download: llava-v1.6-mistral-7b.Q4_K_M.gguf (~4.37 GB)

Place it anywhere on your machine and update the path in Program.cs.

### 3. NuGet packages

In Visual Studio Package Manager Console:

```powershell
Install-Package LLamaSharp -Version 0.25.0
Install-Package LLamaSharp.Backend.Cpu -Version 0.25.0
Install-Package DotNetEnv
```

---

# Setup

### 1. Create .env file

Create a file named .env in the project root (same folder as Program.cs):

```
OPENROUTER_API_KEY=sk-or-v1-yourfullkeyhere
```

In Solution Explorer, right-click .env > Properties:

- Build Action: None
- Copy to Output Directory: Copy if newer

### 2. Update paths in Program.cs

```csharp
// Path to your local GGUF model for text chat
string modelPath = @"C:\path\to\llava-v1.6-mistral-7b.Q4_K_M.gguf";

// Path to your Outputs folder inside the project
string outputFolder = @"C:\path\to\your\project\Outputs";
```

# Running the app

Press F5 in Visual Studio.

```
=== Copyright-Free Image Generator ===

Enter path to your image: C:\Users\thomas\Pictures\example.jpg

Analyzing image...

Extracted Prompt:
A photorealistic scene of a mountain landscape at sunset...

Generating copyright-free image...

Image saved to: C:\...\Outputs\generated_20260331_142301.png

Type changes to regenerate (e.g. 'make it warmer colors')
Type 'save' to keep current image, 'exit' to quit

You: make the sky more dramatic
Regenerating...

You: save
Saved to: C:\...\Outputs\generated_20260331_142415.png

You: exit
Goodbye!
```

## Notes

- Gemini 3.1 Flash handles both image understanding and image generation in a single model
- LLamaSharp runs locally - no internet needed for text chat
- Generated images are saved as PNG with a timestamp filename
