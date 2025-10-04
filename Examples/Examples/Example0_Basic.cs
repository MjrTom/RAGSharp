using RAGSharp.Embeddings;
using RAGSharp.Embeddings.Providers;
using RAGSharp.Embeddings.Tokenizers;
using RAGSharp.IO;
using RAGSharp.RAG;
using RAGSharp.Stores;

namespace SampleApp.Examples
{
    public static class Example0_Basic
    {
        public static async Task Run(string query)
        {
            Console.WriteLine("=== Example 1: InMemory store + FileLoader ===");

            //Load 
            var docs = await new FileLoader().LoadAsync(
                path: Path.Combine(AppContext.BaseDirectory, "sample.txt"));

            //Ingest
            var retriever = new RagRetriever(
                embeddings: new OpenAIEmbeddingClient(
                    baseUrl: "http://127.0.0.1:1234/v1", // LM Studio, OpenAI API, etc.
                    apiKey: "lmstudio",                   // Or Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                    defaultModel: "publisherme/bge/bge-large-en-v1.5-q4_k_m.gguf"
                ),
                store: new InMemoryVectorStore());

            await retriever.AddDocumentsAsync(docs);

            //Search
            var results = await retriever.Search(query, topK: 3);

            Console.WriteLine($"\nTop {results.Count} results for: \"{query}\"");
            foreach (var r in results)
                Console.WriteLine(
                    $"Score: {r.Score:F2} | Source: {r.Metadata.GetValueOrDefault("FileName", "unknown")}\n" +
                    $"Text: {r.Content}\n"
                );
        }
    }
}
