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
    /// Use persistent file-based vector store (survives restarts).
    /// </summary>
    public static class Example5_PersistentStore
    {
        public static async Task Run(string query)
        {
            Console.WriteLine("=== Example 5: FileVectorStore - Persistent Storage ===\n");

            var storePath = Path.Combine(AppContext.BaseDirectory, "vector_store.json");
            var storeExists = File.Exists(storePath);

            // Create retriever with persistent store
            var tokenizer = new SharpTokenTokenizer("gpt-3.5-turbo");
            var retriever = new RagRetriever(
                embeddings: new OpenAIEmbeddingClient(
                    baseUrl: "http://127.0.0.1:1234/v1",
                    apiKey: "lmstudio",
                    defaultModel: "publisherme/bge/bge-large-en-v1.5-q4_k_m.gguf"
                ),
                store: new FileVectorStore(storePath),
                splitter: new RecursiveTextSplitter(tokenizer)
            );

            if (!storeExists)
            {
                Console.WriteLine("Creating new vector store...\n");
                var docs = await new FileLoader().LoadAsync(
                    Path.Combine(AppContext.BaseDirectory, "sample.txt"));
                await retriever.AddDocumentsAsync(docs);
                Console.WriteLine("Store saved to disk\n");
            }
            else
            {
                Console.WriteLine("Using existing vector store (no re-indexing needed)\n");
            }

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