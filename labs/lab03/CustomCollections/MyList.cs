using System;
using System.Collections;
using System.Collections.Generic;

public class MyList<T> : IList<T>, ICollection<T>, IEnumerable<T>
{
    private T[] _items;
    private int _size;
    private int _version;

    private const int DefaultCapacity = 4;

    public MyList()
    {
        _items = new T[DefaultCapacity];
    }

    public MyList(int capacity)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
        _items = new T[capacity];
    }

    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index));
            return _items[index];
        }
        set
        {
            if (index < 0 || index >= _size)
                throw new ArgumentOutOfRangeException(nameof(index));
            _items[index] = value;
            _version++;
        }
    }

    public int Count => _size;

    public bool IsReadOnly => false;

    public void Add(T item)
    {
        if (_size == _items.Length)
            EnsureCapacity(_size + 1);

        _items[_size] = item;
        _size++;
        _version++;
    }

    public void Clear()
    {
        if (_size > 0)
        {
            Array.Clear(_items, 0, _size);
            _size = 0;
        }
        _version++;
    }

    public bool Contains(T item)
    {
        return IndexOf(item) >= 0;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array == null) throw new ArgumentNullException(nameof(array));
        if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
        if (array.Length - arrayIndex < _size)
            throw new ArgumentException("Array too small");

        Array.Copy(_items, 0, array, arrayIndex, _size);
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _size; i++)
        {
            yield return _items[i];
        }
    }

    public int IndexOf(T item)
    {
        for (int i = 0; i < _size; i++)
        {
            if (EqualityComparer<T>.Default.Equals(_items[i], item))
                return i;
        }
        return -1;
    }

    public void Insert(int index, T item)
    {
        if (index < 0 || index > _size)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (_size == _items.Length)
            EnsureCapacity(_size + 1);

        if (index < _size)
            Array.Copy(_items, index, _items, index + 1, _size - index);

        _items[index] = item;
        _size++;
        _version++;
    }

    public bool Remove(T item)
    {
        int index = IndexOf(item);
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= _size)
            throw new ArgumentOutOfRangeException(nameof(index));

        _size--;
        if (index < _size)
            Array.Copy(_items, index + 1, _items, index, _size - index);

        _items[_size] = default!;
        _version++;
    }

    private void EnsureCapacity(int min)
    {
        if (_items.Length < min)
        {
            int newCapacity = _items.Length == 0 ? DefaultCapacity : _items.Length * 2;
            if (newCapacity < min) newCapacity = min;

            Array.Resize(ref _items, newCapacity);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}