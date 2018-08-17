using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhysicsManager))]
public class PhysicsManagerEditor : Editor
{
    SerializedProperty _gravity;
    SerializedProperty _airFriction;

    readonly GUIContent gui_gravity = new GUIContent("Gravity");
    readonly GUIContent gui_airFriction = new GUIContent("Air Friction");

    private void OnEnable()
    {
        _gravity = serializedObject.FindProperty("_gravity");
        _airFriction = serializedObject.FindProperty("_airFriction");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(_gravity, gui_gravity);
        EditorGUILayout.PropertyField(_airFriction, gui_airFriction);

        serializedObject.ApplyModifiedProperties();
    }
}
