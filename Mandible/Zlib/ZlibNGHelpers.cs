namespace Mandible.Zlib
{
    internal static class ZlibNGHelpers
    {
        public static unsafe ZlibNG.zng_stream GetInflateStream(uint compressedLength, byte* data)
            => new()
            {
                zalloc = new ZlibNG.alloc_func
                {
                    Pointer = null
                },
                zfree = new ZlibNG.free_func
                {
                    Pointer = null
                },
                opaque = (void*)0,
                avail_in = compressedLength,
                next_in = data
            };

        public static unsafe CompressionResult InitialiseInflate(ZlibNG.zng_stream* stream)
        {
            CompressionResult res = ZlibNG.InflateInit(stream);
            if (res != CompressionResult.OK)
                throw new ZlibNGCompressionException("Failed to initialise the inflate algorithm.", res);

            return res;
        }

        public static unsafe CompressionResult EndInflate(ZlibNG.zng_stream* stream)
        {
            CompressionResult res = ZlibNG.InflateEnd(stream);
            if (res != ZlibNGConstants.Z_OK)
                throw new ZlibNGCompressionException("Failed to end inflate.", res);

            return res;
        }
    }
}
