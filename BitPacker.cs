// MIT License
// Copyright (c) 2025 Samuel Nchinda

using System;
using System.Runtime.CompilerServices;


namespace TeamRadiance.Data;
public static class BitPacker
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static uint Pack(long value, int offset, int bits, uint currentPacked)
    {
        uint mask = (uint)((1L << bits) - 1);
        return currentPacked | (((uint)value & mask) << offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Pack(long value, int offset, int bits, ulong currentPacked)
    {
        ulong mask = (ulong)((1L << bits) - 1);
        return currentPacked | (((ulong)value & mask) << offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Pack(long value, int offset, int bits, long currentPacked)
    {
        long mask = (1L << bits) - 1;
        return currentPacked | ((value & mask) << offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Unpack(byte packed, int startBit, int bitCount)
        => (packed >> startBit) & ((1 << bitCount) - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Unpack(ushort packed, int startBit, int bitCount)
        => (packed >> startBit) & ((1 << bitCount) - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Unpack(int packed, int startBit, int bitCount)
        => (packed >> startBit) & ((1 << bitCount) - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Unpack(uint packed, int startBit, int bitCount)
        => (int)((packed >> startBit) & ((1u << bitCount) - 1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Unpack(long packed, int startBit, int bitCount)
        => (packed >> startBit) & ((1L << bitCount) - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Unpack(ulong packed, int startBit, int bitCount)
        => (long)((packed >> startBit) & ((1ul << bitCount) - 1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Int128 Pack128(Int128 current, long value, int offset, int bits)
    {
        Int128 mask = (Int128.One << bits) - 1;
        return current | ((Int128)value & mask) << offset;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Unpack128(Int128 packed, int offset, int bits)
    {
        Int128 mask = (Int128.One << bits) - 1;
        return (long)((packed >> offset) & mask);
    }
}