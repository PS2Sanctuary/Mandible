using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Util;

/// <summary>
/// Provides a <see cref="Stream"/> for the contents of a <see cref="ReadOnlyMemory{T}"/>.
/// </summary>
internal sealed class ReadOnlyMemoryStream : Stream
{
    private ReadOnlyMemory<byte> _content;
    private int _position;
    private bool _isOpen;

    public override bool CanRead => _isOpen;
    public override bool CanSeek => _isOpen;
    public override bool CanWrite => false;

    public override long Length
    {
        get
        {
            EnsureNotClosed();
            return _content.Length;
        }
    }

    public override long Position
    {
        get
        {
            EnsureNotClosed();
            return _position;
        }
        set
        {
            EnsureNotClosed();
            if (value is < 0 or > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            _position = (int)value;
        }
    }

    public ReadOnlyMemoryStream(ReadOnlyMemory<byte> content)
    {
        _content = content;
        _isOpen = true;
    }

    private void EnsureNotClosed()
    {
        ObjectDisposedException.ThrowIf(!_isOpen, this);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        EnsureNotClosed();

        long pos = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => _content.Length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };

        ArgumentOutOfRangeException.ThrowIfGreaterThan(pos, int.MaxValue, nameof(offset));
        ArgumentOutOfRangeException.ThrowIfLessThan(pos, 0, nameof(offset));

        _position = (int)pos;
        return _position;
    }

    public override int ReadByte()
    {
        EnsureNotClosed();

        ReadOnlySpan<byte> s = _content.Span;
        return _position < s.Length ? s[_position++] : -1;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ValidateBufferArguments(buffer, offset, count);
        return ReadBuffer(new Span<byte>(buffer, offset, count));
    }

    public override int Read(Span<byte> buffer)
        => ReadBuffer(buffer);

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ValidateBufferArguments(buffer, offset, count);
        EnsureNotClosed();

        return cancellationToken.IsCancellationRequested
            ? Task.FromCanceled<int>(cancellationToken)
            : Task.FromResult(ReadBuffer(new Span<byte>(buffer, offset, count)));
    }

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        EnsureNotClosed();

        return cancellationToken.IsCancellationRequested
            ? ValueTask.FromCanceled<int>(cancellationToken)
            : new ValueTask<int>(ReadBuffer(buffer.Span));
    }

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        => TaskToAsyncResult.Begin(ReadAsync(buffer, offset, count), callback, state);

    public override int EndRead(IAsyncResult asyncResult)
    {
        EnsureNotClosed();
        return TaskToAsyncResult.End<int>(asyncResult);
    }

    public override void CopyTo(Stream destination, int bufferSize)
    {
        ValidateCopyToArguments(destination, bufferSize);
        EnsureNotClosed();

        if (_content.Length > _position)
        {
            destination.Write(_content.Span[_position..]);
            _position = _content.Length;
        }
    }

    public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        ValidateCopyToArguments(destination, bufferSize);
        EnsureNotClosed();

        if (_content.Length <= _position)
            return Task.CompletedTask;

        ReadOnlyMemory<byte> content = _content[_position..];
        _position = _content.Length;
        return destination.WriteAsync(content, cancellationToken).AsTask();
    }

    public override void Flush()
    {
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;

    public override void SetLength(long value)
        => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count)
        => throw new NotSupportedException();

    private int ReadBuffer(Span<byte> buffer)
    {
        EnsureNotClosed();

        int remaining = _content.Length - _position;

        if (remaining <= 0 || buffer.Length == 0)
            return 0;

        if (remaining <= buffer.Length)
        {
            _content.Span[_position..].CopyTo(buffer);
            _position = _content.Length;
            return remaining;
        }

        _content.Span.Slice(_position, buffer.Length).CopyTo(buffer);
        _position += buffer.Length;
        return buffer.Length;
    }

    protected override void Dispose(bool disposing)
    {
        _isOpen = false;
        _content = default;
        base.Dispose(disposing);
    }
}
