using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiverseArcade.Core
{
    /// <summary>
    /// Sistem pemuatan Scene terpusat dengan proteksi anti-stuck (_isLoading auto-reset)
    /// dan jaminan Time.timeScale kembali normal 1.0f setiap kali perpindahan scene.
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
            _isLoading = false;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _isLoading = false;
            Time.timeScale = 1.0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        /// <summary>
        /// Paksa reset status loading jika terjadi hambatan pada scene sebelumnya.
        /// </summary>
        public void ForceResetLoading()
        {
            _isLoading = false;
            Time.timeScale = 1.0f;
            StopAllCoroutines();
        }

        /// <summary>
        /// Memuat scene berdasarkan nama konstanta.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            // Pastikan timeScale selalu 1 dan kursor aktif sebelum memuat scene
            Time.timeScale = 1.0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (_isLoading)
            {
                Debug.LogWarning($"[SceneLoader] Status loading aktif, mereset paksa untuk memuat '{sceneName}'...");
                ForceResetLoading();
            }

            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            _isLoading = true;
            Time.timeScale = 1.0f;

            OnSceneLoadStarted?.Invoke(sceneName);
            Debug.Log($"[SceneLoader] Memulai pemuatan scene: {sceneName}");

            AsyncOperation asyncOp = null;
            try
            {
                asyncOp = SceneManager.LoadSceneAsync(sceneName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneLoader] Gagal memuat scene '{sceneName}': {ex.Message}");
                _isLoading = false;
                yield break;
            }

            if (asyncOp == null)
            {
                // Fallback synchronous jika async gagal
                SceneManager.LoadScene(sceneName);
                _isLoading = false;
                yield break;
            }

            float timeout = 8.0f;
            float timer = 0f;

            while (!asyncOp.isDone)
            {
                timer += Time.unscaledDeltaTime;
                if (timer > timeout)
                {
                    Debug.LogWarning($"[SceneLoader] Timeout memuat scene '{sceneName}', membatalkan status loading.");
                    _isLoading = false;
                    yield break;
                }
                yield return null;
            }

            _isLoading = false;
            Time.timeScale = 1.0f;
            OnSceneLoadCompleted?.Invoke(sceneName);
            Debug.Log($"[SceneLoader] Berhasil memuat scene: {sceneName}");
        }

        // Shorthand navigation helpers
        public void GoToMainMenu() => LoadScene(SceneNames.MainMenu);
        public void GoToGameSelection() => LoadScene(SceneNames.GameSelection);
        public void RestartCurrentScene() => LoadScene(SceneManager.GetActiveScene().name);
    }
}
