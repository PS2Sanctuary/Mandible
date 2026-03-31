using System.Text.Json.Serialization;

namespace Mandible.Cli;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Zone.Zone))]
public partial class AppJsonContext : JsonSerializerContext
{

}
