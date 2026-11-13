using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiverseArcade.Core;

namespace MultiverseArcade.UI
{
    /// <summary>
    /// Controller UI untuk Game Selection.
    /// Membaca GameRegistry dan menampilkan 5 kartu game.
    /// Hanya game dengan isEnabled = true yang dapat dimainkan (Arena Clash); sisanya Coming Soon.
    /// Sesuai GDD Bagian 2.5.
    /// </summary>
    public class GameSelectionUI : MonoBehaviour
    {
        [Header("Registry Data")]
        [SerializeField] private GameRegistry _registry;

        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Card Container (Optional Dynamic Spawn)")]
        [SerializeField] private Transform _cardsContainer;
        [SerializeField] private GameObject _cardPrefab;

        [Header("Pre-built Cards (Fallback / Direct Binding)")]
        [SerializeField] private List<GameCardView> _prebuiltCards = new List<GameCardView>();

        private void Start()
        {
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }

            // Dapatkan registry dari GameManager jika belum diisi
            if (_registry == null && GameManager.Instance != null)
            {
                _registry = GameManager.Instance.Registry;
            }

            PopulateCards();
        }

        public void BindBackButton(Button btn)
        {
            _backButton = btn;
            if (_backButton != null)
            {
                _backButton.onClick.RemoveAllListeners();
                _backButton.onClick.AddListener(OnBackClicked);
            }
        }

        public void SetRegistry(GameRegistry registry)
        {
            _registry = registry;
            PopulateCards();
        }

        public void AddPrebuiltCard(GameCardView card)
        {
            if (card != null && !_prebuiltCards.Contains(card))
            {
                _prebuiltCards.Add(card);
            }
        }

        public void PopulateCards()
        {
            if (_registry == null || _registry.games == null) return;

            // Jika ada pre-built cards di scene, isi datanya langsung
            for (int i = 0; i < _registry.games.Count; i++)
            {
                var info = _registry.games[i];
                if (info == null) continue;

                if (i < _prebuiltCards.Count && _prebuiltCards[i] != null)
                {
                    _prebuiltCards[i].Setup(info, OnPlayGameRequested);
                }
            }
        }

        private void OnPlayGameRequested(GameInfo info)
        {
            if (info == null || !info.isEnabled) return;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }

            Debug.Log($"[GameSelectionUI] Memulai game: {info.displayName} ({info.sceneName})");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoadGame(info.id);
            }
            else if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(info.sceneName);
            }
        }

        private void OnBackClicked()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToMenu();
            }
            else if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.GoToMainMenu();
            }
        }
    }
}
