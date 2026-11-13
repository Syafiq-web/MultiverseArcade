using UnityEngine;
using UnityEngine.UI;
using MultiverseArcade.Core;

namespace MultiverseArcade.UI
{
    /// <summary>
    /// UI Overlay universal yang ditempatkan di setiap Scene gameplay.
    /// Menyediakan kontrol Pause, Switch Game, Main Menu, dan Restart secara seragam.
    /// Sesuai GDD Bagian 2.2 & 2.3.
    /// </summary>
    public class GameOverlayUI : MonoBehaviour
    {
        [Header("Pause Panel")]
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _resumeButton;

        [Header("Navigation Buttons")]
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _switchGameButton;
        [SerializeField] private Button _mainMenuButton;

        private void Start()
        {
            if (_pausePanel != null)
                _pausePanel.SetActive(false);

            if (_pauseButton != null)
                _pauseButton.onClick.AddListener(OnPauseClicked);

            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(OnResumeClicked);

            if (_restartButton != null)
                _restartButton.onClick.AddListener(OnRestartClicked);

            if (_switchGameButton != null)
                _switchGameButton.onClick.AddListener(OnSwitchGameClicked);

            if (_mainMenuButton != null)
                _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnEnable()
        {
            GameManager.OnPauseStateChanged += HandlePauseStateChanged;
        }

        private void OnDisable()
        {
            GameManager.OnPauseStateChanged -= HandlePauseStateChanged;
        }

        private void HandlePauseStateChanged(bool isPaused)
        {
            if (_pausePanel != null)
            {
                _pausePanel.SetActive(isPaused);
            }
        }

        public void BindControls(GameObject pausePanel, Button pauseBtn, Button resumeBtn,
            Button restartBtn, Button switchBtn, Button menuBtn)
        {
            _pausePanel = pausePanel;
            _pauseButton = pauseBtn;
            _resumeButton = resumeBtn;
            _restartButton = restartBtn;
            _switchGameButton = switchBtn;
            _mainMenuButton = menuBtn;

            if (_pausePanel != null) _pausePanel.SetActive(false);

            if (_pauseButton != null)
            {
                _pauseButton.onClick.RemoveAllListeners();
                _pauseButton.onClick.AddListener(OnPauseClicked);
            }
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

        public void OnPauseClicked()
        {
            PlayClick();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }
            else
            {
                Time.timeScale = 0f;
                if (_pausePanel != null) _pausePanel.SetActive(true);
            }
        }

        public void OnResumeClicked()
        {
            PlayClick();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
            else
            {
                Time.timeScale = 1f;
                if (_pausePanel != null) _pausePanel.SetActive(false);
            }
        }

        public void OnRestartClicked()
        {
            PlayClick();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentGame();
            }
            else if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.RestartCurrentScene();
            }
        }

        public void OnSwitchGameClicked()
        {
            PlayClick();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToSelection();
            }
            else if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.GoToGameSelection();
            }
        }

        public void OnMainMenuClicked()
        {
            PlayClick();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToMenu();
            }
            else if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.GoToMainMenu();
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
