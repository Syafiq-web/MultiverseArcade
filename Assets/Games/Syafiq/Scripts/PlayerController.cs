using UnityEngine;

namespace MultiverseArcade.Games.Syafiq
{
    /// <summary>
    /// Mengontrol pergerakan, arah bidik (mouse aim), dan penembakan peluru Player.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float gravity = 20f;

        [Header("Combat")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float fireRate = 0.18f;
        [SerializeField] private int maxHealth = 100;

        [Header("Visual Feedback")]
        [SerializeField] private Transform modelHolder;

        private CharacterController characterController;
        private Camera mainCamera;
        private Plane aimPlane;

        private int currentHealth;
        private float nextFireTime;
        private Vector3 velocity;
        private bool isDead = false;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => isDead;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            mainCamera = Camera.main;
            aimPlane = new Plane(Vector3.up, Vector3.zero);
            currentHealth = maxHealth;
        }

        private void Start()
        {
            if (ArenaGameManager.Instance != null)
            {
                ArenaGameManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
            }
        }

        private void Update()
        {
            if (isDead || (ArenaGameManager.Instance != null && ArenaGameManager.Instance.IsGameOver))
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
        }

        private void HandleAiming()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (aimPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                Vector3 lookTarget = new Vector3(hitPoint.x, transform.position.y, hitPoint.z);
                Vector3 direction = lookTarget - transform.position;

                if (direction.sqrMagnitude > 0.05f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 25f);
                }
            }
        }

        private void HandleShooting()
        {
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
            Vector3 spawnPos = firePoint != null ? firePoint.position : (transform.position + transform.forward * 1.0f + Vector3.up * 0.8f);
            Quaternion spawnRot = transform.rotation;

            if (projectilePrefab != null)
            {
                Instantiate(projectilePrefab, spawnPos, spawnRot);
            }
            else
            {
                // Fallback projectile jika prefab belum di-assign
                GameObject bolt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bolt.transform.position = spawnPos;
                bolt.transform.rotation = spawnRot;
                bolt.transform.localScale = Vector3.one * 0.35f;
                Projectile proj = bolt.AddComponent<Projectile>();
                proj.Init(transform.forward, 25f, 25, true);
            }

            // Recoil kecil
            if (modelHolder != null)
            {
                modelHolder.localPosition = new Vector3(0, 0, -0.1f);
            }
        }

        private void LateUpdate()
        {
            // Pulihkan recoil
            if (modelHolder != null)
            {
                modelHolder.localPosition = Vector3.Lerp(modelHolder.localPosition, Vector3.zero, Time.deltaTime * 15f);
            }
        }

        public void TakeDamage(int amount)
        {
            if (isDead) return;

            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }

            if (ArenaGameManager.Instance != null)
            {
                ArenaGameManager.Instance.UpdatePlayerHealth(currentHealth, maxHealth);
            }
        }

        private void Die()
        {
            isDead = true;
            Debug.Log("[PlayerController] Player gugur di arena!");
            if (ArenaGameManager.Instance != null)
            {
                ArenaGameManager.Instance.OnPlayerDied();
            }
        }
    }
}
