using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Kelas dasar semua Super. Brawler baru = subclass baru, lalu pasang di prefab-nya.
    public abstract class SuperAbility : MonoBehaviour
    {
        [SerializeField] protected Transform origin;   // titik asal efek (muzzle); kosong = posisi objek

        protected Vector3 Origin => origin != null ? origin.position : transform.position;
        protected int TeamId => TryGetComponent(out GemCarrier c) ? c.teamId : -1;

        public void SetOrigin(Transform orig) => origin = orig;

        public abstract void Activate(Vector3 dir);
    }
}
