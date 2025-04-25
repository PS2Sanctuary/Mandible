using BinaryPrimitiveHelpers;
using Mandible.Exceptions;
using System.Collections.Generic;

namespace Mandible.Zone;

/// <summary>
/// Represents a runtime object of the <see cref="Zone"/> class.
/// Used to define actors present in the zone.
/// </summary>
public class RuntimeObject
{
    /// <summary>
    /// Gets or sets the name of the actor file used by the object.
    /// </summary>
    public string ActorFile { get; set; }

    /// <summary>
    /// Gets or sets the distance at which the object will start being rendered.
    /// </summary>
    public float RenderDistance { get; set; }

    /// <summary>
    /// Gets or sets the list of in-world instances of the object.
    /// </summary>
    public List<ObjectInstance> Instances { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeObject"/> class.
    /// </summary>
    /// <param name="actorFile">The name of the actor file used by the object.</param>
    /// <param name="renderDistance">The distance at which the object will start being rendered.</param>
    /// <param name="instances">The list of in-world instances of the object.</param>
    public RuntimeObject(string actorFile, float renderDistance, List<ObjectInstance> instances)
    {
        ActorFile = actorFile;
        RenderDistance = renderDistance;
        Instances = instances;
    }

    /// <summary>
    /// Reads a <see cref="RuntimeObject"/> from a <see cref="BinaryPrimitiveReader"/>.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="version">The zone version that the data is serialized as.</param>
    /// <returns>The deserialized <see cref="RuntimeObject"/>.</returns>
    public static RuntimeObject Read(ref BinaryPrimitiveReader reader, ZoneVersion version)
    {
        string actorFile = reader.ReadStringNullTerminated();
        float renderDistance = reader.ReadSingleLE();

        uint instanceCount = reader.ReadUInt32LE();
        List<ObjectInstance> instances = new((int)instanceCount);
        for (int i = 0; i < instanceCount; i++)
            instances.Add(ObjectInstance.Read(ref reader, version));

        return new RuntimeObject(actorFile, renderDistance, instances);
    }

    /// <summary>
    /// Gets the serialized size of this <see cref="RuntimeObject"/>.
    /// </summary>
    /// <param name="version">The zone version to serialize to.</param>
    /// <returns>The size, in bytes.</returns>
    public int GetSize(ZoneVersion version)
        => ActorFile.Length + 1 // Null-terminated
            + sizeof(float) // RenderDistance
            + sizeof(uint) // Instances.Count
            + Instances.Count * ObjectInstance.GetSize(version);

    /// <summary>
    /// Writes this <see cref="RuntimeObject"/> to a <see cref="BinaryPrimitiveWriter"/>.
    /// </summary>
    /// <param name="writer">The writer.</param>
    /// <param name="version">The zone version to serialize to.</param>
    /// <exception cref="InvalidBufferSizeException">
    /// Thrown if the writer does not have enough remaining space.
    /// </exception>
    public void Write(ref BinaryPrimitiveWriter writer, ZoneVersion version)
    {
        int requiredSize = GetSize(version);
        if (writer.RemainingLength < requiredSize)
            throw new InvalidBufferSizeException(requiredSize, writer.RemainingLength);

        writer.WriteStringNullTerminated(ActorFile);
        writer.WriteSingleLE(RenderDistance);

        writer.WriteUInt32LE((uint)Instances.Count);
        foreach (ObjectInstance instance in Instances)
            instance.Write(ref writer, version);
    }
}
