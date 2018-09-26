using UnityEngine;
using UnityEditor;
using Assault.Maneuvers;

namespace Assault.Editors
{/*
    [CustomEditor(typeof(GroundDash))]
    public class GroundDashEditor : Editor
    {
        FighterController _ownerController;
        GroundDash _targetGDash;

        SerializedProperty _name;
        string newName;
        
        SerializedProperty _animationClip;
        SerializedProperty _totalFrameCount;
        SerializedProperty _cancelRegion;

        SerializedProperty _forceFrame;

        VectorFramesDrawer _framesDrawer;


        readonly GUIContent gui_name = new GUIContent("Name");
        readonly GUIContent gui_animationClip = new GUIContent("Animation Clip");

        readonly GUIContent gui_totalFrameCount = new GUIContent("Total Frame Count");
        readonly GUIContent gui_cancelRegion = new GUIContent("Cancel Region");

        readonly GUIContent gui_forceFrames = new GUIContent("Force Frame");

        private void OnEnable()
        {
            _targetGDash = (GroundDash)serializedObject.targetObject;
            _framesDrawer = new VectorFramesDrawer();

            _name = serializedObject.FindProperty("name");

            _animationClip = serializedObject.FindProperty("animationClip");
            _totalFrameCount = serializedObject.FindProperty("_totalFrameCount");
            _cancelRegion = serializedObject.FindProperty("_cancelRegion");

            _forceFrame = serializedObject.FindProperty("forceFrame");
        }

        void ClipSelector(object target)
        {
            AnimationClip clip = (AnimationClip)target;
            _targetGDash.animationClip = clip;

            int clipFrameCount = (int)(clip.length * 60f);
            _totalFrameCount.intValue = clipFrameCount;

            _framesDrawer.maxFrame = clipFrameCount;

            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            newName = EditorGUILayout.TextField(gui_name, _targetGDash.name);

            // Animation Clip Selection
            EditorGUILayout.LabelField(gui_animationClip);

            Rect clipRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight + 2f);
            clipRect.x += clipRect.width * 0.05f;
            clipRect.width -= clipRect.width * 0.05f;
            clipRect.height = EditorGUIUtility.singleLineHeight;

            Rect labelRect = new Rect(clipRect.x, clipRect.y, (clipRect.width * 0.5f) - clipRect.width * 0.02f, clipRect.height);
            Rect buttonRect = new Rect(clipRect.x + (clipRect.width * 0.5f), clipRect.y, labelRect.width, clipRect.height);

            string clipLabel = "No Clip Selected";
            if (_targetGDash.animationClip != null)
                clipLabel = _targetGDash.animationClip.name;

            EditorGUI.LabelField(labelRect, clipLabel);
            if (GUI.Button(buttonRect, "Select Animation"))
            {
                if (_ownerController != null)
                {
                    GenericMenu menu = new GenericMenu();

                    AnimatorOverrideController animator = new AnimatorOverrideController(_ownerController.animator.runtimeAnimatorController);

                    for (int i = 0; i < animator.animationClips.Length; i++)
                    {
                        AnimationClip currClip = animator.animationClips[i];

                        menu.AddItem(new GUIContent(currClip.name), false, ClipSelector, currClip);
                    }

                    menu.ShowAsContext();
                }
            }

            EditorGUILayout.PropertyField(_totalFrameCount, gui_totalFrameCount);

            IntRangeDrawer rangeDrawer = new IntRangeDrawer(new IntRangeAttribute(1, _totalFrameCount.intValue));
            Rect rangeRect = new Rect(EditorGUILayout.GetControlRect(false, rangeDrawer.GetPropertyHeight(null, GUIContent.none)));
            rangeDrawer.OnGUI(rangeRect, _cancelRegion, gui_cancelRegion);

            Rect frameRect = new Rect(EditorGUILayout.GetControlRect(false, _framesDrawer.GetPropertyHeight(null, GUIContent.none)));
            _framesDrawer.OnGUI(frameRect, _forceFrame, gui_forceFrames);

            if (newName != _targetGDash.name)
            {
                //_targetManeuver.name = newName;
                _name.stringValue = newName;
                string[] path = AssetDatabase.GetAssetPath(_targetGDash).Split('/');
                string newPath = "";

                for (int i = 0; i < path.Length - 1; i++)
                {
                    newPath += path[i] + "/";
                }

                newPath += newName + ".asset";
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(_targetGDash), newName);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }*/
}