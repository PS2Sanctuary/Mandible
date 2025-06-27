using System;
using System.Runtime.InteropServices;
using ZlibNGSharpMinimal.Exceptions;

namespace Mandible.Util.Zlib;

/// <summary>
/// Represents an interface to zlib-ng's deflation algorithm.
/// </summary>
public sealed unsafe class ZngDeflator : IDisposable
{
    private readonly ZlibCompressionLevel _selectedLevel;
    private readonly int _selectedWindowBits;

    private ZlibStream _stream;

    /// <summary>
    /// Gets or sets a value indicating whether this <see cref="ZngDeflator"/> instance has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZngDeflator"/> class.
    /// </summary>
    /// <exception cref="ZngCompressionException"></exception>
    public ZngDeflator(ZlibCompressionLevel level, bool includeZlibHeader = true)
    {
        _selectedLevel = level;
        _selectedWindowBits = includeZlibHeader
            ? ZlibConstants.ZLib_DefaultWindowBits
            : ZlibConstants.Deflate_DefaultWindowBits;

        fixed (ZlibStream* stream = &_stream)
        {
            ZlibErrorCode errC = ZlibInterop.DeflateInit2_
            (
                stream,
                level,
                ZlibCompressionMethod.Deflated,
                _selectedWindowBits,
                ZlibConstants.Deflate_DefaultMemLevel,
                ZlibCompressionStrategy.DefaultStrategy
            );

            if (errC is not ZlibErrorCode.Ok)
                GenerateCompressionError(errC, "Failed to initialize deflator");
        }
    }

    /// <summary>
    /// Deflates a buffer.
    /// </summary>
    /// <param name="input">The buffer.</param>
    /// <param name="output">The output buffer.</param>
    /// <param name="flushMethod">The flush method to use.</param>
    /// <returns>The number of deflated bytes that were produced.</returns>
    /// <exception cref="ZngCompressionException"></exception>
    public ulong Deflate
    (
        ReadOnlySpan<byte> input,
        Span<byte> output,
        ZlibFlushCode flushMethod = ZlibFlushCode.Finish
    )
    {
        Checks();

        fixed (byte* nextIn = input)
        {
            fixed (byte* nextOut = output)
            {
                _stream.NextIn = nextIn;
                _stream.AvailableIn = (uint)input.Length;

                _stream.NextOut = nextOut;
                _stream.availOut = (uint)output.Length;

                fixed (ZlibStream* stream = &_stream)
                {
                    ZlibErrorCode deflateResult = ZlibInterop.Deflate(stream, flushMethod);
                    if (deflateResult is not ZlibErrorCode.StreamEnd)
                        GenerateCompressionError(deflateResult, "Failed to inflate");
                }
            }
        }

        return (ulong)input.Length - _stream.availOut;
    }

    /// <summary>
    /// Resets the internal state of the inflater.
    /// </summary>
    /// <exception cref="ZngCompressionException"></exception>
    public void Reset()
    {
        Checks();

        _stream.NextIn = null;
        _stream.AvailableIn = 0;
        _stream.NextOut = null;
        _stream.availOut = 0;

        fixed (ZlibStream* stream = &_stream)
        {
            ZlibErrorCode errC = ZlibInterop.DeflateEnd(stream);
            if (errC is not ZlibErrorCode.Ok)
                GenerateCompressionError(errC, "Failed to end deflate");

            errC = ZlibInterop.DeflateInit2_
            (
                stream,
                _selectedLevel,
                ZlibCompressionMethod.Deflated,
                _selectedWindowBits,
                ZlibConstants.Deflate_DefaultMemLevel,
                ZlibCompressionStrategy.DefaultStrategy
            );
            if (errC is not ZlibErrorCode.Ok)
                GenerateCompressionError(errC, "Failed to re-init deflater");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (IsDisposed)
            return;

        fixed (ZlibStream* stream = &_stream)
            ZlibInterop.DeflateEnd(stream);

        IsDisposed = true;
        GC.SuppressFinalize(this);
    }

    private void Checks()
    {
        if (IsDisposed)
            throw new ObjectDisposedException(nameof(ZngDeflator));
    }

    private void GenerateCompressionError(ZlibErrorCode result, string genericMessage)
    {
        string? msg = _stream.ErrorMessage != IntPtr.Zero
                ? Marshal.PtrToStringAnsi(_stream.ErrorMessage)
                : genericMessage;

        ZlibNGSharpMinimal.CompressionResult res = (ZlibNGSharpMinimal.CompressionResult)result;
        throw new ZngCompressionException(res, msg);
    }

    ~ZngDeflator()
    {
        Dispose();
    }
}
