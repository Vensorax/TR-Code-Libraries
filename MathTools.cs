// MIT License
// Copyright (c) 2025 Samuel Nchinda

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace TeamRadiance.Science;

public static class MathTools
{
    public static readonly Vector2 Zero = new(0, 0);
    public static readonly Vector3 Zero3D = new(0, 0, 0);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerp(float start, float end, float t) => start + (end - start) * t;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsZeroApprox(this float value) => MathF.Abs(value) < 1E-06f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool BoolNullSafety(this bool? boolean, bool trueOverride = false) => boolean ?? trueOverride;

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static double CollectiveSum(ReadOnlySpan<double> group)
    {
        if (group.Length < Vector<double>.Count)
        {
            double sum = 0;
            for (int i = 0; i < group.Length; i++) sum += group[i];
            return sum;
        }

        var vectors = MemoryMarshal.Cast<double, Vector<double>>(group);
        Vector<double> accVector = Vector<double>.Zero;
        foreach (var v in vectors) accVector += v;

        double finalSum = Vector.Sum(accVector);
        int processedCount = vectors.Length * Vector<double>.Count;
        for (int i = processedCount; i < group.Length; i++) finalSum += group[i];

        return finalSum;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static int CollectiveSum(ReadOnlySpan<int> group)
    {
        if (group.Length < Vector<int>.Count)
        {
            int sum = 0;
            for (int i = 0; i < group.Length; i++) sum += group[i];
            return sum;
        }

        var vectors = MemoryMarshal.Cast<int, Vector<int>>(group);
        Vector<int> accVector = Vector<int>.Zero;
        foreach (var v in vectors) accVector += v;

        int finalSum = Vector.Sum(accVector);
        int processedCount = vectors.Length * Vector<int>.Count;
        for (int i = processedCount; i < group.Length; i++) finalSum += group[i];

        return finalSum;
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static long CollectiveSum(ReadOnlySpan<byte> group)
    {
        long sum = 0;
        foreach (var b in group) sum += b;
        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Mean(ReadOnlySpan<double> group) => 
        group.Length == 0 ? 0 : CollectiveSum(group) / group.Length;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Mean(ReadOnlySpan<int> group) => 
        group.Length == 0 ? 0 : (double)CollectiveSum(group) / group.Length;

    public static double Median(ReadOnlySpan<double> group)
    {
        if (group.IsEmpty) return 0;
        double[] sorted = group.ToArray();
        Array.Sort(sorted);
        int mid = sorted.Length / 2;
        return (sorted.Length % 2 != 0) ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2.0;
    }

    public readonly struct StatsResult(double avg, double dev, double min, double max, double score, double pct)

    {
        public readonly double Average = avg;
        public readonly double StandardDeviation = dev;
        public readonly double Minimum = min;
        public readonly double Maximum = max;
        public readonly double Range = max - min;
        public readonly double BalanceScore = score;
        public readonly double BalancePercentage = pct;

        public FrozenDictionary<string, object> ToFrozenDictionary() => new Dictionary<string, object>
        {
            {"Average", Average}, {"StandardDeviation", StandardDeviation}, {"Minimum", Minimum},
            {"Maximum", Maximum}, {"Range", Range}, {"BalanceScore", BalanceScore}, {"BalancePercentage", BalancePercentage}
        }.ToFrozenDictionary();
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static StatsResult AnalyzeStats(ReadOnlySpan<double> stats)
    {
        if (stats.IsEmpty) return default;

        double sum = 0;
        double min = double.MaxValue;
        double max = double.MinValue;

        for (int i = 0; i < stats.Length; i++)
        {
            double val = stats[i];
            sum += val;
            if (val < min) min = val;
            if (val > max) max = val;
        }

        double average = sum / stats.Length;
        double varianceSum = 0;
        double balanceSum = 0;

        for (int i = 0; i < stats.Length; i++)
        {
            double diff = stats[i] - average;
            varianceSum += diff * diff;
            balanceSum += Math.Abs(diff);
        }

        double deviation = Math.Sqrt(varianceSum / stats.Length);
        double balanceScore = balanceSum / stats.Length;
        double maxImbalance = stats.Length > 1 ? sum - average : 0.0;
        double balancePercentage = maxImbalance > 0.00001 ? 100.0 * (1.0 - (balanceScore / maxImbalance)) : 100.0;

        return new StatsResult(average, deviation, min, max, balanceScore, balancePercentage);
    }

    public static byte[] ConvertDoublesToBytes(ReadOnlySpan<double> doubles)
    {
        byte[] result = GC.AllocateUninitializedArray<byte>(doubles.Length);
        for (int i = 0; i < doubles.Length; i++) result[i] = (byte)Math.Round(doubles[i]);
        return result;
    }

    public static int[] ConvertDoublesToInts(ReadOnlySpan<double> doubles)
    {
        int[] result = GC.AllocateUninitializedArray<int>(doubles.Length);
        for (int i = 0; i < doubles.Length; i++) result[i] = (int)Math.Round(doubles[i]);
        return result;
    }

    public static double[] ConvertIntsToDoubles(ReadOnlySpan<int> ints)
    {
        double[] result = GC.AllocateUninitializedArray<double>(ints.Length);
        for (int i = 0; i < ints.Length; i++) result[i] = ints[i];
        return result;
    }

    public static byte[] ConvertIntsToBytes(ReadOnlySpan<int> bytes)
    {
        byte[] result = GC.AllocateUninitializedArray<byte>(bytes.Length);
        for (int i = 0; i < bytes.Length; i++) result[i] = (byte)bytes[i];
        return result;
    }

    public static double[] ConvertFloatsToDoubles(ReadOnlySpan<float> floats)
    {
        double[] result = GC.AllocateUninitializedArray<double>(floats.Length);
        for (int i = 0; i < floats.Length; i++) result[i] = floats[i];
        return result;
    }

    public static float[] ConvertDoublesToFloats(ReadOnlySpan<double> doubles)
    {
        float[] result = GC.AllocateUninitializedArray<float>(doubles.Length);
        for (int i = 0; i < doubles.Length; i++) result[i] = (float)doubles[i];
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CollectiveSum(this List<double> list) => CollectiveSum(CollectionsMarshal.AsSpan(list));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CollectiveSum(this List<int> list) => CollectiveSum(CollectionsMarshal.AsSpan(list));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CollectiveSum(this List<byte> list) => CollectiveSum(CollectionsMarshal.AsSpan(list));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Mean(this List<double> list) => Mean(CollectionsMarshal.AsSpan(list));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Mean(this List<int> list) => Mean(CollectionsMarshal.AsSpan(list));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Median(this List<double> list) => Median(CollectionsMarshal.AsSpan(list));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StatsResult AnalyzeStats(this List<double> list) => AnalyzeStats(CollectionsMarshal.AsSpan(list));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double CollectiveSum(this double[] array) => CollectiveSum(new ReadOnlySpan<double>(array));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CollectiveSum(this byte[] array) => CollectiveSum(new ReadOnlySpan<byte>(array));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CollectiveSum(this int[] array) => CollectiveSum(new ReadOnlySpan<int>(array));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Mean(this double[] array) => Mean(new ReadOnlySpan<double>(array));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Mean(this int[] array) => Mean(new ReadOnlySpan<int>(array));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Median(this double[] array) => Median(new ReadOnlySpan<double>(array));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StatsResult AnalyzeStats(this double[] array) => AnalyzeStats(new ReadOnlySpan<double>(array));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte Sign(this double value) => (sbyte)(value < 0 ? -1 : (value > 0 ? 1 : 0));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte Sign(this int value) => (sbyte)(value < 0 ? -1 : (value > 0 ? 1 : 0));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte Sign(this float value) => (sbyte)(value < 0 ? -1 : (value > 0 ? 1 : 0));
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static sbyte Sign(this long value) => (sbyte)(value < 0 ? -1 : (value > 0 ? 1 : 0));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ClampMax(this int value, int max) => value > max ? max : value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ClampMax(this double value, double max) => value > max ? max : value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ClampMax(this float value, float max) => value > max ? max : value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ClampMin(this int value, int min) => value < min ? min : value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double ClampMin(this double value, double min) => value < min ? min : value;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ClampMin(this float value, float min) => value < min ? min : value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(this int value, int min, int max) => value < min ? min : (value > max ? max : value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp(this float value, float min, float max) => value < min ? min : (value > max ? max : value);

    /// <summary>
    /// Warning: This will stop at 21! so if you need a longer one, use the ulong version.
    /// Or one of the 128-bit versions.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static long Factorial(this int x)
    {
        if (x < 0) throw new ArgumentException("Negative factorial");
        long result = 1;
        for (int i = 1; i <= x; i++) result *= i;
        return result;
    }

    public static Int128 Factorial128(this int x)
    {
        if (x < 0) throw new ArgumentException("Negative factorial");
        Int128 result = 1;
        for (int i = 1; i <= x; i++) result *= i;
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong Factorial(this ulong x) => (x == 0) ? 1 : x * Factorial(x - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static UInt128 Factorial128(this ulong x) => (x == 0) ? 1 : x * Factorial128(x - 1);

    public static string TruncateTrailingZeros(float number)
    {
        if (float.IsInfinity(number) || float.IsNaN(number)) return number.ToString();
        string result = number.ToString("0.####################");
        return result.Contains('.') ? result.TrimEnd('0').TrimEnd('.') : result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double RoundDecimals(double num, byte places) => Math.Round(num, places);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong SafeAdd(this ulong current, ulong amount) => (ulong.MaxValue - current < amount) ? ulong.MaxValue : current + amount;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ulong SafeSubtract(this ulong current, ulong amount) => (amount >= current) ? 0 : current - amount;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double SafeDivideDouble(this double current, uint amount) => (amount == 0) ? 0 : current / amount;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double SafeDivideDouble(this double current, int amount) => (amount == 0) ? 0 : current / amount;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double SafeDivideDouble(this double current, ulong amount) => (amount == 0) ? 0 : current / amount;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double SafeDivideDouble(this ulong current, ulong amount) => (amount == 0) ? 0 : (double)current / amount;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double SafeDivideDouble(this int current, ulong amount) => amount == 0 ? 0 : (double)current / amount;

    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static float ToFloat(this object value) => Convert.ToSingle(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double ToDouble(this object value) => Convert.ToDouble(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ushort ToUshort(this object value) => Convert.ToUInt16(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static sbyte ToSbyte(this object value) => Convert.ToSByte(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint ToUint(this object value) => Convert.ToUInt32(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static ulong ToUlong(this object value) => Convert.ToUInt64(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static byte ToByte(this object value) => Convert.ToByte(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static short ToShort(this object value) => Convert.ToInt16(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int ToInt(this object value) => Convert.ToInt32(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static long ToLong(this object value) => Convert.ToInt64(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool ToBool(this object value) => Convert.ToBoolean(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double SafeClamp(this double value, double min, double max) => 
        min > max ? Math.Clamp(value, max, min) : Math.Clamp(value, min, max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SafeClamp(this float value, float min, float max) => 
        min > max ? Math.Clamp(value, max, min) : Math.Clamp(value, min, max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SafeClamp(this int value, int min, int max) => 
        min > max ? Math.Clamp(value, max, min) : Math.Clamp(value, min, max);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 MoveSteps(this Vector2 current, double stepsCount, double angle) => 
        new((float)(current.X + stepsCount * Math.Cos(angle)), (float)(current.Y + stepsCount * Math.Sin(angle)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 MoveSteps(this Vector2 current, float stepsCount, float angle) => 
        new((float)(current.X + stepsCount * Math.Cos(angle)), (float)(current.Y + stepsCount * Math.Sin(angle)));

    public static Vector3 MoveSteps3D(this Vector3 current, float steps, float yawRadians, float pitchRadians)
    {
        float cosPitch = MathF.Cos(pitchRadians);
        return new Vector3(
            current.X + steps * cosPitch * MathF.Cos(yawRadians),
            current.Y + steps * MathF.Sin(pitchRadians),
            current.Z + steps * cosPitch * MathF.Sin(yawRadians)
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double TotalYears(this TimeSpan time) => time.TotalDays / 365.25;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int TotalYearsInt(this TimeSpan time) => (int)(time.TotalDays / 365.25);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint TotalYearsUint(this TimeSpan time) => (uint)(time.TotalDays / 365.25);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double TotalWeeks(this TimeSpan time) => time.TotalDays / 7;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int TotalWeeksInt(this TimeSpan time) => (int)(time.TotalDays / 7);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint TotalWeeksUint(this TimeSpan time) => (uint)(time.TotalDays / 7);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double TotalMonths(this TimeSpan time) => time.TotalDays / 30;
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int TotalMonthsInt(this TimeSpan time) => (int)(time.TotalDays / 30);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static uint TotalMonthsUint(this TimeSpan time) => (uint)(time.TotalDays / 30);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 FlattenVector(this Vector3 vector, float focalLength = 500f, bool usePerspective = true)
    {
        if (!usePerspective || IsZeroApprox(vector.Z)) return new Vector2(vector.X, vector.Y);
        float factor = focalLength / (focalLength + vector.Z);
        return new Vector2(vector.X * factor, vector.Y * factor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool FlipSwitch(this bool current, Func<bool> condition = null) => 
        (condition?.Invoke() ?? true) ? !current : current;


    private const float S1 = 10f;    private const float IS1 = 0.1f;

    private const float S2 = 100f;   private const float IS2 = 0.01f;

    private const float S3 = 1000f;  private const float IS3 = 0.001f;



    private const double S1D = 10.0;  private const double IS1D = 0.1;

    private const double S2D = 100.0; private const double IS2D = 0.01;

    private const double S3D = 1000.0; private const double IS3D = 0.001;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FixedAdd1(this float a, float b) => 
        (a.FloatToLongPrecise1() + b.FloatToLongPrecise1()) * IS1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FixedSubtract1(this float a, float b) => 
        (a.FloatToLongPrecise1() - b.FloatToLongPrecise1()) * IS1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FixedMultiply1(this float a, float b) => 
        a.FloatToLongPrecise1() * b.FloatToLongPrecise1() * (IS1 * IS1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FixedDivide1(this float a, float b) => 
        IsZeroApprox(b) ? 0f : a.FloatToLongPrecise1() * (long)S1 / b.FloatToLongPrecise1() * IS1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FixedMod1(this float a, float b) => 
        IsZeroApprox(b) ? 0f : a.FloatToLongPrecise1() % b.FloatToLongPrecise1() * IS1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FixedAdd2(this float a, float b) => 
        (a.FloatToLongPrecise2() + b.FloatToLongPrecise2()) * IS2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FixedSubtract2(this float a, float b) => 
        (a.FloatToLongPrecise2() - b.FloatToLongPrecise2()) * IS2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FixedMultiply2(this float a, float b) => 
        a.FloatToLongPrecise2() * b.FloatToLongPrecise2() * (IS2 * IS2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FixedDivide2(this float a, float b) => 
        IsZeroApprox(b) ? 0f : a.FloatToLongPrecise2() * (long)S2 / b.FloatToLongPrecise2() * IS2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FixedMod2(this float a, float b) => 
        IsZeroApprox(b) ? 0f : a.FloatToLongPrecise2() % b.FloatToLongPrecise2() * IS2;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FixedAdd(this float a, float b) => 
        (a.FloatToLongPrecise() + b.FloatToLongPrecise()) * IS3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FixedSubtract(this float a, float b) => 
        (a.FloatToLongPrecise() - b.FloatToLongPrecise()) * IS3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FixedMultiply(this float a, float b) => 
        a.FloatToLongPrecise() * b.FloatToLongPrecise() * (IS3 * IS3);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FixedDivide(this float a, float b) => 
        IsZeroApprox(b) ? 0f : a.FloatToLongPrecise() * (long)S3 / b.FloatToLongPrecise() * IS3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float FixedMod(this float a, float b) => 
        IsZeroApprox(b) ? 0f : a.FloatToLongPrecise() % b.FloatToLongPrecise() * IS3;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FixedAdd1(this double a, double b) => 
        (a.DoubleToLongPrecise1() + b.DoubleToLongPrecise1()) * IS1D;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FixedSubtract1(this double a, double b) => 
        (a.DoubleToLongPrecise1() - b.DoubleToLongPrecise1()) * IS1D;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FixedMultiply1(this double a, double b) => 
        a.DoubleToLongPrecise1() * b.DoubleToLongPrecise1() * (IS1D * IS1D);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FixedDivide1(this double a, double b) => 
        IsZeroApprox(b) ? 0.0 : a.DoubleToLongPrecise1() * (long)S1D / b.DoubleToLongPrecise1() * IS1D;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FixedMod1(this double a, double b) => 
        IsZeroApprox(b) ? 0.0 : a.DoubleToLongPrecise1() % b.DoubleToLongPrecise1() * IS1D;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FixedAdd2(this double a, double b) => 
        (a.DoubleToLongPrecise2() + b.DoubleToLongPrecise2()) * IS2D;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FixedSubtract2(this double a, double b) => 
        (a.DoubleToLongPrecise2() - b.DoubleToLongPrecise2()) * IS2D;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FixedMultiply2(this double a, double b) => 
        a.DoubleToLongPrecise2() * b.DoubleToLongPrecise2() * (IS2D * IS2D);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FixedDivide2(this double a, double b) => 
        IsZeroApprox(b) ? 0.0 : a.DoubleToLongPrecise2() * (long)S2D / b.DoubleToLongPrecise2() * IS2D;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FixedMod2(this double a, double b) => 
        IsZeroApprox(b) ? 0.0 : a.DoubleToLongPrecise2() % b.DoubleToLongPrecise2() * IS2D;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FixedAdd(this double a, double b) => 
        (a.DoubleToLongPrecise() + b.DoubleToLongPrecise()) * IS3D;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FixedSubtract(this double a, double b) => 
        (a.DoubleToLongPrecise() - b.DoubleToLongPrecise()) * IS3D;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FixedMultiply(this double a, double b) => 
        a.DoubleToLongPrecise() * b.DoubleToLongPrecise() * (IS3D * IS3D);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FixedDivide(this double a, double b) => 
        IsZeroApprox(b) ? 0.0 : a.DoubleToLongPrecise() * (long)S3D / b.DoubleToLongPrecise() * IS3D;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double FixedMod(this double a, double b) => 
        IsZeroApprox(b) ? 0.0 : a.DoubleToLongPrecise() % b.DoubleToLongPrecise() * IS3D;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsZeroApprox(this double value) => Math.Abs(value) < 1E-06;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToUI1Decimal(this float value) => value.ToString("0.#");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToUI2Decimals(this float value) => value.ToString("0.##");
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ToCurrencyUI(this float value) => value.ToString("0.00");

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FloatToIntPrecise1(this float value) => (int)MathF.Round(value * 10f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FloatToIntPrecise2(this float value) => (int)MathF.Round(value * 100f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int FloatToIntPrecise(this float value) => (int)MathF.Round(value * 1000f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DoubleToIntPrecise1(this double value) => (int)Math.Round(value * 10.0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DoubleToIntPrecise2(this double value) => (int)Math.Round(value * 100.0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DoubleToIntPrecise(this double value) => (int)Math.Round(value * 1000.0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long FloatToLongPrecise1(this float value) => (long)MathF.Round(value * 10f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long FloatToLongPrecise2(this float value) => (long)MathF.Round(value * 100f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long FloatToLongPrecise(this float value) => (long)MathF.Round(value * 1000f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long DoubleToLongPrecise1(this double value) => (long)Math.Round(value * 10.0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long DoubleToLongPrecise2(this double value) => (long)Math.Round(value * 100.0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long DoubleToLongPrecise(this double value) => (long)Math.Round(value * 1000.0);
}