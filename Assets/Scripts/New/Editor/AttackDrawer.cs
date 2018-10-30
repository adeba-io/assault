using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Assault.Editors
{
    [CustomPropertyDrawer(typeof(Techniques.Attack))]
    public class AttackDrawer : PropertyDrawer
    {
        public int maxFrame = 60;
        float _buffer = 2f;
        int _ownerID;

        bool _foundJoints = false;
        static bool _jointChange = false;
        Joint _newJoint;
        Joint[] _joints;

        static Dictionary<int, Joint> s_joints = new Dictionary<int, Joint>();

        Rect rect_controlName;

        Rect rect_id;
        Rect rect_priority;
        Rect rect_enableRegion, rect_damage;

        Rect rect_knockbackLabel;
        Rect rect_kbBaseLabel, rect_kbBase;
        Rect rect_kbGrowthLabel, rect_kbGrowth;
        Rect rect_lchAngleLabel, rect_lchAngle;

        Rect rect_hitstunType;
        Rect rect_jointLabel;
        Rect rect_jointName;
        Rect rect_jointSelect;
        Rect rect_hitbox;

        readonly GUIContent gui_id = new GUIContent("ID");
        readonly GUIContent gui_priority = new GUIContent("Priority");
        readonly GUIContent gui_enableRegion = new GUIContent("Enable Region");
        readonly GUIContent gui_damage = new GUIContent("Damage");

        readonly GUIContent gui_knockback = new GUIContent("KNOCKBACK");
        readonly GUIContent gui_kbBase = new GUIContent("Base");
        readonly GUIContent gui_kbGrowth = new GUIContent("Growth");

        readonly GUIContent gui_launch = new GUIContent("LAUNCH");
        readonly GUIContent gui_lchAngle = new GUIContent("Angle");
        readonly GUIContent gui_lchSpeed = new GUIContent("Speed");

        readonly GUIContent gui_hitstunType = new GUIContent("Hit Stun Type");
        readonly GUIContent gui_joint = new GUIContent("Joint Transform");
        readonly GUIContent gui_jointSelect = new GUIContent("Select Joint");

        readonly GUIContent gui_hitbox = new GUIContent("Hit Box");

        public static float propertyHeight
        { get { return new IntRangeDrawer().GetPropertyHeight(null, GUIContent.none) + (EditorGUIUtility.singleLineHeight * 8) + IBoxDataDrawer.propertyHeight; } }

        public AttackDrawer() { }

        public AttackDrawer(FighterController controller)
        {
            _ownerID = controller.GetInstanceID();
            _joints = controller.GetComponentsInChildren<Joint>();
            _foundJoints = true;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float height = 0;

            if (label != GUIContent.none) height += EditorGUIUtility.singleLineHeight;

            height += propertyHeight;

            if (_foundJoints) height += EditorGUIUtility.singleLineHeight;

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SetRects(position, label != GUIContent.none);

            EditorGUI.BeginProperty(position, label, property);

            if (label != GUIContent.none)
            {
                EditorGUI.LabelField(rect_controlName, label, new GUIStyle { fontStyle = FontStyle.Bold });
            }

            EditorGUI.PropertyField(rect_id, property.FindPropertyRelative("ID"), gui_id);
            EditorGUI.IntSlider(rect_priority, property.FindPropertyRelative("priority"), 1, 3, gui_priority);
            IntRangeDrawer rangeDrawer = new IntRangeDrawer(new IntRangeAttribute(1, maxFrame));
            rangeDrawer.OnGUI(rect_enableRegion, property.FindPropertyRelative("enableRegion"), gui_enableRegion);
            EditorGUI.PropertyField(rect_damage, property.FindPropertyRelative("damage"), gui_damage);

            EditorGUI.LabelField(rect_knockbackLabel, gui_knockback);
            EditorGUI.LabelField(rect_kbBaseLabel, gui_kbBase);
            EditorGUI.PropertyField(rect_kbBase, property.FindPropertyRelative("knockbackBase"), GUIContent.none);
            EditorGUI.LabelField(rect_kbGrowthLabel, gui_kbGrowth);
            EditorGUI.PropertyField(rect_kbGrowth, property.FindPropertyRelative("knockbackGrowth"), GUIContent.none);

            EditorGUI.LabelField(rect_lchAngleLabel, gui_lchAngle);
            EditorGUI.IntSlider(rect_lchAngle, property.FindPropertyRelative("launchAngle"), 0, 361, GUIContent.none);

            EditorGUI.PropertyField(rect_hitstunType, property.FindPropertyRelative("hitstunType"), gui_hitstunType);

            if (_jointChange)
            {
                _jointChange = false;

                Debug.Log(s_joints[_ownerID]);
                
                property.FindPropertyRelative("jointID").intValue = s_joints[_ownerID].GetInstanceID();

                property.serializedObject.ApplyModifiedProperties();

                Debug.Log(property.FindPropertyRelative("jointID").intValue);
            }
            
            EditorGUI.LabelField(rect_jointLabel, gui_joint, new GUIStyle { fontStyle = FontStyle.Bold });
            if (_foundJoints)
            {
                Joint corrJoint = GetJointByID(property.FindPropertyRelative("jointID").intValue);
                string jointLabel = corrJoint ? corrJoint.name : "No Joint Selected";

                EditorGUI.LabelField(rect_jointName, jointLabel);
                if (GUI.Button(rect_jointSelect, gui_jointSelect))
                {
                    GenericMenu menu = new GenericMenu();

                    for (int i = 0; i < _joints.Length; i++)
                    {
                        menu.AddItem(new GUIContent(_joints[i].name), false, JointSelect,  _joints[i]);
                    }

                    if (menu.GetItemCount() < 1)
                        menu.AddDisabledItem(new GUIContent("No Joints Found"));

                    menu.ShowAsContext();
                }
            }
            
            EditorGUI.PropertyField(rect_hitbox, property.FindPropertyRelative("hitbox"), gui_hitbox);

            EditorGUI.EndProperty();
        }

        struct JointData { public Joint joint; public SerializedProperty property; }

        void JointSelect(object target)
        {
            s_joints[_ownerID] = (Joint)target;
            _jointChange = true;
        }

        Joint GetJointByID(int instanceID)
        {
            for (int i = 0; i < _joints.Length; i++)
            {
                if (_joints[i].GetInstanceID() == instanceID)
                    return _joints[i];
            }

            return null;
        }
        
        void SetRects(Rect position, bool label)
        {
            if (label)
            {
                rect_controlName = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                position.y += EditorGUIUtility.singleLineHeight + _buffer;
                position.x += position.width * 0.02f;
                position.width *= 0.98f;
            }

            rect_id = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            rect_priority = new Rect(position.x, rect_id.yMax + _buffer, position.width, EditorGUIUtility.singleLineHeight);
            rect_enableRegion = new Rect(position.x, rect_priority.yMax + _buffer, position.width, new IntRangeDrawer().GetPropertyHeight(null, GUIContent.none));
            rect_damage = new Rect(position.x, rect_enableRegion.yMax + _buffer, position.width, EditorGUIUtility.singleLineHeight);

            rect_knockbackLabel = new Rect(position.x + (position.width * 0.1f), rect_damage.yMax + _buffer, position.width * 0.9f, EditorGUIUtility.singleLineHeight);
            rect_kbBaseLabel = new Rect(position.x, rect_knockbackLabel.yMax + _buffer, position.width * 0.24f, EditorGUIUtility.singleLineHeight);
            rect_kbBase = new Rect(rect_kbBaseLabel.xMax, rect_kbBaseLabel.y, position.width * 0.25f, rect_kbBaseLabel.height);
            rect_kbGrowthLabel = new Rect(rect_kbBase.xMax + (position.width * 0.02f), rect_kbBase.y, rect_kbBaseLabel.width, rect_kbBase.height);
            rect_kbGrowth = new Rect(rect_kbGrowthLabel.xMax, rect_kbGrowthLabel.y, rect_kbBase.width, rect_kbBase.height);

            rect_lchAngleLabel = new Rect(position.x, rect_kbGrowthLabel.yMax + _buffer, position.width * 0.19f, EditorGUIUtility.singleLineHeight);
            rect_lchAngle = new Rect(rect_lchAngleLabel.xMax + (position.width * 0.02f), rect_lchAngleLabel.y, position.width * 0.79f, EditorGUIUtility.singleLineHeight);

            rect_jointLabel = new Rect(position.x + (position.width * 0.15f), rect_lchAngleLabel.yMax + _buffer, position.width * 0.85f, EditorGUIUtility.singleLineHeight);
            if (_foundJoints)
            {
                rect_jointName = new Rect(position.x, rect_jointLabel.yMax, position.width * 0.48f, EditorGUIUtility.singleLineHeight);
                rect_jointSelect = new Rect(position.x + (position.width * 0.5f), rect_jointName.y, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
            }

            rect_hitstunType = new Rect(position.x, (_foundJoints ? rect_jointName : rect_jointLabel).yMax + _buffer, position.width, EditorGUIUtility.singleLineHeight);
            rect_hitbox = new Rect(position.x, rect_hitstunType.yMax + _buffer + _buffer, position.width, IBoxDataDrawer.propertyHeight);
        }
    }
}

