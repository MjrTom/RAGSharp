using System;
using System.Linq;

namespace RAGSharp.Utils
{
    /// <summary>
    /// Extension methods for working with vector embeddings.
    /// Provides normalization and cosine similarity utilities.
    /// </summary>
    public static class VectorExtensions
    {
        /// <summary>
        /// Normalize divides each element by the vector’s length so the whole vector ends up with length = 1.
        /// If the vector is all zeros, the original vector is returned.
        /// </summary>
        /// <param name="v">Input vector.</param>
        /// <returns>A normalized copy of the vector.</returns>
        public static float[] Normalize(this float[] v)
        {
            var norm = Math.Sqrt(v.Sum(x => x * x));
            return norm == 0 ? v : v.Select(x => (float)(x / norm)).ToArray();
        }

        /// <summary>
        /// Compute the cosine similarity between two vectors.
        /// Returns 0 if vectors are different lengths or if either is all zeros.
        /// </summary>
        /// <param name="v1">First vector.</param>
        /// <param name="v2">Second vector.</param>
        /// <returns>Cosine similarity in the range [-1, 1].</returns>
        public static double CosineSimilarity(this float[] v1, float[] v2)
        {
            if (v1.Length != v2.Length) return 0.0;

            double dot = 0, norm1 = 0, norm2 = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                dot += v1[i] * v2[i];
                norm1 += v1[i] * v1[i];
                norm2 += v2[i] * v2[i];
            }
            return norm1 == 0 || norm2 == 0 ? 0 : dot / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }
    }
}
