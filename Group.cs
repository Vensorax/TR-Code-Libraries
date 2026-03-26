// MIT License
// Copyright (c) 2026 Team Radiance

using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Runtime.InteropServices;
using TeamRadiance.Extensions;

namespace TeamRadiance.Groups;

/// <summary>
/// A class that is used to be an advanced collection type by providing a secure way to store elements with convenient utility methods...
/// Note: Because this collection is heavily event-driven, it is best used for high-level gameplay systems (like Inventories) rather than tight, high-speed loops.
/// </summary>
public sealed partial class Group<T> : ICollection<T>
{
    public event Action<T> ItemAdded;
    public event Action<T, int> BulkItemAddition;
    public event Action<T> ItemRemoved;
    public event Action<List<T>> BulkItemRemoval;
    public event Action<IEnumerable<T>, bool> CollectionAdded;
    public event Action GroupCleared;
    public event Action<int> ListTrimmed;
    public event Action<int> LimitChanged;

    public enum TryResultStatus
    {
        Success,
        OutOfRange,
        ItemNotFound,
        InvalidOperation
    }

    public struct TryResult
    {
        public bool DidSucceed;
        public T Result;
        public TryResultStatus Status;
        public string Metadata;
    }

    public bool IsReadOnly => false;
    public string Name { get; private set; } = string.Empty;

    private List<T> _items;

    [JsonPropertyName("Items")]
    public List<T> SerializedItems
    {
        get => _items;
        set => _items = value ?? [];
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();

    public Group()
    {
        _items = [];
        Name = "Loaded Group";
    }

    public Group(int initialSize = 0)
    {
        _items = new(initialSize);
        Name = $"Group {Guid.NewGuid().ToString()[..8]}";
    }

    public Group(int initialSize = 0, string name = null)
    {
        _items = new(initialSize);
        Name = string.IsNullOrEmpty(name) ? $"Group {Guid.NewGuid().ToString()[..8]}" : name;
    }

    public Group(int initialSize = 0, int limit = 1, string name = null)
    {
        _items = new(initialSize);
        Limit = limit;
        if (_items.Count > Limit) TrimExcess();
        Name = string.IsNullOrEmpty(name) ? $"Group {Guid.NewGuid().ToString()[..8]}" : name;
    }

    public Group(string name)
    {
        _items = [];
        Name = string.IsNullOrEmpty(name) ? $"Group {Guid.NewGuid().ToString()[..8]}" : name;
    }

    public int Count => _items.Count;
    public bool GroupIsFull => Limit > -1 && Count >= Limit;

    private int _limit = -1;
    public int Limit
    {
        get => _limit;
        set
        {
            int oldLimit = _limit;
            _limit = value < -1 ? -1 : value;

            if (_limit > -1 && _items.Count > _limit)
            {
                int excessCount = _items.Count - _limit;
                _items.RemoveRange(_limit, excessCount);
            }

            if (_limit != oldLimit) LimitChanged?.Invoke(_limit);
        }
    }

    public void Add(T item)
    {
        if (!CanAddMore()) return;
        _items.Add(item);
        ItemAdded?.Invoke(item);
    }

    public bool TryAdd(T item, bool raiseEvent = true)
    {
        if (item == null || !CanAddMore()) return false;
        
        _items.Add(item);
        if (raiseEvent) ItemAdded?.Invoke(item);
        return true;
    }

    public int AddMultiple(T item, int count)
    {
        if (item == null || count <= 0) return 0;

        int itemsAdded = 0;
        for (int i = 0; i < count; i++)
        {
            if (TryAdd(item, false)) itemsAdded++;
            else break;
        }

        if (itemsAdded > 0) BulkItemAddition?.Invoke(item, itemsAdded);
        return itemsAdded;
    }

    public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

    public bool Remove(T item)
    {
        if (item == null || !_items.Contains(item)) return false;
        
        _items.Remove(item);
        ItemRemoved?.Invoke(item);
        return true;
    }

    public bool TryRemove(T item, bool invokeEvent = true)
    {
        if (item == null || !_items.Contains(item)) return false;

        _items.Remove(item);
        if (invokeEvent) ItemRemoved?.Invoke(item);
        return true;
    }

    public T RemoveAt(int index)
    {
        if (!IsInRange(index)) return default;
        
        T removedItem = _items[index];
        _items.RemoveAt(index);
        ItemRemoved?.Invoke(removedItem);
        return removedItem;
    }

    public bool TryRemoveAt(int index)
    {
        if (!IsInRange(index)) return false;

        T removedItem = _items[index];
        _items.RemoveAt(index);
        ItemRemoved?.Invoke(removedItem);
        return true;
    }

    public bool TryRemoveAt(int index, out object removed)
    {
        if (!IsInRange(index))
        {
            removed = null;
            return false;
        }
        
        T removedItem = _items[index];
        _items.RemoveAt(index);
        ItemRemoved?.Invoke(removedItem);
        removed = removedItem;
        return true;
    }

    public void RemoveMultiple(T item, int count)
    {
        if (item == null || count <= 0) return;

        int itemsRemoved = 0;
        for (int i = 0; i < count; i++)
        {
            if (TryRemove(item, false)) itemsRemoved++;
            else break;
        }

        if (itemsRemoved > 0) BulkItemAddition?.Invoke(item, itemsRemoved);
    }

    public void IndexSwap(int first, int second) => _items.Swap(first, second);
    public void Swap(T first, T second) => _items.Swap(_items.IndexOf(first), _items.IndexOf(second));

    public bool TradeItemAt(Group<T> otherGroup, int myIndex, int theirIndex)
    {
        if (!IsInRange(myIndex) || !otherGroup.IsInRange(theirIndex)) return false;

        T myItem = GetItem(myIndex);
        T theirItem = otherGroup.GetItem(theirIndex);

        Replace(myIndex, theirItem);
        otherGroup.Replace(theirIndex, myItem);
        return true;
    }

    public void Shuffle() => _items.Shuffle();

    public void Clear()
    {
        _items.Clear();
        GroupCleared?.Invoke();
    }

    public Group<T> RetrieveContentsAndClear()
    {
        Group<T> contents = [];
        contents.AddCollection(_items);
        Clear();
        return contents;
    }

    public T GetItem(int index) => !IsInRange(index) ? default : _items[index];

    public bool TryGetItem(int index, out T item)
    {
        if (IsInRange(index))
        {
            item = _items[index];
            return true;
        }
        item = default;
        return false;
    }

    public bool Contains(T item) => _items.Contains(item);
    public int IndexOf(T item) => _items.IndexOf(item);

    public T Find(Func<T, bool> predicate)
    {
        for (int i = 0; i < Count; i++)
        {
            if (predicate(this[i])) return this[i];
        }
        return default;
    }

    public Group<T> FindAll(Func<T, bool> predicate)
    {
        Group<T> results = new(0, $"{Name} (Filtered)");
        for (int i = 0; i < Count; i++)
        {
            if (predicate(this[i])) results.Add(this[i]);
        }
        return results;
    }

    public T GetRandomItem()
    {
        if (_items.Count == 0) throw new ArgumentNullException("Group: Attempted to get random item from an empty group.");
        return _items[TRandom.NextInt(_items.Count)];
    }

    public T GetRandomItemAndRemove()
    {
        if (_items.Count == 0) throw new ArgumentNullException("Group: Attempted to get random item from an empty group.");
        int randomIndex = TRandom.NextInt(_items.Count);
        T item = _items[randomIndex];
        _items.RemoveAt(randomIndex);
        return item;
    }

    public Group<T> GetRandomItems(int count, bool force = false)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero!");
        if (!force && _items.Count < count) throw new ArgumentException("Not enough items in the group to fulfill the request.", nameof(count));

        if (force && _items.Count < count) count = _items.Count;

        HashSet<T> uniqueItems = [];
        while (uniqueItems.Count < count)
        {
            uniqueItems.Add(_items[TRandom.NextInt(_items.Count)]);
        }

        Group<T> newGroup = [];
        newGroup.AddCollection(uniqueItems);
        return newGroup;
    }

    public Group<T> GetRandomItemsAndRemove(int count, bool force = false)
    {
        if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than zero!");
        if (force && _items.Count < count) count = _items.Count;
        else if (!force && _items.Count < count) throw new ArgumentException("Not enough items in the group to fulfill the request.", nameof(count));

        Group<T> newGroup = [];
        List<T> tempItems = [.. _items];

        for (int i = 0; i < count; i++)
        {
            int randomIndex = TRandom.NextInt(tempItems.Count);
            newGroup.Add(tempItems[randomIndex]);
            tempItems.RemoveAt(randomIndex);
        }

        foreach (T item in newGroup) _items.Remove(item);
        return newGroup;
    }

    public Group<U> GetItemsOfType<U>() where U : T
    {
        Group<U> newGroup = [];
        newGroup.AddCollection(_items.OfType<U>());
        return newGroup;
    }

    public void RemoveItemsOfType<U>() where U : T => _items.RemoveAll(element => element is U);

    public Group<U> GetItemsAndRemoveItemsOfType<U>() where U : T
    {
        Group<U> collection = GetItemsOfType<U>();
        RemoveItemsOfType<U>();
        return collection;
    }

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    public ref T GetRef(int index) => ref CollectionsMarshal.AsSpan(_items)[index];

    public T this[int index]
    {
        get
        {
            if (!IsInRange(index)) throw new ArgumentOutOfRangeException("Out of bounds!");
            return _items[index];
        }
        set
        {
            if (!IsInRange(index)) throw new ArgumentOutOfRangeException("Out of bounds!");
            _items[index] = value;
        }
    }

    public void TrimFromStart(int count)
    {
        if (count <= 0) return;
        _items.RemoveRange(0, Math.Min(count, _items.Count));
    }

    public void TrimFromEnd(int count)
    {
        if (count <= 0) return;
        int removeCount = Math.Min(count, _items.Count);
        _items.RemoveRange(_items.Count - removeCount, removeCount);
    }

    public void TrimExcess()
    {
        int oldCount = Count;
        _items.TrimExcess();
        ListTrimmed?.Invoke(oldCount - Count);
    }

    public List<T> ToList() => [.._items];
    public T[] ToArray() => [.. _items];

    public int AddCollection(IEnumerable<T> collection)
    {
        if (collection == null) return 0;

        int itemsAdded = 0;
        bool limitReached = false;
        foreach (T item in collection)
        {
            if (TryAdd(item, false)) itemsAdded++;
            else
            {
                limitReached = true;
                break;
            }
        }

        if (itemsAdded > 0) CollectionAdded?.Invoke(collection, limitReached);
        return itemsAdded;
    }

    public int IndexOf(Func<T, bool> predicate)
    {
        for (int i = 0; i < Count; i++)
        {
            if (predicate(this[i])) return i;
        }
        return -1;
    }

    public void Insert(int index, T item)
    {
        if (index >= 0 && index <= _items.Count) _items.Insert(index, item);
    }

    public bool TryInsert(int index, T item)
    {
        if (index < 0 || index > _items.Count) return false;
        _items.Insert(index, item);
        return true;
    }

    public void Replace(int index, T newItem)
    {
        if (IsInRange(index)) _items[index] = newItem;
    }

    public bool TryReplace(int index, T newItem)
    {
        if (!IsInRange(index)) return false;
        _items[index] = newItem;
        return true;
    }

    public bool IsInRange(int index) => index >= 0 && index < _items.Count;

    public bool IsEmpty() => _items.Count == 0;

    public void Sort(Comparison<T> comparison) => _items.Sort(comparison);

    public bool CanAddMore(int extra = 1) => Limit == -1 || extra + _items.Count <= Limit;

    public T First => IsEmpty() ? default : _items[0];
    public T Last => IsEmpty() ? default : _items[Count - 1];

    public IEnumerable<TResult> Select<TResult>(Func<T, TResult> selector) => _items.Select(selector);

    public ReadOnlyCollection<T> ReadOnly => _items.AsReadOnly();

    public void Move(int oldIndex, int newIndex)
    {
        if (!IsInRange(oldIndex) || !IsInRange(newIndex) || oldIndex == newIndex) return;

        T item = _items[oldIndex];
        _items.RemoveAt(oldIndex);
        _items.Insert(newIndex, item);
    }

    public Group<T> GetSubGroup(int startIndex, int count, bool force = false, string nameOverride = "")
    {
        if (force)
        {
            if (startIndex < 0) startIndex = 0;
            count = startIndex < _items.Count ? Math.Min(count, _items.Count - startIndex) : 0;
        }
        else if (startIndex < 0 || count < 0 || startIndex + count > _items.Count)
        {
            throw new ArgumentOutOfRangeException("Sub-group range is invalid.");
        }

        Group<T> subGroup = new(string.IsNullOrEmpty(nameOverride) ? $"{Name} (Sub-Group)" : nameOverride);
        subGroup.AddCollection(_items.GetRange(startIndex, count));
        return subGroup;
    }

    public int RemoveAll(Predicate<T> predicate)
    {
        List<T> itemsToRemove = [.. _items.Where(item => predicate(item))];
        if (itemsToRemove.Count == 0) return 0;

        int removedCount = _items.RemoveAll(predicate);
        if (removedCount > 0) BulkItemRemoval?.Invoke(itemsToRemove);
        return removedCount;
    }

    public IEnumerable<Group<T>> Chunk(int chunkSize)
    {
        if (chunkSize <= 0) yield break;

        foreach (var chunk in _items.Chunk(chunkSize))
        {
            var newGroup = new Group<T>(0, $"{Name} (Chunk)");
            newGroup.AddCollection(chunk);
            yield return newGroup;
        }
    }

    public Dictionary<TKey, Group<T>> GroupBy<TKey>(Func<T, TKey> keySelector)
    {
        var resultDictionary = new Dictionary<TKey, Group<T>>();
        foreach (var grouping in _items.GroupBy(keySelector))
        {
            var newGroup = new Group<T>(0, $"{Name} (Group: {grouping.Key})");
            newGroup.AddCollection(grouping);
            resultDictionary[grouping.Key] = newGroup;
        }
        return resultDictionary;
    }

    public void Reverse() => _items.Reverse();

    public Group<T> ReverseAndDuplicate()
    {
        Group<T> newGroup = [];
        List<T> reversedItems = [.. _items];
        reversedItems.Reverse();
        newGroup.AddCollection(reversedItems);
        return newGroup;
    }

    public void OrderByAscending<TKey>(Func<T, TKey> keySelector) => _items = [.. _items.OrderBy(keySelector)];

    public Group<T> OrderByAscendingAndDuplicate<TKey>(Func<T, TKey> keySelector)
    {
        Group<T> newGroup = [];
        newGroup.AddCollection(_items.OrderBy(keySelector));
        return newGroup;
    }

    public void OrderByDescending<TKey>(Func<T, TKey> keySelector) => _items = [.. _items.OrderByDescending(keySelector)];

    public Group<T> OrderByDescendingAndDuplicate<TKey>(Func<T, TKey> keySelector)
    {
        Group<T> newGroup = [];
        newGroup.AddCollection(_items.OrderByDescending(keySelector));
        return newGroup;
    }

    public struct ItemResult
    {
        public T Item;
        public int Index;
    }

    public ItemResult GetItemWithIndex(int index)
    {
        if (!IsInRange(index)) throw new ArgumentOutOfRangeException("Index is out of bounds!");

        return new ItemResult
        {
            Item = _items[index],
            Index = index
        };
    }
}

// Welcome to the end of the Group<T> class! We're proud of this one, but just between you and me and your CPU, unless for whatever
// reason people need this in a different language (C/C++/Lua/etc.) I'd rather NOT make this again, especially
// because as of September 11, 2025 I'm still learning the basics of those languages hahaha. But if popular demand
// comes up and this gets successful, I'll do my best (but this class alone was quite the toil). Thank you for taking the time
// to use this class though, as surprising as it is that you scrolled down this far and read this entire thing, and we hope
// you can consider to support Team Radiance in the future anyway possible! :)