using System.Collections.Generic;
using System.Threading.Tasks;

namespace RAGSharp.RAG.Embeddings
{
    public interface IEmbeddingClient
    {
        Task<float[]> GetEmbeddingAsync(string input, string? model = null);
        Task<IReadOnlyList<float[]>> GetEmbeddingsAsync(IEnumerable<string> inputs, string? model = null);
    }
}
