using System.Text.Json.Serialization;

namespace Mandible.Cli.Objects;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(IndexMetadata))]
[JsonSerializable(typeof(PackIndex))]
internal partial class CliJsonContext : JsonSerializerContext
{
}
