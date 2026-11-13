using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// HUD gameplay utama Arena Clash 3D: Skor Tim, Timer, Countdown, Player HP Bar, dan Damage Number Spawner.
    /// Sesuai GDD Bagian 5.11.
    /// </summary>
    public class ArenaHUD : MonoBehaviour
    {
        [Header("Top Score & Timer Header")]
        [SerializeField] private TextMeshProUGUI _blueScoreText;
        [SerializeField] private TextMeshProUGUI _redScoreText;
        [SerializeField] private TextMeshProUGUI _timerText;

        [Header("Countdown Banner")]
        [SerializeField] private GameObject _countdownContainer;
        [SerializeField] private TextMeshProUGUI _countdownText;

        [Header("Player HUD (Bottom Left)")]
        [SerializeField] private Slider _playerHpSlider;
        [SerializeField] private TextMeshProUGUI _playerHpText;

        [Header("Damage Number Prefab")]
        [SerializeField] private GameObject _damageNumberPrefab;
        [SerializeField] private Transform _damageNumberContainer;

        private void OnEnable()
        {
            ArenaEvents.OnScoreChanged += HandleScoreChanged;
            ArenaEvents.OnMatchTimerUpdated += HandleTimerUpdated;
            ArenaEvents.OnMatchStateChanged += HandleMatchStateChanged;
            ArenaEvents.OnHealthChanged += HandleHealthChanged;
            ArenaEvents.OnDamageDealt += HandleDamageDealt;
        }

        private void OnDisable()
        {
            ArenaEvents.OnScoreChanged -= HandleScoreChanged;
            ArenaEvents.OnMatchTimerUpdated -= HandleTimerUpdated;
            ArenaEvents.OnMatchStateChanged -= HandleMatchStateChanged;
            ArenaEvents.OnHealthChanged -= HandleHealthChanged;
            ArenaEvents.OnDamageDealt -= HandleDamageDealt;
        }

        public void BindControls(TextMeshProUGUI blueScore, TextMeshProUGUI redScore, TextMeshProUGUI timer,
            GameObject countdownGo, TextMeshProUGUI countdownTxt,
            Slider playerHp, TextMeshProUGUI playerHpTxt,
            GameObject dmgNumPrefab, Transform dmgNumContainer)
        {
            _blueScoreText = blueScore;
            _redScoreText = redScore;
            _timerText = timer;
            _countdownContainer = countdownGo;
            _countdownText = countdownTxt;
            _playerHpSlider = playerHp;
            _playerHpText = playerHpTxt;
            _damageNumberPrefab = dmgNumPrefab;
            _damageNumberContainer = dmgNumContainer;

            if (_countdownContainer != null) _countdownContainer.SetActive(false);
        }

        private void HandleScoreChanged(int blue, int red)
        {
            if (_blueScoreText != null) _blueScoreText.text = blue.ToString();
            if (_redScoreText != null) _redScoreText.text = red.ToString();
        }

        private void HandleTimerUpdated(float timeRemaining)
        {
            if (_timerText == null) return;

            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            _timerText.text = $"{minutes:00}:{seconds:00}";

            // Timer berubah merah pada 10 detik terakhir (GDD 5.11)
            if (timeRemaining <= 10f)
            {
                _timerText.color = new Color(1f, 0.25f, 0.25f);
            }
            else
            {
                _timerText.color = Color.white;
            }
        }

        private void HandleMatchStateChanged(MatchState state)
        {
            if (state == MatchState.Countdown)
            {
                if (_countdownContainer != null)
                {
                    _countdownContainer.SetActive(true);
                    StartCoroutine(CountdownAnimationRoutine());
                }
            }
            else if (state == MatchState.Playing)
            {
                if (_countdownContainer != null) _countdownContainer.SetActive(false);
            }
        }

        private IEnumerator CountdownAnimationRoutine()
        {
            string[] steps = { "3", "2", "1", "BATTLE!" };
            foreach (var step in steps)
            {
                if (_countdownText != null) _countdownText.text = step;
                yield return new WaitForSeconds(0.75f);
            }
            if (_countdownContainer != null) _countdownContainer.SetActive(false);
        }

        private void HandleHealthChanged(Health h, int cur, int max)
        {
            if (h != null && h.IsPlayer)
            {
                if (_playerHpSlider != null)
                {
                    _playerHpSlider.maxValue = max;
                    _playerHpSlider.value = cur;
                }
                if (_playerHpText != null)
                {
                    _playerHpText.text = $"{cur} / {max} HP";
                }
            }
        }

        private void HandleDamageDealt(Vector3 worldPos, int damage, bool isCrit)
        {
            if (_damageNumberPrefab == null) return;

            Transform parent = _damageNumberContainer != null ? _damageNumberContainer : transform;
            GameObject dmgGo = Instantiate(_damageNumberPrefab, parent);
            DamageNumber dn = dmgGo.GetComponent<DamageNumber>();
            if (dn != null)
            {
                dn.Spawn(worldPos, damage, isCrit);
            }
        }
    }
}
