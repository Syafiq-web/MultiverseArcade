using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Kamera top-down bersudut miring (55 derajat) yang mengikuti pemain dengan smooth damp
    /// dan sedikit look-ahead ke arah pointer mouse.
    /// Sesuai GDD Bagian 5.10.
    /// </summary>
    public class TopDownCamera : MonoBehaviour
    {
        public static TopDownCamera Instance { get; private set; }

        [Header("Target & Follow")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 13f, -9f);
        [SerializeField] private float _smoothTime = 0.15f;
        [SerializeField] private float _lookAheadFactor = 0.15f;
        [SerializeField] private float _cameraAngleX = 55f;

        private Vector3 _currentVelocity;
        private Camera _cam;
        private Plane _groundPlane;

        // Camera Shake
        private float _shakeIntensity = 0f;
        private float _shakeDuration = 0f;
        private float _shakeTimer = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _cam = GetComponent<Camera>();
            _groundPlane = new Plane(Vector3.up, Vector3.zero);
            transform.rotation = Quaternion.Euler(_cameraAngleX, 0f, 0f);
        }

        public void TriggerShake(float intensity = 0.08f, float duration = 0.1f)
        {
            _shakeIntensity = Mathf.Max(_shakeIntensity, intensity);
            _shakeDuration = Mathf.Max(0.01f, duration);
            _shakeTimer = _shakeDuration;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            if (_target != null)
            {
                transform.position = _target.position + _offset;
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 targetPos = _target.position + _offset;

            // Look-ahead menuju kursor mouse
            if (_cam != null)
            {
                Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
                if (_groundPlane.Raycast(ray, out float enter))
                {
                    Vector3 mouseFloorPos = ray.GetPoint(enter);
                    Vector3 lookAheadVec = (mouseFloorPos - _target.position) * _lookAheadFactor;
                    lookAheadVec.y = 0f;
                    targetPos += lookAheadVec;
                }
            }

            Vector3 smoothPos = Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, _smoothTime);

            // Terapkan Camera Shake jika sedang aktif
            if (_shakeTimer > 0f)
            {
                _shakeTimer -= Time.deltaTime;
                float progress = _shakeTimer / _shakeDuration;
                float currentIntensity = _shakeIntensity * progress;
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-1f, 1f) * currentIntensity,
                    Random.Range(-0.5f, 0.5f) * currentIntensity,
                    Random.Range(-1f, 1f) * currentIntensity
                );
                smoothPos += shakeOffset;
                if (_shakeTimer <= 0f) _shakeIntensity = 0f;
            }

            transform.position = smoothPos;
        }
    }
}
