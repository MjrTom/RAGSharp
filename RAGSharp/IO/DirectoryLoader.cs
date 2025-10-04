using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RAGSharp.IO
{
    /// <summary>
    /// Load all files from a directory, optionally recursively.
    /// Supports parallel loading with configurable concurrency.
    /// </summary>
    public sealed class DirectoryLoader : IDocumentLoader
    {
        private readonly IDocumentLoader _fileLoader;
        private readonly string _searchPattern;
        private readonly bool _recursive;
        private readonly int _maxConcurrency;

        /// <param name="fileLoader">Loader to use for individual files (default: new FileLoader()).</param>
        /// <param name="searchPattern">File pattern to match (default: "*.*" for all files).</param>
        /// <param name="recursive">Whether to search subdirectories (default: true).</param>
        /// <param name="maxConcurrency">Maximum number of files to load in parallel (default: 4).</param>
        public DirectoryLoader(
            IDocumentLoader fileLoader = null,
            string searchPattern = "*.*",
            bool recursive = true,
            int maxConcurrency = 4)
        {
            if (maxConcurrency <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxConcurrency), "Max concurrency must be positive.");

            _fileLoader = fileLoader ?? new FileLoader();
            _searchPattern = searchPattern ?? "*.*";
            _recursive = recursive;
            _maxConcurrency = maxConcurrency;
        }

        /// <summary>
        /// Load all matching files from the directory.
        /// </summary>
        /// <param name="directoryPath">Path to the directory to scan.</param>
        /// <returns>All documents loaded from matching files.</returns>
        public async Task<IReadOnlyList<Document>> LoadAsync(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentException("Directory path cannot be null or empty.", nameof(directoryPath));

            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            var option = _recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            List<string> files;
            try
            {
                files = Directory.EnumerateFiles(directoryPath, _searchPattern, option).ToList();
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new IOException($"Access denied to directory: {directoryPath}", ex);
            }

            if (files.Count == 0)
                return Array.Empty<Document>();

            var results = new List<Document>();
            var semaphore = new SemaphoreSlim(_maxConcurrency);

            var tasks = files.Select(async file =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await _fileLoader.LoadAsync(file);
                }
                catch (Exception ex)
                {
                    // Log but don't fail entire directory load
                    Console.WriteLine($"[DirectoryLoader] Failed to load file '{file}': {ex.Message}");
                    return Array.Empty<Document>();
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var allResults = await Task.WhenAll(tasks);
            return allResults.SelectMany(r => r).ToList();
        }
    }
}