using UnityEngine;
using UnityEngine.UI;
using MultiverseArcade.Core;

namespace MultiverseArcade.UI
{
    /// <summary>
    /// Controller UI untuk Main Menu utama Multiverse Arcade.
    /// Sesuai GDD Bagian 2.1 & 2.2.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Main Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _quitButton;

        [Header("Dialog Panels")]
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _creditsPanel;

        private void Start()
        {
            if (_playButton != null) _playButton.onClick.AddListener(OnPlayClicked);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(OnSettingsClicked);
            if (_creditsButton != null) _creditsButton.onClick.AddListener(OnCreditsClicked);
            if (_quitButton != null) _quitButton.onClick.AddListener(OnQuitClicked);

            if (_settingsPanel != null) _settingsPanel.SetActive(false);
            if (_creditsPanel != null) _creditsPanel.SetActive(false);
        }

        public void BindControls(Button play, Button settings, Button credits, Button quit, GameObject settingsPanel, GameObject creditsPanel)
        {
            _playButton = play;
            _settingsButton = settings;
            _creditsButton = credits;
            _quitButton = quit;
            _settingsPanel = settingsPanel;
            _creditsPanel = creditsPanel;

            if (_playButton != null)
            {
                _playButton.onClick.RemoveAllListeners();
                _playButton.onClick.AddListener(OnPlayClicked);
            }

            if (_settingsButton != null)
            {
                _settingsButton.onClick.RemoveAllListeners();
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            }

            if (_creditsButton != null)
            {
                _creditsButton.onClick.RemoveAllListeners();
                _creditsButton.onClick.AddListener(OnCreditsClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveAllListeners();
                _quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        private void OnPlayClicked()
        {
            PlayClick();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToSelection();
            }
        }

        private void OnSettingsClicked()
        {
            PlayClick();
            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(true);
            }
        }

        private void OnCreditsClicked()
        {
            PlayClick();
            if (_creditsPanel != null)
            {
                _creditsPanel.SetActive(true);
            }
        }

        private void OnQuitClicked()
        {
            PlayClick();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitApplication();
            }
        }

        private void PlayClick()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
    }
}
