using System;

namespace Mandible.Util.Zlib;

public class ZlibException : Exception
{
    public ZlibErrorCode ErrorCode { get; }

    public ZlibException(ZlibErrorCode error, string? message = null)
        : base(message)
    {
        ErrorCode = error;
    }
}
