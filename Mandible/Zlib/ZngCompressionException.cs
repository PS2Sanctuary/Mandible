﻿using System;

namespace Mandible.Zlib
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1194:Implement exception constructors.")]
    public class ZngCompressionException : Exception
    {
        public CompressionResult ErrorCode { get; }

        public ZngCompressionException(CompressionResult error)
            : this(error, null)
        {
        }

        public ZngCompressionException(CompressionResult error, string? message)
            : base(message)
        {
            ErrorCode = error;
        }

        public override string ToString()
        {
            return $"ZlibCompressionException | Error: {ErrorCode}, {Message}";
        }
    }
}