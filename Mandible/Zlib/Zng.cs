using System;
using System.Runtime.InteropServices;

namespace Mandible.Zlib
{
    public sealed unsafe partial class Zng
    {
        /// <summary>
        /// Gets the major version of zlib-ng that this wrapper was based on.
        /// </summary>
        public const byte ZLIBNG_VER_MAJOR = (byte)'2';

        /// <summary>
        /// Gets the name of the DLL that contains the unmanaged zlib-ng implementation.
        /// </summary>
        public const string LibraryName = "zlib-ng2.dll";

        [DllImport(LibraryName, EntryPoint = "zlibng_version")]
        internal static extern byte* _Version();
    }

    public sealed unsafe partial class Zng
    {
        public static string? Version()
            => Marshal.PtrToStringAnsi((IntPtr)_Version());

        public static void ThrowIfInvalidVersion()
        {
            if (*_Version() != ZLIBNG_VER_MAJOR)
                throw new ZngVersionException(ZLIBNG_VER_MAJOR, Version());
        }
    }
}
