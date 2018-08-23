using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MoveSet))]
public class SerialDictionaryDrawer : PropertyDrawer
{
    bool foldoutMoveSet = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);
        float inspectorWidth = EditorGUIUtility.currentViewWidth;

        GUIContent prefixLabel = new GUIContent(label.text + " Size : " + property.FindPropertyRelative("_count").intValue);
        GUIStyle labelStyle = new GUIStyle(); labelStyle.fontStyle = FontStyle.Bold;

        Rect foldoutRect = new Rect(position.x, position.y, position.width, position.height);
        
        EditorGUI.BeginProperty(position, prefixLabel, property);

        //position = EditorGUI.PrefixLabel(position, prefixLabel, labelStyle);
        foldoutMoveSet = EditorGUI.Foldout(position, foldoutMoveSet, prefixLabel, true);

        if (foldoutMoveSet)
        {
            int count = property.FindPropertyRelative("_count").intValue;

            for (int i = 0; i < count; i++)
            {
                
            }
        }

        EditorGUI.EndProperty();
        
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = 16f;
        int count;

        if (foldoutMoveSet)
        {
            height += 5f;
            count = property.FindPropertyRelative("_count").intValue;

            InputComboDrawer inputComboDrawer = new InputComboDrawer();
            float inputComboHeight = inputComboDrawer.GetPropertyHeight(property, label);
            

            for (int i = 0; i < property.FindPropertyRelative("_count").intValue; i++)
            {
                height += inputComboHeight; // for Input Combo
                height += 5f;
                // For Technique
                height += 10f;
            }
        }
        else
        {
            height = 16f;
        }

        return height;
    }
}
