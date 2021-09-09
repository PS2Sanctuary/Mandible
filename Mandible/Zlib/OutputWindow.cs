namespace Mandible.Zlib
{
    /// <summary>
    /// Contains the output from the Inflation process.
    /// We need to have a window so that we can refer backwards into the output stream
    /// to repeat stuff.<br/>
    /// Author of the original java version : John Leuner
    /// </summary>
    public class OutputWindow
	{
		#region Constants

		private const int WINDOW_SIZE = 1 << 15;
		private const int WINDOW_MASK = WINDOW_SIZE - 1;

		#endregion Constants

		#region Instance Fields

		private readonly byte[] _window = new byte[WINDOW_SIZE];
		private int _windowEnd;
		private int _windowFilled;

		#endregion Instance Fields

		/// <summary>
		/// Write a byte to this output window
		/// </summary>
		/// <param name="value">value to write</param>
		/// <exception cref="InvalidOperationException">
		/// if window is full
		/// </exception>
		public void Write(int value)
		{
			if (_windowFilled++ == WINDOW_SIZE)
			{
				throw new InvalidOperationException("Window full");
			}
			_window[_windowEnd++] = (byte)value;
			_windowEnd &= WINDOW_MASK;
		}

		/// <summary>
		/// Append a byte pattern already in the window itself
		/// </summary>
		/// <param name="length">length of pattern to copy</param>
		/// <param name="distance">distance from end of window pattern occurs</param>
		/// <exception cref="InvalidOperationException">
		/// If the repeated data overflows the window
		/// </exception>
		public void Repeat(int length, int distance)
		{
			if ((_windowFilled += length) > WINDOW_SIZE)
			{
				throw new InvalidOperationException("Window full");
			}

			int repStart = (_windowEnd - distance) & WINDOW_MASK;
			int border = WINDOW_SIZE - length;
			if ((repStart <= border) && (_windowEnd < border))
			{
				if (length <= distance)
				{
					Buffer.BlockCopy(_window, repStart, _window, _windowEnd, length);
					_windowEnd += length;
				}
				else
				{
					// We have to copy manually, since the repeat pattern overlaps.
					while (length-- > 0)
					{
						_window[_windowEnd++] = _window[repStart++];
					}
				}
			}
			else
			{
				SlowRepeat(repStart, length, distance);
			}
		}

		/// <summary>
		/// Copy from input manipulator to internal window
		/// </summary>
		/// <param name="input">source of data</param>
		/// <param name="length">length of data to copy</param>
		/// <returns>the number of bytes copied</returns>
		public int CopyStored(StreamManipulator input, int length)
		{
			length = Math.Min(Math.Min(length, WINDOW_SIZE - _windowFilled), input.AvailableBytes);
			int copied;

			int tailLen = WINDOW_SIZE - _windowEnd;
			if (length > tailLen)
			{
				copied = input.CopyBytes(_window, _windowEnd, tailLen);
				if (copied == tailLen)
				{
					copied += input.CopyBytes(_window, 0, length - tailLen);
				}
			}
			else
			{
				copied = input.CopyBytes(_window, _windowEnd, length);
			}

			_windowEnd = (_windowEnd + copied) & WINDOW_MASK;
			_windowFilled += copied;
			return copied;
		}

		/// <summary>
		/// Copy dictionary to window
		/// </summary>
		/// <param name="dictionary">source dictionary</param>
		/// <param name="offset">offset of start in source dictionary</param>
		/// <param name="length">length of dictionary</param>
		/// <exception cref="InvalidOperationException">
		/// If window isnt empty
		/// </exception>
		public void CopyDict(byte[] dictionary, int offset, int length)
		{
			if (dictionary == null)
			{
				throw new ArgumentNullException(nameof(dictionary));
			}

			if (_windowFilled > 0)
			{
				throw new InvalidOperationException();
			}

			if (length > WINDOW_SIZE)
			{
				offset += length - WINDOW_SIZE;
				length = WINDOW_SIZE;
			}
			Buffer.BlockCopy(dictionary, offset, _window, 0, length);
			_windowEnd = length & WINDOW_MASK;
		}

		// TODO: Introduce Buffer.MemoryCopy span overload, and utilise in the Inflater.SetDictionary

		/// <summary>
		/// Get remaining unfilled space in window
		/// </summary>
		/// <returns>Number of bytes left in window</returns>
		public int GetFreeSpace()
		{
			return WINDOW_SIZE - _windowFilled;
		}

		/// <summary>
		/// Get bytes available for output in window
		/// </summary>
		/// <returns>Number of bytes filled</returns>
		public int GetAvailable()
		{
			return _windowFilled;
		}

		/// <summary>
		/// Copy contents of window to output
		/// </summary>
		/// <param name="output">buffer to copy to</param>
		/// <param name="offset">offset to start at</param>
		/// <param name="len">number of bytes to count</param>
		/// <returns>The number of bytes copied</returns>
		/// <exception cref="InvalidOperationException">
		/// If a window underflow occurs
		/// </exception>
		public int CopyOutput(byte[] output, int offset, int len)
		{
			int copyEnd = _windowEnd;
			if (len > _windowFilled)
			{
				len = _windowFilled;
			}
			else
			{
				copyEnd = (_windowEnd - _windowFilled + len) & WINDOW_MASK;
			}

			int copied = len;
			int tailLen = len - copyEnd;

			if (tailLen > 0)
			{
				Buffer.BlockCopy(_window, WINDOW_SIZE - tailLen, output, offset, tailLen);
				offset += tailLen;
				len = copyEnd;
			}
			Buffer.BlockCopy(_window, copyEnd - len, output, offset, len);
			_windowFilled -= copied;
			if (_windowFilled < 0)
			{
				throw new InvalidOperationException();
			}
			return copied;
		}
		// TODO: Test with introducing native mem copy

		/// <summary>
		/// Reset by clearing window so <see cref="GetAvailable">GetAvailable</see> returns 0
		/// </summary>
		public void Reset()
		{
			_windowFilled = _windowEnd = 0;
		}

		private void SlowRepeat(int repStart, int length, int distance)
		{
			while (length-- > 0)
			{
				_window[_windowEnd++] = _window[repStart++];
				_windowEnd &= WINDOW_MASK;
				repStart &= WINDOW_MASK;
			}
		}
	}
}