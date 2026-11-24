using System.Collections;
using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Mengontrol visual 3D karakter (Player maupun Bot):
    /// - Procedural walk bobbing saat bergerak
    /// - Subtle body lean / tilt ke arah gerakan
    /// - Damage flash saat menerima damage
    /// - Efek saat hero kalah
    /// </summary>
    public class CharacterVisualController : MonoBehaviour
    {
        [Header("Procedural Movement Visuals")]
        [SerializeField] private Transform _modelTransform;
        [SerializeField] private float _bobFrequency = 12f;
        [SerializeField] private float _bobAmount = 0.08f;
        [SerializeField] private float _tiltAmount = 10f;
        [SerializeField] private float _tiltSmoothSpeed = 10f;

        private Rigidbody _rb;
        private Health _health;
        private Renderer[] _renderers;
        private MaterialPropertyBlock _mpb;

        private Vector3 _originalLocalPos;
        private Quaternion _originalLocalRot;
        private float _bobTimer = 0f;
        private float _currentTilt = 0f;
        private float _recoilTilt = 0f;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            if (_modelTransform == null)
            {
                Transform visualChild = transform.Find("Visual");
                _modelTransform = visualChild != null ? visualChild : transform;
            }

            _originalLocalPos = _modelTransform.localPosition;
            _originalLocalRot = _modelTransform.localRotation;

            _rb = GetComponent<Rigidbody>();
            _health = GetComponent<Health>();
            _renderers = _modelTransform.GetComponentsInChildren<Renderer>();
            _mpb = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            if (_health != null)
            {
                _health.OnDamageTaken += HandleDamageTaken;
                _health.OnDied += HandleDied;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDamageTaken -= HandleDamageTaken;
                _health.OnDied -= HandleDied;
            }
        }

        private void Update()
        {
            if (_modelTransform == null) return;

            Vector3 velocity = _rb != null ? _rb.velocity : Vector3.zero;
            // Ignore vertical velocity
            velocity.y = 0;
            float speed = velocity.magnitude;

            if (speed > 0.1f)
            {
                // Procedural walk bob
                _bobTimer += Time.deltaTime * _bobFrequency * (speed / 4f);
                float yOffset = Mathf.Sin(_bobTimer) * _bobAmount;
                _modelTransform.localPosition = _originalLocalPos + new Vector3(0, yOffset, 0);

                // Body lean forward slightly
                _currentTilt = Mathf.Lerp(_currentTilt, _tiltAmount, Time.deltaTime * _tiltSmoothSpeed);
            }
            else
            {
                // Idle breathing bob
                _bobTimer += Time.deltaTime * 2.5f;
                float yOffset = Mathf.Sin(_bobTimer) * (_bobAmount * 0.25f);
                _modelTransform.localPosition = _originalLocalPos + new Vector3(0, yOffset, 0);

                _currentTilt = Mathf.Lerp(_currentTilt, 0f, Time.deltaTime * _tiltSmoothSpeed);
            }

            // Recoil recovery
            if (_recoilTilt > 0.01f)
            {
                _recoilTilt = Mathf.Lerp(_recoilTilt, 0f, Time.deltaTime * 20f);
            }

            _modelTransform.localRotation = _originalLocalRot * Quaternion.Euler(_currentTilt - _recoilTilt, 0, 0);
        }

        /// <summary>
        /// Efek hentakan recoil saat karakter menembak
        /// </summary>
        public void TriggerTorsoRecoil(float amount = 8f)
        {
            _recoilTilt = Mathf.Clamp(_recoilTilt + amount, 0f, 16f);
        }

        private void HandleDamageTaken(int damage, Health source)
        {
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(DamageFlashRoutine());

            // Screen shake jika ini adalah pemain lokal
            if (GetComponent<PlayerController>() != null && TopDownCamera.Instance != null)
            {
                TopDownCamera.Instance.TriggerShake(0.14f, 0.12f);
            }
        }

        private IEnumerator DamageFlashRoutine()
        {
            SetColorOverride(new Color(1f, 0.3f, 0.3f, 1f));
            yield return new WaitForSeconds(0.08f);
            ClearColorOverride();
            _flashRoutine = null;
        }

        private void SetColorOverride(Color color)
        {
            if (_renderers == null) return;
            foreach (var r in _renderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(_mpb);
                _mpb.SetColor("_BaseColor", color);
                _mpb.SetColor("_Color", color);
                r.SetPropertyBlock(_mpb);
            }
        }

        private void ClearColorOverride()
        {
            if (_renderers == null) return;
            foreach (var r in _renderers)
            {
                if (r == null) continue;
                r.SetPropertyBlock(null);
            }
        }

        private void HandleDied(Health victim)
        {
            StartCoroutine(DeathSinkRoutine());
        }

        private IEnumerator DeathSinkRoutine()
        {
            float elapsed = 0f;
            Vector3 startPos = _modelTransform.localPosition;
            Vector3 targetPos = startPos - new Vector3(0, 1.2f, 0);

            while (elapsed < 0.6f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / 0.6f;
                if (_modelTransform != null)
                {
                    _modelTransform.localPosition = Vector3.Lerp(startPos, targetPos, t);
                    _modelTransform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t * 0.8f);
                }
                yield return null;
            }
        }
    }
}
