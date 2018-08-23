using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : InputComponent
{
    public enum FighterInput
    {
        None,
        Forward, Back, Up, Down, ForwardUp, ForwardDown, BackUp, BackDown,
        Jump, AttackLight, AttackHeavy, Special, Meter, Defend
    }

    public enum Direction
    { Any, Neutral, Forward, Back, Up, Down, ForwardUp, ForwardDown, BackUp, BackDown }

    public enum DirectionManeuver
    { Any, Soft, Hard, DoubleSnap }
    
    public enum Button
    { Any, Jump, AttackLight, AttackHeavy, Special, Meter, Defend }

    public enum ButtonManeuver
    { Any, Down, Up, DoubleTap }

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

    PlayerControllerInherit _controller;

    private void Start()
    {
        _controller = GetComponent<PlayerControllerInherit>();
    }

    protected override void FurtherUpdate()
    {
        InputCombo newInputCombo;

        Direction newDirec = Direction.Neutral;
        DirectionManeuver newDirecManeu = DirectionManeuver.Hard;
        Button newBtn = Button.Any;
        ButtonManeuver newBtnManeu = ButtonManeuver.Any;

        if (Control_Y.Value > 0) newDirec = Direction.Up;
        else if (Control_Y.Value < 0) newDirec = Direction.Down;

        if (Control_X.Value > 0)
        {
            if (newDirec == Direction.Up)
            {
                if (_controller.facingRight) newDirec = Direction.ForwardUp;
                else newDirec = Direction.BackUp;
            }
            else if (newDirec == Direction.Down)
            {
                if (_controller.facingRight) newDirec = Direction.ForwardDown;
                else newDirec = Direction.BackDown;
            }
        }

        if (Special.input && _controller._currentState != PlayerControllerInherit.CurrentState.Hitstun)
        {
            newBtn = Button.Special;
        }
        else if (AttackHeavy.input && _controller._currentState != PlayerControllerInherit.CurrentState.Hitstun)
        {
            newBtn = Button.AttackHeavy;
        }
        else if (AttackLight.input && _controller._currentState != PlayerControllerInherit.CurrentState.Hitstun)
        {
            newBtn = Button.AttackLight;
        }
        else if (Jump.input && _controller._currentState != PlayerControllerInherit.CurrentState.Hitstun)
        {
            newBtn = Button.Jump;
        }
        else if (Meter.input && _controller._currentState != PlayerControllerInherit.CurrentState.Hitstun)
        {
            newBtn = Button.Meter;
        }
        else if (Defend.input)
        {
            newBtn = Button.Defend;
        }

        newInputCombo.majorDirection = newDirec;
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

[Serializable]
public struct InputCombo
{
    public PlayerControllerInherit.CurrentState requiredState;

    public PlayerInput.Direction majorDirection;
    public AcceptedDirections acceptedDirection;
    public PlayerInput.DirectionManeuver directionManeuver;

    public PlayerInput.Button inputButton;
    public PlayerInput.ButtonManeuver buttonManeuver;

    /// <summary>
    /// Checks to see if the presented InputCombo matches this one
    /// </summary>
    /// <param name="input">The presented InputCombo</param>
    /// <param name="extendedDirection">Should we accept the input direction if it falls within the accpetedDirection array?</param>
    /// <returns></returns>
    public bool ValidInput(InputCombo input, bool extendedDirection = true)
    {
        if (requiredState != input.requiredState) return false;

        if (inputButton != input.inputButton) return false;
        if (buttonManeuver != input.buttonManeuver) return false;
        
        if (extendedDirection)
        {
            if (majorDirection != input.majorDirection || acceptedDirection != input.acceptedDirection) return false;
        }
        else
        {
            if (majorDirection != input.majorDirection) return false;
        }

        return true;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is InputCombo)) return false;

        InputCombo inputCombo = (InputCombo)obj;
        if (this != inputCombo) return false;

        return true;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(InputCombo first, InputCombo second)
    {
        if (first.requiredState != second.requiredState) return false;

        if (first.inputButton != second.inputButton) return false;

        if (first.acceptedDirection != null && second.acceptedDirection != null)
        {
            if (first.acceptedDirection == second.acceptedDirection) return true;
        }
        else if ((first.acceptedDirection == null && second.acceptedDirection != null) || (first.acceptedDirection != null && second.acceptedDirection == null))
            return false;

        if (first.majorDirection != second.majorDirection) return false;
        if (first.buttonManeuver != second.buttonManeuver) return false;

        return true;
    }

    public static bool operator !=(InputCombo inputCombo1, InputCombo inputCombo2)
    {
        if (inputCombo1.requiredState == inputCombo2.requiredState) return false;
        if (inputCombo1.inputButton == inputCombo2.inputButton) return false;
        if (inputCombo1.majorDirection == inputCombo2.majorDirection) return false;
        if (inputCombo1.buttonManeuver == inputCombo2.buttonManeuver) return false;

        return true;
    }

    public override string ToString()
    {
        string majorDirec, direcManeu, inpBtn, btnManeu;

        switch (majorDirection)
        {
            case PlayerInput.Direction.BackDown:
                majorDirec = "Back Down ";
                break;
            case PlayerInput.Direction.BackUp:
                majorDirec = "Back Up ";
                break;
            case PlayerInput.Direction.ForwardDown:
                majorDirec = "Forward Down ";
                break;
            case PlayerInput.Direction.ForwardUp:
                majorDirec = "Forward Up ";
                break;
            case PlayerInput.Direction.Any:
                majorDirec = "";
                break;
            default:
                majorDirec = majorDirection.ToString() + " ";
                break;
        }

        switch (directionManeuver)
        {
            case PlayerInput.DirectionManeuver.Any:
                direcManeu = "";
                break;
            case PlayerInput.DirectionManeuver.DoubleSnap:
                direcManeu = "Double Snap ";
                break;
            default:
                direcManeu = directionManeuver.ToString() + " ";
                break;
        }

        switch (inputButton)
        {
            case PlayerInput.Button.Any:
                inpBtn = "";
                break;
            case PlayerInput.Button.AttackHeavy:
                inpBtn = "Heavy ";
                break;
            case PlayerInput.Button.AttackLight:
                inpBtn = "Light ";
                break;
            default:
                inpBtn = inputButton.ToString() + " ";
                break;

        }

        switch (buttonManeuver)
        {
            case PlayerInput.ButtonManeuver.Any:
                btnManeu = "";
                break;
            case PlayerInput.ButtonManeuver.DoubleTap:
                btnManeu = "Double Tap ";
                break;
            default:
                btnManeu = buttonManeuver.ToString() + " ";
                break;
        }

        return direcManeu + majorDirec + btnManeu + inpBtn;
    }

    [Serializable]
    public struct AcceptedDirections
    {
        public PlayerInput.Direction[] directions;

        public override bool Equals(object obj)
        {
            AcceptedDirections second = (AcceptedDirections)obj;

            if (this == second) return true;

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(AcceptedDirections first, AcceptedDirections second)
        {
            if (first == second) return true;

            for (int i = 0; i < first.directions.Length; i++)
            {
                for (int j = 0; j < second.directions.Length; j++)
                {
                    if (first.directions[i] == second.directions[j]) return true;
                }
            }

            return false;
        }

        public static bool operator !=(AcceptedDirections first, AcceptedDirections second)
        {
            if (first != second) return true;

            for (int i = 0; i < first.directions.Length; i++)
            {
                for (int j = 0; j < second.directions.Length; j++)
                {
                    if (first.directions[i] != second.directions[j]) return true;
                }
            }

            return false;
        }

        public static bool operator ==(AcceptedDirections accepted, PlayerInput.Direction direction)
        {
            for (int i = 0; i < accepted.directions.Length; i++)
            {
                if (accepted.directions[i] == direction) return true;
            }

            return false;
        }

        public static bool operator !=(AcceptedDirections accepted, PlayerInput.Direction direction)
        {
            for (int i = 0; i < accepted.directions.Length; i++)
            {
                if (accepted.directions[i] != direction) return true;
            }

            return false;
        }
    }
}