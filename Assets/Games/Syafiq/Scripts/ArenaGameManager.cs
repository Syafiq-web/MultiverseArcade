using MultiverseArcade.Audio;
using MultiverseArcade.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MultiverseArcade.Games.Syafiq
{
    /// <summary>
    /// GameManager untuk Arena Clash 3D (Game Syafiq).
    /// Mengatur skor, wave musuh, nyawa player, kondisi Game Over / Menang,
    /// pemutaran BGM & audio game over, serta tombol navigasi universal.
    /// </summary>
    public class ArenaGameManager : MonoBehaviour
    {
        public static ArenaGameManager Instance { get; private set; }

        [Header("UI - Gameplay HUD")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI waveNotificationText;

        [Header("UI - Game Over Screen")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private Button btnRetry;
        [SerializeField] private Button btnBackSelection;
        [SerializeField] private Button btnMainMenu;

        [Header("Audio Settings")]
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] private AudioClip gameOverSound;

        private AudioSource internalBgmSource;
        private int currentScore = 0;
        private int currentWave = 1;
        private bool isGameOver = false;

        public bool IsGameOver => isGameOver;
        public int CurrentScore => currentScore;
        public int CurrentWave => currentWave;

        private const string HIGH_SCORE_KEY = "Syafiq_Arena_HighScore";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            internalBgmSource = GetComponent<AudioSource>();
            if (internalBgmSource == null)
            {
                internalBgmSource = gameObject.AddComponent<AudioSource>();
                internalBgmSource.loop = true;
                internalBgmSource.playOnAwake = false;
                internalBgmSource.volume = 0.65f;
            }
        }

        private void Start()
        {
            Time.timeScale = 1.0f; // Pastikan waktu normal

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            if (btnRetry != null)
                btnRetry.onClick.AddListener(RestartGame);

            if (btnBackSelection != null)
                btnBackSelection.onClick.AddListener(ReturnToGameSelection);

            if (btnMainMenu != null)
                btnMainMenu.onClick.AddListener(ReturnToMainMenu);

            UpdateScoreUI();
            UpdateWaveUI();

            if (waveNotificationText != null)
            {
                waveNotificationText.gameObject.SetActive(false);
            }

            // Putar Backsound BGM
            PlayBacksound();
        }

        private void PlayBacksound()
        {
            if (bgmClip == null) return;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBGM(bgmClip);
            }
            else if (internalBgmSource != null)
            {
                internalBgmSource.clip = bgmClip;
                internalBgmSource.Play();
            }
        }

        private void StopBacksound()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopBGM();
            }
            else if (internalBgmSource != null)
            {
                internalBgmSource.Stop();
            }
        }

        private void OnDestroy()
        {
            if (btnRetry != null) btnRetry.onClick.RemoveListener(RestartGame);
            if (btnBackSelection != null) btnBackSelection.onClick.RemoveListener(ReturnToGameSelection);
            if (btnMainMenu != null) btnMainMenu.onClick.RemoveListener(ReturnToMainMenu);
        }

        public void UpdatePlayerHealth(int currentHealth, int maxHealth)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }

            if (healthText != null)
            {
                healthText.text = $"HP: {currentHealth} / {maxHealth}";
            }

            // Screen shake saat darah berkurang
            if (currentHealth < maxHealth && ArenaCameraController.Instance != null)
            {
                ArenaCameraController.Instance.TriggerShake(0.2f, 0.4f);
            }
        }

        public void OnEnemyDefeated(int scoreValue)
        {
            if (isGameOver) return;

            currentScore += scoreValue;
            UpdateScoreUI();

            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.OnEnemyKilled();
            }
        }

        public void SetWave(int wave)
        {
            currentWave = wave;
            UpdateWaveUI();
            ShowWaveNotification($"WAVE {currentWave} DIMULAI!");
        }

        public void ShowWaveNotification(string message, float duration = 2.5f)
        {
            if (waveNotificationText != null)
            {
                waveNotificationText.text = message;
                waveNotificationText.gameObject.SetActive(true);
                CancelInvoke(nameof(HideWaveNotification));
                Invoke(nameof(HideWaveNotification), duration);
            }
        }

        private void HideWaveNotification()
        {
            if (waveNotificationText != null)
            {
                waveNotificationText.gameObject.SetActive(false);
            }
        }

        private void UpdateScoreUI()
        {
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {currentScore}";
            }
        }

        private void UpdateWaveUI()
        {
            if (waveText != null)
            {
                waveText.text = $"WAVE: {currentWave}";
            }
        }

        public void OnPlayerDied()
        {
            if (isGameOver) return;
            isGameOver = true;

            // Hentikan BGM & mainkan SFX Game Over
            StopBacksound();
            if (gameOverSound != null)
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(gameOverSound);
                else
                    AudioSource.PlayClipAtPoint(gameOverSound, Camera.main ? Camera.main.transform.position : transform.position, 1f);
            }

            int savedHighScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
            if (currentScore > savedHighScore)
            {
                savedHighScore = currentScore;
                PlayerPrefs.SetInt(HIGH_SCORE_KEY, savedHighScore);
                PlayerPrefs.Save();
            }

            if (finalScoreText != null)
            {
                finalScoreText.text = $"FINAL SCORE: {currentScore}";
            }

            if (highScoreText != null)
            {
                highScoreText.text = $"HIGH SCORE: {savedHighScore}";
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1.0f;
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(SceneNames.GameSyafiq);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.GameSyafiq);
            }
        }

        public void ReturnToGameSelection()
        {
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

        public void ReturnToMainMenu()
        {
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

        public void SetAudioClips(AudioClip bgm, AudioClip gameOver)
        {
            bgmClip = bgm;
            gameOverSound = gameOver;
        }
    }
}
