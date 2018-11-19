using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(IntRangeAttribute))]
public class IntRangeDrawer : PropertyDrawer
{
    IntRangeAttribute _attribute;

    public IntRangeDrawer() { }

    public IntRangeDrawer(IntRangeAttribute attribute)
    {
        _attribute = attribute;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        if (label != GUIContent.none) height += EditorGUIUtility.singleLineHeight;

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
       // base.OnGUI(position, property, label);

        if (property.type != typeof(IntRange).ToString())
        {
            Debug.LogWarning("Use only with IntRange type");
            return;
        }

        IntRangeAttribute range;

        if (_attribute != null) range = _attribute;
        else range = (IntRangeAttribute)attribute;

        position.height = EditorGUIUtility.singleLineHeight;
        float posWidth = position.width;

        EditorGUI.BeginProperty(position, label, property);

        if (label != GUIContent.none)
            EditorGUI.LabelField(position, label);

        SerializedProperty minValue = property.FindPropertyRelative("rangeStart");
        SerializedProperty maxValue = property.FindPropertyRelative("rangeEnd");

        if (label != GUIContent.none)
            position.y += EditorGUIUtility.singleLineHeight;
        position.width *= 0.98f;

        Rect rect_minLimit = new Rect(position.x + (posWidth * 0.02f), position.y, posWidth * 0.08f, EditorGUIUtility.singleLineHeight);
        Rect rect_minValue = new Rect(rect_minLimit.xMax, rect_minLimit.y, posWidth * 0.15f, rect_minLimit.height);
        Rect rect_slider = new Rect(rect_minValue.xMax, rect_minValue.y, posWidth * 0.5f, rect_minLimit.height);
        Rect rect_maxValue = new Rect(rect_slider.xMax, rect_slider.y, rect_minValue.width, rect_minLimit.height);
        Rect rect_maxLimit = new Rect(rect_maxValue.xMax, rect_maxValue.y, rect_minLimit.width, rect_minLimit.height);

        float minVal = minValue.intValue;
        float maxVal = maxValue.intValue;

        EditorGUI.MinMaxSlider(rect_slider, ref minVal, ref maxVal, range.minLimit, range.maxLimit);

        // Deals with rounding on negative
        int newMinVal = (int)(minVal - range.minLimit) + range.minLimit;
        int newMaxVal = (int)(maxVal - range.minLimit) + range.minLimit;

        EditorGUI.LabelField(rect_minLimit, range.minLimit.ToString(), new GUIStyle { alignment = TextAnchor.MiddleCenter });
        EditorGUI.LabelField(rect_maxLimit, range.maxLimit.ToString(), new GUIStyle { alignment = TextAnchor.MiddleCenter });

        newMinVal = EditorGUI.IntField(rect_minValue, newMinVal);
        newMaxVal = EditorGUI.IntField(rect_maxValue, newMaxVal);

        newMinVal = Mathf.Clamp(newMinVal, range.minLimit, newMaxVal - 1);
        newMaxVal = Mathf.Clamp(newMaxVal, newMinVal, range.maxLimit);

        if (newMinVal == newMaxVal)
        {
            newMaxVal++;

            if (newMaxVal > range.maxLimit)
            {
                newMaxVal = range.maxLimit;
                newMinVal--;
            }
        }

        minValue.intValue = (int)newMinVal;
        maxValue.intValue = (int)newMaxVal;

        EditorGUI.EndProperty();
    }
}
