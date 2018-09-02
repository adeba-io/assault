using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : InputComponent
{
    #region Enums

    public enum FighterInput
    {
        None,
        Forward, Back, Up, Down, ForwardUp, ForwardDown, BackUp, BackDown,
        Jump, AttackLight, AttackHeavy, Special, Meter, Defend
    }

    public enum Direction
    { Any, Neutral, Forward, Back, Up, Down, ForwardUp, ForwardDown, BackUp, BackDown }

    public enum DirectionManeuver
    { Any, Soft, Hard, Snap }
    
    public enum Button
    { Any, Jump, AttackLight, AttackHeavy, Special, Meter, Defend }

    public enum ButtonManeuver
    { Any, Down, Held, Up }

    #endregion

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
        Direction newDirec = Direction.Neutral;
        DirectionManeuver newDirecManeu = DirectionManeuver.Any;

        if (Control.Y.Value > 0) newDirec = Direction.Up;
        else if (Control.Y.Value < 0) newDirec = Direction.Down;

        if (Control.X.Value > 0) // We're pointing right
        {
            if (newDirec == Direction.Up)
            {
                newDirec = (_playerController.facingRight ? Direction.ForwardUp : Direction.BackUp);
            }
            else if (newDirec == Direction.Down)
            {
                newDirec = (_playerController.facingRight ? Direction.ForwardDown : Direction.BackDown);
            }
            else
            {
                newDirec = (_playerController.facingRight ? Direction.Forward : Direction.Back);
            }
        }
        else if (Control.X.Value < 0) // Pointing left
        {
            if (newDirec == Direction.Up)
            {
                newDirec = ( ! _playerController.facingRight ? Direction.ForwardUp : Direction.BackUp);
            }
            else if (newDirec == Direction.Down)
            {
                newDirec = ( ! _playerController.facingRight ? Direction.ForwardDown : Direction.BackDown);
            }
            else
            {
                newDirec = ( ! _playerController.facingRight ? Direction.Forward : Direction.Back);
            }
        }

        if (Control.Snap) newDirecManeu = DirectionManeuver.Snap;
        else if (Control.Hard) newDirecManeu = DirectionManeuver.Hard;
        else if (Control.Soft) newDirecManeu = DirectionManeuver.Soft;

        InputButton[] buttonInputs = { Special, AttackHeavy, AttackLight, Jump, Meter, Defend };
        Button newBtn = Button.Any;
        ButtonManeuver newBtnManeu = ButtonManeuver.Any;
        
        for (int i = 0; i < buttonInputs.Length; i++)
        {
            InputButton current = buttonInputs[i];

            if (!current.input) continue;
            
            if (_playerController._currentState != PlayerController.CurrentState.Hitstun)
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
                else newBtnManeu = ButtonManeuver.Any;

                _inputFeed.Add(_playerController._currentState, newDirec, newDirecManeu, newBtn, newBtnManeu);
            }
            else if (current == Defend)
            {
                newBtn = Button.Defend;

                if (current.Down) newBtnManeu = ButtonManeuver.Down;
                else if (current.Held) newBtnManeu = ButtonManeuver.Held;
                else if (current.Up) newBtnManeu = ButtonManeuver.Up;
                else newBtnManeu = ButtonManeuver.Any;

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
            InputCombo currCombo = _inputFeed[i];

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
        PlayerController.CurrentState _currentState;

        Direction _direction;
        DirectionManeuver _directionManeuver;

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
                    requiredState = _currentState,
                    majorDirection = _direction,
                    directionManeuver = _directionManeuver,

                    inputButton = _inputButtons[index],
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

        public void Add(PlayerController.CurrentState state, Direction direc,
            DirectionManeuver direcManeu, Button button, ButtonManeuver buttonManeu)
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
            _currentState = inputCombo.requiredState;
            _direction = inputCombo.majorDirection;
            _directionManeuver = inputCombo.directionManeuver;

            _inputButtons.Add(inputCombo.inputButton);
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

[Serializable]
public struct InputCombo
{
    public PlayerController.CurrentState requiredState;
    
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
            case PlayerInput.DirectionManeuver.Snap:
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