using UnityEngine;
using UnityEditor;
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

            string prefabPath = "Assets/Entities/Fighters/" + _newCharacterName + "/" + _newCharacterName + ".prefab";

            GameObject fighter = new GameObject(_newCharacterName, typeof(FighterController), typeof(FighterDamager), typeof(FighterDamageable));
            GameObject renderer = new GameObject("Renderer", typeof(SpriteRenderer));
            renderer.transform.SetParent(fighter.transform);

           // Object @object = PrefabUtility.CreateEmptyPrefab(prefabPath);
           // PrefabUtility.ReplacePrefab(fighter, @object, ReplacePrefabOptions.ConnectToPrefab);
            PrefabUtility.SaveAsPrefabAsset(fighter, prefabPath);
            
            Utility.FighterLibrary.SaveFighterData(new Utility.FighterData(_newCharacterName, 0));

            AssetDatabase.Refresh(ImportAssetOptions.Default);
        }
    }
}
