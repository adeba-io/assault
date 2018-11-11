using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assault.Managers
{
    public class MusicManager : MonoBehaviour
    {
        [SerializeField] SceneAudioPair[] _sceneMusicArray;

        AudioSource _audioSource;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += NewLevelLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= NewLevelLoaded;
        }

        void NewLevelLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("Loaded Level: " + scene.name + "; Build Index: " + scene.buildIndex + "; Load Mode: " + mode.ToString());

            SceneAudioPair currSceneAudioPair = SearchSceneMusicArray(scene.buildIndex);

            if (currSceneAudioPair)
            {
                _audioSource.clip = currSceneAudioPair.GetRandomAudioClip();
                _audioSource.loop = true;
                _audioSource.Play();
                Debug.Log("Playing clip: " + _audioSource.clip.name);
            }
        }

        SceneAudioPair SearchSceneMusicArray(int indexToSearchFor)
        {
            for (int i = 0; i < _sceneMusicArray.Length; i++)
            {
                if (_sceneMusicArray[i].sceneBuildIndex == indexToSearchFor)
                    return _sceneMusicArray[i];
            }

            return null;
        }

        public void SetVolume(float volume)
        {
            _audioSource.volume = volume;
        }
    }
    
    [System.Serializable]
    public class SceneAudioPair
    {
        public int sceneBuildIndex;
        public AudioClip[] sceneMusicArray;

        public string sceneName
        { get { return SceneManager.GetSceneByBuildIndex(sceneBuildIndex).name; } }

        public AudioClip GetRandomAudioClip()
        {
            try
            {
                return sceneMusicArray[Random.Range(0, sceneMusicArray.Length - 1)];
            }
            catch
            {
                return null;
            }
        }

        public static implicit operator bool(SceneAudioPair audioPair)
        {
            return audioPair != null;
        }
    }
}
