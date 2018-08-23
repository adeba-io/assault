using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class MoveSet : SerialDictionary<InputCombo, Techniqu>
{

}

[Serializable]
public class RekkaLinks : SerialDictionary<PlayerInput.FighterInput, Rekka>
{ }


#region SerialDictionary Class Definition
[Serializable]
public class SerialDictionary<TKey, TValue>
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
#endregion
