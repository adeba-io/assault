using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Assault;
using Assault.Techniques;
using Assault.Utility;

namespace Assault.Editors
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(FighterController))]
    public class FighterControllerEditor : Editor
    {
        const string ASSETEXTENSION = ".asset";
        FighterController _target;
        SerializedProperty _inspectedTechnique;

        bool _expandManeuver;

        SerializedProperty _walkAcceleration, _maxWalkSpeed;
        SerializedProperty _runAcceleration, _maxRunSpeed;
        SerializedProperty _dashForce;

        SerializedProperty _airAcceleration, _maxAirSpeed;
        SerializedProperty _airDashForce;

        SerializedProperty _jumpForce, _airJumpForce;
        SerializedProperty _maxAirJumps, _airJumpsLeft;
        SerializedProperty _wallJumpForce;
        
        ReorderableList _standingTechniques;
        ReorderableList _aerialTechniques;
        ReorderableList _runningTechniques;
        ReorderableList _jumpingTechniques;

        readonly GUIContent gui_currentState = new GUIContent("Fighter State :");
        readonly GUIContent gui_currentTechnique = new GUIContent("Curr Technique");

        readonly GUIContent gui_acceleration = new GUIContent("Acceleration");
        readonly GUIContent gui_maxSpeed = new GUIContent("Max Speed");
        readonly GUIContent gui_force = new GUIContent("Force");

        readonly GUIContent gui_groundMovement = new GUIContent("GROUND MOVEMENT", "Values for Walking, Running, Dashing and Crawling (if applicable)");
        readonly GUIContent gui_walking = new GUIContent("WALK");
        readonly GUIContent gui_running = new GUIContent("RUN");
        readonly GUIContent gui_crawling = new GUIContent("CRAWL");
        readonly GUIContent gui_dashing = new GUIContent("DASH");

        readonly GUIContent gui_airMovement = new GUIContent("AIR MOVEMENT", "Values for Air Drifting and Air Dashing");
        readonly GUIContent gui_airDrift = new GUIContent("AIR DRIFT");
        readonly GUIContent gui_airDash = new GUIContent("AIR DASH");

        readonly GUIContent gui_jumps = new GUIContent("JUMPS", "Data on the Grounded, Aerial and Wall Jump forces");
        readonly GUIContent gui_jumpForce = new GUIContent("Grounded");
        readonly GUIContent gui_airJumpForce = new GUIContent("Aerial");
        readonly GUIContent gui_wallJumpForce = new GUIContent("Wall Jump");
        
        readonly GUIContent gui_standingTechniques = new GUIContent("Standing Techniques");
        readonly GUIContent gui_aerialTechniques = new GUIContent("Aerial Techniques");
        readonly GUIContent gui_jumpingTechniques = new GUIContent("Jumping Techniques");
        readonly GUIContent gui_runningTechniques = new GUIContent("Running Techniques");

        readonly GUIContent gui_createTechnique = new GUIContent("Create");
        readonly GUIContent gui_assignTechnique = new GUIContent("Assign");

        readonly GUIContent gui_createNewInputCombo = new GUIContent("Create New InputCombo?");
        readonly GUIContent gui_deleteCurrentTechnique = new GUIContent("Delete?");

        readonly GUIContent gui_selectTechnique = new GUIContent("Select Technique");
        readonly GUIContent gui_noCurrTechnique = new GUIContent("No Technique selected");

        GUIStyle guis_secHeader;
        GUIStyle guis_header;
        GUIStyle guis_label;
        GUIStyle guis_selectionError;
        GUIStyle guis_rlHeader;
        GUIStyle guis_techHeader;

        GUIStyle guis_noTech;

        private void OnEnable()
        {
            _target = (FighterController)serializedObject.targetObject;

            _walkAcceleration = serializedObject.FindProperty("_walkAcceleration");
            _maxWalkSpeed = serializedObject.FindProperty("_maxWalkSpeed");
            _runAcceleration = serializedObject.FindProperty("_runAcceleration");
            _maxRunSpeed = serializedObject.FindProperty("_maxRunSpeed");
            _dashForce = serializedObject.FindProperty("_dashForce");

            _airAcceleration = serializedObject.FindProperty("_airAcceleration");
            _maxAirSpeed = serializedObject.FindProperty("_maxAirSpeed");
            _airDashForce = serializedObject.FindProperty("_airDashForce");

            _jumpForce = serializedObject.FindProperty("_jumpForce");
            _airJumpForce = serializedObject.FindProperty("_airJumpForce");
            _maxAirJumps = serializedObject.FindProperty("_maxAirJumps");
            _airJumpsLeft = serializedObject.FindProperty("_airJumpsLeft");
            _wallJumpForce = serializedObject.FindProperty("_wallJumpForce");

            guis_secHeader = new GUIStyle { fontStyle = FontStyle.Bold, fontSize = 13 };
            guis_header = new GUIStyle { fontStyle = FontStyle.Italic, fontSize = 12 };
            guis_label = new GUIStyle { fontStyle = FontStyle.Bold };
            guis_selectionError = new GUIStyle { fontStyle = FontStyle.BoldAndItalic };
            guis_rlHeader = new GUIStyle { fontStyle = FontStyle.Bold };
            guis_techHeader = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 14 };

            guis_noTech = new GUIStyle { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 12 };
            
            StandingTechniques();
            //RunningTechniques();
            AerialTechniques();
            //JumpingTechniques();
        }

        void StandingTechniques()
        {
            _standingTechniques = new ReorderableList(serializedObject, serializedObject.FindProperty("_standingTechniques"), false, true, true, true)
            {
                elementHeight = 80,
                drawHeaderCallback = TechniqueHeader(gui_standingTechniques),
                drawElementCallback = TechniqueElement(RLists.StndTech),
                onAddDropdownCallback = TechniqueAddDropdown(RLists.StndTech),
                onRemoveCallback = TechniqueRemove(RLists.StndTech),
                onSelectCallback = TechniqueSelect()
            };
        }

        void RunningTechniques()
        {
            _runningTechniques = new ReorderableList(serializedObject, serializedObject.FindProperty("_runningTechniques"), false, true, true, true)
            {
                elementHeight = 80,
                drawHeaderCallback = TechniqueHeader(gui_runningTechniques),
                drawElementCallback = TechniqueElement(RLists.RunTech),
                onAddDropdownCallback = TechniqueAddDropdown(RLists.RunTech),
                onRemoveCallback = TechniqueRemove(RLists.RunTech),
                onSelectCallback = TechniqueSelect()
            };
        }

        void AerialTechniques()
        {
            _aerialTechniques = new ReorderableList(serializedObject, serializedObject.FindProperty("_aerialTechniques"), false, true, true, true)
            {
                elementHeight = 80,
                drawHeaderCallback = TechniqueHeader(gui_aerialTechniques),
                drawElementCallback = TechniqueElement(RLists.AirTech),
                onAddDropdownCallback = TechniqueAddDropdown(RLists.AirTech),
                onRemoveCallback = TechniqueRemove(RLists.AirTech),
                onSelectCallback = TechniqueSelect()
            };
        }

        void JumpingTechniques()
        {
            _jumpingTechniques = new ReorderableList(serializedObject, serializedObject.FindProperty("_jumpingTechniques"), false, true, true, true)
            {
                elementHeight = 80,
                drawHeaderCallback = TechniqueHeader(gui_jumpingTechniques),
                drawElementCallback = TechniqueElement(RLists.JumpTech),
                onAddDropdownCallback = TechniqueAddDropdown(RLists.JumpTech),
                onRemoveCallback = TechniqueRemove(RLists.JumpTech),
                onSelectCallback = TechniqueSelect()
            };
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            serializedObject.Update();

            Rect nextRect = EditorGUILayout.GetControlRect();
            Rect labelRect = new Rect(nextRect.x, nextRect.y, nextRect.width * 0.4f, nextRect.height);
            Rect varRect = new Rect(labelRect.xMax + (nextRect.width * 0.05f), labelRect.y, nextRect.width * 0.55f, nextRect.height);
            EditorGUI.LabelField(labelRect, gui_currentState, new GUIStyle { fontStyle = FontStyle.Bold });
            if (_target.currentState != Types.FighterState.NULL)
                EditorGUI.LabelField(varRect, _target.currentState.ToString());

            nextRect = EditorGUILayout.GetControlRect();
            labelRect.y = nextRect.y;
            varRect.y = nextRect.y;
            EditorGUI.LabelField(labelRect, gui_currentTechnique, new GUIStyle { fontStyle = FontStyle.Bold });
            if (_target.currentTechnique)
                EditorGUI.LabelField(varRect, _target.currentTechnique.name);

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(GUI.skin.box);

            nextRect = EditorGUILayout.GetControlRect(true, 14);
            nextRect.x += nextRect.width * 0.05f;
            nextRect.width *= 0.95f;
            EditorGUI.LabelField(nextRect, gui_groundMovement, guis_secHeader);

            AccelMaxSpeedHeaders();
            AccelMaxSpeedField(gui_walking, ref _walkAcceleration, ref _maxWalkSpeed);
            AccelMaxSpeedField(gui_running, ref _runAcceleration, ref _maxRunSpeed);

            ForceHeader();
            ForceField(gui_dashing, ref _dashForce);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);

            nextRect = EditorGUILayout.GetControlRect(true, 14);
            nextRect.x += nextRect.width * 0.05f;
            nextRect.width *= 0.95f;
            EditorGUI.LabelField(nextRect, gui_airMovement, guis_secHeader);

            AccelMaxSpeedHeaders();
            AccelMaxSpeedField(gui_airDrift, ref _airAcceleration, ref _maxAirSpeed);

            ForceHeader();
            ForceField(gui_airDash, ref _airDashForce);

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUI.skin.box);

            nextRect = EditorGUILayout.GetControlRect(true, 14);
            nextRect.x += nextRect.width * 0.05f;
            nextRect.width *= 0.95f;
            EditorGUI.LabelField(nextRect, gui_jumps, guis_secHeader);

            ForceHeader();
            //DualForceField(gui_jumpForce, ref _jumpForce, gui_airJumpForce, ref _airJumpForce);
            ForceField(gui_jumpForce, ref _jumpForce);
            ForceField(gui_airJumpForce, ref _airJumpForce);
            Vector2Field(gui_wallJumpForce, ref _wallJumpForce);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(GUI.skin.box);

            if (_inspectedTechnique != null)
            {
                if (GUILayout.Button(gui_selectTechnique))
                {
                    Selection.activeObject = _inspectedTechnique.objectReferenceValue;
                }

                EditorGUILayout.PropertyField(_inspectedTechnique, GUIContent.none);
            }
            else
            {
                nextRect = EditorGUILayout.GetControlRect();
                nextRect.x += nextRect.width * 0.1f;
                nextRect.width *= 0.9f;
                EditorGUI.LabelField(nextRect, gui_noCurrTechnique, guis_selectionError);
            }

            EditorGUILayout.EndVertical();
            
            _standingTechniques.DoLayoutList();
            /*
            EditorGUILayout.Space();

            _runningTechniques.DoLayoutList();
            */
            EditorGUILayout.Space();

            _aerialTechniques.DoLayoutList();
            /*
            EditorGUILayout.Space();

            _jumpingTechniques.DoLayoutList();
            */
            serializedObject.ApplyModifiedProperties();
        }

        #region Formatters

        void AccelMaxSpeedHeaders()
        {
            EditorGUILayout.GetControlRect(true, 3f);
            Rect nextRect = EditorGUILayout.GetControlRect();
            Rect labelRect = new Rect(nextRect.x + (nextRect.width * 0.2f), nextRect.y, nextRect.width * 0.39f, nextRect.height);
            EditorGUI.LabelField(labelRect, gui_acceleration, guis_header);
            labelRect.x += labelRect.width + (nextRect.width * 0.02f);
            EditorGUI.LabelField(labelRect, gui_maxSpeed, guis_header);
        } 

        void ForceHeader()
        {
            EditorGUILayout.GetControlRect(true, 3f);
            Rect nextRect = EditorGUILayout.GetControlRect();
            Rect labelRect = new Rect(nextRect.x + (nextRect.width * 0.4f), nextRect.y, nextRect.width * 0.6f, nextRect.height);
            EditorGUI.LabelField(labelRect, gui_force, guis_header);
        }

        void AccelMaxSpeedField(GUIContent label, ref SerializedProperty accelProp, ref SerializedProperty maxSpeedProp)
        {
            Rect fullRect = EditorGUILayout.GetControlRect();
            Rect nextRect = new Rect(fullRect.x, fullRect.y, fullRect.width * 0.19f, fullRect.height);
            EditorGUI.LabelField(nextRect, label, guis_label);
            nextRect.x += nextRect.width + (fullRect.width * 0.02f);
            nextRect.width = fullRect.width * 0.34f;
            EditorGUI.Slider(nextRect, accelProp, 1f, 10f, GUIContent.none);
            nextRect.x += nextRect.width + (fullRect.width * 0.07f);
            EditorGUI.Slider(nextRect, maxSpeedProp, 1f, 10f, GUIContent.none);
        }

        void ForceField(GUIContent label, ref SerializedProperty forceProp)
        {
            Rect fullRect = EditorGUILayout.GetControlRect();
            Rect nextRect = new Rect(fullRect.x, fullRect.y, fullRect.width * 0.24f, fullRect.height);
            EditorGUI.LabelField(nextRect, label, guis_label);
            nextRect.x += nextRect.width + (fullRect.width * 0.07f);
            nextRect.width = fullRect.width * 0.69f;
            EditorGUI.Slider(nextRect, forceProp, 1f, 10f, GUIContent.none);
        }

        void Vector2Field(GUIContent label, ref SerializedProperty vector2Prop)
        {
            Rect fullRect = EditorGUILayout.GetControlRect();
            Rect nextRectX = new Rect(fullRect.x, fullRect.y, fullRect.width * 0.19f, fullRect.height);
            EditorGUI.LabelField(nextRectX, label, guis_label);
            nextRectX.x += nextRectX.width + (fullRect.width * 0.02f);
            nextRectX.width = fullRect.width * 0.34f;
            Rect nextRectY = nextRectX;
            nextRectY.x += nextRectX.width + (fullRect.width * 0.07f);

            vector2Prop.vector2Value = new Vector2(EditorGUI.Slider(nextRectX, vector2Prop.vector2Value.x, 1f, 10f), EditorGUI.Slider(nextRectY, vector2Prop.vector2Value.y, 1f, 10f));
        }

        #endregion

        #region Technique RLs

        enum RLists { StndTech, AirTech, JumpTech, RunTech }
        struct AddData { public bool createNew; public bool increment; public int index; public RLists list; public string path; }

        ReorderableList.HeaderCallbackDelegate TechniqueHeader(GUIContent label)
        {
            return (Rect rect) =>
            {
                rect.x += rect.width * 0.05f;
                rect.width *= 0.95f;
                EditorGUI.LabelField(rect, label, guis_rlHeader);
            };
        }

        ReorderableList.ElementCallbackDelegate TechniqueElement(RLists rl)
        {
            return (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = null;
                switch (rl)
                {
                    case RLists.StndTech:
                        element = _standingTechniques.serializedProperty.GetArrayElementAtIndex(index);
                        break;
                    case RLists.AirTech:
                        element = _aerialTechniques.serializedProperty.GetArrayElementAtIndex(index);
                        break;
                    case RLists.JumpTech:
                        element = _jumpingTechniques.serializedProperty.GetArrayElementAtIndex(index);
                        break;
                    case RLists.RunTech:
                        element = _runningTechniques.serializedProperty.GetArrayElementAtIndex(index);
                        break;
                }

                if (element == null)
                {

                }
                
                var inputCombo = element.FindPropertyRelative("inputCombo");
                var technique = element.FindPropertyRelative("technique");

                Rect rect_inputCombo = new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(inputCombo, GUIContent.none));
                Rect rect_technique = new Rect(rect.x, rect_inputCombo.yMax, rect.width, rect.height - rect_inputCombo.height);

                EditorGUI.PropertyField(rect_inputCombo, inputCombo, GUIContent.none);

                try
                {
                    Technique tech = (Technique)technique.objectReferenceValue;
                    EditorGUI.LabelField(rect_technique, tech.name, guis_techHeader);
                }
                catch
                {
                    Rect noTech = new Rect(rect_technique.x, rect_technique.y, rect_technique.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField(noTech, gui_noCurrTechnique, guis_noTech);

                    Rect createTech = new Rect(rect_technique.x, noTech.yMax, rect_technique.width * 0.49f, rect_technique.height - noTech.height - (rect.height * 0.1f));
                    Rect addTech = new Rect(createTech.xMax + (rect_technique.width * 0.02f), createTech.y, createTech.width, createTech.height);
                    if (GUI.Button(createTech, gui_createTechnique))
                    {
                        AddHandler(new AddData { createNew = true, increment = false, index = index, list = rl });
                    }
                    if (GUI.Button(addTech, gui_assignTechnique))
                    {
                        GenericMenu menu = new GenericMenu();

                        string directory = "";

                        switch (rl)
                        {
                            case RLists.StndTech:
                            case RLists.RunTech:
                                directory = FighterLibrary.GetMyGroundedTechniqueDirectory(_target.name);
                                break;
                            case RLists.AirTech:
                            case RLists.JumpTech:
                                directory = FighterLibrary.GetMyAerialTechniqueDirectory(_target.name);
                                break;
                        }

                        var guids = AssetDatabase.FindAssets("", new[] { directory });
                        for (int i = 0; i < guids.Length; i++)
                        {
                            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                            menu.AddItem(new GUIContent(Path.GetFileNameWithoutExtension(path)),
                                false, AddHandler,
                                new AddData { createNew = false, increment = false, index = index, path = path, list = rl });
                        }

                        menu.ShowAsContext();
                    }
                }
            };
        }
        
        ReorderableList.AddDropdownCallbackDelegate TechniqueAddDropdown(RLists rl)
        {

            return (Rect buttonRect, ReorderableList list) =>
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Create new"), false, AddHandler, new AddData { createNew = true, increment = true, list = rl });
                menu.AddItem(new GUIContent("Create empty"), false, AddHandler, new AddData { createNew = false, increment = true, list = rl });

                menu.ShowAsContext();
            };
        }

        ReorderableList.RemoveCallbackDelegate TechniqueRemove(RLists rl)
        {
            return (ReorderableList list) =>
            {
                if (!EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete this Technique?", "Yes", "No")) return;

                int index = list.index;
                var element = list.serializedProperty.GetArrayElementAtIndex(index);

                Technique delTech = (Technique)element.FindPropertyRelative("technique").objectReferenceValue;

                if (delTech != null)
                {
                    if (delTech == (Technique)_inspectedTechnique.objectReferenceValue) _inspectedTechnique.objectReferenceValue = null;
                }

                list.serializedProperty.DeleteArrayElementAtIndex(index);

                if (EditorUtility.DisplayDialog("Deleted Element", "The Technique connected to the array element still exists in the project folder.", "Delete", "Don't Delete"))
                {
                    string directory = "";

                    switch (rl)
                    {
                        case RLists.StndTech:
                        case RLists.RunTech:
                            directory = FighterLibrary.GetMyGroundedTechniqueDirectory(_target.name);
                            break;
                        case RLists.AirTech:
                        case RLists.JumpTech:
                            directory = FighterLibrary.GetMyAerialTechniqueDirectory(_target.name);
                            break;
                    }

                    AssetDatabase.DeleteAsset(directory + "/" + delTech.name + ASSETEXTENSION);
                }
            };
        }

        ReorderableList.SelectCallbackDelegate TechniqueSelect()
        {
            return (ReorderableList list) =>
            {
                int index = list.index;
                var element = list.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("technique");

                if (element != null)
                {
                    _inspectedTechnique = list.serializedProperty.GetArrayElementAtIndex(index).FindPropertyRelative("technique");
                    Technique tech = (Technique)_inspectedTechnique.objectReferenceValue;
                    tech.fighterController = _target;
                }
            };
        }

        void AddHandler(object target)
        {
            var data = (AddData)target;

            if (!data.createNew && !data.increment)
            {
                Technique newTch = AssetDatabase.LoadAssetAtPath<Technique>(data.path);
                newTch.fighterController = _target;

                switch (data.list)
                {
                    case RLists.StndTech:
                        _standingTechniques.serializedProperty.GetArrayElementAtIndex(data.index).FindPropertyRelative("technique").objectReferenceValue = newTch;
                        break;
                    case RLists.RunTech:
                        _runningTechniques.serializedProperty.GetArrayElementAtIndex(data.index).FindPropertyRelative("technique").objectReferenceValue = newTch;
                        break;
                    case RLists.AirTech:
                        _aerialTechniques.serializedProperty.GetArrayElementAtIndex(data.index).FindPropertyRelative("technique").objectReferenceValue = newTch;
                        break;
                    case RLists.JumpTech:
                        _jumpingTechniques.serializedProperty.GetArrayElementAtIndex(data.index).FindPropertyRelative("technique").objectReferenceValue = newTch;
                        break;
                }

                serializedObject.ApplyModifiedProperties();
                return;
            }

            FighterData oldData = FighterLibrary.LoadFighterData(_target.name);
            int newNode = oldData.nextNode++;
            SerializedProperty element = null;

            switch (data.list)
            {
                case RLists.StndTech:

                    if (data.increment)
                    {
                        int index = _standingTechniques.serializedProperty.arraySize++;
                        _standingTechniques.index = index;
                        element = _standingTechniques.serializedProperty.GetArrayElementAtIndex(index);
                    }
                    else
                    {
                        element = _standingTechniques.serializedProperty.GetArrayElementAtIndex(data.index);
                    }

                    break;
                case RLists.RunTech:

                    if (data.increment)
                    {
                        int index = _runningTechniques.serializedProperty.arraySize++;
                        _runningTechniques.index = index;
                        element = _runningTechniques.serializedProperty.GetArrayElementAtIndex(index);
                    }
                    else
                    {
                        element = _runningTechniques.serializedProperty.GetArrayElementAtIndex(data.index);
                    }

                    break;
                case RLists.AirTech:

                    if (data.increment)
                    {
                        int index = _aerialTechniques.serializedProperty.arraySize++;
                        _aerialTechniques.index = index;
                        element = _aerialTechniques.serializedProperty.GetArrayElementAtIndex(index);
                    }
                    else
                    {
                        element = _aerialTechniques.serializedProperty.GetArrayElementAtIndex(data.index);
                    }

                    break;
                case RLists.JumpTech:

                    if (data.increment)
                    {
                        int index = _jumpingTechniques.serializedProperty.arraySize++;
                        _jumpingTechniques.index = index;
                        element = _jumpingTechniques.serializedProperty.GetArrayElementAtIndex(index);
                    }
                    else
                    {
                        element = _jumpingTechniques.serializedProperty.GetArrayElementAtIndex(data.index);
                    }

                    break;
                default:
                    Debug.LogError("Could not find correct ReorderableList");
                    return;
            }
            
            if (data.createNew)
            {
                Technique newAsset = CreateInstance<Technique>();
                newAsset.name = "New Technique " + newNode;
                newAsset.fighterController = _target;

                string location = "";

                switch (data.list)
                {
                    case RLists.StndTech:
                    case RLists.RunTech:
                        location = FighterLibrary.GetMyGroundedTechniqueDirectory(_target.name);
                        break;
                    case RLists.AirTech:
                    case RLists.JumpTech:
                        location = FighterLibrary.GetMyAerialTechniqueDirectory(_target.name);
                        break;
                }

                location += "/" + newAsset.name + ASSETEXTENSION;
                AssetDatabase.CreateAsset(newAsset, location);
                element.FindPropertyRelative("technique").objectReferenceValue = newAsset;

                switch (data.list)
                {
                    case RLists.StndTech:
                    case RLists.RunTech:
                        newAsset.type = Types.TechniqueType.Grounded;
                        break;
                    case RLists.AirTech:
                    case RLists.JumpTech:
                        newAsset.type = Types.TechniqueType.Aerial;
                        break;
                }
                
                FighterLibrary.SaveFighterData(oldData);
            }
            else
            {
                element.FindPropertyRelative("technique").objectReferenceValue = null;
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}
