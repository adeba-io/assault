using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Assault.Maneuvers;

[CustomEditor(typeof(Maneuver))]
public class ManeuverEditor : Editor
{
    Maneuver _targetManeuver;

    SerializedProperty _name;
    SerializedProperty _toSet;

    SerializedProperty _accelerateCurveX;
    SerializedProperty _accelerateCurveY;

    SerializedProperty _totalFrameCount;

    SerializedProperty _cancelRegion;

    ReorderableList _moveFrames;
    ReorderableList _forceFrames;


    GUIContent gui_name = new GUIContent("Name");
    GUIContent gui_toSet = new GUIContent("State to Set to");

    GUIContent gui_accelerateCurveX = new GUIContent("Accelerate Curve X");
    GUIContent gui_accelerateCurveY = new GUIContent("Accelerate Curve Y");

    GUIContent gui_totalFrameCount = new GUIContent("Total Frame Count");

    GUIContent gui_cancelRegion = new GUIContent("Cancel Region");

    GUIContent gui_moveFrames = new GUIContent("Move Frames");
    GUIContent gui_forceFrames = new GUIContent("Force Frames");

    #region OnEnable

    public void OnEnable()
    {
        _name = serializedObject.FindProperty("_name");
        _toSet = serializedObject.FindProperty("_toSet");

        _accelerateCurveX = serializedObject.FindProperty("_accelerateCurveX");
        _accelerateCurveY = serializedObject.FindProperty("_accelerateCurveY");

        _totalFrameCount = serializedObject.FindProperty("_totalFrameCount");

        _cancelRegion = serializedObject.FindProperty("_cancelRegion");

        MoveFrames();
        ForceFrames();
    }

    void MoveFrames()
    {
        _moveFrames = new ReorderableList(serializedObject, serializedObject.FindProperty("_moveFrames"), false, true, true, true)
        {
            elementHeight = 50,

            drawHeaderCallback = (Rect rect) =>
            {
                rect.x += 20f;
                EditorGUI.LabelField(rect, gui_moveFrames, new GUIStyle { fontStyle = FontStyle.Bold });
            },

           drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
           {
               var element = _moveFrames.serializedProperty.GetArrayElementAtIndex(index);

               EditorGUI.PropertyField(rect, element);
               
               for (int i = 1; i < _moveFrames.serializedProperty.arraySize; i++)
               {
                   for (int j = 0; j < i; j++)
                   {
                       if (_moveFrames.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("frame").intValue > _moveFrames.serializedProperty.GetArrayElementAtIndex(j + 1).FindPropertyRelative("frame").intValue)
                       {
                           int temp = _moveFrames.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("frame").intValue;
                           _moveFrames.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("frame").intValue = _moveFrames.serializedProperty.GetArrayElementAtIndex(j + 1).FindPropertyRelative("frame").intValue;
                           _moveFrames.serializedProperty.GetArrayElementAtIndex(j + 1).FindPropertyRelative("frame").intValue = temp;

                           if (j == index) _moveFrames.index++;
                           else if (j + 1 == index) _moveFrames.index--;
                       }
                   }
               }
           }
           
        };
    }

    void ForceFrames()
    {
        _forceFrames = new ReorderableList(serializedObject, serializedObject.FindProperty("_forceFrames"), false, true, true, true)
        {
            elementHeight = 50,

            drawHeaderCallback = (Rect rect) =>
            {
                rect.x += 20f;
                EditorGUI.LabelField(rect, gui_forceFrames, new GUIStyle { fontStyle = FontStyle.Bold });
            },

            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _forceFrames.serializedProperty.GetArrayElementAtIndex(index);

                EditorGUI.PropertyField(rect, element);

                for (int i = 1; i < _forceFrames.serializedProperty.arraySize; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (_forceFrames.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("frame").intValue > _forceFrames.serializedProperty.GetArrayElementAtIndex(j + 1).FindPropertyRelative("frame").intValue)
                        {
                            int temp = _forceFrames.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("frame").intValue;
                            _forceFrames.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("frame").intValue = _forceFrames.serializedProperty.GetArrayElementAtIndex(j + 1).FindPropertyRelative("frame").intValue;
                            _forceFrames.serializedProperty.GetArrayElementAtIndex(j + 1).FindPropertyRelative("frame").intValue = temp;

                            if (j == index) _forceFrames.index++;
                            else if (j + 1 == index) _forceFrames.index--;
                        }
                    }
                }
            }

        };
    }

    #endregion

    public override void OnInspectorGUI()
    {
       // base.OnInspectorGUI();
        serializedObject.Update();

        EditorGUILayout.PropertyField(_name, gui_name);
        EditorGUILayout.PropertyField(_toSet, gui_toSet);

        EditorGUILayout.PropertyField(_accelerateCurveX, gui_accelerateCurveX);
        EditorGUILayout.PropertyField(_accelerateCurveY, gui_accelerateCurveY);

        EditorGUILayout.PropertyField(_totalFrameCount, gui_totalFrameCount);

        EditorGUILayout.PropertyField(_cancelRegion, gui_cancelRegion);


        _moveFrames.DoLayoutList();
        _forceFrames.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
}
