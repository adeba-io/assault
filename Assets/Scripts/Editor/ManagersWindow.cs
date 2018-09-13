using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Assault
{
    // Window to alter the values of:
    // GameManager, PhysicsManager, InputComponent
    public class ManagersWindow : EditorWindow
    {
        static ManagersWindow window;

        readonly GUIContent gui_gravity = new GUIContent("Gravity");
        readonly GUIContent gui_airFriction = new GUIContent("Air Friction");

        [MenuItem("Assault/Physics")]
        public static void Initialize()
        {
            // Find and show the existing editor window, and if none create one
            window = GetWindow<ManagersWindow>("Physics");

            window.minSize = new Vector2(400f, 600f);
            window.Show();
        }

        private void OnGUI()
        {
            PhysicsManager.gravity = EditorGUILayout.Vector2Field(gui_gravity, PhysicsManager.gravity);
            PhysicsManager.externalFriction = EditorGUILayout.Vector2Field(gui_airFriction, PhysicsManager.externalFriction);
        }
    }
}
