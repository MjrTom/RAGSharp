using RAGSharp.Embeddings;
using RAGSharp.Embeddings.Providers;
using RAGSharp.Utils;
using System;
using System.Threading.Tasks;

namespace SampleApp.Examples
{
    /// <summary>
    /// Low-level API: Direct embedding comparison without RAG pipeline.
    /// For advanced users who want manual control.
    /// </summary>
    public static class Example4_Barebones
    {
        public static async Task Run()
        {
            Console.WriteLine("=== Low-Level: Manual Embeddings ===\n");

            IEmbeddingClient embeddingClient = new OpenAIEmbeddingClient(
                baseUrl: "http://127.0.0.1:1234/v1",
                apiKey: "lmstudio",
                defaultModel: "text-embedding-3-small"
            );

            // Generate embeddings manually
            var text1 = "Quantum entanglement links particles at a distance.";
            var text2 = "Particles can share states instantly even when far apart.";
            var text3 = "Pizza is made with tomato sauce and cheese.";

            Console.WriteLine("Computing embeddings...");
            var v1 = (await embeddingClient.GetEmbeddingAsync(text1)).Normalize();
            var v2 = (await embeddingClient.GetEmbeddingAsync(text2)).Normalize();
            var v3 = (await embeddingClient.GetEmbeddingAsync(text3)).Normalize();

            // Calculate similarities manually
            var score12 = v1.CosineSimilarity(v2);
            var score13 = v1.CosineSimilarity(v3);

            Console.WriteLine($"\n\"{text1}\"");
            Console.WriteLine($"vs \"{text2}\"");
            Console.WriteLine($"Similarity: {score12:F4} (high = related)\n");

            Console.WriteLine($"\"{text1}\"");
            Console.WriteLine($"vs \"{text3}\"");
            Console.WriteLine($"Similarity: {score13:F4} (low = unrelated)\n");

            Console.WriteLine("💡 For most use cases, use RagRetriever instead of manual embeddings.");
        }
    }
}