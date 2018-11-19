using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assault.Types;

namespace Assault.UI
{
    public class SelectorPanel : MonoBehaviour
    {
        public SelectorType selectorType; 

        public string stageName;

        public GameObject fighterReference;
        [Range(0, 8)]
        public int colorNumber = 0;

        [SerializeField] Sprite _panelSprite;
        public Sprite previewImage;

        public bool selectable;

        Vector2 _min, _max;

        // Use this for initialization
        void Start()
        {
            RectTransform rectTransform = GetComponent<RectTransform>();

            _min = rectTransform.anchoredPosition - rectTransform.rect.max;
            _max = rectTransform.anchoredPosition + rectTransform.rect.max;
        }

        public bool Contains(Vector2 position)
        {
            if (!selectable) return false;

            if (position.x > _min.x && position.x < _max.x && position.y > _min.y && position.y < _max.y)
                return true;

            return false;
        }
    }
}

namespace Assault.Types
{
    public enum SelectorType
    { Stage, Fighter, Confirmation }
}