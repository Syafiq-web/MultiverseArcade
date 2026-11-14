using System.Collections.Generic;
using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Object Pool khusus untuk proyektil blaster (kapasitas 60).
    /// Mencegah alokasi memori GC per frame dan menjamin performa 60 FPS.
    /// Sesuai GDD Bagian 5.5 & 5.17.
    /// </summary>
    public class ProjectilePool : MonoBehaviour
    {
        public static ProjectilePool Instance { get; private set; }

        [Header("Pool Settings")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private int _poolSize = 60;

        private readonly Queue<Projectile> _availablePool = new Queue<Projectile>();
        private readonly List<Projectile> _allSpawned = new List<Projectile>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            PrewarmPool();
        }

        public void SetPrefab(GameObject prefab)
        {
            _projectilePrefab = prefab;
            PrewarmPool();
        }

        private void PrewarmPool()
        {
            if (_projectilePrefab == null) return;

            // Pastikan layer projectile saling mengabaikan
            int projLayer = LayerMask.NameToLayer("Syafiq_Projectile");
            if (projLayer >= 0)
            {
                Physics.IgnoreLayerCollision(projLayer, projLayer, true);
            }

            while (_allSpawned.Count < _poolSize)
            {
                GameObject obj = Instantiate(_projectilePrefab, transform);
                obj.name = $"PF_Projectile_{_allSpawned.Count + 1}";
                obj.SetActive(false);

                Projectile p = obj.GetComponent<Projectile>();
                if (p == null) p = obj.AddComponent<Projectile>();

                _allSpawned.Add(p);
                _availablePool.Enqueue(p);
            }
        }

        public Projectile SpawnProjectile(Vector3 position, Vector3 direction, int teamId, GameObject owner, SO_Projectile data, float damageMultiplier = 1.0f)
        {
            Projectile p = null;
            if (_availablePool.Count > 0)
            {
                p = _availablePool.Dequeue();
            }
            else
            {
                // Fallback instansiasi jika melebihi batas 60
                GameObject obj = Instantiate(_projectilePrefab, transform);
                p = obj.GetComponent<Projectile>();
                if (p == null) p = obj.AddComponent<Projectile>();
                _allSpawned.Add(p);
            }

            p.Launch(position, direction, teamId, owner, data, damageMultiplier);
            return p;
        }

        public void ReturnProjectile(Projectile p)
        {
            if (p == null) return;
            p.gameObject.SetActive(false);
            p.transform.SetParent(transform);

            if (!_availablePool.Contains(p))
            {
                _availablePool.Enqueue(p);
            }
        }

        public void ClearPool()
        {
            foreach (var p in _allSpawned)
            {
                if (p != null)
                {
                    p.gameObject.SetActive(false);
                    if (!_availablePool.Contains(p))
                    {
                        _availablePool.Enqueue(p);
                    }
                }
            }
        }
    }
}
