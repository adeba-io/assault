using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Assault.Editors
{
    [CustomPropertyDrawer(typeof(Boxes.InteractionBoxData))]
    public class IBoxDataDrawer : PropertyDrawer
    {
        public static float propertyHeight
        {
            get { return (EditorGUIUtility.singleLineHeight * 4f); }
        }
        
        readonly GUIContent gui_boxSize = new GUIContent("Box Size");
        readonly GUIContent gui_bSX = new GUIContent("X");
        readonly GUIContent gui_bSY = new GUIContent("Y");
        readonly GUIContent gui_hitMask = new GUIContent("Hit Mask");
        readonly GUIContent gui_drawType = new GUIContent("Box Draw Type");

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return propertyHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // base.OnGUI(position, property, label);

            float buffer = 3f;

            Rect boxSizeHeaderRect = new Rect(position.x, position.y, position.width * 0.2f, EditorGUIUtility.singleLineHeight);
            Rect bSXLabelRect = new Rect(boxSizeHeaderRect.xMax, boxSizeHeaderRect.y, position.width * 0.1f, boxSizeHeaderRect.height);
            Rect bSXRect = new Rect(bSXLabelRect.xMax, bSXLabelRect.y, position.width * 0.28f, EditorGUIUtility.singleLineHeight);
            Rect bSYLabelRect = new Rect(bSXRect.xMax + (position.width * 0.02f), bSXRect.y, bSXLabelRect.width, bSXLabelRect.height);
            Rect bSYRect = new Rect(bSYLabelRect.xMax, bSYLabelRect.y, bSXRect.width, bSXRect.height);

            Rect hitMaskRect = new Rect(position.x, bSXRect.yMax + buffer, position.width, EditorGUIUtility.singleLineHeight);
            Rect drawTypeRect = new Rect(position.x, hitMaskRect.yMax + buffer, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(boxSizeHeaderRect, gui_boxSize, new GUIStyle { fontStyle = FontStyle.Bold });
            EditorGUI.LabelField(bSXLabelRect, gui_bSX);
            EditorGUI.PropertyField(bSXRect, property.FindPropertyRelative("boxSizeX"), GUIContent.none);
            EditorGUI.LabelField(bSYLabelRect, gui_bSY);
            EditorGUI.PropertyField(bSYRect, property.FindPropertyRelative("boxSizeY"), GUIContent.none);

            EditorGUI.PropertyField(hitMaskRect, property.FindPropertyRelative("hitMask"), gui_hitMask);
            EditorGUI.PropertyField(drawTypeRect, property.FindPropertyRelative("drawType"), gui_drawType);
        }
    }
}
