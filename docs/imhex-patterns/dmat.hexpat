#pragma endian little

using byte = u8;
using ushort = u16;
using uint = u32;
using ulong = u64;

struct Dmat_Material_Parameter
{
    uint SemanticHash;
    uint D3DXParameterClass;
    uint D3DXParameterType;
    uint DataLen;
    byte Data[DataLen];
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
	char Textures[TexturesBlockLen]; // Actually an array of null-terminated strings
	uint MaterialsLen;
	Dmat_Material Materials[MaterialsLen];
};

Dmat main @ 0x00;
