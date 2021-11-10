using Mandible.Abstractions.Services;
using Mandible.Zng.Inflate;
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
        protected const uint ASSET_COMPRESSION_INDICATOR = 0xA1B2C3D4;

        protected readonly IDataReaderService _dataReader;
        protected readonly ZngInflater _inflater;
        protected readonly MemoryPool<byte> _memoryPool;

        protected Pack2Header? _cachedHeader;
        protected IReadOnlyList<Asset2Header>? _cachedAssetHeaders;

        /// <summary>
        /// Gets a value indicating whether or not this <see cref="Pack2Reader"/> instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; protected set; }

        public Pack2Reader(IDataReaderService dataReader)
        {
            _dataReader = dataReader;

            _memoryPool = MemoryPool<byte>.Shared;
            _inflater = new ZngInflater();
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

            using IMemoryOwner<byte> data = _memoryPool.Rent(Pack2Header.Size);
            Memory<byte> headerBuffer = data.Memory[..Pack2Header.Size];

            await _dataReader.ReadAsync(headerBuffer, 0, ct).ConfigureAwait(false);
            _cachedHeader = Pack2Header.Deserialize(headerBuffer.Span);

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

            using IMemoryOwner<byte> data = _memoryPool.Rent(Pack2Header.Size);
            Span<byte> headerBuffer = data.Memory[..Pack2Header.Size].Span;

            _dataReader.Read(headerBuffer, 0);
            _cachedHeader = Pack2Header.Deserialize(headerBuffer);

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

            int bufferSize = (int)header.AssetCount * Asset2Header.Size;
            using IMemoryOwner<byte> data = _memoryPool.Rent(bufferSize);
            Memory<byte> assetHeadersBuffer = data.Memory[..bufferSize];

            await _dataReader.ReadAsync(assetHeadersBuffer, (long)header.AssetMapOffset, ct).ConfigureAwait(false);

            for (uint i = 0; i < header.AssetCount; i++)
            {
                int baseOffset = (int)i * Asset2Header.Size;
                Memory<byte> assetHeaderData = assetHeadersBuffer.Slice(baseOffset, Asset2Header.Size);

                Asset2Header assetHeader = Asset2Header.Deserialize(assetHeaderData.Span);
                assetHeaders.Add(assetHeader);
            }

            _cachedAssetHeaders = assetHeaders;
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

            int bufferSize = (int)header.AssetCount * Asset2Header.Size;
            using IMemoryOwner<byte> data = _memoryPool.Rent(bufferSize);
            Span<byte> assetHeadersBuffer = data.Memory[..bufferSize].Span;

            _dataReader.Read(assetHeadersBuffer, (long)header.AssetMapOffset);

            for (uint i = 0; i < header.AssetCount; i++)
            {
                int baseOffset = (int)i * Asset2Header.Size;
                Span<byte> assetHeaderData = assetHeadersBuffer.Slice(baseOffset, Asset2Header.Size);

                Asset2Header assetHeader = Asset2Header.Deserialize(assetHeaderData);
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
        public virtual async Task<ReadOnlyMemory<byte>> ReadAssetDataAsync(Asset2Header assetHeader, CancellationToken ct = default)
        {
            Memory<byte> output = new byte[assetHeader.DataSize];
            await _dataReader.ReadAsync(output, (long)assetHeader.DataOffset, ct).ConfigureAwait(false);

            if (assetHeader.ZipStatus == AssetZipDefinition.Zipped || assetHeader.ZipStatus == AssetZipDefinition.ZippedAlternate)
            {
                // Read the compression information
                uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(output[..4].Span);
                uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(output[4..8].Span);

                if (compressionIndicator != ASSET_COMPRESSION_INDICATOR)
                {
                    throw new InvalidDataException
                    (
                        "The asset header indicated that this asset was compressed, but no compression indicator was found in the asset data."
                    );
                }

                output = await Task.Run
                (
                    () =>
                    {
                        Memory<byte> decompData = new byte[decompressedLength];

                        _inflater.Inflate(output[8..].Span, decompData.Span);
                        _inflater.Reset();

                        return decompData;
                    }
                ).ConfigureAwait(false);
            }

            return output;
        }

        /// <summary>
        /// Gets a stream to retrieve the unzipped asset data. Note that the asset data is stored and returned in big endian format.
        /// </summary>
        /// <param name="assetHeader">The asset to retrieve.</param>
        /// <returns>A stream of the asset data.</returns>
        public virtual ReadOnlySpan<byte> ReadAssetData(Asset2Header assetHeader)
        {
            Span<byte> output = new byte[assetHeader.DataSize];
            _dataReader.Read(output, (long)assetHeader.DataOffset);

            if (assetHeader.ZipStatus == AssetZipDefinition.Zipped || assetHeader.ZipStatus == AssetZipDefinition.ZippedAlternate)
            {
                // Read the compression information
                uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(output[..4]);
                uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(output[4..8]);

                if (compressionIndicator != ASSET_COMPRESSION_INDICATOR)
                {
                    throw new InvalidDataException
                    (
                        "The asset header indicated that this asset was compressed, but no compression indicator was found in the asset data."
                    );
                }

                Span<byte> decompData = new byte[decompressedLength];

                _inflater.Inflate(output[8..], decompData);
                _inflater.Reset();

                output = decompData;
            }

            return output;
        }

        /// <inheritdoc />
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
                    _inflater.Dispose();
                }

                _cachedAssetHeaders = null;

                IsDisposed = true;
            }
        }
    }
}
