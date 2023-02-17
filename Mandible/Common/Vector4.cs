using Mandible.Exceptions;
using Mandible.Util;

namespace Mandible.Common;

/// <summary>
/// Represents a vector with four floating-point components.
/// </summary>
/// <param name="X">The X component.</param>
/// <param name="Y">The Y component.</param>
/// <param name="Z">The Z component.</param>
/// <param name="W">The W component.</param>
public record struct Vector4(float X, float Y, float Z, float W)
{
    /// <summary>
    /// Gets the size consumed by a <see cref="Vector4"/> when serialized.
    /// </summary>
    public const int Size = sizeof(float) * 4;

    /// <summary>
    /// Reads a <see cref="Vector4"/> instance from a <see cref="BinaryReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>A <see cref="Vector4"/> instance.</returns>
    public static Vector4 Read(ref BinaryReader reader)
    {
        float x = reader.ReadSingleLE();
        float y = reader.ReadSingleLE();
        float z = reader.ReadSingleLE();
        float w = reader.ReadSingleLE();

        return new Vector4(x, y, z, w);
    }

    /// <summary>
    /// Writes this <see cref="Vector4"/> instance to a <see cref="BinaryWriter"/>.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <exception cref="InvalidBufferSizeException">
    /// Thrown if the writer does not have enough remaining space.
    /// </exception>
    public void Write(ref BinaryWriter writer)
    {
        if (writer.Remaining < Size)
            throw new InvalidBufferSizeException(Size, writer.Remaining);

        writer.WriteSingleLE(X);
        writer.WriteSingleLE(Y);
        writer.WriteSingleLE(Z);
        writer.WriteSingleLE(W);
    }
}
