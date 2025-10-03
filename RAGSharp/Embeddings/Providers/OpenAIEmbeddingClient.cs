using OpenAI;
using OpenAI.Embeddings;
using RAGSharp.RAG.Embeddings;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAGSharp.Embeddings.Providers
{
    public class OpenAIEmbeddingClient : IEmbeddingClient
    {
        private readonly ApiKeyCredential _credential;
        private readonly OpenAIClientOptions _options;
        private readonly string _defaultModel;

        public OpenAIEmbeddingClient(string baseUrl, string apiKey, string defaultModel)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("API key must be provided.", nameof(apiKey));
            if (string.IsNullOrWhiteSpace(defaultModel))
                throw new ArgumentException("Default model must be provided", nameof(defaultModel));

            _credential = new ApiKeyCredential(apiKey);
            _options = new OpenAIClientOptions { Endpoint = new Uri(baseUrl) };
            _defaultModel = defaultModel;
        }

        private EmbeddingClient CreateClient(string model = null)
            => new EmbeddingClient(model ?? _defaultModel, _credential, _options);

        public async Task<float[]> GetEmbeddingAsync(string input, string model = null)
        {
            var client = CreateClient(model);
            var resp = await client.GenerateEmbeddingAsync(input);
            return resp.Value.ToFloats().ToArray();
        }

        public async Task<IReadOnlyList<float[]>> GetEmbeddingsAsync(IEnumerable<string> inputs, string model = null)
        {
            var client = CreateClient(model);
            var resp = await client.GenerateEmbeddingsAsync(inputs);
            return resp.Value.Select(e => e.ToFloats().ToArray()).ToList();
        }
    }
}
