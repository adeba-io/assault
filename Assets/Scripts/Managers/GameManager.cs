using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assault.UI;

namespace Assault.Managers
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager GM { get; protected set; }
        
        [SerializeField] int _nextStageSelectController = 1;
        
        [SerializeField] int _numberOfPlayers = 2;

        [SerializeField] string[] _stageNames;

        [SerializeField] string _nextStage = "";
        [SerializeField] FighterSpawnData[] _spawningFighters = null;

        [SerializeField] GameObject _1PCSS, _2PCSS, _4PCSS;

        [SerializeField] float _FPSUpdateInterval = 0.5f;

        [SerializeField] string _previousStage = "";
        
        double _lastInterval;
        int _frames = 0;
        float _fps;

        public string[] stageNames { get { return _stageNames; } }

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
            SceneManager.sceneUnloaded += SceneUnloaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= StageLoaded;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        void StageLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == LevelManager.LM.stageSelectScreen)
            {
                SelectionPointer stageSelectPointer = FindObjectOfType<SelectionPointer>();
                stageSelectPointer.SetPlayerNumber(_nextStageSelectController);

                _nextStage = "";
                _spawningFighters = null;
                _previousStage = LevelManager.LM.mainMenu;
            }
            else if (scene.name == LevelManager.LM.characterSelectScreen)
            {
                if (_numberOfPlayers == 1)
                {
                    Instantiate(_1PCSS);
                }
                else if (_numberOfPlayers == 2)
                {
                    Instantiate(_2PCSS);
                }
                else
                {
                    Instantiate(_4PCSS);
                }
            }
            else if (_stageNames.Contains(scene.name))
            {
                if (_spawningFighters == null) return;

                Stage stage = FindObjectOfType<Stage>();
                
                List<int> usedNodes = new List<int>();
                for (int i = 0; i < _spawningFighters.Length; i++)
                {
                    int node = 0;
                    while (true)
                    {
                        node = Random.Range(0, stage.spawnPoints.Length);
                        if (!usedNodes.Contains(node))
                        {
                            usedNodes.Add(node);
                            break;
                        }
                    }

                    FighterController fighter = _spawningFighters[i].fighter.GetComponent<FighterController>();
                    fighter.spawnPoint = stage.spawnPoints[node];
                    fighter.playerNumber = _spawningFighters[i].playerNumber;
                    print(fighter.playerNumber);
                    Instantiate(fighter.gameObject);
                }
            }
        }

        void SceneUnloaded(Scene current)
        {
            _previousStage = current.name;
        }
        
        public void SetNextStage(SelectorPanel panel)
        {
            _nextStage = panel.stageName;

            if (_spawningFighters == null)
            {
                LevelManager.LM.LoadCharacterSelectScreen();
            }
            else
            {
                LevelManager.LM.LoadLevel(_nextStage);
            }
        }

        public void SetFighters(SelectorPanel[] panels)
        {
            List<FighterSpawnData> fighters = new List<FighterSpawnData>();
            for (int i = 0; i < panels.Length; i++)
            {
                fighters.Add(new FighterSpawnData { fighter = panels[i].fighterReference, playerNumber = i + 1 });
            }

            _spawningFighters = fighters.ToArray();

            if (_nextStage == "")
            {
                LevelManager.LM.LoadStageSelectScreen();
            }
            else
            {
                LevelManager.LM.LoadLevel(_nextStage);
            }
        }
        
        public void LoadPrevious()
        {
            LevelManager.LM.LoadLevel(_previousStage);
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

    struct FighterSpawnData
    {
        public GameObject fighter;
        public int playerNumber;
    }
}
