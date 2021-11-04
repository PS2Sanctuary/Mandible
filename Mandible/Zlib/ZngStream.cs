using System;
using System.Runtime.InteropServices;

namespace Mandible.Zlib
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct InternalState
    {
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ZngStream
    {
        /// <summary>
        /// The next input byte.
        /// </summary>
        public byte* NextIn;

        /// <summary>
        /// The number of bytes available at <see cref="NextIn"/>.
        /// </summary>
        public uint AvailableIn;

        /// <summary>
        /// The total number of bytes read so far.
        /// </summary>
        public UIntPtr TotalIn;

        /// <summary>
        /// The next output byte.
        /// </summary>
        public byte* NextOut;

        /// <summary>
        /// Remaining free space at <see cref="NextOut"/>
        /// </summary>
        public uint AvailableOut;

        /// <summary>
        /// Total number of bytes output so far.
        /// </summary>
        public UIntPtr TotalOut;

        /// <summary>
        /// The last error message, or null if not applicable.
        /// </summary>
        public byte* ErrorMessage;

        /// <summary>
        /// Not visible by applications.
        /// </summary>
        public InternalState* State;

        /// <summary>
        /// Use to allocate the internal state.
        /// IntPtr opaque, uint items, uint size
        /// </summary>
        public delegate* unmanaged<IntPtr, uint, uint, IntPtr> AllocationFunction;

        /// <summary>
        /// Used to free the internal state.
        /// IntPtr opaque, Intptr address
        /// </summary>
        public delegate* unmanaged<IntPtr, IntPtr, void> FreeFunction;

        /// <summary>
        /// Private data object passed to <see cref="FreeFunction"/> and <see cref="AllocationFunction"/>.
        /// </summary>
        public IntPtr Opaque;

        /// <summary>
        /// Best guess about the data type, or the decoding state for inflate.
        /// </summary>
        public DeflateDataType DataType;

        /// <summary>
        /// Adler-32 or CRC-32 value of the uncompressed data.
        /// </summary>
        public uint Adler;

        /// <summary>
        /// Reserved for future use.
        /// </summary>
        public ulong Reserved;
    }
}
