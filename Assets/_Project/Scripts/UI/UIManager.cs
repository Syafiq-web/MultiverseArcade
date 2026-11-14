using UnityEngine;
using UnityEngine.UI;

namespace MultiverseArcade.Core
{
    /// <summary>
    /// UI Manager global untuk mengelola Canvas persistent (Loading Screen & Global Pause Overlay).
    /// Sesuai GDD Bagian 2.2.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Global Overlays")]
        [SerializeField] private GameObject _pauseOverlayPanel;
        [SerializeField] private GameObject _loadingScreenPanel;
        [SerializeField] private Slider _loadingProgressBar;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_pauseOverlayPanel != null) _pauseOverlayPanel.SetActive(false);
            if (_loadingScreenPanel != null) _loadingScreenPanel.SetActive(false);
        }

        public void SetReferences(GameObject pauseOverlay, GameObject loadingScreen, Slider progressBar)
        {
            _pauseOverlayPanel = pauseOverlay;
            _loadingScreenPanel = loadingScreen;
            _loadingProgressBar = progressBar;

            if (_pauseOverlayPanel != null) _pauseOverlayPanel.SetActive(false);
            if (_loadingScreenPanel != null) _loadingScreenPanel.SetActive(false);
        }

        public bool HasPauseOverlay => _pauseOverlayPanel != null;

        public void ShowPauseOverlay(bool show)
        {
            if (_pauseOverlayPanel != null)
            {
                _pauseOverlayPanel.SetActive(show);
            }
        }

        public void ShowLoadingScreen(bool show)
        {
            if (_loadingScreenPanel != null)
            {
                _loadingScreenPanel.SetActive(show);
                if (show && _loadingProgressBar != null)
                {
                    _loadingProgressBar.value = 0f;
                }
            }
        }

        public void UpdateLoadingProgress(float progress)
        {
            if (_loadingProgressBar != null)
            {
                _loadingProgressBar.value = Mathf.Clamp01(progress);
            }
        }
    }
}
