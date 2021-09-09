﻿using ICSharpCode.SharpZipLib;
using System.Diagnostics.CodeAnalysis;

namespace Mandible.Zlib
{
    internal class InflaterDynHeader
	{
		#region Constants

		// maximum number of literal/length codes
		private const int LITLEN_MAX = 286;

		// maximum number of distance codes
		private const int DIST_MAX = 30;

		// maximum data code lengths to read
		private const int CODELEN_MAX = LITLEN_MAX + DIST_MAX;

		// maximum meta code length codes to read
		private const int META_MAX = 19;

		private static readonly int[] META_CODE_LENGTH_INDEX =
			{ 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };

		#endregion Constants

		#region Instance Fields

		private readonly StreamManipulator _input;
		private readonly IEnumerator<bool> _state;
		private readonly IEnumerable<bool> _stateMachine;

		private byte[] _codeLengths = new byte[CODELEN_MAX];

		private InflaterHuffmanTree _litLenTree;
		private InflaterHuffmanTree _distTree;

		private int _litLenCodeCount, _distanceCodeCount, _metaCodeCount;

		#endregion Instance Fields

		public InflaterDynHeader(StreamManipulator input)
		{
			_input = input;
			_stateMachine = CreateStateMachine();
			_state = _stateMachine.GetEnumerator();
		}

		/// <summary>
		/// Continue decoding header from <see cref="_input"/> until more bits are needed or decoding has been completed
		/// </summary>
		/// <returns>Returns whether decoding could be completed</returns>
		public bool AttemptRead()
			=> !_state.MoveNext() || _state.Current;

		/// <summary>
		/// Get literal/length huffman tree, must not be used before <see cref="AttemptRead"/> has returned true
		/// </summary>
		/// <exception cref="StreamDecodingException">If hader has not been successfully read by the state machine</exception>
		public InflaterHuffmanTree LiteralLengthTree
			=> _litLenTree ?? throw new StreamDecodingException("Header properties were accessed before header had been successfully read");

		/// <summary>
		/// Get distance huffman tree, must not be used before <see cref="AttemptRead"/> has returned true
		/// </summary>
		/// <exception cref="StreamDecodingException">If hader has not been successfully read by the state machine</exception>
		public InflaterHuffmanTree DistanceTree
			=> _distTree ?? throw new StreamDecodingException("Header properties were accessed before header had been successfully read");

		[MemberNotNull(nameof(_litLenTree), nameof(_distTree))]
		private IEnumerable<bool> CreateStateMachine()
		{
			// Read initial code length counts from header
			while (!_input.TryGetBits(5, ref _litLenCodeCount, 257)) yield return false;
			while (!_input.TryGetBits(5, ref _distanceCodeCount, 1)) yield return false;
			while (!_input.TryGetBits(4, ref _metaCodeCount, 4)) yield return false;
			var dataCodeCount = _litLenCodeCount + _distanceCodeCount;

			if (_litLenCodeCount > LITLEN_MAX) throw new ValueOutOfRangeException(nameof(_litLenCodeCount));
			if (_distanceCodeCount > DIST_MAX) throw new ValueOutOfRangeException(nameof(_distanceCodeCount));
			if (_metaCodeCount > META_MAX) throw new ValueOutOfRangeException(nameof(_metaCodeCount));

			// Load code lengths for the meta tree from the header bits
			for (int i = 0; i < _metaCodeCount; i++)
			{
				while (!_input.TryGetBits(3, ref _codeLengths, META_CODE_LENGTH_INDEX[i])) yield return false;
			}

			var metaCodeTree = new InflaterHuffmanTree(_codeLengths);

			// Decompress the meta tree symbols into the data table code lengths
			int index = 0;
			while (index < dataCodeCount)
			{
				byte codeLength;
				int symbol;

				while ((symbol = metaCodeTree.GetSymbol(_input)) < 0) yield return false;

				if (symbol < 16)
				{
					// append literal code length
					_codeLengths[index++] = (byte)symbol;
				}
				else
				{
					int repeatCount = 0;

					if (symbol == 16) // Repeat last code length 3..6 times
					{
						if (index == 0)
							throw new StreamDecodingException("Cannot repeat previous code length when no other code length has been read");

						codeLength = _codeLengths[index - 1];

						// 2 bits + 3, [3..6]
						while (!_input.TryGetBits(2, ref repeatCount, 3)) yield return false;
					}
					else if (symbol == 17) // Repeat zero 3..10 times
					{
						codeLength = 0;

						// 3 bits + 3, [3..10]
						while (!_input.TryGetBits(3, ref repeatCount, 3)) yield return false;
					}
					else // (symbol == 18), Repeat zero 11..138 times
					{
						codeLength = 0;

						// 7 bits + 11, [11..138]
						while (!_input.TryGetBits(7, ref repeatCount, 11)) yield return false;
					}

					if (index + repeatCount > dataCodeCount)
						throw new StreamDecodingException("Cannot repeat code lengths past total number of data code lengths");

					while (repeatCount-- > 0)
						_codeLengths[index++] = codeLength;
				}
			}

			if (_codeLengths[256] == 0)
				throw new StreamDecodingException("Inflater dynamic header end-of-block code missing");

			_litLenTree = new InflaterHuffmanTree(new ArraySegment<byte>(_codeLengths, 0, _litLenCodeCount));
			_distTree = new InflaterHuffmanTree(new ArraySegment<byte>(_codeLengths, _litLenCodeCount, _distanceCodeCount));

			yield return true;
		}
	}
}