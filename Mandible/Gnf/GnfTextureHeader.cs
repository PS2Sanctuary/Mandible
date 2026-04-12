using BinaryPrimitiveHelpers;
using Mandible.Abstractions;

namespace Mandible.Gnf;

public readonly record struct GnfTextureHeader
(
) : IBinarySerializable<GnfTextureHeader>
{
    // Texture headers are designed to be stored in seven registers.
    public const int SIZE = 7 * sizeof(uint);

    public static GnfTextureHeader Deserialize(ref BinaryPrimitiveReader reader)
        => throw new System.NotImplementedException();

    public int GetSerializedSize()
        => SIZE;

    public void Serialize(ref BinaryPrimitiveWriter writer)
        => throw new System.NotImplementedException();
}
