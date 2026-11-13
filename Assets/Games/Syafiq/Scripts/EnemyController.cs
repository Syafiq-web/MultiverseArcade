using System.Collections;
using MultiverseArcade.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MultiverseArcade.Games.Syafiq
{
    /// <summary>
    /// AI Musuh Zombie Chibi Kartun dengan Rigidbody kinematis, deteksi tabrakan presisi,
    /// animasi jalan goyang (cartoon waddle), bar darah di atas kepala, hit flash,
    /// knockback saat tertembak, dan efek ledakan lendir saat mati.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class EnemyController : MonoBehaviour
    {
        public enum EnemyType { ChaserZombie, ShooterZombie }

        [Header("Tipe & Statistik")]
        [SerializeField] private EnemyType type = EnemyType.ChaserZombie;
        [SerializeField] private int maxHealth = 50;
        [SerializeField] private float moveSpeed = 4.2f;
        [SerializeField] private int contactDamage = 15;
        [SerializeField] private int scoreValue = 100;

        [Header("Shooter Specs (Jika Tipe Shooter)")]
        [SerializeField] private float attackRange = 8f;
        [SerializeField] private float fireRate = 1.3f;
        [SerializeField] private Transform firePoint;

        [Header("Audio SFX")]
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip explosionSound;

        [Header("Chibi Visual Model")]
        [SerializeField] private Transform modelHolder;
        [SerializeField] private Renderer[] zombieRenderers;
        [SerializeField] private Slider healthBarSlider;

        private Transform playerTarget;
        private PlayerController playerScript;

        private int currentHealth;
        private bool isDead = false;
        private float nextAttackTime;
        private float nextContactTime;
        private Color[] originalColors;

        public bool IsDead => isDead;

        private void Awake()
        {
            // Pastikan tag terpasang
            gameObject.tag = "Enemy";

            CapsuleCollider cc = GetComponent<CapsuleCollider>();
            if (cc != null)
            {
                cc.center = new Vector3(0, 0.9f, 0);
                cc.radius = 0.55f;
                cc.height = 1.8f;
            }

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            rb.useGravity = false;

            CacheRenderers();
        }

        public void CacheRenderers()
        {
            zombieRenderers = GetComponentsInChildren<Renderer>();
            if (zombieRenderers != null && zombieRenderers.Length > 0)
            {
                originalColors = new Color[zombieRenderers.Length];
                for (int i = 0; i < zombieRenderers.Length; i++)
                {
                    if (zombieRenderers[i] != null && zombieRenderers[i].material != null)
                    {
                        originalColors[i] = zombieRenderers[i].material.color;
                    }
                }
            }
        }

        private void Start()
        {
            currentHealth = maxHealth;
            if (healthBarSlider != null)
            {
                healthBarSlider.maxValue = maxHealth;
                healthBarSlider.value = currentHealth;
            }
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

            // Rotasi hadap ke player
            if (toPlayer.sqrMagnitude > 0.05f)
            {
                Quaternion targetRot = Quaternion.LookRotation(toPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }

            // Animasi jalan waddle kartun zombie
            AnimateChibiWalk();

            // Pemisahan antar zombie (anti-clumping)
            Vector3 separationForce = CalculateSeparation();

            if (type == EnemyType.ChaserZombie)
            {
                Vector3 moveDir = (toPlayer.normalized + separationForce).normalized;
                transform.position += moveDir * moveSpeed * Time.deltaTime;

                if (distance <= 1.4f && Time.time >= nextContactTime)
                {
                    nextContactTime = Time.time + 0.8f;
                    playerScript.TakeDamage(contactDamage);
                }
            }
            else if (type == EnemyType.ShooterZombie)
            {
                if (distance > attackRange)
                {
                    Vector3 moveDir = (toPlayer.normalized + separationForce).normalized;
                    transform.position += moveDir * moveSpeed * Time.deltaTime;
                }
                else if (distance < attackRange * 0.5f)
                {
                    Vector3 moveDir = (-toPlayer.normalized + separationForce).normalized;
                    transform.position += moveDir * (moveSpeed * 0.6f) * Time.deltaTime;
                }

                if (Time.time >= nextAttackTime)
                {
                    nextAttackTime = Time.time + fireRate;
                    ShootAtPlayer();
                }
            }

            // Batas arena 34x34
            Vector3 clampedPos = transform.position;
            clampedPos.x = Mathf.Clamp(clampedPos.x, -14.2f, 14.2f);
            clampedPos.z = Mathf.Clamp(clampedPos.z, -14.2f, 14.2f);
            transform.position = clampedPos;
        }

        private void AnimateChibiWalk()
        {
            if (modelHolder != null)
            {
                // Goyangan langkah chibi kartun
                float bob = Mathf.Sin(Time.time * 9f) * 0.06f;
                float tilt = Mathf.Cos(Time.time * 9f) * 4f;
                modelHolder.localPosition = new Vector3(0, bob, 0);
                modelHolder.localRotation = Quaternion.Euler(0, 0, tilt);
            }
        }

        private Vector3 CalculateSeparation()
        {
            Vector3 separation = Vector3.zero;
            Collider[] neighbors = Physics.OverlapSphere(transform.position, 1.4f);
            int count = 0;

            foreach (var col in neighbors)
            {
                if (col.gameObject != gameObject && col.GetComponent<EnemyController>() != null)
                {
                    Vector3 diff = transform.position - col.transform.position;
                    diff.y = 0;
                    if (diff.sqrMagnitude > 0.001f)
                    {
                        separation += diff.normalized / diff.magnitude;
                        count++;
                    }
                }
            }

            if (count > 0) separation /= count;
            return separation * 0.8f;
        }

        private void ShootAtPlayer()
        {
            if (playerTarget == null) return;

            Vector3 spawnPos = firePoint != null ? firePoint.position : (transform.position + transform.forward * 0.8f + Vector3.up * 0.9f);
            Vector3 shootDir = (playerTarget.position + Vector3.up * 0.4f - spawnPos).normalized;

            GameObject bolt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bolt.name = "Zombie_SlimeSpit";
            bolt.transform.position = spawnPos;
            bolt.transform.localScale = Vector3.one * 0.38f;

            Renderer rend = bolt.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = new Color(0.95f, 0.45f, 0.1f);
            }

            Projectile proj = bolt.AddComponent<Projectile>();
            proj.Init(shootDir, 20f, 15, false);
        }

        public void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHealth -= damage;
            Debug.Log($"[EnemyController] Zombie {name} terkena {damage} damage! Sisa HP: {currentHealth}/{maxHealth}");

            // Update Health Bar Billboard
            if (healthBarSlider != null)
            {
                healthBarSlider.value = currentHealth;
            }

            // Knockback sedikit menjauh dari tembakan
            if (playerTarget != null)
            {
                Vector3 away = (transform.position - playerTarget.position).normalized;
                away.y = 0;
                transform.position += away * 0.35f;
            }

            PlaySound(hitSound, 0.65f);
            StartCoroutine(HitFlashRoutine());

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private IEnumerator HitFlashRoutine()
        {
            if (zombieRenderers != null && zombieRenderers.Length > 0)
            {
                for (int i = 0; i < zombieRenderers.Length; i++)
                {
                    if (zombieRenderers[i] != null && zombieRenderers[i].material != null)
                        zombieRenderers[i].material.color = Color.white;
                }

                yield return new WaitForSeconds(0.09f);

                if (!isDead && zombieRenderers != null)
                {
                    for (int i = 0; i < zombieRenderers.Length; i++)
                    {
                        if (zombieRenderers[i] != null && zombieRenderers[i].material != null && originalColors != null && i < originalColors.Length)
                            zombieRenderers[i].material.color = originalColors[i];
                    }
                }
            }
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            Debug.Log($"[EnemyController] Zombie {name} GUGUR! +{scoreValue} Skor");

            PlaySound(explosionSound, 0.85f);

            if (ArenaGameManager.Instance != null)
            {
                ArenaGameManager.Instance.OnEnemyDefeated(scoreValue);
            }

            SpawnZombieSplatter();
            Destroy(gameObject);
        }

        private void SpawnZombieSplatter()
        {
            for (int i = 0; i < 9; i++)
            {
                GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
                piece.transform.position = transform.position + Vector3.up * 0.8f + Random.insideUnitSphere * 0.4f;
                piece.transform.localScale = Vector3.one * Random.Range(0.18f, 0.28f);
                Destroy(piece.GetComponent<Collider>());

                Renderer r = piece.GetComponent<Renderer>();
                if (r != null)
                {
                    r.material.color = (i % 2 == 0) ? new Color(0.35f, 0.75f, 0.35f) : new Color(0.4f, 0.4f, 0.8f);
                }

                Rigidbody rb = piece.AddComponent<Rigidbody>();
                rb.AddExplosionForce(320f, transform.position, 2.5f);
                Destroy(piece, 1.3f);
            }
        }

        private void PlaySound(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(clip);
            }
            else
            {
                AudioSource.PlayClipAtPoint(clip, transform.position, volume);
            }
        }

        public void SetAudioClips(AudioClip hit, AudioClip explosion)
        {
            hitSound = hit;
            explosionSound = explosion;
        }

        public void SetupHealthBar(Slider slider)
        {
            healthBarSlider = slider;
        }

        public void SetModelHolder(Transform holder)
        {
            modelHolder = holder;
        }
    }
}
