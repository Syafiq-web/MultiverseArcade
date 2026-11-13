using MultiverseArcade.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MultiverseArcade.UI
{
    /// <summary>
    /// Mengelola interaksi pada Scene MainMenu.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("UI Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnDestroy()
        {
            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayClicked);

            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        public void OnPlayClicked()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.GoToGameSelection();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.GameSelection);
            }
        }

        public void OnQuitClicked()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitApplication();
            }
            else
            {
                Application.Quit();
            }
        }
    }
}
