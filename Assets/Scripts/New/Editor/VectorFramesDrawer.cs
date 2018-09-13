using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Assault.Maneuvers.VectorFrame))]
public class VectorFramesDrawer : PropertyDrawer
{
    float _buffer = 5f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float inspectorWidth = EditorGUIUtility.currentViewWidth;
        float propWidth = (inspectorWidth / 3f) - _buffer;
        propWidth = (propWidth > 100f) ? 100f : propWidth;

        position = new Rect(position.x, position.y, propWidth, position.height);

        Rect labelRect = new Rect(position.x, position.y, inspectorWidth / 10f, position.height);

        Rect vectRect = new Rect(position.x + labelRect.width + _buffer, position.y, (inspectorWidth / 2f) - _buffer, position.height);

        Rect framRect = new Rect(position.x + labelRect.width + vectRect.width + _buffer + 10f, position.y, inspectorWidth - labelRect.width - vectRect.width - _buffer, position.height);

        EditorGUI.BeginProperty(position, GUIContent.none, property);

        EditorGUI.LabelField(labelRect, label);

        EditorGUI.LabelField(new Rect(vectRect.x, vectRect.y, 40f, vectRect.height), "Vector");
        EditorGUI.PropertyField(new Rect(vectRect.x + 40f + _buffer, vectRect.y, vectRect.width - 40f - _buffer, vectRect.height), property.FindPropertyRelative("vector"), GUIContent.none);

        EditorGUI.LabelField(new Rect(framRect.x, framRect.y, 40f, vectRect.height), "Frame");
        EditorGUI.PropertyField(new Rect(framRect.x + 40f + _buffer, framRect.y, framRect.width - 80f, framRect.height), property.FindPropertyRelative("frame"), GUIContent.none);

        /* + 40f + _buffer
        EditorGUI.PrefixLabel(new Rect(position.x, position.y, 100f, position.height), label);

        position.x += 100f + _buffer;
        Rect vectorRect = new Rect(position.x, position.y, propWidth * 2, position.height);
        position.x += propWidth + propWidth + _buffer + _buffer;
        Rect frameRect = new Rect(position.x, position.y, propWidth, position.height);

        EditorGUI.PropertyField(vectorRect, property.FindPropertyRelative("vector"), GUIContent.none);
        EditorGUI.PropertyField(frameRect, property.FindPropertyRelative("frame"), new GUIContent("Frame"));
        */
        EditorGUI.EndProperty();
    }
}
