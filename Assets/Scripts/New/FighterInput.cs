using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;
using Assault.Maneuvers;

namespace Assault
{
    public class FighterInput : InputComponent
    {
        public InputGrid Control = new InputGrid(KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.UpArrow, KeyCode.DownArrow, ControllerGrid.LeftStick);

        public InputButton Jump = new InputButton(KeyCode.Space, ControllerButtons.FaceBottom);
        public InputButton AttackLight = new InputButton(KeyCode.F, ControllerButtons.FaceLeft);
        public InputButton AttackHeavy = new InputButton(KeyCode.E, ControllerButtons.FaceTop);
        public InputButton Special = new InputButton(KeyCode.W, ControllerButtons.FaceRight);
        public InputButton Meter = new InputButton(KeyCode.A, ControllerButtons.RightBumper);
        public InputButton Defend = new InputButton(KeyCode.V, ControllerButtons.LeftBumper);

        protected bool _haveControl = true;

        protected FighterController _fighterController;

        protected InputFeed _inputFeed;

        public bool haveControl { get { return _haveControl; } }

        private void Start()
        {
            _fighterController = GetComponent<FighterController>();

        }

        protected override void FurtherUpdate()
        {
            _inputFeed.Initialize();
            UpdateInputFeed();
            Feed();
        }

        #region Overrides

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

        #endregion

        #region InputFeed Handling

        void UpdateInputFeed()
        {
            if (!_fighterController) return;

            ControlDirection newDirec = ControlDirection.NEUTRAL;
            ControlDirectionManeuver newDirecManeu = ControlDirectionManeuver.ANY;

            if (Control.Y.Value > 0) newDirec = ControlDirection.Up;
            else if (Control.Y.Value < 0) newDirec = ControlDirection.Down;

            if (Control.X.Value > 0) // The control stick is pointing right
            {
                if (newDirec == ControlDirection.Up)
                    newDirec = (_fighterController.facingRight ? ControlDirection.ForwardUp : ControlDirection.BackUp);

                else if (newDirec == ControlDirection.Down)
                    newDirec = (_fighterController.facingRight ? ControlDirection.ForwardDown : ControlDirection.BackDown);

                else
                    newDirec = (_fighterController.facingRight ? ControlDirection.Forward : ControlDirection.Back);
            }
            else if (Control.X.Value < 0) // The control stick is pointing left
            {
                if (newDirec == ControlDirection.Up)
                    newDirec = (!_fighterController.facingRight ? ControlDirection.ForwardUp : ControlDirection.BackUp);

                else if (newDirec == ControlDirection.Down)
                    newDirec = (!_fighterController.facingRight ? ControlDirection.ForwardDown : ControlDirection.BackDown);

                else
                    newDirec = (!_fighterController.facingRight ? ControlDirection.Forward : ControlDirection.Back);
            }

            if (Control.Snap) newDirecManeu = ControlDirectionManeuver.Snap;
            else if (Control.Hard) newDirecManeu = ControlDirectionManeuver.Hard;
            else if (Control.Soft) newDirecManeu = ControlDirectionManeuver.Soft;

            _inputFeed.SetControl(newDirec, newDirecManeu);

            InputButton[] buttons = { Special, AttackHeavy, AttackLight, Jump, Meter, Defend };
            Button newBtn = Button.NULL;
            ButtonManeuver newBtnManeu = ButtonManeuver.ANY;

            for (int i = 0; i < buttons.Length; i++)
            {
                InputButton current = buttons[i];

                if (!current.input) continue;
                
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
                
                _inputFeed.Add(newBtn, newBtnManeu);
            }
        }

        void Feed()
        {
            for (int i = 0; i < _inputFeed.Count; i++)
            {
                InputCombo current = _inputFeed[i];
                
                if (_fighterController.ReceiveInput(current))
                    break;
            }

            _fighterController.ReceiveInput(_inputFeed.GetControl());
        }

        protected struct InputFeed
        {
            ControlDirection direction;
            ControlDirectionManeuver directionManeuver;

            List<Button> buttons;
            List<ButtonManeuver> buttonManeuvers;

            int _count;
            public int Count { get { return _count; } }
            
            public InputCombo this[int index]
            {
                get
                {
                    if (index >= _count) return default(InputCombo);
                    
                    return new InputCombo
                    {
                        direction = direction,
                        directionManeuver = directionManeuver,

                        button = buttons[index],
                        buttonManeuver = buttonManeuvers[index]
                    };
                }
            }

            public void Initialize()
            {
                direction = ControlDirection.NEUTRAL;
                directionManeuver = ControlDirectionManeuver.ANY;
                
                buttons = new List<Button>();
                buttonManeuvers = new List<ButtonManeuver>();
                _count = 0;
            }

            public void SetControl(ControlDirection direc, ControlDirectionManeuver direcManeu)
            {
                direction = direc;
                directionManeuver = direcManeu;
            }

            public InputCombo GetControl()
            {
                return new InputCombo
                {
                    direction = direction,
                    directionManeuver = directionManeuver,

                    button = Button.NULL,
                    buttonManeuver = ButtonManeuver.ANY
                };
            }

            public void Add(Button button, ButtonManeuver buttonManeu)
            {
                buttons.Add(button);
                buttonManeuvers.Add(buttonManeu);
                _count++;
            }
        }

        #endregion 
    }
    
}
