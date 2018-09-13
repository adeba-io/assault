using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Assault
{
    [CustomPropertyDrawer(typeof(InputCombo))]
    public class InputComboDrawer : PropertyDrawer
    {
        Rect totalPosition;
        float enumIndent = 30f;
        float enumBuffer = 5f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float inspectorWidth = EditorGUIUtility.currentViewWidth;
            float propWidth = (position.width / 4f) - enumBuffer;

            position = new Rect(position.x, position.y, position.width, 10f);

            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, label);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            position.x = enumIndent;

            position.y += EditorGUIUtility.singleLineHeight;

            EditorGUI.LabelField(new Rect(position.x + 20f, position.y, (propWidth * 2f) + (enumBuffer * 2f), EditorGUIUtility.singleLineHeight), new GUIContent("Directions"));
            EditorGUI.LabelField(new Rect(position.x + (propWidth * 2f) + (enumBuffer * 2f) + 20f, position.y, (propWidth * 2f) + (enumBuffer * 2f), EditorGUIUtility.singleLineHeight), new GUIContent("Buttons"));

            position.y += EditorGUIUtility.singleLineHeight;

            Rect directionRect = new Rect(position.x, position.y, propWidth, position.height);
            Rect directionManeuRect = new Rect(position.x + propWidth + enumBuffer, position.y, propWidth, position.height);
            Rect buttonRect = new Rect(position.x + (propWidth * 2f) + (enumBuffer * 3f), position.y, propWidth, position.height);
            Rect maneuverRect = new Rect(position.x + (propWidth * 3f) + (enumBuffer * 4f), position.y, propWidth, position.height);

            EditorGUI.PropertyField(directionRect, property.FindPropertyRelative("direction"), GUIContent.none);
            EditorGUI.PropertyField(directionManeuRect, property.FindPropertyRelative("directionManeuver"), GUIContent.none);
            EditorGUI.PropertyField(buttonRect, property.FindPropertyRelative("button"), GUIContent.none);
            EditorGUI.PropertyField(maneuverRect, property.FindPropertyRelative("buttonManeuver"), GUIContent.none);

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // return base.GetPropertyHeight(property, label);
            return EditorGUIUtility.singleLineHeight + 34f;
        }
    }
}
