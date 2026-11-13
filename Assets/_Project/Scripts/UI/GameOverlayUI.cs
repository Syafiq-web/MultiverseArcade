using MultiverseArcade.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MultiverseArcade.UI
{
    /// <summary>
    /// UI Overlay universal yang dapat ditempatkan di setiap Scene game anggota.
    /// Menyediakan tombol Pause, Switch Game, Main Menu, dan Restart secara konsisten.
    /// </summary>
    public class GameOverlayUI : MonoBehaviour
    {
        [Header("Pause Panel")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button resumeButton;

        [Header("Navigation Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button switchGameButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            if (pausePanel != null)
                pausePanel.SetActive(false);

            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseClicked);

            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeClicked);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartClicked);

            if (switchGameButton != null)
                switchGameButton.onClick.AddListener(OnSwitchGameClicked);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void Update()
        {
            // Shortcut keyboard Escape untuk Pause/Resume
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        private void OnDestroy()
        {
            if (pauseButton != null) pauseButton.onClick.RemoveListener(OnPauseClicked);
            if (resumeButton != null) resumeButton.onClick.RemoveListener(OnResumeClicked);
            if (restartButton != null) restartButton.onClick.RemoveListener(OnRestartClicked);
            if (switchGameButton != null) switchGameButton.onClick.RemoveListener(OnSwitchGameClicked);
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        public void TogglePause()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.TogglePause();
                if (pausePanel != null)
                    pausePanel.SetActive(GameManager.Instance.IsPaused);
            }
            else
            {
                // Fallback jika tidak ada GameManager di scene
                bool isCurrentlyPaused = Time.timeScale == 0f;
                Time.timeScale = isCurrentlyPaused ? 1f : 0f;
                if (pausePanel != null)
                    pausePanel.SetActive(!isCurrentlyPaused);
            }
        }

        public void OnPauseClicked()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetPause(true);
            else
                Time.timeScale = 0f;

            if (pausePanel != null)
                pausePanel.SetActive(true);
        }

        public void OnResumeClicked()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.SetPause(false);
            else
                Time.timeScale = 1f;

            if (pausePanel != null)
                pausePanel.SetActive(false);
        }

        public void OnRestartClicked()
        {
            // Pastikan timeScale dinormalkan sebelum restart
            Time.timeScale = 1.0f;

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.RestartCurrentScene();
            }
            else
            {
                var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                UnityEngine.SceneManagement.SceneManager.LoadScene(activeScene.name);
            }
        }

        public void OnSwitchGameClicked()
        {
            // Kembali ke halaman GameSelection
            Time.timeScale = 1.0f;

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.GoToGameSelection();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.GameSelection);
            }
        }

        public void OnMainMenuClicked()
        {
            // Kembali ke MainMenu
            Time.timeScale = 1.0f;

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.GoToMainMenu();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.MainMenu);
            }
        }
    }
}
