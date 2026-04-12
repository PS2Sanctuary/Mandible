using BinaryPrimitiveHelpers;
using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Abstractions.Services;
using Mandible.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Gnf;

/// <summary>
/// Manipulates the GNF texture image container.
/// </summary>
public class GnfImage
{
    /// <summary>
    /// Gets the magic identifier of a zone file.
    /// </summary>
    public static readonly ReadOnlyMemory<byte> MAGIC = FileIdentifiers.Magics[FileType.Gnf];

    private readonly IDataReaderService _reader;

    /// <summary>
    /// Custom user data.
    /// </summary>
    public ReadOnlyMemory<byte>? UserData { get; set; }

    /// <summary>
    /// Gets the offset into the file at which the texture data begins.
    /// </summary>
    public int TextureDataOffset { get; private set; }

    /// <summary>
    /// Gets the list of textures.
    /// </summary>
    public IReadOnlyList<GnfTextureHeader> Textures { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="GnfImage"/> class.
    /// </summary>
    /// <param name="dataReader">The data reader to load the image data from.</param>
    public GnfImage(IDataReaderService dataReader)
    {
        _reader = dataReader;
        LoadFromReader();
    }

    /// <summary>
    /// Reads a texture's data.
    /// </summary>
    /// <param name="textureIndex">The index of the texture header in <see cref="Textures"/>.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    /// <returns>The texture data.</returns>
    public async ValueTask<MemoryOwner<byte>> ReadTextureData(int textureIndex, CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(textureIndex, Textures.Count);
        GnfTextureHeader header = Textures[textureIndex];

        MemoryOwner<byte> buffer = MemoryOwner<byte>.Allocate((int)header.TextureSize);
        int address = (int)header.BaseAddress + TextureDataOffset;
        await _reader.ReadAsync(buffer.Memory, address, ct);

        return buffer;
    }

    [MemberNotNull(nameof(Textures))]
    private void LoadFromReader()
    {
        Span<byte> headerBuffer = stackalloc byte[GnfHeader.SIZE];
        _reader.Read(headerBuffer, 0);
        BinaryPrimitiveReader reader = new(headerBuffer);
        GnfHeader header = GnfHeader.Deserialize(ref reader);
        TextureDataOffset = GnfHeader.SIZE + (int)header.ContentsSize;

        byte[] contentsBuffer = new byte[(int)header.ContentsSize];
        _reader.Read(contentsBuffer, GnfHeader.SIZE);
        reader = new BinaryPrimitiveReader(contentsBuffer);
        GnfContents contents = GnfContents.Deserialize(ref reader);
        Textures = contents.Textures;

        if (!reader.IsAtEnd && reader.Remaining.StartsWith("USER"u8))
            UserData = GnfUserData.Deserialize(ref reader).Data;
    }
}
