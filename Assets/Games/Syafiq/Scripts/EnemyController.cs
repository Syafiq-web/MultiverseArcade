using System.Collections;
using UnityEngine;

namespace MultiverseArcade.Games.Syafiq
{
    /// <summary>
    /// AI Musuh (Chaser & Shooter) dengan Rigidbody kinematis, deteksi tabrakan presisi,
    /// flash saat kena tembak, damage ke Player, dan efek ledakan saat gugur.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class EnemyController : MonoBehaviour
    {
        public enum EnemyType { Chaser, Shooter }

        [Header("Tipe & Statistik")]
        [SerializeField] private EnemyType type = EnemyType.Chaser;
        [SerializeField] private int maxHealth = 50;
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private int contactDamage = 15;
        [SerializeField] private int scoreValue = 100;

        [Header("Shooter Specs (Jika Tipe Shooter)")]
        [SerializeField] private float attackRange = 8f;
        [SerializeField] private float fireRate = 1.3f;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject projectilePrefab;

        private Transform playerTarget;
        private PlayerController playerScript;
        private Renderer enemyRenderer;
        private Color baseColor;

        private int currentHealth;
        private bool isDead = false;
        private float nextAttackTime;
        private float nextContactTime;

        public bool IsDead => isDead;

        private void Awake()
        {
            // Pastikan memiliki Rigidbody kinematis agar event trigger physics 100% aktif
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            rb.useGravity = false;

            enemyRenderer = GetComponent<Renderer>();
            if (enemyRenderer != null && enemyRenderer.material != null)
            {
                baseColor = enemyRenderer.material.color;
            }
        }

        private void Start()
        {
            currentHealth = maxHealth;
            FindPlayer();
        }

        private void FindPlayer()
        {
            GameObject playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
            {
                playerTarget = playerGo.transform;
                playerScript = playerGo.GetComponent<PlayerController>();
            }
        }

        private void Update()
        {
            if (isDead) return;

            if (playerTarget == null || playerScript == null || playerScript.IsDead)
            {
                FindPlayer();
                return;
            }

            Vector3 toPlayer = playerTarget.position - transform.position;
            toPlayer.y = 0;
            float distance = toPlayer.magnitude;

            // Putar arah menghadap player
            if (toPlayer.sqrMagnitude > 0.05f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }

            if (type == EnemyType.Chaser)
            {
                // Kejar pemain langsung
                transform.position += toPlayer.normalized * moveSpeed * Time.deltaTime;

                // Garansi jarak dekat (Proximity Damage) jika menempel player
                if (distance <= 1.35f && Time.time >= nextContactTime)
                {
                    nextContactTime = Time.time + 0.8f;
                    playerScript.TakeDamage(contactDamage);
                }
            }
            else if (type == EnemyType.Shooter)
            {
                // Jaga jarak tembak
                if (distance > attackRange)
                {
                    transform.position += toPlayer.normalized * moveSpeed * Time.deltaTime;
                }
                else if (distance < attackRange * 0.5f)
                {
                    // Mundur perlahan jika terlalu dekat
                    transform.position -= toPlayer.normalized * (moveSpeed * 0.6f) * Time.deltaTime;
                }

                // Menembak berkala
                if (Time.time >= nextAttackTime)
                {
                    nextAttackTime = Time.time + fireRate;
                    ShootAtPlayer();
                }
            }
        }

        private void ShootAtPlayer()
        {
            if (playerTarget == null) return;

            Vector3 spawnPos = firePoint != null ? firePoint.position : (transform.position + transform.forward * 0.8f + Vector3.up * 0.4f);
            Vector3 shootDir = (playerTarget.position + Vector3.up * 0.4f - spawnPos).normalized;

            if (projectilePrefab != null)
            {
                GameObject projGo = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(shootDir));
                Projectile proj = projGo.GetComponent<Projectile>();
                if (proj != null) proj.Init(shootDir, 16f, 15, false);
            }
            else
            {
                // Fallback proyektil musuh
                GameObject bolt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bolt.name = "Enemy_PlasmaBolt";
                bolt.transform.position = spawnPos;
                bolt.transform.localScale = Vector3.one * 0.35f;

                Renderer rend = bolt.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = new Color(1f, 0.25f, 0.15f); // Merah/Oranye musuh
                }

                Projectile proj = bolt.AddComponent<Projectile>();
                proj.Init(shootDir, 16f, 15, false);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (isDead || type != EnemyType.Chaser) return;

            if (other.CompareTag("Player") && Time.time >= nextContactTime)
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null && !player.IsDead)
                {
                    nextContactTime = Time.time + 0.8f;
                    player.TakeDamage(contactDamage);
                }
            }
        }

        public void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            Debug.Log($"[EnemyController] Musuh {name} terkena {damage} damage! Sisa HP: {currentHealth}");

            // Flash visual warna putih saat tertembak
            StartCoroutine(HitFlashRoutine());

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private IEnumerator HitFlashRoutine()
        {
            if (enemyRenderer != null && enemyRenderer.material != null)
            {
                enemyRenderer.material.color = Color.white;
                yield return new WaitForSeconds(0.08f);
                if (!isDead && enemyRenderer != null && enemyRenderer.material != null)
                {
                    enemyRenderer.material.color = baseColor;
                }
            }
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            Debug.Log($"[EnemyController] Musuh {name} hancur! +{scoreValue} Skor");

            if (ArenaGameManager.Instance != null)
            {
                ArenaGameManager.Instance.OnEnemyDefeated(scoreValue);
            }

            // Efek partikel hancur
            SpawnDeathEffect();

            Destroy(gameObject);
        }

        private void SpawnDeathEffect()
        {
            for (int i = 0; i < 6; i++)
            {
                GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
                piece.transform.position = transform.position + Random.insideUnitSphere * 0.4f;
                piece.transform.localScale = Vector3.one * 0.2f;
                Destroy(piece.GetComponent<Collider>());

                Renderer r = piece.GetComponent<Renderer>();
                if (r != null) r.material.color = baseColor;

                Rigidbody rb = piece.AddComponent<Rigidbody>();
                rb.AddExplosionForce(250f, transform.position, 2f);
                Destroy(piece, 1.2f);
            }
        }
    }
}
