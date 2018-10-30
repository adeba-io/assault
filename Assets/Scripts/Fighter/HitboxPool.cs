using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Boxes;

namespace Assault
{
    public class HitboxPool : MonoBehaviour
    {
        public InteractionBox _hitboxObject;
        [SerializeField] int _pooledAmount = 10;
        public bool willGrow = true;

        protected List<InteractionBox> _objects;

        private void Start()
        {
            _objects = new List<InteractionBox>();
            for (int i = 0; i < _pooledAmount; i++)
            {
                GameObject go = Instantiate(_hitboxObject.gameObject);
                go.SetActive(false);
                go.transform.SetParent(transform);
                go.transform.position = Vector3.zero;

                _objects.Add(go.GetComponent<InteractionBox>());
            }
        }

        public InteractionBox GetNewHitbox()
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                if (!_objects[i].gameObject.activeInHierarchy)
                {
                    InteractionBox newBox = _objects[i];
                    return newBox;
                }
            }

            if (willGrow)
            {
                GameObject go = Instantiate(_hitboxObject.gameObject, transform);
                InteractionBox box = go.GetComponent<InteractionBox>();
                return box;
            }

            return null;
        }

        public void ReturnHitbox(InteractionBox box)
        {
            if (!_objects.Contains(box)) return;

            box.transform.SetParent(transform);
            box.gameObject.SetActive(false);
        }
    }
}
