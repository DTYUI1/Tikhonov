using System;
using System.Collections;
using System.Collections.Generic;

public class MyDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    private struct Entry
    {
        public int HashCode;
        public int Next;
        public TKey Key;
        public TValue Value;
    }

    private int[] _buckets;
    private Entry[] _entries;
    private int _count;
    private int _freeList;
    private int _freeCount;
    private int _version;

    private const int DefaultCapacity = 4;

    public MyDictionary() : this(DefaultCapacity) { }

    public MyDictionary(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        Initialize(capacity);
    }

    public TValue this[TKey key]
    {
        get
        {
            if (TryGetValue(key, out TValue? value))
                return value;
            throw new KeyNotFoundException();
        }
        set
        {
            Insert(key, value, false);
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            var keys = new MyList<TKey>();
            foreach (var entry in this)
                keys.Add(entry.Key);
            return keys;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            var values = new MyList<TValue>();
            foreach (var entry in this)
                values.Add(entry.Value);
            return values;
        }
    }

    public int Count => _count - _freeCount;

    public bool IsReadOnly => false;

    public void Add(TKey key, TValue value)
    {
        Insert(key, value, true);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        if (_count > 0)
        {
            Array.Clear(_buckets, 0, _buckets.Length);
            Array.Clear(_entries, 0, _count);
            _count = 0;
            _freeList = -1;
            _freeCount = 0;
        }
        _version++;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (TryGetValue(item.Key, out TValue? value))
            return EqualityComparer<TValue>.Default.Equals(value, item.Value);
        return false;
    }

    public bool ContainsKey(TKey key)
    {
        return FindEntry(key) >= 0;
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < Count)
            throw new ArgumentException("Array too small");

        int index = 0;
        for (int i = 0; i < _count; i++)
        {
            if (_entries[i].HashCode >= 0)
            {
                array[arrayIndex + index] = new KeyValuePair<TKey, TValue>(_entries[i].Key, _entries[i].Value);
                index++;
            }
        }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
        {
            if (_entries[i].HashCode >= 0)
            {
                yield return new KeyValuePair<TKey, TValue>(_entries[i].Key, _entries[i].Value);
            }
        }
    }

    public bool Remove(TKey key)
    {
        if (_buckets == null) return false;

        int hashCode = key.GetHashCode() & 0x7FFFFFFF;
        int bucket = hashCode % _buckets.Length;
        int last = -1;

        for (int i = _buckets[bucket]; i >= 0; last = i, i = _entries[i].Next)
        {
            if (_entries[i].HashCode == hashCode && EqualityComparer<TKey>.Default.Equals(_entries[i].Key, key))
            {
                if (last < 0)
                    _buckets[bucket] = _entries[i].Next;
                else
                    _entries[last].Next = _entries[i].Next;

                _entries[i].HashCode = -1;
                _entries[i].Next = _freeList;
                _entries[i].Key = default!;
                _entries[i].Value = default!;

                _freeList = i;
                _freeCount++;
                _version++;
                return true;
            }
        }
        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (Contains(item))
            return Remove(item.Key);
        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        int i = FindEntry(key);
        if (i >= 0)
        {
            value = _entries[i].Value;
            return true;
        }
        value = default!;
        return false;
    }

    private int FindEntry(TKey key)
    {
        if (_buckets == null) return -1;

        int hashCode = key.GetHashCode() & 0x7FFFFFFF;

        for (int i = _buckets[hashCode % _buckets.Length]; i >= 0; i = _entries[i].Next)
        {
            if (_entries[i].HashCode == hashCode && EqualityComparer<TKey>.Default.Equals(_entries[i].Key, key))
                return i;
        }
        return -1;
    }

    private void Insert(TKey key, TValue value, bool add)
    {
        if (_buckets == null) Initialize(DefaultCapacity);

        int hashCode = key.GetHashCode() & 0x7FFFFFFF;
        int targetBucket = hashCode % _buckets.Length;

        for (int i = _buckets[targetBucket]; i >= 0; i = _entries[i].Next)
        {
            if (_entries[i].HashCode == hashCode && EqualityComparer<TKey>.Default.Equals(_entries[i].Key, key))
            {
                if (add) throw new ArgumentException("An element with the same key already exists");

                _entries[i].Value = value;
                _version++;
                return;
            }
        }

        int index;
        if (_freeCount > 0)
        {
            index = _freeList;
            _freeList = _entries[index].Next;
            _freeCount--;
        }
        else
        {
            if (_count == _entries.Length)
            {
                Resize();
                targetBucket = hashCode % _buckets.Length;
            }
            index = _count;
            _count++;
        }

        _entries[index].HashCode = hashCode;
        _entries[index].Next = _buckets[targetBucket];
        _entries[index].Key = key;
        _entries[index].Value = value;
        _buckets[targetBucket] = index;
        _version++;
    }

    private void Initialize(int capacity)
    {
        int size = capacity;
        _buckets = new int[size];
        for (int i = 0; i < size; i++) _buckets[i] = -1;
        _entries = new Entry[size];
        _freeList = -1;
    }

    private void Resize()
    {
        int newSize = _count * 2;
        var newEntries = new Entry[newSize];
        Array.Copy(_entries, 0, newEntries, 0, _count);

        var newBuckets = new int[newSize];
        for (int i = 0; i < newSize; i++) newBuckets[i] = -1;

        for (int i = 0; i < _count; i++)
        {
            if (newEntries[i].HashCode >= 0)
            {
                int bucket = newEntries[i].HashCode % newSize;
                newEntries[i].Next = newBuckets[bucket];
                newBuckets[bucket] = i;
            }
        }

        _buckets = newBuckets;
        _entries = newEntries;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}