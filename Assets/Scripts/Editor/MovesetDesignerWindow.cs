using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MovesetDesignerWindow  : EditorWindow
{
    static MovesetDesignerWindow s_window = null;

    Animator _animator = null;
    AnimationClip[] _clips = null;
    AnimationClip _playingClip = null;
    float _animationTime = 0f;

    Texture2D _headerTexture;
    Texture2D _animationViewTexture;
    Texture2D _currentMoveTexture;
    Texture2D _animationTimelineTexture;
    Texture2D _movesetListTexture;

    Rect _headerRect;
    Rect _animationViewRect;
    Rect _currentMoveRect;
    Rect _animationTimelineRect;
    Rect _movesetListRect;

    [MenuItem("Assault/Moveset Maker")]
    public static void OpenWindow()
    {
        s_window = EditorWindow.GetWindow<MovesetDesignerWindow>(true, "Moveset Designer", true);
        s_window.minSize = new Vector2(800, 400);
        s_window.Show();
    }

    private void OnGUI()
    {
        DrawLayouts();
        DrawAnimationTimeline();
        DrawCurrentMoveData();
        DrawMovesetList();
    }

    private void OnEnable()
    {
        InitTextures();
    }

    private void OnDisable()
    {
        
    }

    void InitTextures()
    {
        _headerTexture = new Texture2D(1, 1);
        _headerTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 0.8f));
        _headerTexture.Apply();

        _animationViewTexture = new Texture2D(1, 1);
        _animationViewTexture.SetPixel(0, 0, Color.blue);
        _animationViewTexture.Apply();

        _currentMoveTexture = new Texture2D(1, 1);
        _currentMoveTexture.SetPixel(0, 0, Color.green);
        _currentMoveTexture.Apply();

        _animationTimelineTexture = new Texture2D(1, 1);
        _animationTimelineTexture.SetPixel(0, 0, Color.gray);
        _animationTimelineTexture.Apply();

        _movesetListTexture = new Texture2D(1, 1);
        _movesetListTexture.SetPixel(0, 0, Color.red);
        _movesetListTexture.Apply();
    }

    void DrawLayouts()
    {
        _headerRect = new Rect(0, 0, Screen.width * 0.4f * 0.8f, Screen.height * 0.1f * 0.8f);

        _animationViewRect = new Rect(0, 0, Screen.width * 0.6f * 0.8f, Screen.height * 0.8f * 0.8f);
        _currentMoveRect = new Rect(Screen.width * 0.6f * 0.8f, 0, Screen.width * 0.2f * 0.8f, Screen.height * 0.8f * 0.8f);
        _movesetListRect = new Rect(Screen.width * 0.8f * 0.8f, 0, Screen.width * 0.2f * 0.8f, Screen.height * 0.8f);
        _animationTimelineRect = new Rect(0, Screen.height * 0.8f * 0.8f, Screen.width * 0.8f * 0.8f, Screen.height * 0.2f * 0.8f);

        Debug.Log(_currentMoveRect.x);
        
        GUI.DrawTexture(_animationViewRect, _animationViewTexture);
        GUI.DrawTexture(_currentMoveRect, _currentMoveTexture);
        GUI.DrawTexture(_movesetListRect, _movesetListTexture);
        GUI.DrawTexture(_animationTimelineRect, _animationTimelineTexture);
        GUI.DrawTexture(_headerRect, _headerTexture);
    }

    void DrawCurrentMoveData()
    {

    }

    void DrawMovesetList()
    {

    }

    void DrawAnimationTimeline()
    {

    }
}
