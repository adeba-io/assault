using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assault.UI
{
    public class FadeIn : MonoBehaviour
    {
        public float _fadeTime = 1f;

        Image _image;

        private void Start()
        {
            _image = GetComponent<Image>();
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0);

            InvokeRepeating("Fade", 0, _fadeTime * Time.deltaTime);
        }

        void Fade()
        {
            if (_fadeTime <= 0)
            {
                CancelInvoke("Fade");
            }

            Color newColor = _image.color;
            newColor.a += _fadeTime * Time.deltaTime;
            _image.color = newColor;

            _fadeTime += Time.deltaTime;
        }
    }
}
