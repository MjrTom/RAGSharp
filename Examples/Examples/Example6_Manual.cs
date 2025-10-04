using RAGSharp.Embeddings;
using RAGSharp.Embeddings.Providers;
using RAGSharp.Utils;

namespace SampleApp.Examples
{
    public static class Example6_Manual
    {
        public static async Task Run()
        {
            Console.WriteLine("=== Example 5: Manual document ===");

            IEmbeddingClient embeddings = new OpenAIEmbeddingClient(
               baseUrl: "http://127.0.0.1:1234/v1",
               apiKey: "lmstudio",
               defaultModel: "text-embedding-3-small"
           );

            var text1 = "Quantum entanglement links particles at a distance.";
            var text2 = "Particles can share states instantly even when far apart.";

            var v1 = (await embeddings.GetEmbeddingAsync(text1)).Normalize();
            var v2 = (await embeddings.GetEmbeddingAsync(text2)).Normalize();

            var score = v1.CosineSimilarity(v2);

            Console.WriteLine($"Similarity: {score:F4}");
        }
    }
}
