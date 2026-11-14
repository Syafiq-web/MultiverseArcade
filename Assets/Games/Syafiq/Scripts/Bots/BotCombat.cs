using UnityEngine;
using MultiverseArcade.Core;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Sistem penembakan AI dengan dukungan senjata hero (Blaster / Shotgun),
    /// deviasi aim error, multiplier damage boost, dan penanganan status Stun.
    /// Sesuai GDD Bagian 5.4, 5.5, dan 5.9.
    /// </summary>
    public class BotCombat : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private SO_Projectile _projectileData;
        [SerializeField] private float _fireInterval = 1.0f;
        [SerializeField] private float _aimErrorDegrees = 8f;
        [SerializeField] private int _projectilesPerShot = 1;
        [SerializeField] private float _spreadAngle = 0f;
        [SerializeField] private Transform _firePoint;

        [Header("Audio")]
        [SerializeField] private AudioClip _shootSound;

        private Health _myHealth;
        private StatusEffectController _statusEffects;
        private float _cooldownTimer = 0f;

        private void Awake()
        {
            _myHealth = GetComponent<Health>();
            _statusEffects = GetComponent<StatusEffectController>();

            if (_firePoint == null) _firePoint = transform;
            _cooldownTimer = Random.Range(0.2f, _fireInterval);
        }

        public void Setup(SO_Projectile proj, float interval, float errorDeg, AudioClip shootClip, int pellets = 1, float spread = 0f)
        {
            _projectileData = proj;
            _fireInterval = interval;
            _aimErrorDegrees = errorDeg;
            _shootSound = shootClip;
            _projectilesPerShot = pellets;
            _spreadAngle = spread;
        }

        public void SetHeroData(SO_Hero hero, float difficultyAimError, float difficultyFireInterval)
        {
            if (hero == null) return;
            _projectileData = hero.projectileData;
            _fireInterval = difficultyFireInterval > 0f ? difficultyFireInterval : hero.fireInterval;
            _aimErrorDegrees = difficultyAimError;
            _projectilesPerShot = hero.projectilesPerShot;
            _spreadAngle = hero.spreadAngle;
        }

        public void UpdateCooldown(float dt)
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= dt;
            }
        }

        public bool CanShoot()
        {
            if (_cooldownTimer > 0f) return false;
            if (_myHealth != null && _myHealth.IsDead) return false;
            if (_statusEffects != null && _statusEffects.IsStunned) return false;
            return true;
        }

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
            float dmgMultiplier = _statusEffects != null ? _statusEffects.DamageDealtMultiplier : 1.0f;

            if (ProjectilePool.Instance != null && _projectileData != null)
            {
                if (_projectilesPerShot <= 1)
                {
                    ProjectilePool.Instance.SpawnProjectile(spawnPos, finalDir, teamId, gameObject, _projectileData, dmgMultiplier);
                }
                else
                {
                    // Shotgun spread
                    float startAngle = -_spreadAngle * 0.5f;
                    float angleStep = _spreadAngle / (_projectilesPerShot - 1);

                    for (int i = 0; i < _projectilesPerShot; i++)
                    {
                        float currentAngle = startAngle + (angleStep * i);
                        Quaternion pelletRot = Quaternion.Euler(0, currentAngle, 0);
                        Vector3 pelletDir = pelletRot * finalDir;
                        ProjectilePool.Instance.SpawnProjectile(spawnPos, pelletDir, teamId, gameObject, _projectileData, dmgMultiplier);
                    }
                }
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
