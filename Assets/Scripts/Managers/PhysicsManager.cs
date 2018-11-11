using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault
{
    public class PhysicsManager : MonoBehaviour
    {
        public static Vector2 gravity = new Vector2(0, -20f);

        public static Vector2 airFriction = new Vector2(1f, 0);
        public static Vector2 externalFriction = new Vector2(3f, 5f);
    }
}
