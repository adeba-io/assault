using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputComponent : MonoBehaviour
{
    #region Enums

    public enum InputType
    { Keyboard, Controller }

    public enum ControllerButtons
    {
        None,
        FaceTop, FaceBottom,
        FaceLeft, FaceRight,
        Start, Select,
        LeftStick, RightStick,
        LeftBumper, RightBumper
    }

    public enum ControllerAxes
    {
        None,
        LeftStick_X, LeftStick_Y,
        RightStick_X, RightStick_Y,
        Dpad_X, Dpad_Y,
        LeftTrigger, RightTrigger
    }

    #endregion

    #region Internal Classes

    #region Button Control

    [Serializable]
    public class InputButton
    {
        public KeyCode key;
        public ControllerButtons controllerButton;
        [HideInInspector]
        public int _playerNumber;

        [SerializeField] protected bool _enabled = true;
        protected bool _gettingInput = true;

        // Used to change the state of the button (Up / Down) only if at least one FixedUpdate has occured
        // between the previous frame and this one.
        // Since movement is made in FixedUpdate, this prevents inputs being lost between FixedUpdates
        bool _afterFixedUpdateDown, _afterFixedUpdateHeld, _afterFixedUpdateUp;

        public bool Down { get; protected set; }
        public bool Held { get; protected set; }
        public bool Up { get; protected set; }
        public bool Enabled { get { return _enabled; } }

        protected static readonly Dictionary<int, string> k_buttonsToName = new Dictionary<int, string>
    {
        { (int)ControllerButtons.FaceTop, "FaceTop" },
        { (int)ControllerButtons.FaceBottom, "FaceBottom" },
        { (int)ControllerButtons.FaceLeft, "FaceLeft" },
        { (int)ControllerButtons.FaceRight, "FaceRight" },
        { (int)ControllerButtons.Start, "Start" },
        { (int)ControllerButtons.Select, "Select" },
        { (int)ControllerButtons.LeftStick, "LeftStick" },
        { (int)ControllerButtons.RightStick, "RightStick" },
        { (int)ControllerButtons.LeftBumper, "LeftBumper" },
        { (int)ControllerButtons.RightBumper, "RightBumper" }
    };

        public InputButton(KeyCode key, ControllerButtons controlBtn)
        { this.key = key; controllerButton = controlBtn; }

        public void StateUpdate(bool fixedUpdateHappened, InputType inputType)
        {
            if (!_enabled)
            {
                Down = false;
                Held = false;
                Up = false;
                return;
            }

            if (!_gettingInput) return;

            if (inputType == InputType.Controller)
            {
                Down = Input.GetButtonDown(k_buttonsToName[(int)controllerButton]);
                Held = Input.GetButton(k_buttonsToName[(int)controllerButton]);
                Up = Input.GetButtonUp(k_buttonsToName[(int)controllerButton]);
            }
            else if (inputType == InputType.Keyboard)
            {
                Down = Input.GetKeyDown(key);
                Held = Input.GetKey(key);
                Up = Input.GetKeyUp(key);
            }

            if (fixedUpdateHappened)
            {
                _afterFixedUpdateDown = Down;
                _afterFixedUpdateHeld = Held;
                _afterFixedUpdateUp = Up;
            }
            else
            {
                _afterFixedUpdateDown |= Down;
                _afterFixedUpdateHeld |= Held;
                _afterFixedUpdateUp |= Up;
            }
        }

        public void Enable() { _enabled = true; }
        public void Disable() { _enabled = false; }

        public void GainControl() { _gettingInput = true; }
        public IEnumerator ReleaseControl(bool resetValues)
        {
            _gettingInput = false;

            if (!resetValues) yield break;

            if (Down) Up = true;
            Down = false;
            Held = false;

            _afterFixedUpdateDown = false;
            _afterFixedUpdateHeld = false;
            _afterFixedUpdateUp = false;

            yield return null;

            Up = false;
        }
    }
    
    #endregion

    #region Axes Control

    [Serializable]
    public class InputAxis
    {
        public KeyCode positive, negative;
        public ControllerAxes controllerAxis;
        [HideInInspector]
        public int _playerNumber;

        [SerializeField] protected bool _enabled = true;
        protected bool _gettingInput = true;

        public float Value { get; protected set; }
        public bool Enabled { get { return _enabled; } }
        public bool ReceivingInput { get; protected set; }

        protected readonly static Dictionary<int, string> k_axisToName = new Dictionary<int, string>
    {
        { (int)ControllerAxes.LeftStick_X, "LeftStick X" },
        { (int)ControllerAxes.LeftStick_Y, "LeftStick Y" },
        { (int)ControllerAxes.RightStick_X, "RightStick X" },
        { (int)ControllerAxes.RightStick_Y, "RightStick Y" },
        { (int)ControllerAxes.Dpad_X, "Dpad X" },
        { (int)ControllerAxes.Dpad_Y, "Dpad Y" },
        { (int)ControllerAxes.LeftTrigger, "Left Trigger" },
        { (int)ControllerAxes.RightTrigger, "Right Trigger" }
    };

        public InputAxis(KeyCode posi, KeyCode nega, ControllerAxes contrAxis)
        { positive = posi; negative = nega; contrAxis = controllerAxis; }

        public void StateUpdate(InputType inputType)
        {
            if (!_enabled)
            {
                Value = 0;
                return;
            }

            if (!_gettingInput) return;

            bool positiveHeld = false, negativeHeld = false;

            if (inputType == InputType.Controller)
            {
                float value = Input.GetAxisRaw(k_axisToName[(int)controllerAxis]);
                positiveHeld = value > Single.Epsilon;
                negativeHeld = value < -Single.Epsilon;
            }
            else if (inputType == InputType.Keyboard)
            {
                positiveHeld = Input.GetKey(positive);
                negativeHeld = Input.GetKey(negative);

                // To prevent double inputs
                if (positiveHeld == negativeHeld)
                    Value = 0;
                else if (positiveHeld)
                    Value = 1f;
                else
                    Value = -1f;
            }

            ReceivingInput = positiveHeld || negativeHeld;
        }

        public void Enable() { _enabled = true; }
        public void Disable() { _enabled = false; }

        public void GainControl() { _gettingInput = true; }
        public void ReleaseControl(bool resetValues)
        {
            _gettingInput = false;
            if (resetValues)
            {
                Value = 0f;
                ReceivingInput = false;
            }
        }
    }

    #endregion

    #endregion

    public InputType _inputType = InputType.Keyboard;

    bool _fixedUpdateHappened;

    private void Update()
    {
        GetInputs(_fixedUpdateHappened || Mathf.Approximately(Time.timeScale, 0));

        _fixedUpdateHappened = false;
    }

    private void FixedUpdate()
    {
        _fixedUpdateHappened = true;
    }

    protected abstract void GetInputs(bool fixedUpdateHappened);

    public abstract void GainControl();

    public abstract void ReleaseControl(bool resetValues = true);

    protected void GainControl(InputButton inputButton)
    {
        inputButton.GainControl();
    }

    protected void GainControl(InputAxis inputAxis)
    {
        inputAxis.GainControl();
    }

    protected void ReleaseControl(InputButton inputButton, bool resetValues)
    {
        StartCoroutine(inputButton.ReleaseControl(resetValues));
    }

    protected void ReleaseControl(InputAxis inputAxis, bool resetValues)
    {
        inputAxis.ReleaseControl(resetValues);
    }
}
