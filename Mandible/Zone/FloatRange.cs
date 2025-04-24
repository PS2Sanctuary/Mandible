using BinaryPrimitiveHelpers;
using Mandible.Exceptions;

namespace Mandible.Zone;

/// <summary>
/// Represents a 32-bit floating-point range.
/// </summary>
/// <param name="Min">The minimum value of the range.</param>
/// <param name="Max">The maximum value of the range.</param>
public record struct FloatRange(float Min, float Max)
{
    /// <summary>
    /// Gets the serialized size of a <see cref="FloatRange"/> object.
    /// </summary>
    public const int Size = sizeof(float) * 2;

    /// <summary>
    /// Reads a <see cref="FloatRange"/> instance from a <see cref="BinaryReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>The deserialized <see cref="FloatRange"/>.</returns>
    public static FloatRange Read(ref BinaryReader reader)
    {
        float min = reader.ReadSingleLE();
        float max = reader.ReadSingleLE();

        return new FloatRange(min, max);
    }

    /// <summary>
    /// Writes this <see cref="FloatRange"/> to a <see cref="BinaryWriter"/>.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <exception cref="InvalidBufferSizeException">
    /// Thrown if the writer does not have enough remaining space.
    /// </exception>
    public void Write(ref BinaryWriter writer)
    {
        if (Size > writer.RemainingLength)
            throw new InvalidBufferSizeException(Size, writer.RemainingLength);

        writer.WriteSingleLE(Min);
        writer.WriteSingleLE(Max);
    }
}
