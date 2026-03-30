using byte = u8;
using uint = u32;
using int = s32;

enum FmodMode : uint
{
    FMOD_DEFAULT = 0x00000000,
    FMOD_LOOP_OFF = 0x00000001,
    FMOD_LOOP_NORMAL = 0x00000002,
    FMOD_LOOP_BIDI = 0x00000004,
    FMOD_2D = 0x00000008,
    FMOD_3D = 0x00000010,
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
};

struct Fsb5Header
{
    char Magic[4];  // 'FSB5'
    int Version;
    int NumSamples; // The number of samples in the file
    int SampleHeaderLen; // The length in bytes of all the sample headers, including extended information
    int NameLen; // The length in bytes of the name table
    int DataLen; // The total size in bytes of the compressed sample data
    FmodMode Mode; // Flags that apply to all samples in the file.
    byte Zero[8]; // An empty buffer with a single 0x1 byte at the start
    byte Hash[16];
    byte Dummy[8];
};

Fsb5Header main @ 0x00;