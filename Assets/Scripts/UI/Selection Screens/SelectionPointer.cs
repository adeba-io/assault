using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assault.UI
{
    [RequireComponent(typeof(SelectionInput))]
    public class SelectionPointer : MonoBehaviour
    {
        [SerializeField] float _movementSpeed = 300f;
        [SerializeField] int _playerNumber;
        
        public SelectorPanel _currPanel;

        public int playerNumber { get { return _playerNumber; } }
        public SelectionScreen selectionScreen { get; set; }
        public SelectorPanel[] selectorPanels { get; set; }
        public PreviewImage previewSpace;

        SelectionInput Input;
        RectTransform _rectTransform;

        private void Awake()
        {
            Input = GetComponent<SelectionInput>();
            _rectTransform = GetComponent<RectTransform>();
            
            SetPlayerNumber(_playerNumber);
        }

        private void Update()
        {
            if (Move())
                HoverOver();

            MakeSelection();
            Deconfirm();
        }

        bool Move()
        {
            Vector2 moveVector = Input.Control.Value;

            if (moveVector != Vector2.zero)
            {
                moveVector *= _movementSpeed * Time.deltaTime;
                transform.Translate(moveVector);
                return true;
            }
            
            return false;
        }

        void HoverOver()
        {
            Debug.Log(selectorPanels[0].selectable);
            for (int i = 0; i < selectorPanels.Length; i++)
            {
                if (selectorPanels[i].Contains(_rectTransform.anchoredPosition))
                {
                    print("Ya");
                    _currPanel = selectorPanels[i];
                    if (previewSpace)
                        previewSpace.SetImage(_currPanel.previewImage);
                    return;
                }
            }

            _currPanel = null;
            if (previewSpace) previewSpace.ResetImage();
        }

        void MakeSelection()
        {
            if (Input.Confirm.Down && _currPanel)
            {
                selectionScreen.MakeSelection(this, _currPanel);
            }
        }
        
        void Deconfirm()
        {
            if (!Input.Back.Down) return;

            selectionScreen.Back(this);
        }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
            Input.SetPlayerNumber(playerNumber);
        }
    }
}

