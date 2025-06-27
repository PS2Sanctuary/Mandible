#pragma endian little

using byte = u8;
using uint = u32;
using ulong = u64;

enum CompFlag : uint
{
    Compressed_1 = 0x11,
    Compressed_2 = 0x01,
    Uncompressed_1 = 0x10,
    Uncompressed_2 = 0x00
};

fn readCompFlag() {
    // This func is called by every asset header
    CompFlag flag @ $ + sizeof(ulong) * 2;
    return flag;
};

struct CompressedAssetData
{
    be uint CompressionIndicator;
    be uint UnpackedSize;
    byte DataStart;
};

struct AssetHeader
{
    ulong NameHash; // CRC-64
    match (readCompFlag()) {
        (CompFlag::Compressed_1 | CompFlag::Compressed_2): CompressedAssetData* DataOffset: ulong;
        (CompFlag::Uncompressed_1 | CompFlag::Uncompressed_2): byte* DataOffset: ulong;
    }
    ulong AssetLength;
    CompFlag Compression;
    uint DataHash; // CRC-32
};

struct PackHeader
{
    char Signature[3];
    byte Version;
    uint AssetCount;
    ulong PackLength;
    AssetHeader *assetMap[AssetCount] : ulong;
    ulong UnknownValue1;
    byte Checksum[128];
};

PackHeader header @ 0x00;