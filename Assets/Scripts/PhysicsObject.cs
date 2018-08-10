using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PhysicsObject : MonoBehaviour
{
    #region Internal Types

    struct RaycastOrigins
    {
        public Vector2 centralRaycastPoint;

        public Vector2 topLeft, bottomLeft, bottomRight;
    }

    public class CollisionState
    {
        public bool above, below;
        public bool left, right;

        public bool groundedLastFrame, groundedThisFrame;
        public Platform groundPlatform;

        public float groundSlopeAngle { get { return groundPlatform.slopeAngle; } }
        public bool hasCollision { get { return above || below || left || right; } }

        public void Reset()
        {
            groundedLastFrame = below;
            above = below = left = right = groundedThisFrame = false;
            groundPlatform = null;
        }

        public override string ToString()
        {
            return string.Format("[CollisionState] = a: {0}, b: {1}, l: {2}, r: {3}, groundedLastFrame: {4}, groundedThisFrame: {5}, Curr Platform: {6}, Curr Slope Angle: {7}",
                                above, below, left, right, groundedLastFrame, groundedThisFrame, groundPlatform.name, groundSlopeAngle);
        }
    }

    #endregion



    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
