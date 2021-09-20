using System;

namespace Mandible.Zlib
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.")]
    public class ZlibNGCompressionException : Exception
    {
        public CompressionResult ErrorCode { get; }

        public ZlibNGCompressionException(CompressionResult error)
            : this(null, error)
        {
        }

        public ZlibNGCompressionException(string? message, CompressionResult error)
            : base(message)
        {
            ErrorCode = error;
        }
    }
}
