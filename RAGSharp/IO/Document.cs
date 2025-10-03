using System;
using System.Collections.Generic;

namespace RAGSharp.IO
{
    /// <summary>
    /// A raw input document before chunking/embedding.
    /// </summary>
    public sealed class Document
    {
        /// <summary>
        /// Full text content of the document.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Source identifier (e.g. file path, URL).
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Optional metadata (e.g. file size, content type).
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; }

        /// <summary>
        /// Create a new document with text, source, and optional metadata.
        /// </summary>
        public Document(string content, string source, IReadOnlyDictionary<string, string> metadata = null)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            Content = content;
            Source = source ?? string.Empty;
            Metadata = metadata ?? new Dictionary<string, string>();
        }
    }
}
