using System.Collections.Generic;

namespace RAGSharp.Text
{
    /// <summary>
    /// Splits text into chunks for embedding.
    /// </summary>
    public interface ITextSplitter
    {
        IEnumerable<string> Split(string text);
    }
}

