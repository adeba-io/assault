using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault
{
    public class ObjectPool : MonoBehaviour
    {
        public GameObject _pooledObject;
        [SerializeField] protected int _pooledAmount = 10;
        public bool _willGrow = true;
        
        protected List<GameObject> _objects;
        
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
}
