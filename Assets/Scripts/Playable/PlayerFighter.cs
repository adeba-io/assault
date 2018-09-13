using System;
using System.Collections;
using System.Collections.Generic;
using Assault;
using UnityEngine;
using UnityEngine.Events;

namespace Assault
{
    [RequireComponent(typeof(HitboxPool))]
    public class PlayerFighter : MonoBehaviour
    {
        [Serializable]
        public class DamageableEvent : UnityEvent<PlayerFighter, Damageable>
        { }

        [Serializable]
        public class NonDamageableEvent : UnityEvent<PlayerFighter>
        { }

        [SerializeField] bool _canDamage;

        PlayerController _playerController;
        Damageable _playerDefender;
        HitboxPool _hitboxPool;

        public Techniqe _currentTechnique = null;
        Queue<InputCombo> _inputQueue = new Queue<InputCombo>();

        public MoveSet moveset = new MoveSet();
        public List<Techniqe> _techniques = new List<Techniqe>();

        public void EnableFighter() { _canDamage = true; }
        public void DisableFighter() { _canDamage = false; }

        public void Attack()
        {

        }
        
        private void Update()
        {
            if (_currentTechnique == null)
            {
                InputCombo curr = _inputQueue.Dequeue();
                int currNode = moveset[curr];

                _currentTechnique = _techniques[currNode];
                if (_currentTechnique != null) _currentTechnique.Initialize();
            }
            else if (_currentTechnique.canCancel)
            {
                InputCombo curr = _inputQueue.Dequeue();
                int currNode = _currentTechnique.links[curr];

                _currentTechnique = _techniques[currNode];
                if (_currentTechnique != null) _currentTechnique.Initialize();
            }

            _currentTechnique.Update();
        }

        public bool ReceiveInput(InputCombo inputCombo)
        {
            if (_currentTechnique == null)
            {
                if (!_inputQueue.Contains(inputCombo))
                    _inputQueue.Enqueue(inputCombo);

                return true;
            }

            return false;
        }
    }


    [Serializable]
    public class MoveSet : Dictionary<InputCombo, int>
    { }
}

