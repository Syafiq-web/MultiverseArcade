using System.Collections;
using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Badai peluru: puluhan peluru dalam satu arah terkunci selama beberapa detik.
    public class ColtSuper : SuperAbility
    {
        [SerializeField] Projectile bulletPrefab;   // matikan "Grants Super Charge" di prefab ini
        [SerializeField] int bullets = 24;
        [SerializeField] float duration = 1.2f;
        [SerializeField] float spreadAngle = 4f;
        [SerializeField] float range = 11f;

        bool firing;

        void OnDisable() => firing = false;   // coroutine berhenti saat unit mati

        public void Setup(Projectile prefab, Transform orig, int bulletCount = 24, float dur = 1.2f, float rng = 11f)
        {
            bulletPrefab = prefab;
            origin = orig;
            bullets = bulletCount;
            duration = dur;
            range = rng;
        }

        public override void Activate(Vector3 dir)
        {
            if (firing) return;
            dir.y = 0;
            dir = dir.sqrMagnitude < 0.001f ? transform.forward : dir.normalized;
            StartCoroutine(Storm(dir));
        }

        IEnumerator Storm(Vector3 dir)
        {
            firing = true;
            float wait = duration / Mathf.Max(1, bullets);

            for (int i = 0; i < bullets; i++)
            {
                Quaternion spread = Quaternion.Euler(0, Random.Range(-spreadAngle, spreadAngle), 0);
                if (bulletPrefab != null)
                {
                    var p = Instantiate(bulletPrefab, Origin, Quaternion.LookRotation(spread * dir));
                    p.Init(range, gameObject);
                }
                yield return new WaitForSeconds(wait);
            }
            firing = false;
        }
    }
}
