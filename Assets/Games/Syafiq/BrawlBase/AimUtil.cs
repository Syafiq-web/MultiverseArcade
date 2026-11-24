using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    public static class AimUtil
    {
        // Arah ke musuh terdekat (beda tim, tidak sedang bersembunyi di semak)
        public static bool NearestEnemyDir(Vector3 origin, float range, LayerMask mask, int myTeam, out Vector3 dir)
        {
            dir = Vector3.zero;
            float best = float.MaxValue;

            foreach (var col in Physics.OverlapSphere(origin, range, mask))
            {
                if (!col.TryGetComponent(out GemCarrier c) || c.teamId == myTeam) continue;
                if (col.TryGetComponent(out Hideable h) && h.InBush) continue;

                Vector3 to = col.transform.position - origin;
                to.y = 0;
                float d = to.sqrMagnitude;
                if (d < best) { best = d; dir = to.normalized; }
            }
            return best < float.MaxValue;
        }
    }
}
