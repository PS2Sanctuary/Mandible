using BinaryPrimitiveHelpers;
using Mandible.Abstractions;
using Mandible.Exceptions;
using System;

namespace Mandible.Dma;

#pragma warning disable CS1591
/// <summary>
/// Represents a
/// <a href="https://docs.microsoft.com/windows/win32/direct3d9/d3dxparameter-class">DirectX parameter class</a>.
/// </summary>
public enum D3DXParameterClass : uint
{
    Scalar = 0,
    Vector = 1,
    MatrixRows = 2,
    MatrixColumns = 3,
    Object = 4,
    Struct = 5
};

/// <summary>
/// Represents a
/// <a href="https://docs.microsoft.com/windows/win32/direct3d9/d3dxparameter-type">DirectX parameter type</a>.
/// </summary>
public enum D3DXParameterType : uint
{
    Void,
    Boolean,
    Integer,
    Float,
    String,
    Texture,
    Texture1D,
    Texture2D,
    Texture3D,
    TextureCube,
    Sampler,
    Sampler1D,
    Sampler2D,
    Sampler3D,
    SamplerCube,
    PixelShader,
    VertexShader,
    PixelFragment,
    VertexFragment,
    Unsupported
};
#pragma warning restore CS1591

/// <summary>
/// Represents a material parameter of the <see cref="Material"/> class.
/// </summary>
/// <param name="SemanticHash">The case-sensitive Jenkins hash of the parameter's semantic.</param>
/// <param name="D3DXParameterClass">The parameter's DirectX parameter class.</param>
/// <param name="D3DXParameterType">The parameter's DirectX parameter type.</param>
/// <param name="Data">
/// Gets the parameter data.
/// <remarks>
/// For texture parameters, this is a <c>uint</c> containing an upper-case Jenkins hash
/// of a texture name stored in the parent <see cref="Dmat.TextureFileNames"/> list,
/// or <c>0</c> to indicate no texture.
/// </remarks>
/// </param>
public record MaterialParameter
(
    uint SemanticHash,
    D3DXParameterClass D3DXParameterClass,
    D3DXParameterType D3DXParameterType,
    ReadOnlyMemory<byte> Data
) : IBinarySerializable<MaterialParameter>
{
    public const int MINIMUM_SIZE = sizeof(uint) // SemanticHash
        + sizeof(uint) // D3DXClass
        + sizeof(uint) // D3DXType
        + sizeof(uint); // DataLength

    /// <inheritdoc />
    public static MaterialParameter Deserialize(BinaryPrimitiveReader reader)
    {
        InvalidBufferSizeException.ThrowIfLessThan(MINIMUM_SIZE, reader.RemainingLength);

        uint semanticHash = reader.ReadUInt32LE();
        uint d3Class = reader.ReadUInt32LE();
        uint d3Type = reader.ReadUInt32LE();
        uint dataLength = reader.ReadUInt32LE();
        ReadOnlySpan<byte> data = reader.ReadBytes((int)dataLength);

        return new MaterialParameter
        (
            semanticHash,
            (D3DXParameterClass)d3Class,
            (D3DXParameterType)d3Type,
            data.ToArray()
        );
    }

    /// <inheritdoc />
    public int GetSerializedSize()
        => MINIMUM_SIZE + Data.Length;

    /// <inheritdoc />
    public void Serialize(BinaryPrimitiveWriter writer)
    {
        InvalidBufferSizeException.ThrowIfLessThan(GetSerializedSize(), writer.RemainingLength);

        writer.WriteUInt32LE(SemanticHash);
        writer.WriteUInt32LE((uint)D3DXParameterClass);
        writer.WriteUInt32LE((uint)D3DXParameterType);
        writer.WriteUInt32LE((uint)Data.Length);
        writer.WriteBytes(Data.Span);
    }
}
