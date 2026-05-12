using UnityEngine;
using UnityEngine.UI;

namespace DragonGlare
{
    public class OptionsMenu : MonoBehaviour
    {
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider seSlider;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Dropdown resolutionDropdown;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button backButton;

        private void Awake()
        {
            bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
            seSlider.onValueChanged.AddListener(OnSeVolumeChanged);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            saveButton.onClick.AddListener(SaveSettings);
            backButton.onClick.AddListener(Hide);
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            bgmSlider.value = PlayerPrefs.GetFloat("BGMVolume", 0.85f);
            seSlider.value = PlayerPrefs.GetFloat("SEVolume", 0.9f);
            fullscreenToggle.isOn = Screen.fullScreen;
        }

        private void OnBgmVolumeChanged(float value)
        {
            GameManager.Instance.Audio.SetBgmVolume(value);
        }

        private void OnSeVolumeChanged(float value)
        {
            GameManager.Instance.Audio.SetSeVolume(value);
        }

        private void OnFullscreenChanged(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen;
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat("BGMVolume", bgmSlider.value);
            PlayerPrefs.SetFloat("SEVolume", seSlider.value);
            PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
