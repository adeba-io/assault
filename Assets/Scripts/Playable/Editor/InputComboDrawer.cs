using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(InputCombo))]
public class InputComboDrawer : PropertyDrawer
{
    Rect totalPosition;
    float enumIndent = 30f;
    float enumBuffer = 5f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float inspectorWidth = EditorGUIUtility.currentViewWidth;
        float propWidth = ((inspectorWidth - enumIndent) / 3f) - enumBuffer;

        position = new Rect(position.x, position.y, position.width, 10f);

        EditorGUI.BeginProperty(position, label, property);
        
        position = EditorGUI.PrefixLabel(position, label);
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        position.y += 16f;
        position.x = enumIndent;

        Rect directionRect = new Rect(position.x, position.y, propWidth, position.height);
        Rect buttonRect = new Rect(position.x + propWidth + enumBuffer, position.y, propWidth, position.height);
        Rect maneuverRect = new Rect(position.x + (propWidth * 2f) + (enumBuffer * 2f), position.y, propWidth, position.height);

        EditorGUI.PropertyField(directionRect, property.FindPropertyRelative("_inputDirection"), GUIContent.none);
        EditorGUI.PropertyField(buttonRect, property.FindPropertyRelative("_inputButton"), GUIContent.none);
        EditorGUI.PropertyField(maneuverRect, property.FindPropertyRelative("_buttonManeuver"), GUIContent.none);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // return base.GetPropertyHeight(property, label);
        return 34f;
    }
}
