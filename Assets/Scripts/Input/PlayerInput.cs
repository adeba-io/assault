using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : InputComponent
{
    [SerializeField] int _playerNumber = 1;

    public InputAxis Control_X = new InputAxis(KeyCode.RightArrow, KeyCode.LeftArrow, ControllerAxes.LeftStick_X);
    public InputAxis Control_Y = new InputAxis(KeyCode.UpArrow, KeyCode.DownArrow, ControllerAxes.LeftStick_Y);

    public InputButton Jump = new InputButton(KeyCode.Space, ControllerButtons.FaceBottom);
    public InputButton AttackLight = new InputButton(KeyCode.F, ControllerButtons.FaceLeft);
    public InputButton AttackHeavy = new InputButton(KeyCode.E, ControllerButtons.FaceTop);
    public InputButton Special = new InputButton(KeyCode.W, ControllerButtons.FaceRight);
    public InputButton Meter = new InputButton(KeyCode.A, ControllerButtons.RightBumper);
    public InputButton Defend = new InputButton(KeyCode.V, ControllerButtons.LeftBumper);

    protected bool _haveControl = true;

    public bool haveControl { get { return _haveControl; } }
    public int playerNumber
    {
        get { return _playerNumber; }
        set
        {
            _playerNumber = value;
        }
    }

    public override void GainControl()
    {
        _haveControl = true;

        GainControl(Control_X);
        GainControl(Control_Y);

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

        ReleaseControl(Control_X, resetValues);
        ReleaseControl(Control_Y, resetValues);

        ReleaseControl(Jump, resetValues);
        ReleaseControl(AttackLight, resetValues);
        ReleaseControl(AttackHeavy, resetValues);
        ReleaseControl(Special, resetValues);
        ReleaseControl(Meter, resetValues);
        ReleaseControl(Defend, resetValues);
    }

    protected override void GetInputs(bool fixedUpdateHappened)
    {
        Control_X.StateUpdate(_inputType);
        Control_Y.StateUpdate(_inputType);

        Jump.StateUpdate(fixedUpdateHappened, _inputType);
        AttackLight.StateUpdate(fixedUpdateHappened, _inputType);
        AttackHeavy.StateUpdate(fixedUpdateHappened, _inputType);
        Special.StateUpdate(fixedUpdateHappened, _inputType);
        Meter.StateUpdate(fixedUpdateHappened, _inputType);
        Defend.StateUpdate(fixedUpdateHappened, _inputType);
    }

    void AssignPlayerNumberToInputs()
    {
        Control_X._playerNumber = _playerNumber;
        Control_Y._playerNumber = _playerNumber;

        Jump._playerNumber = _playerNumber;
        AttackLight._playerNumber = _playerNumber;
    }
}
