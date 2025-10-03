using RAGSharp.IO;
using RAGSharp.Stores;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RAGSharp.RAG
{

    /// <summary>
    /// High-level interface for retrieving relevant text chunks
    /// using Retrieval-Augmented Generation (RAG).
    /// </summary>
    public interface IRagRetriever
    {
        /// <summary>
        /// Adds a single document to the retriever.
        /// The document is split, chunked, embedded, and stored.
        /// </summary>
        Task AddDocumentAsync(Document doc);

        /// <summary>
        /// Adds multiple documents to the retriever.
        /// Documents are split, chunked, embedded, and stored in batches.
        /// </summary>
        /// <param name="docs">The documents to ingest.</param>
        /// <param name="batchSize">How many chunks to embed per batch (default: 32).</param>
        /// <param name="maxParallel">How many batches to embed concurrently (default: 2).</param>
        Task AddDocumentsAsync(IEnumerable<Document> docs, int batchSize = 32, int maxParallel = 2);

        /// <summary>
        /// Searches for the top-k most relevant chunks for a given query string.
        /// </summary>
        /// <param name="query">The search query.</param>
        /// <param name="topK">How many results to return (default: 3).</param>
        Task<IReadOnlyList<SearchResult>> Search(string query, int topK = 3);
    }
}
