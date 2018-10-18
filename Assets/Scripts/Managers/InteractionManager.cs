using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault.Managers
{
    public class InteractionManager : MonoBehaviour
    {
        public static InteractionManager IM { get; protected set; }

        public void Awake()
        {
            if (IM == null)
            {
                IM = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }
    }
}
