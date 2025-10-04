using Microsoft.Extensions.Logging;
using RAGSharp.Embeddings.Providers;
using RAGSharp.Embeddings.Tokenizers;
using RAGSharp.IO;
using RAGSharp.Logging;
using RAGSharp.RAG;
using RAGSharp.Stores;
using RAGSharp.Text;
using RAGSharp.Utils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SampleApp.Examples
{
    /// <summary>
    /// Advanced usage: shows how to customize every injection point in RagRetriever.
    /// </summary>
    public static class Example5_Advanced
    {
        public static async Task Run(string query)
        {
            Console.WriteLine("=== Example 5: Advanced Customization ===\n");

            // Embeddings → using local LM Studio / OpenAI endpoint
            var embeddingClient = new OpenAIEmbeddingClient(
                baseUrl: "http://127.0.0.1:1234/v1",
                apiKey: "lmstudio",
                defaultModel: "text-embedding-3-small"
            );

            // Tokenizer → explicit GPT-3.5 tokenizer
            var tokenizer = new SharpTokenTokenizer("gpt-3.5-turbo");

            // Splitter → custom chunk size/overlap
            var splitter = new RecursiveTextSplitter(tokenizer, chunkSize: 300, chunkOverlap: 50);

            // Store → persistent file-backed store
            var dataPath = Path.Combine(AppContext.BaseDirectory, "data");
            Directory.CreateDirectory(dataPath);

            var storePath = Path.Combine(dataPath, "vector_store");
            Directory.CreateDirectory(storePath);
            IVectorStore store = new FileVectorStore(storePath, "vectors.json");

            // Logger → use console logger (or inject custom)
            ILogger logger = new ConsoleLogger(category: "Example5_Advanced", minLevel: LogLevel.Trace);

            // Build retriever with all injectables
            var retriever = new RagRetriever(
                embeddings: embeddingClient,
                store: store,
                splitter: splitter,
                logger: logger
            );

            // Load multiple sources
            Console.WriteLine("Loading files...");
            var docsPath = Path.Combine(AppContext.BaseDirectory, "data", "documents");
            var docs = await new DirectoryLoader(searchPattern: "*.txt").LoadAsync(docsPath);

            Console.WriteLine("Loading single file...");
            var single = await new FileLoader().LoadAsync(
                Path.Combine(AppContext.BaseDirectory, "data", "sample.txt"));

            Console.WriteLine("Loading web...");
            var webDocs = await new UrlLoader().LoadAsync("https://en.wikipedia.org/wiki/Quantum_entanglement");

            // Add all to retriever
            await retriever.AddDocumentsAsync(docs);
            await retriever.AddDocumentsAsync(single);
            await retriever.AddDocumentsAsync(webDocs);

            // Query
            var results = await retriever.Search(query, topK: 5);

            Console.WriteLine($"\nTop {results.Count} results for: \"{query}\"");
            foreach (var r in results)
            {
                Console.WriteLine(
                    $"Score: {r.Score:F2} | Source: {r.Source}\n" +
                    $"Text: {r.Content}\n"
                );
            }
        }
    }
}
