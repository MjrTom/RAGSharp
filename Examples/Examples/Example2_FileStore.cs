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
    public static class Example2_FileStore
    {
        public static async Task Run()
        {
            Console.WriteLine("=== Example 2: File-backed store ===");

            var store = new FileVectorStore();

            IEmbeddingClient embeddings = new OpenAIEmbeddingClient(
                baseUrl: "http://127.0.0.1:1234/v1",
                apiKey: "lmstudio",
                defaultModel: "text-embedding-3-small"
            );

            ITokenizer tokenizer = new SharpTokenTokenizer("gpt-3.5-turbo");

            var retriever = new RagRetriever(embeddings, store, tokenizer);

            var baseDir = AppContext.BaseDirectory;
            var filePath = Path.Combine(baseDir, "sample.txt");

            var loader = new FileLoader();
            var docs = await loader.LoadAsync(filePath);

            await retriever.AddDocumentsAsync(docs);

            var results = await retriever.Search("black holes");
            foreach (var r in results)
                Console.WriteLine($"{r.Score:F2} - {r.Content}");
        }
    }
}
