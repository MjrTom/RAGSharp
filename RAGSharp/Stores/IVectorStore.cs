using System.Collections.Generic;
using System.Threading.Tasks;

namespace RAGSharp.Stores
{
    /// <summary>
    /// Contract for a vector store backend.
    /// Stores vector embeddings and supports similarity search.
    /// </summary>
    public interface IVectorStore
    {
        /// <summary>
        /// Add a single record to the store.
        /// </summary>
        Task AddAsync(VectorRecord item);

        /// <summary>
        /// Add multiple records to the store in a batch.
        /// </summary>
        Task AddBatchAsync(IEnumerable<VectorRecord> items);

        /// <summary>
        /// Search for the top-k most similar records for a query vector.
        /// </summary>
        /// <param name="queryVector">Normalized embedding of the query text.</param>
        /// <param name="topK">How many results to return (default: 3).</param>
        Task<IReadOnlyList<SearchResult>> SearchAsync(float[] queryVector, int topK = 3);

        /// <summary>
        /// Check if a record with the given id exists in the store.
        /// </summary>
        bool Contains(string id);
    }
}
