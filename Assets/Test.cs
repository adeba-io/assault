﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault
{
    public class Test : MonoBehaviour
    {

        public PlayerController toTest;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            toTest.ForceRigidbody(this, Vector2.zero);
        }
    }
}
