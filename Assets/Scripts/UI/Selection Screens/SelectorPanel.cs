using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assault.Types;

namespace Assault.UI
{
    [RequireComponent(typeof(Image))]
    public class SelectorPanel : MonoBehaviour
    {
        public SelectorType selectorType; 

        [SerializeField] string _stageName;

        [SerializeField] GameObject _fighterReference;
        [SerializeField] [Range(0, 8)]
        int _colorNumber = 0;

        public RectTransform rectTransform { get; protected set; }
        public Rect panelRect { get { return rectTransform.rect; } }

        Vector2 _min, _max;

        // Use this for initialization
        void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            //Debug.Log(name + "; Rect: " + rectTransform.rect + "; Position: " + rectTransform.anchoredPosition);

            _min = rectTransform.anchoredPosition - rectTransform.rect.max;
            _max = rectTransform.anchoredPosition + rectTransform.rect.max;

            Debug.Log(name + "; Min: " + _min + "; Max: " + _max);
        }

        public bool Contains(Vector2 position)
        {
            if (position.x > _min.x && position.x < _max.x && position.y > _min.y && position.y < _max.y)
                return true;

            return false;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

namespace Assault.Types
{
    public enum SelectorType
    { Stage, Fighter }
}