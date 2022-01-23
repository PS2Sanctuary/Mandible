using Mandible.Abstractions.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Services;

/// <summary>
/// Implements an <see cref="IDataReaderService"/> for reading data from a stream.
/// </summary>
public class StreamDataReaderService : IDataReaderService, IDisposable, IAsyncDisposable
{
    private readonly long _baseOffset;
    private readonly Stream _input;
    private readonly bool _leaveOpen;

    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamDataReaderService"/> class.
    /// </summary>
    /// <param name="input">The stream to read from. The current position of the stream is considered the starting point of this reader.</param>
    /// <param name="leaveOpen">A value indicating whether or not to leave the <paramref name="input"/> stream open upon disposing this instance.</param>
    /// <exception cref="ArgumentException">If the input stream is in an invalid state.</exception>
    public StreamDataReaderService(Stream input, bool leaveOpen)
    {
        if (!input.CanRead)
            throw new ArgumentException("The input stream must be readable", nameof(input));

        if (!input.CanSeek)
            throw new ArgumentException("The input stream must be seekable", nameof(input));

        _baseOffset = input.Position;
        _input = input;
        _leaveOpen = leaveOpen;
    }

    /// <inheritdoc />
    public int Read(Span<byte> buffer, long offset)
    {
        _input.Seek(offset + _baseOffset, SeekOrigin.Begin);

        return _input.Read(buffer);
    }

    /// <inheritdoc />
    public long GetLength()
        => _input.Length;

    /// <inheritdoc />
    public async ValueTask<int> ReadAsync(Memory<byte> buffer, long offset, CancellationToken ct = default)
    {
        _input.Seek(offset + _baseOffset, SeekOrigin.Begin);

        return await _input.ReadAsync(buffer, ct).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of managed and unmanaged resources.
    /// </summary>
    /// <param name="disposeManaged">A value indicating whether or not to dispose of managed resources.</param>
    protected virtual void Dispose(bool disposeManaged)
    {
        if (IsDisposed)
            return;

        if (disposeManaged && !_leaveOpen)
            _input.Dispose();

        IsDisposed = true;
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (IsDisposed)
            return;

        await _input.DisposeAsync().ConfigureAwait(false);

        IsDisposed = true;
    }
}
