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
        private CharacterVisualController _visualCtrl;
        private float _fireCooldownTimer = 0f;
        private bool _canShoot = true;

        // Weapon Kickback & Muzzle Flash
        private Vector3 _firePointInitialLocalPos;
        private Vector3 _recoilOffset = Vector3.zero;
        private GameObject _muzzleFlashObj;
        private Coroutine _muzzleFlashRoutine;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _playerCtrl = GetComponent<PlayerController>();
            _statusEffects = GetComponent<StatusEffectController>();
            _visualCtrl = GetComponent<CharacterVisualController>();

            if (_firePoint == null)
            {
                _firePoint = transform;
            }
            _firePointInitialLocalPos = _firePoint.localPosition;
            CreateMuzzleFlashVisual();
        }

        private void CreateMuzzleFlashVisual()
        {
            if (_firePoint == null) return;
            _muzzleFlashObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _muzzleFlashObj.name = "MuzzleFlash";
            _muzzleFlashObj.transform.parent = _firePoint;
            _muzzleFlashObj.transform.localPosition = new Vector3(0, 0, 0.25f);
            _muzzleFlashObj.transform.localScale = Vector3.one * 0.28f;

            Collider col = _muzzleFlashObj.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Renderer ren = _muzzleFlashObj.GetComponent<Renderer>();
            if (ren != null)
            {
                Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
                if (unlitShader != null)
                {
                    Material flashMat = new Material(unlitShader);
                    flashMat.color = new Color(1f, 0.95f, 0.4f, 1f);
                    ren.sharedMaterial = flashMat;
                }
            }
            _muzzleFlashObj.SetActive(false);
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

            // Smooth weapon recoil recovery
            if (_recoilOffset.sqrMagnitude > 0.0001f && _firePoint != null)
            {
                _recoilOffset = Vector3.Lerp(_recoilOffset, Vector3.zero, Time.deltaTime * 24f);
                _firePoint.localPosition = _firePointInitialLocalPos + _recoilOffset;
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

            Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position + transform.forward * 0.8f + Vector3.up * 0.5f;

            // Presisi Balistik: Konvergensi akurat menuju titik bidik kursor (menghilangkan parallax offset)
            Vector3 baseDir;
            if (_playerCtrl != null)
            {
                Vector3 aimTarget = _playerCtrl.AimPoint;
                aimTarget.y = spawnPos.y;
                Vector3 toTarget = aimTarget - spawnPos;
                toTarget.y = 0f;

                if (toTarget.sqrMagnitude > 0.36f)
                {
                    baseDir = toTarget.normalized;
                }
                else
                {
                    baseDir = _playerCtrl.AimDirection;
                }
            }
            else
            {
                baseDir = transform.forward;
            }

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

            // Visual feedback: Gun kickback, Muzzle Flash, Torso Recoil & Camera Shake
            TriggerWeaponFeedback();
            PlayShootSound();
        }

        private void TriggerWeaponFeedback()
        {
            // Weapon kickback
            _recoilOffset = -Vector3.forward * (_projectilesPerShot > 1 ? 0.22f : 0.15f);
            if (_firePoint != null)
            {
                _firePoint.localPosition = _firePointInitialLocalPos + _recoilOffset;
            }

            // Torso lean kickback
            if (_visualCtrl != null)
            {
                _visualCtrl.TriggerTorsoRecoil(_projectilesPerShot > 1 ? 10f : 6f);
            }

            // Aim indicator pulse
            GetComponent<AimIndicator>()?.PulseReticle();

            // Muzzle flash pop
            if (_muzzleFlashRoutine != null) StopCoroutine(_muzzleFlashRoutine);
            _muzzleFlashRoutine = StartCoroutine(MuzzleFlashRoutine());

            // Screen Shake
            if (TopDownCamera.Instance != null)
            {
                float shakeForce = _projectilesPerShot > 1 ? 0.12f : 0.05f;
                float shakeDuration = _projectilesPerShot > 1 ? 0.11f : 0.07f;
                TopDownCamera.Instance.TriggerShake(shakeForce, shakeDuration);
            }

            // HUD crosshair pulse
            if (ArenaHUD.Instance != null)
            {
                ArenaHUD.Instance.PulseReticle(1.35f);
            }
        }

        private IEnumerator MuzzleFlashRoutine()
        {
            if (_muzzleFlashObj != null)
            {
                _muzzleFlashObj.SetActive(true);
                yield return new WaitForSeconds(0.045f);
                _muzzleFlashObj.SetActive(false);
            }
            _muzzleFlashRoutine = null;
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

                Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position + transform.forward * 0.8f + Vector3.up * 0.5f;

                Vector3 shootDir;
                if (_playerCtrl != null)
                {
                    Vector3 aimTarget = _playerCtrl.AimPoint;
                    aimTarget.y = spawnPos.y;
                    Vector3 toTarget = aimTarget - spawnPos;
                    toTarget.y = 0f;
                    shootDir = toTarget.sqrMagnitude > 0.36f ? toTarget.normalized : _playerCtrl.AimDirection;
                }
                else
                {
                    shootDir = transform.forward;
                }

                if (ProjectilePool.Instance != null && _projectileData != null)
                {
                    ProjectilePool.Instance.SpawnProjectile(spawnPos, shootDir, teamId, gameObject, _projectileData, dmgMultiplier);
                    TriggerWeaponFeedback();
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

            if (_firePoint != null)
            {
                _firePointInitialLocalPos = _firePoint.localPosition;
                if (_muzzleFlashObj == null) CreateMuzzleFlashVisual();
            }
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
