using Mandible.Abstractions.Services;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Services
{
    /// <inheritdoc cref="IDataReaderService"/>
    public class StreamDataReaderService : IDataReaderService
    {
        private readonly long _baseOffset;
        private readonly Stream _input;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamDataReaderService"/> class.
        /// </summary>
        /// <param name="input">The stream to read from. The current position of the stream is considered the starting point of this reader.</param>
        /// <exception cref="ArgumentException">If the input stream is in an invalid state.</exception>
        public StreamDataReaderService(Stream input)
        {
            if (!input.CanRead)
                throw new ArgumentException("The input stream must be readable", nameof(input));

            if (!input.CanSeek)
                throw new ArgumentException("The input stream must be seekable", nameof(input));

            _baseOffset = input.Position;
            _input = input;
        }

        /// <inheritdoc />
        public int Read(Span<byte> buffer, long offset)
        {
            _input.Seek(offset + _baseOffset, SeekOrigin.Begin);

            return _input.Read(buffer);
        }

        /// <inheritdoc />
        public async ValueTask<int> ReadAsync(Memory<byte> buffer, long offset, CancellationToken ct = default)
        {
            _input.Seek(offset + _baseOffset, SeekOrigin.Begin);

            return await _input.ReadAsync(buffer, ct).ConfigureAwait(false);
        }
    }
}
