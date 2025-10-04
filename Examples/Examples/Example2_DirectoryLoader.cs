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
    /// Load all text files from a directory.
    /// </summary>
    public static class Example2_DirectoryLoader
    {
        public static async Task Run(string query)
        {
            Console.WriteLine("=== Example 2: DirectoryLoader - Multiple Files ===\n");

            var docsPath = Path.Combine(AppContext.BaseDirectory, "documents");

            // Load all .txt files from directory
            var loader = new DirectoryLoader(
                searchPattern: "*.txt",
                recursive: true
            );
            var docs = await loader.LoadAsync(docsPath);

            Console.WriteLine($"Loaded {docs.Count} documents\n");

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