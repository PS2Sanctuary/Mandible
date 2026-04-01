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
        { FileType.Eco, "*TEXTUREPART"u8.ToArray() },
        { FileType.Elf, new byte[] { 0x7F, (byte)'E', (byte)'L', (byte)'F' } },
        { FileType.FmodSoundBank5, "FSB5"u8.ToArray() },
        { FileType.Fxd, "FXD"u8.ToArray() },
        { FileType.Gfx, "CFX"u8.ToArray() },
        { FileType.Indr, "INDR"u8.ToArray() },
        { FileType.Jpeg, new byte[] { 0xff, 0xd8, 0xff } },
        { FileType.MaterialInfo, "DMAT"u8.ToArray() },
        { FileType.ModelInfo, "DMOD"u8.ToArray() },
        { FileType.MorphemeAnimation, new byte[] { 0x18, 0, 0, 0, 0x06, 0, 0, 0 } },
        { FileType.MorphemeAnimation64Bit, new byte[] { 0x1A, 0, 0, 0, 0x0A, 0, 0, 0 } },
        { FileType.Pack2, "PAK"u8.ToArray() },
        { FileType.Png, new byte[] { 0x89, (byte)'P', (byte)'N', (byte)'G' } },
        { FileType.Riff, "RIFF"u8.ToArray() },
        { FileType.TerrainChunkLod0, "CNK0"u8.ToArray() },
        { FileType.TerrainChunkLod1, "CNK1"u8.ToArray() },
        { FileType.TerrainChunkLod2, "CNK2"u8.ToArray() },
        { FileType.TerrainChunkLod3, "CNK3"u8.ToArray() },
        { FileType.TerrainChunkGeneric, "CNK"u8.ToArray() }, // Below more specific identifiers
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
        { FileType.Eco, "eco" },
        { FileType.Elf, "elf" },
        { FileType.FmodSoundBank5, "fsb" },
        { FileType.Fxd, "fxd" },
        { FileType.Gfx, "gfx" },
        { FileType.Indr, "indr" },
        { FileType.Jpeg, "jpg" },
        { FileType.MaterialInfo, "dma" },
        { FileType.ModelInfo, "dme" },
        { FileType.MorphemeAnimation, "mrn" },
        { FileType.MorphemeAnimation64Bit, "mrn" },
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
}
