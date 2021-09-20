using System.Runtime.InteropServices;

namespace Mandible.Zlib
{
#pragma warning disable IDE1006 // Naming Styles

    public static unsafe partial class ZlibNG
    {
        private const string LibraryName = "zlib-ng2.dll";

        /// <summary>
        /// Gets the version of the linked zlib-ng library as a 1-byte wide character string.
        /// </summary>
        /// <returns>A 1-byte wide character string.</returns>
        [DllImport(LibraryName)]
        private static extern byte* zlibng_version();

        [DllImport(LibraryName)]
        private static extern int zng_inflateInit_(zng_stream* strm, byte* version, int stream_size);

        [DllImport(LibraryName)]
        private static extern int zng_inflate(zng_stream* strm, int flush);

        [DllImport(LibraryName)]
        private static extern int zng_inflateEnd(zng_stream* strm);

        [StructLayout(LayoutKind.Sequential)]
        public struct alloc_func
        {
            public delegate* unmanaged<void*, uint, uint, void*> Pointer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct free_func
        {
            public delegate* unmanaged<void*, void*, void> Pointer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct internal_state
        {
        }

        [StructLayout(LayoutKind.Explicit, Size = 104, Pack = 8)]
        public struct zng_stream
        {
            [FieldOffset(0)] // size = 8, padding = 0
            public byte* next_in;

            [FieldOffset(8)] // size = 4, padding = 4
            public uint avail_in;

            [FieldOffset(16)] // size = 8, padding = 0
            public ulong total_in;

            [FieldOffset(24)] // size = 8, padding = 0
            public byte* next_out;

            [FieldOffset(32)] // size = 4, padding = 4
            public uint avail_out;

            [FieldOffset(40)] // size = 8, padding = 0
            public ulong total_out;

            [FieldOffset(48)] // size = 8, padding = 0
            public byte* msg;

            [FieldOffset(56)] // size = 8, padding = 0
            public internal_state* state;

            [FieldOffset(64)] // size = 8, padding = 0
            public alloc_func zalloc;

            [FieldOffset(72)] // size = 8, padding = 0
            public free_func zfree;

            [FieldOffset(80)] // size = 8, padding = 0
            public void* opaque;

            [FieldOffset(88)] // size = 4, padding = 0
            public int data_type;

            [FieldOffset(92)] // size = 4, padding = 0
            public uint adler;

            [FieldOffset(96)] // size = 4, padding = 4
            public uint reserved;
        }
    }

    #pragma warning restore IDE1006 // Naming Styles

    public static unsafe partial class ZlibNG
    {
        public static CompressionResult InflateInit(zng_stream* stream)
            => (CompressionResult)zng_inflateInit_(stream, zlibng_version(), sizeof(zng_stream));

        public static CompressionResult Inflate(zng_stream* stream, ZlibFlushType flushType)
            => (CompressionResult)zng_inflate(stream, (int)flushType);

        public static CompressionResult InflateEnd(zng_stream* stream)
            => (CompressionResult)zng_inflateEnd(stream);
    }
}
