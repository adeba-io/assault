using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public string _poolName = "--- Pooled Objects ---";
    public GameObject _pooledObject;
    [SerializeField] protected int _pooledAmount = 10;
    public bool _willGrow = true;

    protected Transform _poolParent;
    protected List<GameObject> _objects;
    
    private void Reset()
    {
        _poolParent = transform.Find(_poolName);
        GameObject pool = new GameObject(_poolName);
        pool.transform.parent = transform;
        _poolParent = pool.transform;
    }

    private void Start()
    {
        _objects = new List<GameObject>();
        for (int i = 0; i < _pooledAmount; i++)
        {
            GameObject obj = (GameObject)Instantiate(_pooledObject);
            obj.SetActive(false);
            obj.transform.parent = transform;

            _objects.Add(obj);
        }
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < _objects.Count; i++)
        {
            if (!_objects[i].activeInHierarchy)
            {
                return _objects[i];
            }
        }

        if (_willGrow)
        {
            GameObject obj = (GameObject)Instantiate(_pooledObject);
            _objects.Add(obj);
            return obj;
        }

        return null;
    }
}
