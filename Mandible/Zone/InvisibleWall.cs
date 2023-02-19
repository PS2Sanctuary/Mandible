using Mandible.Exceptions;
using Mandible.Util;

namespace Mandible.Zone;

public class InvisibleWall
{
    public const int Size = sizeof(uint) // UnknownValue1
        + sizeof(float) // UnknownValue2
        + sizeof(float) // UnknownValue3
        + sizeof(float); // UnknownValue4

    public uint UnknownValue1 { get; set; }
    public float UnknownValue2 { get; set; }
    public float UnknownValue3 { get; set; }
    public float UnknownValue4 { get; set; }

    public InvisibleWall(uint unknownValue1, float unknownValue2, float unknownValue3, float unknownValue4)
    {
        UnknownValue1 = unknownValue1;
        UnknownValue2 = unknownValue2;
        UnknownValue3 = unknownValue3;
        UnknownValue4 = unknownValue4;
    }

    public static InvisibleWall Read(ref BinaryReader reader)
    {
        uint unknownValue1 = reader.ReadUInt32LE();
        float unknownValue2 = reader.ReadSingleLE();
        float unknownValue3 = reader.ReadSingleLE();
        float unknownValue4 = reader.ReadSingleLE();

        return new InvisibleWall(unknownValue1, unknownValue2, unknownValue3, unknownValue4);
    }

    public void Write(ref BinaryWriter writer)
    {
        if (Size > writer.Remaining)
            throw new InvalidBufferSizeException(Size, writer.Remaining);

        writer.WriteUInt32LE(UnknownValue1);
        writer.WriteSingleLE(UnknownValue2);
        writer.WriteSingleLE(UnknownValue3);
        writer.WriteSingleLE(UnknownValue4);
    }
}
