using ICSharpCode.SharpZipLib.Zip.Compression;
using Microsoft.Win32.SafeHandles;
using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Pack2
{
    public class Pack2Reader : IDisposable
    {
        /// <summary>
        /// The indicator placed in front of an asset data block to indicate that it has been compressed.
        /// </summary>
        protected const uint ASSET_COMPRESSION_INDICATOR = 2712847316;

        protected readonly SafeFileHandle _packFileHandle;
        protected readonly Inflater _inflater;
        protected readonly ArrayPool<byte> _arrayPool;

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

            _inflater = new Inflater();
            _arrayPool = ArrayPool<byte>.Shared;
        }

        /// <summary>
        /// Reads the pack header. Subsequent calls return a cached value.
        /// </summary>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>The pack header.</returns>
        public virtual async Task<Pack2Header> ReadHeaderAsync(CancellationToken ct = default)
        {
            if (_cachedHeader is not null)
                return _cachedHeader.Value;

            byte[] data = _arrayPool.Rent(Pack2Header.SIZE);
            Memory<byte> headerBuffer = new(data, 0, Pack2Header.SIZE);

            await RandomAccess.ReadAsync(_packFileHandle, headerBuffer, 0, ct).ConfigureAwait(false);
            _cachedHeader = Pack2Header.Deserialise(headerBuffer.Span);

            _arrayPool.Return(data);
            return _cachedHeader.Value;
        }

        /// <summary>
        /// Reads the pack header. Subsequent calls return a cached value.
        /// </summary>
        /// <returns>The pack header.</returns>
        public virtual Pack2Header ReadHeader()
        {
            if (_cachedHeader is not null)
                return _cachedHeader.Value;

            byte[] data = _arrayPool.Rent(Pack2Header.SIZE);
            Span<byte> headerBuffer = new(data, 0, Pack2Header.SIZE);

            RandomAccess.Read(_packFileHandle, headerBuffer, 0);
            _cachedHeader = Pack2Header.Deserialise(headerBuffer);

            _arrayPool.Return(data);
            return _cachedHeader.Value;
        }

        /// <summary>
        /// Gets the asset header list. Subsequent calls return a cached value.
        /// </summary>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>The list of asset headers.</returns>
        public virtual async Task<IReadOnlyList<Asset2Header>> ReadAssetHeadersAsync(CancellationToken ct = default)
        {
            if (_cachedAssetHeaders is not null)
                return _cachedAssetHeaders;

            Pack2Header header = await ReadHeaderAsync(ct).ConfigureAwait(false);
            List<Asset2Header> assetHeaders = new();

            int bufferSize = (int)header.AssetCount * Asset2Header.SIZE;
            byte[] data = _arrayPool.Rent(bufferSize);
            Memory<byte> assetHeadersBuffer = new(data, 0, bufferSize);

            await RandomAccess.ReadAsync(_packFileHandle, assetHeadersBuffer, (long)header.AssetMapOffset, ct).ConfigureAwait(false);

            for (uint i = 0; i < header.AssetCount; i++)
            {
                int baseOffset = (int)i * Asset2Header.SIZE;
                Memory<byte> assetHeaderData = assetHeadersBuffer.Slice(baseOffset, Asset2Header.SIZE);

                Asset2Header assetHeader = Asset2Header.Deserialise(assetHeaderData.Span);
                assetHeaders.Add(assetHeader);
            }

            _cachedAssetHeaders = assetHeaders;
            _arrayPool.Return(data);
            return _cachedAssetHeaders;
        }

        /// <summary>
        /// Gets the asset header list. Subsequent calls return a cached value.
        /// </summary>
        /// <returns>The list of asset headers.</returns>
        public virtual IReadOnlyList<Asset2Header> ReadAssetHeaders()
        {
            if (_cachedAssetHeaders is not null)
                return _cachedAssetHeaders;

            Pack2Header header = ReadHeader();
            List<Asset2Header> assetHeaders = new();

            int bufferSize = (int)header.AssetCount * Asset2Header.SIZE;
            byte[] data = _arrayPool.Rent(bufferSize);
            Span<byte> assetHeadersBuffer = new(data, 0, bufferSize);

            RandomAccess.Read(_packFileHandle, assetHeadersBuffer, (long)header.AssetMapOffset);

            for (uint i = 0; i < header.AssetCount; i++)
            {
                int baseOffset = (int)i * Asset2Header.SIZE;
                Span<byte> assetHeaderData = assetHeadersBuffer.Slice(baseOffset, Asset2Header.SIZE);

                Asset2Header assetHeader = Asset2Header.Deserialise(assetHeaderData);
                assetHeaders.Add(assetHeader);
            }

            _cachedAssetHeaders = assetHeaders;
            _arrayPool.Return(data);
            return _cachedAssetHeaders;
        }

        /// <summary>
        /// Gets a stream to retrieve the unzipped asset data. Note that the asset data is stored and returned in big endian format.
        /// </summary>
        /// <param name="assetHeader">The asset to retrieve.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> used to stop the operation.</param>
        /// <returns>A stream of the asset data.</returns>
        public virtual async Task<ReadOnlyMemory<byte>> ReadAssetDataAsync(Asset2Header assetHeader, CancellationToken ct = default)
        {
            // We can't use the array pool here, because the data is being returned to the user.
            byte[] data = new byte[assetHeader.DataSize];
            Memory<byte> dataMem = new(data);

            await RandomAccess.ReadAsync(_packFileHandle, dataMem, (long)assetHeader.DataOffset, ct).ConfigureAwait(false);

            if (assetHeader.ZipStatus == AssetZipDefinition.Zipped || assetHeader.ZipStatus == AssetZipDefinition.ZippedAlternate)
            {
                // Read the compression information
                uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(dataMem[0..4].Span);
                uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(dataMem[4..8].Span);

                if (compressionIndicator != ASSET_COMPRESSION_INDICATOR)
                    throw new InvalidDataException("The asset header indicated that this asset was compressed, but no compression indicator was found in the asset data.");

                // Read the data
                _inflater.SetInput(data[8..]);

                data = new byte[decompressedLength];
                _inflater.Inflate(data);

                _inflater.Reset();
            }

            return data;
        }

        /// <summary>
        /// Gets a stream to retrieve the unzipped asset data. Note that the asset data is stored and returned in big endian format.
        /// </summary>
        /// <param name="assetHeader">The asset to retrieve.</param>
        /// <returns>A stream of the asset data.</returns>
        public virtual ReadOnlySpan<byte> ReadAssetData(Asset2Header assetHeader)
        {
            // We can't use the array pool here, because the data is being returned to the user.
            byte[] data = new byte[assetHeader.DataSize];
            Span<byte> dataMem = new(data);

            RandomAccess.Read(_packFileHandle, dataMem, (long)assetHeader.DataOffset);

            if (assetHeader.ZipStatus == AssetZipDefinition.Zipped || assetHeader.ZipStatus == AssetZipDefinition.ZippedAlternate)
            {
                // Read the compression information
                uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(dataMem[0..4]);
                uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(dataMem[4..8]);

                if (compressionIndicator != ASSET_COMPRESSION_INDICATOR)
                    throw new InvalidDataException("The asset header indicated that this asset was compressed, but no compression indicator was found in the asset data.");

                // Read the data
                _inflater.SetInput(data[8..]);

                data = new byte[decompressedLength];
                _inflater.Inflate(data);

                _inflater.Reset();
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
                }

                _cachedAssetHeaders = null;

                IsDisposed = true;
            }
        }
    }
}
