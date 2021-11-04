using Mandible.Abstractions.Services;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Mandible.Services
{
    public class RandomAccessDataReaderService : IDataReaderService
    {
        private readonly SafeFileHandle _fileHandle;

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

        public int Read(Span<byte> buffer, long offset)
        {
            return RandomAccess.Read(_fileHandle, buffer, offset);
        }

        public async ValueTask<int> ReadAsync(Memory<byte> buffer, long offset, CancellationToken ct = default)
        {
            return await RandomAccess.ReadAsync(_fileHandle, buffer, offset, ct).ConfigureAwait(false);
        }
    }
}
