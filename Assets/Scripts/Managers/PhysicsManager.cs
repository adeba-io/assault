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

        Dictionary<Collider2D, Platform> platformCache = new Dictionary<Collider2D, Platform>();
        Dictionary<Collider2D, FighterController> fighterControllerCache = new Dictionary<Collider2D, FighterController>();

        private void Awake()
        {
            if (PM != this)
            {
                Destroy(gameObject);
                return;
            }

            PopulateColliderDictionary(platformCache);
            PopulateColliderDictionary(fighterControllerCache);
        }

        protected void PopulateColliderDictionary<TComponent> (Dictionary<Collider2D, TComponent> dict)
            where TComponent : Component
        {
            TComponent[] components = FindObjectsOfType<TComponent>();

            for (int i = 0; i < components.Length; i++)
            {
                Collider2D[] componentColliders = components[i].GetComponents<Collider2D>();

                for (int j = 0; j < componentColliders.Length; j++)
                {
                    dict.Add(componentColliders[j], components[i]);
                }
            }
        }

        public static bool ColliderHasPlatform(Collider2D collider)
        { return PM.platformCache.ContainsKey(collider); }
        public static bool ColliderHasFighterController(Collider2D collider)
        { return PM.fighterControllerCache.ContainsKey(collider); }

        public static bool TryGetPlatform(Collider2D collider, out Platform platform)
        { return PM.platformCache.TryGetValue(collider, out platform); }
        public static bool TryGetFighterController(Collider2D collider, out FighterController fighterController)
        { return PM.fighterControllerCache.TryGetValue(collider, out fighterController); }
    }
}
