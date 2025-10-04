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
    /// Respects semantic boundaries while ensuring token-accurate chunks.
    /// </summary>
    public sealed class RecursiveTextSplitter : ITextSplitter
    {
        private readonly int _chunkSize;
        private readonly int _chunkOverlap;
        private readonly ITokenizer _tokenizer;
        private const int MinChars = 20; // skip fragments smaller than this

        private static readonly Regex ParagraphSplitter = new Regex(@"\n{2,}", RegexOptions.Compiled);
        private static readonly Regex SentenceSplitter = new Regex(@"(?<=[.!?])\s+", RegexOptions.Compiled);

        /// <summary>
        /// Creates a new recursive text splitter.
        /// </summary>
        /// <param name="tokenizer">Required tokenizer for accurate token counting</param>
        /// <param name="chunkSize">Target chunk size in tokens (default: 400)</param>
        /// <param name="chunkOverlap">Overlap between chunks in tokens (default: 100)</param>
        public RecursiveTextSplitter(ITokenizer tokenizer, int chunkSize = 400, int chunkOverlap = 100)
        {
            if (chunkSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(chunkSize), "Chunk size must be positive");
            if (chunkOverlap < 0 || chunkOverlap >= chunkSize)
                throw new ArgumentOutOfRangeException(nameof(chunkOverlap), "Overlap must be non-negative and less than chunk size");

            _tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
            _chunkSize = chunkSize;
            _chunkOverlap = chunkOverlap;
        }

        /// <summary>
        /// Split text into chunks ~chunkSize tokens long with overlap.
        /// Tries to preserve semantic boundaries (paragraphs, sentences) where possible.
        /// </summary>
        public IEnumerable<string> Split(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                yield break;

            // Normalize newlines
            var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");

            // First split: paragraphs (separated by double newlines)
            var paragraphs = ParagraphSplitter.Split(normalized)
                                  .Where(p => !string.IsNullOrWhiteSpace(p));

            foreach (var para in paragraphs)
            {
                var tokens = _tokenizer.Encode(para);

                // If paragraph fits in chunk size, keep it whole
                if (tokens.Count <= _chunkSize)
                {
                    if (para.Trim().Length >= MinChars)
                        yield return para.Trim();
                }
                else
                {
                    // Paragraph too large, split by sentences
                    foreach (var chunk in SplitBySentences(para))
                        yield return chunk;
                }
            }
        }

        private IEnumerable<string> SplitBySentences(string text)
        {
            // Split by sentence-ending punctuation followed by whitespace
            var sentences = SentenceSplitter.Split(text)
                                 .Where(s => !string.IsNullOrWhiteSpace(s));

            var buffer = "";
            foreach (var sentence in sentences)
            {
                var candidate = string.IsNullOrWhiteSpace(buffer)
                    ? sentence
                    : $"{buffer} {sentence}";

                var candidateTokens = _tokenizer.Encode(candidate);

                if (candidateTokens.Count > _chunkSize)
                {
                    // Candidate exceeds limit, yield buffer first
                    if (!string.IsNullOrWhiteSpace(buffer) && buffer.Length >= MinChars)
                        yield return buffer.Trim();

                    // Check if single sentence exceeds chunk size
                    var singleTokens = _tokenizer.Encode(sentence);
                    if (singleTokens.Count > _chunkSize)
                    {
                        // Single sentence too long, needs token window splitting
                        foreach (var chunk in TokenWindow(sentence))
                            yield return chunk;
                        buffer = "";
                    }
                    else
                    {
                        // Sentence fits, start new buffer with it
                        buffer = sentence;
                    }
                }
                else
                {
                    // Candidate fits, update buffer
                    buffer = candidate;
                }
            }

            // Yield remaining buffer
            if (!string.IsNullOrWhiteSpace(buffer) && buffer.Trim().Length >= MinChars)
                yield return buffer.Trim();
        }

        private IEnumerable<string> TokenWindow(string text)
        {
            var tokens = _tokenizer.Encode(text);

            // If text fits in chunk size, return as-is
            if (tokens.Count <= _chunkSize)
            {
                var trimmed = text.Trim();
                if (trimmed.Length >= MinChars)
                    yield return trimmed;
                yield break;
            }

            // Slide a window across tokens with overlap
            for (int start = 0; start < tokens.Count; start += _chunkSize - _chunkOverlap)
            {
                var end = Math.Min(start + _chunkSize, tokens.Count);
                var chunkTokens = tokens.Skip(start).Take(end - start).ToList();
                var chunk = _tokenizer.Decode(chunkTokens).Trim();

                if (chunk.Length >= MinChars)
                    yield return chunk;

                // Prevent infinite loop - ensure forward progress
                if (end >= tokens.Count)
                    break;
            }
        }
    }
}