using UnityEngine;

namespace MultiverseArcade.Games.Syafiq
{
    /// <summary>
    /// Proyektil energi/peluru yang ditembakkan oleh Player atau Musuh.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 25f;
        [SerializeField] private int damage = 25;
        [SerializeField] private float lifetime = 3.5f;
        [SerializeField] private bool isPlayerOwned = true;

        private Vector3 moveDirection;

        public void Init(Vector3 direction, float projSpeed, int projDamage, bool playerOwned)
        {
            moveDirection = direction.normalized;
            speed = projSpeed;
            damage = projDamage;
            isPlayerOwned = playerOwned;
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
            transform.position += moveDirection * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isPlayerOwned)
            {
                // Mengenai Musuh
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy == null) enemy = other.GetComponentInParent<EnemyController>();

                if (enemy != null && !enemy.IsDead)
                {
                    enemy.TakeDamage(damage);
                    SpawnHitEffect();
                    Destroy(gameObject);
                    return;
                }
            }
            else
            {
                // Mengenai Player
                PlayerController player = other.GetComponent<PlayerController>();
                if (player == null) player = other.GetComponentInParent<PlayerController>();

                if (player != null && !player.IsDead)
                {
                    player.TakeDamage(damage);
                    SpawnHitEffect();
                    Destroy(gameObject);
                    return;
                }
            }

            // Mengenai rintangan/dinding (bukan trigger lain)
            if (!other.isTrigger && !other.CompareTag("Player") && !other.CompareTag("Enemy"))
            {
                SpawnHitEffect();
                Destroy(gameObject);
            }
        }

        private void SpawnHitEffect()
        {
            // Efek percikan sederhana
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.transform.position = transform.position;
            spark.transform.localScale = Vector3.one * 0.25f;
            Collider col = spark.GetComponent<Collider>();
            if (col != null) Destroy(col);
            Destroy(spark, 0.15f);
        }
    }
}
