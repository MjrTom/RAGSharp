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
    public static class Example5_Manual
    {
        public static async Task Run()
        {
            Console.WriteLine("=== Example 5: Manual document ===");

            var store = new InMemoryVectorStore();

            IEmbeddingClient embeddings = new OpenAIEmbeddingClient(
                baseUrl: "http://127.0.0.1:1234/v1",
                apiKey: "lmstudio",
                defaultModel: "text-embedding-3-small"
            );

            ITokenizer tokenizer = new SharpTokenTokenizer("gpt-3.5-turbo");

            var retriever = new RagRetriever(embeddings, store, tokenizer);

            var doc = new Document("RAG stands for Retrieval-Augmented Generation.", "manual");
            await retriever.AddDocumentAsync(doc);

            var results = await retriever.Search("what is RAG?");
            foreach (var r in results)
                Console.WriteLine($"{r.Score:F2} - {r.Content}");
        }
    }
}
