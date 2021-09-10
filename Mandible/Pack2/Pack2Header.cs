using System.Buffers.Binary;

namespace Mandible.Pack2
{
    /// <summary>
    /// Header information of a pack2 file.
    /// </summary>
    public readonly struct Pack2Header
    {
        /// <summary>
        /// Gets the size in bytes of the pack2 header.
        /// </summary>
        public const int SIZE = 160;

        /// <summary>
        /// Gets the magic string of the pack2 file type.
        /// </summary>
        public readonly string Magic;

        /// <summary>
        /// Gets the version of the pack. Often combined with the <see cref="Magic"/> value in other pack implementations.
        /// </summary>
        public readonly byte Version;

        /// <summary>
        /// Gets the number of assets stored in the pack.
        /// </summary>
        public readonly uint AssetCount;

        /// <summary>
        /// Gets the length in bytes of the pack.
        /// </summary>
        public readonly ulong Length;

        /// <summary>
        /// Gets the offset into the pack of the asset map, in bytes.
        /// </summary>
        public readonly ulong AssetMapOffset;

        /// <summary>
        /// An unknown value that is always set to 256.
        /// </summary>
        public readonly ulong Unknown;

        /// <summary>
        /// An assumed checksum. How it is calculated is unknown.
        /// </summary>
        public readonly byte[] Checksum;

        /// <summary>
        /// Initialises a new instance of the <see cref="Pack2Header"/> struct.
        /// </summary>
        /// <param name="assetCount">The number of assets stored in the pack.</param>
        /// <param name="length">The length in bytes of the pack.</param>
        /// <param name="assetMapOffset">The offset into the pack of the asset map, in bytes.</param>
        /// <param name="checkSum">The pack checksum.</param>
        /// <param name="version">The version of the pack.</param>
        /// <param name="unknown">The unknown value.</param>
        public Pack2Header(uint assetCount, ulong length, ulong assetMapOffset, byte[] checkSum, byte version = 1, ulong unknown = 256)
        {
            Magic = "PAK";
            Version = version;
            AssetCount = assetCount;
            Length = length;
            AssetMapOffset = assetMapOffset;
            Unknown = unknown;
            Checksum = checkSum;
        }

        public ReadOnlySpan<byte> Serialise()
        {
            byte[] data = new byte[32 + Checksum.Length];
            Span<byte> dataSpan = data.AsSpan();

            // Write the magic bytes
            data[0] = 0x50;
            data[1] = 0x41;
            data[2] = 0x4b;
            data[3] = Version;

            BinaryPrimitives.WriteUInt32LittleEndian(dataSpan[4..8], AssetCount);
            BinaryPrimitives.WriteUInt64LittleEndian(dataSpan[8..16], Length);
            BinaryPrimitives.WriteUInt64LittleEndian(dataSpan[16..24], AssetMapOffset);
            BinaryPrimitives.WriteUInt64LittleEndian(dataSpan[24..32], Unknown);

            Buffer.BlockCopy(Checksum, 0, data, 32, Checksum.Length);

            return dataSpan;
        }

        public static Pack2Header Deserialise(ReadOnlySpan<byte> data)
        {
            byte version = data[3];
            uint assetCount = BinaryPrimitives.ReadUInt32LittleEndian(data[4..8]);
            ulong length = BinaryPrimitives.ReadUInt64LittleEndian(data[8..16]);
            ulong assetMapOffset = BinaryPrimitives.ReadUInt64LittleEndian(data[16..24]);
            ulong unknown = BinaryPrimitives.ReadUInt64LittleEndian(data[24..32]);

            return new Pack2Header(assetCount, length, assetMapOffset, data[32..].ToArray(), version, unknown);
        }
    }
}
