using Mandible.Exceptions;
using Mandible.Util;

namespace Mandible.Zone;

public class TextureInfo
{
    public string Name { get; set; }
    public string ColorNxMapName { get; set; }
    public string SpecBlendNyMapName { get; set; }
    public uint DetailRepeat { get; set; }
    public float BlendStrength { get; set; }
    public FloatRange Specular { get; set; }
    public FloatRange Smoothness { get; set; }
    public string PhysicsMatName { get; set; }

    public TextureInfo
    (
        string name,
        string colorNxMapName,
        string specBlendNyMapName,
        uint detailRepeat,
        float blendStrength,
        FloatRange specular,
        FloatRange smoothness,
        string physicsMatName
    )
    {
        Name = name;
        ColorNxMapName = colorNxMapName;
        SpecBlendNyMapName = specBlendNyMapName;
        DetailRepeat = detailRepeat;
        BlendStrength = blendStrength;
        Specular = specular;
        Smoothness = smoothness;
        PhysicsMatName = physicsMatName;
    }

    public static TextureInfo Read(ref BinaryReader reader)
    {
        string name = reader.ReadStringNullTerminated();
        string cnxName = reader.ReadStringNullTerminated();
        string sbnyName = reader.ReadStringNullTerminated();
        uint detailRepeat = reader.ReadUInt32LE();
        float blendStrength = reader.ReadSingleLE();
        FloatRange specular = FloatRange.Read(ref reader);
        FloatRange smoothness = FloatRange.Read(ref reader);
        string physicsMatName = reader.ReadStringNullTerminated();

        return new TextureInfo
        (
            name,
            cnxName,
            sbnyName,
            detailRepeat,
            blendStrength,
            specular,
            smoothness,
            physicsMatName
        );
    }

    public int GetSize()
        => Name.Length + 1
            + ColorNxMapName.Length + 1
            + SpecBlendNyMapName.Length + 1
            + sizeof(uint) // DetailRepeat
            + sizeof(float) // BlendStrength
            + FloatRange.Size // Specular
            + FloatRange.Size // Smoothness
            + PhysicsMatName.Length + 1;

    public void Write(ref BinaryWriter writer)
    {
        int requiredSize = GetSize();
        if (requiredSize > writer.Remaining)
            throw new InvalidBufferSizeException(requiredSize, writer.Remaining);

        writer.WriteStringNullTerminated(Name);
        writer.WriteStringNullTerminated(ColorNxMapName);
        writer.WriteStringNullTerminated(SpecBlendNyMapName);
        writer.WriteUInt32LE(DetailRepeat);
        writer.WriteSingleLE(BlendStrength);
        Specular.Write(ref writer);
        Smoothness.Write(ref writer);
        writer.WriteStringNullTerminated(PhysicsMatName);
    }
}
