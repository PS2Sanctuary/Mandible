using uint = u32;
using ulong = u64;

#define PADDING 16

// FXD Files seem to be a container for payload data stored in a zlib/deflate stream.
// They appear to be related to dynamic graphics effects e.g. muzzle flash, orbital
// strike beams, construction object displays.

struct Header
{
    ulong Hash;
    char Magic[4]; // "FXD "
    uint UnknownValue1; // Always the same, 0x01 0x02 0x02 0x00. Magic?
    uint DecompressedDataLen; // Total length of the decompressed data
    uint PayloadLen; // Includes the alignment padding before the zlib compressed data
	
	// I suspect this might actually be null terminated. Which would bring into question
	// whether the deflate stream is actually padded to a four byte boundary.
    char PayloadMagic[4];
 
    // The payload is wrapped in a zlib deflate stream and padded to start on a 16 byte boundary
    u8 paddingLen = PADDING - ($ % PADDING);
    padding[paddingLen];
    u8 ZlibDeflateData[PayloadLen - paddingLen];

    uint DecompressedChecksum; // Adler-32 checksum of the decompressed data 
};

Header main @ 0x00;
