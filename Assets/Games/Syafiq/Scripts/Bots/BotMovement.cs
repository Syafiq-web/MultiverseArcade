using UnityEngine;
using UnityEngine.AI;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Pengendali navigasi pergerakan AI menggunakan NavMeshAgent dengan proteksi anti-stuck.
    /// Sesuai GDD Bagian 5.8, 5.9, dan 5.17.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class BotMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 4.2f;
        [SerializeField] private float _angularSpeed = 360f;
        [SerializeField] private float _stoppingDistance = 4f;

        private NavMeshAgent _agent;
        private StatusEffectController _statusEffects;
        private Vector3 _lastPosition;
        private float _stuckTimer = 0f;
        private float _setDestTimer = 0f;
        private const float DEST_INTERVAL = 0.15f; // Batas update 6-7x per detik (GDD 5.17)

        public NavMeshAgent Agent => _agent;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _statusEffects = GetComponent<StatusEffectController>();
            _agent.speed = _moveSpeed;
            _agent.angularSpeed = _angularSpeed;
            _agent.stoppingDistance = _stoppingDistance;
            _agent.acceleration = 12f;
            _agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

            _lastPosition = transform.position;
        }

        private void Update()
        {
            if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

            if (_statusEffects != null && _statusEffects.IsStunned)
            {
                _agent.isStopped = true;
                return;
            }

            _agent.isStopped = false;
            float speedMult = _statusEffects != null ? _statusEffects.MoveSpeedMultiplier : 1.0f;
            _agent.speed = _moveSpeed * speedMult;
        }

        public void SetSpeed(float speed)
        {
            _moveSpeed = speed;
            if (_agent != null) _agent.speed = _moveSpeed;
        }

        public void MoveTo(Vector3 targetPos)
        {
            if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

            _setDestTimer += Time.deltaTime;
            if (_setDestTimer >= DEST_INTERVAL)
            {
                _setDestTimer = 0f;
                _agent.SetDestination(targetPos);
            }

            CheckStuck();
        }

        public void Stop()
        {
            if (_agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.ResetPath();
            }
        }

        public void FaceTarget(Vector3 targetPos)
        {
            Vector3 lookDir = targetPos - transform.position;
            lookDir.y = 0f;
            if (lookDir.sqrMagnitude > 0.05f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }
        }

        private void CheckStuck()
        {
            if (_agent.hasPath && _agent.velocity.sqrMagnitude < 0.05f)
            {
                _stuckTimer += Time.deltaTime;
                if (_stuckTimer >= 1.2f)
                {
                    _stuckTimer = 0f;
                    // Jolt sidestep jika bot terjebak di sudut
                    Vector3 randomJolt = transform.position + (Random.insideUnitSphere * 2.5f);
                    randomJolt.y = transform.position.y;
                    if (NavMesh.SamplePosition(randomJolt, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                    {
                        _agent.SetDestination(hit.position);
                    }
                }
            }
            else
            {
                _stuckTimer = 0f;
            }
        }
    }
}
