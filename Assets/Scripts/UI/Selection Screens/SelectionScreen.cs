using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault.UI
{
    public class SelectionScreen : MonoBehaviour
    {
        [SerializeField] SelectorPanel[] _selectorPanels;
        
        public SelectorPanel[] selectorPanels
        { get { return _selectorPanels; } }

        private void Awake()
        {
            _selectorPanels = FindObjectsOfType<SelectorPanel>();
        }

        private void Start()
        {
            SelectionPointer[] selectionPointers = FindObjectsOfType<SelectionPointer>();
            for (int i = 0; i < selectionPointers.Length; i++)
            {
                selectionPointers[i].selectorPanels = _selectorPanels;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
