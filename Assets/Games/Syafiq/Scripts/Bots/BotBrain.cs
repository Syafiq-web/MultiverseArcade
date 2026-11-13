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
    /// Sesuai GDD Bagian 5.9.
    /// </summary>
    [RequireComponent(typeof(BotMovement))]
    [RequireComponent(typeof(BotTargeting))]
    [RequireComponent(typeof(BotCombat))]
    [RequireComponent(typeof(Health))]
    public class BotBrain : MonoBehaviour
    {
        [Header("Difficulty Config")]
        [SerializeField] private SO_BotDifficulty _difficulty;
        [SerializeField] private BotState _currentState = BotState.Idle;

        [Header("State Parameters")]
        [SerializeField] private float _attackRange = 7.5f;
        [SerializeField] private float _retreatHpRatio = 0.25f;

        private BotMovement _movement;
        private BotTargeting _targeting;
        private BotCombat _combat;
        private Health _health;

        private float _stateTimer = 0f;
        private Vector3 _patrolPoint;
        private bool _canAct = true;

        public BotState CurrentState => _currentState;

        private void Awake()
        {
            _movement = GetComponent<BotMovement>();
            _targeting = GetComponent<BotTargeting>();
            _combat = GetComponent<BotCombat>();
            _health = GetComponent<Health>();

            if (_difficulty != null)
            {
                _retreatHpRatio = _difficulty.retreatHealthRatio;
            }
        }

        private void OnEnable()
        {
            ArenaEvents.OnMatchStateChanged += HandleMatchStateChanged;
        }

        private void OnDisable()
        {
            ArenaEvents.OnMatchStateChanged -= HandleMatchStateChanged;
        }

        private void HandleMatchStateChanged(MatchState state)
        {
            _canAct = (state == MatchState.Playing);
            if (!_canAct && _movement != null)
            {
                _movement.Stop();
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

            float dt = Time.deltaTime;
            _targeting.UpdateTargeting(dt);
            _combat.UpdateCooldown(dt);
            _stateTimer += dt;

            // Cek kondisi retreat jika HP sekarat (< 25%)
            if (_health != null && (float)_health.CurrentHealth / _health.MaxHealth < _retreatHpRatio && _currentState != BotState.Retreat && _currentState != BotState.Dead)
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
            if (_stateTimer > 0.5f)
            {
                // Pilih titik patroli di area tengah arena
                _patrolPoint = new Vector3(Random.Range(-8f, 8f), 0f, Random.Range(-8f, 8f));
                SetState(BotState.Seek);
            }
        }

        private void UpdateSeekState()
        {
            // Jika musuh terdeteksi, beralih ke Chase
            if (_targeting.HasValidTarget)
            {
                SetState(BotState.Chase);
                return;
            }

            // Jika bot tim Blue (rekan pemain), ikuti posisi pemain dalam radius 6-8 u
            if (_health.TeamId == 0)
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

            if (_stateTimer > 1.2f)
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

            // Tembak jika cooldown siap
            if (_combat.CanShoot())
            {
                _combat.ShootAt(targetPos);
            }

            // Bergerak menyamping
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
            // Menjauh dari musuh terdekat menuju zona aman spawn
            Vector3 safePoint = _health.TeamId == 0 ? new Vector3(-10f, 0f, -10f) : new Vector3(10f, 0f, 10f);
            _movement.MoveTo(safePoint);

            if (_stateTimer > 3.0f || (_health != null && (float)_health.CurrentHealth / _health.MaxHealth >= 0.5f))
            {
                SetState(BotState.Seek);
            }
        }
    }
}
