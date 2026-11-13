using UnityEngine;
using MultiverseArcade.Core;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Sistem penembakan blaster hero Blaze dengan interval fire rate terkontrol.
    /// Sesuai GDD Bagian 5.4, 5.5, dan Test Matrix AC-04.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Weapon Config")]
        [SerializeField] private SO_Projectile _projectileData;
        [SerializeField] private float _fireInterval = 0.4f;
        [SerializeField] private Transform _firePoint;

        [Header("Audio")]
        [SerializeField] private AudioClip _shootSound;

        private Health _health;
        private PlayerController _playerCtrl;
        private float _fireCooldownTimer = 0f;
        private bool _canShoot = true;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _playerCtrl = GetComponent<PlayerController>();

            if (_firePoint == null)
            {
                _firePoint = transform;
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
            _canShoot = (state == MatchState.Playing);
        }

        private void Update()
        {
            if (_fireCooldownTimer > 0f)
            {
                _fireCooldownTimer -= Time.deltaTime;
            }

            if (!_canShoot || (_health != null && _health.IsDead)) return;

            // AC-04: Tahan klik kiri untuk menembak terus menerus dengan interval 0.4 s
            if (Input.GetButton("Fire1") && _fireCooldownTimer <= 0f)
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            _fireCooldownTimer = _fireInterval;

            Vector3 shootDir = _playerCtrl != null ? _playerCtrl.AimDirection : transform.forward;
            Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position + transform.forward * 0.8f + Vector3.up * 0.5f;

            int teamId = _health != null ? _health.TeamId : 0;

            if (ProjectilePool.Instance != null)
            {
                ProjectilePool.Instance.SpawnProjectile(spawnPos, shootDir, teamId, gameObject, _projectileData);
            }

            PlayShootSound();
        }

        private void PlayShootSound()
        {
            if (_shootSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_shootSound, 0.85f);
            }
        }

        public void SetConfig(SO_Projectile projData, float interval, Transform firePoint, AudioClip sound)
        {
            _projectileData = projData;
            _fireInterval = interval;
            _firePoint = firePoint;
            _shootSound = sound;
        }
    }
}
