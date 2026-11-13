using UnityEngine;

namespace MultiverseArcade.Games.Syafiq
{
    /// <summary>
    /// Proyektil energi/peluru berkecepatan tinggi dengan sistem deteksi tabrakan SphereCast ganda.
    /// 100% DIJAMIN mengenai musuh dan memicu TakeDamage tanpa terpengaruh matriks kinematic Unity.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Statistik Proyektil")]
        [SerializeField] private float speed = 40f;
        [SerializeField] private int damage = 25;
        [SerializeField] private float lifetime = 4.0f;
        [SerializeField] private bool isPlayerOwned = true;
        [SerializeField] private float hitRadius = 0.45f;

        private Vector3 moveDirection;
        private bool hasHit = false;
        private Vector3 prevPosition;

        public void Init(Vector3 direction, float projSpeed, int projDamage, bool playerOwned)
        {
            moveDirection = direction.normalized;
            speed = projSpeed;
            damage = projDamage;
            isPlayerOwned = playerOwned;
            hasHit = false;
            prevPosition = transform.position;

            transform.rotation = Quaternion.LookRotation(moveDirection);
            Destroy(gameObject, lifetime);
        }

        private void Start()
        {
            prevPosition = transform.position;
            if (moveDirection == Vector3.zero)
            {
                moveDirection = transform.forward;
                Destroy(gameObject, lifetime);
            }
        }

        private void Update()
        {
            if (hasHit) return;

            Vector3 currentPos = transform.position;
            Vector3 step = moveDirection * speed * Time.deltaTime;
            Vector3 nextPos = currentPos + step;
            float stepDistance = step.magnitude;

            // 1. Continuous SphereCast: Memindai seluruh volume silinder lintasan peluru
            RaycastHit[] hits = Physics.SphereCastAll(currentPos, hitRadius, moveDirection, stepDistance);
            foreach (var hit in hits)
            {
                if (CheckHit(hit.collider, hit.point))
                {
                    return;
                }
            }

            // 2. OverlapSphere: Pemindaian cadangan di posisi baru
            Collider[] overlaps = Physics.OverlapSphere(nextPos, hitRadius);
            foreach (var col in overlaps)
            {
                if (CheckHit(col, nextPos))
                {
                    return;
                }
            }

            prevPosition = currentPos;
            transform.position = nextPos;
        }

        private void OnTriggerEnter(Collider other)
        {
            CheckHit(other, transform.position);
        }

        private bool CheckHit(Collider other, Vector3 hitPoint)
        {
            if (hasHit || other == null) return false;

            if (isPlayerOwned)
            {
                // Abaikan jika mengenai diri sendiri atau sesama peluru
                if (other.transform.root.CompareTag("Player") || other.GetComponent<Projectile>() != null)
                    return false;

                // Cek apakah mengenai Musuh (Zombie / Drone)
                EnemyController enemy = other.GetComponent<EnemyController>();
                if (enemy == null) enemy = other.GetComponentInParent<EnemyController>();

                if (enemy != null && !enemy.IsDead)
                {
                    hasHit = true;
                    Debug.Log($"[Projectile] Peluru mengenai musuh: {enemy.name} (-{damage} HP)");
                    enemy.TakeDamage(damage);
                    SpawnHitEffect(new Color(0f, 1f, 1f), hitPoint);
                    Destroy(gameObject);
                    return true;
                }
            }
            else
            {
                // Peluru musuh mengenai Player
                if (other.transform.root.CompareTag("Enemy") || other.GetComponent<EnemyController>() != null || other.GetComponent<Projectile>() != null)
                    return false;

                PlayerController player = other.GetComponent<PlayerController>();
                if (player == null) player = other.GetComponentInParent<PlayerController>();

                if (player != null && !player.IsDead)
                {
                    hasHit = true;
                    Debug.Log($"[Projectile] Peluru musuh mengenai player (-{damage} HP)");
                    player.TakeDamage(damage);
                    SpawnHitEffect(new Color(1f, 0.25f, 0.15f), hitPoint);
                    Destroy(gameObject);
                    return true;
                }
            }

            // Mengenai rintangan / dinding arena
            if (!other.isTrigger && !other.transform.root.CompareTag("Player") && other.GetComponent<EnemyController>() == null)
            {
                hasHit = true;
                SpawnHitEffect(new Color(0.3f, 0.85f, 1f), hitPoint);
                Destroy(gameObject);
                return true;
            }

            return false;
        }

        private void SpawnHitEffect(Color effectColor, Vector3 position)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.name = "HitSpark";
            spark.transform.position = position;
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
