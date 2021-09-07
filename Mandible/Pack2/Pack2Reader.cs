using Microsoft.Win32.SafeHandles;
using System.Buffers.Binary;
using System.IO.Compression;

namespace Mandible.Pack2
{
    public class Pack2Reader : IDisposable
    {
        /// <summary>
        /// The indicator placed in front of an asset data block to indicate that it has been compressed.
        /// </summary>
        protected const uint ASSET_COMPRESSION_INDICATOR = 2712847316;

        protected readonly SafeFileHandle _packFileHandle;
        protected readonly FileStream _packFileStream;

        protected Pack2Header? _cachedHeader;
        protected IReadOnlyList<Asset2Header>? _cachedAssetHeaders;

        /// <summary>
        /// Gets a value indicating whether or not this <see cref="Pack2Reader"/> instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; protected set; }

        public Pack2Reader(string packLocation)
        {
            _packFileHandle = File.OpenHandle(
                packLocation,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                FileOptions.RandomAccess | FileOptions.Asynchronous
            );

            _packFileStream = new FileStream(_packFileHandle, FileAccess.Read, 16384, true);
        }

        /// <summary>
        /// Reads the pack header. Subsequent calls return a cached value.
        /// </summary>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>The pack header.</returns>
        public async Task<Pack2Header> ReadHeaderAsync(CancellationToken ct = default)
        {
            if (_cachedHeader is not null)
                return _cachedHeader.Value;

            Memory<byte> headerBuffer = new(new byte[Pack2Header.SIZE]);

            await RandomAccess.ReadAsync(_packFileHandle, headerBuffer, 0, ct).ConfigureAwait(false);
            _cachedHeader = Pack2Header.Deserialise(headerBuffer.Span);

            return _cachedHeader.Value;
        }

        /// <summary>
        /// Gets the asset header list. Subsequent calls return a cached value.
        /// </summary>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>The list of asset headers.</returns>
        public async Task<IReadOnlyList<Asset2Header>> ReadAssetHeadersAsync(CancellationToken ct = default)
        {
            if (_cachedAssetHeaders is not null)
                return _cachedAssetHeaders;

            Pack2Header header = await ReadHeaderAsync(ct).ConfigureAwait(false);
            List<Asset2Header> assetHeaders = new();

            Memory<byte> assetHeadersBuffer = new(new byte[header.AssetCount * Asset2Header.SIZE]);

            await RandomAccess.ReadAsync(_packFileHandle, assetHeadersBuffer, (long)header.AssetMapOffset, ct).ConfigureAwait(false);

            for (uint i = 0; i < header.AssetCount; i++)
            {
                int baseOffset = (int)i * Asset2Header.SIZE;
                Memory<byte> assetHeaderData = assetHeadersBuffer.Slice(baseOffset, Asset2Header.SIZE);

                Asset2Header assetHeader = Asset2Header.Deserialise(assetHeaderData.Span);
                assetHeaders.Add(assetHeader);
            }

            _cachedAssetHeaders = assetHeaders;
            return _cachedAssetHeaders;
        }

        /// <summary>
        /// Gets a stream to retrieve the unzipped asset data. Note that the asset data is stored and returned in big endian format.
        /// </summary>
        /// <param name="assetHeader">The asset to retrieve.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>A stream of the asset data.</returns>
        public async Task<ReadOnlyMemory<byte>> ReadAssetData(Asset2Header assetHeader, CancellationToken ct = default)
        {
            byte[] data;
            _packFileStream.Seek((long)assetHeader.DataOffset, SeekOrigin.Begin);

            if (assetHeader.ZipStatus == AssetZipDefinition.Zipped || assetHeader.ZipStatus == AssetZipDefinition.ZippedAlternate)
            {
                // Read the compression information
                Memory<byte> compressionBlock = new(new byte[8]);
                await _packFileStream.ReadAsync(compressionBlock, ct).ConfigureAwait(false);

                uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(compressionBlock.Span);
                uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(compressionBlock[4..8].Span);

                if (compressionIndicator != ASSET_COMPRESSION_INDICATOR)
                    throw new InvalidDataException("The asset header indicated that this asset was compressed, but no compression indicator was found in the asset data.");

                // Read the data
                data = new byte[decompressedLength];
                using DeflateStream dfs = new(_packFileStream, CompressionMode.Decompress, true);

                await dfs.ReadAsync(data, ct).ConfigureAwait(false);
            }
            else
            {
                data = new byte[assetHeader.DataSize];
                await _packFileStream.ReadAsync(data, ct).ConfigureAwait(false);
            }

            return data;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _packFileHandle.Dispose();
                    _packFileStream.Dispose();
                }

                _cachedAssetHeaders = null;

                IsDisposed = true;
            }
        }
    }
}
