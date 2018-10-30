using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Assault.Techniques;

namespace Assault.Editors
{
    [CustomEditor(typeof(Technique))]
    public class TechniqueEditor : Editor
    {
        FighterController _owner;
        AnimatorOverrideController _ownerAnimator;
        Technique _target;

        float _hitboxElementHeight = AttackDrawer.propertyHeight + (EditorGUIUtility.singleLineHeight * 2f);

        SerializedProperty owner;
        
        string _newName;
        
        SerializedProperty _animationTrigger;
        string _newTrigger;
        SerializedProperty _totalFrameCount;

        SerializedProperty _accelerateCurveX;
        SerializedProperty _accelerateCurveY;

        SerializedProperty _cancellable;
        SerializedProperty _cancelRegion;

        MaxFrameReorderableList _forceFrames;

        ReorderableList _joints;
        Joint[] _jointObjs;
        bool _foundJoints = false;
        MaxFrameReorderableList _attacks, _links;

        readonly GUIContent gui_name = new GUIContent("Name");
        readonly GUIContent gui_rename = new GUIContent("Rename");

        readonly GUIContent gui_owner = new GUIContent("Owner");

        readonly GUIContent gui_animationClip = new GUIContent("Animation Clip");
        readonly GUIContent gui_selectAnimation = new GUIContent("Select Animation");
        readonly GUIContent gui_animationTrigger = new GUIContent("Animation Trigger");
        readonly GUIContent gui_totalFrameCount = new GUIContent("Total Frame Count");

        readonly GUIContent gui_accelerateCurveX = new GUIContent("Accelerate X", "All values are multiplied by 1000");
        readonly GUIContent gui_accelerateCurveY = new GUIContent("Accelerate Y", "All values are multiplied by 1000");

        readonly GUIContent gui_cancellable = new GUIContent("Cancellable?");
        readonly GUIContent gui_cancelRegion = new GUIContent("Cancel Region");

        readonly GUIContent gui_forceFrames = new GUIContent("Force Frames");
        readonly GUIContent gui_attacks = new GUIContent("Hit Boxes");
        readonly GUIContent gui_links = new GUIContent("Technique Links");

        readonly GUIContent gui_joint = new GUIContent("Joint Transform");
        readonly GUIContent gui_jointSelect = new GUIContent("Select Joint");

        GUIStyle guis_label;
        GUIStyle guis_rlHeader;

        private void OnEnable()
        {
            _target = (Technique)serializedObject.targetObject;

            owner = serializedObject.FindProperty("fighterController");
            /*
            if (owner.objectReferenceValue != null)
            {
                _ownerAnimator = new AnimatorOverrideController(((MonoBehaviour)owner.objectReferenceValue).GetComponent<Animator>().runtimeAnimatorController);
            }
            */
            if (_target.fighterController != null)
            {
                _owner = _target.fighterController;
                _ownerAnimator = new AnimatorOverrideController(_owner.GetComponent<Animator>().runtimeAnimatorController);

                _jointObjs = _owner.GetComponentsInChildren<Joint>();
                _foundJoints = true;
            }
            else
                _owner = null;
            
            _newName = _target.name;

            _animationTrigger = serializedObject.FindProperty("animationTrigger");
            _totalFrameCount = serializedObject.FindProperty("_totalFrameCount");

            _accelerateCurveX = serializedObject.FindProperty("_accelerateCurveX");
            _accelerateCurveY = serializedObject.FindProperty("_accelerateCurveY");

            _cancellable = serializedObject.FindProperty("_cancellable");
            _cancelRegion = serializedObject.FindProperty("_cancelRegion");

            ForceFrames();

           // Joints();
            Attacks();

            _attacks.maxFrame = _totalFrameCount.intValue;
            _forceFrames.maxFrame = _totalFrameCount.intValue;

            guis_label = new GUIStyle { fontStyle = FontStyle.Bold };
            guis_rlHeader = new GUIStyle { fontStyle = FontStyle.Bold };
        }

        void ForceFrames()
        {
            _forceFrames = new MaxFrameReorderableList(serializedObject, serializedObject.FindProperty("_forceFrames"), false, true, true, true)
            {
                elementHeight = 20,
                drawHeaderCallback = (Rect rect) =>
                {
                    rect.x += rect.width * 0.1f;
                    rect.width *= 0.9f;
                    EditorGUI.LabelField(rect, gui_forceFrames, guis_rlHeader);
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = _forceFrames.serializedProperty.GetArrayElementAtIndex(index);

                    VectorFramesDrawer framesDrawer = new VectorFramesDrawer();
                    framesDrawer.maxFrame = _forceFrames.maxFrame;
                    framesDrawer.OnGUI(rect, element, GUIContent.none);
                }
            };
        }

        void Attacks()
        {
            _attacks = new MaxFrameReorderableList(serializedObject, serializedObject.FindProperty("_attacks"), false, true, true, true)
            {
                elementHeight = _hitboxElementHeight,
                drawHeaderCallback = (Rect rect) =>
                {
                    rect.x += rect.width * 0.05f;
                    rect.width *= 0.95f;
                    EditorGUI.LabelField(rect, gui_attacks, guis_rlHeader);
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = _attacks.serializedProperty.GetArrayElementAtIndex(index);

                    Rect rect_attack = new Rect(rect.x, rect.y, rect.width, AttackDrawer.propertyHeight);
                    Rect rect_joint = new Rect(rect.x + (rect.width * 0.15f), rect_attack.yMax, rect.width * 0.85f, EditorGUIUtility.singleLineHeight * 2f);

                    AttackDrawer attackDrawer = new AttackDrawer(_owner);
                    attackDrawer.maxFrame = _attacks.maxFrame;
                    attackDrawer.OnGUI(rect, element, GUIContent.none);

                   // _joints.drawElementCallback(rect_joint, index, true, false);
                },
                onAddCallback = (ReorderableList list) =>
                {
                    ReorderableList.defaultBehaviours.DoAddButton(list);
                   // ReorderableList.defaultBehaviours.DoAddButton(_joints);
                },
                onRemoveCallback = (ReorderableList list) =>
                {
                    ReorderableList.defaultBehaviours.DoRemoveButton(list);
                   // ReorderableList.defaultBehaviours.DoRemoveButton(_joints);
                }
            };
        }

        void Joints()
        {
            _joints = new ReorderableList(serializedObject, serializedObject.FindProperty("_joints"))
            {
                elementHeight = EditorGUIUtility.singleLineHeight * 2f,
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    Rect rect_joint = new Rect(rect.x + (rect.width * 0.15f), rect.y, rect.width * 0.85f, EditorGUIUtility.singleLineHeight);
                    Rect rect_jointName = new Rect(rect.x, rect_joint.yMax, rect.width * 0.48f, EditorGUIUtility.singleLineHeight);
                    Rect rect_jointButton = new Rect(rect.x + (rect.width * 0.5f), rect_jointName.y, rect.width * 0.5f, rect_jointName.height);

                    string jointLabel = _joints.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue == null ? "No Joint Selected" : _joints.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue.name;

                    EditorGUI.LabelField(rect_joint, gui_joint, guis_label);
                    EditorGUI.LabelField(rect_jointButton, jointLabel);
                    if (GUI.Button(rect_jointButton, gui_jointSelect))
                    {
                        GenericMenu menu = new GenericMenu();

                        for (int i = 0; i < _jointObjs.Length; i++)
                        {
                            menu.AddItem(new GUIContent(_jointObjs[i].name), false, JointSelect, new JointData { joint = _jointObjs[i], index = index });
                        }

                        if (menu.GetItemCount() < 1) menu.AddDisabledItem(new GUIContent("no Joints Found"));

                        menu.ShowAsContext();
                    }
                }
            };
        }

        struct JointData { public Joint joint; public int index; }

        void JointSelect(object target)
        {
            JointData data = (JointData)target;

            _joints.serializedProperty.GetArrayElementAtIndex(data.index).objectReferenceValue = data.joint;
            Debug.Log(_joints.serializedProperty.GetArrayElementAtIndex(data.index).objectReferenceValue.name);

            serializedObject.ApplyModifiedProperties();
        }

        void Links()
        {
            _links = new MaxFrameReorderableList(serializedObject, serializedObject.FindProperty("_links"), true, true, true, true)
            {

            };
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            serializedObject.Update();

            Rect nameRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            Rect fieldRect = new Rect(nameRect.x, nameRect.y, nameRect.width * 0.73f, nameRect.height);
            Rect buttonRect = new Rect(fieldRect.xMax + (nameRect.width * 0.02f), fieldRect.y, nameRect.width * 0.25f, nameRect.height);

            _newName = EditorGUI.TextField(fieldRect, gui_name, _newName);
            if (GUI.Button(buttonRect, gui_rename) && _newName != "")// && _newName != _target.name
            {
                _target.name = _newName;
                string[] path = AssetDatabase.GetAssetPath(_target).Split('/');
                string newPath = "";

                // path.Length - 1 to ignore the old name
                for (int i = 0; i < path.Length - 1; i++)
                {
                    newPath += path[i] + "/";
                }

                newPath += _newName + ".asset";
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(_target), _newName);
            }
            /*
            if (owner.objectReferenceValue == null)
            {
                EditorGUI.BeginChangeCheck();
                owner.objectReferenceValue = EditorGUILayout.ObjectField(gui_owner, owner.objectReferenceValue, typeof(FighterController), true);
                if (EditorGUI.EndChangeCheck() && owner.objectReferenceValue != null)
                {
                    _ownerAnimator = _ownerAnimator = new AnimatorOverrideController(_target.fighterController.GetComponent<Animator>().runtimeAnimatorController);
                }

                serializedObject.ApplyModifiedProperties();
                return;
            }
            */
            if (!_owner)
            {
                EditorGUI.BeginChangeCheck();
                _owner = (FighterController)EditorGUILayout.ObjectField(gui_owner, _owner, typeof(FighterController), true);
                if (EditorGUI.EndChangeCheck())
                {
                    _ownerAnimator = new AnimatorOverrideController(_owner.GetComponent<Animator>().runtimeAnimatorController);
                }

                serializedObject.ApplyModifiedProperties();
                return;
            }
            
            // Animation Clip selection
            EditorGUILayout.LabelField(gui_animationClip, guis_label);

            Rect clipRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            clipRect.x += clipRect.width * 0.05f;
            clipRect.width *= 0.95f;

            Rect labelRect = new Rect(clipRect.x, clipRect.y, clipRect.width * 0.48f, clipRect.height);
            buttonRect = new Rect(labelRect.xMax + (clipRect.width * 0.04f), clipRect.y, labelRect.width, clipRect.height);

            string clipLabel = _target.animationClip == null ? "No Clip Selected" : _target.animationClip.name;

            EditorGUI.LabelField(labelRect, clipLabel);
            if (GUI.Button(buttonRect, gui_selectAnimation))
            {
                GenericMenu menu = new GenericMenu();

                for (int i = 0; i < _ownerAnimator.animationClips.Length; i++)
                {
                    if (i == 0)
                        menu.AddItem(new GUIContent("NULL"), false, NoClip);

                    AnimationClip currClip = _ownerAnimator.animationClips[i];

                    menu.AddItem(new GUIContent(currClip.name), false, ClipSelector, currClip);
                }

                if (menu.GetItemCount() < 1)
                    menu.AddDisabledItem(new GUIContent("No Animations found"), false);

                menu.ShowAsContext();
            }

            EditorGUI.BeginChangeCheck();
            _newTrigger = EditorGUILayout.TextField(gui_animationTrigger, _newTrigger, guis_label);
            if (EditorGUI.EndChangeCheck())
            {
                _animationTrigger.intValue = Animator.StringToHash(_newTrigger);
            }
                
            EditorGUILayout.LabelField(gui_totalFrameCount.text, _totalFrameCount.intValue.ToString());

            _forceFrames.DoLayoutList();

            AccelerateCurveField(gui_accelerateCurveX, ref _accelerateCurveX, Color.blue);
            AccelerateCurveField(gui_accelerateCurveY, ref _accelerateCurveY, Color.red);

            EditorGUILayout.PropertyField(_cancellable, gui_cancellable);

            if (_cancellable.boolValue)
            {
                IntRangeDrawer rangeDrawer = new IntRangeDrawer(new IntRangeAttribute(1, _totalFrameCount.intValue));
                Rect rangeRect = new Rect(EditorGUILayout.GetControlRect(false, rangeDrawer.GetPropertyHeight(null, GUIContent.none)));

                rangeDrawer.OnGUI(rangeRect, _cancelRegion, gui_cancelRegion);
            }

            _attacks.DoLayoutList();

            _attacks.elementHeight = _attacks.serializedProperty.arraySize == 0 ? 15f : _hitboxElementHeight;

            EditorGUILayout.Space();

           // _links.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        void AccelerateCurveField(GUIContent label, ref SerializedProperty accelProp, Color curveColor)
        {
            Rect accelRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight * 2.2f);
            Rect nextRect = new Rect(accelRect.x, accelRect.y, accelRect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(nextRect, label, guis_label);
            nextRect.y += EditorGUIUtility.singleLineHeight;
            nextRect.x += accelRect.width * 0.1f;
            nextRect.width = accelRect.width * 0.68f;
            EditorGUI.CurveField(nextRect, accelProp, curveColor, Rect.zero, GUIContent.none);
            nextRect.x += nextRect.width + (accelRect.width * 0.04f);
            nextRect.width = accelRect.width * 0.18f;
            
            if (GUI.Button(nextRect, "Reset?"))
            {
                accelProp.animationCurveValue = new AnimationCurve(new Keyframe(0, 0), new Keyframe(100f, 0));
            }
        }

        void ClipSelector(object target)
        {
            AnimationClip clip = (AnimationClip)target;
            _target.animationClip = clip;
            
            int clipFrameCount = (int)(clip.length * 60f);
            _totalFrameCount.intValue = clipFrameCount;

            _forceFrames.maxFrame = _totalFrameCount.intValue;
            _attacks.maxFrame = _totalFrameCount.intValue;

            serializedObject.ApplyModifiedProperties();
        }

        void NoClip()
        {
            _totalFrameCount.intValue = 0;

            _forceFrames.maxFrame = _totalFrameCount.intValue;
            _attacks.maxFrame = _totalFrameCount.intValue;

            serializedObject.ApplyModifiedProperties();
        }
    }

    public class MaxFrameReorderableList : ReorderableList
    {
        public int maxFrame { get; set; }

        public MaxFrameReorderableList(SerializedObject serializedObject, SerializedProperty property, bool draggable, bool displayHeader, bool displayAddButton, bool displayRemoveButton) :
            base(serializedObject, property, draggable, displayHeader, displayAddButton, displayRemoveButton)
        { maxFrame = 60; }
    }
}