using System.Collections.Generic;

namespace RAGSharp.Stores
{
    /// <summary>
    /// Result of a semantic search query:
    /// includes similarity score and the matching chunk payload.
    /// </summary>
    public sealed class SearchResult
    {
        /// <summary>
        /// The ID of the matching record.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Cosine similarity score between the query and this record.
        /// Higher is more relevant.
        /// </summary>
        public double Score { get; }

        /// <summary>
        /// Raw text content of the matched chunk.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Source identifier (e.g. file path, URL).
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Optional metadata associated with the chunk.
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; }

        public SearchResult(string id, double score, string content, string source, IReadOnlyDictionary<string, string> metadata = null)
        {
            Id = id;
            Score = score;
            Content = content;
            Source = source ?? string.Empty;
            Metadata = metadata ?? new Dictionary<string, string>();
        }
    }
}
