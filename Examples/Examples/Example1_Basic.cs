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
    public static class Example1_Basic
    {
        public static async Task Run()
        {
            Console.WriteLine("=== Example 1: InMemory store + FileLoader ===");

            var store = new InMemoryVectorStore();

            IEmbeddingClient embeddings = new OpenAIEmbeddingClient(
                baseUrl: "http://127.0.0.1:1234/v1", // LM Studio, OpenAI API, etc.
                apiKey: "lmstudio",                   // Or Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                defaultModel: "text-embedding-3-small"
            );

            ITokenizer tokenizer = new SharpTokenTokenizer("gpt-3.5-turbo");

            var retriever = new RagRetriever(embeddings, store, tokenizer);

            var baseDir = AppContext.BaseDirectory;
            var filePath = Path.Combine(baseDir, "sample.txt");

            var loader = new FileLoader();
            var docs = await loader.LoadAsync(filePath);

            await retriever.AddDocumentsAsync(docs);

            var results = await retriever.Search("quantum entanglement", topK: 3);
            foreach (var r in results)
                Console.WriteLine($"{r.Score:F2} - {r.Content}");
        }
    }
}
