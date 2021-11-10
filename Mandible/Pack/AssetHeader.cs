using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace Mandible.Pack
{
    /// <summary>
    /// Represents an asset header used in .pack files.
    /// </summary>
    public class AssetHeader
    {
        /// <summary>
        /// Gets the name of the asset.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the byte offset into the pack at which the asset data is stored.
        /// </summary>
        public uint AssetOffset { get; }

        /// <summary>
        /// Gets the length of the packed data.
        /// </summary>
        public uint DataLength { get; }

        /// <summary>
        /// Gets a CRC-32 checksum of the packed data.
        /// </summary>
        public uint Checksum { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetHeader"/> class.
        /// </summary>
        /// <param name="name">The name of the asset.</param>
        /// <param name="assetOffset">The byte offset into the pack at which the asset data is stored.</param>
        /// <param name="dataLength">The length of the packed data.</param>
        /// <param name="checksum">The CRC-32 checksum of the packed data.</param>
        public AssetHeader(string name, uint assetOffset, uint dataLength, uint checksum)
        {
            Name = name;
            AssetOffset = assetOffset;
            DataLength = dataLength;
            Checksum = checksum;
        }

        /// <summary>
        /// Gets the number of bytes that this <see cref="AssetHeader"/> will use when stored within a pack.
        /// </summary>
        /// <returns>The size in bytes of this <see cref="AssetHeader"/>.</returns>
        public int GetSize()
            => 16 + Name.Length;

        /// <summary>
        /// Gets the number of bytes that an <see cref="AssetHeader"/> will use when stored within a pack.
        /// </summary>
        /// <param name="name">The name of the asset.</param>
        /// <returns>The size in bytes of an <see cref="AssetHeader"/>.</returns>
        public static int GetSize(string name)
            => 16 + name.Length;

        /// <summary>
        /// Deserializes a buffer to a <see cref="AssetHeader"/> instance.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>An <see cref="AssetHeader"/>.</returns>
        public static unsafe AssetHeader Deserialize(ReadOnlySpan<byte> buffer)
        {
            int index = 0;

            uint nameLength = BinaryPrimitives.ReadUInt32BigEndian(buffer[index..(index += sizeof(uint))]);
            string name = string.Empty;

            fixed (byte* bufferPtr = buffer[index..])
            {
                name = Marshal.PtrToStringAnsi((IntPtr)bufferPtr, (int)nameLength);
            }

            index += (int)nameLength;
            uint assetOffset = BinaryPrimitives.ReadUInt32BigEndian(buffer[index..(index += sizeof(uint))]);
            uint dataLength = BinaryPrimitives.ReadUInt32BigEndian(buffer[index..(index += sizeof(uint))]);
            uint checksum = BinaryPrimitives.ReadUInt32BigEndian(buffer[index..(index += sizeof(uint))]);

            return new AssetHeader(name, assetOffset, dataLength, checksum);
        }

        /// <summary>
        /// Serializes this <see cref="PackChunkHeader"/> to a byte buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <exception cref="ArgumentException">Thrown if the buffer is too small.</exception>
        public unsafe void Serialize(Span<byte> buffer)
        {
            if (buffer.Length < GetSize())
                throw new ArgumentException($"Buffer must be at least {16 + Name.Length} bytes", nameof(buffer));

            int index = 0;

            BinaryPrimitives.WriteUInt32BigEndian(buffer[index..(index += sizeof(uint))], (uint)Name.Length);

            foreach (byte value in Name)
                buffer[index++] = value;

            BinaryPrimitives.WriteUInt32BigEndian(buffer[index..(index += sizeof(uint))], AssetOffset);
            BinaryPrimitives.WriteUInt32BigEndian(buffer[index..(index += sizeof(uint))], DataLength);
            BinaryPrimitives.WriteUInt32BigEndian(buffer[index..(index += sizeof(uint))], Checksum);
        }
    }
}
