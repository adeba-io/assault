using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Boxes;
using Assault.Techniques;
using Assault.Managers;

namespace Assault
{
    public class FighterDamager : MonoBehaviour
    {
        [SerializeField] float _meter;

        Joint[] _joints;

        HitboxPool _hitboxPool;

        List<Hitbox> _currentHitboxes;

        FighterController _controller;
        FighterDamageable _damageable;

        HitboxData _hitboxData;

        private void Reset()
        {
            Transform joints = transform.Find("--- Joints ---");
            if (!joints)
            {
                joints = new GameObject("--- Joints ---").transform;
                joints.SetParent(transform);
            }
            joints.localPosition = Vector3.zero;

            Transform hitboxes = transform.Find("--- Hitboxes ---");
            if (!hitboxes)
            {
                hitboxes = new GameObject("--- Hitboxes ---", typeof(HitboxPool)).transform;
                hitboxes.SetParent(transform);
            }
            hitboxes.localPosition = Vector3.zero;
        }

        private void Start()
        {
            _hitboxPool = GetComponentInChildren<HitboxPool>();
            _controller = GetComponent<FighterController>();
            _damageable = GetComponent<FighterDamageable>();

            _hitboxData = new HitboxData { damager = this };

            _joints = GetComponentsInChildren<Joint>();
            _currentHitboxes = new List<Hitbox>();
        }

        void Assault(Attack attackData, FighterDamageable damageable)
        {
            // Don't want to send the reference as something may change before the calc
            HitboxData dataToSend = _hitboxData;
            dataToSend.attack = attackData;
            dataToSend.facingRight = _controller.facingRight;
            
            if (damageable != _damageable)
                FighterManager.FM.Assault(dataToSend, damageable.hurtboxData);
        }

        public void ActivateHitbox(Attack attackData, InteractionBoxData boxData, int jointID)
        {
            Hitbox newBox = _hitboxPool.GetNewHitbox();

            for (int i = 0; i < _joints.Length; i++)
            {
                if (_joints[i].ID == jointID)
                {
                    boxData.parent = _joints[i].transform;
                    break;
                }
            }

            newBox.Enable(gameObject, boxData, attackData, Assault);

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

    [System.Serializable]
    public struct HitboxData
    {
        public FighterDamager damager;
        public bool facingRight;
        public Attack attack;
    }
}
