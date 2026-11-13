using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MultiverseArcade.Core
{
    /// <summary>
    /// Sistem pemuatan Scene async terpusat dengan proteksi anti-stuck (_isLoading auto-reset),
    /// progress tracking untuk Loading Screen, dan jaminan Time.timeScale kembali normal 1.0f.
    /// Sesuai GDD Bagian 2.2 & 2.3.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        public static event Action<string> OnSceneLoadStarted;
        public static event Action<float> OnSceneLoadProgress;
        public static event Action<string> OnSceneLoadCompleted;

        private bool _isLoading = false;
        public bool IsLoading => _isLoading;

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
        /// Memuat scene secara async dengan progress tracking.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            Time.timeScale = 1.0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (_isLoading)
            {
                Debug.LogWarning($"[SceneLoader] Status loading aktif, mereset paksa untuk memuat '{sceneName}'...");
                ForceResetLoading();
            }

            EventBus.RaiseSceneLoadRequested(sceneName);
            StartCoroutine(LoadSceneRoutine(sceneName));
        }

        private IEnumerator LoadSceneRoutine(string sceneName)
        {
            _isLoading = true;
            Time.timeScale = 1.0f;

            OnSceneLoadStarted?.Invoke(sceneName);
            OnSceneLoadProgress?.Invoke(0f);
            Debug.Log($"[SceneLoader] Memulai pemuatan scene async: {sceneName}");

            // Tampilkan loading screen via UIManager jika ada
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowLoadingScreen(true);
            }

            yield return new WaitForSecondsRealtime(0.05f);

            AsyncOperation asyncOp = null;
            try
            {
                asyncOp = SceneManager.LoadSceneAsync(sceneName);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SceneLoader] Gagal memuat scene '{sceneName}': {ex.Message}");
                _isLoading = false;
                if (UIManager.Instance != null) UIManager.Instance.ShowLoadingScreen(false);
                yield break;
            }

            if (asyncOp == null)
            {
                SceneManager.LoadScene(sceneName);
                _isLoading = false;
                if (UIManager.Instance != null) UIManager.Instance.ShowLoadingScreen(false);
                yield break;
            }

            float timeout = 10.0f;
            float timer = 0f;

            while (!asyncOp.isDone)
            {
                float progress = Mathf.Clamp01(asyncOp.progress / 0.9f);
                OnSceneLoadProgress?.Invoke(progress);
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateLoadingProgress(progress);
                }

                timer += Time.unscaledDeltaTime;
                if (timer > timeout)
                {
                    Debug.LogWarning($"[SceneLoader] Timeout memuat scene '{sceneName}', membatalkan status loading.");
                    break;
                }
                yield return null;
            }

            OnSceneLoadProgress?.Invoke(1.0f);
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateLoadingProgress(1.0f);
                yield return new WaitForSecondsRealtime(0.1f);
                UIManager.Instance.ShowLoadingScreen(false);
            }

            _isLoading = false;
            Time.timeScale = 1.0f;
            OnSceneLoadCompleted?.Invoke(sceneName);
            Debug.Log($"[SceneLoader] Berhasil memuat scene: {sceneName}");
        }

        public void GoToMainMenu() => LoadScene(SceneNames.MainMenu);
        public void GoToGameSelection() => LoadScene(SceneNames.GameSelection);
        public void RestartCurrentScene() => LoadScene(SceneManager.GetActiveScene().name);
    }
}
