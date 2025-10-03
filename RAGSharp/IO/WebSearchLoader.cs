using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace RAGSharp.IO
{
    public sealed class WikipediaSearchLoader : IDocumentLoader
    {
        private readonly int _maxResults;
        private static readonly string ApiUrl = "https://en.wikipedia.org/w/api.php";

        public WikipediaSearchLoader(int maxResults = 3)
        {
            _maxResults = maxResults;
        }

        public async Task<IReadOnlyList<Document>> LoadAsync(string query)
        {
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; RagSharp/1.0)");

                var url = $"{ApiUrl}?action=query&list=search&srsearch={Uri.EscapeDataString(query)}&utf8=&format=json&srlimit={_maxResults}";
                var resp = await http.GetAsync(url);
                resp.EnsureSuccessStatusCode();

                var json = await resp.Content.ReadAsStringAsync();
                using (var doc = JsonDocument.Parse(json))
                {
                    var results = new List<Document>();

                    foreach (var item in doc.RootElement
                                             .GetProperty("query")
                                             .GetProperty("search")
                                             .EnumerateArray())
                    {
                        var title = item.GetProperty("title").GetString() ?? "";
                        var pageUrl = $"https://en.wikipedia.org/wiki/{Uri.EscapeDataString(title.Replace(' ', '_'))}";

                        // fetch page extract
                        var extractUrl = $"{ApiUrl}?action=query&prop=extracts&explaintext=true&titles={Uri.EscapeDataString(title)}&format=json";
                        var extractResp = await http.GetStringAsync(extractUrl);

                        using (var extractDoc = JsonDocument.Parse(extractResp))
                        {
                            foreach (var page in extractDoc.RootElement
                                                           .GetProperty("query")
                                                           .GetProperty("pages")
                                                           .EnumerateObject())
                            {
                                if (page.Value.TryGetProperty("extract", out var extract))
                                {
                                    var text = extract.GetString() ?? "";
                                    if (!string.IsNullOrWhiteSpace(text))
                                    {
                                        var metadata = new Dictionary<string, string>
                                    {
                                        { "Title", title },
                                        { "Url", pageUrl }
                                    };
                                        results.Add(new Document(text, pageUrl, metadata));
                                    }
                                }
                            }
                        }
                    }

                    return results;
                }
            }
        }
    }

}
