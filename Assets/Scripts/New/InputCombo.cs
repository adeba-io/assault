using UnityEngine;
using System.Collections;
using Assault.Types;

namespace Assault
{
    [System.Serializable]
    public struct InputCombo //: UnityEngine.Object
    {
        public ControlDirection direction;
        [SerializeField]
        public ControlDirectionManeuver directionManeuver;
        [SerializeField]
        public Button button;
        [SerializeField]
        public ButtonManeuver buttonManeuver;

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

            if (direction != input.direction) return false;

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

        public override string ToString()
        {
            string majorDirec, direcManeu, inpBtn, btnManeu;

            switch (direction)
            {
                case ControlDirection.BackDown:
                    majorDirec = "Back Down ";
                    break;
                case ControlDirection.BackUp:
                    majorDirec = "Back Up ";
                    break;
                case ControlDirection.ForwardDown:
                    majorDirec = "Forward Down ";
                    break;
                case ControlDirection.ForwardUp:
                    majorDirec = "Forward Up ";
                    break;
                case ControlDirection.NEUTRAL:
                case ControlDirection.Any:
                    majorDirec = "";
                    break;
                default:
                    majorDirec = direction.ToString() + " ";
                    break;
            }

            switch (directionManeuver)
            {
                case ControlDirectionManeuver.ANY:
                    direcManeu = "";
                    break;
                case ControlDirectionManeuver.Snap:
                    direcManeu = "Double Snap ";
                    break;
                default:
                    direcManeu = directionManeuver.ToString() + " ";
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
                default:
                    btnManeu = buttonManeuver.ToString() + " ";
                    break;
            }

            string toReturn = direcManeu + majorDirec + btnManeu + inpBtn;

            return (majorDirec == "" && inpBtn == "") ? "No Combo" : toReturn;
        }

        public static bool operator ==(InputCombo first, InputCombo second)
        {
            if (first.button != second.button) return false;

            if (first.direction != second.direction) return false;

            if (first.buttonManeuver == ButtonManeuver.ANY || second.buttonManeuver == ButtonManeuver.ANY)
                return true;

            if (first.buttonManeuver != second.buttonManeuver) return false;

            return true;
        }

        public static bool operator !=(InputCombo first, InputCombo second)
        {
            if (first.button == second.button) return false;
            if (first.direction == second.direction) return false;
            
            if (first.buttonManeuver != ButtonManeuver.ANY || second.buttonManeuver != ButtonManeuver.ANY)
                if (first.buttonManeuver == second.buttonManeuver) return false;

            return true;
        }
    }

}
