#pragma endian little

using byte = u8;
using ushort = u16;
using uint = u32;
using int = s32;
using ulong = u64;

uint _startOffset; // Temporary value used for dynamic-length array reading

struct NullTerminatedString
{
    char _value[];
} [[inline]];

enum D3DXParameterClass : uint
{
    Scalar = 0,
    Vector = 1,
    MatrixRows = 2,
    MatrixColumns = 3,
    Object = 4,
    Struct = 5,
    ForceDword = 0x7fffffff
};

enum D3DXParameterType : uint
{
    Void = 0,
    Boolean = 1,
    Integer = 2,
    Float = 3,
    String = 4,
    Texture = 5,
    Texture1D = 6,
    Texture2D = 7,
    Texture3D = 8,
    TextureCube = 9,
    Sampler = 10,
    Sampler1D = 11,
    Sampler2D = 12,
    Sampler3D = 13,
    SamplerCube = 14,
    PixelShader = 15,
    VertexShader = 16,
    PixelFragment = 17,
    VertexFragment = 18,
    Unsupported = 19,
    ForceDword = 0x7fffffff
};


struct Dmat_Material_Parameter
{
    uint SemanticHash;
    D3DXParameterClass Class;
    D3DXParameterType Type;
    uint DataLen;
    
    _startOffset = $;
    match (Class, Type)
    {
        (D3DXParameterClass::Scalar, D3DXParameterType::Boolean): bool Data;
        (D3DXParameterClass::Scalar, D3DXParameterType::Integer): int Data;
        (D3DXParameterClass::Scalar, D3DXParameterType::Float): float Data;
        (D3DXParameterClass::Scalar, D3DXParameterType::String): char Data[DataLen];
        (D3DXParameterClass::Vector, D3DXParameterType::Boolean): bool Data[DataLen];
        (D3DXParameterClass::Vector, D3DXParameterType::Integer): int Data[DataLen / 4];
        (D3DXParameterClass::Vector, D3DXParameterType::Float): float Data[DataLen / 4];
        (D3DXParameterClass::Vector, D3DXParameterType::String): NullTerminatedString Data[while($ - _startOffset < DataLen)];
        (D3DXParameterClass::Object, D3DXParameterType::Texture): uint NameHash;
        (_, _): byte Data[DataLen];
    }
};

struct Dmat_Material
{
    uint NameHash;
    uint DataLen;
    uint MaterialDefinitionHash;
    uint ParametersLen;
    Dmat_Material_Parameter Parameters[ParametersLen];
};

struct Dmat
{
	char Magic[4];
	uint Version;
	uint TexturesBlockLen;
	_startOffset = $;
	NullTerminatedString Textures[while($ - _startOffset < TexturesBlockLen)];
	uint MaterialsLen;
	Dmat_Material Materials[MaterialsLen];
};

Dmat main @ 0x00;