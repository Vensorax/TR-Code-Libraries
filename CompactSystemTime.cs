// MIT License
// Copyright (c) 2026 Team Radiance

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TeamRadiance.Data;

/// <summary>
/// A high-performance, 10-byte struct that replaces the 16-byte DateTimeOffset.
/// Perfect for saving massive amounts of RAM when storing thousands of timestamps.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 2)]
public readonly partial struct CompactSystemTime : IEquatable<CompactSystemTime>, IComparable<CompactSystemTime>
{
    #region Data Fields (10 Bytes Total)

    /// <summary>
    /// The local time ticks.
    /// </summary>
    public readonly long Ticks; 

    /// <summary>
    /// The timezone offset in minutes.
    /// </summary>
    public readonly short OffsetMinutes;

    #endregion

    #region Static Getters

    public static CompactSystemTime UtcNow => new(DateTimeOffset.UtcNow);
    public static CompactSystemTime Now => new(DateTimeOffset.Now);

    #endregion

    #region Constructors

    public CompactSystemTime(DateTimeOffset dt)
    {
        Ticks = dt.Ticks;
        OffsetMinutes = (short)dt.Offset.TotalMinutes;
    }
    
    public CompactSystemTime(long ticks, short offsetMinutes)
    {
        Ticks = ticks;
        OffsetMinutes = offsetMinutes;
    }

    #endregion

    #region Calculated Properties (Zero RAM Cost)

    public long UnixSeconds 
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (Ticks - 621355968000000000L) / TimeSpan.TicksPerSecond;
    }

    /// <summary>
    /// Calculates the absolute UTC Ticks for accurate cross-timezone comparisons.
    /// </summary>
    public long UtcTicks
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Ticks - (OffsetMinutes * TimeSpan.TicksPerMinute);
    }

    #endregion

    #region Conversion & Logic

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DateTimeOffset ToDateTimeOffset() => 
        new(Ticks, TimeSpan.FromMinutes(OffsetMinutes));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(CompactSystemTime other) => 
        Ticks == other.Ticks && OffsetMinutes == other.OffsetMinutes;

    public override bool Equals(object? obj) => obj is CompactSystemTime other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Ticks, OffsetMinutes);

    public string ToString(string format = "yyyy-MM-dd HH:mm:ss.fff (zzz)") => ToDateTimeOffset().ToString(format);

    #endregion

    #region Evolution Methods (With Pattern)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CompactSystemTime WithOffset(TimeSpan newOffset) => new(Ticks, (short)newOffset.TotalMinutes);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static CompactSystemTime WithDateTime(DateTimeOffset newDateTime) => new(newDateTime);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CompactSystemTime WithTicks(long newTicks) => new(newTicks, OffsetMinutes);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public CompactSystemTime WithOffsetMinutes(short newOffsetMinutes) => new(Ticks, newOffsetMinutes);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int CompareTo(CompactSystemTime other)
    {
        return UtcTicks.CompareTo(other.UtcTicks);
    }

    public bool IsBefore(CompactSystemTime other) => this < other;

    public bool IsAfter(CompactSystemTime other) => this > other;

    public static CompactSystemTime MinValue => new(0, 0);

    public static CompactSystemTime MaxValue => new(long.MaxValue, short.MaxValue);

    #endregion

    #region Operators
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(CompactSystemTime left, CompactSystemTime right) => left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(CompactSystemTime left, CompactSystemTime right) => !left.Equals(right);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan operator -(CompactSystemTime left, CompactSystemTime right) 
    {
        return new TimeSpan(left.UtcTicks - right.UtcTicks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(CompactSystemTime left, CompactSystemTime right) => left.UtcTicks > right.UtcTicks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(CompactSystemTime left, CompactSystemTime right) => left.UtcTicks < right.UtcTicks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(CompactSystemTime left, CompactSystemTime right) => left.UtcTicks >= right.UtcTicks;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(CompactSystemTime left, CompactSystemTime right) => left.UtcTicks <= right.UtcTicks;

    #endregion
}