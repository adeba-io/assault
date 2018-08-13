using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    static Vector2 _gravity = new Vector2(0, -9.81f);
    static float _airFriction = 3f;

    public static Vector2 gravity { get { return _gravity; } }
    public static float airFriction { get { return _airFriction; } }
}
