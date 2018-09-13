using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;

namespace Assault
{
    public class PlayerInput : InputComponent
    {
        [SerializeField] int _playerNumber = 1;

        public InputGrid Control = new InputGrid(KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.UpArrow, KeyCode.DownArrow, ControllerGrid.LeftStick);

        public InputButton Jump = new InputButton(KeyCode.Space, ControllerButtons.FaceBottom);
        public InputButton AttackLight = new InputButton(KeyCode.F, ControllerButtons.FaceLeft);
        public InputButton AttackHeavy = new InputButton(KeyCode.E, ControllerButtons.FaceTop);
        public InputButton Special = new InputButton(KeyCode.W, ControllerButtons.FaceRight);
        public InputButton Meter = new InputButton(KeyCode.A, ControllerButtons.RightBumper);
        public InputButton Defend = new InputButton(KeyCode.V, ControllerButtons.LeftBumper);

        protected bool _haveControl = true;

        protected PlayerController _playerController;
        protected PlayerFighter _playerFighter;
        protected Damageable _playerDefender;

        protected InputFeed _inputFeed;

        public FighterInput generalInput { get; protected set; }
        public InputCombo currentInputCombo { get; protected set; }

        public bool haveControl { get { return _haveControl; } }
        public int playerNumber
        {
            get { return _playerNumber; }
            set
            {
                _playerNumber = value;
            }
        }

        private void Start()
        {
            _playerController = GetComponent<PlayerController>();
            _playerFighter = GetComponent<PlayerFighter>();
            _playerDefender = GetComponent<Damageable>();

            _inputFeed.Setup();
        }

        protected override void FurtherUpdate()
        {
            UpdateInputFeed();
            Feed();

            _inputFeed.Clear();
        }

        void UpdateInputFeed()
        {
            if (!_playerController) return;

            ControlDirection newDirec = ControlDirection.NEUTRAL;
            ControlDirectionManeuver newDirecManeu = ControlDirectionManeuver.ANY;

            if (Control.Y.Value > 0) newDirec = ControlDirection.Up;
            else if (Control.Y.Value < 0) newDirec = ControlDirection.Down;

            if (Control.X.Value > 0) // We're pointing right
            {
                if (newDirec == ControlDirection.Up)
                {
                    newDirec = (_playerController.facingRight ? ControlDirection.ForwardUp : ControlDirection.BackUp);
                }
                else if (newDirec == ControlDirection.Down)
                {
                    newDirec = (_playerController.facingRight ? ControlDirection.ForwardDown : ControlDirection.BackDown);
                }
                else
                {
                    newDirec = (_playerController.facingRight ? ControlDirection.Forward : ControlDirection.Back);
                }
            }
            else if (Control.X.Value < 0) // Pointing left
            {
                if (newDirec == ControlDirection.Up)
                {
                    newDirec = (!_playerController.facingRight ? ControlDirection.ForwardUp : ControlDirection.BackUp);
                }
                else if (newDirec == ControlDirection.Down)
                {
                    newDirec = (!_playerController.facingRight ? ControlDirection.ForwardDown : ControlDirection.BackDown);
                }
                else
                {
                    newDirec = (!_playerController.facingRight ? ControlDirection.Forward : ControlDirection.Back);
                }
            }

            if (Control.Snap) newDirecManeu = ControlDirectionManeuver.Snap;
            else if (Control.Hard) newDirecManeu = ControlDirectionManeuver.Hard;
            else if (Control.Soft) newDirecManeu = ControlDirectionManeuver.Soft;

            InputButton[] buttonInputs = { Special, AttackHeavy, AttackLight, Jump, Meter, Defend };
            Button newBtn = Button.Any;
            ButtonManeuver newBtnManeu = ButtonManeuver.ANY;

            for (int i = 0; i < buttonInputs.Length; i++)
            {
                InputButton current = buttonInputs[i];

                if (!current.input) continue;

                if (_playerController._currentState != FighterState.Hitstun)
                {
                    if (current == Special) newBtn = Button.Special;
                    else if (current == AttackHeavy) newBtn = Button.AttackHeavy;
                    else if (current == AttackLight) newBtn = Button.AttackLight;
                    else if (current == Jump) newBtn = Button.Jump;
                    else if (current == Meter) newBtn = Button.Meter;
                    else if (current == Defend) newBtn = Button.Defend;
                    else continue;

                    if (current.Down) newBtnManeu = ButtonManeuver.Down;
                    else if (current.Held) newBtnManeu = ButtonManeuver.Held;
                    else if (current.Up) newBtnManeu = ButtonManeuver.Up;
                    else newBtnManeu = ButtonManeuver.ANY;

                    _inputFeed.Add(_playerController._currentState, newDirec, newDirecManeu, newBtn, newBtnManeu);
                }
                else if (current == Defend)
                {
                    newBtn = Button.Defend;

                    if (current.Down) newBtnManeu = ButtonManeuver.Down;
                    else if (current.Held) newBtnManeu = ButtonManeuver.Held;
                    else if (current.Up) newBtnManeu = ButtonManeuver.Up;
                    else newBtnManeu = ButtonManeuver.ANY;

                    _inputFeed.Add(_playerController._currentState, newDirec, newDirecManeu, newBtn, newBtnManeu);
                }
            }
        }

        public override void GainControl()
        {
            _haveControl = true;

            GainControl(Control);

            GainControl(Jump);
            GainControl(AttackLight);
            GainControl(AttackHeavy);
            GainControl(Special);
            GainControl(Meter);
            GainControl(Defend);
        }

        public override void ReleaseControl(bool resetValues = true)
        {
            _haveControl = false;

            ReleaseControl(Control, resetValues);

            ReleaseControl(Jump, resetValues);
            ReleaseControl(AttackLight, resetValues);
            ReleaseControl(AttackHeavy, resetValues);
            ReleaseControl(Special, resetValues);
            ReleaseControl(Meter, resetValues);
            ReleaseControl(Defend, resetValues);
        }

        protected override void GetInputs(bool fixedUpdateHappened)
        {
            Control.StateUpdate(_inputType);

            Jump.StateUpdate(fixedUpdateHappened, _inputType);
            AttackLight.StateUpdate(fixedUpdateHappened, _inputType);
            AttackHeavy.StateUpdate(fixedUpdateHappened, _inputType);
            Special.StateUpdate(fixedUpdateHappened, _inputType);
            Meter.StateUpdate(fixedUpdateHappened, _inputType);
            Defend.StateUpdate(fixedUpdateHappened, _inputType);
        }

        void Feed()
        {
            bool cont = false;

            for (int i = 0; i < _inputFeed.Count; i++)
            {
                Assault.InputCombo currCombo = _inputFeed[i];

                if (!cont)
                {
                    cont = _playerFighter.ReceiveInput(currCombo);

                    if (cont) continue;

                    cont = _playerController.ReceiveInput(currCombo);
                    // cont = _playerDefender.ReceiveInput(currCombo);
                    break;
                }
                else
                {
                    _playerFighter.ReceiveInput(currCombo);
                }
            }
        }

        void AssignPlayerNumberToInputs()
        {
            Jump._playerNumber = _playerNumber;
            AttackLight._playerNumber = _playerNumber;


        }

        protected struct InputFeed
        {
            FighterState _currentState;

            ControlDirection _direction;
            ControlDirectionManeuver _directionManeuver;

            List<Button> _inputButtons;
            List<ButtonManeuver> _buttonManeuvers;

            int _count;

            public int Count { get { return _count; } }

            public InputCombo this[int index]
            {
                get
                {
                    if (index >= _count) return default(InputCombo);

                    InputCombo combo = new InputCombo
                    {
                        direction = _direction,
                        directionManeuver = _directionManeuver,

                        button = _inputButtons[index],
                        buttonManeuver = _buttonManeuvers[index]
                    };
                    return combo;
                }
            }

            public void Setup()
            {
                _inputButtons = new List<Button>();
                _buttonManeuvers = new List<ButtonManeuver>();

                _count = 0;
            }

            public void Add(FighterState state, ControlDirection direc,
                ControlDirectionManeuver direcManeu, Button button, ButtonManeuver buttonManeu)
            {
                _currentState = state;
                _direction = direc;
                _directionManeuver = direcManeu;

                _inputButtons.Add(button);
                _buttonManeuvers.Add(buttonManeu);
                _count++;
            }

            public void Add(InputCombo inputCombo)
            {
                _direction = inputCombo.direction;
                _directionManeuver = inputCombo.directionManeuver;

                _inputButtons.Add(inputCombo.button);
                _buttonManeuvers.Add(inputCombo.buttonManeuver);
                _count++;
            }

            public void Clear()
            {
                _inputButtons.Clear();
                _buttonManeuvers.Clear();

                _count = 0;
            }
        }
    }
}