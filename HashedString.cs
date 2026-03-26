// MIT License

// Copyright (c) 2026 Samuel Nchinda

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// The ultimate high-performance Flyweight string.
/// 8-byte footprint. Zero-allocation hashing. Aggressive JIT inlining.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct HashedString : IEquatable<HashedString>, IComparable<HashedString>
{
    /// <summary>
    /// The only thing that matters: the 64-bit hash of the string.
    /// </summary>
    public readonly long Hash;

    /// <summary>
    /// The Global String Table
    /// </summary>
    private static readonly ConcurrentDictionary<long, string> _registry = new();

    /// <summary>
    /// The "Null" state
    /// </summary>
    public static readonly HashedString Empty = new(0);

    #region Constructors

    /// <summary>
    /// FAST PATH: Direct assignment for internal use.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal HashedString(long hash) => Hash = hash;

    /// <summary>
    /// STANDARD PATH: Hashes a string and registers it.
    /// </summary>
    public HashedString(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            Hash = 0;
            return;
        }

        Hash = HashingAlgo.Calculate(value);
        
        // TryAdd is thread-safe and faster than Contains + Add
        _registry.TryAdd(Hash, value);
    }

    /// <summary>
    /// SPAN PATH: Zero-allocation lookup for existing strings.
    /// </summary>
    public HashedString(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
        {
            Hash = 0;
            return;
        }

        long h = HashingAlgo.Calculate(value);
        Hash = h;

        if (_registry.TryGetValue(h, out string? existingValue))
        {
            // Console.WriteLine($"[HashedString] Reusing existing hash for '{existingValue}'");
            return;
        }

        _registry.TryAdd(h, value.ToString());
    }

    #endregion

    #region Engine Properties

    [JsonIgnore]
    public bool IsEmpty 
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Hash == 0;
    }

    /// <summary>
    /// Returns true if this hash is actually registered in the string table.
    /// </summary>
    [JsonIgnore]
    public bool IsResolved => _registry.ContainsKey(Hash);

    /// <summary>
    /// The "Human" view of the hash.
    /// </summary>
    [JsonIgnore]
    public string Name
    {
        get
        {
            if (Hash == 0) return string.Empty;
            return _registry.TryGetValue(Hash, out string? val) ? val : $"Hash:{Hash}";
        }
    }

    public int Length => Name.Length;

    #endregion

    #region Operators (JIT Inlined)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator HashedString(string? s) => new(s);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<char>(HashedString h) 
    {
        if (h.Hash == 0) return [];

        if (_registry.TryGetValue(h.Hash, out string? val))
        {
            return val.AsSpan();
        }

        return []; 
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator long(HashedString h) => h.Hash;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator string(HashedString h) => h.Name;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(HashedString left, HashedString right) => left.Hash == right.Hash;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(HashedString left, HashedString right) => left.Hash != right.Hash;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(HashedString left, string? right) => 
        left.Hash == (string.IsNullOrEmpty(right) ? 0 : HashingAlgo.Calculate(right));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(HashedString left, string? right) => 
        left.Hash != (string.IsNullOrEmpty(right) ? 0 : HashingAlgo.Calculate(right));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(HashedString left, ReadOnlySpan<char> right)
    {
        if (right.IsEmpty) return left.Hash == 0;
        
        ReadOnlySpan<char> leftSpan = left; 
        return leftSpan.SequenceEqual(right);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(HashedString left, ReadOnlySpan<char> right) => !(left == right);

    #endregion

    #region Utilities

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<char> AsSpan()
    {
        if (Hash == 0) return [];
        if (_registry.TryGetValue(Hash, out string? val))
        {
            return val.AsSpan();
        }
        return []; 
    }

    /// <summary>
    /// Combines two hashes into a unique third hash (for bit-packing IDs).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HashedString Combine(HashedString first, HashedString second)
    {
        unchecked
        {
            // 🚀 FIX: 64-bit Jenkins / Murmur inspired mixer using the 64-bit golden ratio
            long combined = first.Hash ^ (second.Hash + unchecked((long)0x9e3779b97f4a7c15) + (first.Hash << 6) + (first.Hash >> 2));
            return new HashedString(combined);
        }
    }

    public static HashedString FromLower(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty) return Empty;
        
        char[]? rented = null;
        Span<char> lower = value.Length <= 256 
            ? stackalloc char[value.Length] 
            : (rented = ArrayPool<char>.Shared.Rent(value.Length)).AsSpan(0, value.Length);

        try
        {
            value.ToLowerInvariant(lower);
            return new HashedString(lower);
        }
        finally
        {
            if (rented != null) ArrayPool<char>.Shared.Return(rented);
        }
    }

    #endregion

    #region Interface Boilerplate

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(HashedString other) => Hash == other.Hash;

    public override bool Equals(object? obj) => obj is HashedString other && Equals(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => (int)(Hash ^ (Hash >> 32));

    public override string ToString() => Name;

    public int CompareTo(HashedString other) => Hash.CompareTo(other.Hash);

    #endregion

    #region Surgical Registry Tools

    public static bool RemoveString(HashedString h) => _registry.TryRemove(h.Hash, out _);
    public static void ClearRegistry() => _registry.Clear();

    #endregion
}

#region Serialization

/// <summary>
/// Custom JSON converter for HashedString. Serializes as the original string name for readability.
/// </summary>
public sealed class HashedStringConverter : JsonConverter<HashedString>
{
    public override HashedString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return new HashedString(value);
    }

    public override void Write(Utf8JsonWriter writer, HashedString value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Name);
    }

    public override HashedString ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? value = reader.GetString();
        return new HashedString(value);
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, HashedString value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.Name);
    }
}

#endregion

/// <summary>
/// Extension methods for HashedString, such as extracting the base name without path or extension.
/// </summary>
public static class HashedPathExtensions
{
    public static HashedString GetBaseName(this HashedString path)
    {
        ReadOnlySpan<char> span = path; 
        if (span.IsEmpty) return path;

        int sep = Math.Max(span.LastIndexOf('/'), span.LastIndexOf('\\'));
        if (sep != -1) span = span[(sep + 1)..];

        int dot = span.LastIndexOf('.');
        if (dot != -1) span = span[..dot];

        return new HashedString(span);
    }
}

public static class HashingAlgo
{
    private const long OffsetBasis = unchecked((long)14695981039346656037UL);
    private const long Prime = 1099511628211L;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Calculate(ReadOnlySpan<char> text)
    {
        if (text.IsEmpty) return 0;

        unchecked
        {
            long hash = OffsetBasis;
            foreach (char c in text)
            {
                // XOR the low-order byte of the character
                hash ^= c;
                hash *= Prime;
            }
            return hash;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long CalculateIgnoreCase(ReadOnlySpan<char> text)
    {
        if (text.IsEmpty) return 0;

        unchecked
        {
            long hash = OffsetBasis;
            foreach (char c in text)
            {
                char lowerChar = (c >= 'A' && c <= 'Z') ? (char)(c | 0x20) : c;
                hash ^= lowerChar;
                hash *= Prime;
            }
            return hash;
        }
    }
}