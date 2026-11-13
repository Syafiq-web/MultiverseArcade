using UnityEngine;

namespace MultiverseArcade.Games.Syafiq
{
    /// <summary>
    /// Proyektil energi/peluru yang ditembakkan oleh Player atau Musuh.
    /// Dilengkapi Rigidbody kinematis dan Trigger collider agar deteksi tabrakan 100% konsisten.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 28f;
        [SerializeField] private int damage = 25;
        [SerializeField] private float lifetime = 3.0f;
        [SerializeField] private bool isPlayerOwned = true;

        private Vector3 moveDirection;
        private Rigidbody rb;
        private Collider col;
        private bool hasHit = false;

        private void Awake()
        {
            col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        public void Init(Vector3 direction, float projSpeed, int projDamage, bool playerOwned)
        {
            moveDirection = direction.normalized;
            speed = projSpeed;
            damage = projDamage;
            isPlayerOwned = playerOwned;
            hasHit = false;

            transform.rotation = Quaternion.LookRotation(moveDirection);
            Destroy(gameObject, lifetime);
        }

        private void Start()
        {
            if (moveDirection == Vector3.zero)
            {
                moveDirection = transform.forward;
                Destroy(gameObject, lifetime);
            }
        }

        private void Update()
        {
            if (hasHit) return;

            // Pergerakan peluru
            Vector3 step = moveDirection * speed * Time.deltaTime;
            
            // Sweep test / raycast pendek untuk mencegah tunnelling pada fps tinggi
            if (Physics.Raycast(transform.position, moveDirection, out RaycastHit hit, step.magnitude * 1.5f))
            {
                if (CheckAndApplyHit(hit.collider))
                {
                    return;
                }
            }

            transform.position += step;
        }

        private void OnTriggerEnter(Collider other)
        {
            CheckAndApplyHit(other);
        }

        private bool CheckAndApplyHit(Collider other)
        {
            if (hasHit || other == null) return false;

            if (isPlayerOwned)
            {
                // Jangan tabrak diri sendiri atau sesama peluru
                if (other.CompareTag("Player") || other.GetComponent<Projectile>() != null)
                    return false;

                // Mengenai Musuh
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy == null) enemy = other.GetComponentInParent<EnemyController>();

                if (enemy != null && !enemy.IsDead)
                {
                    hasHit = true;
                    enemy.TakeDamage(damage);
                    SpawnHitEffect(new Color(0f, 1f, 1f)); // Percikan Cyan
                    Destroy(gameObject);
                    return true;
                }
            }
            else
            {
                // Jangan tabrak musuh lain atau sesama peluru
                if (other.GetComponent<EnemyController>() != null || other.GetComponent<Projectile>() != null)
                    return false;

                // Mengenai Player
                PlayerController player = other.GetComponent<PlayerController>();
                if (player == null) player = other.GetComponentInParent<PlayerController>();

                if (player != null && !player.IsDead)
                {
                    hasHit = true;
                    player.TakeDamage(damage);
                    SpawnHitEffect(new Color(1f, 0.3f, 0.2f)); // Percikan Merah
                    Destroy(gameObject);
                    return true;
                }
            }

            // Mengenai rintangan / dinding (Bukan Trigger lain)
            if (!other.isTrigger)
            {
                hasHit = true;
                SpawnHitEffect(Color.yellow);
                Destroy(gameObject);
                return true;
            }

            return false;
        }

        private void SpawnHitEffect(Color effectColor)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.name = "HitSpark";
            spark.transform.position = transform.position;
            spark.transform.localScale = Vector3.one * 0.35f;

            Collider sparkCol = spark.GetComponent<Collider>();
            if (sparkCol != null) Destroy(sparkCol);

            Renderer rend = spark.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = effectColor;
            }

            Destroy(spark, 0.12f);
        }
    }
}
