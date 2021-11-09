using Mandible.Zng.Core;
using Mandible.Zng.Exceptions;
using System;
using System.Runtime.InteropServices;

namespace Mandible.Zng.Deflate
{
    public sealed unsafe partial class ZngDeflater
    {
        public const string LibraryName = Zlib.LibraryName;

        /// <summary>
        /// Initializes the internal stream state for compression.
        /// The fields <see cref="ZngStream.AllocationFunction"/>, <see cref="ZngStream.FreeFunction"/>
        /// and <see cref="ZngStream.Opaque"/> must be initialized before by the caller.
        /// If <see cref="ZngStream.AllocationFunction"/> and <see cref="ZngStream.FreeFunction"/> are set to <see cref="IntPtr.Zero"/>,
        /// deflateInit updates them to use default allocation functions.
        /// </summary>
        /// <param name="stream">The stream to initialize.</param>
        /// <param name="level">The compression level to operate at.</param>
        /// <param name="version">The expected version of the zlib library.</param>
        /// <param name="streamSize">The size of the <see cref="ZngStream"/> object.</param>
        /// <returns>
        /// <see cref="CompressionResult>OK"/> on success,
        /// <see cref="CompressionResult.MemoryError"/> if there was not enough memory,
        /// <see cref="CompressionResult.StreamEnd"/> if the level is not valid,
        /// or <see cref="CompressionResult.VersionError"/> if the zlib library version is incompatible with the caller's assumed version.
        /// </returns>
        [DllImport(LibraryName, EntryPoint = "zng_deflateInit_")]
        private static extern CompressionResult _DeflateInit(ZngStream* stream, CompressionLevel level, byte* version, int streamSize);

        [DllImport(LibraryName, EntryPoint = "zng_deflate")]
        private static extern CompressionResult _Deflate(ZngStream* stream, DeflateFlushMethod flushMethod);

        /// <summary>
        /// Frees any dynamically allocated data structures for the given stream.
        /// This function discards any unprocessed input and does not flush any pending output.
        /// </summary>
        /// <param name="stream">The stream to free.</param>
        /// <returns>
        /// <see cref="CompressionResult.OK"/> on success,
        /// <see cref="CompressionResult.DataError"/> if the stream was freed prematurely (some input or output was discarded).
        /// or <see cref="CompressionResult.StreamError"/> if the stream state was inconsistent.
        /// </returns>
        [DllImport(LibraryName, EntryPoint = "zng_deflateEnd")]
        private static extern CompressionResult _DeflateEnd(ZngStream* stream);

        /// <summary>
        /// This function is equivalent to <see cref="_DeflateEnd(ZngStream*)"/>
        /// followed by <see cref="_DeflateInit(ZngStream*)"/>,
        /// but does not free and reallocate the internal decompression state.
        /// The stream will keep attributes that may have been set by <see cref="_DeflateInit(ZngStream*, CompressionLevel, byte*, int)"/>.
        /// </summary>
        /// <param name="stream">The stream to reset.</param>
        /// <returns>
        /// <see cref="CompressionResult.OK"/> on success,
        /// or <see cref="CompressionResult.StreamError"/> if the stream state was inconsistent.
        /// </returns>
        [DllImport(LibraryName, EntryPoint = "zng_deflateReset")]
        private static extern CompressionResult _DeflateReset(ZngStream* stream);
    }

    /// <summary>
    /// Represents an inflater utilising the zlib-ng algorithm.
    /// </summary>
    public sealed unsafe partial class ZngDeflater : IDisposable
    {
        private readonly ZngStream* _streamPtr;

        /// <summary>
        /// Gets or sets a value indicating whether or not this instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ZngDeflater"/> class.
        /// </summary>
        /// <exception cref="ZngCompressionException"></exception>
        public ZngDeflater(CompressionLevel compressionLevel = CompressionLevel.BestCompression)
        {
            ZngStream stream = new()
            {
                AllocationFunction = null,
                FreeFunction = null,
                Opaque = IntPtr.Zero,
            };

            _streamPtr = (ZngStream*)Marshal.AllocHGlobal(sizeof(ZngStream));
            Marshal.StructureToPtr(stream, (IntPtr)_streamPtr, false);

            CompressionResult initResult = _DeflateInit(_streamPtr, compressionLevel, Zlib._Version(), sizeof(ZngStream));
            if (initResult is not CompressionResult.OK)
                GenerateCompressionError(initResult, "Failed to initialize deflater");
        }

        /// <summary>
        /// Deflates a buffer. The buffer should contain the complete input sequence.
        /// </summary>
        /// <param name="input">The buffer.</param>
        /// <param name="output">The output buffer.</param>
        /// <returns>The number of deflated bytes that were produced.</returns>
        /// <exception cref="ZngCompressionException"></exception>
        public nuint Deflate(ReadOnlySpan<byte> input, Span<byte> output)
        {
            Checks();

            fixed (byte* nextIn = input)
            {
                fixed (byte* nextOut = output)
                {
                    (*_streamPtr).NextIn = nextIn;
                    (*_streamPtr).AvailableIn = (uint)input.Length;

                    (*_streamPtr).NextOut = nextOut;
                    (*_streamPtr).AvailableOut = (uint)output.Length;

                    CompressionResult deflateResult = _Deflate(_streamPtr, DeflateFlushMethod.Finish);
                    if (deflateResult is not CompressionResult.StreamEnd)
                        GenerateCompressionError(deflateResult, "Failed to inflate");
                }
            }

            return (*_streamPtr).TotalOut;
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

            (*_streamPtr).TotalOut = UIntPtr.Zero;

            CompressionResult result = _DeflateReset(_streamPtr);
            if (result is not CompressionResult.OK)
                GenerateCompressionError(result, "Failed to reset deflater");
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!IsDisposed)
            {
                _DeflateEnd(_streamPtr);
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
                throw new ObjectDisposedException(nameof(ZngDeflater));
        }

        private void GenerateCompressionError(CompressionResult result, string genericMessage)
        {
            string? msg = (*_streamPtr).ErrorMessage != null
                    ? Marshal.PtrToStringAnsi((IntPtr)(*_streamPtr).ErrorMessage)
                    : genericMessage;

            throw new ZngCompressionException(result, msg);
        }

        ~ZngDeflater()
        {
            Dispose();
        }
    }
}
