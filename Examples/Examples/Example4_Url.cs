using RAGSharp;
using RAGSharp.Embeddings;
using RAGSharp.Embeddings.Providers;
using RAGSharp.Embeddings.Tokenizers;
using RAGSharp.IO;
using RAGSharp.RAG;
using RAGSharp.RAG.Embeddings;
using RAGSharp.Stores;

namespace SampleApp.Examples
{
    public static class Example4_Url
    {
        public static async Task Run()
        {
            Console.WriteLine("=== Example 4: URL loader (Wikipedia page) ===");

            var store = new InMemoryVectorStore();

            IEmbeddingClient embeddings = new OpenAIEmbeddingClient(
                baseUrl: "http://127.0.0.1:1234/v1",
                apiKey: "lmstudio",
                defaultModel: "text-embedding-3-small"
            );

            ITokenizer tokenizer = new SharpTokenTokenizer("gpt-3.5-turbo");

            var retriever = new RagRetriever(embeddings, store, tokenizer);

            var docs = await new UrlLoader().LoadAsync("https://en.wikipedia.org/wiki/Artificial_intelligence");
            await retriever.AddDocumentsAsync(docs);

            var results = await retriever.Search("machine learning history");
            foreach (var r in results)
                Console.WriteLine($"{r.Score:F2} - {r.Content}");
        }
    }
}
