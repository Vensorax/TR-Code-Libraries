// MIT License
// Copyright (c) 2026 Team Radiance
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TeamRadiance;

/// <summary>
/// The central entropy engine for the Team Radiance project.
/// Supports deterministic replay via SetSeed() (Logic Stream) and chaotic effects (Visual Stream).
/// </summary>
public static class TRandom
{
    public enum RollType : byte
    {
        Standard,
        Double,
        DoubleDisadv,
        Average
    }

    private static Random _logicRandom = new();

    private static readonly Random _visualRandom = new();

    /// <summary>
    /// Resets the Logic RNG stream to a specific seed. 
    /// </summary>
    public static void SetSeed(int seed)
    {
        _logicRandom = new Random(seed);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Next(int minValue, int maxValue) => _logicRandom.Next(minValue, maxValue);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Next(int maxValue) => _logicRandom.Next(maxValue);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte NextByte(byte maxValue) => (byte)_logicRandom.Next(maxValue);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte NextByte(byte minValue, byte maxValue) => (byte)_logicRandom.Next(minValue, maxValue);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short NextShortS(short maxValue) => (short)_logicRandom.Next(maxValue);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short NextShort(short minValue, short maxValue) => (short)_logicRandom.Next(minValue, maxValue);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NextInt(int maxValue) => _logicRandom.Next(maxValue);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NextInt(int minValue, int maxValue) => _logicRandom.Next(minValue, maxValue);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double NextDouble() => _logicRandom.NextDouble();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NextRange(int minValue, int maxValue) => _logicRandom.Next(minValue, maxValue + 1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NextRange(short minValue, short maxValue) => _logicRandom.Next(minValue, maxValue + 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double NextRangeDouble(double minValue, double maxValue)
        => (_logicRandom.NextDouble() * (maxValue - minValue)) + minValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NextVisualInt(int max) => _visualRandom.Next(max);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NextVisualRange(int min, int max) => _visualRandom.Next(min, max + 1);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float NextVisualFloat() => (float)_visualRandom.NextDouble();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double NextVisualDouble() => _visualRandom.NextDouble();

    /// <summary>
    /// Returns a random float between min and max (Visual Stream).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float NextVisualRange(float min, float max)

        => (float)(_visualRandom.NextDouble() * (max - min) + min);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static short NextSafe(int min, int max, ReadOnlySpan<int> excluded)
    {
        int virtualRange = max - min - excluded.Length;
        int roll = _logicRandom.Next(min, min + virtualRange);

        for (int i = 0; i < excluded.Length; i++)
        {
            if (roll < excluded[i]) break;
            roll++;
        }
        return (short)roll;
    }

    /// <summary>
    /// Checks a probability out of 1000 using the Logic Stream.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CheckProbability(this ushort percent, RollType rollType = RollType.Standard)
    {
        ushort filter = Math.Clamp(percent, (ushort)0, (ushort)1000);
        ushort initialRoll = (ushort)_logicRandom.Next(0, 1000);
        ushort rollResult = initialRoll;

        switch (rollType)
        {
            case RollType.Double:
                rollResult = (ushort)Math.Min(initialRoll, _logicRandom.Next(0, 1000));
                break;
            case RollType.DoubleDisadv:
                rollResult = (ushort)Math.Max(initialRoll, _logicRandom.Next(0, 1000));
                break;
            case RollType.Average:
                rollResult = (ushort)((initialRoll + _logicRandom.Next(0, 1000)) / 2);
                break;
        }

        return filter > rollResult;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CheckBiasProbability(ushort basePercent, short bias = 0, RollType rollType = RollType.Standard)
    {
        return CheckProbability((ushort)Math.Clamp(basePercent + bias, 0, 1000), rollType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CheckChanceIn(ushort difficulty) => _logicRandom.Next(1, difficulty + 1) == 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetRandomElement<T>(IList<T> list)
    {
        if (list == null || list.Count == 0) return default;
        return list[_logicRandom.Next(list.Count)];
    }

    /// <summary>
    /// Weighted Index Picker optimized for performance by avoiding LINQ Sum.
    /// Uses Logic Stream.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetWeightedIndex(IList<int> weights)
    {
        if (weights == null || weights.Count == 0) return -1;

        int totalWeight = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            totalWeight += weights[i];
        }

        int randomRoll = _logicRandom.Next(totalWeight);

        for (int i = 0; i < weights.Count; i++)
        {
            if (randomRoll < weights[i])
            {
                return i;
            }
            randomRoll -= weights[i];
        }

        return weights.Count - 1;
    }
}