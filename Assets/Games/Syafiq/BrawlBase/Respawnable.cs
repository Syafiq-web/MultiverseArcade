using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Pasang di pemain dan bot (bersama Health + GemCarrier).
    [RequireComponent(typeof(Health), typeof(GemCarrier))]
    public class Respawnable : MonoBehaviour
    {
        [SerializeField] float invulnerableSeconds = 2f;
        [SerializeField] GameObject shieldVfx;       // opsional: bola/bubble di sekitar karakter
        [SerializeField] AudioClip spawnClip;

        public int TeamId => carrier != null ? carrier.teamId : 0;
        public Vector3 StartPos { get; private set; }
        public Quaternion StartRot { get; private set; }

        Health hp;
        GemCarrier carrier;

        void Awake()
        {
            hp = GetComponent<Health>();
            carrier = GetComponent<GemCarrier>();
            if (hp != null) hp.destroyOnDeath = false; // jangan dihancurkan, unit respawn
            StartPos = transform.position;
            StartRot = transform.rotation;
        }

        void OnEnable()
        {
            if (hp != null) hp.OnDeath += HandleDeath;
        }

        void OnDisable()
        {
            if (hp != null) hp.OnDeath -= HandleDeath;
            if (shieldVfx != null) shieldVfx.SetActive(false);
        }

        public void Setup(GameObject shieldEffect, AudioClip clip = null)
        {
            shieldVfx = shieldEffect;
            spawnClip = clip;
            if (shieldVfx != null) shieldVfx.SetActive(false);
        }

        void HandleDeath(Health h)
        {
            if (RespawnManager.Instance != null) RespawnManager.Instance.Schedule(this);
        }

        // Dipanggil RespawnManager setelah delay selesai
        public void Respawn(Vector3 pos, Quaternion rot)
        {
            // CharacterController harus dimatikan dulu agar posisi baru tidak ditimpa engine
            var cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            transform.SetPositionAndRotation(pos, rot);
            gameObject.SetActive(true);

            if (cc != null) cc.enabled = true;
            if (TryGetComponent(out NavMeshAgent agent) && agent.isOnNavMesh) agent.Warp(pos);

            if (hp != null)
            {
                hp.ResetFull();
                hp.SetInvulnerable(invulnerableSeconds);
            }

            // Reset amunisi penuh saat respawn
            if (TryGetComponent(out AimController ac))
            {
                ac.Ammo = ac.MaxAmmo;
            }

            // Reset bush count dan gerak knockback
            if (TryGetComponent(out Hideable h)) h.ResetBush();
            if (TryGetComponent(out PlayerController pc)) pc.ResetMotion();
            if (spawnClip != null) AudioSource.PlayClipAtPoint(spawnClip, pos);

            StartCoroutine(ShieldRoutine());
        }

        IEnumerator ShieldRoutine()
        {
            if (shieldVfx != null) shieldVfx.SetActive(true);
            yield return new WaitForSeconds(invulnerableSeconds);
            if (shieldVfx != null) shieldVfx.SetActive(false);
        }
    }
}
