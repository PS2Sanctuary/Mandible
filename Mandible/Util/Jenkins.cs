using System;
using System.Runtime.CompilerServices;

namespace Mandible.Util;

/// <summary>
/// Contains functions to calculate hashes using
/// algorithms designed by Bob Jenkins.
/// </summary>
public static class Jenkins
{
    /// <summary>
    /// ForgeLight specific - gets the locale ID of an item/vehicle.
    /// </summary>
    /// <param name="itemNameID">The NAME ID of the item/vehicle.</param>
    /// <returns>The calculated locale ID.</returns>
    public static uint GetItemLocaleID(uint itemNameID)
        => Lookup2("Global.Text." + itemNameID);

    /// <summary>
    /// Performs a one-at-a-time hash on the input characters.
    /// </summary>
    /// <param name="data">The character array to hash.</param>
    /// <returns>The hash.</returns>
    public static uint OneAtATime(ReadOnlySpan<char> data)
    {
        // Note: PS2LS uses a signed int here. Might be a ForgeLight thing?
        uint hash = 0;

        foreach (char c in data)
        {
            hash += c;
            hash += hash << 10;
            hash ^= hash >> 6;
        }

        hash += hash << 3;
        hash ^= hash >> 11;
        hash += hash << 15;

        return hash;
    }

    /// <summary>
    /// Performs a lookup2 hash on the input characters.
    /// <see href="http://burtleburtle.net/bob/c/lookup2.c"/>
    /// </summary>
    /// <param name="data">The character array to hash.</param>
    /// <returns>The hash.</returns>
    public static uint Lookup2(ReadOnlySpan<char> data)
    {
        /*
        --------------------------------------------------------------------
        Returns a 32-bit value.  Every bit of the key affects every bit of
        the return value.  Every 1-bit and 2-bit delta achieves avalanche.
        About 36+6len instructions.
        The best hash table sizes are powers of 2.  There is no need to do
        mod a prime (mod is sooo slow!).  If you need less than 32 bits,
        use a bitmask.  For example, if you need only 10 bits, do
            h = (h & hashmask(10));
        In which case, the hash table should have hashsize(10) elements.
        If you are hashing n strings (ub1 **)k, do it like this:
            for (i=0, h=0; i<n; ++i) h = hash( k[i], len[i], h);
        By Bob Jenkins, 1996.  bob_jenkins@burtleburtle.net.  You may use this
        code any way you wish, private, educational, or commercial.  It's free.
        See http://burtleburtle.net/bob/hash/evahash.html
        Use for hash table lookup, or anything where one collision in 2^32 is
        acceptable.  Do NOT use for cryptographic purposes.
        --------------------------------------------------------------------
        */

        const uint initval = 0;
        uint lenpos = (uint)data.Length;
        uint length = lenpos;

        if (length == 0)
            return 0;

        // Set up the internal state
        uint a = 0x9e3779b9;
        uint b = 0x9e3779b9;
        uint c = initval;
        int p = 0;

        // ---------------------------------------- handle most of the key
        while (lenpos >= 12)
        {
            a += Ord(data[p + 0]) + (Ord(data[p + 1]) << 8) + (Ord(data[p + 2]) << 16) + (Ord(data[p + 3]) << 24);
            b += Ord(data[p + 4]) + (Ord(data[p + 5]) << 8) + (Ord(data[p + 6]) << 16) + (Ord(data[p + 7]) << 24);
            c += Ord(data[p + 8]) + (Ord(data[p + 9]) << 8) + (Ord(data[p + 10]) << 16) + (Ord(data[p + 11]) << 24);

            (a, b, c) = Mix(a, b, c);
            p += 12;
            lenpos -= 12;
        }

        // ------------------------- handle the last 11 bytes
        c += length;
        if (lenpos >= 11) c += Ord(data[p + 10]) << 24;
        if (lenpos >= 10) c += Ord(data[p + 9]) << 16;
        if (lenpos >= 9) c += Ord(data[p + 8]) << 8;
        // the first byte of c is reserved for the length
        if (lenpos >= 8) b += Ord(data[p + 7]) << 24;
        if (lenpos >= 7) b += Ord(data[p + 6]) << 16;
        if (lenpos >= 6) b += Ord(data[p + 5]) << 8;
        if (lenpos >= 5) b += Ord(data[p + 4]);
        if (lenpos >= 4) a += Ord(data[p + 3]) << 24;
        if (lenpos >= 3) a += Ord(data[p + 2]) << 16;
        if (lenpos >= 2) a += Ord(data[p + 1]) << 8;
        if (lenpos >= 1) a += Ord(data[p + 0]);

        (_, _, c) = Mix(a, b, c);
        return c >> 0;
    }

    private static (uint A, uint B, uint C) Mix(uint a, uint b, uint c)
    {
        /*
        --------------------------------------------------------------------
        mix -- mix 3 32-bit values reversibly.
        For every delta with one or two bit set, and the deltas of all three
            high bits or all three low bits, whether the original value of a,b,c
            is almost all zero or is uniformly distributed,
        * If mix() is run forward or backward, at least 32 bits in a,b,c
            have at least 1/4 probability of changing.
        * If mix() is run forward, every bit of c will change between 1/3 and
            2/3 of the time.  (Well, 22/100 and 78/100 for some 2-bit deltas.)
        mix() was built out of 36 single-cycle latency instructions in a 
            structure that could supported 2x parallelism, like so:
                a -= b; 
                a -= c; x = (c>>13);
                b -= c; a ^= x;
                b -= a; x = (a<<8);
                c -= a; b ^= x;
                c -= b; x = (b>>13);
                ...
            Unfortunately, superscalar Pentiums and Sparcs can't take advantage 
            of that parallelism.  They've also turned some of those single-cycle
            latency instructions into multi-cycle latency instructions.  Still,
            this is the fastest good hash I could find.  There were about 2^^68
            to choose from.  I only looked at a billion or so.
        --------------------------------------------------------------------
        */

        a >>= 0;
        b >>= 0;
        c >>= 0;

        a -= b; a -= c; a ^= (c >> 13); a >>= 0;
        b -= c; b -= a; b ^= (a << 8); b >>= 0;
        c -= a; c -= b; c ^= (b >> 13); c >>= 0;

        a -= b; a -= c; a ^= (c >> 12); a >>= 0;
        b -= c; b -= a; b ^= (a << 16); b >>= 0;
        c -= a; c -= b; c ^= (b >> 5); c >>= 0;

        a -= b; a -= c; a ^= (c >> 3); a >>= 0;
        b -= c; b -= a; b ^= (a << 10); b >>= 0;
        c -= a; c -= b; c ^= (b >> 15); c >>= 0;

        return (a, b, c);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Ord(char c)
        => c;
}
