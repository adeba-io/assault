using System.IO;
using System.Collections;
using System.Collections.Generic;
using Assault.Managers;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;

namespace Assault.Editors
{
    [CustomEditor(typeof(GameManager))]
    public class GameMngrEditor : Editor
    {
        AnimBool _showDebugValues;
        
        SerializedProperty _FPSUpdateInterval;

        ReorderableList _stageNames;
        string[] _stages;

        readonly GUIContent gui_stageNames = new GUIContent("Fighter Stages");

        readonly GUIContent gui_debugValues = new GUIContent("Debug");
        readonly GUIContent gui_FPSUpdateInterval = new GUIContent("FPS Update");

        private void OnEnable()
        {
            _showDebugValues = new AnimBool(false);
            _showDebugValues.valueChanged.AddListener(Repaint);
            
            _FPSUpdateInterval = serializedObject.FindProperty("_FPSUpdateInterval");

            StageNames();
        }

        void StageNames()
        {
            _stageNames = new ReorderableList(serializedObject, serializedObject.FindProperty("_stageNames"), false, true, true, true)
            {
                elementHeight = 24,
                drawHeaderCallback = (Rect rect) =>
                {
                    rect.x += rect.width * 0.1f;
                    rect.width *= 0.9f;
                    EditorGUI.LabelField(rect, gui_stageNames);
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = _stageNames.serializedProperty.GetArrayElementAtIndex(index);
                    rect.x += rect.width * 0.05f;
                    rect.width *= 0.95f;

                    EditorGUI.LabelField(rect, element.stringValue, new GUIStyle { alignment = TextAnchor.MiddleLeft });
                },
                onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
                {
                    GenericMenu menu = new GenericMenu();
                    var guids = AssetDatabase.FindAssets("", new[] { "Assets/_Scenes/Stages" });

                    for (int i = 0; i < guids.Length; i++)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        string name = Path.GetFileNameWithoutExtension(path);

                        if (AlreadyInStages(name)) continue;

                        menu.AddItem(new GUIContent(name), false, AddHandler, path);
                    }

                    if (menu.GetItemCount() < 1)
                    {
                        menu.AddDisabledItem(new GUIContent("No Stages Found in"));
                        menu.AddDisabledItem(new GUIContent("'_Scenes/Stages'"));
                    }

                    menu.ShowAsContext();
                }
            };
        }

        void AddHandler(object target)
        {
            var data = (string)target;

            int index = _stageNames.serializedProperty.arraySize++;
            _stageNames.index = index;
            var element = _stageNames.serializedProperty.GetArrayElementAtIndex(index);
            element.stringValue = Path.GetFileNameWithoutExtension(data);

            serializedObject.ApplyModifiedProperties();
        }

        bool AlreadyInStages(string stage)
        {
            for (int i = 0; i < _stageNames.serializedProperty.arraySize; i++)
            {
                if (_stageNames.serializedProperty.GetArrayElementAtIndex(i).stringValue == stage)
                    return true;
            }

            return false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            _showDebugValues.target = EditorGUILayout.ToggleLeft(gui_debugValues, _showDebugValues.target);

            if (EditorGUILayout.BeginFadeGroup(_showDebugValues.faded))
            {
                EditorGUILayout.PropertyField(_FPSUpdateInterval, gui_FPSUpdateInterval);
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.Space();

            _stageNames.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
