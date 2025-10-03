using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RAGSharp.IO
{
    /// <summary>
    /// Load plain text content from a web page.
    /// </summary>
    public sealed class UrlLoader : IDocumentLoader
    {
        private readonly int _maxContentLength;
        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        public UrlLoader(int maxContentLength = 5_000_000)
        {
            _maxContentLength = maxContentLength;
        }

        public async Task<IReadOnlyList<Document>> LoadAsync(string url)
        {
            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (compatible; RagSharp/1.0)");

                    var resp = await _http.SendAsync(request);
                    if (!resp.IsSuccessStatusCode)
                        return Array.Empty<Document>();

                    if (resp.Content.Headers.ContentLength.HasValue &&
                        resp.Content.Headers.ContentLength.Value > _maxContentLength)
                        return Array.Empty<Document>();

                    var html = await resp.Content.ReadAsStringAsync();
                    var text = ExtractPlainTextFromHtml(html);

                    return string.IsNullOrWhiteSpace(text)
                        ? Array.Empty<Document>()
                        : new[] { new Document(text, url) };
                }
            }
            catch (Exception ex)
            {
                if (ex is OutOfMemoryException) throw;
                return Array.Empty<Document>();
            }
        }

        private string ExtractPlainTextFromHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var remove = doc.DocumentNode.SelectNodes("//script|//style|//nav|//footer|//form|//noscript|//aside")
                         ?? Enumerable.Empty<HtmlNode>();
            foreach (var node in remove.ToList())
                node.Remove();

            var sb = new StringBuilder();

            // Optional: extract infobox tables (common on Wikipedia)
            var infobox = doc.DocumentNode.SelectSingleNode("//table[contains(@class,'infobox')]");
            if (infobox != null)
            {
                foreach (var row in infobox.SelectNodes(".//tr") ?? Enumerable.Empty<HtmlNode>())
                {
                    var header = row.SelectSingleNode("./th")?.InnerText?.Trim();
                    var value = row.SelectSingleNode("./td")?.InnerText?.Trim();
                    if (!string.IsNullOrEmpty(header) && !string.IsNullOrEmpty(value))
                        sb.AppendLine(string.Format("{0}: {1}", CleanText(header), CleanText(value)));
                }
            }

            var mainNodes = doc.DocumentNode.SelectNodes("//p|//h1|//h2|//h3|//li") ?? Enumerable.Empty<HtmlNode>();
            foreach (var node in mainNodes)
            {
                var text = CleanText(node.InnerText);
                if (!string.IsNullOrWhiteSpace(text))
                    sb.AppendLine(text);
            }

            return sb.ToString().Trim();
        }

        private static string CleanText(string text)
        {
            return text.Replace("\n", " ").Replace("\r", " ").Replace("\t", " ").Trim();
        }
    }
}
