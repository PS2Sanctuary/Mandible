using Mandible.Cli.Objects;
using System.Text.Json.Serialization;

namespace Mandible.Cli;

[JsonSourceGenerationOptions(
    WriteIndented = true,
    UseStringEnumConverter = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(IndexMetadata))]
[JsonSerializable(typeof(PackIndex))]
[JsonSerializable(typeof(Zone.Zone))]
internal partial class CliJsonContext : JsonSerializerContext
{
}
