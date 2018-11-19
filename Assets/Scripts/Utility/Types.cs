using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault
{
    namespace Types
    {
        [System.Flags]
        public enum FighterState
        {
            NULL = 0x0,

            Standing = 0x1,
            Crouching = 0x2,
            Walking = 0x4,
            Dashing = 0x8,
            Running = 0x10,

            Skidding = 0x20,
            
            JumpSquat = 0x40,
            AirJumpSquat = 0x80,
            Jumping = 0x100,

            Aerial = 0x200,
            AirDash = 0x400,
            Tumble = 0x800,

            Fallen = 0x1000,

            MidTech = 0x2000,
            MidTechnique = 0x4000,
            Hitstun = 0x8000,
            Landing = 0x10000
        }

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

        public enum HorizontalControl
        { NEUTRAL, Any, Forward, Back }

        public enum HorizontalControlGeneral
        { NEUTRAL, Left, Right }

        public enum VerticalControl
        { NEUTRAL, Any, Up, Down }

        public enum ControlManeuver
        { ANY, Soft, Hard, Snap, DoubleSnap }

        public enum Button
        { NULL, Any, Jump, AttackLight, AttackHeavy, Special, Meter, Defend }

        public enum ButtonManeuver
        { ANY, Down, Held, Up }


        #endregion

        public enum HitstunType
        { STANDARD, Chain, Burst, NoDI }

        public enum BoxDrawType
        { STATIC, Continuous }

        public enum KnockbackType
        { Growing, Set }

        public enum ArmourType
        { NONE, Super, Heavy }
    }
}
