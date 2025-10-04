using HtmlAgilityPack;
using RAGSharp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RAGSharp.IO
{
    /// <summary>
    /// Load plain text content from a web page with HTML cleaning.
    /// </summary>
    public sealed class UrlLoader : IDocumentLoader
    {
        private readonly int _maxContentLength;
        private readonly bool _extractMainContent;

        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        public UrlLoader(int maxContentLength = 5_000_000, bool extractMainContent = true)
        {
            _maxContentLength = maxContentLength;
            _extractMainContent = extractMainContent;
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

                    var text = _extractMainContent
                        ? ExtractPlainTextFromHtml(html)
                        : html;

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

            // Remove unwanted elements
            var unwantedTags = new[]
            {
                "script", "style", "nav", "footer", "form", "noscript", "aside",
                "header", "iframe", "button", "svg", "path", "input", "select"
            };

            foreach (var tag in unwantedTags)
            {
                var nodes = doc.DocumentNode.SelectNodes($"//{tag}");
                if (nodes != null)
                {
                    foreach (var node in nodes.ToList())
                        node.Remove();
                }
            }

            // Remove common navigation/metadata classes and IDs
            var unwantedSelectors = new[]
            {
                "//*[contains(@class, 'navigation')]",
                "//*[contains(@class, 'menu')]",
                "//*[contains(@class, 'sidebar')]",
                "//*[contains(@class, 'comment')]",
                "//*[contains(@class, 'advertisement')]",
                "//*[contains(@class, 'ad-')]",
                "//*[contains(@id, 'comments')]",
                "//*[contains(@id, 'footer')]",
                "//*[contains(@id, 'header')]"
            };

            foreach (var selector in unwantedSelectors)
            {
                var nodes = doc.DocumentNode.SelectNodes(selector);
                if (nodes != null)
                {
                    foreach (var node in nodes.ToList())
                        node.Remove();
                }
            }

            var sb = new StringBuilder();

            // Extract infobox (Wikipedia-specific)
            var infobox = doc.DocumentNode.SelectSingleNode("//table[contains(@class,'infobox')]");
            if (infobox != null)
            {
                foreach (var row in infobox.SelectNodes(".//tr") ?? Enumerable.Empty<HtmlNode>())
                {
                    var header = row.SelectSingleNode("./th")?.InnerText?.Trim();
                    var value = row.SelectSingleNode("./td")?.InnerText?.Trim();
                    if (!string.IsNullOrEmpty(header) && !string.IsNullOrEmpty(value))
                    {
                        sb.AppendLine($"{CleanHtmlText(header)}: {CleanHtmlText(value)}");
                    }
                }
                sb.AppendLine();
            }

            // Extract main content elements
            var mainNodes = doc.DocumentNode.SelectNodes("//article//p|//article//h1|//article//h2|//article//h3|//article//li|//main//p|//main//h1|//main//h2|//main//h3|//main//li|//p|//h1|//h2|//h3|//li")
                            ?? Enumerable.Empty<HtmlNode>();

            var seenTexts = new HashSet<string>();

            foreach (var node in mainNodes)
            {
                var text = CleanHtmlText(node.InnerText);

                if (string.IsNullOrWhiteSpace(text) || text.Length < 20)
                    continue;

                if (seenTexts.Contains(text))
                    continue;

                seenTexts.Add(text);
                sb.AppendLine(text);
            }

            var result = sb.ToString();
            result = TextCleaner.NormalizeWhitespace(result);

            return result.Trim();
        }

        /// <summary>
        /// Clean text extracted from HTML: decode entities and normalize whitespace.
        /// </summary>
        public static string CleanHtmlText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Decode HTML entities (&nbsp; → space, &amp; → &, etc.)
            text = System.Net.WebUtility.HtmlDecode(text);

            // Normalize whitespace
            text = text.Replace("\n", " ")
                       .Replace("\r", " ")
                       .Replace("\t", " ");

            // Collapse multiple spaces
            text = Regex.Replace(text, @"\s+", " ");

            return text.Trim();
        }
    }
}