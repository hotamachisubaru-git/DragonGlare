using UnityEngine;

namespace DragonGlare
{
    public class UnityAudioMixer : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Audio.AudioMixer mixer;

        public void SetBgmVolume(float volume)
        {
            if (mixer != null)
            {
                mixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f);
            }
        }

        public void SetSeVolume(float volume)
        {
            if (mixer != null)
            {
                mixer.SetFloat("SEVolume", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f);
            }
        }
    }
}
