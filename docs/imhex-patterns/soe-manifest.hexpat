/* N.B. incomplete */

#pragma endian big

using byte = u8;
using short = s16;
using uint = u32;
using int = s32;

struct DigestProperty<T>
{
    byte Id;
    byte Length;
    T Value;
};

struct DigestArrayProperty<T>
{
    byte Id;
    byte Length;
    T Value[Length];
};

// Workaround as I don't know how to pass an ImHex "string" to a template
struct DigestStringProperty
{
    byte Id;
    byte Length;
    char Value[];
};

struct FallbackItem
{
    short UnknownValue1;
    DigestStringProperty Host;
};

struct File
{
    //short UnknownValue1;
    short UnknownValue2;
    DigestStringProperty Name;
    int UnknownValue3;
    int UnknownValue4;
    int UnknownValue5;
    int Crc;
    short UnknownValue6;
    int Timestamp;
    byte UnknownValue7;
    int UnknownValue8; // Another timestamp?
    byte Sha[20];
};

struct Folder
{
    byte FileCount;
    byte DownloadPriority; // Maybe? Could be part of the file structure
    File Files[FileCount];
};

struct Digest
{
    short UnknownValue1; // Magic value?
    byte DigestBuilderVersion; // Maybe
    DigestStringProperty ProductName;
    DigestStringProperty DefaultServerFolder;
    DigestStringProperty Publisher;
    DigestProperty<uint> PackageSizeKiB;
    DigestStringProperty DefaultLocalFolder;
    DigestStringProperty ShaAssetUrl;
    DigestProperty<int> Timestamp;
    int UnknownValue2;
    byte UnknownValue3;
    byte UnknownValue4; // I feel like this, and the next byte, are part of the Fallback structure
    DigestArrayProperty<FallbackItem> FallbackList;
};

Digest main @ 0x00;
Folder folder @ 0x18B;
Folder folder2 @ 0x1E0;