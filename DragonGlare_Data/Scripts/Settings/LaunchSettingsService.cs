using UnityEngine;

namespace DragonGlare.Settings
{
    public class LaunchSettingsService : MonoBehaviour
    {
        private const string DisplayModeKey = "DisplayMode";
        private const string PromptOnStartupKey = "PromptOnStartup";
        private const string BgmVolumeKey = "BGMVolume";
        private const string SeVolumeKey = "SEVolume";

        public LaunchSettings LoadSettings()
        {
            return new LaunchSettings
            {
                DisplayMode = (LaunchDisplayMode)PlayerPrefs.GetInt(DisplayModeKey, 0),
                PromptOnStartup = PlayerPrefs.GetInt(PromptOnStartupKey, 1) == 1,
                BgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, 0.85f),
                SeVolume = PlayerPrefs.GetFloat(SeVolumeKey, 0.9f)
            };
        }

        public void SaveSettings(LaunchSettings settings)
        {
            PlayerPrefs.SetInt(DisplayModeKey, (int)settings.DisplayMode);
            PlayerPrefs.SetInt(PromptOnStartupKey, settings.PromptOnStartup ? 1 : 0);
            PlayerPrefs.SetFloat(BgmVolumeKey, settings.BgmVolume);
            PlayerPrefs.SetFloat(SeVolumeKey, settings.SeVolume);
            PlayerPrefs.Save();
        }

        public void ResetSettings()
        {
            PlayerPrefs.DeleteKey(DisplayModeKey);
            PlayerPrefs.DeleteKey(PromptOnStartupKey);
            PlayerPrefs.DeleteKey(BgmVolumeKey);
            PlayerPrefs.DeleteKey(SeVolumeKey);
            PlayerPrefs.Save();
        }
    }

    [System.Serializable]
    public class LaunchSettings
    {
        public LaunchDisplayMode DisplayMode { get; set; } = LaunchDisplayMode.Window640x480;
        public bool PromptOnStartup { get; set; } = true;
        public float BgmVolume { get; set; } = 0.85f;
        public float SeVolume { get; set; } = 0.9f;
    }
}
