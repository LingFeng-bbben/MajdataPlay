using System;

namespace ManagedBass
{
    /// <summary>
    /// An Exception wrapping <see cref="Bass.LastError"/> or BassAsio.LastError.
    /// </summary>
    public class BassException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="BassException"/> with <see cref="Bass.LastError"/>.
        /// </summary>
        public BassException() : this(Bass.LastError) { }

        /// <summary>
        /// Creates a new instance of <see cref="BassException"/> with provided Error code.
        /// </summary>
        public BassException(Errors ErrorCode)
            : base($"Error: {ErrorCode}")
        {
            this.ErrorCode = ErrorCode;
        }

        /// <summary>
        /// Gets the Bass Error Code.
        /// </summary>
        public Errors ErrorCode { get; }
    }
}
