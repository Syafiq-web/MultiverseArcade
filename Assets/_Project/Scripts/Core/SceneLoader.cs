using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiverseArcade.Core
{
    /// <summary>
    /// Sistem pemuatan Scene terpusat.
    /// Memastikan Time.timeScale selalu dinormalkan kembali ke 1.0f saat perpindahan scene.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        public static event Action<string> OnSceneLoadStarted;
        public static event Action<string> OnSceneLoadCompleted;

        private bool _isLoading = false;

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

        /// <summary>
        /// Memuat scene berdasarkan nama konstanta.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            if (_isLoading)
            {
                Debug.LogWarning($"[SceneLoader] Sedang memuat scene lain. Permintaan memuat '{sceneName}' diabaikan.");
                return;
            }

            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            _isLoading = true;

            // Reset Time.timeScale agar game yang sempat di-pause tidak macet di scene berikutnya
            Time.timeScale = 1.0f;

            OnSceneLoadStarted?.Invoke(sceneName);
            Debug.Log($"[SceneLoader] Memulai pemuatan scene: {sceneName}");

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            while (!asyncOp.isDone)
            {
                yield return null;
            }

            _isLoading = false;
            OnSceneLoadCompleted?.Invoke(sceneName);
            Debug.Log($"[SceneLoader] Berhasil memuat scene: {sceneName}");
        }

        // Shorthand navigation helpers
        public void GoToMainMenu() => LoadScene(SceneNames.MainMenu);
        public void GoToGameSelection() => LoadScene(SceneNames.GameSelection);
        public void RestartCurrentScene() => LoadScene(SceneManager.GetActiveScene().name);
    }
}
