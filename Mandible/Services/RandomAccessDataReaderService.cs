using Mandible.Abstractions.Services;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Services;

/// <summary>
/// Represents an interface for reading data using the <see cref="RandomAccess"/> API.
/// </summary>
public class RandomAccessDataReaderService : IDataReaderService, IDisposable
{
    private readonly SafeFileHandle _fileHandle;

    /// <summary>
    /// Gets a value indicating whether or not this instance has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RandomAccessDataReaderService"/> class.
    /// </summary>
    /// <param name="filePath">The path to the file to read from.</param>
    public RandomAccessDataReaderService(string filePath)
    {
        _fileHandle = File.OpenHandle
        (
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            FileOptions.RandomAccess | FileOptions.Asynchronous
        );
    }

    /// <inheritdoc />
    public int Read(Span<byte> buffer, long offset)
        => RandomAccess.Read(_fileHandle, buffer, offset);

    /// <inheritdoc />
    public async ValueTask<int> ReadAsync(Memory<byte> buffer, long offset, CancellationToken ct = default)
        => await RandomAccess.ReadAsync(_fileHandle, buffer, offset, ct).ConfigureAwait(false);

    /// <inheritdoc />
    public void Dispose()
    {
        if (!IsDisposed)
        {
            _fileHandle.Dispose();

            IsDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
