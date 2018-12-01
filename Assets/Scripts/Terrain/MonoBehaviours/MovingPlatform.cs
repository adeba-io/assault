using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;

namespace Assault
{
    [SelectionBase]
    [RequireComponent(typeof(Platform))]
    public class MovingPlatform : Platform
    {
        [SerializeField] MovingPlatformType _type;
        [SerializeField] float _speed = 1.0f;

        [SerializeField] bool _startMovingOnlyWhenVisible;
        [SerializeField] bool _isMovingAtStart = true;

        [HideInInspector]
        public Vector2[] _localNodes = new Vector2[1]; // Only used during EditTime
        [SerializeField] float[] _waitTimes = new float[1];

        [SerializeField] protected Vector2[] _worldNodes;

        protected int _current = 0;
        protected int _next = 0;
        protected int _direction = 1;

        protected float _waitTime = -1.0f;

        protected Vector2 _velocity;

        protected bool _started = false;
        protected bool _veryFirstStart = false;

        public Vector2 velocity { get { return _velocity; } }
        public Vector2[] worldNodes {  get { return _worldNodes; } }

        private void Reset()
        {
            // Must always have at least one node, the local position
            _localNodes[0] = Vector2.zero;
            _waitTimes[0] = 0;
        }

        private void Start()
        {
            // We make a point in the path being defined in local space so designer can move the platform and path together
            // but as the platform will move during gameplay, that would also move the node. So we convert the localNodes
            // (which are only used during edit time) to world position (which are only used at runtime)
            _worldNodes = new Vector2[_localNodes.Length];
            for (int i = 0; i < _worldNodes.Length; i++)
                _worldNodes[i] = transform.TransformPoint(_localNodes[i]);

            Init();
        }

        protected void Init()
        {
            _current = 0;
            _direction = 1;
            _next = _localNodes.Length > 1 ? 1 : 0;

            _waitTime = _waitTimes[0];

            _veryFirstStart = false;
            if (_isMovingAtStart)
            {
                _started = !_startMovingOnlyWhenVisible;
                _veryFirstStart = true;
            }
            else
                _started = false;
        }
    }
}
