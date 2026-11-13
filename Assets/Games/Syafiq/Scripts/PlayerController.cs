using System.Collections;
using UnityEngine;

namespace MultiverseArcade.Games.Syafiq
{
    /// <summary>
    /// Mengontrol pergerakan (WASD), arah bidik presisi (Mouse Aim dengan Reticle/Laser),
    /// penembakan peluru plasma, penerimaan damage, flash efek, dan kematian hero.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float gravity = 20f;

        [Header("Combat & Aiming")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float fireRate = 0.18f;
        [SerializeField] private int maxHealth = 100;

        [Header("Visual Feedback")]
        [SerializeField] private Transform modelHolder;
        [SerializeField] private Renderer playerRenderer;

        private CharacterController characterController;
        private Camera mainCamera;
        private Plane aimPlane;
        private Vector3 currentAimPoint;

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

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            mainCamera = Camera.main;
            currentHealth = maxHealth;

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

            laserLine.startWidth = 0.05f;
            laserLine.endWidth = 0.02f;
            laserLine.positionCount = 2;
            laserLine.material = new Material(Shader.Find("Sprites/Default"));
            laserLine.startColor = new Color(0f, 1f, 1f, 0.7f);
            laserLine.endColor = new Color(0f, 1f, 1f, 0.1f);

            // 2. Setup Reticle Bidik di Tanah
            aimReticle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            aimReticle.name = "Aim_Reticle";
            aimReticle.transform.localScale = new Vector3(0.8f, 0.02f, 0.8f);
            Collider retCol = aimReticle.GetComponent<Collider>();
            if (retCol != null) Destroy(retCol);

            Renderer retRend = aimReticle.GetComponent<Renderer>();
            if (retRend != null)
            {
                Material retMat = new Material(Shader.Find("Sprites/Default"));
                retMat.color = new Color(0f, 1f, 0.9f, 0.6f);
                retRend.material = retMat;
            }
        }

        private void Start()
        {
            aimPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

            if (ArenaGameManager.Instance != null)
            {
                ArenaGameManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
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

            HandleMovement();
            HandleAiming();
            HandleShooting();
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

            // Batasi posisi agar tidak keluar dari arena (30x30)
            Vector3 clampedPos = transform.position;
            clampedPos.x = Mathf.Clamp(clampedPos.x, -14.5f, 14.5f);
            clampedPos.z = Mathf.Clamp(clampedPos.z, -14.5f, 14.5f);
            transform.position = clampedPos;
        }

        private void HandleAiming()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            // Update bidang bidik sejajar tinggi badan player
            aimPlane.SetNormalAndPosition(Vector3.up, new Vector3(0, transform.position.y, 0));

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (aimPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                currentAimPoint = hitPoint;

                Vector3 direction = hitPoint - transform.position;
                direction.y = 0; // Kunci pada sumbu horizontal

                if (direction.sqrMagnitude > 0.05f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 35f);
                }

                // Update posisi Reticle di tanah
                if (aimReticle != null)
                {
                    aimReticle.transform.position = new Vector3(hitPoint.x, 0.05f, hitPoint.z);
                }

                // Update Garis Laser Sight dari Moncong Senjata
                if (laserLine != null)
                {
                    Vector3 startPos = firePoint != null ? firePoint.position : (transform.position + transform.forward * 0.8f + Vector3.up * 0.3f);
                    laserLine.SetPosition(0, startPos);
                    laserLine.SetPosition(1, new Vector3(hitPoint.x, startPos.y, hitPoint.z));
                }
            }
        }

        private void HandleShooting()
        {
            // Tembak dengan Klik Kiri Mouse atau Spacebar
            if (Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space))
            {
                if (Time.time >= nextFireTime)
                {
                    nextFireTime = Time.time + fireRate;
                    Shoot();
                }
            }
        }

        private void Shoot()
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : (transform.position + transform.forward * 0.9f + Vector3.up * 0.3f);
            Vector3 shootDir = (currentAimPoint - spawnPos);
            shootDir.y = 0; // Tembakan mendatar rata dengan tubuh

            if (shootDir == Vector3.zero)
            {
                shootDir = transform.forward;
            }
            shootDir.Normalize();

            if (projectilePrefab != null)
            {
                GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(shootDir));
                Projectile pComp = proj.GetComponent<Projectile>();
                if (pComp != null) pComp.Init(shootDir, 30f, 30, true);
            }
            else
            {
                // Fallback projectile procedural
                GameObject bolt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bolt.name = "Player_PlasmaBolt";
                bolt.transform.position = spawnPos;
                bolt.transform.localScale = Vector3.one * 0.4f;

                Renderer boltRend = bolt.GetComponent<Renderer>();
                if (boltRend != null)
                {
                    boltRend.material.color = new Color(0f, 1f, 1f); // Cyan terang
                }

                Projectile proj = bolt.AddComponent<Projectile>();
                proj.Init(shootDir, 30f, 30, true);
            }

            // Recoil
            if (modelHolder != null)
            {
                modelHolder.localPosition = new Vector3(0, 0, -0.15f);
            }
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

            // Flash visual warna merah
            StartCoroutine(DamageFlashRoutine());

            // Screen shake
            if (ArenaCameraController.Instance != null)
            {
                ArenaCameraController.Instance.TriggerShake(0.2f, 0.4f);
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

            // Efek partikel ledakan kematian player
            SpawnDeathExplosion();

            // Sembunyikan badan player
            if (playerRenderer != null) playerRenderer.enabled = false;
            characterController.enabled = false;

            if (ArenaGameManager.Instance != null)
            {
                ArenaGameManager.Instance.OnPlayerDied();
            }
        }

        private void SpawnDeathExplosion()
        {
            for (int i = 0; i < 8; i++)
            {
                GameObject bit = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bit.transform.position = transform.position + Random.insideUnitSphere * 0.5f;
                bit.transform.localScale = Vector3.one * 0.25f;
                Destroy(bit.GetComponent<Collider>());

                Renderer r = bit.GetComponent<Renderer>();
                if (r != null) r.material.color = Color.cyan;

                Rigidbody rb = bit.AddComponent<Rigidbody>();
                rb.AddExplosionForce(350f, transform.position, 3f);
                Destroy(bit, 1.5f);
            }
        }

        private void OnDestroy()
        {
            if (aimReticle != null) Destroy(aimReticle);
        }
    }
}
