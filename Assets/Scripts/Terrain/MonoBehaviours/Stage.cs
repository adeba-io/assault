﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault
{
    public class Stage : MonoBehaviour
    {
        public Platform[] _platforms;

        public Transform[] spawnPoints { get; protected set; }

        private void Reset()
        {
            _platforms = GetComponentsInChildren<Platform>();
        }

        private void Awake()
        {
            FindSpawnPoints();
        }
        
        public void FindSpawnPoints()
        {
            spawnPoints = new Transform[_platforms.Length];
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                spawnPoints[i] = _platforms[i].transform;
            }
        }
    }
}
