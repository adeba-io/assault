using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


#region SerialDictionary Class Definition
[Serializable]
public class SerialDictionary<TKey, TValue>// : ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
{
    [SerializeField]
    int _count = 0;

    [SerializeField]
    List<TKey> _keys;

    [SerializeField]
    List<TValue> _values;

    public TValue this[TKey key]
    {
        get
        {
            if (!_keys.Contains(key)) return default(TValue);

            int index = _keys.IndexOf(key);
            return _values[index];
        }
        set
        {
            int index;
            if (_keys.Contains(key))
            {
                index = _keys.IndexOf(key);
                _values[index] = value;
            }
        }
    }

    public int Count { get { return _count; } }

    public SerialDictionary()
    {
        _keys = new List<TKey>();
        _values = new List<TValue>();
    }

    public SerialDictionary(int size)
    {
        _keys = new List<TKey>(size);
        _values = new List<TValue>(size);
        _count = size;
    }

    public void Add(TKey key, TValue value)
    {
        if (_keys.Contains(key)) return;

        _keys.Add(key);
        _values.Add(value);
        _count++;
    }

    public void Remove(TKey key)
    {
        if (!_keys.Contains(key)) return;

        int index = _keys.IndexOf(key);

        _keys.RemoveAt(index);
        _values.RemoveAt(index);
        _count--;
    }
}

[Serializable]
public class Dict<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>
{
    public TValue this[TKey key]
    {
        get
        {
            if (!_keys.Contains(key)) return default(TValue);

            int index = _keys.IndexOf(key);
            return _values[index];
        }
        set
        {
            if (_keys.Contains(key))
            {
                int index = _keys.IndexOf(key);
                _values[index] = value;
            }
        }
    }

    [SerializeField] int _count = 0;
    [SerializeField] List<TKey> _keys = new List<TKey>();
    [SerializeField] List<TValue> _values = new List<TValue>();

    public int Count { get { return _count; } }

    public bool IsReadOnly { get; set; }

    public ICollection<TKey> Keys { get { return _keys; } }

    public ICollection<TValue> Values { get { return _values; } }

    public Dict()
    {
        _keys = new List<TKey>();
        _values = new List<TValue>();
        _count = 0;
    }

    public Dict(int capacity)
    {
        _keys = new List<TKey>(capacity);
        _values = new List<TValue>(capacity);
        _count = capacity;
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        if (Keys.Contains(item.Key)) return;

        _keys.Add(item.Key);
        _values.Add(item.Value);
        _count++;
    }

    public void Add(TKey key, TValue value)
    {
        if (Keys.Contains(key)) return;

        _keys.Add(key);
        _values.Add(value);
    }

    public void Clear()
    {
        _keys.Clear();
        _values.Clear();
        _count = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
         return _keys.Contains(item.Key) || _values.Contains(item.Value);
    }

    public bool ContainsKey(TKey key)
    {
        return _keys.Contains(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new DictEnum<TKey, TValue>(this);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (_keys.Contains(item.Key) && _values.Contains(item.Value))
        {
            if (_keys.IndexOf(item.Key) == _values.IndexOf(item.Value))
            {
                int index = _keys.IndexOf(item.Key);
                _keys.RemoveAt(index);
                _values.RemoveAt(index);
                _count--;
                return true;
            }
        }

        return false;
    }

    public bool Remove(TKey key)
    {
        if (_keys.Contains(key))
        {
            int index = _keys.IndexOf(key);
            _keys.RemoveAt(index);
            _values.RemoveAt(index);
            _count--;
            return true;
        }

        return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (_keys.Contains(key))
        {
            int index = _keys.IndexOf(key);
            value = _values[index];
            return true;
        }

        value = default(TValue);
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return (IEnumerator)GetEnumerator();
    }
}

public class DictEnum<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>
{
    Dict<TKey, TValue> _dictionary;

    TKey _next;

    public KeyValuePair<TKey, TValue> Current
    {
        get
        {
            try
            {
                return new KeyValuePair<TKey, TValue>(_next, _dictionary[_next]);
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
        }
    }

    object IEnumerator.Current { get { return Current; } }

    public DictEnum(Dict<TKey, TValue> dictionary)
    {
        _dictionary = dictionary;
    }
    
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public bool MoveNext()
    {
        throw new NotImplementedException();
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }
}
#endregion
