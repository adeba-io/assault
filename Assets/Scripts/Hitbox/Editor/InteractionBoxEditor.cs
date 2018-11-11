using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Assault.Boxes;

namespace Assault.Editors
{
    [CustomEditor(typeof(InteractionBox), true)]
    public class InteractionBoxEditor : Editor
    {
        InteractionBox _target;
        SpriteRenderer[] _renderers;
        Collider2D _collider;
        Rigidbody2D _rigidbody;

        SerializedProperty _renderHitbox;
        SerializedProperty _boxColor;

        SerializedProperty _boxData;

        readonly GUIContent gui_renderHitbox = new GUIContent("Render Hitboxes?");
        readonly GUIContent gui_boxColor = new GUIContent("Hitbox Color");

        private void OnEnable()
        {
            _target = (InteractionBox)target;
            _renderers = _target.GetComponentsInChildren<SpriteRenderer>();
            _rigidbody = _target.GetComponent<Rigidbody2D>();
            _collider = _target.GetComponent<Collider2D>();

            _boxData = serializedObject.FindProperty("_boxData");

            _renderHitbox = serializedObject.FindProperty("_renderHitbox");
            _boxColor = serializedObject.FindProperty("_boxColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //base.OnInspectorGUI();

            Rect rect_control = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight + 2f);
            float nextBuffer = rect_control.width * 0.02f;
            Rect rect_next = new Rect(rect_control.x, rect_control.y, rect_control.width * 0.29f, EditorGUIUtility.singleLineHeight);

            EditorGUI.LabelField(rect_next, gui_renderHitbox);

            rect_next.x += rect_next.width + nextBuffer;
            rect_next.width = rect_control.width * 0.19f;
            EditorGUI.PropertyField(rect_next, _renderHitbox, GUIContent.none);

            if (_renderHitbox.boolValue)
            {
                rect_next.x += rect_next.width + nextBuffer;
                rect_next.width = rect_control.width * 0.49f;

                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(rect_next, _boxColor, GUIContent.none);
                if (EditorGUI.EndChangeCheck())
                {
                    for (int i = 0; i < _renderers.Length; i++)
                    {
                        _renderers[i].color = _boxColor.colorValue;
                    }
                }
            }
            else
            {
                for (int i = 0; i < _renderers.Length; i++)
                {
                    _renderers[i].gameObject.SetActive(false);
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_boxData, GUIContent.none);
            if (EditorGUI.EndChangeCheck())
            {
                Vector2 boxSize = _boxData.FindPropertyRelative("boxSize").vector2Value;
                Vector2 offset = _boxData.FindPropertyRelative("offset").vector2Value;

                if (_collider.GetType() == typeof(BoxCollider2D))
                {
                    BoxCollider2D col = (BoxCollider2D)_collider;
                    col.size = boxSize;
                    col.offset = offset;
                    _collider = col;

                    _renderers[0].transform.localScale = boxSize;
                    _renderers[0].transform.localPosition = offset;
                }
                else if (_collider.GetType() == typeof(CapsuleCollider2D))
                {
                    _rigidbody.rotation = _boxData.FindPropertyRelative("rotation").floatValue;

                    float parallelHeight = boxSize.y - boxSize.x;
                    if (parallelHeight < 0) parallelHeight = 0;

                    CapsuleCollider2D col = (CapsuleCollider2D)_collider;
                    col.size = boxSize;
                    col.offset = offset - new Vector2(0, parallelHeight / 2f);
                    _collider = col;

                    _renderers[0].transform.localScale = new Vector3(boxSize.x, boxSize.x);
                    _renderers[1].transform.localScale = new Vector3(boxSize.x, (boxSize.y - 2f) < 0 ? 0 : boxSize.y - 2f);
                    _renderers[2].transform.localScale = new Vector3(boxSize.x, boxSize.x);

                    _renderers[1].transform.localPosition = new Vector3(0, -parallelHeight / 2f) + (Vector3)offset;
                    _renderers[2].transform.localPosition = new Vector3(0, -parallelHeight) + (Vector3)offset;
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
