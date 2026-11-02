using System;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace BenScr.Names;

internal sealed class SeededIndex(ulong seed)
{
    private ulong state = seed;

    internal static ulong CreateSeed()
    {
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        return BinaryPrimitives.ReadUInt64LittleEndian(bytes);
    }

    internal ulong NextUInt64()
    {
        unchecked
        {
            state += 0x9E3779B97F4A7C15UL;
            ulong value = state;
            value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
            value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
            return value ^ (value >> 31);
        }
    }

    internal int Next(int count)
    {
        ulong bound = (ulong)count;
        ulong threshold = unchecked(0UL - bound) % bound;
        ulong value;
        do
        {
            value = NextUInt64();
        } while (value < threshold);
        return (int)(value % bound);
    }
}
