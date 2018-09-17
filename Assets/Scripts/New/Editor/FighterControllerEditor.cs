using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Assault;
using Assault.Maneuvers;

namespace Assault.Editors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(FighterController))]
    public class FighterControllerEditor : Editor
    {
        List<int> _nodeList = new List<int>();
        int newFileNum = 1;

        const string assetExtension = ".asset";
        string fighterDirectory = "Assets/Entities/Fighters/";
        string maneuverDirectory, techniqueDirectory;

        FighterController _targetController;
        SerializedProperty _currentManeuver;
        bool _expandManeuver;

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
            maneuverDirectory = fighterDirectory + _targetController.name + "/Maneuvers/";
            techniqueDirectory = fighterDirectory + _targetController.name + "/Techniques/";

            _currentState = serializedObject.FindProperty("currentState");

            _maxWalkSpeed = serializedObject.FindProperty("_maxWalkSpeed");
            _maxRunSpeed = serializedObject.FindProperty("_maxRunSpeed");

            _airAcceleration = serializedObject.FindProperty("_airAcceleration");
            _maxAirSpeed = serializedObject.FindProperty("_maxAirSpeed");

            _maxAirJumps = serializedObject.FindProperty("_maxAirJumps");

            Moveset();
            Maneuvers();
        }

        void FileNumberSetup()
        {

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

                    if (inputCombo == null)
                    {
                        EditorGUI.LabelField(rect, "Corresponding InputComboNode could not be found", new GUIStyle { fontStyle = FontStyle.BoldAndItalic });

                        if (GUI.Button(
                            new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 3f, rect.width, EditorGUIUtility.singleLineHeight + 5f),
                            new GUIContent("Delete?")))
                        {
                            Maneuver maneuDel = (Maneuver)element.FindPropertyRelative("maneuver").objectReferenceValue;
                            if (_currentManeuver.objectReferenceValue == maneuDel) _currentManeuver = null;

                            try
                            {
                                AssetDatabase.DeleteAsset(maneuverDirectory + maneuDel.name + assetExtension);
                            }
                            catch { }

                            _maneuvers.serializedProperty.DeleteArrayElementAtIndex(index);
                        }

                        return;
                    }

                    rect.y -= EditorGUIUtility.singleLineHeight;

                    EditorGUI.PropertyField(rect, inputCombo, GUIContent.none);

                    rect.y += EditorGUI.GetPropertyHeight(inputCombo, GUIContent.none);
                    
                    Maneuver maneuver = (Maneuver)element.FindPropertyRelative("maneuver").objectReferenceValue;
                    EditorGUI.LabelField(new Rect(rect.x + 30f, rect.y + 5f, rect.width, EditorGUIUtility.singleLineHeight), maneuver.name, new GUIStyle { fontSize = 16, fontStyle = FontStyle.Bold });
                    
                },

                onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
                {

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

                    int nodeNum = Random.Range(0, 1000);
                    bool keepLooking = true;
                    while (keepLooking)
                    {
                        if (_nodeList.Contains(nodeNum)) nodeNum = Random.Range(0, 10000);

                        else keepLooking = false;
                    }

                    elementMoveS.FindPropertyRelative("node").intValue = nodeNum;
                    elementManeu.FindPropertyRelative("node").intValue = nodeNum;

                    Maneuver newAsset = ScriptableObject.CreateInstance<Maneuver>();
                    newAsset.name = "New Maneuver " + newFileNum.ToString();
                    string path = maneuverDirectory + newAsset.name + assetExtension;
                    AssetDatabase.CreateAsset(newAsset, path);
                    elementManeu.FindPropertyRelative("maneuver").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Maneuver>(path);
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

                    Maneuver maneuver = (Maneuver)currentManeu.FindPropertyRelative("maneuver").objectReferenceValue;

                    if ((Maneuver)_currentManeuver.objectReferenceValue == maneuver) _currentManeuver = null;

                    AssetDatabase.DeleteAsset(maneuverDirectory + currentManeu.name + assetExtension);

                    _moveset.serializedProperty.DeleteArrayElementAtIndex(indexMoveS);
                    list.serializedProperty.DeleteArrayElementAtIndex(index);
                },

                onSelectCallback = (ReorderableList list) =>
                {
                    int index = list.index;
                    _currentManeuver = list.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("maneuver");
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

            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            if (_currentManeuver != null)
            {
                /*
                Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);

                rect.x += 10; rect.width -= 10;
                _expandManeuver = EditorGUI.Foldout(rect, _expandManeuver, GUIContent.none);

                rect.x += 5; rect.width -= 5;
                EditorGUI.PropertyField(rect, _currentManeuver, GUIContent.none);
                */
                EditorGUILayout.PropertyField(_currentManeuver, GUIContent.none);
            }
            else
            {
                EditorGUILayout.LabelField(new GUIContent("No Maneuver selected"), new GUIStyle { fontStyle = FontStyle.BoldAndItalic });
            }


            EditorGUILayout.EndVertical();

            //EditorGUILayout.Space();
            
           // _moveset.DoLayoutList();


            EditorGUILayout.Space();
            _maneuvers.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        void SetupFolders()
        {

        }
    }
}
