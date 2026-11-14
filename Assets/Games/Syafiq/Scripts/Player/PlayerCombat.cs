using System.Collections;
using UnityEngine;
using MultiverseArcade.Core;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Sistem penembakan senjata utama (Blaster / Shotgun) dan salvo burst hero.
    /// Mendukung spread shotgun, multiplier damage boost, status Stun, dan salvo super.
    /// Sesuai GDD Bagian 5.4, 5.5, dan 5.6.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Weapon Config")]
        [SerializeField] private SO_Projectile _projectileData;
        [SerializeField] private float _fireInterval = 0.4f;
        [SerializeField] private int _projectilesPerShot = 1;
        [SerializeField] private float _spreadAngle = 0f;
        [SerializeField] private Transform _firePoint;

        [Header("Audio")]
        [SerializeField] private AudioClip _shootSound;

        private Health _health;
        private PlayerController _playerCtrl;
        private StatusEffectController _statusEffects;
        private float _fireCooldownTimer = 0f;
        private bool _canShoot = true;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _playerCtrl = GetComponent<PlayerController>();
            _statusEffects = GetComponent<StatusEffectController>();

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
            if (_statusEffects != null && _statusEffects.IsStunned) return;

            // AC-04: Tahan klik kiri untuk menembak terus menerus dengan interval sesuai hero
            if (Input.GetButton("Fire1") && _fireCooldownTimer <= 0f)
            {
                Shoot();
            }
        }

        public void Shoot()
        {
            _fireCooldownTimer = _fireInterval;

            Vector3 baseDir = _playerCtrl != null ? _playerCtrl.AimDirection : transform.forward;
            Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position + transform.forward * 0.8f + Vector3.up * 0.5f;

            int teamId = _health != null ? _health.TeamId : 0;
            float dmgMultiplier = _statusEffects != null ? _statusEffects.DamageDealtMultiplier : 1.0f;

            if (ProjectilePool.Instance != null && _projectileData != null)
            {
                if (_projectilesPerShot <= 1)
                {
                    ProjectilePool.Instance.SpawnProjectile(spawnPos, baseDir, teamId, gameObject, _projectileData, dmgMultiplier);
                }
                else
                {
                    // Shotgun spread (misal Bulwark: 3 pelet)
                    float startAngle = -_spreadAngle * 0.5f;
                    float angleStep = _spreadAngle / (_projectilesPerShot - 1);

                    for (int i = 0; i < _projectilesPerShot; i++)
                    {
                        float currentAngle = startAngle + (angleStep * i);
                        Quaternion rot = Quaternion.Euler(0, currentAngle, 0);
                        Vector3 pelletDir = rot * baseDir;
                        ProjectilePool.Instance.SpawnProjectile(spawnPos, pelletDir, teamId, gameObject, _projectileData, dmgMultiplier);
                    }
                }
            }

            PlayShootSound();
        }

        public void FireSalvoBurst(int count, float interval)
        {
            StartCoroutine(SalvoBurstRoutine(count, interval));
        }

        private IEnumerator SalvoBurstRoutine(int count, float interval)
        {
            int teamId = _health != null ? _health.TeamId : 0;
            float dmgMultiplier = _statusEffects != null ? _statusEffects.DamageDealtMultiplier : 1.0f;

            for (int i = 0; i < count; i++)
            {
                if (_health != null && _health.IsDead) yield break;

                Vector3 shootDir = _playerCtrl != null ? _playerCtrl.AimDirection : transform.forward;
                Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position + transform.forward * 0.8f + Vector3.up * 0.5f;

                if (ProjectilePool.Instance != null && _projectileData != null)
                {
                    ProjectilePool.Instance.SpawnProjectile(spawnPos, shootDir, teamId, gameObject, _projectileData, dmgMultiplier);
                    PlayShootSound();
                }

                yield return new WaitForSeconds(interval);
            }
        }

        private void PlayShootSound()
        {
            if (_shootSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_shootSound, 0.85f);
            }
        }

        public void SetConfig(SO_Projectile projData, float interval, Transform firePoint, AudioClip sound, int projectilesPerShot = 1, float spreadAngle = 0f)
        {
            _projectileData = projData;
            _fireInterval = interval;
            _firePoint = firePoint;
            _shootSound = sound;
            _projectilesPerShot = projectilesPerShot;
            _spreadAngle = spreadAngle;
        }

        public void SetHeroData(SO_Hero heroData)
        {
            if (heroData == null) return;
            _projectileData = heroData.projectileData;
            _fireInterval = heroData.fireInterval;
            _projectilesPerShot = heroData.projectilesPerShot;
            _spreadAngle = heroData.spreadAngle;
        }
    }
}
