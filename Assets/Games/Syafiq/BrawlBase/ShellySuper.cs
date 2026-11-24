using System.Collections.Generic;
using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Ledakan shotgun berbentuk kerucut: damage + knockback besar + menghancurkan BreakableWall.
    public class ShellySuper : SuperAbility
    {
        [SerializeField] float range = 7f;
        [SerializeField, Range(10, 180)] float coneAngle = 80f;
        [SerializeField] int damage = 1400;
        [SerializeField] float knockback = 14f;
        [SerializeField] LayerMask hitMask = ~0;
        [SerializeField] GameObject blastVfx;
        [SerializeField] AudioClip blastClip;

        public void Setup(Transform orig, LayerMask mask, GameObject vfx = null, AudioClip clip = null)
        {
            origin = orig;
            hitMask = mask;
            blastVfx = vfx;
            blastClip = clip;
        }

        public override void Activate(Vector3 dir)
        {
            dir.y = 0;
            dir = dir.sqrMagnitude < 0.001f ? transform.forward : dir.normalized;
            Vector3 o = Origin;

            if (blastVfx != null) Destroy(Instantiate(blastVfx, o, Quaternion.LookRotation(dir)), 2f);
            if (blastClip != null) AudioSource.PlayClipAtPoint(blastClip, o);

            var hit = new HashSet<Health>();      // satu unit bisa punya beberapa collider
            var broken = new HashSet<BreakableWall>();

            foreach (var col in Physics.OverlapSphere(o, range, hitMask, QueryTriggerInteraction.Ignore))
            {
                if (col.transform.IsChildOf(transform)) continue;   // jangan kena diri sendiri

                Vector3 to = col.bounds.center - o;
                to.y = 0;
                if (to.sqrMagnitude > 0.0001f && Vector3.Angle(dir, to) > coneAngle * 0.5f) continue;

                if (col.TryGetComponent(out BreakableWall wall))
                {
                    if (broken.Add(wall)) wall.Break();
                    continue;
                }

                if (!col.TryGetComponent(out Health hp) || !hit.Add(hp)) continue;
                if (hp.TryGetComponent(out GemCarrier other) && other.teamId == TeamId) continue;   // tidak melukai tim sendiri

                hp.TakeDamage(damage);
                Knockback.Apply(hp.gameObject, to.normalized * knockback);
            }
        }
    }
}
