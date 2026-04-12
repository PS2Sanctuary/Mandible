using BinaryPrimitiveHelpers;

namespace Mandible.Abstractions;

/// <summary>
/// Represents an object that can be serialized in binary format.
/// </summary>
/// <typeparam name="T">The type of the object.</typeparam>
public interface IBinarySerializable<out T>
{
    /// <summary>
    /// Deserializes an instance of this object from the given buffer.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize.</typeparam>
    /// <param name="reader">A binary primitive reader wrapping the data buffer.</param>
    /// <returns>The deserialized object.</returns>
    static abstract T Deserialize(ref BinaryPrimitiveReader reader);

    /// <summary>
    /// Gets the size in bytes of this object when serialized.
    /// </summary>
    /// <returns>The size in bytes of this object when serialized.</returns>
    int GetSerializedSize();

    /// <summary>
    /// Serializes this object to the given buffer.
    /// </summary>
    /// <param name="writer">A binary primitive writer wrapping the data buffer.</param>
    void Serialize(ref BinaryPrimitiveWriter writer);
}
