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

struct FloatRange
{
    float Minimum;
    float Maximum;
};

struct TextureInfo
{
    char Name[];
    char ColorNxMapName[];
    char SpecBlendNyMapName[];
    uint DetailRepeat;
    float BlendStrength;
    FloatRange Specular;
    FloatRange Smoothness;
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
    FloatRange Scale;
    float SlopePeak;
    float SlopeExtent;
    FloatRange Elevation;
    byte MinAlpha;
    char FloraName[];
    uint TintsLen;
    EcoTint Tints[TintsLen];
};

struct Eco
{
    uint Index;
    TextureInfo Texture;
    uint LayersLen;
    EcoLayer Layers[LayersLen];
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
    uint UnknownValue1;
    float UnknownValue2;
    float UnknownValue3;
    float UnknownValue4;
};

struct Vector4
{
    float X;
    float Y;
    float Z;
    float W;
};

struct ObjectInstance
{
    Vector4 Translation;
    Vector4 Rotation;
    Vector4 Scale;
    uint Id;
    byte UnknownValue1;
    float UnknownValue2;
};

struct RuntimeObject
{
    char ActorFile[];
    float RenderDistance;
    uint InstancesLen;
    ObjectInstance Instances[InstancesLen];
};

enum LightType : ushort
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

struct Light
{
    char Name[];
    char ColorName[];
    LightType Type;
    ushort UnknownValue1;
    bool UnknownValue2;
    Vector4 Translation;
    Vector4 Rotation;
    float Range;
    float InnerRange;
    ColorARGB Color;
    uint UnknownValue3;
    byte UnknownValue4;
    Vector4 UnknownValue5;
    char UnknownValue6[];
    uint Id;
    float UnknownValue7;
    uint UnknownValue8;
    bool UnknownValue9;
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
    
    uint FloraeLen;
    Flora Florae[FloraeLen];
    
    uint InvisWallsLen;
    InvisibleWall InvisWalls[InvisWallsLen];
    
    uint ObjectsLen;
    RuntimeObject Objects[ObjectsLen];
    
    uint LightsLen;
    Light Lights[LightsLen];
    
    uint UVal1Len;
    byte UnknownValue1[UVal1Len];
};

Zone main @ 0x00;
