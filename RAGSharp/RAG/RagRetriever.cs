using Microsoft.Extensions.Logging;
using RAGSharp.Embeddings;
using RAGSharp.Embeddings.Tokenizers;
using RAGSharp.IO;
using RAGSharp.Logging;
using RAGSharp.Stores;
using RAGSharp.Text;
using RAGSharp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RAGSharp.RAG
{
    /// <summary>
    /// Default implementation of <see cref="IRagRetriever"/>.
    /// Handles document ingestion (chunking + embedding) and semantic search.
    /// </summary>
    public sealed class RagRetriever : IRagRetriever
    {
        private readonly IEmbeddingClient _embeddings;
        private readonly IVectorStore _store;
        private readonly ILogger _logger;
        private readonly ITextSplitter _splitter;

        /// <summary>
        /// Creates a new retriever instance.
        /// </summary>
        /// <param name="embeddings">Embedding client (e.g. OpenAI, local model).</param>
        /// <param name="store">Vector store implementation (in-memory, file-backed, etc.).</param>
        /// <param name="splitter">Text splitter for breaking documents into chunks. If null, defaults to <see cref="RecursiveTextSplitter"/>.</param>
        /// <param name="logger">Optional logger. If null, defaults to <see cref="ConsoleLogger"/>.</param>
        public RagRetriever(
            IEmbeddingClient embeddings,
            IVectorStore store,
            ITextSplitter splitter = null,
            ILogger logger = null)
        {
            _embeddings = embeddings ?? throw new ArgumentNullException(nameof(embeddings));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _splitter = splitter ?? new RecursiveTextSplitter(new SharpTokenTokenizer("gpt-3.5-turbo"));
            _logger = logger ?? new ConsoleLogger();
        }


        /// <inheritdoc/>
        public Task AddDocumentAsync(Document doc) =>
            AddDocumentsAsync(new[] { doc });

        /// <inheritdoc/>
        public async Task AddDocumentsAsync(
            IEnumerable<Document> docs,
            int batchSize = 32,
            int maxParallel = 2)
        {
            // chunk all documents into vector records with empty embeddings
            var allChunks = docs
             .SelectMany(d =>
             {
                 return _splitter.Split(d.Content)
                     .Select(chunk => new VectorRecord(
                         id: HashingHelper.ComputeId(chunk),
                         content: chunk,
                         embedding: Array.Empty<float>(),
                         source: d.Source,
                         metadata: d.Metadata
                     ));
             })
             .GroupBy(x => x.Id)
             .Select(g => g.First())
             .ToList();


            if (allChunks.Count == 0)
            {
                _logger?.LogInformation("[RAG] No chunks produced from input documents.");
                return;
            }

            _logger?.LogInformation($"[RAG] {allChunks.Count} total unique chunks produced.");

            // filter out already-present chunks
            var newChunks = allChunks.Where(c => !_store.Contains(c.Id)).ToList();
            if (newChunks.Count == 0)
            {
                _logger?.LogInformation("[RAG] All chunks already exist in the store. Nothing new to add.");
                return;
            }

            _logger?.LogInformation($"[RAG] {newChunks.Count} new chunks to embed and add to store.");

            // batch embedding work
            var batches = newChunks
                .Select((c, i) => new { c, i })
                .GroupBy(x => x.i / batchSize)
                .Select(g => g.Select(x => x.c).ToList())
                .ToList();

            _logger?.LogInformation($"[RAG] Processing {batches.Count} batches (batchSize={batchSize}, concurrency={maxParallel}).");

            var semaphore = new SemaphoreSlim(maxParallel);
            var tasks = batches.Select(async (batch, batchIndex) =>
            {
                await semaphore.WaitAsync();
                try
                {
                    _logger?.LogDebug($"[RAG] Embedding batch {batchIndex + 1}/{batches.Count} with {batch.Count} chunks...");
                    var texts = batch.Select(x => x.Content).ToList();

                    var vectors = (await _embeddings.GetEmbeddingsAsync(texts)).ToArray();
                    var items = batch
                        .Select((x, j) => new VectorRecord(
                            x.Id,
                            x.Content,
                            vectors[j].Normalize(),
                            x.Source,
                            x.Metadata))
                        .ToList();

                    _logger?.LogDebug($"[RAG] Completed embeddings for batch {batchIndex + 1}.");
                    return items;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);
            var itemsToAdd = results.SelectMany(r => r).ToList();

            await _store.AddBatchAsync(itemsToAdd);
            _logger?.LogInformation($"[RAG] Ingestion complete. {itemsToAdd.Count} chunks added to persistent store.");
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<SearchResult>> Search(string query, int topK = 3)
        {
            var qVec = (await _embeddings.GetEmbeddingAsync(query)).Normalize();
            return await _store.SearchAsync(qVec, topK);
        }
    }
}
