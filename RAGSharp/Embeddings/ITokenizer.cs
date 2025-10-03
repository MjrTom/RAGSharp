using SharpToken;
using System.Collections.Generic;
using System.Linq;

namespace RAGSharp.Embeddings
{
    /// <summary>
    /// Tokenizer interface for converting between text and token IDs.
    /// </summary>
    public interface ITokenizer
    {
        /// <summary>
        /// Encode a text string into a sequence of token IDs.
        /// </summary>
        IReadOnlyList<int> Encode(string text);

        /// <summary>
        /// Decode a sequence of token IDs back into text.
        /// </summary>
        string Decode(IEnumerable<int> tokens);
    }
}
