using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Abstractions.Services;

/// <summary>
/// Represents a generic interface for reading data from an IO source.
/// </summary>
public interface IDataReaderService
{
    /// <summary>
    /// Reads as much data as the underlying source can provide into the given buffer
    /// </summary>
    /// <param name="buffer">The buffer to fill.</param>
    /// <param name="offset">The offset into the source at which to begin reading data.</param>
    /// <returns>The number of bytes read.</returns>
    int Read(Span<byte> buffer, long offset);

    /// <summary>
    /// Reads as much data as the underlying source can provide into the given buffer
    /// </summary>
    /// <param name="buffer">The buffer to fill.</param>
    /// <param name="offset">The offset into the source at which to begin reading data.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The number of bytes read.</returns>
    ValueTask<int> ReadAsync(Memory<byte> buffer, long offset, CancellationToken ct = default);

    /// <summary>
    /// Gets the length in bytes of the data source.
    /// </summary>
    /// <returns>The length</returns>
    long GetLength();
}
