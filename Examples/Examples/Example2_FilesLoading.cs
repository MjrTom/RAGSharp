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
    public static class Example2_FilesLoading
    {
        static FileVectorStore persistentStore;
        static Example2_FilesLoading()
        {
            var dataPath = Path.Combine(AppContext.BaseDirectory, "data");
            Directory.CreateDirectory(dataPath);

            persistentStore = new FileVectorStore(
                    persistDir: Path.Combine(dataPath, "vector_store"),
                    fileName: "vectors.json");
        }

        public static async Task Run(string query)
        {
            Console.WriteLine("=== Example 2: DirectoryLoader - Multiple Files ===\n");

            var embeddingClient = new OpenAIEmbeddingClient(
               baseUrl: "http://127.0.0.1:1234/v1",
               apiKey: "lmstudio",
               defaultModel: "text-embedding-3-small"
           );

            var retriever = new RagRetriever(embeddingClient, persistentStore);


            // Example A: Single file
            Console.WriteLine("Loading single file...");
            var singleFile = await new FileLoader()
                .LoadAsync(Path.Combine(AppContext.BaseDirectory, "data", "sample.txt"));
            await retriever.AddDocumentsAsync(singleFile);


            // Example B: All files in directory
            Console.WriteLine("Loading directory (*.txt files)...");
            var docsPath = Path.Combine(AppContext.BaseDirectory, "data", "documents");
            if (Directory.Exists(docsPath))
            {
                var dirDocs = await new DirectoryLoader(searchPattern: "*.txt").LoadAsync(docsPath);
                Console.WriteLine($"Found {dirDocs.Count} files in documents/");
                await retriever.AddDocumentsAsync(dirDocs);
            }
            else
            {
                Console.WriteLine("No documents/ folder found — skipping directory load.");
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