using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : InputComponent
{
    [SerializeField] int _playerNumber = 1;

    public InputAxis ControlHorizontal = new InputAxis(KeyCode.D, KeyCode.A, ControllerAxes.LeftStickHorizontal);
    public InputAxis ControlVertical = new InputAxis(KeyCode.W, KeyCode.S, ControllerAxes.LeftStickVertical);

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
        throw new System.NotImplementedException();
    }

    public override void ReleaseControl(bool resetVAlues = true)
    {
        throw new System.NotImplementedException();
    }

    protected override void GetInputs(bool fixedUpdateHappened)
    {
        throw new System.NotImplementedException();
    }

    void AssignPlayerNumberToInputs()
    {
        ControlHorizontal._playerNumber = _playerNumber;
        ControlVertical._playerNumber = _playerNumber;

        Jump._playerNumber = _playerNumber;
        AttackLight._playerNumber = _playerNumber;
    }
}
