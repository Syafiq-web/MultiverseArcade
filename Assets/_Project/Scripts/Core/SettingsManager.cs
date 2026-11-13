using UnityEngine;

namespace MultiverseArcade.Core
{
    /// <summary>
    /// Manager pengaturan grafis, audio, dan display terpusat.
    /// Tersinkronisasi dengan SaveManager dan AudioManager.
    /// Sesuai GDD Bagian 2.2.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        public float MasterVolume => SaveManager.Instance != null ? SaveManager.Instance.Data.masterVolume : 0.8f;
        public float MusicVolume => SaveManager.Instance != null ? SaveManager.Instance.Data.musicVolume : 0.7f;
        public float SFXVolume => SaveManager.Instance != null ? SaveManager.Instance.Data.sfxVolume : 0.9f;
        public int GraphicQuality => SaveManager.Instance != null ? SaveManager.Instance.Data.graphicQuality : 2;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ApplyAllSettings();
        }

        public void ApplyAllSettings()
        {
            if (SaveManager.Instance == null) return;

            var data = SaveManager.Instance.Data;
            SetGraphicQuality(data.graphicQuality, false);
            SetMasterVolume(data.masterVolume, false);
            SetMusicVolume(data.musicVolume, false);
            SetSFXVolume(data.sfxVolume, false);
        }

        public void SetMasterVolume(float volume, bool save = true)
        {
            volume = Mathf.Clamp01(volume);
            if (SaveManager.Instance != null && save)
            {
                SaveManager.Instance.Data.masterVolume = volume;
                SaveManager.Instance.Save();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMasterVolume(volume);
            }

            EventBus.RaiseSettingsChanged();
        }

        public void SetMusicVolume(float volume, bool save = true)
        {
            volume = Mathf.Clamp01(volume);
            if (SaveManager.Instance != null && save)
            {
                SaveManager.Instance.Data.musicVolume = volume;
                SaveManager.Instance.Save();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(volume);
            }

            EventBus.RaiseSettingsChanged();
        }

        public void SetSFXVolume(float volume, bool save = true)
        {
            volume = Mathf.Clamp01(volume);
            if (SaveManager.Instance != null && save)
            {
                SaveManager.Instance.Data.sfxVolume = volume;
                SaveManager.Instance.Save();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(volume);
            }

            EventBus.RaiseSettingsChanged();
        }

        public void SetGraphicQuality(int qualityLevel, bool save = true)
        {
            int maxLevels = QualitySettings.names.Length;
            qualityLevel = Mathf.Clamp(qualityLevel, 0, Mathf.Max(0, maxLevels - 1));

            QualitySettings.SetQualityLevel(qualityLevel, true);

            if (SaveManager.Instance != null && save)
            {
                SaveManager.Instance.Data.graphicQuality = qualityLevel;
                SaveManager.Instance.Save();
            }

            EventBus.RaiseSettingsChanged();
            Debug.Log($"[SettingsManager] Kualitas grafis diatur ke level: {qualityLevel} ({QualitySettings.names[qualityLevel]})");
        }
    }
}
