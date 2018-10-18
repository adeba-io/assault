using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault.StateMachines
{
    public class TechniqueSMB : SceneLinkedSMB<FighterController>
    {
        public override void OnSLStatePostEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _monoBehavior.currentManeuver.Initialize(_monoBehavior);
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _monoBehavior.currentManeuver.Update();
        }

        public override void OnSLStatePreExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _monoBehavior.currentManeuver.End();
            _monoBehavior.currentManeuver = null;
        }

        public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            _monoBehavior.currentManeuver.End();
            _monoBehavior.currentManeuver = null;
        }
    }
}