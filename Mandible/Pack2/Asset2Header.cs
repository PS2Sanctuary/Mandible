using System.Buffers.Binary;

namespace Mandible.Pack2
{
    public readonly struct Asset2Header
    {
        /// <summary>
        /// Gets the size in bytes of the asset header.
        /// </summary>
        public const int SIZE = 32;

        /// <summary>
        /// Gets the CRC-64 hash of the uppercase file name.
        /// </summary>
        public readonly ulong NameHash;

        /// <summary>
        /// Gets the offset into the pack of the asset data, in bytes.
        /// </summary>
        public readonly ulong DataOffset;

        /// <summary>
        /// Gets the size in bytes of the packed asset data.
        /// </summary>
        public readonly ulong DataSize;

        /// <summary>
        /// Gets a value indicating whether the packed asset has been compressed.
        /// </summary>
        public readonly AssetZipDefinition ZipStatus;

        /// <summary>
        /// Gets the CRC-32 hash of the asset data.
        /// </summary>
        public readonly uint DataHash;

        /// <summary>
        /// Initialises a new instance of the <see cref="Asset2Header"/> struct.
        /// </summary>
        /// <param name="nameHash">The CRC-64 hash of the uppercase file name.</param>
        /// <param name="dataOffset">The offset into the pack of the asset data, in bytes.</param>
        /// <param name="dataSize">The size in bytes of the packed asset data.</param>
        /// <param name="isZipped">A value indicating whether the packet asset has been compressed.</param>
        /// <param name="dataHash">The CRC-32 hash of the asset data.</param>
        public Asset2Header(ulong nameHash, ulong dataOffset, ulong dataSize, AssetZipDefinition isZipped, uint dataHash)
        {
            NameHash = nameHash;
            DataOffset = dataOffset;
            DataSize = dataSize;
            ZipStatus = isZipped;
            DataHash = dataHash;
        }

        public ReadOnlySpan<byte> Serialise()
        {
            Span<byte> dataSpan = new(new byte[32]);

            BinaryPrimitives.WriteUInt64LittleEndian(dataSpan[0..8], NameHash);
            BinaryPrimitives.WriteUInt64LittleEndian(dataSpan[8..16], DataOffset);
            BinaryPrimitives.WriteUInt64LittleEndian(dataSpan[16..24], DataSize);
            BinaryPrimitives.WriteUInt32LittleEndian(dataSpan[24..28], (uint)ZipStatus);
            BinaryPrimitives.WriteUInt32LittleEndian(dataSpan[28..32], DataHash);

            return dataSpan;
        }

        public static Asset2Header Deserialise(ReadOnlySpan<byte> data)
        {
            ulong nameHash = BinaryPrimitives.ReadUInt64LittleEndian(data[0..8]);
            ulong dataOffset = BinaryPrimitives.ReadUInt64LittleEndian(data[8..16]);
            ulong dataSize = BinaryPrimitives.ReadUInt64LittleEndian(data[16..24]);
            AssetZipDefinition isZipped = (AssetZipDefinition)BinaryPrimitives.ReadUInt32LittleEndian(data[24..28]);
            uint dataHash = BinaryPrimitives.ReadUInt32LittleEndian(data[28..32]);

            return new Asset2Header(nameHash, dataOffset, dataSize, isZipped, dataHash);
        }
    }
}
