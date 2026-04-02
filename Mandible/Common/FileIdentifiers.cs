using System;
using System.Collections.Generic;

namespace Mandible.Common;

/// <summary>
/// Maps file types to identifiers.
/// </summary>
public static class FileIdentifiers
{
    /// <summary>
    /// Magic bytes for various file types.
    /// </summary>
    public static IReadOnlyDictionary<FileType, ReadOnlyMemory<byte>> Magics { get; } = new Dictionary<FileType, ReadOnlyMemory<byte>>
    {
        { FileType.ActorDefinition, "<ActorRuntime>"u8.ToArray() },
        { FileType.CollisionData, "CDTA"u8.ToArray() },
        { FileType.DdsImage, "DDS"u8.ToArray() },
        { FileType.Dxbc, "DXBC"u8.ToArray() },
        { FileType.Eco, "*TEXTUREPART"u8.ToArray() },
        // Uncertain whether the dx11efb "magics" here aren't actually meaningful header data, but they work.
        { FileType.EfbDx11_Model4, new byte[] { 0x01, 0x00, 0x04, 0x01, 0, 0, 0, 0, 0x78, 0, 0, 0, 0, 0, 0, 0 } },
        { FileType.EfbDx11_Model5, new byte[] { 0x01, 0x00, 0x05, 0x01, 0, 0, 0, 0, 0x78, 0, 0, 0, 0, 0, 0, 0 } },
        { FileType.Elf, new byte[] { 0x7F, (byte)'E', (byte)'L', (byte)'F' } },
        { FileType.FmodSoundBank5, "FSB5"u8.ToArray() },
        { FileType.Fxd, "FXD "u8.ToArray() },
        { FileType.Fxo, new byte[] { 0x01, 0x09, 0xFF, 0xFE } },
        { FileType.Gfx, "CFX"u8.ToArray() },
        { FileType.Indr, "INDR"u8.ToArray() },
        { FileType.Jpeg, new byte[] { 0xff, 0xd8, 0xff } },
        { FileType.MaterialInfo, "DMAT"u8.ToArray() },
        { FileType.ModelInfo, "DMOD"u8.ToArray() },
        { FileType.MorphemeRuntimeNetwork, new byte[] { 0x18, 0, 0, 0, 0x06, 0, 0, 0 } },
        { FileType.MorphemeRuntimeNetwork64Bit, new byte[] { 0x1A, 0, 0, 0, 0x0A, 0, 0, 0 } },
        { FileType.Pack2, "PAK"u8.ToArray() },
        { FileType.Png, new byte[] { 0x89, (byte)'P', (byte)'N', (byte)'G' } },
        { FileType.Riff, "RIFF"u8.ToArray() },
        { FileType.TerrainChunkLod0, "CNK0"u8.ToArray() },
        { FileType.TerrainChunkLod1, "CNK1"u8.ToArray() },
        { FileType.TerrainChunkLod2, "CNK2"u8.ToArray() },
        { FileType.TerrainChunkLod3, "CNK3"u8.ToArray() },
        { FileType.Tome, new byte[] { 0x14, 0x00, 0x00, 0xD6 } },
        { FileType.TruevisionTga, "TRUEVISION-XFILE.\0"u8.ToArray() },
        { FileType.Vnfo, "VNFO"u8.ToArray() },
        { FileType.Zone, "ZONE"u8.ToArray() }
    };

    /// <summary>
    /// Filesystem extensions, excluding the period, for various file types.
    /// </summary>
    public static IReadOnlyDictionary<FileType, string> Extensions { get; } = new Dictionary<FileType, string>
    {
        { FileType.ActorDefinition, "adr" },
        { FileType.CollisionData, "cdt" },
        { FileType.DdsImage, "dds" },
        { FileType.Dxbc, "cso" },
        { FileType.Eco, "eco" },
        { FileType.Efb, "efb" },
        { FileType.EfbDx11_Model4, "dx11efb" },
        { FileType.EfbDx11_Model5, "dx11efb" },
        { FileType.Elf, "elf" },
        { FileType.FmodSoundBank5, "fsb" },
        { FileType.Fxd, "fxd" },
        { FileType.Fxo, "fxo" },
        { FileType.Gfx, "gfx" },
        { FileType.Indr, "indr" },
        { FileType.Jpeg, "jpg" },
        { FileType.MaterialInfo, "dma" },
        { FileType.ModelInfo, "dme" },
        { FileType.MorphemeRuntimeNetwork, "mrn" },
        { FileType.MorphemeRuntimeNetwork64Bit, "mrn" },
        { FileType.Pack1, "pack" },
        { FileType.Pack2, "pack2" },
        { FileType.Png, "png" },
        { FileType.TerrainChunkLod0, "cnk0" },
        { FileType.TerrainChunkLod1, "cnk1" },
        { FileType.TerrainChunkLod2, "cnk2" },
        { FileType.TerrainChunkLod3, "cnk3" },
        { FileType.Tome, "tome" },
        { FileType.TruevisionTga, "tga" },
        { FileType.Vnfo, "vnfo" },
        { FileType.Zone, "zone" }
    };

    /// <summary>
    /// Infers the type of the given <paramref name="data"/> by searching for magic bytes.
    /// </summary>
    /// <param name="data">
    /// The data to inspect. Include the whole file, as some magic identifiers are placed at the end of the data.
    /// </param>
    /// <returns>The type of the data, or <see cref="FileType.Unknown"/> if the type could not be inferred.</returns>
    public static FileType InferFileType(ReadOnlySpan<byte> data)
    {
        foreach ((FileType type, ReadOnlyMemory<byte> magic) in Magics)
        {
            switch (type)
            {
                case FileType.TruevisionTga:
                    if (data.EndsWith(Magics[type].Span))
                        return type;
                    break;
                case FileType.Fxd:
                    if (data.Length > 11 && data[8..].StartsWith(Magics[type].Span))
                        return type;
                    break;
                default:
                    if (data.StartsWith(magic.Span))
                        return type;
                    break;
            }
        }

        return FileType.Unknown;
    }
}
