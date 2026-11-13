using System;
using UnityEngine;

namespace MultiverseArcade.Core
{
    /// <summary>
    /// Singleton GameManager Global untuk Multiverse Arcade.
    /// Berperan sebagai koordinator global (state permainan, navigasi, dan inisialisasi).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public string CurrentActiveGame { get; private set; } = string.Empty;
        public bool IsPaused { get; private set; } = false;

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

        private void OnEnable()
        {
            SceneLoader.OnSceneLoadStarted += HandleSceneLoadStarted;
        }

        private void OnDisable()
        {
            SceneLoader.OnSceneLoadStarted -= HandleSceneLoadStarted;
        }

        private void HandleSceneLoadStarted(string sceneName)
        {
            // Reset pause jika pindah scene
            SetPause(false);

            if (sceneName == SceneNames.MainMenu || sceneName == SceneNames.GameSelection)
            {
                CurrentActiveGame = string.Empty;
            }
            else
            {
                CurrentActiveGame = sceneName;
            }
        }

        /// <summary>
        /// Mengatur kondisi pause permainan dan menyesuaikan Time.timeScale.
        /// </summary>
        public void SetPause(bool pause)
        {
            IsPaused = pause;
            Time.timeScale = pause ? 0.0f : 1.0f;
            OnPauseStateChanged?.Invoke(IsPaused);
            Debug.Log($"[GameManager] Pause state diubah ke: {IsPaused} (Time.timeScale: {Time.timeScale})");
        }

        /// <summary>
        /// Toggle status pause saat ini.
        /// </summary>
        public void TogglePause()
        {
            SetPause(!IsPaused);
        }

        /// <summary>
        /// Keluar dari aplikasi.
        /// </summary>
        public void QuitApplication()
        {
            Debug.Log("[GameManager] Keluar dari aplikasi.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
