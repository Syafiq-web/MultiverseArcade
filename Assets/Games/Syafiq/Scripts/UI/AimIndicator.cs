using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Visualisasi garis bidik laser presisi & reticle target di lantai.
    /// Dilengkapi deteksi target musuh (color shift cyan -> red), gradient laser,
    /// dan reticle feedback saat menembak.
    /// Sesuai GDD Bagian 5.11.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class AimIndicator : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerCtrl;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private float _range = 14f;

        private LineRenderer _line;
        private Transform _reticleTransform;
        private SpriteRenderer _reticleRenderer;
        private Color _normalLaserColor = new Color(0f, 0.9f, 1f, 0.85f);
        private Color _targetEnemyColor = new Color(1f, 0.25f, 0.35f, 0.95f);
        private float _pulseScale = 1f;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.positionCount = 2;
            _line.useWorldSpace = true;
            _line.startWidth = 0.04f;
            _line.endWidth = 0.06f;

            CreateReticle();
        }

        private void CreateReticle()
        {
            GameObject reticleObj = new GameObject("GroundAimReticle");
            reticleObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            _reticleTransform = reticleObj.transform;

            _reticleRenderer = reticleObj.AddComponent<SpriteRenderer>();
            Sprite reticleSpr = Resources.Load<Sprite>("UI/Sprites/reticle_arcade");
            if (reticleSpr == null)
            {
                #if UNITY_EDITOR
                reticleSpr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Games/Syafiq/UI/Sprites/reticle_arcade.png");
                #endif
            }

            if (reticleSpr != null)
            {
                _reticleRenderer.sprite = reticleSpr;
            }
            _reticleRenderer.color = _normalLaserColor;
            _reticleTransform.localScale = Vector3.one * 0.7f;
        }

        public void Setup(PlayerController player, Transform firePoint)
        {
            _playerCtrl = player;
            _firePoint = firePoint;
        }

        public void PulseReticle()
        {
            _pulseScale = 1.4f;
        }

        private void LateUpdate()
        {
            if (_playerCtrl == null) return;

            Vector3 startPos = _firePoint != null ? _firePoint.position : transform.position + Vector3.up * 0.35f;
            startPos.y = 0.15f;

            Vector3 aimTarget = _playerCtrl.AimPoint;
            aimTarget.y = 0.15f;

            Vector3 toAim = (aimTarget - startPos);
            toAim.y = 0f;

            Vector3 dir = toAim.sqrMagnitude > 0.05f ? toAim.normalized : _playerCtrl.AimDirection;
            float maxDist = Mathf.Min(_range, toAim.magnitude);
            if (maxDist < 0.5f) maxDist = _range;

            Vector3 endPos = startPos + dir * maxDist;
            bool hoveringEnemy = false;

            // Deteksi tabrakan laser terhadap cover / wall / musuh
            int layerMask = LayerMask.GetMask("Syafiq_Wall", "Syafiq_Enemy", "Default");
            if (Physics.Raycast(startPos + Vector3.up * 0.3f, dir, out RaycastHit hit, _range, layerMask))
            {
                endPos = hit.point;
                endPos.y = 0.15f;

                Health hitHealth = hit.collider.GetComponentInParent<Health>();
                if (hitHealth != null && hitHealth.TeamId != 0 && !hitHealth.IsDead)
                {
                    hoveringEnemy = true;
                }
            }

            // Atur posisi garis laser
            _line.SetPosition(0, startPos);
            _line.SetPosition(1, endPos);

            // Update warna laser & reticle sesuai target musuh
            Color activeColor = hoveringEnemy ? _targetEnemyColor : _normalLaserColor;
            _line.startColor = new Color(activeColor.r, activeColor.g, activeColor.b, 0.35f);
            _line.endColor = new Color(activeColor.r, activeColor.g, activeColor.b, 0.95f);

            // Atur posisi dan animasi pulsasi reticle
            if (_reticleTransform != null)
            {
                _reticleTransform.position = new Vector3(endPos.x, 0.08f, endPos.z);
                _reticleTransform.Rotate(0f, 0f, 30f * Time.deltaTime);

                if (_pulseScale > 1f)
                {
                    _pulseScale = Mathf.Lerp(_pulseScale, 1f, Time.deltaTime * 12f);
                }
                _reticleTransform.localScale = Vector3.one * (0.6f * _pulseScale);

                if (_reticleRenderer != null)
                {
                    _reticleRenderer.color = activeColor;
                }
            }
        }

        private void OnDestroy()
        {
            if (_reticleTransform != null)
            {
                Destroy(_reticleTransform.gameObject);
            }
        }
    }
}
