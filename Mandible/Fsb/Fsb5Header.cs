#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using BinaryPrimitiveHelpers;
using Mandible.Abstractions;
using Mandible.Common;
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

    /// <summary>
    /// Decompress at loadtime, decompressing or decoding whole file into memory as the target sample format (ie PCM).
    /// Fastest for playback and most flexible.
    /// </summary>
    FMOD_CREATESAMPLE = 0x00000100,

    /// <summary>
    /// Load MP2/MP3/FADPCM/IMAADPCM/Vorbis/AT9 or XMA into memory and leave it compressed. Vorbis/AT9/FADPCM encoding
    /// only supported in the .FSB container format. During playback the FMOD software mixer will decode it in realtime
    /// as a 'compressed sample'. Overrides FMOD_CREATESAMPLE. If the sound data is not one of the supported formats,
    /// it will behave as if it was created with FMOD_CREATESAMPLE and decode the sound into PCM.
    /// </summary>
    FMOD_CREATECOMPRESSEDSAMPLE = 0x00000200,

    /// <summary>
    /// Opens a user created static sample or stream. Use FMOD_CREATESOUNDEXINFO to specify format, defaultfrequency,
    /// numchannels, and optionally a read callback. If a user created 'sample' is created with no read callback, the
    /// sample will be empty. Use Sound::lock and Sound::unlock to place sound data into the sound if this is the case.
    /// </summary>
    FMOD_OPENUSER = 0x00000400,

    /// <summary>
    /// "name_or_data" will be interpreted as a pointer to memory instead of filename for creating sounds. Use
    /// FMOD_CREATESOUNDEXINFO to specify length. If used with FMOD_CREATESAMPLE or FMOD_CREATECOMPRESSEDSAMPLE, FMOD
    /// duplicates the memory into its own buffers. Your own buffer can be freed after open. If used with
    /// FMOD_CREATESTREAM, FMOD will stream out of the buffer whose pointer you passed in. In this case, your own buffer
    /// should not be freed until you have finished with and released the stream.
    /// </summary>
    FMOD_OPENMEMORY = 0x00000800,

    /// <summary>
    /// "name_or_data" will be interpreted as a pointer to memory instead of filename for creating sounds. Use
    /// FMOD_CREATESOUNDEXINFO to specify length. This differs to FMOD_OPENMEMORY in that it uses the memory as is,
    /// without duplicating the memory into its own buffers. Cannot be freed after open, only after Sound::release.
    /// Will not work if the data is compressed and FMOD_CREATECOMPRESSEDSAMPLE is not used.
    /// </summary>
    FMOD_OPENMEMORY_POINT = 0x10000000,

    /// <summary>
    /// Will ignore file format and treat as raw pcm. Use FMOD_CREATESOUNDEXINFO to specify format. Requires at least
    /// defaultfrequency, numchannels and format to be specified before it will open. Must be little endian data.
    /// </summary>
    FMOD_OPENRAW = 0x00001000,

    /// <summary>
    /// Just open the file, don't prebuffer or read. Good for fast opens for info, or when sound::readData is to be
    /// used.
    /// </summary>
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
public class Fsb5Header : IBinarySerializable<Fsb5Header>
{
    /// <summary>
    /// Gets the size in bytes of a serialized <see cref="Fsb5Header"/> structure.
    /// </summary>
    public const int SIZE = 4 // Magic
        + sizeof(int) // Version
        + sizeof(int) // NumSamples
        + sizeof(int) // SampleHeaderLen
        + sizeof(int) // NameLen
        + sizeof(int) // DataLen
        + sizeof(FmodMode) // Mode
        + 8 // Zero
        + 16 // Hash
        + 8; // Dummy

    /// <summary>
    /// Gets the magic identifier of a zone file.
    /// </summary>
    public static readonly ReadOnlyMemory<byte> MAGIC = FileIdentifiers.Magics[FileType.FmodSoundBank5];

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

    /// <inheritdoc />
    public static Fsb5Header Deserialize(ref BinaryPrimitiveReader reader)
    {
        ReadOnlySpan<byte> magic = reader.ReadBytes(MAGIC.Length);
        UnrecognisedMagicException.ThrowIfNotAtStart(MAGIC.Span, magic);

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

    /// <inheritdoc />
    public int GetSerializedSize()
        => SIZE;

    /// <inheritdoc />
    public void Serialize(ref BinaryPrimitiveWriter writer)
        => throw new NotImplementedException();
}
