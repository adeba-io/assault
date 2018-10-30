using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Boxes;

namespace Assault
{
    public class FighterDamager : MonoBehaviour
    {
        Joint[] _joints;
        Transform _hitboxes;

        HitboxPool _hitboxPool;

        List<InteractionBox> _currentHitboxes;

        private void Reset()
        {
            Transform joints = transform.Find("--- Joints ---");
            if (!joints)
            {
                joints = new GameObject("--- Joints ---").transform;
                joints.transform.SetParent(transform);
            }
            joints.transform.localPosition = Vector3.zero;

            Transform hitboxes = transform.Find("--- Hitboxes ---");
            if (!hitboxes)
            {
                hitboxes = new GameObject("--- Hitboxes ---", typeof(HitboxPool)).transform;
                hitboxes.transform.SetParent(transform);
            }
            hitboxes.transform.localPosition = Vector3.zero;
        }

        private void Start()
        {
            _hitboxPool = GetComponentInChildren<HitboxPool>();

            _joints = GetComponentsInChildren<Joint>();
            _currentHitboxes = new List<InteractionBox>();
        }

        public void Assault(Collider2D collider)
        {

        }

        public void ActivateHitbox(InteractionBoxData boxData, int jointID)
        {
            InteractionBox newBox = _hitboxPool.GetNewHitbox();

            for (int i = 0; i < _joints.Length; i++)
            {
                if (_joints[i].GetInstanceID() == jointID)
                {
                    boxData.parent = _joints[i].transform;
                    break;
                }
            }

            newBox.Enable(gameObject, boxData);

            _currentHitboxes.Add(newBox);
        }

        public void DeactivateHitbox(int hitboxID)
        {
            for (int i = 0; i < _currentHitboxes.Count; i++)
            {
                if (_currentHitboxes[i].boxData.ID == hitboxID)
                {
                    _hitboxPool.ReturnHitbox(_currentHitboxes[i]);
                    _currentHitboxes.RemoveAt(i);
                }
            }
        }
    }
}
