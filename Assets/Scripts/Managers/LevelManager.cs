using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

namespace Assault.Managers
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager LM { get; protected set; }

        [SerializeField] float _loadOffSplashScreenTime = 5f;

        [SerializeField] string _stageSelectScreen = "02a Stage Select";
        [SerializeField] string _characterSelectScreen = "02b Character Select";
        [SerializeField] string _mainMenu = "01a Main Menu";

        public string stageSelectScreen { get { return _stageSelectScreen; } }
        public string characterSelectScreen { get { return _characterSelectScreen; } }
        public string mainMenu { get { return _mainMenu; } }

        private void Awake()
        {
            if (LM == null)
            {
                LM = this;
                DontDestroyOnLoad(this);
            }
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            if (_loadOffSplashScreenTime <= 0)
                Debug.Log("Level auto load disabled or use a positive value");
            else
                Invoke("LoadNextLevel", _loadOffSplashScreenTime);

            if (SceneManager.GetActiveScene().buildIndex != 0) CancelInvoke("LoadNextLevel");
        }

        public void LoadLevel(string sceneName)
        {
            Debug.Log("Loading Level: " + sceneName);

            SceneManager.LoadScene(sceneName);
        }

        public void QuitRequest()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void LoadNextLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        public void LoadStageSelectScreen()
        {
            SceneManager.LoadScene(_stageSelectScreen);
        }

        public void LoadCharacterSelectScreen()
        {
            SceneManager.LoadScene(_characterSelectScreen);
        }

        public void LoadMainMenu()
        {
            SceneManager.LoadScene(_mainMenu);
        }
    }
}
