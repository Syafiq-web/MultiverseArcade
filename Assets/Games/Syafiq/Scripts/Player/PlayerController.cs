using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Kontroler pergerakan fisik WASD dan bidikan rotasi mouse hero Blaze.
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
        private Vector3 _moveInput;
        private Vector3 _aimPoint;
        private Plane _groundPlane;
        private Camera _mainCamera;
        private bool _canControl = true;

        public Vector3 AimDirection { get; private set; } = Vector3.forward;
        public Vector3 AimPoint => _aimPoint;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();

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

            // Fisika pergerakan konstan
            Vector3 targetVelocity = _moveInput * _moveSpeed;
            _rb.velocity = new Vector3(targetVelocity.x, _rb.velocity.y, targetVelocity.z);
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
                    transform.rotation = Quaternion.LookRotation(AimDirection);
                }

                if (_aimTargetTransform != null)
                {
                    _aimTargetTransform.position = _aimPoint;
                }
            }
        }

        public void SetHeroData(SO_Hero hero)
        {
            _heroData = hero;
            if (_heroData != null)
            {
                _moveSpeed = _heroData.moveSpeed;
            }
        }

        public void SetControlEnabled(bool enabled)
        {
            _canControl = enabled;
            if (!enabled && _rb != null)
            {
                _rb.velocity = Vector3.zero;
            }
        }
    }
}
