using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Pasang di dinding yang bisa hancur. Tetap beri tag "Wall" agar peluru biasa tertahan.
    // Hanya Super (mis. ShellySuper) yang memanggil Break().
    // Untuk bot: pakai NavMeshObstacle (Carve = true) pada dinding ini, BUKAN Navigation Static,
    // agar jalur NavMesh terbuka otomatis setelah dinding hancur.
    public class BreakableWall : MonoBehaviour
    {
        [SerializeField] GameObject breakVfx;
        [SerializeField] AudioClip breakClip;

        public void Setup(GameObject vfx = null, AudioClip clip = null)
        {
            breakVfx = vfx;
            breakClip = clip;
        }

        public void Break()
        {
            if (breakVfx != null) Destroy(Instantiate(breakVfx, transform.position, transform.rotation), 3f);
            if (breakClip != null) AudioSource.PlayClipAtPoint(breakClip, transform.position);
            Destroy(gameObject);
        }
    }
}
