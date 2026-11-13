using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Visualisasi garis bidik laser / reticle di lantai dari karakter menuju titik kursor.
    /// Sesuai GDD Bagian 5.11.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class AimIndicator : MonoBehaviour
    {
        [SerializeField] private PlayerController _playerCtrl;
        [SerializeField] private Transform _firePoint;
        [SerializeField] private float _range = 10f;

        private LineRenderer _line;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.positionCount = 2;
            _line.useWorldSpace = true;
            _line.startWidth = 0.05f;
            _line.endWidth = 0.05f;
        }

        public void Setup(PlayerController player, Transform firePoint)
        {
            _playerCtrl = player;
            _firePoint = firePoint;
        }

        private void LateUpdate()
        {
            if (_playerCtrl == null) return;

            Vector3 startPos = _firePoint != null ? _firePoint.position : transform.position + Vector3.up * 0.3f;
            startPos.y = 0.15f; // Sedikit di atas permukaan lantai

            Vector3 endPos = startPos + _playerCtrl.AimDirection * _range;

            // Jika ada dinding di depan, perpendek garis reticle
            if (Physics.Raycast(startPos, _playerCtrl.AimDirection, out RaycastHit hit, _range, LayerMask.GetMask("Syafiq_Wall", "Default")))
            {
                endPos = hit.point;
                endPos.y = 0.15f;
            }

            _line.SetPosition(0, startPos);
            _line.SetPosition(1, endPos);
        }
    }
}
