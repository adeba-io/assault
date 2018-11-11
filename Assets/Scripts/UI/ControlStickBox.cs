using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assault
{
    public class ControlStickBox : MonoBehaviour
    {
        public FighterInput Input;

        public Image _stickPlacement;
        public Image _hardRegion;

        public Text _alteredX, _alteredY;
        public Text _rawX, _rawY;

        public Image _snapX, _snapY;
        public Image _doubleSnapX, _doubleSnapY;

        int _snapStartX, _snapStartY;
        int _dSnapStartX, _dSnapStartY;
        int _snapInterval = 3;

        RectTransform _stickRectTransform;

        Vector2 _defaultPosition;

        private void Start()
        {
            _stickRectTransform = _stickPlacement.GetComponent<RectTransform>();

            _defaultPosition = _stickRectTransform.anchoredPosition;
        }

        private void Update()
        {
            Vector2 currVal = Input.Control.Value;

            _stickRectTransform.anchoredPosition = _defaultPosition + (currVal * 120f);

            _alteredX.text = Input.Control.X.Value.ToString();
            _alteredY.text = Input.Control.Y.Value.ToString();

            _rawX.text = Input.Control.Raws.X.ToString();
            _rawY.text = Input.Control.Raws.Y.ToString();

            SnapX();
            SnapY();
        }

        void SnapX()
        {
            if (Input.Control.X.Snap)
            {
                _snapStartX = Time.frameCount;
                _snapX.color = Color.green;
            }

            if (Input.Control.X.DoubleSnap)
            {
                print(Time.frameCount);
                _dSnapStartX = Time.frameCount;
                _doubleSnapX.color = Color.green;
            }

            if (Time.frameCount - _snapStartX == _snapInterval)
            { _snapX.color = Color.red; }

            if (Time.frameCount - _dSnapStartX == _snapInterval)
            { _doubleSnapX.color = Color.red; }
        }

        void SnapY()
        {
            if (Input.Control.Y.Snap)
            {
                _snapStartY = Time.frameCount;
                _snapY.color = Color.green;
            }

            if (Input.Control.Y.DoubleSnap)
            {
                print(Time.frameCount);
                _dSnapStartY = Time.frameCount;
                _doubleSnapY.color = Color.green;
            }

            if (Time.frameCount - _snapStartY == _snapInterval)
            { _snapY.color = Color.red; }

            if (Time.frameCount - _dSnapStartY == _snapInterval)
            { _doubleSnapY.color = Color.red; }
        }
    }
}
