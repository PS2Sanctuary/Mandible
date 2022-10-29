using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Abstractions.Services;

/// <summary>
/// Represents a generic interface for writing non-sequential data to an IO source.
/// </summary>
public interface IDataWriterService
{
    /// <summary>
    /// Reads as much data as the underlying source can provide into the given buffer
    /// </summary>
    /// <param name="buffer">The buffer to fill.</param>
    /// <param name="offset">The offset into the source at which to begin reading data.</param>
    void Write(ReadOnlySpan<byte> buffer, long offset);

    /// <summary>
    /// Reads as much data as the underlying source can provide into the given buffer
    /// </summary>
    /// <param name="buffer">The buffer to fill.</param>
    /// <param name="offset">The offset into the source at which to begin reading data.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="ValueTask"/> representing the potentially asynchronous operation.</returns>
    ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, long offset, CancellationToken ct = default);
}
