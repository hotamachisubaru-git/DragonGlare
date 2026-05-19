using UnityEngine;

namespace DragonGlare.Services
{
    public class PlayerPrefsStartupSettingsService : IStartupSettingsService
    {
        private const string KeyDisplayMode = "DisplayMode";
        private const string KeyPromptOnStartup = "PromptOnStartup";

        public Domain.Startup.LaunchSettings LoadSettings()
        {
            var displayMode = (Domain.LaunchDisplayMode)PlayerPrefs.GetInt(KeyDisplayMode, (int)Domain.LaunchDisplayMode.Window640x480);
            var promptOnStartup = PlayerPrefs.GetInt(KeyPromptOnStartup, 1) == 1;

            return new Domain.Startup.LaunchSettings
            {
                DisplayMode = displayMode,
                PromptOnStartup = promptOnStartup
            };
        }

        public void SaveSettings(Domain.Startup.LaunchSettings settings)
        {
            PlayerPrefs.SetInt(KeyDisplayMode, (int)settings.DisplayMode);
            PlayerPrefs.SetInt(KeyPromptOnStartup, settings.PromptOnStartup ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}