#pragma endian little
#define ALIGNMENT 8

using ulong = u64;

struct Header
{
    u8 Magic[16]; // 0x01, 0x00, 0x05, 0x01, 0, 0, 0, 0, 0x78, 0, 0, 0, 0, 0, 0, 0
    ulong UnknownValue1;
    ulong StartMarker; // Some sort of ID?
    ulong UnknownValue3;
    ulong DataLen; // The length of everything excluding the Magic bytes
    ulong EndMarkerOffset; // Value at offset always matches StartMarker
    ulong UnknownValue4;
    ulong UnknownValue5;
    ulong UnknownValue6;
    ulong UnknownValue7;
    ulong UnknownValue8;
    ulong UnknownValue9;
    ulong UnknownValue10;
    ulong UnknownValue11;
    char Name[];
    padding[ALIGNMENT - ($ % ALIGNMENT)];
};

Header main @ 0x00;