using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Pasang di pemain dan bot. Suara + VFX + camera shake saat kena damage dan mati.
    [RequireComponent(typeof(Health))]
    public class HitFeedback : MonoBehaviour
    {
        [SerializeField] bool isLocalPlayer;         // hanya pemain lokal yang mengguncang kamera
        [SerializeField] AudioClip hitClip;
        [SerializeField] AudioClip deathClip;
        [SerializeField] GameObject deathVfx;        // prefab partikel
        [SerializeField] float hitShakePower = 0.1f;
        [SerializeField] float deathShakePower = 0.3f;

        Health hp;
        CameraFollow cam;

        void Awake()
        {
            hp = GetComponent<Health>();
            cam = Object.FindFirstObjectByType<CameraFollow>();
        }

        void OnEnable()
        {
            if (hp != null)
            {
                hp.OnDamaged += HandleDamaged;
                hp.OnDeath += HandleDeath;
            }
        }

        void OnDisable()
        {
            if (hp != null)
            {
                hp.OnDamaged -= HandleDamaged;
                hp.OnDeath -= HandleDeath;
            }
        }

        public void Setup(bool localPlayer, GameObject vfx = null, AudioClip hit = null, AudioClip death = null)
        {
            isLocalPlayer = localPlayer;
            deathVfx = vfx;
            hitClip = hit;
            deathClip = death;
        }

        void HandleDamaged(Health h, int dmg)
        {
            if (hitClip != null) AudioSource.PlayClipAtPoint(hitClip, transform.position, 0.6f);
            if (isLocalPlayer && cam != null) cam.Shake(0.1f, hitShakePower);
        }

        void HandleDeath(Health h)
        {
            if (deathClip != null) AudioSource.PlayClipAtPoint(deathClip, transform.position);
            if (deathVfx != null)
            {
                var v = Instantiate(deathVfx, transform.position, Quaternion.identity);
                Destroy(v, 3f);
            }
            if (isLocalPlayer && cam != null) cam.Shake(0.35f, deathShakePower);
        }
    }
}
