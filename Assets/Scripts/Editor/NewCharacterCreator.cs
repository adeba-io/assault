using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Assault
{
    namespace Utilities
    {
        public class NewCharacterCreator : EditorWindow
        {
            protected string _newCharacterName;

            readonly GUIContent _nameContent = new GUIContent("New Character Name");

            [MenuItem("Assault/Create New Character...")]
            public static void Initialize()
            {
                NewCharacterCreator window = GetWindow<NewCharacterCreator>();
                window.Show();
            }

            private void OnGUI()
            {
                _newCharacterName = EditorGUILayout.TextField(_nameContent, _newCharacterName);

                if (GUILayout.Button("Create"))
                    CheckAndCreateCharacter();
            }

            void CheckAndCreateCharacter()
            {
                if (EditorApplication.isPlaying)
                {
                    EditorUtility.DisplayDialog("Editor is Playing", "Cannot create a new character while in Play Mode. Exit Play Mode first", "OK");
                    return;
                }

                string[] fighters = AssetDatabase.GetSubFolders("Assets/Entities/Fighters");

                for (int i = 0; i < fighters.Length; i++)
                {
                    if (fighters[i] == _newCharacterName)
                    {
                        EditorUtility.DisplayDialog("A Fighter with that name already exists!", "Please use a different name", "OK");
                        return;
                    }
                }

                string path = "Assets/Entities/Fighters";
                AssetDatabase.CreateFolder(path, _newCharacterName);
                path += "/" + _newCharacterName;

                AssetDatabase.CreateFolder(path, "Maneuvers");

                AssetDatabase.CreateFolder(path, "Techniques");
                string techPath = path + "/Techniques";
                AssetDatabase.CreateFolder(techPath, "Ground");
                AssetDatabase.CreateFolder(techPath, "Aerial");
                
                GameObject prefab = (GameObject)AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FighterTemplate.prefab");
                prefab.name = _newCharacterName;
                AssetDatabase.CopyAsset("Assets/Prefabs/FighterTemplate.prefab", path + "/" + _newCharacterName + ".prefab");

            }
        }
    }
}
