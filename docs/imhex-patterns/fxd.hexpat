#pragma endian little
#define ALIGNMENT 4

using uint = u32;
using ulong = u64;

// FXD files contain dynamic graphics effects/FX data.
// They act as a container for payload data stored in a zlib/deflate stream.
// Presumably, this is because they are also intended to be sent over the network.

struct Header
{
    ulong Hash;
    char Magic[4]; // "FXD "
    uint UnknownValue1; // Always the same, 0x01 0x02 0x02 0x00. Magic?
    uint DecompressedDataLen; // Total length of the decompressed data
    uint PayloadLen; // Includes the alignment padding before the zlib compressed data

	// Null terminated (probably).
    char PayloadMagic[];
 
    // The payload is wrapped in a zlib deflate stream and padded to start on a 4 byte boundary
    u8 paddingLen = ALIGNMENT - ($ % ALIGNMENT);
    padding[paddingLen];
    u8 ZlibDeflateData[PayloadLen - paddingLen];

    uint DecompressedChecksum; // Adler-32 checksum of the decompressed data 
};

Header main @ 0x00;
