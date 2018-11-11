using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Boxes;

namespace Assault
{
    public class HitboxPool : MonoBehaviour
    {
        public Hitbox _hitboxObject;
        [SerializeField] int _pooledAmount = 10;
        public bool willGrow = true;

        protected List<Hitbox> _objects;

        private void Start()
        {
            _objects = new List<Hitbox>();
            for (int i = 0; i < _pooledAmount; i++)
            {
                GameObject go = Instantiate(_hitboxObject.gameObject);
                go.SetActive(false);
                go.transform.SetParent(transform);
                go.transform.localPosition = Vector3.zero;

                _objects.Add(go.GetComponent<Hitbox>());
            }
        }

        public Hitbox GetNewHitbox()
        {
            for (int i = 0; i < _objects.Count; i++)
            {
                if (!_objects[i].gameObject.activeInHierarchy)
                {
                    Hitbox newBox = _objects[i];
                    return newBox;
                }
            }

            if (willGrow)
            {
                GameObject go = Instantiate(_hitboxObject.gameObject, transform);
                Hitbox box = go.GetComponent<Hitbox>();
                return box;
            }

            return null;
        }

        public void ReturnHitbox(Hitbox box)
        {
            if (!_objects.Contains(box)) return;

            box.transform.SetParent(transform);
            box.gameObject.SetActive(false);
        }
    }
}
