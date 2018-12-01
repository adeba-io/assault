using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Boxes;

namespace Assault.Managers
{
    public class PhysicsManager : MonoBehaviour
    {
        public static Vector2 gravity = new Vector2(0, -20f);

        public static Vector2 airFriction = new Vector2(1f, 0);
        public static Vector2 externalFriction = new Vector2(3f, 5f);

        public static float platformContactBuffer = 0.05f;

        static PhysicsManager s_instance;
        static PhysicsManager PM
        {
            get
            {
                if (s_instance != null)
                    return s_instance;

                s_instance = FindObjectOfType<PhysicsManager>();

                if (s_instance != null)
                    return s_instance;

                Create();
                return s_instance;
            }
            set { s_instance = value; }
        }

        static void Create()
        {
            GameObject go = new GameObject("PhysicsManager");
            s_instance = go.AddComponent<PhysicsManager>();
        }

        Dictionary<Collider2D, Rigidbody2D> coll_rigidbodyCache = new Dictionary<Collider2D, Rigidbody2D>();
        Dictionary<Rigidbody2D, Collider2D> rb_colliderCache = new Dictionary<Rigidbody2D, Collider2D>();

        Dictionary<Collider2D, Platform> coll_platformCache = new Dictionary<Collider2D, Platform>();
        Dictionary<Collider2D, FighterController> coll_fighterControllerCache = new Dictionary<Collider2D, FighterController>();

        Dictionary<Rigidbody2D, PhysicsObject> rb_fighterControllerCache = new Dictionary<Rigidbody2D, PhysicsObject>();

        private void Awake()
        {
            if (PM != this)
            {
                Destroy(gameObject);
                return;
            }

            PopulateColliderDictionary(coll_rigidbodyCache);
            PopulateRigidbodyDictionary(rb_colliderCache);

            PopulateColliderDictionary(coll_platformCache);
            PopulateColliderDictionary(coll_fighterControllerCache);

            PopulateRigidbodyDictionary(rb_fighterControllerCache);
        }

        protected void PopulateColliderDictionary<TComponent> (Dictionary<Collider2D, TComponent> dict)
            where TComponent : Component
        {
            TComponent[] components = FindObjectsOfType<TComponent>();

            for (int i = 0; i < components.Length; i++)
            {
                Collider2D[] componentColliders = components[i].GetComponents<Collider2D>();

                for (int j = 0; j < componentColliders.Length; j++)
                    dict.Add(componentColliders[j], components[i]);
            }
        }

        protected void PopulateRigidbodyDictionary<TComponent> (Dictionary<Rigidbody2D, TComponent> dict)
            where TComponent : Component
        {
            TComponent[] components = FindObjectsOfType<TComponent>();

            for (int i = 0; i < components.Length; i++)
            {
                Rigidbody2D[] componentRBs = components[i].GetComponents<Rigidbody2D>();

                for (int j = 0; j < componentRBs.Length; j++)
                    dict.Add(componentRBs[j], components[i]);
            }
        }

        public static bool ColliderHasRigidbody(Collider2D collider) { return PM.coll_rigidbodyCache.ContainsKey(collider); }
        public static bool RigidbodyHasCollider(Rigidbody2D rigidbody) { return PM.rb_colliderCache.ContainsKey(rigidbody); }

        public static bool TryGetColliderRigidbody(Collider2D collider, out Rigidbody2D rigidbody) { return PM.coll_rigidbodyCache.TryGetValue(collider, out rigidbody); }
        public static bool TryGetRigidbodyCollider(Rigidbody2D rigidbody, out Collider2D collider) { return PM.rb_colliderCache.TryGetValue(rigidbody, out collider); }


        public static bool ColliderHasPlatform(Collider2D collider) { return PM.coll_platformCache.ContainsKey(collider); }
        public static bool ColliderHasFighterController(Collider2D collider) { return PM.coll_fighterControllerCache.ContainsKey(collider); }

        public static bool TryGetColliderPlatform(Collider2D collider, out Platform platform) { return PM.coll_platformCache.TryGetValue(collider, out platform); }
        public static bool TryGetColliderFighterController(Collider2D collider, out FighterController fighterController) { return PM.coll_fighterControllerCache.TryGetValue(collider, out fighterController); }


        public static bool RigidbodyHasFighterController(Rigidbody2D rigidbody) { return PM.rb_fighterControllerCache.ContainsKey(rigidbody); }

        public static bool TryGetRigidbodyFighterController(Rigidbody2D rigidbody, out PhysicsObject physicsObject) { return PM.rb_fighterControllerCache.TryGetValue(rigidbody, out physicsObject); }
    }
}
