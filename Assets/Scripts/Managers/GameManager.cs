using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assault
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager _instance;

        [SerializeField] float _FPSUpdateInterval = 0.5f;

        double _lastInterval;
        int _frames = 0;
        float _fps;

        private void Awake()
        {
            if (_instance != null && _instance != this)
                Destroy(gameObject);
            else
                _instance = this;

            //Application.targetFrameRate = 60;
        }

        private void Start()
        {
            _lastInterval = Time.realtimeSinceStartup;
            _frames = 0;
        }

        private void Update()
        {
            FrameCount();
        }

        void FrameCount()
        {
            _frames++;
            float timeNow = Time.realtimeSinceStartup;

            if (timeNow > _lastInterval + _FPSUpdateInterval)
            {
                _fps = (float)(_frames / (timeNow - _lastInterval));
                _frames = 0;
                _lastInterval = timeNow;
            }
        }

        private void OnGUI()
        {
            const float height = 50f;

            GUILayout.BeginArea(new Rect(10, 10, 150, height));

            GUILayout.BeginVertical("box");

            GUILayout.Label("FPS: " + _fps);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
