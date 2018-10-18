using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault
{
    public class Test : MonoBehaviour
    {
        public bool flip = false;

        Rigidbody2D rb;

        // Use this for initialization
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            float hori = Input.GetAxis("Horizontal"), vert = Input.GetAxis("Vertical");

            transform.Translate(new Vector3(Random.Range(0, 1), vert) * 3f * Time.deltaTime);
        }
    }
}
