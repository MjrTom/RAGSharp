using System;
using System.Collections.Generic;

namespace RAGSharp.Stores
{
    /// <summary>
    /// Represents a single retrievable unit in the RAG system:
    /// the chunk text, its embedding, and optional metadata.
    /// </summary>
    public sealed class VectorRecord
    {
        /// <summary>
        /// Unique identifier for this chunk.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Raw text content of the chunk.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Optional metadata (e.g. file name, source URL).
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; }

        /// <summary>
        /// Vector embedding of the chunk content.
        /// </summary>
        public float[] Embedding { get; }

        /// <summary>
        /// Create a new vector record.
        /// </summary>
        public VectorRecord(string id, string content, float[] embedding, IReadOnlyDictionary<string, string> metadata = null)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Embedding = embedding ?? throw new ArgumentNullException(nameof(embedding));
            Metadata = metadata ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Clone with new id or embedding.
        /// </summary>
        public VectorRecord With(string id = null, float[] embedding = null)
        {
            return new VectorRecord(
                id ?? Id,
                Content,
                embedding ?? Embedding,
                Metadata
            );
        }
    }
}

