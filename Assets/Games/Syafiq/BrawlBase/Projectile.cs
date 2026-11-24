using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] float speed = 16f;
        [SerializeField] int damage = 800;

        Vector3 startPos;
        float maxRange;
        GameObject owner;

        public void Init(float range, GameObject shooter)
        {
            maxRange = range;
            owner = shooter;
            startPos = transform.position;
            GetComponent<Collider>().isTrigger = true;
            GetComponent<Rigidbody>().isKinematic = true;
        }

        void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            if ((transform.position - startPos).sqrMagnitude >= maxRange * maxRange)
            {
                Destroy(gameObject);   // hilang di jarak maksimal
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == owner || (owner != null && other.transform.IsChildOf(owner.transform))) return;

            // Dinding menghentikan peluru
            if (other.CompareTag("Wall"))
            {
                Destroy(gameObject);
                return;
            }

            // Jangan tembak teman satu tim (friendly fire check)
            if (other.TryGetComponent(out GemCarrier target) &&
                owner != null && owner.TryGetComponent(out GemCarrier shooter) &&
                target.teamId == shooter.teamId)
            {
                return;
            }

            if (other.TryGetComponent(out Health hp))
            {
                hp.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }
}
