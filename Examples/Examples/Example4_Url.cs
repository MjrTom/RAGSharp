using RAGSharp.Embeddings;
using RAGSharp.Embeddings.Providers;
using RAGSharp.Embeddings.Tokenizers;
using RAGSharp.IO;
using RAGSharp.RAG;
using RAGSharp.Stores;

namespace SampleApp.Examples
{
    public static class Example4_Url
    {
        public static async Task Run(string query)
        {
            Console.WriteLine("=== Example 4: URL loader (Wikipedia page) ===");

            var store = new InMemoryVectorStore();

            IEmbeddingClient embeddings = new OpenAIEmbeddingClient(
                baseUrl: "http://127.0.0.1:1234/v1",
                apiKey: "lmstudio",
                defaultModel: "text-embedding-3-small"
            );

            ITokenizer tokenizer = new SharpTokenTokenizer("gpt-3.5-turbo");

            var retriever = new RagRetriever(embeddings, store);

            var docs = await new UrlLoader().LoadAsync("https://en.wikipedia.org/wiki/Quantum_mechanics");
            await retriever.AddDocumentsAsync(docs);

            var results = await retriever.Search(query);

            Console.WriteLine($"\nTop {results.Count} results for: \"{query}\"");
            foreach (var r in results)
                Console.WriteLine(
                    $"Score: {r.Score:F2} | Source: {r.Source}\n" +
                    $"Text: {r.Content}\n"
                );
        }
    }
}
