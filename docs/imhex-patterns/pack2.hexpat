enum CompressionIndicator : u32
{
	Compressed_1 = 0x11,
	Compressed_2 = 0x01,
	Uncompressed_1 = 0x10,
	Uncompressed_2 = 0x00
};

struct AssetHeader
{
	u64 NameHash; // CRC-64
	u8 *AssetOffset : u64;
	u64 AssetLength;
	CompressionIndicator Compression;
	u32 DataHash; // CRC-32
};

struct PackHeader
{
	char Signature[3];
	u8 Version;
	u32 AssetCount;
	u64 PackLength;
	AssetHeader *assetMap[AssetCount] : u64;
	u64 UnknownValue1;
	u8 Checksum[128];
};

PackHeader header @ 0x00;
