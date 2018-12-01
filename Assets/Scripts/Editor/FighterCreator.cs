using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

namespace Assault.Editors
{
    public class FighterCreatorWindow : EditorWindow
    {
        static FighterCreatorWindow s_window = null;

        protected string _newCharacterName;
        readonly GUIContent _nameContent = new GUIContent("New Character Name : ");

        [MenuItem("Assault/Create New Fighter")]
        static void Initialize()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Editor is Playing", "Cannot create a new fighter while in Play Mode", "OK");
                return;
            }

            s_window = GetWindow<FighterCreatorWindow>();
            s_window.titleContent = new GUIContent("Create New Fighter");
            s_window.maxSize = new Vector2(400f, 100f);
            s_window.Show();
        }

        private void OnGUI()
        {
            _newCharacterName = EditorGUILayout.TextField(_nameContent, _newCharacterName);

            if (GUILayout.Button("Create"))
            {
                CheckAndCreateCharacter();
            }
        }

        void CheckAndCreateCharacter()
        {
            // Check to see if another fighter with the same name already exists
            
            string path = Directory.GetCurrentDirectory() + Utility.FighterLibrary.FIGHTERDIRECTORY + _newCharacterName;
            var folder = Directory.CreateDirectory(path);

            string dataPath = path + "\\Editor";
            string animationPath = path + "\\Animations", 
                spritesPath = path + "\\Sprites",
                techniquePath = path + "\\Techniques";
            string groundedTechPath = techniquePath + "\\Grounded",
                aerialTechPath = techniquePath + "\\Aerial",
                specialTechPath = techniquePath + "\\Specials";

            folder.CreateSubdirectory(dataPath);
            folder.CreateSubdirectory(animationPath);
            folder.CreateSubdirectory(spritesPath);
            folder.CreateSubdirectory(groundedTechPath);
            folder.CreateSubdirectory(aerialTechPath);
            folder.CreateSubdirectory(specialTechPath);

            string unityPath = "Assets/Entities/Fighters/" + _newCharacterName + "/";
            string prefabPath = unityPath + _newCharacterName + ".prefab";

            GameObject fighter = new GameObject(_newCharacterName, typeof(FighterController), typeof(FighterDamager), typeof(FighterDamageable));
            GameObject model = new GameObject("Model");
            model.transform.SetParent(fighter.transform);
            GameObject renderer = new GameObject("Renderer", typeof(SpriteRenderer));
            renderer.transform.SetParent(model.transform);
            
            PrefabUtility.SaveAsPrefabAsset(fighter, prefabPath);
            
            Utility.FighterLibrary.SaveFighterData(new Utility.FighterData(_newCharacterName, 0));

            // Create the animator controller
            var controller = CreateController(unityPath + _newCharacterName + ".controller");

            fighter.GetComponent<Animator>().runtimeAnimatorController = controller;

            AssetDatabase.Refresh(ImportAssetOptions.Default);
        }

        readonly string para_jumpsquat = "JumpSquat";
        readonly string para_jump = "Jump";
        readonly string para_dash = "Dash";
        readonly string para_backDash = "Back Dash";
        readonly string para_turnaround = "Turnaround";
        readonly string para_wallJump = "WallJump";
        readonly string para_hardland = "HardLand";
        readonly string para_noLandAnim = "NoLandAnim";
        readonly string para_spawn = "Spawn";
        readonly string para_cancel = "Cancel";

        readonly string para_isGrounded = "isGrounded";
        readonly string para_crouching = "Crouching";
        readonly string para_skidding = "Skidding";

        readonly string para_groundMove = "GroundMove";

        readonly string para_hardLandModifier = "HardLandModifier";

        AnimatorController CreateController(string path)
        {
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            // Add Parameters
            controller.AddParameter(para_jumpsquat, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(para_jump, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(para_dash, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(para_backDash, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(para_turnaround, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(para_wallJump, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(para_hardland, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(para_noLandAnim, AnimatorControllerParameterType.Trigger);

            controller.AddParameter(para_spawn, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(para_cancel, AnimatorControllerParameterType.Trigger);


            controller.AddParameter(para_isGrounded, AnimatorControllerParameterType.Bool);
            controller.AddParameter(para_crouching, AnimatorControllerParameterType.Bool);
            controller.AddParameter(para_skidding, AnimatorControllerParameterType.Bool);


            controller.AddParameter(para_groundMove, AnimatorControllerParameterType.Int);


            controller.AddParameter(para_hardLandModifier, AnimatorControllerParameterType.Float);


            // Add StateMachines
            var rootStateMachine = controller.layers[0].stateMachine;
            rootStateMachine.anyStatePosition = Vector3.down * 50f;
            rootStateMachine.entryPosition = Vector3.zero;
            rootStateMachine.exitPosition = Vector3.up * 100f;

            var aerialLocomo = rootStateMachine.AddStateMachine("Aerial Locomotion", new Vector3(2f, -1f) * 100f);
            var groundedLocomo = rootStateMachine.AddStateMachine("Grounded Locomotion", new Vector3(2f, 1f) * 100f);
            

            // Add States
            groundedLocomo.anyStatePosition = Vector3.down * 50f;
            groundedLocomo.entryPosition = Vector3.zero;
            groundedLocomo.exitPosition = Vector3.up * 100f;
            groundedLocomo.parentStateMachinePosition = new Vector3(-0.2f, -3f) * 100f;
            
            var state_softland = groundedLocomo.AddState("Soft Landing", new Vector3(0.4f, -0.2f) * 500f);
            var state_hardland = groundedLocomo.AddState("Hard Landing", new Vector3(0.4f, 0.2f) * 500f);
            state_hardland.speedParameter = para_hardLandModifier;

            var state_standing = groundedLocomo.AddState("Standing", new Vector3(1.2f, 0) * 500f);
            var state_crouching = groundedLocomo.AddState("Crouching", new Vector3(1.2f, -0.5f) * 500f);

            var state_dash = groundedLocomo.AddState("Dash", new Vector3(1.5f, 0.5f) * 500f);
            var state_backDash = groundedLocomo.AddState("Back Dash", new Vector3(0.9f, 0.5f) * 500f);
            var state_backDashTurnaround = groundedLocomo.AddState("Back Dash Turnaround", new Vector3(0.3f, 0.5f) * 500f);

            var state_walking = groundedLocomo.AddState("Walking", new Vector3(2f, -0.5f) * 500f);
            var state_walkTurnaround = groundedLocomo.AddState("Walk Turnaround", new Vector3(2.5f, -0.5f) * 500f);

            var state_running = groundedLocomo.AddState("Running", new Vector3(2f, 0.7f) * 500f);
            var state_skidding = groundedLocomo.AddState("Skidding", new Vector3(2.6f, 0.2f) * 500f);

            var state_jumpsquat = groundedLocomo.AddState("Jump Squat", new Vector3(2f, 0) * 500f);


            aerialLocomo.anyStatePosition = Vector3.down * 50f;
            aerialLocomo.entryPosition = Vector3.zero;
            aerialLocomo.exitPosition = Vector3.up * 100f;
            aerialLocomo.parentStateMachinePosition = new Vector3(-0.2f, -3f) * 100f;

            var state_aerial = aerialLocomo.AddState("Aerial", new Vector3(0.4f, -0.2f) * 500f);
            var state_airDash = aerialLocomo.AddState("Air Dash", new Vector3(1f, -0.5f) * 500f);
            var state_airBackDash = aerialLocomo.AddState("Air Back Dash", new Vector3(1.7f, -0.5f) * 500f);

            var state_jumping = aerialLocomo.AddState("Jumping", new Vector3(0.4f, 0.2f) * 500f);
            var state_airJumpsquat = aerialLocomo.AddState("Air Jump Squat", new Vector3(1f, 0.5f) * 500f);
            var state_airJump = aerialLocomo.AddState("Air Jumping", new Vector3(1.7f, 0.5f) * 500f);
            var state_wallJumpsquat = aerialLocomo.AddState("Wall Jump Squat", new Vector3(2.4f, -0.2f) * 500f);
            var state_wallJump = aerialLocomo.AddState("Wall Jumping", new Vector3(2.4f, 0.2f) * 500f);


            // Add Transitions
            AnimatorStateTransition transition;

            #region Grounded Locomotion
            groundedLocomo.AddEntryTransition(state_softland);
            groundedLocomo.AddEntryTransition(state_hardland).AddCondition(AnimatorConditionMode.If, 0, para_hardland);
            groundedLocomo.AddEntryTransition(state_standing).AddCondition(AnimatorConditionMode.If, 0, para_noLandAnim);

            //rootStateMachine.AddStateMachineTransition(groundedLocomo, aerialLocomo).AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            #endregion

            #region Standing
            transition = state_standing.AddTransition(state_crouching); // Standing to Crouching
            transition.AddCondition(AnimatorConditionMode.If, 0, para_crouching);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_standing.AddTransition(state_dash); // Standing to Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_dash);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_standing.AddTransition(state_backDash); // Standing to Back Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_backDash);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_standing.AddTransition(state_walking); // Standing to Walking
            transition.AddCondition(AnimatorConditionMode.Equals, 1f, para_groundMove);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_standing.AddTransition(state_jumpsquat); // Standing to Jump Squat
            transition.AddCondition(AnimatorConditionMode.If, 0, para_jumpsquat);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_standing.AddTransition(aerialLocomo); // Standing to Aerial Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;
            #endregion

            #region Crouching
            transition = state_crouching.AddTransition(state_standing); // Crouching to Standing
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_crouching);
            transition.hasExitTime = false;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_crouching.AddTransition(state_jumpsquat); // Crouching to Jump Squat
            transition.AddCondition(AnimatorConditionMode.If, 0, para_jumpsquat);
            transition.hasExitTime = false;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_crouching.AddTransition(aerialLocomo); // Crouhing to Aerial Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;
            #endregion

            #region Dash
            transition = state_dash.AddTransition(state_dash); // Dash to Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_dash);
            transition.hasExitTime = true;
            transition.exitTime = 0.6f; transition.duration = 0;

            transition = state_dash.AddTransition(state_backDash); // Dash to Back Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_backDash);
            transition.hasExitTime = true;
            transition.exitTime = 0.5f; transition.duration = 0;

            transition = state_dash.AddTransition(state_jumpsquat); // Dash to Jump Squat
            transition.AddCondition(AnimatorConditionMode.If, 0, para_jumpsquat);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_dash.AddTransition(state_running); // Dash to Running
            transition.AddCondition(AnimatorConditionMode.Equals, 2f, para_groundMove);
            transition.hasExitTime = true;
            transition.exitTime = 0.8f; transition.duration = 0;

            transition = state_dash.AddTransition(state_standing); // Dash to Standing
            transition.hasExitTime = true;
            transition.exitTime = transition.duration = 0;

            transition = state_dash.AddTransition(aerialLocomo); // Dash to Aerial Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;
            #endregion

            #region Back Dash
            transition = state_backDash.AddTransition(state_dash); // Back Dash to Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_dash);
            transition.hasExitTime = true;
            transition.exitTime = 0.4f; transition.duration = 0;

            transition = state_backDash.AddTransition(state_jumpsquat); // Back Dash to Jump Squat
            transition.AddCondition(AnimatorConditionMode.If, 0, para_jumpsquat);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_backDash.AddTransition(state_backDashTurnaround); // Back Dash to Back Dash Turnaround -> Running
            transition.AddCondition(AnimatorConditionMode.Equals, 2f, para_groundMove);
            transition.hasExitTime = true;
            transition.exitTime = 0.5f; transition.duration = 0;

            transition = state_backDash.AddTransition(state_standing); // Back Dash to Standing
            transition.hasExitTime = true;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_backDash.AddTransition(aerialLocomo); // Back Dash to Aerial Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_backDashTurnaround.AddTransition(state_running); // Back Dash Turnaround to Running
            transition.hasExitTime = true;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_backDashTurnaround.AddTransition(aerialLocomo); // Back Dash Turnaround to Aerial Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;
            #endregion

            #region Walking
            transition = state_walking.AddTransition(state_dash); // Walking to Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_dash);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_walking.AddTransition(state_backDash); // Walking to Back Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_backDash);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_walking.AddTransition(state_walkTurnaround); // Walking to Walk Turnaround
            transition.AddCondition(AnimatorConditionMode.If, 0, para_turnaround);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_walking.AddTransition(state_jumpsquat); // Walking to JumpSquat
            transition.AddCondition(AnimatorConditionMode.If, 0, para_jumpsquat);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_walking.AddTransition(state_crouching); // Walking to Crouch
            transition.AddCondition(AnimatorConditionMode.If, 0, para_crouching);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_walking.AddTransition(state_standing); // Walking to Standing
            transition.AddCondition(AnimatorConditionMode.Equals, 0f, para_groundMove);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_walking.AddTransition(aerialLocomo); // Walking to Aerial Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_walkTurnaround.AddTransition(state_walking); // Walk Turnaround to Walking
            transition.hasExitTime = true;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_walkTurnaround.AddTransition(aerialLocomo); // Walk Turnaround to Aerial Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;
            #endregion

            #region Running
            transition = state_running.AddTransition(state_backDash); // Running to Back Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_backDash);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_running.AddTransition(state_jumpsquat); // Running to Jump Squat
            transition.AddCondition(AnimatorConditionMode.If, 0, para_jumpsquat);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_running.AddTransition(state_skidding); // Running to Skidding
            transition.AddCondition(AnimatorConditionMode.If, 0, para_skidding);
            transition.AddCondition(AnimatorConditionMode.Equals, 0, para_groundMove);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_running.AddTransition(aerialLocomo); // Running to Aerial Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;
            #endregion

            #region Skidding
            transition = state_skidding.AddTransition(state_standing); // Skidding to Standing
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_skidding);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_skidding.AddTransition(state_jumpsquat); // Skidding to Jump Squat
            transition.AddCondition(AnimatorConditionMode.If, 0, para_jumpsquat);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_skidding.AddTransition(state_dash); // Skidding to Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_dash);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_skidding.AddTransition(state_backDash); // Skidding to Back Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_backDash);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_skidding.AddTransition(aerialLocomo); // Skidding to Aerial Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;
            #endregion

            #region Jump Squat
            transition = state_jumpsquat.AddTransition(aerialLocomo); // Jump Squat to Aerial Locomotion -> Jumping
            transition.hasExitTime = true;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_jumpsquat.AddTransition(aerialLocomo); // Jump Squat to Aerial Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;
            #endregion

            #region Landing
            transition = state_softland.AddTransition(state_standing); // Soft Land to Standing
            transition.hasExitTime = true;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_softland.AddTransition(aerialLocomo); // Soft Land to Aerial Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_hardland.AddTransition(state_standing); // Hard Land to Standing
            transition.hasExitTime = true;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_hardland.AddTransition(aerialLocomo); // Hard Land to Aerial Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;
            #endregion


            #region Aerial Locomotion
            aerialLocomo.AddEntryTransition(state_aerial);
            aerialLocomo.AddEntryTransition(state_jumping).AddCondition(AnimatorConditionMode.If, 0, para_jump);

            //rootStateMachine.AddStateMachineTransition(aerialLocomo, groundedLocomo).AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            #endregion

            #region Aerial
            transition = state_aerial.AddTransition(state_airJumpsquat); // Aerial to Air Jump Squat
            transition.AddCondition(AnimatorConditionMode.If, 0, para_jumpsquat);
            transition.hasExitTime = false;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_aerial.AddTransition(state_airDash); // Aerial to Air Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_dash);
            transition.hasExitTime = false;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_aerial.AddTransition(state_airBackDash); // Aerial to Air Back Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_backDash);
            transition.hasExitTime = false;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_aerial.AddTransition(state_wallJumpsquat); // Aerial to Wall Jump Squat
            transition.AddCondition(AnimatorConditionMode.If, 0, para_wallJump);
            transition.hasExitTime = false;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_aerial.AddTransition(groundedLocomo); // Aerial to Grounded Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;
            #endregion

            #region Jumping
            transition = state_jumping.AddTransition(state_aerial); // Jumping to Aerial
            transition.hasExitTime = true;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_jumping.AddTransition(state_airJumpsquat); // Jumping to Air Jump Squat
            transition.AddCondition(AnimatorConditionMode.If, 0, para_jumpsquat);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_jumping.AddTransition(state_wallJumpsquat); // Jumping to Wall Jump Squat
            transition.AddCondition(AnimatorConditionMode.If, 0, para_wallJump);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_jumping.AddTransition(state_airDash); // Jumping to Air Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_dash);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_jumping.AddTransition(state_airBackDash); // Jumping to Air Back Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_backDash);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_jumping.AddTransition(groundedLocomo); // Jumping to Grounded Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;
            #endregion

            #region Air Dash
            transition = state_airDash.AddTransition(state_aerial); // Air Dash to Aerial
            transition.hasExitTime = true;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_airDash.AddTransition(groundedLocomo); // Air Dash to Grounded Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_airBackDash.AddTransition(state_aerial); // Air Back Dash to Aerial
            transition.hasExitTime = true;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_airBackDash.AddTransition(groundedLocomo); // Air Back Dash to Grounded Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;
            #endregion

            #region Air Jump Squats
            transition = state_airJumpsquat.AddTransition(state_airJump); // Air Jump Squat to Air Jumping
            transition.hasExitTime = true;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_airJumpsquat.AddTransition(groundedLocomo); // Air Jump Squat to Grounded Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_wallJumpsquat.AddTransition(state_wallJump); // Wall Jump Squat to Wall Jumping
            transition.hasExitTime = true;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_wallJumpsquat.AddTransition(groundedLocomo); // Wall Jump Squat to Grounded Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;
            #endregion

            #region Air Jumping
            transition = state_airJump.AddTransition(state_aerial); // Air Jumping to Aerial
            transition.hasExitTime = true;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_airJump.AddTransition(state_airJumpsquat); // Air Jumping to Air Jump Squat
            transition.AddCondition(AnimatorConditionMode.If, 0, para_jumpsquat);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_airJump.AddTransition(state_wallJumpsquat); // Air Jumping to Wall Jump Squat
            transition.AddCondition(AnimatorConditionMode.If, 0, para_wallJump);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_airJump.AddTransition(state_airDash); // Air Jumping to Air Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_dash);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_airJump.AddTransition(state_airBackDash); // Air Jumping to Air Back Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_backDash);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_aerial.AddTransition(groundedLocomo); // Air Jumping to Grounded Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;
            #endregion

            #region Wall Jumping
            transition = state_wallJump.AddTransition(state_aerial); // Wall Jumping to Aerial
            transition.hasExitTime = true;
            transition.exitTime = 1f; transition.duration = 0;

            transition = state_wallJump.AddTransition(state_airJumpsquat); // Wall Jumping to Air Jump Squat
            transition.AddCondition(AnimatorConditionMode.If, 0, para_jumpsquat);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_wallJump.AddTransition(state_wallJumpsquat); // Wall Jumping to Wall Jump Squat
            transition.AddCondition(AnimatorConditionMode.If, 0, para_wallJump);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_wallJump.AddTransition(state_airDash); // Wall Jumping to Air Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_dash);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_wallJump.AddTransition(state_airBackDash); // Wall Jumping to Air Back Dash
            transition.AddCondition(AnimatorConditionMode.If, 0, para_backDash);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;

            transition = state_wallJump.AddTransition(groundedLocomo); // Wall Jumping to Grounded Locomotion
            transition.AddCondition(AnimatorConditionMode.IfNot, 0, para_isGrounded);
            transition.hasExitTime = false;
            transition.exitTime = transition.duration = 0;
            #endregion

            return controller;
        }
    }
}
