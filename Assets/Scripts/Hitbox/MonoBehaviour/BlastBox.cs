using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault.Boxes
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class BlastBox : MonoBehaviour
    {
        public Action<Collider2D> onBlastBoxEnter;
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (onBlastBoxEnter != null)
            {
                onBlastBoxEnter(collision);
            }
        }
    }
}
