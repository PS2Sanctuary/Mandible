#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using BinaryPrimitiveHelpers;
using Mandible.Exceptions;
using System;

namespace Mandible.Fsb;

/// <summary>
/// Enumerates the various modes of an FMOD file.
/// </summary>
[Flags]
public enum FmodMode : uint
{
    /// <summary>
    /// Default for all modes listed below. FMOD_LOOP_OFF, FMOD_2D, FMOD_3D_WORLDRELATIVE, FMOD_3D_INVERSEROLLOFF
    /// </summary>
    FMOD_DEFAULT = 0x00000000,

    /// <summary>
    /// For non looping sounds. (DEFAULT).  Overrides FMOD_LOOP_NORMAL / FMOD_LOOP_BIDI.
    /// </summary>
    FMOD_LOOP_OFF = 0x00000001,

    /// <summary>
    /// For forward looping samples.
    /// </summary>
    FMOD_LOOP_NORMAL = 0x00000002,

    /// <summary>
    /// For bidirectional looping sounds. (only works on software mixed static sounds).
    /// </summary>
    FMOD_LOOP_BIDI = 0x00000004,

    /// <summary>
    /// Ignores any 3d processing. (DEFAULT).
    /// </summary>
    FMOD_2D = 0x00000008,

    /// <summary>
    /// Makes the sound positionable in 3D.  Overrides FMOD_2D.
    /// </summary>
    FMOD_3D = 0x00000010,

    /// <summary>
    /// Decompress at runtime, streaming from the source provided (ie from disk).  Overrides FMOD_CREATESAMPLE and
    /// FMOD_CREATECOMPRESSEDSAMPLE. Note a stream can only be played once at a time due to a stream only having 1
    /// stream buffer and file handle.  Open multiple streams to have them play concurrently.
    /// </summary>
    FMOD_CREATESTREAM = 0x00000080,
    FMOD_CREATESAMPLE = 0x00000100,
    FMOD_CREATECOMPRESSEDSAMPLE = 0x00000200,
    FMOD_OPENUSER = 0x00000400,
    FMOD_OPENMEMORY = 0x00000800,
    FMOD_OPENMEMORY_POINT = 0x10000000,
    FMOD_OPENRAW = 0x00001000,
    FMOD_OPENONLY = 0x00002000,
    FMOD_ACCURATETIME = 0x00004000,
    FMOD_MPEGSEARCH = 0x00008000,
    FMOD_NONBLOCKING = 0x00010000,
    FMOD_UNIQUE = 0x00020000,
    FMOD_3D_HEADRELATIVE = 0x00040000,
    FMOD_3D_WORLDRELATIVE = 0x00080000,
    FMOD_3D_INVERSEROLLOFF = 0x00100000,
    FMOD_3D_LINEARROLLOFF = 0x00200000,
    FMOD_3D_LINEARSQUAREROLLOFF = 0x00400000,
    FMOD_3D_INVERSETAPEREDROLLOFF = 0x00800000,
    FMOD_3D_CUSTOMROLLOFF = 0x04000000,
    FMOD_3D_IGNOREGEOMETRY = 0x40000000,
    FMOD_IGNORETAGS = 0x02000000,
    FMOD_LOWMEM = 0x08000000,
    FMOD_VIRTUAL_PLAYFROMSTART = 0x80000000
}

/// <summary>
/// Represents the file header information from an FSB5 sound bank file.
/// </summary>
public class Fsb5Header
{
    public const int SIZE = 4 // Magic
        + sizeof(int) // Version
        + sizeof(int) // NumSamples
        + sizeof(int) // SampleHeaderLen
        + sizeof(int) // NameLen
        + sizeof(int) // DataLen
        + sizeof(uint) // Mode
        + 8 // Zero
        + 16 // Hash
        + 8; // Dummy

    /// <summary>
    /// Gets the magic identifier of a zone file.
    /// </summary>
    public static readonly ReadOnlyMemory<byte> MAGIC = "FSB5"u8.ToArray();

    /// <summary>
    /// The version of the FSB5 header.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// The number of audio samples in the FSB file.
    /// </summary>
    public int NumSamples { get; set; }

    /// <summary>
    /// The length in bytes of all the sample headers, including extended information.
    /// </summary>
    public int SampleHeaderLen { get; set; }

    /// <summary>
    /// The length in bytes of the name table.
    /// </summary>
    public int NameLen { get; set; }

    /// <summary>
    /// The total length in bytes of the compressed sample data.
    /// </summary>
    public int DataLen { get; set; }

    /// <summary>
    /// Flags that apply to all samples in the FSB5 file.
    /// </summary>
    public FmodMode Mode { get; set; }

    /// <summary>
    /// A zeroed, eight-byte buffer with a single 0x1 byte at the start. May be used to denote the endianness of the data?
    /// </summary>
    public ReadOnlyMemory<byte> Zero => throw new NotImplementedException();

    /// <summary>
    /// A 16-byte hash.
    /// </summary>
    public ReadOnlyMemory<byte> Hash => throw new NotImplementedException();

    /// <summary>
    /// An eight-byte dummy value.
    /// </summary>
    public ReadOnlyMemory<byte> Dummy => throw new NotImplementedException();

    public Fsb5Header
    (
        int version,
        int numSamples,
        int sampleHeaderLen,
        int nameLen,
        int dataLen,
        FmodMode mode
    )
    {
        Version = version;
        NumSamples = numSamples;
        SampleHeaderLen = sampleHeaderLen;
        NameLen = nameLen;
        DataLen = dataLen;
        Mode = mode;
    }

    public static Fsb5Header Read(ref BinaryPrimitiveReader reader)
    {
        ReadOnlySpan<byte> magic = reader.ReadBytes(MAGIC.Length);
        if (!magic.SequenceEqual(MAGIC.Span))
            throw new UnrecognisedMagicException(MAGIC.ToArray(), magic.ToArray());

        int version = reader.ReadInt32LE();
        int numSamples = reader.ReadInt32LE();
        int sampleHeaderLen = reader.ReadInt32LE();
        int nameLen = reader.ReadInt32LE();
        int dataLen = reader.ReadInt32LE();
        FmodMode mode = (FmodMode)reader.ReadUInt32LE();

        // Skip the zero bytes, hash bytes, and dummy bytes
        reader.Seek(8 + 16 + 8);

        return new Fsb5Header
        (
            version,
            numSamples,
            sampleHeaderLen,
            nameLen,
            dataLen,
            mode
        );
    }
}
