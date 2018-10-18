
using UnityEngine;
using UnityEditor;


namespace Assault.Editors
{
    [CustomEditor(typeof(FighterInput))]
    public class FighterInputEditor : Editor
    {
        FighterInput fighterInput;
        InputCombo _inputCombo;

        private void OnEnable()
        {
            fighterInput = (FighterInput)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("toFeed"), GUIContent.none);

            if (GUILayout.Button("Feed Input"))
            {
                fighterInput.FeedDefined(fighterInput.toFeed);
            }
        }
    }
}