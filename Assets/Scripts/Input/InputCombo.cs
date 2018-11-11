using UnityEngine;
using System.Collections;
using Assault.Types;

namespace Assault
{
    [System.Serializable]
    public struct InputCombo //: UnityEngine.Object
    {
        public HorizontalControl horizontalControl;
        public VerticalControl verticalControl;
        public ControlManeuver controlManeuver;

        public HorizontalControlGeneral horizontalControlGeneral;

        public Button button;
        public ButtonManeuver buttonManeuver;

        public static InputCombo none
        {
            get
            {
                return new InputCombo { horizontalControl = HorizontalControl.NEUTRAL, verticalControl = VerticalControl.NEUTRAL, horizontalControlGeneral = HorizontalControlGeneral.NEUTRAL, button = Button.NULL };
            }
        }

        /// <summary>
        /// Checks to see if the presented InputCombo matches this one
        /// </summary>
        /// <param name="input">The presented InputCombo</param>
        /// <param name="extendedDirection">Should we accept the input direction if it falls within the accpetedDirection array?</param>
        /// <returns></returns>
        public bool ValidInput(InputCombo input, bool extendedDirection = true)
        {
            if (button != input.button) return false;
            if (buttonManeuver != input.buttonManeuver) return false;

            if (horizontalControl != input.horizontalControl) return false;
            if (verticalControl != input.verticalControl) return false;

            return true;
        }

        #region Overrides

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

        public override string ToString()
        {
            string majorDirec, direcManeu, inpBtn, btnManeu;
            
            switch (horizontalControl)
            {
                case HorizontalControl.NEUTRAL:
                case HorizontalControl.Any:
                    majorDirec = "";
                    break;
                default:
                    majorDirec = horizontalControl.ToString() + " ";
                    break;
            }

            switch (verticalControl)
            {
                case VerticalControl.NEUTRAL:
                case VerticalControl.Any:
                    break;
                default:
                    majorDirec += verticalControl.ToString() + " ";
                    break;
            }

            switch (controlManeuver)
            {
                case ControlManeuver.ANY:
                    direcManeu = "";
                    break;
                case ControlManeuver.DoubleSnap:
                    direcManeu = "Double Snap ";
                    break;
                default:
                    direcManeu = controlManeuver.ToString() + " ";
                    break;
            }

            switch (button)
            {
                case Button.NULL:
                case Button.Any:
                    inpBtn = "";
                    break;
                case Button.AttackHeavy:
                    inpBtn = "Heavy ";
                    break;
                case Button.AttackLight:
                    inpBtn = "Light ";
                    break;
                default:
                    inpBtn = button.ToString() + " ";
                    break;

            }

            switch (buttonManeuver)
            {
                case ButtonManeuver.ANY:
                    btnManeu = "";
                    break;
                case ButtonManeuver.Up:
                    btnManeu = "Released ";
                    break;
                default:
                    btnManeu = buttonManeuver.ToString() + " ";
                    break;
            }

            string toReturn = direcManeu + majorDirec + btnManeu + inpBtn;

            return (majorDirec == "" && inpBtn == "") ? "No Combo" : toReturn;
        }

        #endregion

        #region Operators

        public static bool operator ==(InputCombo first, InputCombo second)
        {
            if (first != second.button) return false;
            if (first != second.horizontalControl) return false;
            if (first != second.verticalControl) return false;
            if (first != second.buttonManeuver) return false;

            return true;
        }
        public static bool operator !=(InputCombo first, InputCombo second)
        {
            return !(first == second);
        }


        public static bool operator ==(InputCombo combo, HorizontalControl direction)
        {
            if (combo.horizontalControl == HorizontalControl.Any || direction == HorizontalControl.Any) return true;
            return combo.horizontalControl == direction;
        }
        public static bool operator !=(InputCombo combo, HorizontalControl direction)
        {
            return !(combo == direction);
        }

        public static bool operator ==(InputCombo combo, VerticalControl direction)
        {
            if (combo.verticalControl == VerticalControl.Any || direction == VerticalControl.Any) return true;
            return combo.verticalControl == direction;
        }
        public static bool operator !=(InputCombo combo, VerticalControl direction)
        {
            return !(combo == direction);
        }

        public static bool operator ==(InputCombo combo, ControlManeuver directionManeuver)
        {
            if (combo.controlManeuver == ControlManeuver.ANY || directionManeuver == ControlManeuver.ANY) return true;
            return combo.controlManeuver == directionManeuver;
        }
        public static bool operator !=(InputCombo combo, ControlManeuver directionManeuver)
        {
            return !(combo == directionManeuver);
        }

        public static bool operator ==(InputCombo combo, HorizontalControlGeneral direction)
        {
            return combo.horizontalControlGeneral == direction;
        }
        public static bool operator !=(InputCombo combo, HorizontalControlGeneral direction)
        {
            return combo.horizontalControlGeneral != direction;
        }
        
        public static bool operator ==(InputCombo combo, Button button)
        {
            if (combo.button == Button.Any || button == Button.Any) return true;
            return combo.button == button;
        }
        public static bool operator !=(InputCombo combo, Button button)
        {
            return combo.button != button;
        }

        public static bool operator ==(InputCombo combo, ButtonManeuver maneuver)
        {
            if (combo.buttonManeuver == ButtonManeuver.ANY || maneuver == ButtonManeuver.ANY) return true;
            return combo.buttonManeuver == maneuver;
        }
        public static bool operator !=(InputCombo combo, ButtonManeuver maneuver)
        {
            return !(combo == maneuver);
        }

        public static implicit operator bool(InputCombo inputCombo)
        {
            return inputCombo != HorizontalControl.NEUTRAL || inputCombo != VerticalControl.NEUTRAL || inputCombo != Button.NULL;
        }

        #endregion
    }
}
