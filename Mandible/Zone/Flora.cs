using BinaryPrimitiveHelpers;
using Mandible.Exceptions;

namespace Mandible.Zone;

public class Flora
{
    public string Name { get; set; }
    public string Texture { get; set; }
    public string Model { get; set; }
    public bool UnknownValue1 { get; set; }
    public float UnknownValue2 { get; set; }
    public float UnknownValue3 { get; set; }

    public Flora
    (
        string name,
        string texture,
        string model,
        bool unknownValue1,
        float unknownValue2,
        float unknownValue3
    )
    {
        Name = name;
        Texture = texture;
        Model = model;
        UnknownValue1 = unknownValue1;
        UnknownValue2 = unknownValue2;
        UnknownValue3 = unknownValue3;
    }

    public static Flora Read(ref BinaryPrimitiveReader reader)
    {
        string name = reader.ReadStringNullTerminated();
        string texture = reader.ReadStringNullTerminated();
        string model = reader.ReadStringNullTerminated();
        bool unknownValue1 = reader.ReadBool();
        float unknownValue2 = reader.ReadSingleLE();
        float unknownValue3 = reader.ReadSingleLE();

        return new Flora(name, texture, model, unknownValue1, unknownValue2, unknownValue3);
    }

    public int GetSize()
        => Name.Length + 1
            + Texture.Length + 1
            + Model.Length + 1
            + sizeof(bool) // UnknownValue1
            + sizeof(float) // UnknownValue2
            + sizeof(float); // UnknownValue3

    public void Write(ref BinaryPrimitiveWriter writer)
    {
        int requiredSize = GetSize();
        if (requiredSize > writer.RemainingLength)
            throw new InvalidBufferSizeException(requiredSize, writer.RemainingLength);

        writer.WriteStringNullTerminated(Name);
        writer.WriteStringNullTerminated(Texture);
        writer.WriteStringNullTerminated(Model);
        writer.WriteBool(UnknownValue1);
        writer.WriteSingleLE(UnknownValue2);
        writer.WriteSingleLE(UnknownValue3);
    }
}
