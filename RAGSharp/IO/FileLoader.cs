using RAGSharp.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RAGSharp.IO
{
    /// <summary>
    /// Loads a single text file into a <see cref="Document"/>.
    /// Supports automatic encoding detection and text normalization.
    /// </summary>
    public sealed class FileLoader : IDocumentLoader
    {
        private readonly long _maxFileSizeBytes;
        private readonly bool _normalizeWhitespace;

        public FileLoader(long maxFileSizeBytes = 10_000_000, bool normalizeWhitespace = true)
        {
            if (maxFileSizeBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxFileSizeBytes), "Max file size must be positive.");

            _maxFileSizeBytes = maxFileSizeBytes;
            _normalizeWhitespace = normalizeWhitespace;
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
                var text = await Task.Run(() => ReadFileWithEncodingDetection(path));

                if (string.IsNullOrWhiteSpace(text))
                    return Array.Empty<Document>();

                if (_normalizeWhitespace)
                    text = TextCleaner.NormalizeWhitespace(text);

                var metadata = new Dictionary<string, string>
                {
                    { "FileName", fileInfo.Name },
                    { "SizeBytes", fileInfo.Length.ToString() },
                    { "Extension", fileInfo.Extension },
                    { "LastModified", fileInfo.LastWriteTimeUtc.ToString("O") }
                };

                return new[] { new Document(text, fileInfo.FullName, metadata) };
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException || ex is FileNotFoundException)
                    throw;

                throw new IOException($"Failed to read file '{path}': {ex.Message}", ex);
            }
        }

        private static string ReadFileWithEncodingDetection(string path)
        {
            try
            {
                return File.ReadAllText(path, Encoding.UTF8);
            }
            catch (DecoderFallbackException)
            {
                try
                {
                    return File.ReadAllText(path, Encoding.Default);
                }
                catch
                {
                    return File.ReadAllText(path, Encoding.GetEncoding("ISO-8859-1"));
                }
            }
        }
    }
}