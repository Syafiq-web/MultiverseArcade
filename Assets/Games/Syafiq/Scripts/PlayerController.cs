using System.Collections;
using MultiverseArcade.Core;
using UnityEngine;

namespace MultiverseArcade.Games.Syafiq
{
    /// <summary>
    /// Mengontrol pergerakan (WASD), arah bidik presisi (Mouse Aim dengan Reticle/Laser),
    /// sistem pergantian senjata (Plasma Rifle vs Scatter Shotgun), penembakan peluru,
    /// audio SFX, damage flash, dan kematian hero.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public enum WeaponType { PlasmaRifle, ScatterShotgun }

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float gravity = 20f;

        [Header("Combat & Weapons")]
        [SerializeField] private WeaponType currentWeapon = WeaponType.PlasmaRifle;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private int maxHealth = 100;

        [Header("Weapon Specs - Plasma Rifle")]
        [SerializeField] private float rifleFireRate = 0.16f;
        [SerializeField] private int rifleDamage = 25;

        [Header("Weapon Specs - Scatter Shotgun")]
        [SerializeField] private float shotgunFireRate = 0.55f;
        [SerializeField] private int shotgunPellets = 6;
        [SerializeField] private float shotgunSpreadAngle = 24f;
        [SerializeField] private int shotgunDamagePerPellet = 20;

        [Header("Audio SFX")]
        [SerializeField] private AudioClip shootSound;
        [SerializeField] private AudioClip hurtSound;
        [SerializeField] private AudioClip weaponSwitchSound;

        [Header("Visual Feedback")]
        [SerializeField] private Transform modelHolder;
        [SerializeField] private Renderer playerRenderer;

        private CharacterController characterController;
        private Camera mainCamera;
        private Plane aimPlane;
        private Vector3 currentAimPoint;
        private AudioSource audioSource;

        private int currentHealth;
        private float nextFireTime;
        private Vector3 velocity;
        private bool isDead = false;

        private LineRenderer laserLine;
        private GameObject aimReticle;
        private Color originalColor = new Color(0f, 0.85f, 0.95f);

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => isDead;
        public WeaponType CurrentWeapon => currentWeapon;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            characterController.center = new Vector3(0, 1f, 0);
            characterController.radius = 0.5f;
            characterController.height = 2f;
            characterController.skinWidth = 0.08f;
            characterController.minMoveDistance = 0f;

            mainCamera = Camera.main;
            currentHealth = maxHealth;

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.2f;
            }

            if (playerRenderer == null)
            {
                playerRenderer = GetComponent<Renderer>();
            }

            if (playerRenderer != null && playerRenderer.material != null)
            {
                originalColor = playerRenderer.material.color;
            }

            SetupAimAids();
        }

        private void SetupAimAids()
        {
            // 1. Setup Laser Sight (LineRenderer)
            laserLine = gameObject.GetComponent<LineRenderer>();
            if (laserLine == null) laserLine = gameObject.AddComponent<LineRenderer>();

            laserLine.startWidth = 0.06f;
            laserLine.endWidth = 0.02f;
            laserLine.positionCount = 2;
            laserLine.material = new Material(Shader.Find("Sprites/Default"));
            UpdateLaserColor();

            // 2. Setup Reticle Bidik di Tanah
            aimReticle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            aimReticle.name = "Aim_Reticle";
            aimReticle.transform.localScale = new Vector3(0.85f, 0.02f, 0.85f);
            Collider retCol = aimReticle.GetComponent<Collider>();
            if (retCol != null) Destroy(retCol);

            Renderer retRend = aimReticle.GetComponent<Renderer>();
            if (retRend != null)
            {
                Material retMat = new Material(Shader.Find("Sprites/Default"));
                retMat.color = new Color(0f, 1f, 0.9f, 0.65f);
                retRend.material = retMat;
            }
        }

        private void Start()
        {
            aimPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

            if (ArenaGameManager.Instance != null)
            {
                ArenaGameManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
                NotifyWeaponChanged(false);
            }
        }

        private void Update()
        {
            if (isDead)
            {
                if (aimReticle != null) aimReticle.SetActive(false);
                if (laserLine != null) laserLine.enabled = false;
                return;
            }

            if (ArenaGameManager.Instance != null && ArenaGameManager.Instance.IsGameOver)
                return;

            HandleWeaponSwitchInput();
            HandleMovement();
            HandleAiming();
            HandleShooting();
        }

        private void HandleWeaponSwitchInput()
        {
            // Tombol Q untuk toggle senjata, atau angka 1 dan 2, atau scroll wheel mouse
            if (Input.GetKeyDown(KeyCode.Q))
            {
                ToggleWeapon();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetWeapon(WeaponType.PlasmaRifle);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetWeapon(WeaponType.ScatterShotgun);
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.05f)
            {
                ToggleWeapon();
            }
        }

        public void ToggleWeapon()
        {
            WeaponType next = (currentWeapon == WeaponType.PlasmaRifle) ? WeaponType.ScatterShotgun : WeaponType.PlasmaRifle;
            SetWeapon(next);
        }

        public void SetWeapon(WeaponType weapon)
        {
            if (currentWeapon == weapon) return;
            currentWeapon = weapon;
            NotifyWeaponChanged(true);
        }

        private void NotifyWeaponChanged(bool playSound)
        {
            UpdateLaserColor();

            string wName = (currentWeapon == WeaponType.PlasmaRifle) ? "PLASMA RIFLE" : "SCATTER SHOTGUN";
            Color wColor = (currentWeapon == WeaponType.PlasmaRifle) ? new Color(0f, 0.95f, 1f) : new Color(1f, 0.65f, 0.1f);

            if (playSound)
            {
                PlaySound(weaponSwitchSound, 0.85f);
            }

            if (ArenaGameManager.Instance != null)
            {
                ArenaGameManager.Instance.ShowWeaponPopup(wName, wColor);
            }
        }

        private void UpdateLaserColor()
        {
            if (laserLine == null) return;

            Color c = (currentWeapon == WeaponType.PlasmaRifle) ? new Color(0f, 1f, 1f) : new Color(1f, 0.65f, 0.1f);
            laserLine.startColor = new Color(c.r, c.g, c.b, 0.85f);
            laserLine.endColor = new Color(c.r, c.g, c.b, 0.15f);
        }

        private void HandleMovement()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 moveDirection = new Vector3(h, 0f, v).normalized;

            if (characterController.isGrounded)
            {
                velocity.y = -1f;
            }
            else
            {
                velocity.y -= gravity * Time.deltaTime;
            }

            Vector3 motion = (moveDirection * moveSpeed) + new Vector3(0, velocity.y, 0);
            characterController.Move(motion * Time.deltaTime);

            // Batas arena 34x34
            Vector3 clampedPos = transform.position;
            clampedPos.x = Mathf.Clamp(clampedPos.x, -14.2f, 14.2f);
            clampedPos.z = Mathf.Clamp(clampedPos.z, -14.2f, 14.2f);
            transform.position = clampedPos;
        }

        private void HandleAiming()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            aimPlane.SetNormalAndPosition(Vector3.up, new Vector3(0, transform.position.y + 0.3f, 0));

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (aimPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                currentAimPoint = hitPoint;

                Vector3 direction = hitPoint - transform.position;
                direction.y = 0;

                if (direction.sqrMagnitude > 0.05f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 35f);
                }

                if (aimReticle != null)
                {
                    aimReticle.transform.position = new Vector3(hitPoint.x, 0.05f, hitPoint.z);
                }

                if (laserLine != null)
                {
                    Vector3 startPos = firePoint != null ? firePoint.position : (transform.position + transform.forward * 0.8f + Vector3.up * 0.3f);
                    Vector3 laserDir = (hitPoint - startPos);
                    laserDir.y = 0;
                    if (laserDir == Vector3.zero) laserDir = transform.forward;
                    laserDir.Normalize();

                    Vector3 laserEnd = startPos + laserDir * 45f;
                    if (Physics.Raycast(startPos, laserDir, out RaycastHit wallHit, 45f))
                    {
                        if (!wallHit.collider.isTrigger && !wallHit.collider.transform.root.CompareTag("Player"))
                        {
                            laserEnd = wallHit.point;
                        }
                    }

                    laserLine.SetPosition(0, startPos);
                    laserLine.SetPosition(1, laserEnd);
                }
            }
        }

        private void HandleShooting()
        {
            float rate = (currentWeapon == WeaponType.PlasmaRifle) ? rifleFireRate : shotgunFireRate;

            if (Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space))
            {
                if (Time.time >= nextFireTime)
                {
                    nextFireTime = Time.time + rate;
                    if (currentWeapon == WeaponType.PlasmaRifle)
                    {
                        ShootRifle();
                    }
                    else
                    {
                        ShootShotgun();
                    }
                }
            }
        }

        private void ShootRifle()
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : (transform.position + transform.forward * 0.9f + Vector3.up * 0.3f);
            Vector3 shootDir = (currentAimPoint - spawnPos);
            shootDir.y = 0;
            if (shootDir == Vector3.zero) shootDir = transform.forward;
            shootDir.Normalize();

            PlaySound(shootSound, 0.75f);

            SpawnBullet(spawnPos, shootDir, 42f, rifleDamage, new Color(0f, 1f, 1f));

            if (modelHolder != null) modelHolder.localPosition = new Vector3(0, 0, -0.15f);
        }

        private void ShootShotgun()
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : (transform.position + transform.forward * 0.9f + Vector3.up * 0.3f);
            Vector3 baseDir = (currentAimPoint - spawnPos);
            baseDir.y = 0;
            if (baseDir == Vector3.zero) baseDir = transform.forward;
            baseDir.Normalize();

            // Putar sound tembakan shotgun (volume lebih tebal)
            PlaySound(shootSound, 1.0f);

            // Tembakkan sebaran 6 pellet (Scatter)
            for (int i = 0; i < shotgunPellets; i++)
            {
                float angleOffset = Random.Range(-shotgunSpreadAngle * 0.5f, shotgunSpreadAngle * 0.5f);
                Quaternion rot = Quaternion.Euler(0, angleOffset, 0);
                Vector3 pelletDir = rot * baseDir;

                SpawnBullet(spawnPos, pelletDir, 36f, shotgunDamagePerPellet, new Color(1f, 0.65f, 0.1f));
            }

            // Recoil lebih kuat untuk shotgun
            if (modelHolder != null) modelHolder.localPosition = new Vector3(0, 0, -0.3f);
            if (ArenaCameraController.Instance != null) ArenaCameraController.Instance.TriggerShake(0.12f, 0.25f);
        }

        private void SpawnBullet(Vector3 position, Vector3 direction, float speed, int damage, Color bulletColor)
        {
            GameObject bolt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bolt.name = "PlasmaPellet";
            bolt.transform.position = position;
            bolt.transform.localScale = Vector3.one * 0.35f;

            Renderer boltRend = bolt.GetComponent<Renderer>();
            if (boltRend != null)
            {
                boltRend.material.color = bulletColor;
            }

            Projectile proj = bolt.AddComponent<Projectile>();
            proj.Init(direction, speed, damage, true);
        }

        private void LateUpdate()
        {
            if (modelHolder != null)
            {
                modelHolder.localPosition = Vector3.Lerp(modelHolder.localPosition, Vector3.zero, Time.deltaTime * 15f);
            }
        }

        public void TakeDamage(int amount)
        {
            if (isDead) return;

            currentHealth -= amount;
            Debug.Log($"[PlayerController] Player terkena damage {amount}! Sisa HP: {currentHealth}");

            PlaySound(hurtSound, 0.9f);
            StartCoroutine(DamageFlashRoutine());

            if (ArenaCameraController.Instance != null)
            {
                ArenaCameraController.Instance.TriggerShake(0.22f, 0.45f);
            }

            if (ArenaGameManager.Instance != null)
            {
                ArenaGameManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
            }

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        private IEnumerator DamageFlashRoutine()
        {
            if (playerRenderer != null && playerRenderer.material != null)
            {
                playerRenderer.material.color = Color.red;
                yield return new WaitForSeconds(0.12f);
                if (!isDead && playerRenderer != null && playerRenderer.material != null)
                {
                    playerRenderer.material.color = originalColor;
                }
            }
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            Debug.Log("[PlayerController] Player GUGUR di arena!");

            if (aimReticle != null) aimReticle.SetActive(false);
            if (laserLine != null) laserLine.enabled = false;

            SpawnDeathExplosion();

            if (playerRenderer != null) playerRenderer.enabled = false;
            characterController.enabled = false;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (ArenaGameManager.Instance != null)
            {
                ArenaGameManager.Instance.OnPlayerDied();
            }
        }

        private void SpawnDeathExplosion()
        {
            for (int i = 0; i < 10; i++)
            {
                GameObject bit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bit.transform.position = transform.position + Random.insideUnitSphere * 0.5f;
                bit.transform.localScale = Vector3.one * 0.25f;
                Destroy(bit.GetComponent<Collider>());

                Renderer r = bit.GetComponent<Renderer>();
                if (r != null) r.material.color = Color.cyan;

                Rigidbody rb = bit.AddComponent<Rigidbody>();
                rb.AddExplosionForce(400f, transform.position, 3.5f);
                Destroy(bit, 1.5f);
            }
        }

        private void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(clip);
            }
            else if (audioSource != null)
            {
                audioSource.PlayOneShot(clip, volume);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clip, transform.position, volume);
            }
        }

        public void SetAudioClips(AudioClip shoot, AudioClip hurt, AudioClip switchSound = null)
        {
            shootSound = shoot;
            hurtSound = hurt;
            weaponSwitchSound = switchSound;
        }

        private void OnDestroy()
        {
            if (aimReticle != null) Destroy(aimReticle);
        }
    }
}
