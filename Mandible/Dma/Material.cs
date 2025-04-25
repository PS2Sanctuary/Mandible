using BinaryPrimitiveHelpers;
using Mandible.Abstractions;
using Mandible.Exceptions;
using System;
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
) : IBufferWritable
{
    /// <summary>
    /// Reads a <see cref="Material"/> instance from a buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="amountRead">The amount of data read from the <paramref name="buffer"/>.</param>
    /// <returns>A <see cref="Material"/> instance.</returns>
    public static Material Read(ReadOnlySpan<byte> buffer, out int amountRead)
    {
        BinaryPrimitiveReader reader = new(buffer);

        uint nameHash = reader.ReadUInt32LE();
        reader.Seek(sizeof(uint)); // Skip the data length field
        uint materialDefinitionHash = reader.ReadUInt32LE();
        uint parameterCount = reader.ReadUInt32LE();

        List<MaterialParameter> parameters = new();
        for (int i = 0; i < parameterCount; i++)
        {
            MaterialParameter parameter = MaterialParameter.Read(buffer[reader.Offset..], out int paramAmountRead);
            parameters.Add(parameter);
            reader.Seek(paramAmountRead);
        }

        amountRead = reader.Offset;
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
           + sizeof(uint) // DataLen
           + sizeof(uint) // MaterialDefinitionHash
           + sizeof(uint) // ParameterCount
           + Parameters.Sum(p => p.GetRequiredBufferSize());

    /// <inheritdoc />
    public int Write(Span<byte> buffer)
    {
        int requiredBufferSize = GetRequiredBufferSize();
        if (buffer.Length < requiredBufferSize)
            throw new InvalidBufferSizeException(requiredBufferSize, buffer.Length);

        BinaryPrimitiveWriter writer = new(buffer);
        writer.WriteUInt32LE(NameHash);
        writer.WriteUInt32LE((uint)requiredBufferSize - sizeof(uint) - sizeof(uint));
        writer.WriteUInt32LE(MaterialDefinitionHash);
        writer.WriteUInt32LE((uint)Parameters.Count);

        foreach (MaterialParameter param in Parameters)
        {
            int paramAmountWritten = param.Write(buffer[writer.Offset..]);
            writer.Seek(paramAmountWritten);
        }

        return writer.Offset;
    }
}
