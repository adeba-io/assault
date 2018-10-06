using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assault.Types;

namespace Assault
{
    public abstract class InputComponent : MonoBehaviour
    {
        
        #region Internal Classes

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
            public bool input { get { return Down || Held || Up; } }


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
            { (int)ControllerButtons.RightBumper, "RightBumper" },
            { (int)ControllerButtons.LeftTrigger, "Left Trigger" },
            { (int)ControllerButtons.RightTrigger, "Right Trigger" }
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

        [Serializable]
        public class InputGrid
        {
            public KeyCode positiveX, negativeX;
            public KeyCode positiveY, negativeY;
            public KeyCode softInputToggle;

            public ControllerGrid controllerGrid;

            ControllerAxes controllerAxisX, controllerAxisY;

            [SerializeField] protected bool _enabled = true;
            protected bool _gettingInput = true;

            protected Vector2[] _previousValue = new Vector2[3];

            public Vector2 Value { get; protected set; }

            public Axis X { get; protected set; }
            public Axis Y { get; protected set; }

            public Raw Raws { get; protected set; }

            public bool Soft
            { get { return Value.x != 0 || Value.y != 0; } }
            public bool Hard
            { get { return Mathf.Abs(Value.x) == 1f || Mathf.Abs(Value.y) == 1f; } }
            public bool Snap { get; protected set; }

            public bool Enabled { get { return _enabled; } }
            public bool ReceivingInput { get; protected set; }

            protected readonly static Dictionary<int, string> k_axisToName = new Dictionary<int, string>
            {
                { (int)ControllerAxes.LeftStick_X, "LeftStick X" },
                { (int)ControllerAxes.LeftStick_Y, "LeftStick Y" },
                { (int)ControllerAxes.RightStick_X, "RightStick X" },
                { (int)ControllerAxes.RightStick_Y, "RightStick Y" },
                { (int)ControllerAxes.Dpad_X, "Dpad X" },
                { (int)ControllerAxes.Dpad_Y, "Dpad Y" }
            };

            public InputGrid(KeyCode posiX, KeyCode negaX, KeyCode posiY, KeyCode negaY, KeyCode softInput, ControllerGrid contrGrid)
            {
                positiveX = posiX; negativeX = negaX;
                positiveY = posiY; negativeY = negaY;
                softInputToggle = softInput;
                controllerGrid = contrGrid;

                switch (controllerGrid)
                {
                    case ControllerGrid.LeftStick:
                        controllerAxisX = ControllerAxes.LeftStick_X;
                        controllerAxisY = ControllerAxes.LeftStick_Y;
                        break;
                    case ControllerGrid.RightStick:
                        controllerAxisX = ControllerAxes.RightStick_X;
                        controllerAxisY = ControllerAxes.RightStick_Y;
                        break;
                    case ControllerGrid.Dpad:
                        controllerAxisX = ControllerAxes.Dpad_X;
                        controllerAxisY = ControllerAxes.Dpad_Y;
                        break;
                    default:
                        controllerAxisX = ControllerAxes.NULL;
                        controllerAxisY = ControllerAxes.NULL;
                        break;
                }
            }

            public InputGrid(InputAxis axisX, InputAxis axisY, ControllerGrid contrGrid)
            {
                positiveX = axisX.positive; negativeX = axisX.negative;
                positiveY = axisY.positive; negativeY = axisY.negative;
                controllerGrid = contrGrid;

                switch (controllerGrid)
                {
                    case ControllerGrid.LeftStick:
                        controllerAxisX = ControllerAxes.LeftStick_X;
                        controllerAxisY = ControllerAxes.LeftStick_Y;
                        break;
                    case ControllerGrid.RightStick:
                        controllerAxisX = ControllerAxes.RightStick_X;
                        controllerAxisY = ControllerAxes.RightStick_Y;
                        break;
                    case ControllerGrid.Dpad:
                        controllerAxisX = ControllerAxes.Dpad_X;
                        controllerAxisY = ControllerAxes.Dpad_Y;
                        break;
                    default:
                        controllerAxisX = ControllerAxes.NULL;
                        controllerAxisY = ControllerAxes.NULL;
                        break;
                }
            }

            public void StateUpdate(InputType inputType)
            {
                Snap = false;

                if (!_enabled)
                {
                    Value = Vector2.zero;
                    _previousValue = new Vector2[3];
                    return;
                }

                _previousValue[0] = Value;
                _previousValue[1] = _previousValue[0];
                _previousValue[2] = _previousValue[1];

                if (!_gettingInput) return;

                Vector2 val = Vector2.zero;
                Vector2 rawVal = Vector2.zero;

                Axis x, y;
                Raw r = new Raw();
                if (inputType == InputType.Controller)
                {
                    float[] values = { Input.GetAxis(k_axisToName[(int)controllerAxisX]), Input.GetAxis(k_axisToName[(int)controllerAxisY]) };
                    rawVal = new Vector2(values[0], values[1]);

                    for (int i = 0; i < values.Length; i++)
                    {
                        if (values[i] >= 1f || values[i] <= -1f)
                        {
                            values[i] = Mathf.Sign(values[i]) * 1f;
                        }
                        else if (values[i] > 0.7f || values[i] < -0.7f)
                        {
                            values[i] = Mathf.Sign(values[i]) * 0.5f;
                        }
                        else
                        {
                            values[i] = 0;
                        }
                    }

                    val = new Vector2(values[0], values[1]);
                }
                else if (inputType == InputType.Keyboard)
                {
                    bool positiveHeldX = Input.GetKey(positiveX), negativeHeldX = Input.GetKey(negativeX);
                    bool positiveHeldY = Input.GetKey(positiveY), negativeHeldY = Input.GetKey(negativeY);

                    bool soft = Input.GetKey(softInputToggle);

                    if (positiveHeldX == negativeHeldX)
                        val.x = 0;
                    else if (positiveHeldX)
                        val.x = soft ? 0.5f : 1f;
                    else if (negativeHeldX)
                        val.x = soft ? -0.5f : -1f;

                    if (positiveHeldY == negativeHeldY)
                        val.y = 0;
                    else if (positiveHeldY)
                        val.y = soft ? 0.5f : 1f;
                    else if (negativeHeldY)
                        val.y = soft ? -0.5f : -1f;

                    
                    rawVal = val;
                }

                Value = val;
                r.Value = rawVal;

                x.Value = Value.x;
                y.Value = Value.y;

                x.Snap = false; y.Snap = false;

                ReceivingInput = Value.x != 0 || Value.y != 0;

                if (ReceivingInput && inputType == InputType.Controller)
                {
                    if (_previousValue[2] == Vector2.zero)
                    {
                        if (Mathf.Abs(Value.x) >= 1f)
                        {
                            x.Snap = true;
                        }
                            
                        if (Mathf.Abs(Value.y) >= 1f)
                        {
                            Snap = true;
                            y.Snap = true;

                        }
                    }
                }
                else if (ReceivingInput && inputType == InputType.Keyboard)
                {
                    if (_previousValue[2] == Vector2.zero)
                    {

                        if (Mathf.Abs(Value.x) >= 1f)
                        {
                            Snap = true;
                            x.Snap = true;
                        }

                        if (Mathf.Abs(Value.y) >= 1f)
                        {
                            Snap = true;
                            y.Snap = true;
                        }
                    }
                }

                X = x;
                Y = y;
                Raws = r;
            }

            public void Enable() { _enabled = true; }
            public void Disable() { _enabled = false; }

            public void GainControl() { _gettingInput = true; }
            public void ReleaseControl(bool resetValues)
            {
                _gettingInput = false;
                if (resetValues)
                {
                    Value = Vector2.zero;
                    ReceivingInput = false;
                }
                _previousValue = new Vector2[3];
            }

            public struct Axis
            {
                public float Value;
                public bool Snap;

                public bool Hard
                { get { return Mathf.Abs(Value) == 1f || Mathf.Abs(Value) == 1f; } }
                public bool Soft
                { get { return Value != 0 || Value != 0; } }
            }

            public struct Raw
            {
                public Vector2 _value;

                public Vector2 Value
                {
                    get { return _value; }
                    set
                    {
                        _value = value;
                        X = (float)Math.Round(_value.x, 2);
                        Y = (float)Math.Round(_value.y, 2);
                    }
                }

                public float X;
                public float Y;
            }
        }

        [Serializable]
        public class InputAxis
        {
            public float value;
            public KeyCode positive, negative;
            public ControllerAxes controllerAxis;
            public float deadzoneValue = Single.Epsilon;
            [HideInInspector]
            public int _playerNumber;

            [SerializeField] protected bool _enabled = true;
            protected bool _gettingInput = true;

            protected float _previousValue;

            public float Value { get; protected set; }

            public bool Soft { get { return Mathf.Abs(Value) != 0; } }
            public bool Hard { get { return Mathf.Abs(Value) == 1f; } }
            public bool Snap { get; protected set; }

            public bool Enabled { get { return _enabled; } }
            public bool ReceivingInput { get; protected set; }

            protected readonly static Dictionary<int, string> k_axisToName = new Dictionary<int, string>
        {
        { (int)ControllerAxes.LeftStick_X, "LeftStick X" },
        { (int)ControllerAxes.LeftStick_Y, "LeftStick Y" },
        { (int)ControllerAxes.RightStick_X, "RightStick X" },
        { (int)ControllerAxes.RightStick_Y, "RightStick Y" },
        { (int)ControllerAxes.Dpad_X, "Dpad X" },
        { (int)ControllerAxes.Dpad_Y, "Dpad Y" }
        };

            public InputAxis(KeyCode posi, KeyCode nega, ControllerAxes contrAxis)
            { positive = posi; negative = nega; contrAxis = controllerAxis; }

            public InputAxis(KeyCode posi, KeyCode nega, ControllerAxes contrAxis, float deadzone)
            { positive = posi; negative = nega; controllerAxis = contrAxis; deadzoneValue = deadzone; }

            public void StateUpdate(InputType inputType)
            {
                Snap = false;

                if (!_enabled)
                {
                    Value = 0;
                    _previousValue = Value;
                    return;
                }

                _previousValue = Value;

                if (!_gettingInput) return;

                bool positiveHeld = false, negativeHeld = false;

                float val = 0;
                if (inputType == InputType.Controller)
                {
                    val = Input.GetAxis(k_axisToName[(int)controllerAxis]);

                    if (val > 0.9f || val < -0.9f)
                        val = Mathf.Sign(val) * 1f;
                    else if (val > 0.3f || val < -0.3f)
                        val = Mathf.Sign(val) * 0.5f;
                    else
                        val = 0;
                }
                else if (inputType == InputType.Keyboard)
                {
                    positiveHeld = Input.GetKey(positive);
                    negativeHeld = Input.GetKey(negative);

                    // To prevent double inputs
                    if (positiveHeld == negativeHeld)
                        val = 0;
                    else if (positiveHeld)
                        val = 1f;
                    else
                        val = -1f;
                }
                Value = val;

                ReceivingInput = Value != 0;

                if (ReceivingInput && inputType == InputType.Controller)
                {
                    if (_previousValue == 0)
                    {
                        if (Value == Mathf.Abs(Value))
                            Snap = true;
                    }
                }

                value = Value;
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
                _previousValue = Value;
            }
        }

        #endregion

        public InputType _inputType = InputType.Keyboard;

        bool _fixedUpdateHappened;

        private void Update()
        {
            GetInputs(_fixedUpdateHappened || Mathf.Approximately(Time.timeScale, 0));

            _fixedUpdateHappened = false;

            FurtherUpdate();
        }

        private void FixedUpdate()
        {
            _fixedUpdateHappened = true;
        }

        protected abstract void FurtherUpdate();

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

        protected void GainControl(InputGrid inputGrid)
        {
            inputGrid.GainControl();
        }

        protected void ReleaseControl(InputButton inputButton, bool resetValues)
        {
            StartCoroutine(inputButton.ReleaseControl(resetValues));
        }

        protected void ReleaseControl(InputAxis inputAxis, bool resetValues)
        {
            inputAxis.ReleaseControl(resetValues);
        }

        protected void ReleaseControl(InputGrid inputGrid, bool resetValues)
        {
            inputGrid.ReleaseControl(resetValues);
        }
    }
}
