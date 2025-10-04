using RAGSharp.Text;
using RAGSharp.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RAGSharp.IO
{
    /// <summary>
    /// Searches Wikipedia and loads article content as documents.
    /// Uses Wikipedia API to search and extract plain text content.
    /// </summary>
    public sealed class WebSearchLoader : IDocumentLoader
    {
        private readonly int _maxResults;
        private readonly bool _normalizeWhitespace;
        private static readonly string ApiUrl = "https://en.wikipedia.org/w/api.php";
        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        static WebSearchLoader()
        {
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; RagSharp/1.0)");
        }

        /// <param name="maxResults">Maximum number of search results to fetch (default: 3).</param>
        /// <param name="normalizeWhitespace">Whether to normalize whitespace in extracted text (default: true).</param>
        public WebSearchLoader(int maxResults = 3, bool normalizeWhitespace = true)
        {
            if (maxResults <= 0 || maxResults > 50)
                throw new ArgumentOutOfRangeException(nameof(maxResults), "Max results must be between 1 and 50.");

            _maxResults = maxResults;
            _normalizeWhitespace = normalizeWhitespace;
        }

        /// <summary>
        /// Search Wikipedia and load article content.
        /// </summary>
        /// <param name="query">Search query (e.g., "quantum mechanics").</param>
        /// <returns>List of documents containing article text.</returns>
        public async Task<IReadOnlyList<Document>> LoadAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            try
            {
                var searchResults = await SearchWikipediaAsync(query);
                var documents = new List<Document>();

                foreach (var (title, pageId) in searchResults)
                {
                    try
                    {
                        var text = await FetchArticleExtractAsync(title, pageId);

                        if (string.IsNullOrWhiteSpace(text))
                            continue;

                        if (_normalizeWhitespace)
                            text = TextCleaner.NormalizeWhitespace(text);

                        var pageUrl = $"https://en.wikipedia.org/wiki/{Uri.EscapeDataString(title.Replace(' ', '_'))}";

                        var metadata = new Dictionary<string, string>
                        {
                            { "Title", title },
                            { "Url", pageUrl },
                            { "PageId", pageId },
                            { "Source", "Wikipedia" }
                        };

                        documents.Add(new Document(text, pageUrl, metadata));
                    }
                    catch (Exception ex)
                    {
                        // Skip individual article failures, continue with others
                        Console.WriteLine($"[WikipediaSearchLoader] Failed to load article '{title}': {ex.Message}");
                    }
                }

                return documents;
            }
            catch (HttpRequestException ex)
            {
                throw new IOException($"Failed to search Wikipedia: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new IOException($"Failed to parse Wikipedia API response: {ex.Message}", ex);
            }
        }

        private async Task<List<(string Title, string PageId)>> SearchWikipediaAsync(string query)
        {
            var url = $"{ApiUrl}?action=query&list=search&srsearch={Uri.EscapeDataString(query)}&utf8=&format=json&srlimit={_maxResults}";

            var response = await _http.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using (var doc = JsonDocument.Parse(json))
            {
                var results = new List<(string, string)>();

                if (!doc.RootElement.TryGetProperty("query", out var queryProp) ||
                    !queryProp.TryGetProperty("search", out var searchProp))
                {
                    return results;
                }

                foreach (var item in searchProp.EnumerateArray())
                {
                    if (item.TryGetProperty("title", out var titleProp) &&
                        item.TryGetProperty("pageid", out var pageIdProp))
                    {
                        var title = titleProp.GetString();
                        var pageId = pageIdProp.GetInt32().ToString();

                        if (!string.IsNullOrEmpty(title))
                            results.Add((title, pageId));
                    }
                }

                return results;
            }
        }

        private async Task<string> FetchArticleExtractAsync(string title, string pageId)
        {
            // Use pageids instead of titles for more reliable lookup
            var url = $"{ApiUrl}?action=query&prop=extracts&explaintext=true&pageids={pageId}&format=json&redirects=1";

            var response = await _http.GetStringAsync(url);

            using (var doc = JsonDocument.Parse(response))
            {
                if (!doc.RootElement.TryGetProperty("query", out var queryProp) ||
                    !queryProp.TryGetProperty("pages", out var pagesProp))
                {
                    return string.Empty;
                }

                // Get the first (and only) page
                foreach (var page in pagesProp.EnumerateObject())
                {
                    if (page.Value.TryGetProperty("extract", out var extractProp))
                    {
                        return extractProp.GetString() ?? string.Empty;
                    }
                }

                return string.Empty;
            }
        }
    }
}