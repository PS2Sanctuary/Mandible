using System.Buffers;
using System.Buffers.Binary;
using System.IO.Compression;

namespace Mandible.Pack2
{
    /// <summary>
    /// Provides methods to read pack2 data.
    /// </summary>
    public class Pack2StreamReader : IDisposable
    {
        /// <summary>
        /// Gets a rough value for the average asset size. Useful for choosing buffer sizes.
        /// </summary>
        public const int AVERAGE_ASSET_SIZE = 76000;

        /// <summary>
        /// The indicator placed in front of an asset data block to indicate that it has been compressed.
        /// </summary>
        protected const uint ASSET_COMPRESSION_INDICATOR = 2712847316;

        protected readonly Stream _baseStream;
        protected readonly long _baseStreamPositionOffset;
        protected readonly ArrayPool<byte> _arrayPool;
        protected readonly bool _disposeBase;

        protected Pack2Header? _cachedHeader;
        protected IReadOnlyList<Asset2Header>? _cachedAssetHeaders;

        /// <summary>
        /// Gets or sets a value indicating whether or not this instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; protected set; }

        /// <summary>
        /// Initialises a new instance of the <see cref="Pack2StreamReader"/> class.
        /// </summary>
        /// <param name="packFilePath">The path to a pack file.</param>
        /// <param name="useAsync">Sets a value indicating whether you will use the async operations of this <see cref="Pack2StreamReader"/>.</param>
        public Pack2StreamReader(string packFilePath, bool useAsync)
            : this(new FileStream(packFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, AVERAGE_ASSET_SIZE, useAsync))
        {
        }

        /// <summary>
        /// Initialises a new instance of the <see cref="Pack2StreamReader"/> class.
        /// </summary>
        /// <param name="baseStream">The stream to read the pack2 data from. The reader assumes that the current position of the stream is at the start of the pack data.</param>
        /// <param name="disposeBase">Sets a value indicating whether the base stream should be disposed when <see cref="Dispose"/> is called.</param>
        public Pack2StreamReader(Stream baseStream, bool disposeBase = false)
        {
            _arrayPool = ArrayPool<byte>.Shared;
            _baseStream = baseStream;
            _baseStreamPositionOffset = baseStream.Position;
            _disposeBase = disposeBase;
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

            _baseStream.Seek(_baseStreamPositionOffset, SeekOrigin.Begin);
            await _baseStream.ReadAsync(headerBuffer, ct).ConfigureAwait(false);

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

            _baseStream.Seek(_baseStreamPositionOffset, SeekOrigin.Begin);
            _baseStream.Read(headerBuffer);

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

            _baseStream.Seek(_baseStreamPositionOffset + (long)header.AssetMapOffset, SeekOrigin.Begin);
            await _baseStream.ReadAsync(assetHeadersBuffer, ct).ConfigureAwait(false);

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

            _baseStream.Seek(_baseStreamPositionOffset + (long)header.AssetMapOffset, SeekOrigin.Begin);
            _baseStream.Read(assetHeadersBuffer);

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
            _baseStream.Seek(_baseStreamPositionOffset + (long)assetHeader.DataOffset, SeekOrigin.Begin);

            if (assetHeader.ZipStatus == AssetZipDefinition.Zipped || assetHeader.ZipStatus == AssetZipDefinition.ZippedAlternate)
            {
                Memory<byte> compressionInfoData = new byte[8];
                await _baseStream.ReadAsync(compressionInfoData, ct).ConfigureAwait(false);

                uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(compressionInfoData[0..4].Span);
                uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(compressionInfoData[4..8].Span);

                if (compressionIndicator != ASSET_COMPRESSION_INDICATOR)
                    throw new InvalidDataException("The asset header indicated that this asset was compressed, but no compression indicator was found in the asset data.");

                Memory<byte> data = new byte[decompressedLength];
                using ZLibStream inflaterStream = new(_baseStream, CompressionMode.Decompress, true);
                await inflaterStream.ReadAsync(data, ct).ConfigureAwait(false);

                return data;
            }
            else if (assetHeader.ZipStatus == AssetZipDefinition.Unzipped || assetHeader.ZipStatus == AssetZipDefinition.UnzippedAlternate)
            {
                Memory<byte> data = new byte[assetHeader.DataSize];
                await _baseStream.ReadAsync(data, ct).ConfigureAwait(false);

                return data;
            }
            else
            {
                throw new ArgumentException("Asset header was invalid.", nameof(assetHeader));
            }
        }

        /// <summary>
        /// Gets a stream to retrieve the unzipped asset data. Note that the asset data is stored and returned in big endian format.
        /// </summary>
        /// <param name="assetHeader">The asset to retrieve.</param>
        /// <returns>A stream of the asset data.</returns>
        public virtual ReadOnlySpan<byte> ReadAssetData(Asset2Header assetHeader)
        {
            _baseStream.Seek(_baseStreamPositionOffset + (long)assetHeader.DataOffset, SeekOrigin.Begin);

            if (assetHeader.ZipStatus == AssetZipDefinition.Zipped || assetHeader.ZipStatus == AssetZipDefinition.ZippedAlternate)
            {
                Span<byte> compressionInfoData = new byte[8];
                _baseStream.Read(compressionInfoData);

                uint compressionIndicator = BinaryPrimitives.ReadUInt32BigEndian(compressionInfoData[0..4]);
                uint decompressedLength = BinaryPrimitives.ReadUInt32BigEndian(compressionInfoData[4..8]);

                if (compressionIndicator != ASSET_COMPRESSION_INDICATOR)
                    throw new InvalidDataException("The asset header indicated that this asset was compressed, but no compression indicator was found in the asset data.");

                Span<byte> data = new byte[decompressedLength];
                using ZLibStream inflaterStream = new(_baseStream, CompressionMode.Decompress, true);
                inflaterStream.Read(data);

                return data;
            }
            else if (assetHeader.ZipStatus == AssetZipDefinition.Unzipped || assetHeader.ZipStatus == AssetZipDefinition.UnzippedAlternate)
            {
                Span<byte> data = new byte[assetHeader.DataSize];
                _baseStream.Read(data);

                return data;
            }
            else
            {
                throw new ArgumentException("Asset header was invalid.", nameof(assetHeader));
            }
        }

        #region IDisposable

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (_disposeBase)
                        _baseStream.Dispose();
                }

                _cachedAssetHeaders = null;

                IsDisposed = true;
            }
        }

        #endregion
    }
}
