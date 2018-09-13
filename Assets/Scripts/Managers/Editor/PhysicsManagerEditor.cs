using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Assault
{
    [CustomEditor(typeof(PhysicsManager))]
    public class PhysicsManagerEditor : Editor
    {
        SerializedProperty _gravity;
        SerializedProperty _externalFriction;

        readonly GUIContent gui_gravity = new GUIContent("Gravity");
        readonly GUIContent gui_externalFriction = new GUIContent("External Friction");

        private void OnEnable()
        {
            _gravity = serializedObject.FindProperty("_gravity");
            _externalFriction = serializedObject.FindProperty("_externalFriction");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(_gravity, gui_gravity);
            EditorGUILayout.PropertyField(_externalFriction, gui_externalFriction);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
