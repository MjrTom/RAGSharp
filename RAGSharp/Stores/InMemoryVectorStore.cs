using RAGSharp.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RAGSharp.Stores
{
    /// <summary>
    /// Simple in-memory vector store (non-persistent).
    /// Suitable for testing and demos.
    /// </summary>
    public sealed class InMemoryVectorStore : IVectorStore
    {
        private readonly ConcurrentDictionary<string, VectorRecord> _store = new ConcurrentDictionary<string, VectorRecord>();

        public Task AddAsync(VectorRecord item)
        {
            var id = string.IsNullOrWhiteSpace(item.Id)
                ? HashingHelper.ComputeId(item.Content)
                : item.Id;

            _store.TryAdd(id, item.With(id: id));
            return Task.CompletedTask;
        }

        public Task AddBatchAsync(IEnumerable<VectorRecord> items)
        {
            foreach (var item in items)
            {
                var id = string.IsNullOrWhiteSpace(item.Id)
                    ? HashingHelper.ComputeId(item.Content)
                    : item.Id;

                _store.TryAdd(id, item.With(id: id));
            }
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<SearchResult>> SearchAsync(float[] queryVector, int topK = 3)
        {
            var results = _store.Values
                .Select(e => new SearchResult(
                    e.Id,
                    queryVector.CosineSimilarity(e.Embedding),
                    e.Content,
                    e.Metadata))
                .OrderByDescending(r => r.Score)
                .Take(topK)
                .ToList();

            return Task.FromResult<IReadOnlyList<SearchResult>>(results);
        }

        public bool Contains(string id) =>
            !string.IsNullOrWhiteSpace(id) && _store.ContainsKey(id);

        public void Clear() => _store.Clear();
    }
}
