using Mandible.Pack2;
using System.Collections.Generic;

namespace Mandible.Gui.Models.Pack;

public record Pack2Info
(
    Pack2Header Header,
    IReadOnlyList<Asset2Header> Assets,
    string FilePath
) : BasePackInfo(FilePath, PackType.Pack2);
