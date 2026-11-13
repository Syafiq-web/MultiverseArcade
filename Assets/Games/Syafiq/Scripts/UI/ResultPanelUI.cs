using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiverseArcade.Core;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Panel hasil akhir pertandingan (VICTORY / DEFEAT / DRAW) lengkap dengan statistik,
    /// tombol Rematch, dan tombol Switch Game ke Hub.
    /// Sesuai GDD Bagian 5.11.
    /// </summary>
    public class ResultPanelUI : MonoBehaviour
    {
        [Header("Banner & Texts")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _statsText;

        [Header("Buttons")]
        [SerializeField] private Button _rematchButton;
        [SerializeField] private Button _switchGameButton;

        private void Awake()
        {
            if (_rematchButton != null) _rematchButton.onClick.AddListener(OnRematchClicked);
            if (_switchGameButton != null) _switchGameButton.onClick.AddListener(OnSwitchGameClicked);
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            ArenaEvents.OnMatchFinished += HandleMatchFinished;
        }

        private void OnDisable()
        {
            ArenaEvents.OnMatchFinished -= HandleMatchFinished;
        }

        public void BindControls(TextMeshProUGUI title, TextMeshProUGUI score, TextMeshProUGUI stats, Button rematch, Button switchGame)
        {
            _titleText = title;
            _scoreText = score;
            _statsText = stats;
            _rematchButton = rematch;
            _switchGameButton = switchGame;

            if (_rematchButton != null)
            {
                _rematchButton.onClick.RemoveAllListeners();
                _rematchButton.onClick.AddListener(OnRematchClicked);
            }
            if (_switchGameButton != null)
            {
                _switchGameButton.onClick.RemoveAllListeners();
                _switchGameButton.onClick.AddListener(OnSwitchGameClicked);
            }
        }

        private void HandleMatchFinished(MatchResult result, int blueScore, int redScore)
        {
            gameObject.SetActive(true);

            if (_titleText != null)
            {
                switch (result)
                {
                    case MatchResult.BlueVictory:
                        _titleText.text = "VICTORY!";
                        _titleText.color = new Color(0f, 1f, 0.95f);
                        break;
                    case MatchResult.RedVictory:
                        _titleText.text = "DEFEAT!";
                        _titleText.color = new Color(1f, 0.3f, 0.3f);
                        break;
                    case MatchResult.Draw:
                        _titleText.text = "DRAW MATCH!";
                        _titleText.color = new Color(1f, 0.85f, 0.2f);
                        break;
                }
            }

            if (_scoreText != null)
            {
                _scoreText.text = $"<color=#00E5FF>BLUE  {blueScore}</color>   —   <color=#FF5252>{redScore}  RED</color>";
            }

            if (_statsText != null)
            {
                _statsText.text = "Target Eliminasi: 10   |   Format: 3v3 Team Battle";
            }
        }

        private void OnRematchClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            gameObject.SetActive(false);

            if (ArenaMatchManager.Instance != null)
            {
                ArenaMatchManager.Instance.RestartMatch();
            }
        }

        private void OnSwitchGameClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToSelection();
            }
            else if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.GoToGameSelection();
            }
        }
    }
}
