using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(Assault.Maneuvers.VectorFrame))]
public class VectorFramesDrawer : PropertyDrawer
{
    float _buffer = 5f;

    public int maxFrame = 60;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect vectorLabel = new Rect(position.x, position.y, position.width / 8f, position.height);
        Rect vectorRect = new Rect(vectorLabel.x + vectorLabel.width, position.y, (position.width * (3f / 8f)) - _buffer, position.height);
        Rect frameLabel = new Rect(vectorRect.x + vectorRect.width + _buffer, position.y, position.width / 8f, position.height);
        Rect frameRect = new Rect(frameLabel.x + frameLabel.width, position.y, position.width * (3f / 8f), position.height);

        EditorGUI.LabelField(vectorLabel, new GUIContent("Vector"));
        EditorGUI.PropertyField(vectorRect, property.FindPropertyRelative("vector"), GUIContent.none);
        EditorGUI.LabelField(frameLabel, new GUIContent("Frame"));
        EditorGUI.IntSlider(frameRect, property.FindPropertyRelative("frame"), 1, maxFrame, GUIContent.none);

        /*
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
        
        EditorGUI.EndProperty();*/
    }
}
