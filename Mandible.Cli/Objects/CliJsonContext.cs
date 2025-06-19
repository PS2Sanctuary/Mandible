using System.Text.Json.Serialization;

namespace Mandible.Cli.Objects;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    UseStringEnumConverter = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(IndexMetadata))]
[JsonSerializable(typeof(PackIndex))]
internal partial class CliJsonContext : JsonSerializerContext
{
}
