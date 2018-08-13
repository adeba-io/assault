using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : InputComponent
{
    [SerializeField] int _playerNumber = 1;

    public InputAxis ControlHorizontal = new InputAxis(KeyCode.LeftArrow, KeyCode.RightArrow, ControllerAxes.LeftStick_X);
    public InputAxis ControlVertical = new InputAxis(KeyCode.UpArrow, KeyCode.DownArrow, ControllerAxes.LeftStick_Y);

    public InputButton Jump = new InputButton(KeyCode.Space, ControllerButtons.FaceBottom);
    public InputButton AttackLight = new InputButton(KeyCode.J, ControllerButtons.FaceLeft);
    public InputButton AttackHeavy = new InputButton(KeyCode.K, ControllerButtons.FaceTop);
    public InputButton Special = new InputButton(KeyCode.L, ControllerButtons.FaceRight);
    public InputButton Meter = new InputButton(KeyCode.B, ControllerButtons.RightBumper);
    public InputButton Defend = new InputButton(KeyCode.N, ControllerButtons.LeftBumper);

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

        GainControl(ControlHorizontal);
        GainControl(ControlVertical);

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

        ReleaseControl(ControlHorizontal, resetValues);
        ReleaseControl(ControlVertical, resetValues);

        ReleaseControl(Jump, resetValues);
        ReleaseControl(AttackLight, resetValues);
        ReleaseControl(AttackHeavy, resetValues);
        ReleaseControl(Special, resetValues);
        ReleaseControl(Meter, resetValues);
        ReleaseControl(Defend, resetValues);
    }

    protected override void GetInputs(bool fixedUpdateHappened)
    {
        ControlHorizontal.StateUpdate(_inputType);
        ControlVertical.StateUpdate(_inputType);

        Jump.StateUpdate(fixedUpdateHappened, _inputType);
        AttackLight.StateUpdate(fixedUpdateHappened, _inputType);
        AttackHeavy.StateUpdate(fixedUpdateHappened, _inputType);
        Special.StateUpdate(fixedUpdateHappened, _inputType);
        Meter.StateUpdate(fixedUpdateHappened, _inputType);
        Defend.StateUpdate(fixedUpdateHappened, _inputType);
    }

    void AssignPlayerNumberToInputs()
    {
        ControlHorizontal._playerNumber = _playerNumber;
        ControlVertical._playerNumber = _playerNumber;

        Jump._playerNumber = _playerNumber;
        AttackLight._playerNumber = _playerNumber;
    }
}
