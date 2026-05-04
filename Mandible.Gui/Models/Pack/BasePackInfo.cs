namespace Mandible.Gui.Models.Pack;

public record BasePackInfo
(
    string FilePath,
    BasePackInfo.PackType Type
)
{
    public enum PackType
    {
        Pack1,
        Pack2
    }
}
