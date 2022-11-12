using Mandible.Abstractions;
using Mandible.Exceptions;
using System;
using System.Buffers.Binary;

namespace Mandible.Dma;

/*
struct material_parameter
{
    unsigned int semantic_hash;
    unsigned int d3dxparameter_class;
    unsigned int d3dxparameter_type;
    unsigned int data_length;
    byte data[data_length];
};
*/

/// <summary>
/// Represents a material parameter of the <see cref="Material"/> class.
/// </summary>
public class MaterialParameter : IBufferWritable
{
    /// <summary>
    /// Gets the case-sensitive Jenkins hash of the parameter's semantic.
    /// </summary>
    public uint SemanticHash { get; }

    /// <summary>
    /// Gets the <a href="https://docs.microsoft.com/windows/win32/direct3d9/d3dxparameter-class">D3DXPARAMETER_CLASS</a>
    /// of the parameter.
    /// </summary>
    public uint D3DXParameterClass { get; }

    /// <summary>
    /// Gets the <a href="https://docs.microsoft.com/windows/win32/direct3d9/d3dxparameter-type">D3DXPARAMETER_TYPE</a>
    /// of the parameter.
    /// </summary>
    public uint D3DXParameterType { get; }

    /// <summary>
    /// Gets the parameter data.
    /// </summary>
    /// <remarks>
    /// For texture parameters, this is a <c>uint</c> containing an upper-case Jenkins hash
    /// of a texture name stored in the parent <see cref="Dmat.TextureFileNames"/> list,
    /// or <c>0</c> to indicate no texture.
    /// </remarks>
    public ReadOnlyMemory<byte> Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MaterialParameter"/> class.
    /// </summary>
    /// <param name="semanticHash">The case-sensitive Jenkins hash of the parameter's semantic.</param>
    /// <param name="d3DxParameterClass">The D3DX parameter class.</param>
    /// <param name="d3DxParameterType">The D3DX parameter type.</param>
    /// <param name="data">The parameter data.</param>
    public MaterialParameter
    (
        uint semanticHash,
        uint d3DxParameterClass,
        uint d3DxParameterType,
        ReadOnlyMemory<byte> data
    )
    {
        SemanticHash = semanticHash;
        D3DXParameterClass = d3DxParameterClass;
        D3DXParameterType = d3DxParameterType;
        Data = data;
    }

    /// <summary>
    /// Reads a <see cref="MaterialParameter"/> instance from a buffer.
    /// </summary>
    /// <param name="buffer">The buffer.</param>
    /// <param name="amountRead">The amount of data read from the <paramref name="buffer"/>.</param>
    /// <returns>A <see cref="MaterialParameter"/> instance.</returns>
    public static MaterialParameter Read(ReadOnlySpan<byte> buffer, out int amountRead)
    {
        uint semanticHash = BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        uint d3Class = BinaryPrimitives.ReadUInt32LittleEndian(buffer[4..]);
        uint d3Type = BinaryPrimitives.ReadUInt32LittleEndian(buffer[8..]);
        uint dataLength = BinaryPrimitives.ReadUInt32LittleEndian(buffer[12..]);
        ReadOnlyMemory<byte> data = buffer.Slice(16, (int)dataLength).ToArray();

        amountRead = 16 + (int)dataLength;
        return new MaterialParameter
        (
            semanticHash,
            d3Class,
            d3Type,
            data
        );
    }

    /// <inheritdoc />
    public int GetRequiredBufferSize()
        => sizeof(uint) // SemanticHash
           + sizeof(uint) // D3DXClass
           + sizeof(uint) // D3DXType
           + sizeof(uint) // DataLength
           + Data.Length;

    /// <inheritdoc />
    public int Write(Span<byte> buffer)
    {
        int requiredBufferSize = GetRequiredBufferSize();
        if (buffer.Length < requiredBufferSize)
            throw new InvalidBufferSizeException(requiredBufferSize, buffer.Length);

        BinaryPrimitives.WriteUInt32LittleEndian(buffer, SemanticHash);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[4..], D3DXParameterClass);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[8..], D3DXParameterType);
        BinaryPrimitives.WriteUInt32LittleEndian(buffer[12..], (uint)Data.Length);
        Data.Span.CopyTo(buffer[16..]);

        return requiredBufferSize;
    }
}
