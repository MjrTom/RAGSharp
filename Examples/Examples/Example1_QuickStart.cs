using RAGSharp.Embeddings.Providers;
using RAGSharp.IO;
using RAGSharp.RAG;
using RAGSharp.Stores;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SampleApp.Examples
{
    /// <summary>
    /// Simplest possible example: Load a file, search it.
    /// </summary>
    public static class Example1_QuickStart
    {
        public static async Task Run(string query)
        {
            Console.WriteLine("=== Quick Start: Load → Index → Search ===\n");

            // 1. Load document
            var docs = await new FileLoader().LoadAsync(
                Path.Combine(AppContext.BaseDirectory, "sample.txt"));

            // 2. Create retriever (uses sensible defaults)
            var retriever = new RagRetriever(
                embeddings: new OpenAIEmbeddingClient(
                    baseUrl: "http://127.0.0.1:1234/v1",
                    apiKey: "lmstudio",
                    defaultModel: "text-embedding-3-small"
                ),
                store: new InMemoryVectorStore() //use new FileVectorStore(filePath, fileName); for presistence vector store
            );

            await retriever.AddDocumentsAsync(docs);

            // 3. Search
            var results = await retriever.Search(query, topK: 3);

            Console.WriteLine($"Top {results.Count} results for: \"{query}\"\n");
            foreach (var r in results)
            {
                Console.WriteLine($"Score: {r.Score:F2}\n{r.Content}\n");
            }
        }
    }
}