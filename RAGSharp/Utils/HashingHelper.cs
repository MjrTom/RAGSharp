using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace RAGSharp.Utils
{
    /// <summary>
    /// Provides utilities for generating stable, content-based identifiers.
    /// </summary>
    public static class HashingHelper
    {
        /// <summary>
        /// Compute a stable SHA256-based ID for a given text.
        /// Text is normalized (Unicode NFC, lowercased, whitespace collapsed) before hashing.
        /// </summary>
        /// <param name="text">The input text to hash.</param>
        /// <returns>Hex-encoded SHA256 hash as a lowercase string.</returns>
        /// <exception cref="ArgumentNullException">Thrown if text is null.</exception>
        public static string ComputeId(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            // Unicode normalize to composed form
            var normalized = text.Normalize(NormalizationForm.FormC);

            // ignore differences in uppercase/lowercase when hashing
            normalized = normalized.ToLowerInvariant();

            // any run of whitespace characters (spaces, tabs, newlines) replaced it with a single space " "
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(normalized);
            var hash = sha.ComputeHash(bytes);

            // Lowercase hex string
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
