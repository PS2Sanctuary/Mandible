using BinaryPrimitiveHelpers;
using Mandible.Abstractions;
using Mandible.Exceptions;
using System.Collections.Generic;
using System.Linq;

namespace Mandible.Dma;

/// <summary>
/// Represents a material definition of the <see cref="Dmat"/> class.
/// </summary>
/// <param name="NameHash">A value that is assumed to be a hash of the material's name.</param>
/// <param name="MaterialDefinitionHash">The hashed name of the material definition as defined in <c>materials_3.xml</c>.</param>
/// <param name="Parameters">The material's parameters.</param>
public record Material
(
    uint NameHash,
    uint MaterialDefinitionHash,
    IReadOnlyList<MaterialParameter> Parameters
) : IBinarySerializable<Material>
{
    public const int MINIMUM_SIZE = sizeof(uint) // NameHash
        + sizeof(uint) // DataLen
        + sizeof(uint) // MaterialDefinitionHash
        + sizeof(uint); // ParameterCount

    /// <inheritdoc />
    public static Material Deserialize(BinaryPrimitiveReader reader)
    {
        InvalidBufferSizeException.ThrowIfLessThan(MINIMUM_SIZE, reader.RemainingLength);

        uint nameHash = reader.ReadUInt32LE();
        reader.Seek(sizeof(uint)); // Skip the data length field
        uint materialDefinitionHash = reader.ReadUInt32LE();
        uint parameterCount = reader.ReadUInt32LE();

        List<MaterialParameter> parameters = [];
        for (int i = 0; i < parameterCount; i++)
        {
            MaterialParameter parameter = MaterialParameter.Deserialize(reader);
            parameters.Add(parameter);
        }

        return new Material
        (
            nameHash,
            materialDefinitionHash,
            parameters
        );
    }

    /// <inheritdoc />
    public int GetSerializedSize()
        => MINIMUM_SIZE + Parameters.Sum(p => p.GetSerializedSize());

    /// <inheritdoc />
    public void Serialize(BinaryPrimitiveWriter writer)
    {
        int requiredBufferSize = GetSerializedSize();
        InvalidBufferSizeException.ThrowIfLessThan(requiredBufferSize, writer.RemainingLength);

        writer.WriteUInt32LE(NameHash);
        writer.WriteUInt32LE((uint)requiredBufferSize - sizeof(uint) - sizeof(uint));
        writer.WriteUInt32LE(MaterialDefinitionHash);
        writer.WriteUInt32LE((uint)Parameters.Count);

        foreach (MaterialParameter param in Parameters)
            param.Serialize(writer);
    }
}
