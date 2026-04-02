// Indoor Data. Referenced by Actor files.

import type.guid;

using uint = u32;

struct Item
{
    uint Type; // Suspected
    // I'm not convinced that these first four values aren't a hash
    float UnknownValue1;
    float UnknownValue2;
    float UnknownValue3;
    uint GlobalHash; // The same across all INDR files that I've seen
    float UnknownValue5;
    type::GUID RelatedId; // The same across some/all Items. Might pair Items together?
    //uint FileHash[4]; // The same across all Items in an INDR file
    float UnknownValue7;
    float UnknownValue8;
    float UnknownValue9;
    uint UnknownValue10;
    uint UnknownValue11;
    float UnknownValue12;
    float UnknownValue13;
    float UnknownValue14;
    float UnknownValue15;
    float UnknownValue16;
};

/*struct Item_UnknownType1
{
    // I'm not convinced that these first four values aren't a hash
    float UnknownValue1;
    float UnknownValue2;
    float UnknownValue3;
    uint GlobalHash; // The same across all INDR files
    float UnknownValue5;
    type::GUID FileHash; // Not correct, produces invalid GUIDs
};*/

/*struct Item
{
    uint ElementCount; // Suspected
    Item_UnknownType1 Elements[ElementCount];
    float UnknownValue2;
};*/

struct Header
{
    char Magic[4]; // INDR
    uint Version; // Suspected
    uint ItemCount;
    Item Items[ItemCount];
};

Header main @ 0x00;