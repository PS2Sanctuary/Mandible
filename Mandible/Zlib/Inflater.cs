using ICSharpCode.SharpZipLib;

namespace Mandible.Zlib
{
    /// <summary>
    /// <para>
    /// Inflater is used to decompress data that has been compressed according to the "deflate" standard described in rfc1951.
    /// </para>
    /// <para>
    /// By default Zlib (rfc1950) headers and footers are expected in the input.
    /// You can use constructor <see cref="Inflater(bool)"/> passing true
    /// if there is no Zlib header information.
    /// </para>
    /// Author of the original java version : John Leuner, Jochen Hoenicke
    /// </summary>
    /// <example>
    /// The usage is as following. First you have to set some input with <see cref="SetInput(byte[])"/>, then <see cref="Inflate(byte[])"/> it.
    /// If inflate doesn't inflate any bytes there may be three reasons:
    /// <ul>
    /// <li><see cref="IsNeedingInput"/> returns true because the input buffer is empty.
    /// You have to provide more input with <see cref="SetInput(byte[])"/>.
    /// NOTE: <see cref="IsNeedingInput"/> also returns true when, the stream is finished.</li>
    /// <li><see cref="IsNeedingDictionary"/> returns true, you have to provide a preset
    ///    dictionary with <see cref="SetDictionary(byte[])"/>.</li>
    /// <li><see cref="IsFinished"/> returns true, the inflater has finished.</li>
    /// </ul>
    /// Once the first output byte is produced, a dictionary will not be
    /// needed at a later stage.
    /// </example>
    public class Inflater
	{
		// TODO: Refactor OutputWindow

		#region Constants/Readonly

		/// <summary>
		/// Copy lengths for literal codes 257..285
		/// </summary>
		private static readonly int[] CPLENS = {
								  3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31,
								  35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258
							  };

		/// <summary>
		/// Extra bits for literal codes 257..285
		/// </summary>
		private static readonly int[] CPLEXT = {
								  0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
								  3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0
							  };

		/// <summary>
		/// Copy offsets for distance codes 0..29
		/// </summary>
		private static readonly int[] CPDIST = {
								1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193,
								257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145,
								8193, 12289, 16385, 24577
							  };

		/// <summary>
		/// Extra bits for distance codes
		/// </summary>
		private static readonly int[] CPDEXT = {
								0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6,
								7, 7, 8, 8, 9, 9, 10, 10, 11, 11,
								12, 12, 13, 13
							  };

		/// <summary>
		/// These are the possible states for an inflater
		/// </summary>
		private const int DECODE_HEADER = 0;

		private const int DECODE_DICT = 1;
		private const int DECODE_BLOCKS = 2;
		private const int DECODE_STORED_LEN1 = 3;
		private const int DECODE_STORED_LEN2 = 4;
		private const int DECODE_STORED = 5;
		private const int DECODE_DYN_HEADER = 6;
		private const int DECODE_HUFFMAN = 7;
		private const int DECODE_HUFFMAN_LENBITS = 8;
		private const int DECODE_HUFFMAN_DIST = 9;
		private const int DECODE_HUFFMAN_DISTBITS = 10;
		private const int DECODE_CHKSUM = 11;
		private const int FINISHED = 12;

		#endregion Constants/Readonly

		#region Instance Fields

		/// <summary>
		/// This variable stores the noHeader flag that was given to the constructor.
		/// True means, that the inflated stream doesn't contain a Zlib header or
		/// footer.
		/// </summary>
		private readonly bool _noHeader;
		private readonly Adler32? _adler;
		private readonly StreamManipulator _input;
		private readonly OutputWindow _outputWindow;

		/// <summary>
		/// This variable contains the current state.
		/// </summary>
		private int _mode;

		/// <summary>
		/// The adler checksum of the dictionary or of the decompressed
		/// stream, as it is written in the header resp. footer of the
		/// compressed stream.
		/// Only valid if mode is DECODE_DICT or DECODE_CHKSUM.
		/// </summary>
		private int _readAdler;

		/// <summary>
		/// The number of bits needed to complete the current state.  This
		/// is valid, if mode is DECODE_DICT, DECODE_CHKSUM,
		/// DECODE_HUFFMAN_LENBITS or DECODE_HUFFMAN_DISTBITS.
		/// </summary>
		private int _neededBits;

		private int _repLength;
		private int _repDist;
		private int _uncomprLen;

		/// <summary>
		/// True, if the last block flag was set in the last block of the
		/// inflated stream.  This means that the stream ends after the
		/// current block.
		/// </summary>
		private bool _isLastBlock;

		/// <summary>
		/// The total number of inflated bytes.
		/// </summary>
		private long _totalOut;

		/// <summary>
		/// The total number of bytes set with setInput().  This is not the
		/// value returned by the TotalIn property, since this also includes the
		/// unprocessed input.
		/// </summary>
		private long _totalIn;

		private InflaterDynHeader? _dynHeader;
		private InflaterHuffmanTree? _litlenTree, _distTree;

		#endregion Instance Fields

		/// <summary>
		/// Returns true, if the input buffer is empty.
		/// You should then call setInput().
		/// NOTE: This method also returns true when the stream is finished.
		/// </summary>
		public bool IsNeedingInput => _input.IsNeedingInput;

		/// <summary>
		/// Returns true, if a preset dictionary is needed to inflate the input.
		/// </summary>
		public bool IsNeedingDictionary => _mode == DECODE_DICT && _neededBits == 0;

		/// <summary>
		/// Returns true, if the inflater has finished.  This means, that no
		/// input is needed and no output can be produced.
		/// </summary>
		public bool IsFinished => _mode == FINISHED && _outputWindow.GetAvailable() == 0;

		/// <summary>
		/// Gets the adler checksum.  This is either the checksum of all
		/// uncompressed bytes returned by inflate(), or if needsDictionary()
		/// returns true (and thus no output was yet produced) this is the
		/// adler checksum of the expected dictionary.
		/// </summary>
		public int Adler
		{
			get
			{
				if (IsNeedingDictionary)
				{
					return _readAdler;
				}
				else if (_adler != null)
				{
					return (int)_adler.Value;
				}
				else
				{
					return 0;
				}
			}
		}

		/// <summary>
		/// Gets the total number of output bytes returned by Inflate().
		/// </summary>
		public long TotalOut => _totalOut;

		/// <summary>
		/// Gets the total number of processed compressed input bytes.
		/// </summary>
		public long TotalIn => _totalIn - RemainingInput;

		/// <summary>
		/// Gets the number of unprocessed input bytes.  Useful, if the end of the
		/// stream is reached and you want to further process the bytes after
		/// the deflate stream.
		/// </summary>
		public int RemainingInput => _input.AvailableBytes;

		#region Constructors

		/// <summary>
		/// Creates a new inflater or RFC1951 decompressor
		/// RFC1950/Zlib headers and footers will be expected in the input data
		/// </summary>
		public Inflater()
			: this(false)
		{
		}

		/// <summary>
		/// Creates a new inflater.
		/// </summary>
		/// <param name="noHeader">
		/// True if no RFC1950/Zlib header and footer fields are expected in the input data
		///
		/// This is used for GZIPed/Zipped input.
		///
		/// For compatibility with
		/// Sun JDK you should provide one byte of input more than needed in
		/// this case.
		/// </param>
		public Inflater(bool noHeader)
		{
			_noHeader = noHeader;
			if (!noHeader)
				_adler = new Adler32();

			_input = new StreamManipulator();
			_outputWindow = new OutputWindow();
			_mode = noHeader ? DECODE_BLOCKS : DECODE_HEADER;
		}

		#endregion Constructors

		/// <summary>
		/// Resets the inflater so that a new stream can be decompressed.  All
		/// pending input and output will be discarded.
		/// </summary>
		public void Reset()
		{
			_mode = _noHeader ? DECODE_BLOCKS : DECODE_HEADER;
			_totalIn = 0;
			_totalOut = 0;
			_input.Reset();
			_outputWindow.Reset();
			_dynHeader = null;
			_litlenTree = null;
			_distTree = null;
			_isLastBlock = false;
			_adler?.Reset();
		}

		/// <summary>
		/// Decodes a zlib/RFC1950 header.
		/// </summary>
		/// <returns>
		/// False if more input is needed.
		/// </returns>
		/// <exception cref="SharpZipBaseException">
		/// The header is invalid.
		/// </exception>
		private bool DecodeHeader()
		{
			int header = _input.PeekBits(16);
			if (header < 0)
			{
				return false;
			}
			_input.DropBits(16);

			// The header is written in "wrong" byte order
			header = ((header << 8) | (header >> 8)) & 0xffff;
			if (header % 31 != 0)
			{
				throw new SharpZipBaseException("Header checksum illegal");
			}

			if ((header & 0x0f00) != (ICSharpCode.SharpZipLib.Zip.Compression.Deflater.DEFLATED << 8))
			{
				throw new SharpZipBaseException("Compression Method unknown");
			}

			/* Maximum size of the backwards window in bits.
			* We currently ignore this, but we could use it to make the
			* inflater window more space efficient. On the other hand the
			* full window (15 bits) is needed most times, anyway.
			int max_wbits = ((header & 0x7000) >> 12) + 8;
			*/

			if ((header & 0x0020) == 0)
			{ // Dictionary flag?
				_mode = DECODE_BLOCKS;
			}
			else
			{
				_mode = DECODE_DICT;
				_neededBits = 32;
			}
			return true;
		}

		/// <summary>
		/// Decodes the dictionary checksum after the deflate header.
		/// </summary>
		/// <returns>
		/// False if more input is needed.
		/// </returns>
		private bool DecodeDict()
		{
			while (_neededBits > 0)
			{
				int dictByte = _input.PeekBits(8);
				if (dictByte < 0)
				{
					return false;
				}
				_input.DropBits(8);
				_readAdler = (_readAdler << 8) | dictByte;
				_neededBits -= 8;
			}
			return false;
		}

		/// <summary>
		/// Decodes the huffman encoded symbols in the input stream.
		/// </summary>
		/// <returns>
		/// false if more input is needed, true if output window is
		/// full or the current block ends.
		/// </returns>
		/// <exception cref="SharpZipBaseException">
		/// if deflated stream is invalid.
		/// </exception>
		private bool DecodeHuffman()
		{
			int free = _outputWindow.GetFreeSpace();
			while (free >= 258)
			{
				int symbol;
				switch (_mode)
				{
					case DECODE_HUFFMAN:
						// This is the inner loop so it is optimized a bit
						while (((symbol = _litlenTree!.GetSymbol(_input)) & ~0xff) == 0)
						{
							_outputWindow.Write(symbol);
							if (--free < 258)
							{
								return true;
							}
						}

						if (symbol < 257)
						{
							if (symbol < 0)
							{
								return false;
							}
							else
							{
								// symbol == 256: end of block
								_distTree = null;
								_litlenTree = null;
								_mode = DECODE_BLOCKS;
								return true;
							}
						}

						try
						{
							_repLength = CPLENS[symbol - 257];
							_neededBits = CPLEXT[symbol - 257];
						}
						catch (Exception)
						{
							throw new SharpZipBaseException("Illegal rep length code");
						}
						goto case DECODE_HUFFMAN_LENBITS; // fall through

					case DECODE_HUFFMAN_LENBITS:
						if (_neededBits > 0)
						{
							_mode = DECODE_HUFFMAN_LENBITS;
							int i = _input.PeekBits(_neededBits);
							if (i < 0)
							{
								return false;
							}
							_input.DropBits(_neededBits);
							_repLength += i;
						}
						_mode = DECODE_HUFFMAN_DIST;
						goto case DECODE_HUFFMAN_DIST; // fall through

					case DECODE_HUFFMAN_DIST:
						symbol = _distTree!.GetSymbol(_input);
						if (symbol < 0)
						{
							return false;
						}

						try
						{
							_repDist = CPDIST[symbol];
							_neededBits = CPDEXT[symbol];
						}
						catch (Exception)
						{
							throw new SharpZipBaseException("Illegal rep dist code");
						}

						goto case DECODE_HUFFMAN_DISTBITS; // fall through

					case DECODE_HUFFMAN_DISTBITS:
						if (_neededBits > 0)
						{
							_mode = DECODE_HUFFMAN_DISTBITS;
							int i = _input.PeekBits(_neededBits);
							if (i < 0)
							{
								return false;
							}
							_input.DropBits(_neededBits);
							_repDist += i;
						}

						_outputWindow.Repeat(_repLength, _repDist);
						free -= _repLength;
						_mode = DECODE_HUFFMAN;
						break;

					default:
						throw new SharpZipBaseException("Inflater unknown mode");
				}
			}
			return true;
		}

		/// <summary>
		/// Decodes the adler checksum after the deflate stream.
		/// </summary>
		/// <returns>
		/// false if more input is needed.
		/// </returns>
		/// <exception cref="SharpZipBaseException">
		/// If checksum doesn't match.
		/// </exception>
		private bool DecodeChksum()
		{
			while (_neededBits > 0)
			{
				int chkByte = _input.PeekBits(8);
				if (chkByte < 0)
				{
					return false;
				}
				_input.DropBits(8);
				_readAdler = (_readAdler << 8) | chkByte;
				_neededBits -= 8;
			}

			if (_adler is not null && (int)_adler.Value != _readAdler)
			{
				throw new SharpZipBaseException("Adler chksum doesn't match: " + (int)_adler.Value + " vs. " + _readAdler);
			}

			_mode = FINISHED;
			return false;
		}

		/// <summary>
		/// Decodes the deflated stream.
		/// </summary>
		/// <returns>
		/// false if more input is needed, or if finished.
		/// </returns>
		/// <exception cref="SharpZipBaseException">
		/// if deflated stream is invalid.
		/// </exception>
		private bool Decode()
		{
			switch (_mode)
			{
				case DECODE_HEADER:
					return DecodeHeader();

				case DECODE_DICT:
					return DecodeDict();

				case DECODE_CHKSUM:
					return DecodeChksum();

				case DECODE_BLOCKS:
					if (_isLastBlock)
					{
						if (_noHeader)
						{
							_mode = FINISHED;
							return false;
						}
						else
						{
							_input.SkipToByteBoundary();
							_neededBits = 32;
							_mode = DECODE_CHKSUM;
							return true;
						}
					}

					int type = _input.PeekBits(3);
					if (type < 0)
					{
						return false;
					}
					_input.DropBits(3);

					_isLastBlock |= (type & 1) != 0;
					switch (type >> 1)
					{
						case ICSharpCode.SharpZipLib.Zip.Compression.DeflaterConstants.STORED_BLOCK:
							_input.SkipToByteBoundary();
							_mode = DECODE_STORED_LEN1;
							break;

						case ICSharpCode.SharpZipLib.Zip.Compression.DeflaterConstants.STATIC_TREES:
							_litlenTree = InflaterHuffmanTree.DefLitLenTree;
							_distTree = InflaterHuffmanTree.DefDistTree;
							_mode = DECODE_HUFFMAN;
							break;

						case ICSharpCode.SharpZipLib.Zip.Compression.DeflaterConstants.DYN_TREES:
							_dynHeader = new InflaterDynHeader(_input);
							_mode = DECODE_DYN_HEADER;
							break;

						default:
							throw new SharpZipBaseException("Unknown block type " + type);
					}
					return true;

				case DECODE_STORED_LEN1:
					{
						if ((_uncomprLen = _input.PeekBits(16)) < 0)
						{
							return false;
						}
						_input.DropBits(16);
						_mode = DECODE_STORED_LEN2;
					}
					goto case DECODE_STORED_LEN2; // fall through

				case DECODE_STORED_LEN2:
					{
						int nlen = _input.PeekBits(16);
						if (nlen < 0)
						{
							return false;
						}
						_input.DropBits(16);
						if (nlen != (_uncomprLen ^ 0xffff))
						{
							throw new SharpZipBaseException("broken uncompressed block");
						}
						_mode = DECODE_STORED;
					}
					goto case DECODE_STORED; // fall through

				case DECODE_STORED:
					{
						int more = _outputWindow.CopyStored(_input, _uncomprLen);
						_uncomprLen -= more;
						if (_uncomprLen == 0)
						{
							_mode = DECODE_BLOCKS;
							return true;
						}
						return !_input.IsNeedingInput;
					}

				case DECODE_DYN_HEADER:
					if (_dynHeader?.AttemptRead() != true)
					{
						return false;
					}

					_litlenTree = _dynHeader.LiteralLengthTree;
					_distTree = _dynHeader.DistanceTree;
					_mode = DECODE_HUFFMAN;
					goto case DECODE_HUFFMAN; // fall through

				case DECODE_HUFFMAN:
				case DECODE_HUFFMAN_LENBITS:
				case DECODE_HUFFMAN_DIST:
				case DECODE_HUFFMAN_DISTBITS:
					return DecodeHuffman();

				case FINISHED:
					return false;

				default:
					throw new SharpZipBaseException("Inflater.Decode unknown mode");
			}
		}

		/// <summary>
		/// Sets the preset dictionary.  This should only be called, if
		/// needsDictionary() returns true and it should set the same
		/// dictionary, that was used for deflating.  The getAdler()
		/// function returns the checksum of the dictionary needed.
		/// </summary>
		/// <param name="buffer">
		/// The dictionary.
		/// </param>
		/// <exception cref="System.InvalidOperationException">
		/// No dictionary is needed.
		/// </exception>
		/// <exception cref="SharpZipBaseException">
		/// The adler checksum for the buffer is invalid
		/// </exception>
		public void SetDictionary(ReadOnlySpan<byte> buffer)
		{
			if (!IsNeedingDictionary)
			{
				throw new InvalidOperationException("Dictionary is not needed");
			}

			_adler?.Update(buffer);

			if (_adler != null && (int)_adler.Value != _readAdler)
			{
				throw new SharpZipBaseException("Wrong adler checksum");
			}
			_adler?.Reset();
			_outputWindow.CopyDict(buffer.ToArray(), 0, buffer.Length);
			_mode = DECODE_BLOCKS;
		}

		/// <summary>
		/// Sets the input.  This should only be called, if needsInput()
		/// returns true.
		/// </summary>
		/// <param name="buffer">
		/// the input.
		/// </param>
		public void SetInput(byte[] buffer)
		{
			SetInput(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Sets the input.  This should only be called, if needsInput()
		/// returns true.
		/// </summary>
		/// <param name="buffer">
		/// The source of input data
		/// </param>
		/// <param name="index">
		/// The index into buffer where the input starts.
		/// </param>
		/// <param name="count">
		/// The number of bytes of input to use.
		/// </param>
		/// <exception cref="System.InvalidOperationException">
		/// No input is needed.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// The index and/or count are wrong.
		/// </exception>
		public void SetInput(byte[] buffer, int index, int count)
		{
			_input.SetInput(buffer, index, count);
			_totalIn += count;
		}

		/// <summary>
		/// Inflates the compressed stream to the output buffer.  If this
		/// returns 0, you should check, whether IsNeedingDictionary(),
		/// IsNeedingInput() or IsFinished() returns true, to determine why no
		/// further output is produced.
		/// </summary>
		/// <param name="buffer">
		/// the output buffer.
		/// </param>
		/// <returns>
		/// The number of bytes written to the buffer, 0 if no further
		/// output can be produced.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// if buffer has length 0.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// if deflated stream is invalid.
		/// </exception>
		public int Inflate(byte[] buffer)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			return Inflate(buffer, 0, buffer.Length);
		}

		/// <summary>
		/// Inflates the compressed stream to the output buffer.  If this
		/// returns 0, you should check, whether needsDictionary(),
		/// needsInput() or finished() returns true, to determine why no
		/// further output is produced.
		/// </summary>
		/// <param name="buffer">
		/// the output buffer.
		/// </param>
		/// <param name="offset">
		/// the offset in buffer where storing starts.
		/// </param>
		/// <param name="count">
		/// the maximum number of bytes to output.
		/// </param>
		/// <returns>
		/// the number of bytes written to the buffer, 0 if no further output can be produced.
		/// </returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// if count is less than 0.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// if the index and / or count are wrong.
		/// </exception>
		/// <exception cref="System.FormatException">
		/// if deflated stream is invalid.
		/// </exception>
		public int Inflate(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative");
			}

			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(offset), "offset cannot be negative");
			}

			if (offset + count > buffer.Length)
			{
				throw new ArgumentException("count exceeds buffer bounds");
			}

			// Special case: count may be zero
			if (count == 0)
			{
				if (!IsFinished)
				{ // -jr- 08-Nov-2003 INFLATE_BUG fix..
					Decode();
				}
				return 0;
			}

			int bytesCopied = 0;

			do
			{
				if (_mode != DECODE_CHKSUM)
				{
					/* Don't give away any output, if we are waiting for the
					* checksum in the input stream.
					*
					* With this trick we have always:
					*   IsNeedingInput() and not IsFinished()
					*   implies more output can be produced.
					*/
					int more = _outputWindow.CopyOutput(buffer, offset, count);
					if (more > 0)
					{
						_adler?.Update(buffer.AsSpan(offset, count));
						offset += more;
						bytesCopied += more;
						_totalOut += (long)more;
						count -= more;
						if (count == 0)
						{
							return bytesCopied;
						}
					}
				}
			} while (Decode() || ((_outputWindow.GetAvailable() > 0) && (_mode != DECODE_CHKSUM)));
			return bytesCopied;
		}
	}
}