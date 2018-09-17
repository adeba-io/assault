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
        return EditorGUIUtility.singleLineHeight + 16f;
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
        float buffer = position.width * 0.02f;

        EditorGUI.BeginProperty(position, label, property);

        EditorGUI.LabelField(position, label);

        SerializedProperty minValue = property.FindPropertyRelative("rangeStart");
        SerializedProperty maxValue = property.FindPropertyRelative("rangeEnd");

        position.y += EditorGUIUtility.singleLineHeight;

        Rect minLimitRect = new Rect(position.x + (position.width * 0.1f), position.y, (position.width * 0.1f) - buffer, position.height);
        Rect minValueRect = new Rect(position.x + (position.width * 0.2f), position.y, (position.width * 0.1f) - buffer, position.height);
        Rect sliderRect = new Rect(position.x + (position.width * 0.3f), position.y, (position.width * 0.5f) - buffer, position.height);
        Rect maxValueRect = new Rect(position.x + (position.width * 0.8f), position.y, (position.width * 0.1f) - buffer, position.height);
        Rect maxLimitRect = new Rect(position.x + (position.width * 0.9f), position.y, (position.width * 0.1f) - buffer, position.height);

        float minVal = minValue.intValue;
        float maxVal = maxValue.intValue;

        EditorGUI.MinMaxSlider(sliderRect, ref minVal, ref maxVal, range.minLimit, range.maxLimit);

        // Deals with rounding on negative
        int newMinVal = (int)(minVal - range.minLimit) + range.minLimit;
        int newMaxVal = (int)(maxVal - range.minLimit) + range.minLimit;

        EditorGUI.LabelField(minLimitRect, range.minLimit.ToString(), new GUIStyle { alignment = TextAnchor.MiddleCenter });
        EditorGUI.LabelField(maxLimitRect, range.maxLimit.ToString(), new GUIStyle { alignment = TextAnchor.MiddleCenter });

        newMinVal = EditorGUI.IntField(minValueRect, newMinVal);
        newMaxVal = EditorGUI.IntField(maxValueRect, newMaxVal);

        newMinVal = Mathf.Clamp(newMinVal, range.minLimit, newMaxVal - 1);
        newMaxVal = Mathf.Clamp(newMaxVal, newMinVal, range.maxLimit);

        minValue.intValue = (int)newMinVal;
        maxValue.intValue = (int)newMaxVal;

        EditorGUI.EndProperty();
        /*
        float newMin = minValue.intValue;
        float newMax = maxValue.intValue;

        float xDivision = position.width * 0.5f;
        float xLabelDiv = xDivision * 0.125f;

        float yDivision = position.height * 0.5f;
        EditorGUI.LabelField(new Rect(position.x, position.y, xDivision, yDivision), label);

        Rect mmRect = new Rect(position.x + xDivision + xLabelDiv, position.y, position.width - (xDivision + (xLabelDiv * 2f)), yDivision);

        EditorGUI.MinMaxSlider(mmRect, ref newMin, ref newMax, range.minLimit, range.maxLimit);

        // Deals with rounding on negatives
        int newMinI = (int)(newMin - (float)range.minLimit) + range.minLimit;
        int newMaxI = (int)(newMax - (float)range.minLimit) + range.minLimit;

        // Left Label
        Rect minRangeRect = new Rect(position.x + xDivision, position.y, xLabelDiv, yDivision);
        minRangeRect.x += xLabelDiv * 0.5f - 12f;
        minRangeRect.width = 24f;
        EditorGUI.LabelField(minRangeRect, range.minLimit.ToString());

        // Right Label
        Rect maxRangeRect = new Rect(minRangeRect);
        maxRangeRect.x = mmRect.xMax + (xLabelDiv * 0.5f) - 12f;
        maxRangeRect.width = 24f;
        EditorGUI.LabelField(maxRangeRect, range.maxLimit.ToString());

        int totalRange = Mathf.Max(range.maxLimit - range.minLimit, 1);
        Rect minLabelRect = new Rect(mmRect);
        minLabelRect.x = minLabelRect.x + minLabelRect.width * ((newMin - range.minLimit) / totalRange);
        minLabelRect.x -= 12;
        minLabelRect.y += yDivision;
        minLabelRect.width = 24;
        newMinI = Mathf.Clamp(EditorGUI.IntField(minLabelRect, newMinI), range.minLimit, newMaxI);

        Rect maxLabelRect = new Rect(mmRect);
        maxLabelRect.x = maxLabelRect.x + maxLabelRect.width * ((newMax - range.minLimit) / totalRange);
        maxLabelRect.x -= 12;
        maxLabelRect.x = Mathf.Max(maxLabelRect.x, minLabelRect.xMax);
        maxLabelRect.y += yDivision;
        maxLabelRect.width = 24;
        newMaxI = Mathf.Clamp(EditorGUI.IntField(maxLabelRect, newMaxI), newMinI, range.maxLimit);

        minValue.intValue = (int)newMinI;
        maxValue.intValue = (int)newMaxI;*/
    }
}
