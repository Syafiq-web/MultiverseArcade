using MultiverseArcade.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MultiverseArcade.UI
{
    /// <summary>
    /// Mengelola interaksi pada Scene GameSelection (pemilihan 5 game).
    /// </summary>
    public class GameSelectionUI : MonoBehaviour
    {
        [Header("Game Selection Buttons")]
        [SerializeField] private Button btnGameSyafiq;
        [SerializeField] private Button btnGameJaya;
        [SerializeField] private Button btnGamePrima;
        [SerializeField] private Button btnGameBrili;
        [SerializeField] private Button btnGameRisa;

        [Header("Navigation Buttons")]
        [SerializeField] private Button btnBackToMainMenu;

        private void Start()
        {
            if (btnGameSyafiq != null) btnGameSyafiq.onClick.AddListener(() => LaunchGame(SceneNames.GameSyafiq));
            if (btnGameJaya != null)   btnGameJaya.onClick.AddListener(() => LaunchGame(SceneNames.GameJaya));
            if (btnGamePrima != null)  btnGamePrima.onClick.AddListener(() => LaunchGame(SceneNames.GamePrima));
            if (btnGameBrili != null)  btnGameBrili.onClick.AddListener(() => LaunchGame(SceneNames.GameBrili));
            if (btnGameRisa != null)   btnGameRisa.onClick.AddListener(() => LaunchGame(SceneNames.GameRisa));

            if (btnBackToMainMenu != null)
                btnBackToMainMenu.onClick.AddListener(OnBackClicked);
        }

        private void OnDestroy()
        {
            if (btnGameSyafiq != null) btnGameSyafiq.onClick.RemoveAllListeners();
            if (btnGameJaya != null)   btnGameJaya.onClick.RemoveAllListeners();
            if (btnGamePrima != null)  btnGamePrima.onClick.RemoveAllListeners();
            if (btnGameBrili != null)  btnGameBrili.onClick.RemoveAllListeners();
            if (btnGameRisa != null)   btnGameRisa.onClick.RemoveAllListeners();
            if (btnBackToMainMenu != null) btnBackToMainMenu.onClick.RemoveAllListeners();
        }

        public void LaunchGame(string gameSceneName)
        {
            Debug.Log($"[GameSelectionUI] Membuka game: {gameSceneName}");
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(gameSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
            }
        }

        public void OnBackClicked()
        {
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
