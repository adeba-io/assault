using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

namespace Assault.Managers
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] float _loadOffSplashScreenTime = 5f;

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
    }
}
