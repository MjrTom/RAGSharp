using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;


namespace RAGSharp.IO
{
    /// <summary>
    /// Load all files from a directory.
    /// </summary>
    public sealed class DirectoryLoader : IDocumentLoader
    {
        private readonly IDocumentLoader _fileLoader;
        private readonly string _searchPattern;
        private readonly bool _recursive;
        private readonly int _maxConcurrency;

        public DirectoryLoader(
            IDocumentLoader fileLoader = null,
            string searchPattern = "*.*",
            bool recursive = true,
            int maxConcurrency = 4)
        {
            _fileLoader = fileLoader ?? new FileLoader();
            _searchPattern = searchPattern;
            _recursive = recursive;
            _maxConcurrency = maxConcurrency;
        }

        public async Task<IReadOnlyList<Document>> LoadAsync(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return Array.Empty<Document>();

            var option = _recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.EnumerateFiles(directoryPath, _searchPattern, option).ToList();

            var loader = new FileLoader();
            var results = new List<Document>();
            var semaphore = new SemaphoreSlim(_maxConcurrency);

            var tasks = files.Select(async file =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await loader.LoadAsync(file);
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
