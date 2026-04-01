import type.guid;

using uint = u32;

/*struct Item
{
    uint Type; // Suspected
    // I'm not convinced that these first four values aren't a hash
    float UnknownValue1;
    float UnknownValue2;
    float UnknownValue3;
    uint GlobalHash; // The same across all INDR files
    float UnknownValue5;
    type::GUID FileHash;
    //uint FileHash[4]; // The same across all Items in an INDR file
    //uint UnknownValue7;
    //uint UnknownValue8;
    //float UnknownValue9;
    float UnknownValue10;
    float UnknownValue11;
    float UnknownValue12;
    float UnknownValue13;
    float UnknownValue14;
    float UnknownValue15;
    float UnknownValue16;
    float UnknownValue17;
    float UnknownValue18;
    float UnknownValue19;
};*/

struct Item_UnknownType1
{
    // I'm not convinced that these first four values aren't a hash
    float UnknownValue1;
    float UnknownValue2;
    float UnknownValue3;
    uint GlobalHash; // The same across all INDR files
    float UnknownValue5;
    type::GUID FileHash; // Not correct, produces invalid GUIDs
};

struct Item
{
    uint ElementCount; // Suspected
    Item_UnknownType1 Elements[ElementCount];
    float UnknownValue2;
};

struct Header
{
    char Magic[4]; // INDR
    uint Version; // Suspected
    uint ItemCount;
    Item Items[ItemCount];
};

Header main @ 0x00;