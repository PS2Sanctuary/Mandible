﻿using System.Diagnostics.CodeAnalysis;

namespace Mandible.Zlib
{
    /// <summary>
	/// <para>
    /// This class allows us to retrieve a specified number of bits from
    /// the input buffer, as well as copy big byte blocks.
	/// </para>
    ///
	/// <para>
    /// It uses an int buffer to store up to 31 bits for direct
    /// manipulation.  This guarantees that we can get at least 16 bits,
    /// but we only need at most 15, so this is all safe.
    /// </para>
	///
	/// <para>
    /// There are some optimizations in this class, for example, you must
    /// never peek more than 8 bits more than needed, and you must first
    /// peek bits before you may drop them.  This is not a general purpose
    /// class but optimized for the behaviour of the Inflater.
	/// </para>
    ///
    /// Authors of the original java version : John Leuner, Jochen Hoenicke
    /// </summary>
    public class StreamManipulator
	{
		#region Instance Fields

		[AllowNull]
		private byte[] _window;
		private int _windowStart;
		private int _windowEnd;

		private uint _buffer;
		private int _bitsInBuffer;

		#endregion Instance Fields

		/// <summary>
		/// Gets the number of bits available in the bit buffer. This must
		/// only be called when a previous PeekBits() returned -1.
		/// </summary>
		public int AvailableBits => _bitsInBuffer;

		/// <summary>
		/// Gets the number of bytes available.
		/// </summary>
		public int AvailableBytes => _windowEnd - _windowStart + (_bitsInBuffer >> 3);

		/// <summary>
		/// Returns true when SetInput can be called.
		/// </summary>
		public bool IsNeedingInput => _windowStart == _windowEnd;

		/// <summary>
		/// Get the next sequence of bits but don't increase input pointer.  bitCount must be
		/// less or equal 16 and if this call succeeds, you must drop
		/// at least n - 8 bits in the next call.
		/// </summary>
		/// <param name="bitCount">The number of bits to peek.</param>
		/// <returns>
		/// the value of the bits, or -1 if not enough bits available.  */
		/// </returns>
		public int PeekBits(int bitCount)
		{
			if (_bitsInBuffer < bitCount)
			{
				if (_windowStart == _windowEnd)
				{
					return -1; // ok
				}

				_buffer |= (uint)(((_window[_windowStart++] & 0xff) | (_window[_windowStart++] & 0xff) << 8) << _bitsInBuffer);
				_bitsInBuffer += 16;
			}

			return (int)(_buffer & ((1 << bitCount) - 1));
		}

		/// <summary>
		/// Tries to grab the next <paramref name="bitCount"/> bits from the input and
		/// sets <paramref name="output"/> to the value, adding <paramref name="outputOffset"/>.
		/// </summary>
		/// <returns>true if enough bits could be read, otherwise false</returns>
		public bool TryGetBits(int bitCount, ref int output, int outputOffset = 0)
		{
			var bits = PeekBits(bitCount);
			if (bits < 0)
				return false;

			output = bits + outputOffset;
			DropBits(bitCount);

			return true;
		}

		/// <summary>
		/// Tries to grab the next <paramref name="bitCount"/> bits from the input and
		/// sets <paramref name="index"/> of <paramref name="array"/> to the value.
		/// </summary>
		/// <returns>true if enough bits could be read, otherwise false</returns>
		public bool TryGetBits(int bitCount, ref byte[] array, int index)
		{
			var bits = PeekBits(bitCount);
			if (bits < 0)
				return false;

			array[index] = (byte)bits;
			DropBits(bitCount);

			return true;
		}

		/// <summary>
		/// Drops the next n bits from the input.  You should have called PeekBits
		/// with a bigger or equal n before, to make sure that enough bits are in
		/// the bit buffer.
		/// </summary>
		/// <param name="bitCount">The number of bits to drop.</param>
		public void DropBits(int bitCount)
		{
			_buffer >>= bitCount;
			_bitsInBuffer -= bitCount;
		}

		/// <summary>
		/// Gets the next n bits and increases input pointer.  This is equivalent
		/// to <see cref="PeekBits"/> followed by <see cref="DropBits"/>, except for correct error handling.
		/// </summary>
		/// <param name="bitCount">The number of bits to retrieve.</param>
		/// <returns>
		/// the value of the bits, or -1 if not enough bits available.
		/// </returns>
		public int GetBits(int bitCount)
		{
			int bits = PeekBits(bitCount);
			if (bits >= 0)
			{
				DropBits(bitCount);
			}
			return bits;
		}

		/// <summary>
		/// Skips to the next byte boundary.
		/// </summary>
		public void SkipToByteBoundary()
		{
			_buffer >>= (_bitsInBuffer & 7);
			_bitsInBuffer &= ~7;
		}

		/// <summary>
		/// Copies bytes from input buffer to output buffer starting
		/// at output[offset].  You have to make sure, that the buffer is
		/// byte aligned.  If not enough bytes are available, copies fewer
		/// bytes.
		/// </summary>
		/// <param name="output">
		/// The buffer to copy bytes to.
		/// </param>
		/// <param name="offset">
		/// The offset in the buffer at which copying starts
		/// </param>
		/// <param name="length">
		/// The length to copy, 0 is allowed.
		/// </param>
		/// <returns>
		/// The number of bytes copied, 0 if no bytes were available.
		/// </returns>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Length is less than zero
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// Bit buffer isnt byte aligned
		/// </exception>
		public int CopyBytes(byte[] output, int offset, int length)
		{
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(length));
			}

			if ((_bitsInBuffer & 7) != 0)
			{
				// bits_in_buffer may only be 0 or a multiple of 8
				throw new InvalidOperationException("Bit buffer is not byte aligned!");
			}

			int count = 0;
			while ((_bitsInBuffer > 0) && (length > 0))
			{
				output[offset++] = (byte)_buffer;
				_buffer >>= 8;
				_bitsInBuffer -= 8;
				length--;
				count++;
			}

			if (length == 0)
			{
				return count;
			}

			int avail = _windowEnd - _windowStart;
			if (length > avail)
			{
				length = avail;
			}
			Buffer.BlockCopy(_window, _windowStart, output, offset, length);
			_windowStart += length;

			if (((_windowStart - _windowEnd) & 1) != 0)
			{
				// We always want an even number of bytes in input, see peekBits
				_buffer = (uint)(_window[_windowStart++] & 0xff);
				_bitsInBuffer = 8;
			}
			return count + length;
		}

		/// <summary>
		/// Resets state and empties internal buffers
		/// </summary>
		public void Reset()
		{
			_buffer = 0;
			_windowStart = _windowEnd = _bitsInBuffer = 0;
		}

		/// <summary>
		/// Add more input for consumption.
		/// Only call when IsNeedingInput returns true
		/// </summary>
		/// <param name="buffer">data to be input</param>
		/// <param name="offset">offset of first byte of input</param>
		/// <param name="count">number of bytes of input to add.</param>
		public void SetInput(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset), "Cannot be negative");
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), "Cannot be negative");
			}

			if (_windowStart < _windowEnd)
			{
				throw new InvalidOperationException("Old input was not completely processed");
			}

			int end = offset + count;

			// We want to throw an ArrayIndexOutOfBoundsException early.
			// Note the check also handles integer wrap around.
			if ((offset > end) || (end > buffer.Length))
			{
				throw new ArgumentOutOfRangeException(nameof(count));
			}

			if ((count & 1) != 0)
			{
				// We always want an even number of bytes in input, see PeekBits
				_buffer |= (uint)((buffer[offset++] & 0xff) << _bitsInBuffer);
				_bitsInBuffer += 8;
			}

			_window = buffer;
			_windowStart = offset;
			_windowEnd = end;
		}

		// TODO: Introduce native memory copy overload
	}
}