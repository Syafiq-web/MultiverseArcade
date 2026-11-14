using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MultiverseArcade.Core;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// UI Modal Pemilihan Hero (Blaze / Bulwark) dan Tingkat Kesulitan Bot (Easy / Normal / Hard)
    /// sebelum pertandingan Arena Clash 3D dimulai.
    /// Pilihan disimpan ke SaveManager. Rematch tetap memakai konfigurasi terakhir.
    /// Sesuai GDD Bagian 5.4, 5.9, dan 5.11.
    /// </summary>
    public class HeroSelectionUI : MonoBehaviour
    {
        [Header("Hero Data References")]
        [SerializeField] private SO_Hero _heroBlaze;
        [SerializeField] private SO_Hero _heroBulwark;

        [Header("Bot Difficulties")]
        [SerializeField] private SO_BotDifficulty _diffEasy;
        [SerializeField] private SO_BotDifficulty _diffNormal;
        [SerializeField] private SO_BotDifficulty _diffHard;

        [Header("Hero Cards")]
        [SerializeField] private Button _blazeCardBtn;
        [SerializeField] private Image _blazeHighlight;
        [SerializeField] private Button _bulwarkCardBtn;
        [SerializeField] private Image _bulwarkHighlight;

        [Header("Difficulty Buttons")]
        [SerializeField] private Button _easyBtn;
        [SerializeField] private Image _easyBg;
        [SerializeField] private Button _normalBtn;
        [SerializeField] private Image _normalBg;
        [SerializeField] private Button _hardBtn;
        [SerializeField] private Image _hardBg;

        [Header("Action")]
        [SerializeField] private Button _startBattleBtn;

        private SO_Hero _selectedHero;
        private SO_BotDifficulty _selectedDifficulty;

        public SO_Hero SelectedHero => _selectedHero;
        public SO_BotDifficulty SelectedDifficulty => _selectedDifficulty;

        private readonly Color SelectedCardColor = new Color(0f, 0.9f, 1f, 0.9f);
        private readonly Color UnselectedCardColor = new Color(0.15f, 0.2f, 0.3f, 0.5f);

        private readonly Color DiffActiveColor = new Color(0f, 0.75f, 0.65f);
        private readonly Color DiffInactiveColor = new Color(0.15f, 0.2f, 0.3f);

        private void Awake()
        {
            _selectedHero = _heroBlaze;
            _selectedDifficulty = _diffNormal;

            if (_blazeCardBtn != null) _blazeCardBtn.onClick.AddListener(() => SelectHero(_heroBlaze));
            if (_bulwarkCardBtn != null) _bulwarkCardBtn.onClick.AddListener(() => SelectHero(_heroBulwark));

            if (_easyBtn != null) _easyBtn.onClick.AddListener(() => SelectDifficulty(_diffEasy));
            if (_normalBtn != null) _normalBtn.onClick.AddListener(() => SelectDifficulty(_diffNormal));
            if (_hardBtn != null) _hardBtn.onClick.AddListener(() => SelectDifficulty(_diffHard));

            if (_startBattleBtn != null) _startBattleBtn.onClick.AddListener(OnStartBattleClicked);
        }

        private void Start()
        {
            LoadSavedPreferences();
            UpdateUI();
        }

        public void BindControls(SO_Hero blaze, SO_Hero bulwark, SO_BotDifficulty easy, SO_BotDifficulty normal, SO_BotDifficulty hard,
            Button blazeBtn, Image blazeHl, Button bulwarkBtn, Image bulwarkHl,
            Button easyBtn, Image easyBg, Button normalBtn, Image normalBg, Button hardBtn, Image hardBg,
            Button startBtn)
        {
            _heroBlaze = blaze;
            _heroBulwark = bulwark;
            _diffEasy = easy;
            _diffNormal = normal;
            _diffHard = hard;

            _blazeCardBtn = blazeBtn;
            _blazeHighlight = blazeHl;
            _bulwarkCardBtn = bulwarkBtn;
            _bulwarkHighlight = bulwarkHl;

            _easyBtn = easyBtn;
            _easyBg = easyBg;
            _normalBtn = normalBtn;
            _normalBg = normalBg;
            _hardBtn = hardBtn;
            _hardBg = hardBg;

            _startBattleBtn = startBtn;

            _selectedHero = _heroBlaze;
            _selectedDifficulty = _diffNormal;

            if (_blazeCardBtn != null)
            {
                _blazeCardBtn.onClick.RemoveAllListeners();
                _blazeCardBtn.onClick.AddListener(() => SelectHero(_heroBlaze));
            }
            if (_bulwarkCardBtn != null)
            {
                _bulwarkCardBtn.onClick.RemoveAllListeners();
                _bulwarkCardBtn.onClick.AddListener(() => SelectHero(_heroBulwark));
            }

            if (_easyBtn != null)
            {
                _easyBtn.onClick.RemoveAllListeners();
                _easyBtn.onClick.AddListener(() => SelectDifficulty(_diffEasy));
            }
            if (_normalBtn != null)
            {
                _normalBtn.onClick.RemoveAllListeners();
                _normalBtn.onClick.AddListener(() => SelectDifficulty(_diffNormal));
            }
            if (_hardBtn != null)
            {
                _hardBtn.onClick.RemoveAllListeners();
                _hardBtn.onClick.AddListener(() => SelectDifficulty(_diffHard));
            }

            if (_startBattleBtn != null)
            {
                _startBattleBtn.onClick.RemoveAllListeners();
                _startBattleBtn.onClick.AddListener(OnStartBattleClicked);
            }

            LoadSavedPreferences();
            UpdateUI();
        }

        public void ShowPanel()
        {
            gameObject.SetActive(true);
            UpdateUI();
        }

        public void HidePanel()
        {
            gameObject.SetActive(false);
        }

        public void SelectHero(SO_Hero hero)
        {
            if (hero == null) return;
            _selectedHero = hero;
            PlayClick();
            UpdateUI();
        }

        public void SelectDifficulty(SO_BotDifficulty diff)
        {
            if (diff == null) return;
            _selectedDifficulty = diff;
            PlayClick();
            UpdateUI();
        }

        private void UpdateUI()
        {
            // Highlight Kartu Hero
            bool isBlaze = (_selectedHero == _heroBlaze);
            if (_blazeHighlight != null) _blazeHighlight.color = isBlaze ? SelectedCardColor : UnselectedCardColor;
            if (_bulwarkHighlight != null) _bulwarkHighlight.color = !isBlaze ? SelectedCardColor : UnselectedCardColor;

            // Highlight Tingkat Kesulitan
            if (_easyBg != null) _easyBg.color = (_selectedDifficulty == _diffEasy) ? DiffActiveColor : DiffInactiveColor;
            if (_normalBg != null) _normalBg.color = (_selectedDifficulty == _diffNormal) ? DiffActiveColor : DiffInactiveColor;
            if (_hardBg != null) _hardBg.color = (_selectedDifficulty == _diffHard) ? DiffActiveColor : DiffInactiveColor;
        }

        private void OnStartBattleClicked()
        {
            PlayClick();
            SavePreferences();
            HidePanel();

            if (ArenaMatchManager.Instance != null)
            {
                ArenaMatchManager.Instance.ApplyMatchConfiguration(_selectedHero, _selectedDifficulty);
                ArenaMatchManager.Instance.StartMatchCountdown();
            }
        }

        private void LoadSavedPreferences()
        {
            // Ambil pilihan terakhir dari PlayerPrefs / SaveManager jika tersedia
            string savedHeroId = PlayerPrefs.GetString("ArenaClash_LastHero", "blaze");
            string savedDiff = PlayerPrefs.GetString("ArenaClash_LastDiff", "Normal");

            _selectedHero = (savedHeroId == "bulwark" && _heroBulwark != null) ? _heroBulwark : _heroBlaze;

            if (savedDiff == "Easy" && _diffEasy != null) _selectedDifficulty = _diffEasy;
            else if (savedDiff == "Hard" && _diffHard != null) _selectedDifficulty = _diffHard;
            else _selectedDifficulty = _diffNormal != null ? _diffNormal : _diffEasy;
        }

        private void SavePreferences()
        {
            if (_selectedHero != null)
            {
                PlayerPrefs.SetString("ArenaClash_LastHero", _selectedHero.heroId);
            }
            if (_selectedDifficulty != null)
            {
                PlayerPrefs.SetString("ArenaClash_LastDiff", _selectedDifficulty.difficultyName);
            }
            PlayerPrefs.Save();
        }

        private void PlayClick()
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonClick();
            }
        }
    }
}
