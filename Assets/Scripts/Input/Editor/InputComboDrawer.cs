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
            float ropWidth = (position.width / 4f) - enumBuffer;
            float propWidth = position.width / 11f;

            position = new Rect(position.x, position.y, position.width, 10f);

            EditorGUI.BeginProperty(position, label, property);

            int indent = EditorGUI.indentLevel;

            if (label != GUIContent.none)
            {
                position = EditorGUI.PrefixLabel(position, label);
                EditorGUI.indentLevel = 0;
                position.x = enumIndent;

                position.y += EditorGUIUtility.singleLineHeight;
            }

            EditorGUI.LabelField(new Rect(position.x + 20f, position.y, (ropWidth * 2f) + (enumBuffer * 2f), EditorGUIUtility.singleLineHeight), new GUIContent("Directions"));
            EditorGUI.LabelField(new Rect(position.x + (ropWidth * 2f) + (enumBuffer * 2f) + 20f, position.y, (ropWidth * 2f) + (enumBuffer * 2f), EditorGUIUtility.singleLineHeight), new GUIContent("Buttons"));

            position.y += EditorGUIUtility.singleLineHeight;

            Rect horiRect = new Rect(position.x, position.y, propWidth * 2 - enumBuffer, position.height);
            Rect vertRect = new Rect(horiRect.x + horiRect.width + enumBuffer, position.y, propWidth * 2 - enumBuffer, position.height);
            Rect directionManeuRect = new Rect(vertRect.x + vertRect.width + enumBuffer, position.y, propWidth * 2 - enumBuffer, position.height);


            Rect buttonRect = new Rect(directionManeuRect.xMax + enumBuffer + enumBuffer, position.y, ropWidth - enumBuffer - enumBuffer, position.height);
            Rect maneuverRect = new Rect(buttonRect.xMax + enumBuffer, position.y, ropWidth - enumBuffer - enumBuffer, position.height);

            EditorGUI.PropertyField(horiRect, property.FindPropertyRelative("horizontalControl"), GUIContent.none);
            EditorGUI.PropertyField(vertRect, property.FindPropertyRelative("verticalControl"), GUIContent.none);
            EditorGUI.PropertyField(directionManeuRect, property.FindPropertyRelative("directionManeuver"), GUIContent.none);
            EditorGUI.PropertyField(buttonRect, property.FindPropertyRelative("button"), GUIContent.none);
            EditorGUI.PropertyField(maneuverRect, property.FindPropertyRelative("buttonManeuver"), GUIContent.none);

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // return base.GetPropertyHeight(property, label);
            return (label == GUIContent.none ? 0 : EditorGUIUtility.singleLineHeight) + 34f;
        }
    }
}
