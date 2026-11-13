using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiverseArcade.Core
{
    /// <summary>
    /// Singleton GameManager Global untuk Multiverse Arcade.
    /// Mengelola State Machine aplikasi, navigasi global, koordinasi IArcadeGame,
    /// dan jaminan reset state / Time.timeScale saat berpindah game.
    /// Sesuai GDD Bagian 2.2 & 2.3.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private AppState _currentState = AppState.Boot;
        public AppState CurrentState => _currentState;

        [Header("Registry")]
        [SerializeField] private GameRegistry _gameRegistry;
        public GameRegistry Registry => _gameRegistry;

        public string CurrentGameId { get; private set; } = string.Empty;
        public IArcadeGame CurrentArcadeGame { get; private set; } = null;
        public bool IsPaused => _currentState == AppState.Paused;

        public static event Action<AppState> OnAppStateChanged;
        public static event Action<bool> OnPauseStateChanged;

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
            // Jika dimulai dari Bootstrap, arahkan ke MainMenu
            if (SceneManager.GetActiveScene().name == SceneNames.Bootstrap)
            {
                SetState(AppState.Boot);
                GoToMenu();
            }
            else
            {
                SyncStateWithActiveScene(SceneManager.GetActiveScene().name);
            }
        }

        private void OnEnable()
        {
            SceneLoader.OnSceneLoadStarted += HandleSceneLoadStarted;
            SceneLoader.OnSceneLoadCompleted += HandleSceneLoadCompleted;
        }

        private void OnDisable()
        {
            SceneLoader.OnSceneLoadStarted -= HandleSceneLoadStarted;
            SceneLoader.OnSceneLoadCompleted -= HandleSceneLoadCompleted;
        }

        private void Update()
        {
            // Esc shortcut untuk toggle pause saat InGame / Paused
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_currentState == AppState.InGame)
                {
                    PauseGame();
                }
                else if (_currentState == AppState.Paused)
                {
                    ResumeGame();
                }
            }
        }

        public void SetRegistry(GameRegistry registry)
        {
            _gameRegistry = registry;
        }

        public void SetState(AppState newState)
        {
            if (_currentState == newState) return;

            AppState previousState = _currentState;
            _currentState = newState;
            Debug.Log($"[GameManager] AppState: {previousState} -> {_currentState}");
            OnAppStateChanged?.Invoke(_currentState);
        }

        private void HandleSceneLoadStarted(string sceneName)
        {
            // Keluar dari InGame wajib memanggil OnGameExit() dan reset Time.timeScale = 1
            if (_currentState == AppState.InGame || _currentState == AppState.Paused)
            {
                ExitCurrentGame();
            }

            SetState(AppState.Loading);
        }

        private void HandleSceneLoadCompleted(string sceneName)
        {
            SyncStateWithActiveScene(sceneName);
        }

        private void SyncStateWithActiveScene(string sceneName)
        {
            Time.timeScale = 1.0f;

            if (sceneName == SceneNames.Bootstrap)
            {
                SetState(AppState.Boot);
                CurrentGameId = string.Empty;
            }
            else if (sceneName == SceneNames.MainMenu)
            {
                SetState(AppState.MainMenu);
                CurrentGameId = string.Empty;
                if (AudioManager.Instance != null) AudioManager.Instance.PlayDefaultMenuMusic();
            }
            else if (sceneName == SceneNames.GameSelection)
            {
                SetState(AppState.GameSelection);
                CurrentGameId = string.Empty;
                if (AudioManager.Instance != null) AudioManager.Instance.PlayDefaultMenuMusic();
            }
            else
            {
                // Merupakan Gameplay Scene
                SetState(AppState.InGame);

                if (_gameRegistry != null)
                {
                    var info = _gameRegistry.GetGameByScene(sceneName);
                    CurrentGameId = info != null ? info.id : sceneName;
                }
                else
                {
                    CurrentGameId = sceneName;
                }

                if (SaveManager.Instance != null && !string.IsNullOrEmpty(CurrentGameId))
                {
                    SaveManager.Instance.SetLastPlayedGame(CurrentGameId);
                }

                EventBus.RaiseGameStarted(CurrentGameId);
            }
        }

        /// <summary>
        /// Mendaftarkan implementasi IArcadeGame dari scene gameplay aktif.
        /// </summary>
        public void RegisterArcadeGame(IArcadeGame game)
        {
            CurrentArcadeGame = game;
            Debug.Log($"[GameManager] Game terdaftar: {game.GameId}");
            game.OnGameEnter();
        }

        /// <summary>
        /// Melepaskan pendaftaran IArcadeGame.
        /// </summary>
        public void UnregisterArcadeGame(IArcadeGame game)
        {
            if (CurrentArcadeGame == game)
            {
                CurrentArcadeGame = null;
            }
        }

        /// <summary>
        /// Memanggil OnGameExit() dan memastikan Time.timeScale = 1.0f.
        /// </summary>
        public void ExitCurrentGame()
        {
            if (CurrentArcadeGame != null)
            {
                try
                {
                    CurrentArcadeGame.OnGameExit();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[GameManager] Exception di OnGameExit: {ex.Message}");
                }
                CurrentArcadeGame = null;
            }

            Time.timeScale = 1.0f;
            CurrentGameId = string.Empty;
        }

        public void PauseGame()
        {
            if (_currentState != AppState.InGame) return;

            SetState(AppState.Paused);
            Time.timeScale = 0.0f;

            if (CurrentArcadeGame != null)
            {
                CurrentArcadeGame.OnGamePause();
            }

            EventBus.RaiseGamePaused();
            OnPauseStateChanged?.Invoke(true);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPauseOverlay(true);
            }
        }

        public void ResumeGame()
        {
            if (_currentState != AppState.Paused) return;

            Time.timeScale = 1.0f;
            SetState(AppState.InGame);

            if (CurrentArcadeGame != null)
            {
                CurrentArcadeGame.OnGameResume();
            }

            EventBus.RaiseGameResumed();
            OnPauseStateChanged?.Invoke(false);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowPauseOverlay(false);
            }
        }

        public void TogglePause()
        {
            if (_currentState == AppState.InGame) PauseGame();
            else if (_currentState == AppState.Paused) ResumeGame();
        }

        public void RestartCurrentGame()
        {
            if (CurrentArcadeGame != null)
            {
                CurrentArcadeGame.OnGameRestart();
            }

            Time.timeScale = 1.0f;
            if (UIManager.Instance != null) UIManager.Instance.ShowPauseOverlay(false);

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.RestartCurrentScene();
            }
        }

        public void LoadGame(string gameId)
        {
            if (_gameRegistry != null)
            {
                var gameInfo = _gameRegistry.GetGame(gameId);
                if (gameInfo != null)
                {
                    if (!gameInfo.isEnabled)
                    {
                        Debug.LogWarning($"[GameManager] Game '{gameInfo.displayName}' berstatus Coming Soon.");
                        return;
                    }

                    if (SceneLoader.Instance != null)
                    {
                        SceneLoader.Instance.LoadScene(gameInfo.sceneName);
                    }
                    return;
                }
            }

            // Fallback nama scene
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(gameId);
            }
        }

        public void GoToMenu()
        {
            Time.timeScale = 1.0f;
            if (UIManager.Instance != null) UIManager.Instance.ShowPauseOverlay(false);
            if (SceneLoader.Instance != null) SceneLoader.Instance.GoToMainMenu();
        }

        public void GoToSelection()
        {
            Time.timeScale = 1.0f;
            if (UIManager.Instance != null) UIManager.Instance.ShowPauseOverlay(false);
            if (SceneLoader.Instance != null) SceneLoader.Instance.GoToGameSelection();
        }

        public void QuitApplication()
        {
            Debug.Log("[GameManager] Menutup aplikasi Multiverse Arcade...");
            SetState(AppState.Quit);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
