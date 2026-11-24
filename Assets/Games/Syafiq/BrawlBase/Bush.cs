using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Pasang di objek semak dengan Collider (isTrigger = true).
    // Pemain di dalam semak tersembunyi dari musuh, kecuali musuh dekat atau pemain menembak.
    public class Bush : MonoBehaviour
    {
        [SerializeField] float revealDistance = 2.5f;

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Hideable h)) h.EnterBush(this);
        }

        void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Hideable h)) h.ExitBush(this);
        }

        public float RevealDistance => revealDistance;
    }

    public class Hideable : MonoBehaviour
    {
        [SerializeField] Renderer[] renderers;
        [SerializeField] bool isLocalPlayer;
        int bushCount;
        float revealUntil;

        public bool InBush => bushCount > 0;

        void Awake()
        {
            if (renderers == null || renderers.Length == 0)
            {
                renderers = GetComponentsInChildren<Renderer>();
            }
        }

        public void Setup(bool localPlayer)
        {
            isLocalPlayer = localPlayer;
            renderers = GetComponentsInChildren<Renderer>();
        }

        public void EnterBush(Bush b) { bushCount++; Refresh(); }
        public void ExitBush(Bush b)  { bushCount--; Refresh(); }
        public void RevealFor(float seconds) { revealUntil = Time.time + seconds; Refresh(); }

        void Update()
        {
            if (revealUntil > 0 && Time.time > revealUntil)
            {
                revealUntil = 0;
                Refresh();
            }
        }

        public void Refresh()
        {
            bool hidden = InBush && Time.time > revealUntil;
            if (renderers == null) return;
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = isLocalPlayer || !hidden;
            }
        }
    }
}
