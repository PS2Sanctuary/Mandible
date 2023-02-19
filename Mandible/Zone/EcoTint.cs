using Mandible.Common;
using Mandible.Exceptions;
using Mandible.Util;

namespace Mandible.Zone;

public record struct EcoTint(ColorRGBA Color, int Strength)
{
    public const int Size = ColorRGBA.Size // Color
        + sizeof(int); // Strength

    public static EcoTint Read(ref BinaryReader reader)
    {
        ColorRGBA color = ColorRGBA.Read(ref reader);
        int strength = reader.ReadInt32LE();

        return new EcoTint(color, strength);
    }

    public void Write(ref BinaryWriter writer)
    {
        if (Size > writer.Remaining)
            throw new InvalidBufferSizeException(Size, writer.Remaining);

        Color.Write(ref writer);
        writer.WriteInt32LE(Strength);
    }
}
