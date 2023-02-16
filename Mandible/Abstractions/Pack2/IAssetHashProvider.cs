using Mandible.Pack2;
using System;

namespace Mandible.Abstractions.Pack2;

/// <summary>
/// Defines an interface for generating hashes of asset data.
/// </summary>
public interface IAssetHashProvider
{
    /// <summary>
    /// Calculates a hash of the given asset's data.
    /// </summary>
    /// <param name="header">The asset header.</param>
    /// <param name="data">The asset data.</param>
    /// <returns></returns>
    public uint CalculateDataHash(Asset2Header header, ReadOnlySpan<byte> data);
}
