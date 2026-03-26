// MIT License
// Copyright (c) 2026 Samuel Nchinda

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MemoryPack;

namespace TeamRadiance.Collections;

[Flags]
public enum BufferSettings : byte 
{
    None = 0,
    ReadOnly = 1 << 0,
    ClearFatOnRemove = 1 << 1,
    PreserveOrder = 1 << 2,
    LogErrors = 1 << 3,
    UniqueElements = 1 << 4
}

[CollectionBuilder(typeof(PackedBufferBuilder), nameof(PackedBufferBuilder.Create))]
[StructLayout(LayoutKind.Sequential)]
[MemoryPackable]
public sealed partial class PackedBuffer<T>
{
    [JsonIgnore] [MemoryPackIgnore] public T[] _data { get; internal set; }
    [JsonIgnore] [MemoryPackIgnore] public int _count { get; internal set; }

    [MemoryPackOrder(0)] [MemoryPackAllowSerialize] public BufferSettings Settings { get; set; }
    [MemoryPackOrder(1)] public int SavedCapacity => _data.Length;
    [MemoryPackOrder(2)] public T[] ActiveData => _data.AsSpan(0, _count).ToArray();

    [MemoryPackConstructor]
    private PackedBuffer(BufferSettings settings, int savedCapacity, T[] activeData)
    {
        Settings = settings;
        _data = new T[Math.Max(16, savedCapacity)];

        if (activeData != null && activeData.Length > 0)
        {
            _count = activeData.Length;
            Array.Copy(activeData, _data, _count);
        }
        else
        {
            _count = 0;
        }
    }
    
    public bool IsReadOnly
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (Settings & BufferSettings.ReadOnly) != 0; 
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set 
        {
            if (value) Settings |= BufferSettings.ReadOnly;
            else Settings &= ~BufferSettings.ReadOnly;
        }
    }

    public bool ClearFat
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => (Settings & BufferSettings.ClearFatOnRemove) != 0;
    }

    public int Count => _count;
    public int Capacity => _data.Length;
    public bool IsFull => _count >= _data.Length;

    public PackedBuffer(int capacity, BufferSettings settings = BufferSettings.ClearFatOnRemove)
    {
        _data = new T[capacity];
        _count = 0;
        Settings = settings;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        if ((Settings & BufferSettings.ReadOnly) != 0 || _count >= _data.Length) return;

        if ((Settings & BufferSettings.UniqueElements) != 0)
        {
            for (int i = 0; i < _count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(_data[i], item)) return; 
            }
        }

        _data[_count++] = item;
    }

    public bool Remove(T item)
    {
        if ((Settings & BufferSettings.ReadOnly) != 0) return false;

        for (int i = 0; i < _count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_data[i], item))
            {
                RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public bool Contains(T item)
    {
        for (int i = 0; i < _count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_data[i], item)) return true;
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        if (IsReadOnly || index < 0 || index >= _count) return;

        _count--;

        if ((Settings & BufferSettings.PreserveOrder) != 0)
        {
            if (index < _count) Array.Copy(_data, index + 1, _data, index, _count - index);
        }
        else
        {
            _data[index] = _data[_count];
        }

        if ((Settings & BufferSettings.ClearFatOnRemove) != 0)
        {
            _data[_count] = default!;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T> AsSpan() => new(_data, 0, _count);

    public IEnumerable<T> AsEnumerable() => _data.Take(_count); 

    public PackedBuffer<T> Clone()
    {
        var clone = new PackedBuffer<T>(_data.Length, Settings);
        Array.Copy(_data, clone._data, _count);
        clone._count = _count;
        return clone;
    }

    public void Clear()
    {
        if (IsReadOnly) return;
        if ((Settings & BufferSettings.ClearFatOnRemove) != 0) Array.Clear(_data, 0, _count);
        _count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ReadOnlySpan<T>(PackedBuffer<T> buffer) => buffer != null ? buffer.AsSpan() : [];

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _data.Length) throw new ArgumentOutOfRangeException(nameof(index));
            return _data[index];
        }
        set
        {
            if (index < 0 || index >= _data.Length) throw new ArgumentOutOfRangeException(nameof(index));
            _data[index] = value;
        }
    }

    public bool Swap(int index1, int index2)
    {
        if ((Settings & BufferSettings.ReadOnly) != 0) return false;
        if (index1 < 0 || index1 >= _count || index2 < 0 || index2 >= _count) return false;

        (_data[index1], _data[index2]) = (_data[index2], _data[index1]);
        return true;
    }

    public bool Swap(T item1, T item2)
    {
        if ((Settings & BufferSettings.ReadOnly) != 0) return false;

        int index1 = -1, index2 = -1;
        for (int i = 0; i < _count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_data[i], item1)) index1 = i;
            else if (EqualityComparer<T>.Default.Equals(_data[i], item2)) index2 = i;

            if (index1 != -1 && index2 != -1) break;
        }

        if (index1 == -1 || index2 == -1) return false;

        (_data[index1], _data[index2]) = (_data[index2], _data[index1]);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<T>.Enumerator GetEnumerator() => AsSpan().GetEnumerator();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEmpty() => _count == 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int SpacesLeft() => Capacity - Count;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRange(ReadOnlySpan<T> items, bool force = false)
    {
        if ((Settings & BufferSettings.ReadOnly) != 0 || items.IsEmpty) return;

        int itemsToAdd = items.Length;
        if (_count + itemsToAdd > _data.Length)
        {
            if (!force) return;
            itemsToAdd = _data.Length - _count;
        }

        if (itemsToAdd <= 0) return;
        items.Slice(0, itemsToAdd).CopyTo(_data.AsSpan(_count));
        _count += itemsToAdd;
    }
}

public static class PackedBufferExtensions
{
    public static PackedBuffer<T> ToPackedBuffer<T>(this IEnumerable<T> source, int capacity, BufferSettings settings = BufferSettings.ClearFatOnRemove)
    {
        var buffer = new PackedBuffer<T>(capacity, settings);
        foreach (var item in source) buffer.Add(item);
        return buffer;
    }
}

public static class PackedBufferBuilder
{
    public static PackedBuffer<T> Create<T>(ReadOnlySpan<T> items)
    {
        int capacity = items.Length > 0 ? items.Length : 16; 
        var buffer = new PackedBuffer<T>(capacity);
        foreach (var item in items) buffer.Add(item);
        return buffer;
    }
}

public static class PackedLinq
{
    public static T FirstOr<T>(this ReadOnlySpan<T> span, Predicate<T> match, T defaultValue = default!)
    {
        for (int i = 0; i < span.Length; i++)
            if (match(span[i])) return span[i];
        return defaultValue;
    }

    public static bool Any<T>(this ReadOnlySpan<T> span, Predicate<T> match)
    {
        for (int i = 0; i < span.Length; i++)
            if (match(span[i])) return true;
        return false;
    }

    public static void SelectInto<T, TResult>(this ReadOnlySpan<T> span, PackedBuffer<TResult> destination, Func<T, TResult> selector)
    {
        destination.Clear();
        for (int i = 0; i < span.Length; i++)
            destination.Add(selector(span[i]));
    }
}