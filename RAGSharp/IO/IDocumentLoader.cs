using System.Collections.Generic;
using System.Threading.Tasks;

namespace RAGSharp.IO
{
    /// <summary>
    /// Contract for document loaders (file, directory, URL, API, etc.).
    /// A loader produces one or more <see cref="Document"/> objects from an input.
    /// </summary>
    public interface IDocumentLoader
    {
        /// <summary>
        /// Load one or more documents from the given input.
        /// Input may be a path, URL, or query depending on the loader.
        /// </summary>
        Task<IReadOnlyList<Document>> LoadAsync(string input);
    }
}
