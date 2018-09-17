using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Assault.Maneuvers;

namespace Assault.Editors
{
    [CustomEditor(typeof(Maneuver))]
    public class ManeuverEditor : Editor
    {
        Maneuver _targetManeuver;

        SerializedProperty _name;
        string newName;
        SerializedProperty _toSet;

        SerializedProperty _accelerateCurveX;
        SerializedProperty _accelerateCurveY;

        SerializedProperty _totalFrameCount;

        SerializedProperty _cancelRegion;

        VectorFramesReorderableList _moveFrames;
        VectorFramesReorderableList _forceFrames;

        readonly GUIContent gui_name = new GUIContent("Name");
        readonly GUIContent gui_toSet = new GUIContent("State to Set to");

        readonly GUIContent gui_accelerateCurveX = new GUIContent("Acceleration Curve X");
        readonly GUIContent gui_accelerateCurveY = new GUIContent("Acceleration Curve Y");

        readonly GUIContent gui_totalFrameCount = new GUIContent("Total Frame Count");
        readonly GUIContent gui_cancelRegion = new GUIContent("Cancel Region");

        readonly GUIContent gui_moveFrames = new GUIContent("Move Frames");
        readonly GUIContent gui_forceFrames = new GUIContent("Force Frames");

        #region OnEnable

        private void OnEnable()
        {
            _targetManeuver = (Maneuver)serializedObject.targetObject;

            _name = serializedObject.FindProperty("name");
            _toSet = serializedObject.FindProperty("_toSet");

            _accelerateCurveX = serializedObject.FindProperty("_accelerateCurveX");
            _accelerateCurveY = serializedObject.FindProperty("_accelerateCurveY");

            _totalFrameCount = serializedObject.FindProperty("_totalFrameCount");

            _cancelRegion = serializedObject.FindProperty("_cancelRegion");

            MoveFrames();
            ForceFrames();
        }

        void MoveFrames()
        {
            _moveFrames = new VectorFramesReorderableList(serializedObject, serializedObject.FindProperty("_moveFrames"), false, true, true, true)
            {
                elementHeight = 20,

                drawHeaderCallback = (Rect rect) =>
                {
                    rect.x += 40f;
                    EditorGUI.LabelField(rect, new GUIContent("Move Frames"), new GUIStyle { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft });
                },

                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = _moveFrames.serializedProperty.GetArrayElementAtIndex(index);
                    
                    VectorFramesDrawer framesDrawer = new VectorFramesDrawer();
                    framesDrawer.maxFrame = _moveFrames.maxFrame;

                    framesDrawer.OnGUI(rect, element, GUIContent.none);

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
                            }
                        }
                    }*/
                }
            };
        }

        void ForceFrames()
        {
            _forceFrames = new VectorFramesReorderableList(serializedObject, serializedObject.FindProperty("_forceFrames"), false, true, true, true)
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

                    VectorFramesDrawer framesDrawer = new VectorFramesDrawer();
                    framesDrawer.maxFrame = _forceFrames.maxFrame;

                    framesDrawer.OnGUI(rect, element, GUIContent.none);

                    // OrganizeReorderableList(_forceFrames);
                }
            };
        }

        #endregion

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            serializedObject.Update();
            newName = EditorGUILayout.TextField(gui_name, _targetManeuver.name);
           // EditorGUILayout.PropertyField(_name, gui_name);
            EditorGUILayout.PropertyField(_toSet, gui_toSet);

            EditorGUILayout.PropertyField(_accelerateCurveX, gui_accelerateCurveX);
            EditorGUILayout.PropertyField(_accelerateCurveY, gui_accelerateCurveY);

            EditorGUILayout.PropertyField(_totalFrameCount, gui_totalFrameCount);

            IntRangeDrawer rangeDrawer = new IntRangeDrawer(new IntRangeAttribute(0, _totalFrameCount.intValue));
            Rect rangeRect = new Rect(EditorGUILayout.GetControlRect(false, rangeDrawer.GetPropertyHeight(null, GUIContent.none)));

            rangeDrawer.OnGUI(rangeRect, _cancelRegion, gui_cancelRegion);

            EditorGUILayout.Space();

            _moveFrames.maxFrame = _totalFrameCount.intValue;
            _moveFrames.DoLayoutList();

            EditorGUILayout.Space();

            _forceFrames.maxFrame = _totalFrameCount.intValue;
            _forceFrames.DoLayoutList();
            /*
            if (Event.current.isKey)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        _name.stringValue = 
                }
            }
            */
            
            if (newName != _targetManeuver.name)
            {
                //_targetManeuver.name = newName;
                _name.stringValue = newName;
                string[] path = AssetDatabase.GetAssetPath(_targetManeuver).Split('/');
                string newPath = "";

                for (int i = 0; i < path.Length - 1; i++)
                {
                    newPath += path[i] + "/";
                }

                newPath += newName + ".asset";
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(_targetManeuver), newName);
            }
            

            serializedObject.ApplyModifiedProperties();
        }
    }

    public class VectorFramesReorderableList : ReorderableList
    {
        public int maxFrame { get; set; }

        public VectorFramesReorderableList(SerializedObject serializedObject, SerializedProperty property, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton) :
            base(serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton)
        { maxFrame = 60; }
    }
}
