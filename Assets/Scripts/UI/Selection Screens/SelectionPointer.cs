using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assault.UI
{
    [RequireComponent(typeof(SelectionInput))]
    public class SelectionPointer : MonoBehaviour
    {
        public float _movementSpeed = 50f;
        public RectTransform selectPanel;

        public SelectorPanel[] selectorPanels { get; set; }

        SelectionInput Input;
        RectTransform _rectTransform;

        private void Start()
        {
            Input = GetComponent<SelectionInput>();
            _rectTransform = GetComponent<RectTransform>();
            Debug.Log(selectPanel.rect);
        }

        private void Update()
        {
            Vector2 moveVector = Input.Control.Value;

            moveVector *= _movementSpeed * Time.deltaTime;
            transform.Translate(moveVector);

            MakeSelection();
        }

        void MakeSelection()
        {
            if (Input.Confirm.Down)
            {
                Debug.Log("Trying");

                for (int i = 0; i < selectorPanels.Length; i++)
                {
                    if (selectorPanels[i].Contains(_rectTransform.anchoredPosition))
                    {

                        break;
                    }
                }
            }
        }
    }
}

