using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assault.Managers
{
    public class OptionsManager : MonoBehaviour
    {
        public Slider _masterVolumeSlider, _sfxSlider, _voiceSlider;
        public LevelManager _levelManager;

        MusicManager _musicManager;

        private void Start()
        {
            _musicManager = GameObject.FindObjectOfType<MusicManager>();

            _masterVolumeSlider.value = PlayerPrefsManager.MASTER_VOLUME * 10f;
            _sfxSlider.value = PlayerPrefsManager.SOUND_EFFECTS_VOLUME * 10f;
            _voiceSlider.value = PlayerPrefsManager.VOICE_VOLUME * 10f;
        }

        private void Update()
        {
            _musicManager.SetVolume(_masterVolumeSlider.value);
        }

        public void SaveAndExit()
        {
            PlayerPrefsManager.MASTER_VOLUME = _masterVolumeSlider.value / 10f;
            PlayerPrefsManager.SOUND_EFFECTS_VOLUME = _sfxSlider.value / 10f;
            PlayerPrefsManager.VOICE_VOLUME = _voiceSlider.value / 10f;

            _levelManager.LoadLevel("01a Main Menu");
        }

        public void SetDefaults()
        {
            _masterVolumeSlider.value = 8;
            _sfxSlider.value = 8;
            _voiceSlider.value = 8;
        }
    }
}
