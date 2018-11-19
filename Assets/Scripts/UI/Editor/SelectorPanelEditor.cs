using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

using Assault.Managers;

namespace Assault.Editors
{
    [CustomEditor(typeof(UI.SelectorPanel))]
    public class SelectorPanelEditor : Editor
    {
        UI.SelectorPanel _target;
        Image _targetImage;

        SerializedProperty _selectorType;
        SerializedProperty _panelSprite;
        SerializedProperty _previewImage;

        SerializedProperty _stageName;

        SerializedProperty _fighterReference;
        SerializedProperty _colorNumber;

        readonly GUIContent gui_selectorType = new GUIContent("Panel Type");
        readonly GUIContent gui_panelSprite = new GUIContent("Panel Sprite");
        readonly GUIContent gui_previewImage = new GUIContent("Preview Image");

        readonly GUIContent gui_stageName = new GUIContent("Stage Name");

        readonly GUIContent gui_fighterReference = new GUIContent("Fighter Reference");
        readonly GUIContent gui_colorNumber = new GUIContent("Color Number");

        readonly GUIContent gui_select = new GUIContent("Select");

        private void OnEnable()
        {
            _target = (UI.SelectorPanel)target;
            _targetImage = _target.GetComponent<Image>();

            _selectorType = serializedObject.FindProperty("selectorType");
            _panelSprite = serializedObject.FindProperty("_panelSprite");
            _previewImage = serializedObject.FindProperty("previewImage");

            _stageName = serializedObject.FindProperty("stageName");

            _fighterReference = serializedObject.FindProperty("fighterReference");
            _colorNumber = serializedObject.FindProperty("colorNumber");
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_selectorType, gui_selectorType);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                if (_target.selectorType == Types.SelectorType.Confirmation)
                    _targetImage.color = Color.clear;
                else _targetImage.color = Color.white;

                return;
            }

            if (_target.selectorType == Types.SelectorType.Stage)
            {
                Rect rect_field = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
                Rect rect_next = new Rect(rect_field.x, rect_field.y, rect_field.width * 0.69f, rect_field.height);
                string stage = _stageName.stringValue == "" ? "No Stage Selected" : _stageName.stringValue;

                EditorGUI.LabelField(rect_next, gui_stageName.text, stage);

                rect_next.x += rect_next.width + (rect_field.width * 0.02f);
                rect_next.width = rect_field.width * 0.29f;

                if (GUI.Button(rect_next, gui_select))
                {
                    GenericMenu menu = new GenericMenu();

                    GameManager manager = AssetDatabase.LoadAssetAtPath<GameManager>("Assets/Prefabs/Managers/GameManager.prefab");
                    string[] stageNames = manager.stageNames;
                    for (int i = 0; i < stageNames.Length; i++)
                    {
                        menu.AddItem(new GUIContent(stageNames[i]), false, StageHandler, stageNames[i]);
                    }

                    if (menu.GetItemCount() < 1)
                    {
                        menu.AddDisabledItem(new GUIContent("No Stages found in"));
                        menu.AddDisabledItem(new GUIContent("GameManager prefab"));
                    }

                    menu.ShowAsContext();
                }
            }
            else if (_target.selectorType == Types.SelectorType.Fighter)
            {
                Rect rect_field = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
                Rect rect_next = new Rect(rect_field.x, rect_field.y, rect_field.width * 0.69f, rect_field.height);
                string fighterName = _fighterReference.objectReferenceValue == null ? "No Fighter Selected" : _fighterReference.objectReferenceValue.name;

                EditorGUI.LabelField(rect_next, gui_fighterReference.text, fighterName);

                rect_next.x += rect_next.width + (rect_field.width * 0.02f);
                rect_next.width = rect_field.width * 0.29f;

                if (GUI.Button(rect_next, gui_select))
                {
                    GenericMenu menu = new GenericMenu();
                    var guids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Entities/Fighters" });

                    for (int i = 0; i < guids.Length; i++)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                        string name = Path.GetFileNameWithoutExtension(path);

                        menu.AddItem(new GUIContent(name), false, FighterHandler, path);
                    }

                    if (menu.GetItemCount() < 1)
                    {
                        menu.AddDisabledItem(new GUIContent("No Fighters Found"));
                    }

                    menu.ShowAsContext();
                }

                EditorGUILayout.IntSlider(_colorNumber, 1, 8, gui_colorNumber);
            }

            if (_target.selectorType == Types.SelectorType.Confirmation)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("selectable"));
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_panelSprite, gui_panelSprite);
            if (EditorGUI.EndChangeCheck())
            {
                _targetImage.sprite = (Sprite)_panelSprite.objectReferenceValue;
            }

            EditorGUILayout.PropertyField(_previewImage, gui_previewImage);

            serializedObject.ApplyModifiedProperties();
        }

        void StageHandler(object target)
        {
            string stageName = (string)target;
            _stageName.stringValue = stageName;
            serializedObject.ApplyModifiedProperties();
        }

        void FighterHandler(object target)
        {
            string path = (string)target;
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            _fighterReference.objectReferenceValue = go;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
