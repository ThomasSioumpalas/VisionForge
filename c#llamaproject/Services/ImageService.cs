using System.Diagnostics;

namespace c_llamaproject.Services
{
    public class ImageService
    {
        private readonly string _outputFolder;

        public ImageService(string outputFolder)
        {
            _outputFolder = outputFolder;
            Directory.CreateDirectory(outputFolder);
        }

        public async Task<string> SaveImageAsync(byte[] imageBytes)
        {
            string filename = $"generated_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string fullPath = Path.Combine(_outputFolder, filename);

            await File.WriteAllBytesAsync(fullPath, imageBytes);
            return fullPath;
        }

        public void OpenImage(string imagePath)
        {

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = imagePath,
                UseShellExecute = true
            });
        }
    }
}