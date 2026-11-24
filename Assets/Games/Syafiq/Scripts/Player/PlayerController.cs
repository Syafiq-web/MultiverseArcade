using System.Collections;
using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Kontroler pergerakan fisik WASD, bidikan rotasi mouse hero, dan skill Dash aman tabrakan.
    /// Menggunakan Rigidbody non-kinematic dengan freeze rotation X/Z.
    /// Sesuai GDD Bagian 5.4, 5.5, dan Test Matrix AC-01, AC-02, AC-03.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Hero Data")]
        [SerializeField] private SO_Hero _heroData;
        [SerializeField] private float _moveSpeed = 5.0f;

        [Header("Aiming Plane")]
        [SerializeField] private Transform _aimTargetTransform;

        private Rigidbody _rb;
        private Health _health;
        private StatusEffectController _statusEffects;
        private Vector3 _moveInput;
        private Vector3 _aimPoint;
        private Plane _groundPlane;
        private Camera _mainCamera;
        private bool _canControl = true;
        private bool _isDashing = false;

        public Vector3 AimDirection { get; private set; } = Vector3.forward;
        public Vector3 AimPoint => _aimPoint;
        public Vector3 MoveInput => _moveInput;
        public bool IsDashing => _isDashing;

        private const float ARENA_HALF_BOUND = 13.8f; // Batas arena dinding dalam 30x30

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _statusEffects = GetComponent<StatusEffectController>();

            _rb.isKinematic = false;
            _rb.mass = 1.0f;
            _rb.drag = 0f;
            _rb.angularDrag = 0.05f;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

            _groundPlane = new Plane(Vector3.up, Vector3.zero);
            _mainCamera = Camera.main;

            if (_heroData != null)
            {
                _moveSpeed = _heroData.moveSpeed;
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
            _canControl = (state == MatchState.Playing);
            if (!_canControl && _rb != null)
            {
                _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
            }
        }

        private void Update()
        {
            if (!_canControl || (_health != null && _health.IsDead))
            {
                _moveInput = Vector3.zero;
                return;
            }

            if (_statusEffects != null && _statusEffects.IsStunned)
            {
                _moveInput = Vector3.zero;
                return;
            }

            // WASD Movement Input (AC-01: Diagonal Dinormalisasi)
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 rawInput = new Vector3(h, 0f, v);
            _moveInput = Vector3.ClampMagnitude(rawInput, 1.0f);

            // Mouse Aiming (AC-03: Rotasi hanya pada sumbu Y)
            UpdateMouseAim();
        }

        private void FixedUpdate()
        {
            if (!_canControl || (_health != null && _health.IsDead))
            {
                _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
                return;
            }

            if (_isDashing)
            {
                // Dash ditangani oleh coroutine fisika khusus
                return;
            }

            if (_statusEffects != null && _statusEffects.IsStunned)
            {
                _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
                return;
            }

            float speedMult = _statusEffects != null ? _statusEffects.MoveSpeedMultiplier : 1.0f;
            Vector3 targetVelocity = _moveInput * (_moveSpeed * speedMult);

            // Responsive acceleration & crisp braking curve
            Vector3 currentPlanarVel = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
            float accelRate = _moveInput.sqrMagnitude > 0.01f ? 45f : 65f;
            Vector3 smoothPlanarVel = Vector3.MoveTowards(currentPlanarVel, targetVelocity, accelRate * Time.fixedDeltaTime);
            _rb.velocity = new Vector3(smoothPlanarVel.x, _rb.velocity.y, smoothPlanarVel.z);
        }

        private void UpdateMouseAim()
        {
            if (_mainCamera == null) _mainCamera = Camera.main;
            if (_mainCamera == null) return;

            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            if (_groundPlane.Raycast(ray, out float enter))
            {
                _aimPoint = ray.GetPoint(enter);
                Vector3 lookDirection = _aimPoint - transform.position;
                lookDirection.y = 0f;

                if (lookDirection.sqrMagnitude > 0.01f)
                {
                    AimDirection = lookDirection.normalized;
                    Quaternion targetRot = Quaternion.LookRotation(AimDirection);
                    // Snappy 1440 deg/sec smooth rotation prevents 1-frame visual snapping
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 1440f * Time.deltaTime);
                }

                if (_aimTargetTransform != null)
                {
                    _aimTargetTransform.position = _aimPoint;
                }
            }
        }

        /// <summary>
        /// Mengeksekusi Dash aman yang mematuhi collision dan batas arena agar TIDAK tembus tembok.
        /// </summary>
        public void ExecuteDash(float distance, float duration)
        {
            if (_isDashing || !_canControl) return;

            Vector3 dashDir = _moveInput.sqrMagnitude > 0.05f ? _moveInput.normalized : AimDirection;
            dashDir.y = 0f;
            dashDir.Normalize();

            StartCoroutine(DashRoutine(dashDir, distance, duration));
        }

        private IEnumerator DashRoutine(Vector3 direction, float distance, float duration)
        {
            _isDashing = true;

            // 1. Periksa halangan dengan SphereCast terhadap Wall & Cover
            int obstacleLayerMask = LayerMask.GetMask("Syafiq_Wall", "Default");
            float allowedDistance = distance;
            float radius = 0.45f;

            if (Physics.SphereCast(transform.position + Vector3.up * 0.5f, radius, direction, out RaycastHit hit, distance, obstacleLayerMask, QueryTriggerInteraction.Ignore))
            {
                allowedDistance = Mathf.Max(0f, hit.distance - 0.2f);
            }

            // 2. Cegah keluar dari batas perimeter arena (-13.8 s.d. 13.8)
            Vector3 targetPos = transform.position + (direction * allowedDistance);
            targetPos.x = Mathf.Clamp(targetPos.x, -ARENA_HALF_BOUND, ARENA_HALF_BOUND);
            targetPos.z = Mathf.Clamp(targetPos.z, -ARENA_HALF_BOUND, ARENA_HALF_BOUND);
            targetPos.y = transform.position.y;

            // 3. Gerakkan via Rigidbody velocity terkontrol
            float elapsed = 0f;
            float speed = (targetPos - transform.position).magnitude / Mathf.Max(0.01f, duration);

            while (elapsed < duration)
            {
                elapsed += Time.fixedDeltaTime;
                Vector3 remaining = targetPos - transform.position;
                remaining.y = 0f;

                if (remaining.magnitude < 0.15f) break;

                _rb.velocity = direction * speed;
                yield return new WaitForFixedUpdate();
            }

            _rb.velocity = Vector3.zero;
            _isDashing = false;
        }

        public void SetHeroData(SO_Hero heroData)
        {
            _heroData = heroData;
            if (_heroData != null)
            {
                _moveSpeed = _heroData.moveSpeed;
            }
        }
    }
}
