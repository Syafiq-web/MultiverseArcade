using UnityEngine;
using MultiverseArcade.Core;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Sistem penembakan AI dengan variasi aim error dan interval fire rate.
    /// Sesuai GDD Bagian 5.9.
    /// </summary>
    public class BotCombat : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private SO_Projectile _projectileData;
        [SerializeField] private float _fireInterval = 1.0f;
        [SerializeField] private float _aimErrorDegrees = 8f;
        [SerializeField] private Transform _firePoint;

        [Header("Audio")]
        [SerializeField] private AudioClip _shootSound;

        private Health _myHealth;
        private float _cooldownTimer = 0f;

        private void Awake()
        {
            _myHealth = GetComponent<Health>();
            if (_firePoint == null) _firePoint = transform;
            _cooldownTimer = Random.Range(0.2f, _fireInterval);
        }

        public void Setup(SO_Projectile proj, float interval, float errorDeg, AudioClip shootClip)
        {
            _projectileData = proj;
            _fireInterval = interval;
            _aimErrorDegrees = errorDeg;
            _shootSound = shootClip;
        }

        public void UpdateCooldown(float dt)
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= dt;
            }
        }

        public bool CanShoot() => _cooldownTimer <= 0f && (_myHealth == null || !_myHealth.IsDead);

        public void ShootAt(Vector3 targetPos)
        {
            if (!CanShoot()) return;

            _cooldownTimer = _fireInterval;

            Vector3 baseDir = (targetPos - transform.position);
            baseDir.y = 0f;
            baseDir.Normalize();

            // Berikan deviasi aim error
            float angleDev = Random.Range(-_aimErrorDegrees, _aimErrorDegrees);
            Quaternion rot = Quaternion.Euler(0, angleDev, 0);
            Vector3 finalDir = rot * baseDir;

            Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position + transform.forward * 0.8f + Vector3.up * 0.5f;
            int teamId = _myHealth != null ? _myHealth.TeamId : 1;

            if (ProjectilePool.Instance != null)
            {
                ProjectilePool.Instance.SpawnProjectile(spawnPos, finalDir, teamId, gameObject, _projectileData);
            }

            PlayShootSound();
        }

        private void PlayShootSound()
        {
            if (_shootSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_shootSound, 0.6f);
            }
        }
    }
}
