using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Assault;
using Assault.Maneuvers;

[CanEditMultipleObjects]
[CustomEditor(typeof(FighterController))]
public class FighterControllerEditor : Editor
{
    static List<int> _nodeList = new List<int>();

    FighterController _targetController;

    SerializedProperty _currentState;

    SerializedProperty _maxWalkSpeed;
    SerializedProperty _maxRunSpeed;

    SerializedProperty _airAcceleration;
    SerializedProperty _maxAirSpeed;

    SerializedProperty _maxAirJumps;

    ReorderableList _moveset;
    ReorderableList _maneuvers;


    GUIContent gui_currentState = new GUIContent("Current State");

    GUIContent gui_maxWalkSpeed = new GUIContent("Max Walk Speed");
    GUIContent gui_maxRunSpeed = new GUIContent("Max Run Speed");

    GUIContent gui_airAcceleration = new GUIContent("Air Acceleration");
    GUIContent gui_maxAirSpeed = new GUIContent("Max Air Speed");

    GUIContent gui_maxAirJumps = new GUIContent("Max Air Jumps");

    GUIContent gui_moveset = new GUIContent("Move List");
    GUIContent gui_maneuvers = new GUIContent("Maneuvers");

    #region OnEnable

    private void OnEnable()
    {
        _targetController = (FighterController)serializedObject.targetObject;

        _currentState = serializedObject.FindProperty("currentState");

        _maxWalkSpeed = serializedObject.FindProperty("_maxWalkSpeed");
        _maxRunSpeed = serializedObject.FindProperty("_maxRunSpeed");

        _airAcceleration = serializedObject.FindProperty("_airAcceleration");
        _maxAirSpeed = serializedObject.FindProperty("_maxAirSpeed");

        _maxAirJumps = serializedObject.FindProperty("_maxAirJumps");
        
        Moveset();
        Maneuvers();
    }

    void Moveset()
    {
        _moveset = new ReorderableList(serializedObject, serializedObject.FindProperty("_moveset"), true, true, true, true);
        _moveset.elementHeight = 100;

        _moveset.drawHeaderCallback =
            (Rect rect) =>
            {
                GUIStyle style = new GUIStyle();
                style.fontStyle = FontStyle.Bold;
                
                EditorGUI.LabelField(rect, new GUIContent("Moveset"), style);
            };

        _moveset.drawElementCallback =
            (Rect rect, int index, bool isActive, bool isFocuesd) =>
            {
                var element = _moveset.serializedProperty.GetArrayElementAtIndex(index);
                rect.y -= 10;

                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("inputCombo"), GUIContent.none);

                EditorGUI.PropertyField(
                    new Rect(rect.x + 20, rect.y + 20, rect.width - 50, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("node"));
                Debug.Log("Moveset Node " + element.FindPropertyRelative("node").intValue);
            };
    }

    void Maneuvers()
    {
        _maneuvers = new ReorderableList(serializedObject, serializedObject.FindProperty("_maneuvers"), true, true, true, true)
        {
            elementHeight = 60,

            drawHeaderCallback = (Rect rect) =>
            {
                rect.x += 20f;
                EditorGUI.LabelField(rect, new GUIContent("Maneuvers"), new GUIStyle { fontStyle = FontStyle.Bold });
            },

            drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = _maneuvers.serializedProperty.GetArrayElementAtIndex(index);
                int node = element.FindPropertyRelative("node").intValue;
                SerializedProperty inputCombo = null;
                
                for (int i = 0; i < _moveset.serializedProperty.arraySize; i++)
                {
                    var current = _moveset.serializedProperty.GetArrayElementAtIndex(i);

                    if (current.FindPropertyRelative("node").intValue == node)
                    {
                        inputCombo = current.FindPropertyRelative("inputCombo");
                        break;
                    }
                }
                Debug.Log("Maneuver Node " + element.FindPropertyRelative("node").intValue);

                if (inputCombo == null)
                {
                    EditorGUI.LabelField(rect, "Corresponding InputComboNode could not be found", new GUIStyle { fontStyle = FontStyle.BoldAndItalic });

                    if (GUI.Button(
                        new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 3f, rect.width, EditorGUIUtility.singleLineHeight + 5f), 
                        new GUIContent("Delete?") ))
                    {
                        _maneuvers.serializedProperty.DeleteArrayElementAtIndex(index);
                    }

                    return;
                }

                rect.y -= EditorGUIUtility.singleLineHeight;

                EditorGUI.PropertyField(rect, inputCombo, GUIContent.none);

                rect.y += EditorGUI.GetPropertyHeight(inputCombo, GUIContent.none);

                SerializedProperty maneuver = element.FindPropertyRelative("maneuver");
                EditorGUI.LabelField(new Rect(rect.x + 30f, rect.y + 5f, rect.width, EditorGUIUtility.singleLineHeight), maneuver.FindPropertyRelative("_name").stringValue, new GUIStyle { fontSize = 16, fontStyle = FontStyle.Bold });
            },

            onAddCallback = (ReorderableList list) =>
            {
                var indexManeu = list.serializedProperty.arraySize;
                list.serializedProperty.arraySize++;
                list.index = indexManeu;

                var indexMoveS = _moveset.serializedProperty.arraySize;
                _moveset.serializedProperty.arraySize++;
                _moveset.index = indexMoveS;

                var elementMoveS = _moveset.serializedProperty.GetArrayElementAtIndex(indexMoveS);
                var elementManeu = list.serializedProperty.GetArrayElementAtIndex(indexManeu);

                elementMoveS.FindPropertyRelative("node").intValue = 1;
                elementManeu.FindPropertyRelative("node").intValue = 1;

                
            },
            
            onRemoveCallback = (ReorderableList list) =>
            {
                int index = list.index;

                var currentManeu = list.serializedProperty.GetArrayElementAtIndex(index);
                int nodeManeu = currentManeu.FindPropertyRelative("node").intValue;

                SerializedProperty currentMoveS = null;
                int indexMoveS = 0;

                for (int i = 0; i < _moveset.serializedProperty.arraySize; i++)
                {
                    var current = _moveset.serializedProperty.GetArrayElementAtIndex(i);

                    if (nodeManeu == current.FindPropertyRelative("node").intValue)
                    {
                        currentMoveS = current;
                        indexMoveS = i;
                        break;
                    }
                }

                if (currentMoveS == null) return;

                _moveset.serializedProperty.DeleteArrayElementAtIndex(indexMoveS);
                list.serializedProperty.DeleteArrayElementAtIndex(index);
            }
        };
    }

    #endregion

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.PropertyField(_currentState, gui_currentState);

        EditorGUILayout.PropertyField(_maxWalkSpeed, gui_maxWalkSpeed);
        EditorGUILayout.PropertyField(_maxRunSpeed, gui_maxRunSpeed);

        EditorGUILayout.PropertyField(_airAcceleration, gui_airAcceleration);
        EditorGUILayout.PropertyField(_maxAirSpeed, gui_maxAirSpeed);

        EditorGUILayout.PropertyField(_maxAirJumps, gui_maxAirJumps);

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal(GUI.skin.box);


        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        _moveset.DoLayoutList();
        _maneuvers.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
    /*
    void DrawMoveset()
    {
        if (!_moveset.isArray || !_maneuvers.isArray)
        {/*
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Moveset fields are not arrays");
            EditorGUILayout.EndVertical();*
            return;
        }

        SerialPropToArray<InputComboNode>(_moveset);
    }

    List<T> SerialPropToArray<T>(SerializedProperty array)
    {
        if (!array.isArray) return new List<T>();
        
        array.Next(true); // skip generic field
        array.Next(true); // advance to size field

        int arrayLength = array.intValue;

        array.Next(true); // advance to first array index

        // Write values to list
        List<T> values = new List<T>(arrayLength);
        int lastIndex = arrayLength - 1;/*
        for (int i = 0;  i < arrayLength; i++)
        {
            Debug.Log(array.arrayElementType);
        }*

        Debug.Log(serializedObject.FindProperty("_moveset.Array.data[0].inputCombo").objectReferenceValue);

        return new List<T>();
    }*/
}
