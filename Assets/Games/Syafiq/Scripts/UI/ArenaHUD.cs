using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// HUD gameplay utama Arena Clash 3D (Enhanced Visuals):
    /// - Skor Tim, Timer (Pulsing di 10 detik terakhir), Countdown
    /// - Player Hero Avatar & Status Label
    /// - Smooth Player HP Bar & Red Ghost Trail Bar
    /// - Radial Cooldown Skill (Space)
    /// - Pulsing Neon Super Meter Bar & Ready Banner (E)
    /// - Aim Reticle HUD tracking cursor
    /// - Damage Number Spawner
    /// Sesuai GDD Bagian 5.11.
    /// </summary>
    public class ArenaHUD : MonoBehaviour
    {
        public static ArenaHUD Instance { get; private set; }

        [Header("Top Score & Timer Header")]
        [SerializeField] private TextMeshProUGUI _blueScoreText;
        [SerializeField] private TextMeshProUGUI _redScoreText;
        [SerializeField] private TextMeshProUGUI _timerText;

        [Header("Countdown Banner")]
        [SerializeField] private GameObject _countdownContainer;
        [SerializeField] private TextMeshProUGUI _countdownText;

        [Header("Player HUD (Bottom Left)")]
        [SerializeField] private Image _playerAvatarImg;
        [SerializeField] private Slider _playerHpSlider;
        [SerializeField] private Slider _playerHpGhostSlider;
        [SerializeField] private TextMeshProUGUI _playerHpText;
        [SerializeField] private TextMeshProUGUI _playerHeroLabel;

        [Header("Skill & Super Controls")]
        [SerializeField] private Image _skillCooldownFill;
        [SerializeField] private TextMeshProUGUI _skillKeyText;
        [SerializeField] private Slider _superMeterSlider;
        [SerializeField] private TextMeshProUGUI _superMeterText;

        [Header("Aim Reticle")]
        [SerializeField] private RectTransform _aimReticleRt;

        [Header("Damage Number Prefab")]
        [SerializeField] private GameObject _damageNumberPrefab;
        [SerializeField] private Transform _damageNumberContainer;

        private PlayerSkillController _boundSkillCtrl;
        private float _targetHpValue = 100f;
        private float _currentHpValue = 100f;
        private float _ghostHpValue = 100f;
        private float _aimReticleScale = 1f;

        private void Awake()
        {
            Instance = this;
        }

        public void PulseReticle(float scale = 1.35f)
        {
            _aimReticleScale = scale;
        }

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

            // Smooth HP Lerp
            if (_playerHpSlider != null)
            {
                _currentHpValue = Mathf.Lerp(_currentHpValue, _targetHpValue, Time.deltaTime * 15f);
                _playerHpSlider.value = _currentHpValue;
            }

            // Smooth Ghost HP Trail
            if (_playerHpGhostSlider != null)
            {
                if (_ghostHpValue > _targetHpValue)
                {
                    _ghostHpValue = Mathf.Lerp(_ghostHpValue, _targetHpValue, Time.deltaTime * 4f);
                }
                else
                {
                    _ghostHpValue = _targetHpValue;
                }
                _playerHpGhostSlider.value = _ghostHpValue;
            }

            // Pulse animation jika Super Ready
            if (_boundSkillCtrl != null && _boundSkillCtrl.IsSuperReady && _superMeterText != null)
            {
                float pulse = 1f + (Mathf.Sin(Time.time * 8f) * 0.12f);
                _superMeterText.transform.localScale = new Vector3(pulse, pulse, 1f);
            }
            else if (_superMeterText != null)
            {
                _superMeterText.transform.localScale = Vector3.one;
            }

            // Update Aim Reticle posisi mouse
            if (_aimReticleRt != null)
            {
                Vector2 mousePos = Input.mousePosition;
                _aimReticleRt.position = mousePos;

                if (_aimReticleScale > 1f)
                {
                    _aimReticleScale = Mathf.Lerp(_aimReticleScale, 1f, Time.deltaTime * 14f);
                }
                _aimReticleRt.localScale = Vector3.one * _aimReticleScale;
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

            if (_playerHpSlider != null)
            {
                _currentHpValue = _playerHpSlider.value;
                _targetHpValue = _playerHpSlider.value;
                _ghostHpValue = _playerHpSlider.value;
            }

            if (_countdownContainer != null) _countdownContainer.SetActive(false);
        }

        public void BindAvatarAndGhost(Image avatarImg, Slider ghostSlider, RectTransform reticleRt)
        {
            _playerAvatarImg = avatarImg;
            _playerHpGhostSlider = ghostSlider;
            _aimReticleRt = reticleRt;
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

        public void SetHeroAvatar(Sprite avatarSprite)
        {
            if (_playerAvatarImg != null && avatarSprite != null)
            {
                _playerAvatarImg.sprite = avatarSprite;
                _playerAvatarImg.color = Color.white;
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
                    _skillKeyText.text = "SPACE (READY)";
                    _skillKeyText.color = new Color(0.1f, 1f, 0.9f);
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
                    _superMeterText.text = "SUPER READY [E]";
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

            // Timer berubah merah & berdenyut pada 10 detik terakhir
            if (timeRemaining <= 10f)
            {
                _timerText.color = new Color(1f, 0.25f, 0.25f);
                float pulse = 1f + (Mathf.Sin(Time.time * 10f) * 0.15f);
                _timerText.transform.localScale = new Vector3(pulse, pulse, 1f);
            }
            else
            {
                _timerText.color = Color.white;
                _timerText.transform.localScale = Vector3.one;
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
                _targetHpValue = cur;
                if (_playerHpSlider != null)
                {
                    _playerHpSlider.maxValue = max;
                }
                if (_playerHpGhostSlider != null)
                {
                    _playerHpGhostSlider.maxValue = max;
                }
                if (_playerHpText != null)
                {
                    int pct = max > 0 ? Mathf.RoundToInt(((float)cur / max) * 100f) : 0;
                    _playerHpText.text = $"{cur} / {max} HP ({pct}%)";
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
