using BinaryPrimitiveHelpers;
using Mandible.Abstractions.Services;
using Mandible.Common;
using System;

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
    /// Initializes a new instance of the <see cref="GnfImage"/> class.
    /// </summary>
    /// <param name="dataReader">The data reader to load the image data from.</param>
    public GnfImage(IDataReaderService dataReader)
    {
        _reader = dataReader;
        LoadFromReader();
    }

    private void LoadFromReader()
    {
        Span<byte> headerBuffer = stackalloc byte[GnfHeader.SIZE];
        _reader.Read(headerBuffer, 0);
        BinaryPrimitiveReader reader = new(headerBuffer);
        GnfHeader header = GnfHeader.Deserialize(ref reader);

        byte[] contentsBuffer = new byte[(int)header.ContentsSize];
        _reader.Read(contentsBuffer, GnfHeader.SIZE);
        reader = new BinaryPrimitiveReader(contentsBuffer);
        GnfContents contents = GnfContents.Deserialize(ref reader);
    }
}
