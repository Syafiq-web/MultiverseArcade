using UnityEngine;
using UnityEngine.UI;
using MultiverseArcade.Core;

namespace MultiverseArcade.UI
{
    /// <summary>
    /// UI Overlay universal yang ditempatkan di setiap Scene gameplay.
    /// Menyediakan kontrol Pause, Switch Game, Main Menu, dan Restart secara seragam.
    /// Dilengkapi proteksi panel ganda, penanganan kursor, dan fallback standalone.
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

        private void Update()
        {
            // Esc shortcut jika berjalan mandiri (tanpa GameManager dari Bootstrap)
            if (GameManager.Instance == null && Input.GetKeyDown(KeyCode.Escape))
            {
                if (_pausePanel != null && _pausePanel.activeSelf)
                {
                    OnResumeClicked();
                }
                else
                {
                    OnPauseClicked();
                }
            }
        }

        private void HandlePauseStateChanged(bool isPaused)
        {
            // Proteksi panel ganda: jika UIManager global sudah menampilkan pause overlay di GlobalCanvas,
            // nonaktifkan pause panel lokal pada scene.
            if (UIManager.Instance != null && UIManager.Instance.HasPauseOverlay)
            {
                if (_pausePanel != null) _pausePanel.SetActive(false);
                return;
            }

            if (_pausePanel != null)
            {
                _pausePanel.SetActive(isPaused);
            }

            if (isPaused)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
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
            // Cek kondisi tepi: Dilarang pause jika game sedang countdown atau selesai
            var activeGame = GameManager.Instance != null ? GameManager.Instance.CurrentArcadeGame : FindObjectOfType<MonoBehaviour>() as IArcadeGame;
            if (activeGame != null && !activeGame.CanPause)
            {
                return;
            }

            PlayClick();
            if (GameManager.Instance != null)
            {
                GameManager.Instance.PauseGame();
            }
            else
            {
                Time.timeScale = 0f;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                if (activeGame != null) activeGame.OnGamePause();
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
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                var activeGame = FindObjectOfType<MonoBehaviour>() as IArcadeGame;
                if (activeGame != null) activeGame.OnGameResume();

                if (_pausePanel != null) _pausePanel.SetActive(false);
            }
        }

        public void OnRestartClicked()
        {
            PlayClick();
            Time.timeScale = 1.0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (_pausePanel != null) _pausePanel.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentGame();
            }
            else
            {
                var activeGame = FindObjectOfType<MonoBehaviour>() as IArcadeGame;
                if (activeGame != null)
                {
                    activeGame.OnGameRestart();
                }
                else if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.RestartCurrentScene();
                }
            }
        }

        public void OnSwitchGameClicked()
        {
            PlayClick();
            Time.timeScale = 1.0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (_pausePanel != null) _pausePanel.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToSelection();
            }
            else
            {
                var activeGame = FindObjectOfType<MonoBehaviour>() as IArcadeGame;
                if (activeGame != null) activeGame.OnGameExit();

                if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.GoToGameSelection();
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("GameSelection");
                }
            }
        }

        public void OnMainMenuClicked()
        {
            PlayClick();
            Time.timeScale = 1.0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (_pausePanel != null) _pausePanel.SetActive(false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToMenu();
            }
            else
            {
                var activeGame = FindObjectOfType<MonoBehaviour>() as IArcadeGame;
                if (activeGame != null) activeGame.OnGameExit();

                if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.GoToMainMenu();
                }
                else
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                }
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
