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
    public static class Example3_Directory
    {
        public static async Task Run()
        {
            Console.WriteLine("=== Example 3: Directory loader ===");

            var store = new InMemoryVectorStore();

            IEmbeddingClient embeddings = new OpenAIEmbeddingClient(
                baseUrl: "http://127.0.0.1:1234/v1",
                apiKey: "lmstudio",
                defaultModel: "text-embedding-3-small"
            );

            ITokenizer tokenizer = new SharpTokenTokenizer("gpt-3.5-turbo");

            var retriever = new RagRetriever(embeddings, store, tokenizer);

            var docs = await new DirectoryLoader().LoadAsync("docs/");
            await retriever.AddDocumentsAsync(docs);

            var results = await retriever.Search("neural networks", topK: 5);
            foreach (var r in results)
                Console.WriteLine($"{r.Score:F2} - {r.Content}");
        }
    }
}
