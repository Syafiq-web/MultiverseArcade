using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Taruh di tengah map. Memunculkan gem tiap beberapa detik.
    public class GemMine : MonoBehaviour
    {
        [SerializeField] Gem gemPrefab;
        [SerializeField] float interval = 7f;
        [SerializeField] float popForce = 3f;
        [SerializeField] int maxGemsOnField = 12;   // cegah gem menumpuk tanpa batas

        float timer;

        public void Setup(Gem prefab, float spawnInterval = 7f)
        {
            gemPrefab = prefab;
            interval = spawnInterval;
        }

        void Update()
        {
            timer += Time.deltaTime;
            if (timer < interval) return;
            timer = 0;

            if (Gem.ActiveCount >= maxGemsOnField || gemPrefab == null) return;

            var g = Instantiate(gemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            g.Pop(Random.insideUnitCircle.normalized * popForce);
        }
    }
}
