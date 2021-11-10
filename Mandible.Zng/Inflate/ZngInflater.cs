﻿using Mandible.Zng.Core;
using Mandible.Zng.Exceptions;
using System;
using System.Runtime.InteropServices;

namespace Mandible.Zng.Inflate;

public sealed unsafe partial class ZngInflater
{
    public const string LibraryName = Zlib.LibraryName;

    /// <summary>
    /// Initializes the internal stream state for decompression.
    /// The fields <see cref="ZngStream.AllocationFunction"/>, <see cref="ZngStream.FreeFunction"/>
    /// and <see cref="ZngStream.Opaque"/> must be initialized before by the caller.
    /// If <see cref="ZngStream.AllocationFunction"/> and <see cref="ZngStream.FreeFunction"/> are set to <see cref="IntPtr.Zero"/>,
    /// inflateInit updates them to use default allocation functions.
    /// </summary>
    /// <param name="stream">The stream to initialize.</param>
    /// <param name="version">The expected version of the zlib library.</param>
    /// <param name="streamSize">The size of the <see cref="ZngStream"/> object.</param>
    /// <returns>
    /// <see cref="CompressionResult.OK"/> on success,
    /// <see cref="CompressionResult.MemoryError"/> if there was not enough memory,
    /// <see cref="CompressionResult.VersionError"/> if the zlib library version is incompatible with the caller's assumed version,
    /// or <see cref="CompressionResult.StreamError"/> if the parameters are invalid.
    /// </returns>
    [DllImport(LibraryName, EntryPoint = "zng_inflateInit_")]
    private static extern CompressionResult _InflateInit(ZngStream* stream, byte* version, int streamSize);

    [DllImport(LibraryName, EntryPoint = "zng_inflate")]
    private static extern CompressionResult _Inflate(ZngStream* stream, InflateFlushMethod flushMethod);

    /// <summary>
    /// Frees any dynamically allocated data structures for the given stream.
    /// This function discards any unprocessed input and does not flush any pending output.
    /// </summary>
    /// <param name="stream">The stream to free.</param>
    /// <returns>
    /// <see cref="CompressionResult.OK"/> on success,
    /// or <see cref="CompressionResult.StreamError"/> if the stream state was inconsistent.
    /// </returns>
    [DllImport(LibraryName, EntryPoint = "zng_inflateEnd")]
    private static extern CompressionResult _InflateEnd(ZngStream* stream);

    /// <summary>
    /// This function is equivalent to <see cref="_InflateEnd(ZngStream*)"/>
    /// followed by <see cref="_InflateInit(ZngStream*)"/>,
    /// but does not free and reallocate the internal decompression state.
    /// The stream will keep attributes that may have been set by inflateInit2.
    /// </summary>
    /// <param name="stream">The stream to reset.</param>
    /// <returns>
    /// <see cref="CompressionResult.OK"/> on success,
    /// or <see cref="CompressionResult.StreamError"/> if the stream state was inconsistent.
    /// </returns>
    [DllImport(LibraryName, EntryPoint = "zng_inflateReset")]
    private static extern CompressionResult _InflateReset(ZngStream* stream);
}

/// <summary>
/// Represents an inflater utilising the zlib-ng algorithm.
/// </summary>
public sealed unsafe partial class ZngInflater : IDisposable
{
    private readonly ZngStream* _streamPtr;

    /// <summary>
    /// Gets or sets a value indicating whether or not this instance has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZngInflater"/> class.
    /// </summary>
    /// <exception cref="ZngCompressionException"></exception>
    public ZngInflater()
    {
        ZngStream stream = new()
        {
            AllocationFunction = null,
            FreeFunction = null,
            Opaque = IntPtr.Zero,
        };

        _streamPtr = (ZngStream*)Marshal.AllocHGlobal(sizeof(ZngStream));
        Marshal.StructureToPtr(stream, (IntPtr)_streamPtr, false);

        CompressionResult initResult = _InflateInit(_streamPtr, Zlib._Version(), sizeof(ZngStream));
        if (initResult is not CompressionResult.OK)
            GenerateCompressionError(initResult, "Failed to initialize");
    }

    /// <summary>
    /// Inflates a compressed buffer. The buffer should contain the complete deflated sequence.
    /// </summary>
    /// <param name="input">The compressed buffer.</param>
    /// <param name="output">The output buffer.</param>
    /// <exception cref="ZngCompressionException"></exception>
    public void Inflate(ReadOnlySpan<byte> input, Span<byte> output)
    {
        Checks();

        fixed (byte* nextIn = input)
        {
            fixed (byte* nextOut = output)
            {
                CompressionResult inflateResult = CompressionResult.StreamEnd;

                (*_streamPtr).NextIn = nextIn;
                (*_streamPtr).AvailableIn = (uint)input.Length;

                (*_streamPtr).NextOut = nextOut;
                (*_streamPtr).AvailableOut = (uint)output.Length;

                inflateResult = _Inflate(_streamPtr, InflateFlushMethod.Finish);

                if (inflateResult is not CompressionResult.StreamEnd)
                    GenerateCompressionError(inflateResult, "Failed to inflate");
            }
        }
    }

    /// <summary>
    /// Resets the internal state of the inflater.
    /// </summary>
    /// <exception cref="ZngCompressionException"></exception>
    public void Reset()
    {
        Checks();

        (*_streamPtr).NextIn = (byte*)IntPtr.Zero;
        (*_streamPtr).AvailableIn = 0;

        (*_streamPtr).NextOut = (byte*)IntPtr.Zero;
        (*_streamPtr).AvailableOut = 0;

        CompressionResult result = _InflateReset(_streamPtr);
        if (result is not CompressionResult.OK)
            GenerateCompressionError(result, "Failed to reset inflater");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!IsDisposed)
        {
            _InflateEnd(_streamPtr);
            Marshal.DestroyStructure<ZngStream>((IntPtr)_streamPtr);
            Marshal.FreeHGlobal((IntPtr)_streamPtr);

            IsDisposed = true;
        }

        GC.SuppressFinalize(this);
    }

    private void Checks()
    {
        Zlib.ThrowIfInvalidVersion();

        if (IsDisposed)
            throw new ObjectDisposedException(nameof(ZngInflater));
    }

    private void GenerateCompressionError(CompressionResult result, string genericMessage)
    {
        string? msg = (*_streamPtr).ErrorMessage != null
                ? Marshal.PtrToStringAnsi((IntPtr)(*_streamPtr).ErrorMessage)
                : genericMessage;

        throw new ZngCompressionException(result, msg);
    }

    ~ZngInflater()
    {
        Dispose();
    }
}
