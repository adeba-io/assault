﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    [SerializeField] static Vector2 _gravity = new Vector2(0, -20f);
    [SerializeField] static float _airFriction = 3f;

    public static Vector2 gravity { get { return _gravity; } }
    public static float airFriction { get { return _airFriction; } }
}
