using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using Assault.Techniques;
using Assault.Utility;
using System.IO;

namespace Assault.Editors
{
    [CustomEditor(typeof(Technique))]
    public class TechniqueEditor : Editor
    {
        const int MAXLANDINGLAG = 30;

        FighterController _owner;
        AnimatorOverrideController _ownerAnimator;
        Technique _target;

        AnimBool _showLinks;

        float _hitboxElementHeight = AttackDrawer.propertyHeight + (EditorGUIUtility.singleLineHeight * 2f);

        SerializedProperty owner;
        
        string _newName;

        SerializedProperty _type;
        
        SerializedProperty _animationTrigger;
        string _newTrigger;
        SerializedProperty _totalFrameCount;

        SerializedProperty _accelerateCurveX;
        SerializedProperty _accelerateCurveY;

        SerializedProperty _cancellable;
        SerializedProperty _landCancellable;

        SerializedProperty _cancelFrame;
        SerializedProperty _landingLag;
        SerializedProperty _hardLandingRegion;

        MaxFrameReorderableList _forceFrames;
        
        Joint[] _jointObjs;
        bool _foundJoints = false;
        MaxFrameReorderableList _attacks, _links;

        readonly GUIContent gui_name = new GUIContent("Name");
        readonly GUIContent gui_rename = new GUIContent("Rename");

        readonly GUIContent gui_owner = new GUIContent("Owner");
        readonly GUIContent gui_type = new GUIContent("Type");

        readonly GUIContent gui_animationClip = new GUIContent("Animation Clip");
        readonly GUIContent gui_selectAnimation = new GUIContent("Select Animation");
        readonly GUIContent gui_animationTrigger = new GUIContent("Animation Trigger");
        readonly GUIContent gui_totalFrameCount = new GUIContent("Total Frame Count");
        readonly GUIContent gui_animationLabel = new GUIContent("Be sure to create the animation state with triggers");

        readonly GUIContent gui_accelerateCurveX = new GUIContent("Accelerate X", "All values are multiplied by 1000");
        readonly GUIContent gui_accelerateCurveY = new GUIContent("Accelerate Y", "All values are multiplied by 1000");
        
        readonly GUIContent gui_cancellable = new GUIContent("General Cancellable?");
        readonly GUIContent gui_landCancellable = new GUIContent("Land Cancellable?");

        readonly GUIContent gui_cancelFrame = new GUIContent("Cancel Frame");
        readonly GUIContent gui_landingLag = new GUIContent("Landing Lag");
        readonly GUIContent gui_hardLandingRegion = new GUIContent("Hard Land Region");
        readonly GUIContent gui_landingRegion = new GUIContent("Landing Region");

        readonly GUIContent gui_forceFrames = new GUIContent("Force Frames");
        readonly GUIContent gui_attacks = new GUIContent("Hit Boxes");

        readonly GUIContent gui_links = new GUIContent("Technique Links");
        readonly GUIContent gui_noTech = new GUIContent("No Technique Found");
        readonly GUIContent gui_conditionRegion = new GUIContent("Condition Region");
        readonly GUIContent gui_createTechnique = new GUIContent("Create");
        readonly GUIContent gui_assignTechnique = new GUIContent("Assign");

        readonly GUIContent gui_showLinks = new GUIContent("Show Links?");

        readonly GUIContent gui_joint = new GUIContent("Joint Transform");
        readonly GUIContent gui_jointSelect = new GUIContent("Select Joint");

        GUIStyle guis_label;
        GUIStyle guis_subtitle;
        GUIStyle guis_rlHeader;
        GUIStyle guis_techHeader;
        GUIStyle guis_techHeaderIC;
        GUIStyle guis_noTech;

        private void OnEnable()
        {
            _target = (Technique)serializedObject.targetObject;

            owner = serializedObject.FindProperty("fighterController");
            
            if (_target.fighterController != null)
            {
                _owner = _target.fighterController;
                _ownerAnimator = new AnimatorOverrideController(_owner.GetComponent<Animator>().runtimeAnimatorController);

                _jointObjs = _owner.GetComponentsInChildren<Joint>();
                _foundJoints = true;
            }
            else
                _owner = null;

            _showLinks = new AnimBool(false);
            _showLinks.valueChanged.AddListener(Repaint);
            
            _newName = _target.name;

            _type = serializedObject.FindProperty("_type");

            _animationTrigger = serializedObject.FindProperty("animationTrigger");
            _totalFrameCount = serializedObject.FindProperty("_totalFrameCount");

            _accelerateCurveX = serializedObject.FindProperty("_accelerateCurveX");
            _accelerateCurveY = serializedObject.FindProperty("_accelerateCurveY");
            
            _cancellable = serializedObject.FindProperty("_cancellable");
            _landCancellable = serializedObject.FindProperty("_landCancellable");

            _cancelFrame = serializedObject.FindProperty("_cancelFrame");
            _landingLag = serializedObject.FindProperty("_landingLag");
            _hardLandingRegion = serializedObject.FindProperty("_hardLandingRegion");

            ForceFrames();
            
            Attacks();
            Links();

            _attacks.maxFrame = _totalFrameCount.intValue;
            _forceFrames.maxFrame = _totalFrameCount.intValue;

            guis_label = new GUIStyle { fontStyle = FontStyle.Bold };
            guis_subtitle = new GUIStyle { fontStyle = FontStyle.Italic, fontSize = 10 };
            guis_rlHeader = new GUIStyle { fontStyle = FontStyle.Bold };
            guis_techHeader = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 14 };
            guis_techHeaderIC = new GUIStyle { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 14 };
            guis_noTech = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 12 };
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
                }
            };
        }

        void Links()
        {
            _links = new MaxFrameReorderableList(serializedObject, serializedObject.FindProperty("_links"), true, true, true, true)
            {
                elementHeight = 100f,
                drawHeaderCallback = (Rect rect) =>
                {
                    rect.x += rect.width * 0.05f;
                    rect.width *= 0.95f;
                    EditorGUI.LabelField(rect, gui_links, guis_rlHeader);
                },
                drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    var element = _links.serializedProperty.GetArrayElementAtIndex(index);

                    Rect rect_next = new Rect(rect.x, rect.y + 2f, rect.width * 0.24f, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField(rect_next, new GUIContent("Condition"));

                    rect_next.x += rect.width * 0.26f;
                    rect_next.width = rect.width * 0.74f;
                    var linkCondition = element.FindPropertyRelative("linkCondition");
                    EditorGUI.PropertyField(rect_next, linkCondition, GUIContent.none);

                    switch ((Types.LinkCondition)linkCondition.enumValueIndex)
                    {
                        case Types.LinkCondition.InputCombo:
                            var inputCombo = element.FindPropertyRelative("inputCombo");

                            rect_next.x = rect.x;
                            rect_next.y += rect_next.height;
                            rect_next.width = rect.width;
                            rect_next.height = EditorGUI.GetPropertyHeight(inputCombo, GUIContent.none);

                            EditorGUI.PropertyField(rect_next, inputCombo, GUIContent.none);
                            break;
                        case Types.LinkCondition.OnHit:

                            break;
                        case Types.LinkCondition.OnLand:

                            break;
                        case Types.LinkCondition.WhenHit:

                            break;
                    }
                    
                    var region = element.FindPropertyRelative("conditionRegion");

                    rect_next.x = rect.x;
                    rect_next.y += rect_next.height;
                    rect_next.width = rect.width;
                    rect_next.height = EditorGUI.GetPropertyHeight(region, GUIContent.none) + 2f;

                    IntRangeAttribute rangeAttribute = new IntRangeAttribute(1, _links.maxFrame);
                    IntRangeDrawer rangeDrawer = new IntRangeDrawer(rangeAttribute);
                    rangeDrawer.OnGUI(rect_next, region, GUIContent.none);

                    var technique = element.FindPropertyRelative("technique");

                    rect_next.x = rect.x;
                    rect_next.y += rect_next.height;
                    rect_next.width = rect.width;
                    rect_next.height = (EditorGUIUtility.singleLineHeight * 1.5f) + 2f;

                    try
                    {
                        if ((Types.LinkCondition)linkCondition.enumValueIndex == Types.LinkCondition.InputCombo)
                        {
                            rect_next.width *= 0.49f;

                            Technique tech = (Technique)technique.objectReferenceValue;
                            EditorGUI.LabelField(rect_next, tech.name, guis_techHeaderIC);

                            rect_next.x += rect.width * 0.51f;
                            if (GUI.Button(rect_next, "Select Technique?"))
                            {
                                tech.fighterController = _owner;
                                Selection.activeObject = tech;
                            }
                        }
                        else
                        {
                            Technique tech = (Technique)technique.objectReferenceValue;
                            EditorGUI.LabelField(rect_next, tech.name, guis_techHeader);

                            rect_next.y += rect_next.height;
                            rect_next.height = EditorGUIUtility.singleLineHeight + 2f;
                            if (GUI.Button(rect_next, "Select Technique?"))
                            {
                                tech.fighterController = _owner;
                                Selection.activeObject = tech;
                            }
                        }
                    }
                    catch
                    {
                        rect_next.height = EditorGUIUtility.singleLineHeight;
                        EditorGUI.LabelField(rect_next, gui_noTech, guis_noTech);

                        rect_next.y += rect_next.height;
                        rect_next.width = rect.width * 0.49f;
                        if (GUI.Button(rect_next, gui_createTechnique))
                        {
                            AddLink(new AddData { createNew = true, increment = false, index = index });
                        }

                        rect_next.x += rect_next.width + (rect.width * 0.02f);
                        if (GUI.Button(rect_next, gui_assignTechnique))
                        {
                            GenericMenu menu = new GenericMenu();

                            string directory = "";
                            switch (_target.type)
                            {
                                case Types.TechniqueType.Grounded:
                                    directory = FighterLibrary.GetMyGroundedTechniqueDirectory(_owner.name);
                                    break;
                                case Types.TechniqueType.Aerial:
                                    directory = FighterLibrary.GetMyAerialTechniqueDirectory(_owner.name);
                                    break;
                                case Types.TechniqueType.Special:

                                    break;
                            }

                            string[] guids = AssetDatabase.FindAssets("", new[] { directory });
                            for (int i = 0; i < guids.Length; i++)
                            {
                                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                                string name = Path.GetFileNameWithoutExtension(path);

                                if (name == _target.name) continue;

                                menu.AddItem(new GUIContent(Path.GetFileNameWithoutExtension(path)),
                                    false, AddLink,
                                    new AddData { createNew = false, increment = false, index = index, path = path });
                            }

                            menu.ShowAsContext();
                        }
                    }
                },
                onAddDropdownCallback = (Rect buttonRect, ReorderableList list) =>
                {
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Create new"), false, AddLink, new AddData { createNew = true, increment = true });
                    menu.AddItem(new GUIContent("Create empty"), false, AddLink, new AddData { createNew = false, increment = true });

                    menu.ShowAsContext();
                },
                onRemoveCallback = (ReorderableList list) =>
                {
                    if (!EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this Technique?", "Yes", "No")) return;

                    int index = list.index;
                    var element = list.serializedProperty.GetArrayElementAtIndex(index);

                    Technique delTech = (Technique)element.FindPropertyRelative("technique").objectReferenceValue;

                    list.serializedProperty.DeleteArrayElementAtIndex(index);

                    if (EditorUtility.DisplayDialog("Deleted Element", "The Technique connected to the array element still exists in the project folder.", "Delete", "Don't Delete"))
                    {
                        string directory = "";

                        switch (_target.type)
                        {
                            case Types.TechniqueType.Grounded:
                                directory = FighterLibrary.GetMyGroundedTechniqueDirectory(_owner.name);
                                break;
                            case Types.TechniqueType.Aerial:
                                directory = FighterLibrary.GetMyAerialTechniqueDirectory(_owner.name);
                                break;
                            case Types.TechniqueType.Special:

                                break;
                        }

                        AssetDatabase.DeleteAsset(directory + "/" + delTech.name + ".asset");
                    }
                }
            };
        }

        struct AddData
        {
            public bool createNew;
            public bool increment;
            public int index;
            public string path;
        }

        void AddLink(object target)
        {
            var data = (AddData)target;

            if (!data.createNew && !data.increment)
            {
                Debug.Log(data.index);
                Technique newTch = AssetDatabase.LoadAssetAtPath<Technique>(data.path);
                newTch.fighterController = _owner;

                _links.serializedProperty.GetArrayElementAtIndex(data.index).FindPropertyRelative("technique").objectReferenceValue = newTch;

                serializedObject.ApplyModifiedProperties();
                return;
            }

            FighterData oldData = FighterLibrary.LoadFighterData(_owner.name);
            int newNode = oldData.nextNode++;
            SerializedProperty element = null;

            if (data.increment)
            {
                int index = _links.serializedProperty.arraySize++;
                _links.index = index;
                element = _links.serializedProperty.GetArrayElementAtIndex(index);
            }
            else
            {
                element = _links.serializedProperty.GetArrayElementAtIndex(data.index);
            }

            if (data.createNew)
            {
                Technique newAsset = CreateInstance<Technique>();
                newAsset.name = "New Technique " + newNode;
                newAsset.fighterController = _owner;

                string location = "";
                switch (_target.type)
                {
                    case Types.TechniqueType.Grounded:
                        location = FighterLibrary.GetMyGroundedTechniqueDirectory(_owner.name);
                        newAsset.type = Types.TechniqueType.Grounded;
                        break;
                    case Types.TechniqueType.Aerial:
                        location = FighterLibrary.GetMyAerialTechniqueDirectory(_owner.name);
                        newAsset.type = Types.TechniqueType.Aerial;
                        break;
                    case Types.TechniqueType.Special:

                        break;
                }

                location += "/" + newAsset.name + ".asset";
                AssetDatabase.CreateAsset(newAsset, location);
                element.FindPropertyRelative("technique").objectReferenceValue = newAsset;

                FighterLibrary.SaveFighterData(oldData);
            }
            else
            {
                element.FindPropertyRelative("technique").objectReferenceValue = null;
            }

            serializedObject.ApplyModifiedProperties();
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

            EditorGUILayout.PropertyField(_type, gui_type);
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

            EditorGUILayout.LabelField(gui_animationLabel, guis_subtitle);

            Rect rect_field = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            Rect rect_next = new Rect(rect_field.x, rect_field.y, rect_field.width * 0.5f, rect_field.height);
            EditorGUI.LabelField(rect_next, gui_totalFrameCount);
            rect_next.x += rect_next.width;
            EditorGUI.LabelField(rect_next, _target.totalFrameCount.ToString(), new GUIStyle { alignment = TextAnchor.MiddleCenter });
            
            EditorGUILayout.Space();

            _forceFrames.DoLayoutList();

            AccelerateCurveField(gui_accelerateCurveX, ref _accelerateCurveX, Color.blue);
            AccelerateCurveField(gui_accelerateCurveY, ref _accelerateCurveY, Color.red);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_cancellable, gui_cancellable);

            if (_cancellable.boolValue)
            {
                rect_field = EditorGUILayout.GetControlRect();
                rect_field.x += rect_field.width * 0.05f;
                rect_field.width *= 0.95f;

                EditorGUI.IntSlider(rect_field, _cancelFrame, 1, _forceFrames.maxFrame, gui_cancelFrame);
            }

            if ((Types.TechniqueType)_type.enumValueIndex == Types.TechniqueType.Aerial)
            {
                IntRangeAttribute rangeAttribute = new IntRangeAttribute(1, _totalFrameCount.intValue);
                IntRangeDrawer rangeDrawer = new IntRangeDrawer(rangeAttribute);
                rect_next = EditorGUILayout.GetControlRect(false, rangeDrawer.GetPropertyHeight(_hardLandingRegion, gui_hardLandingRegion));
                rangeDrawer.OnGUI(rect_next, _hardLandingRegion, gui_hardLandingRegion);
                
                EditorGUILayout.IntSlider(_landingLag, 0, MAXLANDINGLAG, gui_landingLag);
            }
            else if ((Types.TechniqueType)_type.enumValueIndex == Types.TechniqueType.Special)
            {
                EditorGUILayout.PropertyField(_landCancellable, gui_landCancellable);

                if (_landCancellable.boolValue)
                {
                    IntRangeAttribute rangeAttribute = new IntRangeAttribute(1, _totalFrameCount.intValue);
                    IntRangeDrawer rangeDrawer = new IntRangeDrawer(rangeAttribute);
                    rect_next = EditorGUILayout.GetControlRect(false, rangeDrawer.GetPropertyHeight(_hardLandingRegion, gui_hardLandingRegion));
                    rect_next.x += rect_next.width * 0.05f;
                    rect_next.width *= 0.95f;

                    rangeDrawer.OnGUI(rect_next, _hardLandingRegion, gui_landingRegion);
                }
            }

            EditorGUILayout.Space();

            _attacks.elementHeight = _attacks.serializedProperty.arraySize == 0 ? 15f : _hitboxElementHeight;
            _attacks.DoLayoutList();

            EditorGUILayout.Space();

            _links.elementHeight = _links.serializedProperty.arraySize == 0 ? 20f : 100f;
            _links.DoLayoutList();

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
            _links.maxFrame = _totalFrameCount.intValue;

            _animationTrigger.intValue = Animator.StringToHash(clip.name);

            serializedObject.ApplyModifiedProperties();
        }

        void NoClip()
        {
            _totalFrameCount.intValue = 0;

            _forceFrames.maxFrame = _totalFrameCount.intValue;
            _attacks.maxFrame = _totalFrameCount.intValue;

            _animationTrigger.intValue = 0;

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