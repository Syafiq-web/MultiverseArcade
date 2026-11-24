using System.Collections.Generic;
using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Mengelola gem yang dibawa unit.
    // Gem dijatuhkan lewat Health.OnDeath (bukan OnDestroy), mendukung sistem respawn.
    [RequireComponent(typeof(Health))]
    public class GemCarrier : MonoBehaviour
    {
        public static readonly List<GemCarrier> All = new List<GemCarrier>();

        public int teamId;
        [SerializeField] Gem gemPrefab;
        [SerializeField] float dropForce = 3.5f;

        public int Gems { get; private set; }
        public event System.Action<GemCarrier> OnChanged;   // untuk update UI

        Health health;

        void Awake() => health = GetComponent<Health>();

        void OnEnable()
        {
            All.Add(this);                     // unit mati (nonaktif) otomatis keluar dari daftar
            if (health != null) health.OnDeath += HandleDeath;
        }

        void OnDisable()
        {
            All.Remove(this);
            if (health != null) health.OnDeath -= HandleDeath;
        }

        public void Add(int n)
        {
            Gems += n;
            OnChanged?.Invoke(this);
        }

        public void Setup(Gem prefab, int team)
        {
            gemPrefab = prefab;
            teamId = team;
        }

        public void SetGemPrefab(Gem prefab) => gemPrefab = prefab;

        void HandleDeath(Health h)
        {
            if (gemPrefab != null)
            {
                for (int i = 0; i < Gems; i++)
                {
                    var g = Instantiate(gemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
                    Vector2 randDir = Random.insideUnitCircle.normalized;
                    if (randDir.sqrMagnitude < 0.01f) randDir = Random.onUnitSphere;
                    g.Pop(randDir * Random.Range(1.5f, dropForce));
                }
            }
            Gems = 0;
            OnChanged?.Invoke(this);
        }
    }
}
