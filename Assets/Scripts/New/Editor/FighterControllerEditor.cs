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
        const string assetExtension = ".asset";
        string fighterDirectory = "Assets/Entities/Fighters/";
        string maneuverDirectory, techniqueDirectory;

        int _maxManeuvers = 100;
        List<int> _maneuverNodes = new List<int>();
        Queue<int> _nodeQueue;

        bool _isPrefab = false;
        bool _isNotInstance = false;

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
            _targetController.path = fighterDirectory + _targetController.name;
            maneuverDirectory = _targetController.path + "/Maneuvers/";
            techniqueDirectory = _targetController.path + "/Techniques/";

            _isPrefab = AssetDatabase.Contains(target);
            _isNotInstance = PrefabUtility.GetCorrespondingObjectFromSource(target) == null;
            

            _currentState = serializedObject.FindProperty("currentState");

            _maxWalkSpeed = serializedObject.FindProperty("_maxWalkSpeed");
            _maxRunSpeed = serializedObject.FindProperty("_maxRunSpeed");

            _airAcceleration = serializedObject.FindProperty("_airAcceleration");
            _maxAirSpeed = serializedObject.FindProperty("_maxAirSpeed");

            _maxAirJumps = serializedObject.FindProperty("_maxAirJumps");

            Moveset();
            Maneuvers();

            NodeSetup();
        }

        void NodeSetup()
        {
            _nodeQueue = new Queue<int>(_maxManeuvers);

            for (int i = 0; i < _maxManeuvers; i++)
            {
                bool nodeAlreadyUsed = false;

                for (int j = 0; j < _moveset.serializedProperty.arraySize; j++)
                {
                    var current = _moveset.serializedProperty.GetArrayElementAtIndex(j);
                    int currNode = current.FindPropertyRelative("node").intValue;

                    if (currNode == i)
                    {
                        nodeAlreadyUsed = true;
                        break;
                    }
                }

                if (!nodeAlreadyUsed)
                    _nodeQueue.Enqueue(i);
            }
        }

        void Maneuvers()
        {
            _maneuvers = new ReorderableList(serializedObject, serializedObject.FindProperty("_maneuvers"), false, true, true, true)
            {
                drawHeaderCallback = (Rect rect) =>
                {
                    rect.x += 15f;
                    rect.width -= 15f;
                    EditorGUI.LabelField(rect, new GUIContent("Maneuvers"), new GUIStyle { fontStyle = FontStyle.Bold });
                },
                
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = _maneuvers.serializedProperty.GetArrayElementAtIndex(index);
                    int node = element.FindPropertyRelative("node").intValue;
                    SerializedProperty inputCombo = FindInputComboByNode(node);

                    // Handles the case of no existing corresponding InputComboNode
                    if (inputCombo == null)
                    {
                        Rect errorRect = rect;
                        errorRect.x += 5; errorRect.width -= 5;

                        EditorGUI.LabelField(errorRect, "Corresponding InputComboNode could not be found", new GUIStyle { fontStyle = FontStyle.BoldAndItalic });

                        errorRect.y += EditorGUIUtility.singleLineHeight + 3f;
                        errorRect.height = EditorGUIUtility.singleLineHeight + 2f;

                        Rect createNew = errorRect;
                        createNew.y += EditorGUIUtility.singleLineHeight + 5f;

                        if (GUI.Button(createNew, new GUIContent("Create New InputCombo?")))
                        {
                            int movesetIndex = _moveset.serializedProperty.arraySize++;

                            var movesetElement = _moveset.serializedProperty.GetArrayElementAtIndex(movesetIndex);
                            movesetElement.FindPropertyRelative("node").intValue = node;
                        }
                        else if (GUI.Button(errorRect, new GUIContent("Delete?")))
                        {
                            Maneuver maneuDel = (Maneuver)element.FindPropertyRelative("maneuver").objectReferenceValue;
                            if (_currentManeuver.objectReferenceValue == maneuDel) _currentManeuver = null;

                            try
                            {
                                AssetDatabase.DeleteAsset(maneuDel.path);
                            }
                            catch { }

                            _maneuvers.serializedProperty.DeleteArrayElementAtIndex(index);
                        }

                        return;
                    }
                    
                    EditorGUI.PropertyField(rect, inputCombo, GUIContent.none);
                    rect.y += EditorGUI.GetPropertyHeight(inputCombo, GUIContent.none);

                    try
                    {
                        Maneuver maneuver = (Maneuver)element.FindPropertyRelative("maneuver").objectReferenceValue;
                        EditorGUI.LabelField(new Rect(rect.x + 15, rect.y + 5, rect.width - 15, 30f), maneuver.name, new GUIStyle { fontSize = 12, fontStyle = FontStyle.Bold });

                        maneuver.path = maneuverDirectory + maneuver.name + assetExtension;
                        maneuver.fighterController = _targetController;
                    }
                    catch
                    {
                        EditorGUI.LabelField(new Rect(rect.x + 15, rect.y + 5, rect.width - 15, 30f), "No Maneuver", new GUIStyle { fontSize = 12, fontStyle = FontStyle.BoldAndItalic });
                    }
                },

                onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Create new"), false, AddHandler, new AddData { type = AddType.New, list = ReLists.Maneuvers });
                    menu.AddItem(new GUIContent("Create empty"), false, AddHandler, new AddData { type = AddType.FromExisting, list = ReLists.Maneuvers });

                    menu.ShowAsContext();
                },

                onRemoveCallback = (ReorderableList list) =>
                {
                    if (!EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this Maneuver?", "Yes", "No")) return;

                    int index = list.index;

                    var maneuverCurrent = list.serializedProperty.GetArrayElementAtIndex(index);
                    int nodeManeuver = maneuverCurrent.FindPropertyRelative("node").intValue;

                    SerializedProperty currentMoveS = null;
                    int movesetIndex = 0;

                    for (int i = 0; i < _moveset.serializedProperty.arraySize; i++)
                    {
                        var current = _moveset.serializedProperty.GetArrayElementAtIndex(i);

                        if (nodeManeuver == current.FindPropertyRelative("node").intValue)
                        {
                            currentMoveS = current;
                            movesetIndex = i;
                            break;
                        }
                    }

                    if (currentMoveS == null) return;

                    Maneuver delManeuver = (Maneuver)maneuverCurrent.FindPropertyRelative("maneuver").objectReferenceValue;

                    if (delManeuver != null)
                    {
                        if (delManeuver == (Maneuver)_currentManeuver.objectReferenceValue) _currentManeuver = null;

                        if (delManeuver.path != "" && delManeuver.path != null)
                            AssetDatabase.DeleteAsset(delManeuver.path);
                    }

                    _moveset.serializedProperty.DeleteArrayElementAtIndex(movesetIndex);
                    list.serializedProperty.DeleteArrayElementAtIndex(index);

                    _nodeQueue.Enqueue(index);
                },

                onSelectCallback = (ReorderableList list) =>
                {
                    int index = list.index;
                    _currentManeuver = list.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("maneuver");
                }
            };
        }

        void AddHandler(object target)
        {
            var data = (AddData)target;

            int movesetIndex, listIndex;

            movesetIndex = _moveset.serializedProperty.arraySize++;
            _moveset.index = movesetIndex;

            var movesetElement = _moveset.serializedProperty.GetArrayElementAtIndex(movesetIndex);
            SerializedProperty listElement = null;

            switch (data.list)
            {
                case ReLists.Maneuvers:
                    listIndex = _maneuvers.serializedProperty.arraySize++;
                    _maneuvers.index = listIndex;

                    listElement = _maneuvers.serializedProperty.GetArrayElementAtIndex(listIndex);
                    break;
                case ReLists.Techniques:

                    break;
                default:
                    Debug.LogError("Could not find correct ReorderableList");
                    return;
            }

            int newNode = _nodeQueue.Dequeue();

            movesetElement.FindPropertyRelative("node").intValue = newNode;
            listElement.FindPropertyRelative("node").intValue = newNode;
            
            listElement.FindPropertyRelative("maneuver").objectReferenceValue = null;
            
            if (data.type == AddType.New)
            {
                switch (data.list)
                {
                    case ReLists.Maneuvers:
                        Maneuver newAsset = CreateInstance<Maneuver>();
                        newAsset.name = "New Maneuver " + newNode;
                        newAsset.path = maneuverDirectory + newAsset.name + assetExtension;
                        newAsset.fighterController = _targetController;

                        AssetDatabase.CreateAsset(newAsset, newAsset.path);
                        listElement.FindPropertyRelative("maneuver").objectReferenceValue = newAsset;
                        break;
                    case ReLists.Techniques:

                        break;
                }
            }

            serializedObject.ApplyModifiedProperties();
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
                    
                };
        }

        SerializedProperty FindInputComboByNode(int node)
        {
            for (int i = 0; i < _moveset.serializedProperty.arraySize; i++)
            {
                var current = _moveset.serializedProperty.GetArrayElementAtIndex(i);

                if (current.FindPropertyRelative("node").intValue == node)
                    return current.FindPropertyRelative("inputCombo");
            }

            return null;
        }

        #endregion

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            /*
            if (_isPrefab || _isNotInstance)
            {
                EditorGUILayout.HelpBox("Modify the prefab and not this instance", MessageType.Warning);

                if (GUILayout.Button("Select Prefab"))
                    Selection.activeObject = PrefabUtility.GetPrefabObject(target);

                return;
            }
            */
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
                EditorGUILayout.PropertyField(_currentManeuver, GUIContent.none);
            }
            else
            {
                EditorGUILayout.LabelField(new GUIContent("No Maneuver selected"), new GUIStyle { fontStyle = FontStyle.BoldAndItalic });
            }


            EditorGUILayout.EndVertical();

            //EditorGUILayout.Space();

             //_moveset.DoLayoutList();

            if (_maneuvers.serializedProperty.arraySize < 1)
                _maneuvers.elementHeight = 20;
            else
                _maneuvers.elementHeight = 63;

            EditorGUILayout.Space();
            _maneuvers.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        enum AddType { New, FromExisting }
        enum ReLists { Maneuvers, Techniques }

        struct AddData { public AddType type; public ReLists list; }
    }
}
