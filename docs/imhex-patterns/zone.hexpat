#pragma pattern_limit 0xffff

using byte = u8;
using ushort = u16;
using uint = u32;
using int = s32;
using ulong = u64;

struct DataOffsets
{
    uint Ecos;
    uint Floras;
    uint InvisWalls;
    uint Objects;
    uint Lights;
    uint Unknown;
};

struct TileInfo
{
    uint QuadCount;
    float Width;
    float Height;
    uint VertexCount;
};

struct ChunkInfo
{
    uint TileCount;
    int StartX;
    int StartY;
    uint CountX;
    uint CountY;
};

struct FloatMinMax
{
    float Minimum;
    float Maximum;
};

struct TextureInfo
{
    char Name[];
    char CnxName[];
    char SbnyName[];
    uint DetailRepeat;
    float BlendStrength;
    FloatMinMax Specular;
    FloatMinMax Smoothness;
    char PhysicsMatName[];
};

struct ColorRGBA
{
    byte R;
    byte G;
    byte B;
    byte Alpha;
};

struct EcoTint
{
    ColorRGBA Color;
    int Strength;
};

struct EcoLayer
{
    float Density;
    FloatMinMax Scale;
    float SlopePeak;
    float SlopeExtent;
    FloatMinMax Elevation;
    byte MinAlpha;
    char FloraName[];
    uint TintsLen;
    EcoTint Tints[TintsLen];
};

struct FloraInfo
{
    uint LayersLen;
    EcoLayer Layers[LayersLen];
};

struct Eco
{
    uint Index;
    TextureInfo Texture;
    FloraInfo Flora;
};

struct Flora
{
    char Name[];
    char Texture[];
    char Model[];
    bool UnknownValue1;
    float UnknownValue2;
    float UnknownValue3;
};

struct InvisibleWall
{
    // Unknown; no reference data
};

struct Vector4
{
    float X;
    float Y;
    float Z;
    float W;
};

struct Instance
{
    Vector4 Translation;
    Vector4 Rotation;
    Vector4 Scale;
    uint UnknownValue1;
    byte UnknownValue2;
    byte UnknownValue3; // This byte is not present in v1
    float UnknownValue4;
};

struct RuntimeObject
{
    char ActorFile[];
    float RenderDistance;
    uint InstancesLen;
    Instance Instances[InstancesLen];
};

enum LightType : u32
{
    Point = 1,
    Spot = 2
};

struct ColorARGB
{
    byte Alpha;
    byte R;
    byte G;
    byte B;
};

struct Vector2
{
    float X;
    float Y;
};

struct Light
{
    char Name[];
    char ColorName[];
    LightType Type;
    bool UnknownValue1;
    Vector4 Translation;
    Vector4 Rotation;
    Vector2 UnknownValue2;
    ColorARGB Color;
    byte UnknownValue3[26];
};

struct Zone
{
    char Magic[4];
    uint Version;
    DataOffsets Offsets;
    TileInfo TileInfo;
    ChunkInfo ChunkInfo;

    uint EcosLen;
    Eco Ecos[EcosLen];
    
    uint FlorasLen;
    Flora Floras[FlorasLen];
    
    uint InvisWallsLen;
    InvisibleWall InvisWalls[InvisWallsLen];
    
    uint ObjectsLen;
    RuntimeObject Objects[ObjectsLen];
    
    uint LightsLen;
    Light Lights[LightsLen];
    
    uint UVal1Len;
    u8 UnknownValue1[UVal1Len];
};

Zone main @ 0x00;
