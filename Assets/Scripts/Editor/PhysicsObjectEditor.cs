using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(PhysicsObject))]
public class PhysicsObjectEditor : Editor
{
    PhysicsObject physicsObject;

    SerializedProperty _collisionState;

    SerializedProperty _useGravity;
    SerializedProperty _gravityMultiplier;
    SerializedProperty _projectedVelocity;
    SerializedProperty _currentVelocity;

    SerializedProperty _skinWidth;
    SerializedProperty _collisionMask;
    SerializedProperty _triggerMask;

    SerializedProperty _slopeLimit;
    SerializedProperty _slopeSpeedModifier;

    SerializedProperty _totalHoriRays;
    SerializedProperty _totalVertRays;

    readonly GUIContent gui_collisionState = new GUIContent("Collision State");

    readonly GUIContent gui_useGravity = new GUIContent("Use Gravity");
    readonly GUIContent gui_gravityMultiplier = new GUIContent("Gravity Multiplier");
    readonly GUIContent gui_projectedVelocity = new GUIContent("Projected Velocity");
    readonly GUIContent gui_currentVelocity = new GUIContent("Current Velocity");

    readonly GUIContent gui_skinWidth = new GUIContent("Skin Width");
    readonly GUIContent gui_collisionMask = new GUIContent("Collision Mask");
    readonly GUIContent gui_triggerMask = new GUIContent("Trigger Mask");

    readonly GUIContent gui_slopeLimit = new GUIContent("Slope Limit");
    readonly GUIContent gui_slopeSpeedModifier = new GUIContent("Slope Speed Modifier");

    readonly GUIContent gui_totalHoriRays = new GUIContent("Total Horizontal Rays");
    readonly GUIContent gui_totalVertRays = new GUIContent("Total Vertical Rays");

    private void OnEnable()
    {
        _collisionState = serializedObject.FindProperty("_collisionState");

        _useGravity = serializedObject.FindProperty("_useGravity");
        _gravityMultiplier = serializedObject.FindProperty("_gravityMultiplier");
        _projectedVelocity = serializedObject.FindProperty("_projectedVelocity");
        _currentVelocity = serializedObject.FindProperty("_currentVelocity");

        _skinWidth = serializedObject.FindProperty("_slopeLimit");
        _slopeSpeedModifier = serializedObject.FindProperty("_slopeSpeedModifier");

        _totalHoriRays = serializedObject.FindProperty("_totalHoriRays");
        _totalVertRays = serializedObject.FindProperty("_totalVertRays");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        /*
        serializedObject.Update();

        physicsObject = (PhysicsObject)target;

        EditorGUILayout.PropertyField(_collisionState, gui_collisionState, true);

        EditorGUILayout.BeginHorizontal();

        bool usingGravity = EditorGUILayout.PropertyField(_useGravity, gui_useGravity);

        EditorGUILayout.PropertyField(_gravityMultiplier, gui_gravityMultiplier);

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Jump by 10f"))
        {
            physicsObject.collisions.ignoreGroundThisFrame = true;
            physicsObject.ForceRigidbody(0f, 10f, false, true);
        }

        serializedObject.ApplyModifiedProperties();*/  
    }
}
