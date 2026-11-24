using System.Collections.Generic;
using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Pasang di player dan bot. Menyimpan jumlah gem + tim.
    public class GemCarrier : MonoBehaviour
    {
        public static readonly List<GemCarrier> All = new List<GemCarrier>();
        static bool quitting;

        public int teamId;
        [SerializeField] Gem gemPrefab;
        [SerializeField] float dropForce = 3.5f;

        public int Gems { get; private set; }
        public event System.Action<GemCarrier> OnChanged;   // untuk update UI

        void OnEnable()  => All.Add(this);
        void OnDisable() => All.Remove(this);
        void OnApplicationQuit() => quitting = true;

        public void SetGemPrefab(Gem prefab) => gemPrefab = prefab;

        public void Add(int n)
        {
            Gems += n;
            OnChanged?.Invoke(this);
        }

        // Saat mati (Health.TakeDamage -> Destroy), semua gem tumpah
        void OnDestroy()
        {
            if (quitting || !gameObject.scene.isLoaded || gemPrefab == null) return;

            for (int i = 0; i < Gems; i++)
            {
                var g = Instantiate(gemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
                g.Pop(Random.insideUnitCircle.normalized * Random.Range(1.5f, dropForce));
            }
        }
    }
}
