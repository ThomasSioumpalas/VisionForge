using c_llamaproject.Services;
using DotNetEnv;

Env.Load();
string apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
    ?? throw new Exception("OPENROUTER_API_KEY not found in .env file");


string modelPath = @"C:\Users\thomas\Downloads\llava-v1.6-mistral-7b.Q5_K_M.gguf";
string outputFolder = @"C:\Users\thomas\Downloads\c#llamaproject\c#llamaproject\Outputs";

// Services
var openRouter = new OpenRouterService(apiKey);   // OpenRouter API (vision + image gen)
var imageService = new ImageService(outputFolder); // saves + opens images
using var chatService = new ChatService(modelPath); // LLamaSharp text chat

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("=== Copyright-Free Image Generator ===\n");

Console.ForegroundColor = ConsoleColor.Green;
Console.Write("Enter path to your image: ");
string imagePath = Console.ReadLine() ?? "";

if (!File.Exists(imagePath))
{
    Console.WriteLine("Image not found. Exiting.");
    return;
}

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("\nAnalyzing image...");
string analysis = await openRouter.AnalyzeImageAsync(imagePath);

string prompt = analysis.Contains("PROMPT:")
    ? analysis.Substring(analysis.IndexOf("PROMPT:") + 7).Trim()
    : analysis;

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("\nExtracted Prompt:\n" + prompt);

chatService.SetContext($"An image was analyzed. Here is the analysis:\n{analysis}");

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("\nGenerating copyright-free image...");
byte[] generatedImage = await openRouter.GenerateImageAsync(prompt);

string savedPath = await imageService.SaveImageAsync(generatedImage);
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"\nImage saved to: {savedPath}");
imageService.OpenImage(savedPath); // opens in Windows Photos

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("\nType changes to regenerate (e.g. 'make it warmer colors')");
Console.WriteLine("Type 'save' to keep current image, 'exit' to quit\n");

byte[] currentImage = generatedImage;

while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("You: ");
    string input = Console.ReadLine() ?? "";

    if (input.ToLower() == "exit") break;

    if (input.ToLower() == "save")
    {
        string finalPath = await imageService.SaveImageAsync(currentImage);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Saved to: {finalPath}");
        continue;
    }

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\nRegenerating...");
    string modifiedPrompt = $"{prompt}. Additional requirements: {input}";
    currentImage = await openRouter.GenerateImageAsync(modifiedPrompt);

    string newPath = await imageService.SaveImageAsync(currentImage);
    Console.WriteLine($"New image saved to: {newPath}");
    imageService.OpenImage(newPath);
}

Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("Goodbye!");
