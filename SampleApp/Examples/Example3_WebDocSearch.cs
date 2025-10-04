using RAGSharp.Embeddings.Providers;
using RAGSharp.IO;
using RAGSharp.RAG;
using RAGSharp.Stores;
using System.IO;

namespace SampleApp.Examples
{
    /// <summary>
    /// Load from web sources.
    /// </summary>
    public static class Example3_WebDocSearch
    {
        private static RagRetriever retriever;

        public static async Task Run(string query)
        {
            if (retriever == null)
            {
                var embeddingClient = new OpenAIEmbeddingClient(
                    baseUrl: "http://127.0.0.1:1234/v1",
                    apiKey: "lmstudio",
                    defaultModel: "text-embedding-3-small"
                );

                // Swap to FileVectorStore if you want persistence
                var store = new InMemoryVectorStore();

                retriever = new RagRetriever(embeddingClient, store);
            }

            Console.WriteLine("Loading web page (one-time init)...");
            var webDocs = await new WebSearchLoader()
                .LoadAsync(query);
            await retriever.AddDocumentsAsync(webDocs);

            Console.WriteLine("Ingestion complete.\n");

            // Search across all sources
            var results = await retriever.Search(query, topK: 3);

            Console.WriteLine($"Top {results.Count} results across all sources:\n");
            foreach (var r in results)
            {
                var source = r.Metadata.GetValueOrDefault("FileName")
                    ?? Path.GetFileName(r.Source);
                Console.WriteLine($"Score: {r.Score:F2} | Source: {source}\n{r.Content}\n");
            }
        }
    }
}
