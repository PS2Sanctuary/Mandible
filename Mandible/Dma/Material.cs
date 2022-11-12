using Mandible.Abstractions;
using Mandible.Exceptions;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;

namespace Mandible.Dma;

/*
struct material
{
    unsigned int name_hash;
    unsigned int data_length;
    unsigned int material_definition_hash;
    unsigned int parameter_count;
    material_parameter parameters[parameter_count];
};
*/

/// <summary>
/// Represents a material definition of the <see cref="Dmat"/> class.
/// </summary>
public class Material : IBufferWritable
{
    /// <summary>
    /// Gets a value that is assumed to be a hash of the material's name.
    /// </summary>
    public uint NameHash { get; }

    /// <summary>
    /// Gets the hashed name of the material definition as defined in <c>materials_3.xml</c>.
    /// </summary>
    public uint MaterialDefinitionHash { get; }

    /// <summary>
    /// Gets the material parameters defined by this material.
    /// </summary>
    public IReadOnlyList<MaterialParameter> Parameters { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Material"/> class.
    /// </summary>
    /// <param name="nameHash">The presumed hash of the material's name.</param>
    /// <param name="materialDefinitionHash">The hashed name of the material definition.</param>
    /// <param name="parameters">The material's parameters.</param>
    public Material
    (
        uint nameHash,
        uint materialDefinitionHash,
        IReadOnlyList<MaterialParameter> parameters
    )
    {
        NameHash = nameHash;
        MaterialDefinitionHash = materialDefinitionHash;
        Parameters = parameters;
    }

    /// <summary>
    /// Reads a <see cref="Material"/> instance from a buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="amountRead">The amount of data read from the <paramref name="buffer"/>.</param>
    /// <returns>A <see cref="Material"/> instance.</returns>
    public static Material Read(ReadOnlySpan<byte> buffer, out int amountRead)
    {
        int offset = 0;

        uint nameHash = BinaryPrimitives.ReadUInt32LittleEndian(buffer[offset..]);
        offset += sizeof(uint);

        uint materialDefinitionHash = BinaryPrimitives.ReadUInt32LittleEndian(buffer[offset..]);
        offset += sizeof(uint);

        uint parameterCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer[offset..]);
        offset += sizeof(uint);

        List<MaterialParameter> parameters = new();
        for (int i = 0; i < parameterCount; i++)
        {
            MaterialParameter parameter = MaterialParameter.Read(buffer[offset..], out int paramAmountRead);
            parameters.Add(parameter);
            offset += paramAmountRead;
        }

        amountRead = offset;
        return new Material
        (
            nameHash,
            materialDefinitionHash,
            parameters
        );
    }

    /// <inheritdoc />
    public int GetRequiredBufferSize()
        => sizeof(uint) // NameHash
           + sizeof(uint) // RequiredBufferSize
           + sizeof(uint) // MaterialDefinitionHash
           + sizeof(uint) // ParameterCount
           + Parameters.Sum(p => p.GetRequiredBufferSize());

    /// <inheritdoc />
    public int Write(Span<byte> buffer)
    {
        int requiredBufferSize = GetRequiredBufferSize();
        if (buffer.Length < requiredBufferSize)
            throw new InvalidBufferSizeException(requiredBufferSize, buffer.Length);

        BinaryPrimitives.WriteUInt32LittleEndian(buffer, NameHash);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[4..], (uint)requiredBufferSize);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[8..], MaterialDefinitionHash);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[12..], (uint)Parameters.Count);

        int offset = 16;
        foreach (MaterialParameter param in Parameters)
            offset += param.Write(buffer[offset..]);

        return requiredBufferSize;
    }
}
