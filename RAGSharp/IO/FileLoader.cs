using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RAGSharp.IO
{
    /// <summary>
    /// Loads a single text file into a <see cref="Document"/>.
    /// </summary>
    public sealed class FileLoader : IDocumentLoader
    {
        private readonly long _maxFileSizeBytes;

        /// <param name="maxFileSizeBytes">Maximum allowed file size in bytes (default: 10 MB).</param>
        public FileLoader(long maxFileSizeBytes = 10_000_000)
        {
            _maxFileSizeBytes = maxFileSizeBytes;
        }

        public async Task<IReadOnlyList<Document>> LoadAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty.", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException("File not found.", path);

            var fileInfo = new FileInfo(path);
            if (fileInfo.Length > _maxFileSizeBytes)
                throw new IOException(
                    $"File '{path}' exceeds maximum allowed size ({_maxFileSizeBytes} bytes).");

            try
            {
                // .NET Standard 2.0: no ReadAllTextAsync, so wrap sync in Task.Run
                var text = await Task.Run(() => File.ReadAllText(path));

                var metadata = new Dictionary<string, string>
                {
                    { "FileName", fileInfo.Name },
                    { "SizeBytes", fileInfo.Length.ToString() },
                    { "Extension", fileInfo.Extension }
                };

                return new[] { new Document(text, fileInfo.FullName, metadata) };
            }
            catch (OutOfMemoryException) // Let this bubble up
            {
                throw;
            }
            catch (Exception ex)
            {
                // Wrap unexpected errors with context
                throw new IOException($"Failed to read file '{path}': {ex.Message}", ex);
            }
        }
    }
}
