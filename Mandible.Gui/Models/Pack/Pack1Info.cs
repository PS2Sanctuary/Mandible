using Mandible.Pack;
using System.Collections.Generic;

namespace Mandible.Gui.Models.Pack;

public record Pack1Info
(
    IReadOnlyList<AssetHeader> Assets,
    string FilePath
) : BasePackInfo(FilePath, PackType.Pack1);
