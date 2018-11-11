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
            get { return (EditorGUIUtility.singleLineHeight * 6f); }
        }
        
        readonly GUIContent gui_boxSize = new GUIContent("Box Size");
        readonly GUIContent gui_offset = new GUIContent("Offset");
        readonly GUIContent gui_x = new GUIContent("X");
        readonly GUIContent gui_y = new GUIContent("Y");

        readonly GUIContent gui_rotation = new GUIContent("Rotation");

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

            Rect rect_vectHeader = new Rect(position.x, position.y, position.width * 0.2f, EditorGUIUtility.singleLineHeight);
            Rect rect_xLabel = new Rect(rect_vectHeader.xMax, rect_vectHeader.y, position.width * 0.1f, rect_vectHeader.height);
            Rect rect_x = new Rect(rect_xLabel.xMax, rect_xLabel.y, position.width * 0.28f, EditorGUIUtility.singleLineHeight);
            Rect rect_yLabel = new Rect(rect_x.xMax + (position.width * 0.02f), rect_x.y, rect_xLabel.width, rect_xLabel.height);
            Rect rect_y = new Rect(rect_yLabel.xMax, rect_yLabel.y, rect_x.width, rect_x.height);

            EditorGUI.LabelField(rect_vectHeader, gui_boxSize, new GUIStyle { fontStyle = FontStyle.Bold });
            EditorGUI.LabelField(rect_xLabel, gui_x);
            EditorGUI.PropertyField(rect_x, property.FindPropertyRelative("boxSize.x"), GUIContent.none);
            EditorGUI.LabelField(rect_yLabel, gui_y);
            EditorGUI.PropertyField(rect_y, property.FindPropertyRelative("boxSize.y"), GUIContent.none);

            rect_vectHeader.y += EditorGUIUtility.singleLineHeight + 2f;
            rect_xLabel.y += EditorGUIUtility.singleLineHeight + 2f;
            rect_x.y += EditorGUIUtility.singleLineHeight + 2f;
            rect_yLabel.y += EditorGUIUtility.singleLineHeight + 2f;
            rect_y.y += EditorGUIUtility.singleLineHeight + 2f;

            EditorGUI.LabelField(rect_vectHeader, gui_offset, new GUIStyle { fontStyle = FontStyle.Bold });
            EditorGUI.LabelField(rect_xLabel, gui_x);
            EditorGUI.PropertyField(rect_x, property.FindPropertyRelative("offset.x"), GUIContent.none);
            EditorGUI.LabelField(rect_yLabel, gui_y);
            EditorGUI.PropertyField(rect_y, property.FindPropertyRelative("offset.y"), GUIContent.none);

            Rect rect_rotation = new Rect(position.x, rect_vectHeader.yMax + buffer, position.width, EditorGUIUtility.singleLineHeight); Rect rect_hitMask = new Rect(position.x, rect_rotation.yMax + buffer, position.width, EditorGUIUtility.singleLineHeight);
            Rect rect_drawType = new Rect(position.x, rect_hitMask.yMax + buffer, position.width, EditorGUIUtility.singleLineHeight);
            
            EditorGUI.Slider(rect_rotation, property.FindPropertyRelative("rotation"), 0, 360f);
            EditorGUI.PropertyField(rect_hitMask, property.FindPropertyRelative("hitMask"), gui_hitMask);
            EditorGUI.PropertyField(rect_drawType, property.FindPropertyRelative("drawType"), gui_drawType);

            float boxSizeX = property.FindPropertyRelative("boxSize.x").floatValue;
            float boxSizeY = property.FindPropertyRelative("boxSize.y").floatValue;

            if (Mathf.Abs(boxSizeX) < 0.01f) boxSizeX = Mathf.Sign(boxSizeX) * 0.01f;
            if (Mathf.Abs(boxSizeY) < 0.01f) boxSizeY = Mathf.Sign(boxSizeY) * 0.01f;

            property.FindPropertyRelative("boxSize.x").floatValue = boxSizeX;
            property.FindPropertyRelative("boxSize.y").floatValue = boxSizeY;
        }
    }
}
