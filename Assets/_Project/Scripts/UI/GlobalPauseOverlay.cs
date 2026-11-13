using UnityEngine;
using UnityEngine.UI;
using MultiverseArcade.Core;

namespace MultiverseArcade.UI
{
    /// <summary>
    /// UI Controller untuk Global Pause Overlay yang muncul di semua scene gameplay saat tombol Esc ditekan.
    /// Sesuai GDD Bagian 2.2 & 2.3.
    /// </summary>
    public class GlobalPauseOverlay : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _switchGameButton;
        [SerializeField] private Button _mainMenuButton;

        private void Awake()
        {
            if (_resumeButton != null) _resumeButton.onClick.AddListener(OnResumeClicked);
            if (_restartButton != null) _restartButton.onClick.AddListener(OnRestartClicked);
            if (_switchGameButton != null) _switchGameButton.onClick.AddListener(OnSwitchGameClicked);
            if (_mainMenuButton != null) _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        public void BindButtons(Button resume, Button restart, Button switchGame, Button mainMenu)
        {
            _resumeButton = resume;
            _restartButton = restart;
            _switchGameButton = switchGame;
            _mainMenuButton = mainMenu;

            if (_resumeButton != null)
            {
                _resumeButton.onClick.RemoveAllListeners();
                _resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveAllListeners();
                _restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (_switchGameButton != null)
            {
                _switchGameButton.onClick.RemoveAllListeners();
                _switchGameButton.onClick.AddListener(OnSwitchGameClicked);
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.onClick.RemoveAllListeners();
                _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        private void OnResumeClicked()
        {
            PlayClick();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }

        private void OnRestartClicked()
        {
            PlayClick();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentGame();
            }
        }

        private void OnSwitchGameClicked()
        {
            PlayClick();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToSelection();
            }
        }

        private void OnMainMenuClicked()
        {
            PlayClick();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToMenu();
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
