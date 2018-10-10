using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault
{
    public class Test : MonoBehaviour
    {
        public bool flip = false;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            transform.position = new Vector3(transform.position.x + Time.deltaTime, transform.position.y, transform.position.z);

            if (flip)
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                flip = false;
            }
        }
    }
}
