using System;
using UnityEngine;
using MultiverseArcade.Core;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Pengelola eksekusi skill aktif (Space) dan kemampuan super (E),
    /// tracking super meter (+10% per 20 dmg, +25% per kill), serta komunikasi event HUD.
    /// Sesuai GDD Bagian 5.4, 5.5, dan 5.6.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(PlayerCombat))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(StatusEffectController))]
    public class PlayerSkillController : MonoBehaviour
    {
        public static PlayerSkillController Instance { get; private set; }

        [Header("Hero Config")]
        [SerializeField] private SO_Hero _heroData;

        [Header("Audio")]
        [SerializeField] private AudioClip _skillSound;
        [SerializeField] private AudioClip _superSound;

        private PlayerController _playerCtrl;
        private PlayerCombat _playerCombat;
        private Health _health;
        private StatusEffectController _statusEffects;

        private float _skillCooldownTimer = 0f;
        private float _superCharge = 0f; // 0 s.d. 100
        private bool _canAct = true;
        private bool _isSuperReady = false;

        public event Action<float, float> OnSkillCooldownChanged; // currentTimer, maxCooldown
        public event Action<float> OnSuperChargeChanged; // currentPercent (0-100)
        public event Action<bool> OnSuperReadyStateChanged;

        public float SuperChargePercent => _superCharge;
        public bool IsSuperReady => _isSuperReady;
        public float SkillCooldownRemaining => _skillCooldownTimer;
        public float SkillCooldownMax => _heroData != null ? _heroData.skillCooldown : 6.0f;
        public string SkillName => _heroData != null ? _heroData.skillName : "Skill";
        public string SuperName => _heroData != null ? _heroData.superName : "Super";

        private void Awake()
        {
            if (_health != null && _health.IsPlayer)
            {
                Instance = this;
            }

            _playerCtrl = GetComponent<PlayerController>();
            _playerCombat = GetComponent<PlayerCombat>();
            _health = GetComponent<Health>();
            _statusEffects = GetComponent<StatusEffectController>();
        }

        private void OnEnable()
        {
            ArenaEvents.OnMatchStateChanged += HandleMatchStateChanged;
            ArenaEvents.OnDamageCaused += HandleDamageCaused;
            ArenaEvents.OnKillScored += HandleKillScored;
        }

        private void OnDisable()
        {
            ArenaEvents.OnMatchStateChanged -= HandleMatchStateChanged;
            ArenaEvents.OnDamageCaused -= HandleDamageCaused;
            ArenaEvents.OnKillScored -= HandleKillScored;

            if (Instance == this) Instance = null;
        }

        private void HandleMatchStateChanged(MatchState state)
        {
            _canAct = (state == MatchState.Playing);
        }

        private void HandleDamageCaused(GameObject attacker, int damage)
        {
            if (attacker == gameObject && _canAct && !_health.IsDead)
            {
                // +10% super per 20 damage (i.e. damage * 0.5%)
                float chargeToAdd = (damage / 20f) * 10f;
                AddSuperCharge(chargeToAdd);
            }
        }

        private void HandleKillScored(GameObject killer)
        {
            if (killer == gameObject && _canAct && !_health.IsDead)
            {
                // +25% super per eliminasi
                AddSuperCharge(25f);
            }
        }

        public void AddSuperCharge(float amount)
        {
            if (_isSuperReady) return;

            _superCharge = Mathf.Clamp(_superCharge + amount, 0f, 100f);
            OnSuperChargeChanged?.Invoke(_superCharge);

            if (_superCharge >= 100f && !_isSuperReady)
            {
                _isSuperReady = true;
                OnSuperReadyStateChanged?.Invoke(true);
            }
        }

        private void Update()
        {
            // Cooldown timer update memakai scaled time
            if (_skillCooldownTimer > 0f)
            {
                _skillCooldownTimer -= Time.deltaTime;
                if (_skillCooldownTimer < 0f) _skillCooldownTimer = 0f;
                OnSkillCooldownChanged?.Invoke(_skillCooldownTimer, SkillCooldownMax);
            }

            if (!_canAct || (_health != null && _health.IsDead)) return;
            if (_statusEffects != null && _statusEffects.IsStunned) return;

            // Input Skill Space
            if (Input.GetKeyDown(KeyCode.Space) && _skillCooldownTimer <= 0f)
            {
                ActivateSkill();
            }

            // Input Super E
            if (Input.GetKeyDown(KeyCode.E) && _isSuperReady)
            {
                ActivateSuper();
            }
        }

        public void ActivateSkill()
        {
            if (_skillCooldownTimer > 0f || _heroData == null) return;
            if (_statusEffects != null && _statusEffects.IsStunned) return;

            _skillCooldownTimer = _heroData.skillCooldown;
            OnSkillCooldownChanged?.Invoke(_skillCooldownTimer, SkillCooldownMax);

            PlaySound(_skillSound, 0.9f);

            switch (_heroData.skillType)
            {
                case HeroSkillType.Dash:
                    // Blaze Dash (jarak 4 u aman collision)
                    if (_playerCtrl != null)
                    {
                        _playerCtrl.ExecuteDash(_heroData.dashDistance, 0.15f);
                    }
                    break;

                case HeroSkillType.Shield:
                    // Bulwark Shield (-60% damage selama 2 s)
                    if (_statusEffects != null)
                    {
                        _statusEffects.ApplyEffect(StatusEffectType.Shield, _heroData.shieldDuration);
                    }
                    break;
            }
        }

        public void ActivateSuper()
        {
            if (!_isSuperReady || _heroData == null) return;
            if (_statusEffects != null && _statusEffects.IsStunned) return;

            _superCharge = 0f;
            _isSuperReady = false;
            OnSuperChargeChanged?.Invoke(0f);
            OnSuperReadyStateChanged?.Invoke(false);

            PlaySound(_superSound, 1.0f);

            switch (_heroData.superType)
            {
                case HeroSuperType.SalvoBurst:
                    // Blaze: Salvo 5 peluru beruntun
                    if (_playerCombat != null)
                    {
                        _playerCombat.FireSalvoBurst(_heroData.superSalvoCount, _heroData.superSalvoInterval);
                    }
                    break;

                case HeroSuperType.GroundSlam:
                    // Bulwark: Hentakan area radius 6 u stun 1 s + 25 dmg
                    ExecuteGroundSlam(_heroData.slamRadius, _heroData.slamDamage, _heroData.slamStunDuration);
                    break;
            }
        }

        private void ExecuteGroundSlam(float radius, int damage, float stunDuration)
        {
            if (TopDownCamera.Instance != null)
            {
                TopDownCamera.Instance.TriggerShake(0.22f, 0.18f);
            }

            Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
            int myTeam = _health != null ? _health.TeamId : 0;

            foreach (var col in colliders)
            {
                Health targetHealth = col.GetComponent<Health>();
                if (targetHealth != null && targetHealth.TeamId != myTeam && !targetHealth.IsDead)
                {
                    DamageInfo info = new DamageInfo(damage, myTeam, gameObject, col.transform.position, Vector3.up, true);
                    targetHealth.TakeDamage(info);

                    StatusEffectController targetSec = col.GetComponent<StatusEffectController>();
                    if (targetSec != null)
                    {
                        targetSec.ApplyEffect(StatusEffectType.Stun, stunDuration);
                    }
                }
            }
        }

        public void ResetSkillAndSuper()
        {
            _skillCooldownTimer = 0f;
            _superCharge = 0f;
            _isSuperReady = false;

            OnSkillCooldownChanged?.Invoke(0f, SkillCooldownMax);
            OnSuperChargeChanged?.Invoke(0f);
            OnSuperReadyStateChanged?.Invoke(false);
        }

        public void SetHeroData(SO_Hero heroData)
        {
            _heroData = heroData;
            ResetSkillAndSuper();
        }

        public void SetAudioClips(AudioClip skillClip, AudioClip superClip)
        {
            _skillSound = skillClip;
            _superSound = superClip;
        }

        private void PlaySound(AudioClip clip, float vol)
        {
            if (clip != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(clip, vol);
            }
        }
    }
}
