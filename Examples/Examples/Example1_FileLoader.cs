using RAGSharp.Embeddings.Providers;
using RAGSharp.Embeddings.Tokenizers;
using RAGSharp.IO;
using RAGSharp.RAG;
using RAGSharp.Stores;
using RAGSharp.Text;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SampleApp.Examples
{
    /// <summary>
    /// Load a single text file and search it.
    /// </summary>
    public static class Example1_FileLoader
    {
        public static async Task Run(string query)
        {
            Console.WriteLine("=== Example 1: FileLoader - Single Text File ===\n");

            // Load file
            var docs = await new FileLoader().LoadAsync(
                path: Path.Combine(AppContext.BaseDirectory, "sample.txt"));

            // Create retriever
            var tokenizer = new SharpTokenTokenizer("gpt-3.5-turbo");
            var retriever = new RagRetriever(
                embeddings: new OpenAIEmbeddingClient(
                    baseUrl: "http://127.0.0.1:1234/v1",
                    apiKey: "lmstudio",
                    defaultModel: "publisherme/bge/bge-large-en-v1.5-q4_k_m.gguf"
                ),
                store: new InMemoryVectorStore(),
                splitter: new RecursiveTextSplitter(tokenizer)
            );

            // Ingest documents
            await retriever.AddDocumentsAsync(docs);

            // Search
            var results = await retriever.Search(query, topK: 3);

            Console.WriteLine($"\nTop {results.Count} results for: \"{query}\"");
            foreach (var r in results)
            {
                Console.WriteLine(
                    $"Score: {r.Score:F2} | Source: {r.Metadata.GetValueOrDefault("FileName", "unknown")}\n" +
                    $"Text: {r.Content}\n"
                );
            }
        }
    }
}