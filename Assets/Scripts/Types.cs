using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault
{
    namespace Types
    {
        #region Input Enums

        public enum InputType { NULL, Keyboard, Controller }

        public enum ControllerButtons
        {
            NULL,
            FaceTop, FaceBottom,
            FaceLeft, FaceRight,
            Start, Select,
            LeftStick, RightStick,
            LeftBumper, RightBumper,
            LeftTrigger, RightTrigger
        }

        public enum ControllerAxes
        {
            NULL,
            LeftStick_X, LeftStick_Y,
            RightStick_X, RightStick_Y,
            Dpad_X, Dpad_Y
        }

        public enum ControllerGrid
        {
            NULL,
            LeftStick, RightStick, Dpad
        }

        public enum ControlDirection
        { NEUTRAL, Any, Forward, Back, Up, Down, ForwardUp, ForwardDown, BackUp, BackDown }

        public enum ControlDirectionManeuver
        { ANY, Soft, Hard, Snap }

        public enum Button
        { NULL, Any, Jump, AttackLight, AttackHeavy, Special, Meter, Defend }
        
        public enum ButtonManeuver
        { ANY, Down, Held, Up }

        #endregion

        public enum FighterState
        { NULL, Standing, Crouching, Dashing, Running, JumpSquat, Aerial, Fallen, MidTech, MidTechnique, Hitstun }

        public enum HitstunType
        { STANDARD, Chain, Burst, NoDI }
    }
}
