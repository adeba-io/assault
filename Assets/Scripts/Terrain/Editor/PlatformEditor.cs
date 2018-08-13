using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Platform))]
public class PlatformEditor : Editor
{
    Platform platform;
    Collider2D collider;

    SerializedProperty _platformType;
    SerializedProperty _slopeAngle;
    SerializedProperty _physicsMaterial;

    readonly GUIContent gui_platformType = new GUIContent("Platform Type");
    readonly GUIContent gui_slopeAngle = new GUIContent("Slope Angle", "The rotation of the attached transform");
    readonly GUIContent gui_physicsMaterial = new GUIContent("Physics Material");

    private void OnEnable()
    {
        platform = (Platform)target;
        collider = platform.GetComponent<Collider2D>();

        _platformType = serializedObject.FindProperty("_platformType");
        _slopeAngle = serializedObject.FindProperty("_slopeAngle");
        _physicsMaterial = serializedObject.FindProperty("_physicsMaterial");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_platformType, gui_platformType);

        EditorGUILayout.Slider(_slopeAngle, -90f, 90f, gui_slopeAngle);
        platform.transform.rotation = Quaternion.Euler(0, 0, _slopeAngle.floatValue);

        EditorGUILayout.PropertyField(_physicsMaterial, gui_physicsMaterial);

        if (platform.physicsMaterial && !collider.sharedMaterial)
            collider.sharedMaterial = platform.physicsMaterial;
        else if (collider.sharedMaterial && !platform.physicsMaterial)
            platform.physicsMaterial = collider.sharedMaterial;

        serializedObject.ApplyModifiedProperties();
    }
}
