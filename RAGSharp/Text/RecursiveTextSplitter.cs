using RAGSharp.Embeddings;
using RAGSharp.Embeddings.Tokenizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RAGSharp.Text
{
    /// <summary>
    /// Recursive text splitter inspired by LangChain.
    /// Splits into paragraphs → sentences → token windows, with overlap.
    /// Avoids junk chunks (very short fragments).
    /// </summary>
    public sealed class RecursiveTextSplitter : ITextSplitter
    {
        private readonly int _chunkSize;
        private readonly int _chunkOverlap;
        private readonly ITokenizer _tokenizer;
        private const int MinChars = 20; // skip fragments smaller than this

        public RecursiveTextSplitter(ITokenizer tokenizer, int chunkSize = 400, int chunkOverlap = 100)
        {
            if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));
            if (chunkOverlap < 0 || chunkOverlap >= chunkSize) throw new ArgumentOutOfRangeException(nameof(chunkOverlap));
            _tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
            _chunkSize = chunkSize;
            _chunkOverlap = chunkOverlap;
        }

        /// <summary>
        /// Split text into chunks ~chunkSize tokens long with overlap.
        /// </summary>
        public IEnumerable<string> Split(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                yield break;

            // Normalize newlines
            var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");

            // First split: paragraphs
            foreach (var para in Regex.Split(normalized, @"\n{2,}").Where(p => !string.IsNullOrWhiteSpace(p)))
            {
                var tokens = _tokenizer.Encode(para);

                if (tokens.Count <= _chunkSize)
                {
                    if (para.Length >= MinChars)
                        yield return para.Trim();
                }
                else
                {
                    // Split by sentences if paragraph too large
                    foreach (var sentenceChunk in SplitBySentences(para))
                        foreach (var c in TokenWindow(sentenceChunk))
                            yield return c;
                }
            }
        }

        private IEnumerable<string> SplitBySentences(string text)
        {
            // Naive regex split by sentence end punctuation
            var sentences = Regex.Split(text, @"(?<=[.!?])\s+")
                                 .Where(s => !string.IsNullOrWhiteSpace(s));

            var buffer = "";
            foreach (var s in sentences)
            {
                var candidate = (buffer + " " + s).Trim();
                var tokens = _tokenizer.Encode(candidate);

                if (tokens.Count > _chunkSize)
                {
                    if (!string.IsNullOrWhiteSpace(buffer))
                        yield return buffer;

                    buffer = s; // start new
                }
                else
                {
                    buffer = candidate;
                }
            }

            if (!string.IsNullOrWhiteSpace(buffer))
                yield return buffer;
        }

        private IEnumerable<string> TokenWindow(string text)
        {
            var tokens = _tokenizer.Encode(text);

            if (tokens.Count <= _chunkSize)
            {
                if (text.Length >= MinChars)
                    yield return text.Trim();
                yield break;
            }

            for (int start = 0; start < tokens.Count; start += _chunkSize - _chunkOverlap)
            {
                var end = Math.Min(start + _chunkSize, tokens.Count);
                var chunkTokens = tokens.Skip(start).Take(end - start).ToList();
                var chunk = _tokenizer.Decode(chunkTokens);

                if (chunk.Length >= MinChars)
                    yield return chunk.Trim();
            }
        }
    }
}
