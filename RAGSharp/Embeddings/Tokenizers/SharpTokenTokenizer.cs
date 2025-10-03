using SharpToken;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RAGSharp.Embeddings.Tokenizers
{
    /// <summary>
    /// Tokenizer implementation based on the SharpToken library.
    /// Supports GPT model encodings (e.g. "gpt-3.5-turbo", "gpt-4").
    /// </summary>
    public sealed class SharpTokenTokenizer : ITokenizer
    {
        private readonly GptEncoding _encoder;

        /// <summary>
        /// Create a tokenizer for a specific model encoding.
        /// </summary>
        /// <param name="model">Model name (e.g. "gpt-3.5-turbo").</param>
        public SharpTokenTokenizer(string model)
        {
            _encoder = GptEncoding.GetEncodingForModel(model);
        }

        /// <summary>
        /// Create a tokenizer with a default encoding (gpt-3.5-turbo).
        /// </summary>
        public SharpTokenTokenizer() : this("gpt-3.5-turbo") { }

        /// <inheritdoc/>
        public IReadOnlyList<int> Encode(string text) =>
            _encoder.Encode(text);

        /// <inheritdoc/>
        public string Decode(IEnumerable<int> tokens) =>
            _encoder.Decode(tokens.ToList());
    }
}
