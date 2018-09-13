using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Assault.Maneuvers.Maneuver))]
public class ManeuverEditor : Editor
{
    Assault.Maneuvers.Maneuver _targetManeuver;

    SerializedProperty _name;
    SerializedProperty _toSet;

    SerializedProperty _moveFrames;
    SerializedProperty _forceFrames;

    SerializedProperty _accelerateCurveX;
    SerializedProperty _accelerateCurveY;
}
