using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Managers;

namespace Assault.UI
{
    [RequireComponent(typeof(Animator))]
    public class SelectionScreen : MonoBehaviour
    {
        [SerializeField] Types.SelectorType _type;
        [SerializeField] int _players;

        [SerializeField] PreviewImage[] _previewSpaces;
        [SerializeField] SelectorPanel[] _selectorPanels;

        SelectorPanel _selectedPanel;

        // Only to be used for Character Select Screen
        public bool[] _madeSelection;
        public SelectorPanel[] _selectedPanels;

        Dictionary<int, bool> _madeSelectio;

        public SelectorPanel[] selectorPanels
        { get { return _selectorPanels; } }

        Animator m_animator;
        readonly int ANIM_CONFIRM = Animator.StringToHash("confirmed");

        private void OnEnable()
        {
            _selectorPanels = FindObjectsOfType<SelectorPanel>();
            
            switch (_type)
            {
                case Types.SelectorType.Stage:
                    _previewSpaces = new[] { FindObjectOfType<PreviewImage>() };
                    break;
                case Types.SelectorType.Fighter:
                    _previewSpaces = FindObjectsOfType<PreviewImage>();
                    break;
            }
            
        }

        private void Start()
        {
            m_animator = GetComponent<Animator>();

            SelectionPointer[] selectionPointers = FindObjectsOfType<SelectionPointer>();
            bool fighterType = _type == Types.SelectorType.Fighter;

            if (fighterType)
            {
                _madeSelection = new bool[selectionPointers.Length];
                _selectedPanels = new SelectorPanel[selectionPointers.Length];
            }

            for (int i = 0; i < selectionPointers.Length; i++)
            {
                SelectionPointer currPointer = selectionPointers[i];
                currPointer.selectionScreen = this;
                currPointer.selectorPanels = _selectorPanels;

                if (fighterType)
                {
                    currPointer.previewSpace = GetPreviewSpace(currPointer.playerNumber);

                    _madeSelection[currPointer.playerNumber - 1] = false;
                }
                else
                {
                    currPointer.previewSpace = _previewSpaces[0];
                }

                if (currPointer.previewSpace)
                    currPointer.previewSpace.locked = false;
            }

            print("Okay");
        }

        PreviewImage GetPreviewSpace(int playerNumber)
        {
            for (int i = 0; i < _previewSpaces.Length; i++)
            {
                if (_previewSpaces[i].playerNumber == playerNumber)
                    return _previewSpaces[i];
            }

            return null;
        }

        public void MakeSelection(SelectionPointer selectionPointer, SelectorPanel panel)
        {
            if (!panel) return;

            switch (_type)
            {
                case Types.SelectorType.Stage:

                    if (panel.selectorType != Types.SelectorType.Confirmation)
                    {
                        m_animator.SetBool(ANIM_CONFIRM, true);
                        _selectedPanel = panel;
                        _previewSpaces[0].locked = true;
                    }
                    else
                        GameManager.GM.SetNextStage(_selectedPanel);

                    break;
                case Types.SelectorType.Fighter:

                    if (panel.selectorType != Types.SelectorType.Confirmation)
                    {
                        _selectedPanels[selectionPointer.playerNumber - 1] = panel;
                        GetPreviewSpace(selectionPointer.playerNumber).locked = true;

                        bool weGood = true;
                        for (int i = 0; i < _selectedPanels.Length; i++)
                            if (!_selectedPanels[i]) weGood = false;

                        if (weGood)
                            m_animator.SetBool(ANIM_CONFIRM, true);
                    }
                    else
                        GameManager.GM.SetFighters(_selectedPanels);

                    break;
            }
        }

        public void Back(SelectionPointer selectionPointer)
        {
            if (m_animator.GetBool(ANIM_CONFIRM))
            {
                m_animator.SetBool(ANIM_CONFIRM, false);

                switch (_type)
                {
                    case Types.SelectorType.Stage:
                        _selectedPanel = null;
                        _previewSpaces[0].Unlock();
                        break;
                    case Types.SelectorType.Fighter:
                        
                        _selectedPanels[selectionPointer.playerNumber - 1] = null;
                        GetPreviewSpace(selectionPointer.playerNumber).Unlock();

                        break;
                }
            }
            else
            {
                switch (_type)
                {
                    case Types.SelectorType.Fighter:

                        bool loadPrevious = true;
                        for (int i = 0; i < _selectedPanels.Length; i++)
                            if (_selectedPanels[i]) loadPrevious = false;

                        if (loadPrevious)
                            GameManager.GM.LoadPrevious();
                        else
                        {
                            _selectedPanels[selectionPointer.playerNumber - 1] = null;
                            GetPreviewSpace(selectionPointer.playerNumber).Unlock();
                        }

                        break;
                    default:
                        GameManager.GM.LoadPrevious();
                        break;
                }
            }
        }

        void EnableSelectorPanels()
        {
            for (int i = 0; i < _selectorPanels.Length; i++)
            {
                _selectorPanels[i].selectable = _selectorPanels[i].selectorType != Types.SelectorType.Confirmation;
            }
        }
        void DisableSelectorPanels()
        {
            for (int i = 0; i < _selectorPanels.Length; i++)
            {
                _selectorPanels[i].selectable = _selectorPanels[i].selectorType == Types.SelectorType.Confirmation;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
