using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiverseArcade.Core;

namespace MultiverseArcade.UI
{
    /// <summary>
    /// Controller UI untuk panel Pengaturan (Audio volume & Kualitas Grafis).
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("Audio Sliders")]
        [SerializeField] private Slider _masterSlider;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;

        [Header("Audio Percentage Texts")]
        [SerializeField] private TextMeshProUGUI _masterText;
        [SerializeField] private TextMeshProUGUI _musicText;
        [SerializeField] private TextMeshProUGUI _sfxText;

        [Header("Graphics Buttons")]
        [SerializeField] private Button _qualityLowButton;
        [SerializeField] private Button _qualityMedButton;
        [SerializeField] private Button _qualityHighButton;

        [Header("Close Button")]
        [SerializeField] private Button _closeButton;

        private void OnEnable()
        {
            RefreshUI();

            if (_masterSlider != null) _masterSlider.onValueChanged.AddListener(OnMasterChanged);
            if (_musicSlider != null) _musicSlider.onValueChanged.AddListener(OnMusicChanged);
            if (_sfxSlider != null) _sfxSlider.onValueChanged.AddListener(OnSFXChanged);

            if (_qualityLowButton != null) _qualityLowButton.onClick.AddListener(() => SetQuality(0));
            if (_qualityMedButton != null) _qualityMedButton.onClick.AddListener(() => SetQuality(1));
            if (_qualityHighButton != null) _qualityHighButton.onClick.AddListener(() => SetQuality(2));

            if (_closeButton != null) _closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void OnDisable()
        {
            if (_masterSlider != null) _masterSlider.onValueChanged.RemoveListener(OnMasterChanged);
            if (_musicSlider != null) _musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
            if (_sfxSlider != null) _sfxSlider.onValueChanged.RemoveListener(OnSFXChanged);

            if (_qualityLowButton != null) _qualityLowButton.onClick.RemoveAllListeners();
            if (_qualityMedButton != null) _qualityMedButton.onClick.RemoveAllListeners();
            if (_qualityHighButton != null) _qualityHighButton.onClick.RemoveAllListeners();

            if (_closeButton != null) _closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        public void BindControls(Slider master, Slider music, Slider sfx,
            TextMeshProUGUI masterTxt, TextMeshProUGUI musicTxt, TextMeshProUGUI sfxTxt,
            Button qLow, Button qMed, Button qHigh, Button closeBtn)
        {
            _masterSlider = master;
            _musicSlider = music;
            _sfxSlider = sfx;
            _masterText = masterTxt;
            _musicText = musicTxt;
            _sfxText = sfxTxt;
            _qualityLowButton = qLow;
            _qualityMedButton = qMed;
            _qualityHighButton = qHigh;
            _closeButton = closeBtn;

            RefreshUI();
        }

        public void RefreshUI()
        {
            float m = SettingsManager.Instance != null ? SettingsManager.Instance.MasterVolume : 0.8f;
            float bgm = SettingsManager.Instance != null ? SettingsManager.Instance.MusicVolume : 0.7f;
            float sfx = SettingsManager.Instance != null ? SettingsManager.Instance.SFXVolume : 0.9f;

            if (_masterSlider != null) _masterSlider.value = m;
            if (_musicSlider != null) _musicSlider.value = bgm;
            if (_sfxSlider != null) _sfxSlider.value = sfx;

            UpdateTexts(m, bgm, sfx);
        }

        private void OnMasterChanged(float val)
        {
            if (SettingsManager.Instance != null) SettingsManager.Instance.SetMasterVolume(val);
            if (_masterText != null) _masterText.text = $"{Mathf.RoundToInt(val * 100)}%";
        }

        private void OnMusicChanged(float val)
        {
            if (SettingsManager.Instance != null) SettingsManager.Instance.SetMusicVolume(val);
            if (_musicText != null) _musicText.text = $"{Mathf.RoundToInt(val * 100)}%";
        }

        private void OnSFXChanged(float val)
        {
            if (SettingsManager.Instance != null) SettingsManager.Instance.SetSFXVolume(val);
            if (_sfxText != null) _sfxText.text = $"{Mathf.RoundToInt(val * 100)}%";
        }

        private void SetQuality(int level)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            if (SettingsManager.Instance != null) SettingsManager.Instance.SetGraphicQuality(level);
        }

        private void UpdateTexts(float m, float bgm, float sfx)
        {
            if (_masterText != null) _masterText.text = $"{Mathf.RoundToInt(m * 100)}%";
            if (_musicText != null) _musicText.text = $"{Mathf.RoundToInt(bgm * 100)}%";
            if (_sfxText != null) _sfxText.text = $"{Mathf.RoundToInt(sfx * 100)}%";
        }

        private void OnCloseClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            gameObject.SetActive(false);
        }
    }
}
