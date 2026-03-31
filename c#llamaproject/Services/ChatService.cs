using System.Text;
using LLama;
using LLama.Common;
using LLama.Sampling;

namespace c_llamaproject.Services
{

    public class ChatService : IDisposable
    {
        private readonly LLamaWeights _model;
        private readonly LLamaContext _context;
        private readonly InteractiveExecutor _executor;
        private readonly InferenceParams _inferenceParams;
        private string _systemContext = "";

        public ChatService(string modelPath)
        {
            var parameters = new ModelParams(modelPath)
            {
                ContextSize = 4096,
                GpuLayerCount = 0 // CPU only
            };

            _model = LLamaWeights.LoadFromFile(parameters);
            _context = _model.CreateContext(parameters);
            _executor = new InteractiveExecutor(_context);

            _inferenceParams = new InferenceParams
            {
                MaxTokens = 512,
                AntiPrompts = new List<string> { "User:", "USER:" },
                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Temperature = 0.7f
                }
            };
        }

        public void SetContext(string context)
        {
            _systemContext = context;
        }

        public async Task<string> ChatAsync(string userMessage)
        {
            string fullPrompt = string.IsNullOrEmpty(_systemContext)
                ? $"USER:\n{userMessage}\nASSISTANT:\n"
                : $"{_systemContext}\n\nUSER:\n{userMessage}\nASSISTANT:\n";

            var response = new StringBuilder();

            await foreach (var token in _executor.InferAsync(fullPrompt, _inferenceParams))
            {
                response.Append(token);
                Console.Write(token);
            }

            return response.ToString().Trim();
        }

        public void Dispose()
        {
            _context?.Dispose();
            _model?.Dispose();
        }
    }
}