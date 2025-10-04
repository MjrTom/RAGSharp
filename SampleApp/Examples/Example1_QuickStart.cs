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
        static RagRetriever retriever;

        public static async Task Run(string query)
        {
            if (retriever == null)
            {
                var embeddings = new OpenAIEmbeddingClient(
                    baseUrl: "http://127.0.0.1:1234/v1",
                    apiKey: "lmstudio",
                    defaultModel: "text-embedding-3-small"
                );

                var store = new InMemoryVectorStore(); //use new FileVectorStore() for presistence

                retriever = new RagRetriever(embeddings, store);

                var docs = await new FileLoader().LoadAsync(
                    Path.Combine(AppContext.BaseDirectory, "data", "sample.txt"));
                await retriever.AddDocumentsAsync(docs);
            }

            var results = await retriever.Search(query, topK: 3);

            Console.WriteLine($"Top {results.Count} results for: \"{query}\"\n");
            foreach (var r in results)
                Console.WriteLine($"Score: {r.Score:F2}\n{r.Content}\n");
        }
    }

}