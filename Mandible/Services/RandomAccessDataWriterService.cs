using Mandible.Abstractions.Services;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Services;

/// <summary>
/// Implements an <see cref="IDataWriterService"/> for writing data using the <see cref="RandomAccess"/> API.
/// </summary>
public sealed class RandomAccessDataWriterService : IDataWriterService, IDisposable
{
    private readonly SafeFileHandle _fileHandle;

    /// <summary>
    /// Gets a value indicating whether or not this instance has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RandomAccessDataWriterService"/>.
    /// </summary>
    /// <param name="path">The path to the file to write to.</param>
    /// <param name="createMode">The access mode.</param>
    public RandomAccessDataWriterService(string path, FileMode createMode = FileMode.CreateNew)
    {
        _fileHandle = File.OpenHandle
        (
            path,
            createMode,
            FileAccess.Write,
            FileShare.Read,
            FileOptions.RandomAccess | FileOptions.Asynchronous
        );
    }

    /// <inheritdoc />
    public void Write(ReadOnlySpan<byte> buffer, long offset)
        => RandomAccess.Write(_fileHandle, buffer, offset);

    /// <inheritdoc />
    public async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, long offset, CancellationToken ct = default)
        => await RandomAccess.WriteAsync(_fileHandle, buffer, offset, ct).ConfigureAwait(false);

    /// <inheritdoc />
    public void Dispose()
    {
        if (IsDisposed)
            return;

        _fileHandle.Dispose();
        IsDisposed = true;
    }
}
