using BinaryPrimitiveHelpers;
using Mandible.Exceptions;

namespace Mandible.Zone;

/// <summary>
/// Represents a TextureInfo definition of the <see cref="Zone"/> class.
/// </summary>
public class TextureInfo
{
    /// <summary>
    /// The name of the texture.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The name of the Color NX map used by the texture.
    /// </summary>
    public string ColorNxMapName { get; set; }

    /// <summary>
    /// The name of the specular blend NY map used by the texture.
    /// </summary>
    public string SpecBlendNyMapName { get; set; }

    /// <summary>
    /// The number of times the texture repeats.
    /// </summary>
    public uint DetailRepeat { get; set; }

    /// <summary>
    /// The blend strength of the texture.
    /// </summary>
    public float BlendStrength { get; set; }

    /// <summary>
    /// The specular range of the texture.
    /// </summary>
    public FloatRange Specular { get; set; }

    /// <summary>
    /// The smoothness range of the texture.
    /// </summary>
    public FloatRange Smoothness { get; set; }

    /// <summary>
    /// The name of the physics material that this texture uses.
    /// </summary>
    public string PhysicsMatName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextureInfo"/> class.
    /// </summary>
    /// <param name="name">The name of the texture.</param>
    /// <param name="colorNxMapName">The name of the Color NX map used by the texture.</param>
    /// <param name="specBlendNyMapName">The name of the specular blend NY map used by the texture.</param>
    /// <param name="detailRepeat">The number of times the texture repeats.</param>
    /// <param name="blendStrength">The blend strength of the texture.</param>
    /// <param name="specular">The specular range of the texture.</param>
    /// <param name="smoothness">The smoothness range of the texture.</param>
    /// <param name="physicsMatName">The name of the physics material that this texture uses.</param>
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

    /// <summary>
    /// Reads a <see cref="TextureInfo"/> instance from a <see cref="BinaryPrimitiveReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <returns>A <see cref="TextureInfo"/> instance.</returns>
    public static TextureInfo Read(ref BinaryPrimitiveReader reader)
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

    /// <summary>
    /// Gets the serialized size of this <see cref="TextureInfo"/>.
    /// </summary>
    public int GetSize()
        => Name.Length + 1
            + ColorNxMapName.Length + 1
            + SpecBlendNyMapName.Length + 1
            + sizeof(uint) // DetailRepeat
            + sizeof(float) // BlendStrength
            + FloatRange.Size // Specular
            + FloatRange.Size // Smoothness
            + PhysicsMatName.Length + 1;

    /// <summary>
    /// Writes this <see cref="TextureInfo"/> instance to a <see cref="BinaryPrimitiveWriter"/>.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <exception cref="InvalidBufferSizeException">
    /// Thrown if there is not enough space remaining in the writer.
    /// </exception>
    public void Write(ref BinaryPrimitiveWriter writer)
    {
        int requiredSize = GetSize();
        if (requiredSize > writer.RemainingLength)
            throw new InvalidBufferSizeException(requiredSize, writer.RemainingLength);

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
