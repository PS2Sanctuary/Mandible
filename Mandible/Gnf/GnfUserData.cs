using BinaryPrimitiveHelpers;
using Mandible.Abstractions;
using Mandible.Exceptions;
using System;

namespace Mandible.Gnf;

/// <summary>
/// Represents a GNF user data structure, which stores custom data in the file.
/// </summary>
/// <param name="Data">The user data.</param>
public readonly record struct GnfUserData(ReadOnlyMemory<byte> Data) : IBinarySerializable<GnfUserData>
{
    /// <summary>
    /// The magic bytes that indicate a user data structure.
    /// </summary>
    public static readonly ReadOnlyMemory<byte> MAGIC = "USER"u8.ToArray();

    /// <summary>
    /// Gets the size in bytes of a serialized GNF Header structure.
    /// </summary>
    public static readonly int MINIMUM_SIZE = MAGIC.Length
        + sizeof(uint); // DataSize

    /// <inheritdoc />
    public static GnfUserData Deserialize(ref BinaryPrimitiveReader reader)
    {
        InvalidBufferSizeException.ThrowIfLessThan(MINIMUM_SIZE, reader.RemainingLength);

        ReadOnlySpan<byte> magic = reader.ReadBytes(MAGIC.Length);
        UnrecognisedMagicException.ThrowIfNotAtStart(MAGIC.Span, magic);

        uint dataSize = reader.ReadUInt32LE();
        ReadOnlySpan<byte> data = reader.ReadBytes((int)dataSize);
        return new GnfUserData(data.ToArray());
    }

    /// <inheritdoc />
    public int GetSerializedSize()
        => MINIMUM_SIZE + Data.Length;

    /// <inheritdoc />
    public void Serialize(ref BinaryPrimitiveWriter writer)
    {
        InvalidBufferSizeException.ThrowIfLessThan(GetSerializedSize(), writer.RemainingLength);

        writer.WriteBytes(MAGIC.Span);
        writer.WriteUInt32LE((uint)Data.Length);
        writer.WriteBytes(Data.Span);
    }
}
