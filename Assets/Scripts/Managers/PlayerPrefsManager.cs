using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assault.Managers
{
    public static class PlayerPrefsManager
    {
        const string    MASTER_VOLUME_KEY = "master_volume",
                        SOUND_EFFECTS_VOLUME_KEY = "sound_effects_volume",
                        VOICE_VOLUME_KEY = "voice_volume";
        
        public static float MASTER_VOLUME
        {
            get { return PlayerPrefs.GetFloat(MASTER_VOLUME_KEY); }
            set
            {
                if (value < 0f || value > 1f)
                {
                    Debug.LogError("Value out of Master Volume Range");
                }
                else
                {
                    PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, value);
                }
            }
        }

        public static float SOUND_EFFECTS_VOLUME
        {
            get { return PlayerPrefs.GetFloat(SOUND_EFFECTS_VOLUME_KEY); }
            set
            {
                if (value < 0f || value > 1f)
                {
                    Debug.LogError("Value out of Sound Effects Volume Range");
                }
                else
                {
                    PlayerPrefs.SetFloat(SOUND_EFFECTS_VOLUME_KEY, value);
                }
            }
        }

        public static float VOICE_VOLUME
        {
            get { return PlayerPrefs.GetFloat(VOICE_VOLUME_KEY); }
            set
            {
                if (value < 0f || value > 1f)
                {
                    Debug.LogError("Value out of Voice Volume Range");
                }
                else
                {
                    PlayerPrefs.SetFloat(VOICE_VOLUME_KEY, value);
                }
            }
        }
    }
}
