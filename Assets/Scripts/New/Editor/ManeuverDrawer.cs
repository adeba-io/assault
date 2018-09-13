using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Assault.Maneuvers;

[CustomPropertyDrawer(typeof(Maneuver))]
public class ManeuverDrawer : PropertyDrawer
{
    bool initialize = true;

    float _buffer = 4f;
    bool expanded;

    ReorderableList _moveFrames;
    ReorderableList _forceFrames;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);

        //Enable(property);

        Rect foldoutRect = new Rect(position.position, new Vector2(position.width, EditorGUIUtility.singleLineHeight));

        expanded = EditorGUI.Foldout(foldoutRect, expanded, "Maneuver: " + property.FindPropertyRelative("_name").stringValue, true);

        if (expanded)
        {
            Rect posi = new Rect(foldoutRect.x + (foldoutRect.width * 0.02f), foldoutRect.y + EditorGUIUtility.singleLineHeight, foldoutRect.width * 0.95f, EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(posi, property.FindPropertyRelative("_name"));
            posi.y += posi.height + _buffer;

            EditorGUI.PropertyField(posi, property.FindPropertyRelative("_toSet"), new GUIContent("State to Set to"));
            posi.y += posi.height + _buffer;

            EditorGUI.PropertyField(posi, property.FindPropertyRelative("_accelerateCurveX"));
            posi.y += posi.height + _buffer;
            EditorGUI.PropertyField(posi, property.FindPropertyRelative("_accelerateCurveY"));
            posi.y += posi.height + _buffer;

            EditorGUI.PropertyField(posi, property.FindPropertyRelative("_totalFrameCount"));
            posi.y += posi.height + _buffer;

            EditorGUI.PropertyField(posi, property.FindPropertyRelative("_cancelRegion"));
            IntRangeDrawer rangeDrawer = new IntRangeDrawer();
            posi.y += rangeDrawer.GetPropertyHeight(property.FindPropertyRelative("_cancelRegion"), GUIContent.none) + _buffer;

            _moveFrames.DoList(posi);
            posi.y += _moveFrames.headerHeight + (_moveFrames.elementHeight * Mathf.Max(_moveFrames.serializedProperty.arraySize, 1)) + _moveFrames.footerHeight + (_moveFrames.displayAdd || _moveFrames.displayRemove ? 10f : 0) + _buffer;

            _forceFrames.DoList(posi);
        }
    }

    void Enable(SerializedProperty property)
    {
        _moveFrames = new ReorderableList(property.serializedObject, property.FindPropertyRelative("_moveFrames"), true, true, true, true)
        {
            elementHeight = 20,
            footerHeight = 20,

            drawHeaderCallback = (Rect rect) =>
            {
                rect.x += 40f;
                EditorGUI.LabelField(rect, new GUIContent("Move Frames"), new GUIStyle { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft });
            },
            
            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _moveFrames.serializedProperty.GetArrayElementAtIndex(index);

                EditorGUI.PropertyField(rect, element);
                
                /*
                for (int i = 1; i < _moveFrames.serializedProperty.arraySize; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (_moveFrames.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("frame").intValue > _moveFrames.serializedProperty.GetArrayElementAtIndex(j + 1).FindPropertyRelative("frame").intValue)
                        {
                            int temp = _moveFrames.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("frame").intValue;
                            _moveFrames.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("frame").intValue = _moveFrames.serializedProperty.GetArrayElementAtIndex(j + 1).FindPropertyRelative("frame").intValue;
                            _moveFrames.serializedProperty.GetArrayElementAtIndex(j + 1).FindPropertyRelative("frame").intValue = temp;

                            if (j == index) _moveFrames.index++;
                            else if (j + 1 == index) _moveFrames.index--;
                        }
                    }
                }*/
            },
            /*
            drawFooterCallback = (Rect rect) =>
            {
                if (GUI.Button(rect, new GUIContent("Organize")))
                {
                    int index = _moveFrames.index;

                    for (int i = 1; i < _moveFrames.serializedProperty.arraySize; i++)
                    {
                        for (int j = 0; j < i; j++)
                        {
                            if (_moveFrames.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("frame").intValue > _moveFrames.serializedProperty.GetArrayElementAtIndex(j + 1).FindPropertyRelative("frame").intValue)
                            {
                                int temp = _moveFrames.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("frame").intValue;
                                _moveFrames.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("frame").intValue = _moveFrames.serializedProperty.GetArrayElementAtIndex(j + 1).FindPropertyRelative("frame").intValue;
                                _moveFrames.serializedProperty.GetArrayElementAtIndex(j + 1).FindPropertyRelative("frame").intValue = temp;

                                if (j == index) _moveFrames.index++;
                                else if (j + 1 == index) _moveFrames.index--;
                            }
                        }
                    }
                }
            }*/

        };

        _forceFrames = new ReorderableList(property.serializedObject, property.FindPropertyRelative("_forceFrames"), true, true, true, true)
        {
            elementHeight = 20,

            drawHeaderCallback = (Rect rect) =>
            {
                rect.x += 40f;
                EditorGUI.LabelField(rect, new GUIContent("Force Frames"), new GUIStyle { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft });
            },

            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _forceFrames.serializedProperty.GetArrayElementAtIndex(index);

                EditorGUI.PropertyField(rect, element);

                for (int i = 1; i < _forceFrames.serializedProperty.arraySize; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        if (_forceFrames.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("frame").intValue > _forceFrames.serializedProperty.GetArrayElementAtIndex(j + 1).FindPropertyRelative("frame").intValue)
                        {
                            int temp = _forceFrames.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("frame").intValue;
                            _forceFrames.serializedProperty.GetArrayElementAtIndex(j).FindPropertyRelative("frame").intValue = _forceFrames.serializedProperty.GetArrayElementAtIndex(j + 1).FindPropertyRelative("frame").intValue;
                            _forceFrames.serializedProperty.GetArrayElementAtIndex(j + 1).FindPropertyRelative("frame").intValue = temp;

                            if (j == index) _forceFrames.index++;
                            else if (j + 1 == index) _forceFrames.index--;
                        }
                    }
                }
            }

        };
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        Debug.Log("Called");
        Enable(property);

        IntRangeDrawer rangeDrawer = new IntRangeDrawer();
        float height = base.GetPropertyHeight(property, label);
        if (expanded)
        {
            height += (EditorGUIUtility.singleLineHeight + _buffer) * 5;
            height += rangeDrawer.GetPropertyHeight(property.FindPropertyRelative("_cancelRegion"), GUIContent.none);
            height += _moveFrames.headerHeight + (_moveFrames.elementHeight * Mathf.Max(_moveFrames.serializedProperty.arraySize, 1)) + _moveFrames.footerHeight + (_moveFrames.displayAdd || _moveFrames.displayRemove ? 10f : 0) + _buffer;
            height += _forceFrames.headerHeight + (_forceFrames.elementHeight * Mathf.Max(_forceFrames.serializedProperty.arraySize, 1)) + _forceFrames.footerHeight + (_forceFrames.displayAdd || _forceFrames.displayRemove ? 10f : 0) + _buffer;
        }
        return height;
    }

    
}
