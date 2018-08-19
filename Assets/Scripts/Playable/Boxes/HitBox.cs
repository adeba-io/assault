using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class HitBox : MonoBehaviour
{
    public enum HitBoxType
    { HitBox, HurtBox }

    public Action<Collider2D, PhysicsObject> OnHitboxEnter;
    public Action<Collider2D, PhysicsObject> OnHitboxExit;

    public HitBoxType hitboxType;
    public PhysicsObject owner;
    public LayerMask hitMask;

    [HideInInspector] public BoxCollider2D hitbox;

    private void Reset()
    {
        hitbox = GetComponent<BoxCollider2D>();
        hitbox.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (OnHitboxEnter != null)
        {
            PhysicsObject hitboxOwner = collision.GetComponent<HitBox>().owner;
            if (hitboxOwner)
                OnHitboxEnter(collision, hitboxOwner);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (OnHitboxExit != null)
        {
            PhysicsObject hitboxOwner = collision.GetComponent<HitBox>().owner;
            if (hitboxOwner)
                OnHitboxExit(collision, hitboxOwner);
        }
    }
}
