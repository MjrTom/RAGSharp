using RAGSharp.Embeddings.Providers;
using RAGSharp.IO;
using RAGSharp.RAG;
using RAGSharp.Stores;

namespace SampleApp.Examples
{
    /// <summary>
    /// Load from files, directories, or web sources.
    /// </summary>
    public static class Example3_WebDocLoading
    {
        public static async Task Run(string query)
        {
            Console.WriteLine("=== Loading Data: Files, Directories, Web ===\n");

            var embeddingClient = new OpenAIEmbeddingClient(
               baseUrl: "http://127.0.0.1:1234/v1",
               apiKey: "lmstudio",
               defaultModel: "text-embedding-3-small"
           );

            var retriever = new RagRetriever(embeddingClient, new InMemoryVectorStore());

            // Example C: Web page (requires RAGSharp.Web package)
            Console.WriteLine("Loading web page...");
            var webDocs = await new UrlLoader()
                .LoadAsync("https://en.wikipedia.org/wiki/Vector_database");
            await retriever.AddDocumentsAsync(webDocs);

            Console.WriteLine();

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