namespace Mandible.Mrn;

public enum PacketType
{
    Skeleton = 0x01,
    SkeletonMap = 0x02, // Maybe a network object? Header hashes are interesting
    EventTrack = 0x03,
    EventTrackIk = 0x04,
    Data = 0x07,
    NetworkData = 0x0A, // not sure but it has some names in it that sound like it
    PluginList = 0x0C,

    /// <summary>
    /// Contains the names of related files or files used to produce the MRN.
    /// </summary>
    FileNames = 0x0E,

    SkeletonNames = 0x0F,

    /// <summary>
    /// NSA animation data. Packets have an index rather than a namehash in the header.
    /// </summary>
    NsaData = 0x10,
}
