 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault.Boxes
{
    public class BlastZone : MonoBehaviour
    {
        BlastBox[] _blastBoxes;

        private void Reset()
        {
            string top = "Top", bottom = "Bottom", right = "Right", left = "Left";
            
            if (!transform.Find(top))
            {
                GameObject go = new GameObject(top, typeof(BlastBox));
                go.transform.SetParent(transform);
                go.transform.position = Vector3.up * 10f;
            }

            if (!transform.Find(bottom))
            {
                GameObject go = new GameObject(bottom, typeof(BlastBox));
                go.transform.SetParent(transform);
                go.transform.position = Vector3.down * 10f;
            }

            if (!transform.Find(right))
            {
                GameObject go = new GameObject(right, typeof(BlastBox));
                go.transform.SetParent(transform);
                go.transform.position = Vector3.right * 10f;
            }

            if (!transform.Find(left))
            {
                GameObject go = new GameObject(left, typeof(BlastBox));
                go.transform.SetParent(transform);
                go.transform.position = Vector3.left * 10f;
            }
        }

        private void Awake()
        {
            _blastBoxes = GetComponentsInChildren<BlastBox>();

            for (int i = 0; i < _blastBoxes.Length; i++)
                _blastBoxes[i].onBlastBoxEnter = HitBlastZone;
        }

        void HitBlastZone(Collider2D collision)
        {
            FighterController controller = collision.GetComponent<FighterController>();
            if (controller)
            {
                Destroy(controller.gameObject);
            }
        }
    }
}
