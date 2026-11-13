using UnityEngine;

namespace MultiverseArcade.Games.Syafiq
{
    /// <summary>
    /// AI Musuh (Drone / Robot) yang mengejar pemain dan menyerang saat berada dalam jarak tembak/kontak.
    /// </summary>
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
        [SerializeField] private float fireRate = 1.2f;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject projectilePrefab;

        private Transform playerTarget;
        private int currentHealth;
        private bool isDead = false;
        private float nextAttackTime;

        public bool IsDead => isDead;

        private void Start()
        {
            currentHealth = maxHealth;
            GameObject playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null)
            {
                playerTarget = playerGo.transform;
            }
        }

        private void Update()
        {
            if (isDead) return;

            if (playerTarget == null)
            {
                GameObject playerGo = GameObject.FindWithTag("Player");
                if (playerGo != null) playerTarget = playerGo.transform;
                return;
            }

            Vector3 toPlayer = playerTarget.position - transform.position;
            toPlayer.y = 0;
            float distance = toPlayer.magnitude;

            if (toPlayer.sqrMagnitude > 0.05f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }

            if (type == EnemyType.Chaser)
            {
                // Kejar pemain langsung
                transform.position += toPlayer.normalized * moveSpeed * Time.deltaTime;
            }
            else if (type == EnemyType.Shooter)
            {
                // Jaga jarak dan tembak
                if (distance > attackRange)
                {
                    transform.position += toPlayer.normalized * moveSpeed * Time.deltaTime;
                }
                else if (distance < attackRange * 0.5f)
                {
                    // Mundur sedikit jika terlalu dekat
                    transform.position -= toPlayer.normalized * (moveSpeed * 0.6f) * Time.deltaTime;
                }

                // Menyerang berkala
                if (Time.time >= nextAttackTime)
                {
                    nextAttackTime = Time.time + fireRate;
                    ShootAtPlayer();
                }
            }
        }

        private void ShootAtPlayer()
        {
            Vector3 spawnPos = firePoint != null ? firePoint.position : (transform.position + transform.forward * 0.8f + Vector3.up * 0.5f);
            Vector3 shootDir = (playerTarget.position + Vector3.up * 0.5f - spawnPos).normalized;

            if (projectilePrefab != null)
            {
                GameObject projGo = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(shootDir));
                Projectile proj = projGo.GetComponent<Projectile>();
                if (proj != null) proj.Init(shootDir, 16f, 15, false);
            }
            else
            {
                GameObject bolt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bolt.transform.position = spawnPos;
                bolt.transform.localScale = Vector3.one * 0.3f;
                Projectile proj = bolt.AddComponent<Projectile>();
                proj.Init(shootDir, 16f, 15, false);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (isDead || type != EnemyType.Chaser) return;

            if (other.CompareTag("Player"))
            {
                PlayerController player = other.GetComponent<PlayerController>();
                if (player != null && !player.IsDead && Time.time >= nextAttackTime)
                {
                    nextAttackTime = Time.time + 0.8f;
                    player.TakeDamage(contactDamage);
                }
            }
        }

        public void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            isDead = true;

            if (ArenaGameManager.Instance != null)
            {
                ArenaGameManager.Instance.OnEnemyDefeated(scoreValue);
            }

            // Efek kematian sederhana
            Destroy(gameObject);
        }
    }
}
