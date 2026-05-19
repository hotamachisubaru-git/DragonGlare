namespace DragonGlare.Services
{
    public interface IStartupSettingsService
    {
        Domain.Startup.LaunchSettings LoadSettings();
        void SaveSettings(Domain.Startup.LaunchSettings settings);
    }
}