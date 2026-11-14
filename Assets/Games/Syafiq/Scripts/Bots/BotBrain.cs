using System.Collections.Generic;
using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    public enum BotState
    {
        Idle,
        Seek,
        Chase,
        Attack,
        Strafe,
        Retreat,
        Dead
    }

    /// <summary>
    /// FSM Brain pengontrol logika aksi bot AI (Idle -> Seek -> Chase -> Attack <-> Strafe -> Retreat -> Dead).
    /// Mendukung pemilihan power-up oportunistik, variasi hero data-driven (Blaze/Bulwark),
    /// pemakaian skill/super AI, dan tingkat kesulitan Easy/Normal/Hard.
    /// Sesuai GDD Bagian 5.7 & 5.9.
    /// </summary>
    [RequireComponent(typeof(BotMovement))]
    [RequireComponent(typeof(BotTargeting))]
    [RequireComponent(typeof(BotCombat))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(StatusEffectController))]
    public class BotBrain : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private SO_BotDifficulty _difficulty;
        [SerializeField] private SO_Hero _heroData;
        [SerializeField] private BotState _currentState = BotState.Idle;

        [Header("State Parameters")]
        [SerializeField] private float _attackRange = 7.5f;
        [SerializeField] private float _retreatHpRatio = 0.25f;

        private BotMovement _movement;
        private BotTargeting _targeting;
        private BotCombat _combat;
        private Health _health;
        private StatusEffectController _statusEffects;

        private float _stateTimer = 0f;
        private Vector3 _patrolPoint;
        private bool _canAct = true;

        // Skill & Super timer for bots
        private float _skillCooldownTimer = 0f;
        private float _superCharge = 0f;
        private bool _isSuperReady = false;

        public BotState CurrentState => _currentState;

        private void Awake()
        {
            _movement = GetComponent<BotMovement>();
            _targeting = GetComponent<BotTargeting>();
            _combat = GetComponent<BotCombat>();
            _health = GetComponent<Health>();
            _statusEffects = GetComponent<StatusEffectController>();

            ApplyDifficultyConfig();
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
        }

        private void HandleMatchStateChanged(MatchState state)
        {
            _canAct = (state == MatchState.Playing);
            if (!_canAct && _movement != null)
            {
                _movement.Stop();
            }
        }

        private void HandleDamageCaused(GameObject attacker, int damage)
        {
            if (attacker == gameObject && _canAct && _health != null && !_health.IsDead)
            {
                _superCharge = Mathf.Clamp(_superCharge + (damage / 20f) * 10f, 0f, 100f);
                if (_superCharge >= 100f) _isSuperReady = true;
            }
        }

        private void HandleKillScored(GameObject killer)
        {
            if (killer == gameObject && _canAct && _health != null && !_health.IsDead)
            {
                _superCharge = Mathf.Clamp(_superCharge + 25f, 0f, 100f);
                if (_superCharge >= 100f) _isSuperReady = true;
            }
        }

        public void Setup(SO_Hero hero, SO_BotDifficulty diff)
        {
            _heroData = hero;
            _difficulty = diff;

            ApplyDifficultyConfig();

            if (_heroData != null && _movement != null)
            {
                _movement.SetSpeed(_heroData.moveSpeed);
            }

            if (_combat != null && _heroData != null && _difficulty != null)
            {
                _combat.SetHeroData(_heroData, _difficulty.aimErrorDegrees, _difficulty.fireInterval);
            }
        }

        private void ApplyDifficultyConfig()
        {
            if (_difficulty != null)
            {
                _retreatHpRatio = _difficulty.retreatHealthRatio;
                if (_targeting != null)
                {
                    _targeting.SetEvaluationInterval(_difficulty.targetEvaluationInterval);
                }
            }
        }

        private void Update()
        {
            if (!_canAct) return;

            if (_health != null && _health.IsDead)
            {
                SetState(BotState.Dead);
                return;
            }

            // Jika bot sedang Stun, hentikan pergerakan dan aksi
            if (_statusEffects != null && _statusEffects.IsStunned)
            {
                _movement.Stop();
                return;
            }

            float dt = Time.deltaTime;
            _targeting.UpdateTargeting(dt);
            _combat.UpdateCooldown(dt);
            _stateTimer += dt;

            if (_skillCooldownTimer > 0f) _skillCooldownTimer -= dt;

            // Evaluasi penggunaan skill & super AI
            CheckBotSkillAndSuper();

            // Cek kondisi retreat jika HP sekarat
            bool canRetreat = _difficulty == null || _difficulty.allowRetreat;
            if (canRetreat && _health != null && (float)_health.CurrentHealth / _health.MaxHealth < _retreatHpRatio && _currentState != BotState.Retreat && _currentState != BotState.Dead)
            {
                SetState(BotState.Retreat);
                return;
            }

            switch (_currentState)
            {
                case BotState.Idle:
                    UpdateIdleState();
                    break;
                case BotState.Seek:
                    UpdateSeekState();
                    break;
                case BotState.Chase:
                    UpdateChaseState();
                    break;
                case BotState.Attack:
                    UpdateAttackState();
                    break;
                case BotState.Strafe:
                    UpdateStrafeState();
                    break;
                case BotState.Retreat:
                    UpdateRetreatState();
                    break;
                case BotState.Dead:
                    _movement.Stop();
                    break;
            }
        }

        private void SetState(BotState newState)
        {
            if (_currentState == newState) return;
            _currentState = newState;
            _stateTimer = 0f;
        }

        private void UpdateIdleState()
        {
            _movement.Stop();
            if (_stateTimer > 0.4f)
            {
                _patrolPoint = new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-8f, 8f));
                SetState(BotState.Seek);
            }
        }

        private void UpdateSeekState()
        {
            if (_targeting.HasValidTarget)
            {
                SetState(BotState.Chase);
                return;
            }

            // Oportunistik: ambil power-up terdekat jika berada dalam jangkauan < 6 u
            PowerUpItem nearbyPowerUp = FindNearestAvailablePowerUp(null, 6.0f);
            if (nearbyPowerUp != null)
            {
                _movement.MoveTo(nearbyPowerUp.transform.position);
                return;
            }

            // Jika bot tim Blue (rekan pemain), ikuti pemain dalam radius 6-8 u
            if (_health != null && _health.TeamId == 0)
            {
                Transform player = _targeting.GetPlayerTransform();
                if (player != null)
                {
                    float distToPlayer = Vector3.Distance(transform.position, player.position);
                    if (distToPlayer > 6.0f)
                    {
                        _movement.MoveTo(player.position + (Random.insideUnitSphere * 2f));
                        return;
                    }
                }
            }

            _movement.MoveTo(_patrolPoint);
            if (Vector3.Distance(transform.position, _patrolPoint) < 2f || _stateTimer > 5f)
            {
                SetState(BotState.Idle);
            }
        }

        private void UpdateChaseState()
        {
            if (!_targeting.HasValidTarget)
            {
                SetState(BotState.Seek);
                return;
            }

            Vector3 targetPos = _targeting.CurrentTarget.transform.position;
            float dist = Vector3.Distance(transform.position, targetPos);

            if (dist <= _attackRange && _targeting.HasClearLineOfSight(targetPos))
            {
                SetState(BotState.Attack);
                return;
            }

            _movement.MoveTo(targetPos);
        }

        private void UpdateAttackState()
        {
            if (!_targeting.HasValidTarget)
            {
                SetState(BotState.Seek);
                return;
            }

            Vector3 targetPos = _targeting.CurrentTarget.transform.position;
            float dist = Vector3.Distance(transform.position, targetPos);

            if (dist > _attackRange || !_targeting.HasClearLineOfSight(targetPos))
            {
                SetState(BotState.Chase);
                return;
            }

            _movement.Stop();
            _movement.FaceTarget(targetPos);

            if (_combat.CanShoot())
            {
                _combat.ShootAt(targetPos);
            }

            bool allowStrafe = _difficulty == null || _difficulty.allowStrafe;
            if (allowStrafe && _stateTimer > 1.2f)
            {
                SetState(BotState.Strafe);
            }
        }

        private void UpdateStrafeState()
        {
            if (!_targeting.HasValidTarget)
            {
                SetState(BotState.Seek);
                return;
            }

            Vector3 targetPos = _targeting.CurrentTarget.transform.position;
            _movement.FaceTarget(targetPos);

            if (_combat.CanShoot())
            {
                _combat.ShootAt(targetPos);
            }

            Vector3 dirToTarget = (targetPos - transform.position).normalized;
            Vector3 strafeDir = Vector3.Cross(dirToTarget, Vector3.up);
            if (Random.value > 0.5f) strafeDir = -strafeDir;

            _movement.MoveTo(transform.position + strafeDir * 3f);

            if (_stateTimer > 1.0f)
            {
                SetState(BotState.Attack);
            }
        }

        private void UpdateRetreatState()
        {
            // Cari Health Pack aktif terdekat dalam radius 16 u
            PowerUpItem healthPack = FindNearestAvailablePowerUp(PowerUpType.HealthPack, 16.0f);
            if (healthPack != null)
            {
                _movement.MoveTo(healthPack.transform.position);
            }
            else
            {
                // Menuju zona spawn aman
                Vector3 safePoint = (_health != null && _health.TeamId == 0) ? new Vector3(-10f, 0f, -10f) : new Vector3(10f, 0f, 10f);
                _movement.MoveTo(safePoint);
            }

            if (_stateTimer > 3.5f || (_health != null && (float)_health.CurrentHealth / _health.MaxHealth >= 0.5f))
            {
                SetState(BotState.Seek);
            }
        }

        private void CheckBotSkillAndSuper()
        {
            if (_heroData == null) return;

            // Skill Shield Bulwark: aktifkan saat HP < 50% dalam combat
            if (_heroData.skillType == HeroSkillType.Shield && _skillCooldownTimer <= 0f)
            {
                if (_health != null && (float)_health.CurrentHealth / _health.MaxHealth < 0.5f && _targeting.HasValidTarget)
                {
                    _skillCooldownTimer = _heroData.skillCooldown;
                    if (_statusEffects != null) _statusEffects.ApplyEffect(StatusEffectType.Shield, _heroData.shieldDuration);
                }
            }

            // Super Execution:
            if (_isSuperReady)
            {
                if (_heroData.superType == HeroSuperType.GroundSlam)
                {
                    // Bulwark Slam: gunakan saat ada target dalam radius 5 u
                    if (_targeting.HasValidTarget && Vector3.Distance(transform.position, _targeting.CurrentTarget.transform.position) <= _heroData.slamRadius)
                    {
                        ExecuteBotGroundSlam();
                    }
                }
                else if (_heroData.superType == HeroSuperType.SalvoBurst)
                {
                    // Blaze Salvo: gunakan saat Attack state
                    if (_currentState == BotState.Attack && _targeting.HasValidTarget)
                    {
                        ExecuteBotSalvo();
                    }
                }
            }
        }

        private void ExecuteBotGroundSlam()
        {
            _superCharge = 0f;
            _isSuperReady = false;

            Collider[] cols = Physics.OverlapSphere(transform.position, _heroData.slamRadius);
            int myTeam = _health != null ? _health.TeamId : 1;

            foreach (var col in cols)
            {
                Health h = col.GetComponent<Health>();
                if (h != null && h.TeamId != myTeam && !h.IsDead)
                {
                    h.TakeDamage(new DamageInfo(_heroData.slamDamage, myTeam, gameObject, col.transform.position, Vector3.up, true));
                    StatusEffectController sec = col.GetComponent<StatusEffectController>();
                    if (sec != null) sec.ApplyEffect(StatusEffectType.Stun, _heroData.slamStunDuration);
                }
            }
        }

        private void ExecuteBotSalvo()
        {
            _superCharge = 0f;
            _isSuperReady = false;

            if (_targeting.HasValidTarget)
            {
                _combat.ShootAt(_targeting.CurrentTarget.transform.position);
            }
        }

        private PowerUpItem FindNearestAvailablePowerUp(PowerUpType? targetType, float maxRadius)
        {
            PowerUpItem best = null;
            float bestDistSq = maxRadius * maxRadius;
            Vector3 myPos = transform.position;

            foreach (var p in PowerUpItem.ActivePowerUps)
            {
                if (p == null || !p.IsAvailable) continue;
                if (targetType.HasValue && p.Type != targetType.Value) continue;

                float distSq = (p.transform.position - myPos).sqrMagnitude;
                if (distSq < bestDistSq)
                {
                    bestDistSq = distSq;
                    best = p;
                }
            }
            return best;
        }
    }
}
