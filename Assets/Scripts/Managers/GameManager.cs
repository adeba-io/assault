using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assault.UI;

namespace Assault
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager GM { get; protected set; }

        [SerializeField] string[] _stageNames;

        [SerializeField] float _FPSUpdateInterval = 0.5f;
        
        double _lastInterval;
        int _frames = 0;
        float _fps;

        private void Awake()
        {
            if (GM == null)
            {
                GM = this;
                DontDestroyOnLoad(gameObject);
            }
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            _lastInterval = Time.realtimeSinceStartup;
            _frames = 0;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += StageLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= StageLoaded;
        }

        void StageLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_stageNames.Contains(scene.name))
            {
                Debug.Log("Ya");
                Debug.Log(GameObject.FindObjectOfType<Stage>());
            }
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
