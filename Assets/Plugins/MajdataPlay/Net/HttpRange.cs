using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MajdataPlay.Net
{
    /// <summary>
    /// Represents the download range for an HTTP 206 Partial Content request.
    /// </summary>
    public readonly struct HttpRange : IEquatable<HttpRange>
    {
        /// <summary>
        /// The starting byte position. 
        /// If null, it indicates a suffix byte range request (fetching the last N bytes of the file).
        /// </summary>
        public long? Start { get; }

        /// <summary>
        /// The ending byte position. 
        /// If null, it indicates requesting data up to the end of the file.
        /// </summary>
        public long? End { get; }

        // Private constructor to enforce the use of intent-revealing static factory methods
        HttpRange(long? start, long? end)
        {
            if (start.HasValue && start < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(start), "Start position cannot be less than 0.");
            }

            if (end.HasValue && end < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(end), "End position cannot be less than 0.");
            }

            if (start.HasValue && end.HasValue && start.Value > end.Value)
            {
                throw new ArgumentException("Start position cannot be greater than the end position.");
            }

            if (!start.HasValue && !end.HasValue)
            {
                throw new ArgumentException("Start and End cannot both be null.");
            }

            Start = start;
            End = end;
        }

        /// <summary>
        /// 1. Resume Download: Requests from the specified position to the end of the file (e.g., bytes=1000-).
        /// </summary>
        /// <param name="start">The number of bytes already downloaded (starting index).</param>
        public static HttpRange From(long start)
        {
            return new HttpRange(start, null);
        }

        /// <summary>
        /// 2. Exact Fragment: Requests an exact closed-interval byte range (e.g., bytes=1000-2000).
        /// </summary>
        /// <param name="start">The starting index.</param>
        /// <param name="end">The ending index (inclusive).</param>
        public static HttpRange Between(long start, long end)
        {
            return new HttpRange(start, end);
        }

        /// <summary>
        /// 3. Tail Fragment: Requests only the last N bytes of the file (e.g., bytes=-500).
        /// </summary>
        /// <param name="suffixLength">The number of bytes to download from the end of the file.</param>
        public static HttpRange Tail(long suffixLength)
        {
            if (suffixLength <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(suffixLength), "Suffix length must be greater than 0.");
            }

            return new HttpRange(null, suffixLength);
        }

        /// <summary>
        /// Converts to the native RangeHeaderValue used by HttpClient.
        /// </summary>
        public RangeHeaderValue ToRangeHeaderValue()
        {
            return new RangeHeaderValue(Start, End);
        }

        /// <summary>
        /// Returns the standard HTTP protocol string representation of the Range, 
        /// useful for debugging and logging.
        /// </summary>
        public override string ToString()
        {
            if (Start.HasValue && End.HasValue)
            {
                // e.g., [1000, 2000]
                return $"bytes={Start}-{End}";
            }

            if (Start.HasValue)
            {
                // e.g., [1000, End of file]
                return $"bytes={Start}-";      
            }

            // e.g., [Last 500 bytes]
            return $"bytes=-{End}";
        }

        public bool Equals(HttpRange other) => Start == other.Start && End == other.End;
        public override bool Equals(object obj) => obj is HttpRange other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Start, End);
        public static bool operator ==(HttpRange left, HttpRange right) => left.Equals(right);
        public static bool operator !=(HttpRange left, HttpRange right) => !left.Equals(right);
    }
}
