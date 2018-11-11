using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;

namespace Assault
{
    public class SelectionInput : InputComponent
    {
        public InputGrid Control = new InputGrid(KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.Z, ControllerGrid.LeftStick);

        public InputButton Confirm = new InputButton(KeyCode.Space, ControllerButtons.FaceBottom);
        public InputButton Back = new InputButton(KeyCode.LeftShift, ControllerButtons.FaceRight);
        public InputButton AlternateLeft = new InputButton(KeyCode.X, ControllerButtons.LeftBumper);
        public InputButton AlternateRight = new InputButton(KeyCode.C, ControllerButtons.RightBumper);

        private void Start()
        {
            SetPlayerNumber(_playerNumber);
        }
        
        public override void GainControl()
        {
            GainControl(Control);
            GainControl(Confirm);
            GainControl(Back);
            GainControl(AlternateLeft);
            GainControl(AlternateRight);
        }

        public override void ReleaseControl(bool resetValues = true)
        {
            ReleaseControl(Control, resetValues);
            ReleaseControl(Confirm, resetValues);
            ReleaseControl(Back, resetValues);
            ReleaseControl(AlternateLeft, resetValues);
            ReleaseControl(AlternateRight, resetValues);
        }

        public override void SetPlayerNumber(int playerNumber)
        {
            Control.SetPlayerNumber(_playerNumber);
            Confirm.SetPlayerNumber(_playerNumber);
            Back.SetPlayerNumber(_playerNumber);
            AlternateLeft.SetPlayerNumber(_playerNumber);
            AlternateRight.SetPlayerNumber(_playerNumber);
        }

        protected override void GetInputs(bool fixedUpdateHappened)
        {
            Control.StateUpdate(_inputType);
            Confirm.StateUpdate(fixedUpdateHappened, _inputType);
            Back.StateUpdate(fixedUpdateHappened, _inputType);
            AlternateLeft.StateUpdate(fixedUpdateHappened, _inputType);
            AlternateRight.StateUpdate(fixedUpdateHappened, _inputType);
        }
    }
}
