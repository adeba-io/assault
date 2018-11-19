using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assault.UI
{
    [RequireComponent(typeof(Image))]
    public class PreviewImage : MonoBehaviour
    {
        public Types.PreviewImageType previewImageType;

        public int playerNumber;
        public bool locked;
        
        [SerializeField] Sprite _currentImage;

        Sprite _defaultSprite;

        Image m_image;

        private void Start()
        {
            m_image = GetComponent<Image>();
            _defaultSprite = m_image.sprite;
        }

        public void SetImage(Sprite newPreview)
        {
            if (locked) return;

            _currentImage = newPreview;
            m_image.sprite = newPreview;
        }

        public void ResetImage()
        {
            if (locked) return;

            SetImage(_defaultSprite);
        }

        public void Unlock()
        {
            locked = false;
            ResetImage();
        }
    }
}

namespace Assault.Types
{
    public enum PreviewImageType
    { Stage, FighterCSS, FighterIcon }
}
