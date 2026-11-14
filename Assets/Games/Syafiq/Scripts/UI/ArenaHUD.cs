using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// HUD gameplay utama Arena Clash 3D:
    /// - Skor Tim, Timer, Countdown
    /// - Player HP Bar
    /// - Radial Cooldown Skill (Space)
    /// - Super Meter Slider & Status Ready (E)
    /// - Damage Number Spawner
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
        [SerializeField] private TextMeshProUGUI _playerHeroLabel;

        [Header("Skill & Super Controls")]
        [SerializeField] private Image _skillCooldownFill;
        [SerializeField] private TextMeshProUGUI _skillKeyText;
        [SerializeField] private Slider _superMeterSlider;
        [SerializeField] private TextMeshProUGUI _superMeterText;

        [Header("Damage Number Prefab")]
        [SerializeField] private GameObject _damageNumberPrefab;
        [SerializeField] private Transform _damageNumberContainer;

        private PlayerSkillController _boundSkillCtrl;

        private void OnEnable()
        {
            ArenaEvents.OnScoreChanged += HandleScoreChanged;
            ArenaEvents.OnMatchTimerUpdated += HandleTimerUpdated;
            ArenaEvents.OnMatchStateChanged += HandleMatchStateChanged;
            ArenaEvents.OnHealthChanged += HandleHealthChanged;
            ArenaEvents.OnDamageDealt += HandleDamageDealt;

            BindPlayerSkillEvents();
        }

        private void OnDisable()
        {
            ArenaEvents.OnScoreChanged -= HandleScoreChanged;
            ArenaEvents.OnMatchTimerUpdated -= HandleTimerUpdated;
            ArenaEvents.OnMatchStateChanged -= HandleMatchStateChanged;
            ArenaEvents.OnHealthChanged -= HandleHealthChanged;
            ArenaEvents.OnDamageDealt -= HandleDamageDealt;

            UnbindPlayerSkillEvents();
        }

        private void Update()
        {
            // Polling fallback jika skill controller belum terikat saat OnEnable
            if (_boundSkillCtrl == null)
            {
                BindPlayerSkillEvents();
            }
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

        public void BindSkillAndSuperControls(Image skillFill, TextMeshProUGUI skillText,
            Slider superSlider, TextMeshProUGUI superText, TextMeshProUGUI heroLabel)
        {
            _skillCooldownFill = skillFill;
            _skillKeyText = skillText;
            _superMeterSlider = superSlider;
            _superMeterText = superText;
            _playerHeroLabel = heroLabel;

            if (_skillCooldownFill != null) _skillCooldownFill.fillAmount = 0f;
            if (_superMeterSlider != null) _superMeterSlider.value = 0f;
            if (_superMeterText != null) _superMeterText.text = "SUPER: 0%";
        }

        public void SetHeroLabel(string heroName)
        {
            if (_playerHeroLabel != null)
            {
                _playerHeroLabel.text = $"{heroName.ToUpper()} (YOU)";
            }
        }

        private void BindPlayerSkillEvents()
        {
            if (_boundSkillCtrl != null) return;

            _boundSkillCtrl = PlayerSkillController.Instance;
            if (_boundSkillCtrl == null)
            {
                _boundSkillCtrl = FindObjectOfType<PlayerSkillController>();
            }

            if (_boundSkillCtrl != null)
            {
                _boundSkillCtrl.OnSkillCooldownChanged += HandleSkillCooldownChanged;
                _boundSkillCtrl.OnSuperChargeChanged += HandleSuperChargeChanged;
                _boundSkillCtrl.OnSuperReadyStateChanged += HandleSuperReadyChanged;

                HandleSkillCooldownChanged(_boundSkillCtrl.SkillCooldownRemaining, _boundSkillCtrl.SkillCooldownMax);
                HandleSuperChargeChanged(_boundSkillCtrl.SuperChargePercent);
                HandleSuperReadyChanged(_boundSkillCtrl.IsSuperReady);
            }
        }

        private void UnbindPlayerSkillEvents()
        {
            if (_boundSkillCtrl != null)
            {
                _boundSkillCtrl.OnSkillCooldownChanged -= HandleSkillCooldownChanged;
                _boundSkillCtrl.OnSuperChargeChanged -= HandleSuperChargeChanged;
                _boundSkillCtrl.OnSuperReadyStateChanged -= HandleSuperReadyChanged;
                _boundSkillCtrl = null;
            }
        }

        private void HandleSkillCooldownChanged(float remaining, float max)
        {
            if (_skillCooldownFill != null)
            {
                _skillCooldownFill.fillAmount = max > 0f ? (remaining / max) : 0f;
            }

            if (_skillKeyText != null)
            {
                if (remaining > 0f)
                {
                    _skillKeyText.text = $"{remaining:F1}s";
                    _skillKeyText.color = new Color(1f, 0.6f, 0.2f);
                }
                else
                {
                    _skillKeyText.text = "SPACE";
                    _skillKeyText.color = Color.white;
                }
            }
        }

        private void HandleSuperChargeChanged(float percent)
        {
            if (_superMeterSlider != null)
            {
                _superMeterSlider.value = percent;
            }

            if (_superMeterText != null && percent < 100f)
            {
                _superMeterText.text = $"SUPER: {Mathf.FloorToInt(percent)}%";
                _superMeterText.color = new Color(0.7f, 0.85f, 1f);
            }
        }

        private void HandleSuperReadyChanged(bool isReady)
        {
            if (_superMeterText != null)
            {
                if (isReady)
                {
                    _superMeterText.text = "⚡ SUPER READY! (E)";
                    _superMeterText.color = new Color(1f, 0.9f, 0.1f);
                }
                else
                {
                    float cur = _boundSkillCtrl != null ? _boundSkillCtrl.SuperChargePercent : 0f;
                    _superMeterText.text = $"SUPER: {Mathf.FloorToInt(cur)}%";
                    _superMeterText.color = new Color(0.7f, 0.85f, 1f);
                }
            }
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
