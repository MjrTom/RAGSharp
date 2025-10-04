using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RAGSharp.Text
{
    /// <summary>
    /// Utilities for cleaning and normalizing text content.
    /// </summary>
    internal static class TextCleaner
    {
        /// <summary>
        /// Normalize whitespace in text: collapse spaces, trim lines, normalize line endings.
        /// </summary>
        public static string NormalizeWhitespace(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Normalize line endings to \n
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");

            // Collapse multiple spaces/tabs (but preserve newlines)
            text = Regex.Replace(text, @"[ \t]+", " ");

            // Trim whitespace from each line
            text = Regex.Replace(text, @"^[ \t]+", "", RegexOptions.Multiline);
            text = Regex.Replace(text, @"[ \t]+$", "", RegexOptions.Multiline);

            // Collapse more than 2 consecutive newlines (preserve paragraph breaks)
            text = Regex.Replace(text, @"\n{3,}", "\n\n");

            return text.Trim();
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
