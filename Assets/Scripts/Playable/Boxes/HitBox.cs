using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class HitBox : MonoBehaviour
{
    public enum HitBoxType
    { HitBox, HurtBox }

    public Action<Collider2D, Damageable> OnHitboxEnter;
    public Action<Collider2D, Damageable> OnHitboxExit;

    public HitBoxType hitboxType;
    public Damageable owner;
    public LayerMask hitMask;

    [HideInInspector] public BoxCollider2D hitbox;

    Rigidbody2D _rigidbody;

    private void Reset()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.bodyType = RigidbodyType2D.Kinematic;

        hitbox = GetComponent<BoxCollider2D>();
        hitbox.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (OnHitboxEnter != null)
        {
            Damageable hitboxOwner = collision.GetComponent<HitBox>().owner;
            if (hitboxOwner)
                OnHitboxEnter(collision, hitboxOwner);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (OnHitboxExit != null)
        {
            Damageable hitboxOwner = collision.GetComponent<HitBox>().owner;
            if (hitboxOwner)
                OnHitboxExit(collision, hitboxOwner);
        }
    }
}
