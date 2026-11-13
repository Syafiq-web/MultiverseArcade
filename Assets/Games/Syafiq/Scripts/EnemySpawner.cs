using System.Collections;
using UnityEngine;

namespace MultiverseArcade.Games.Syafiq
{
    /// <summary>
    /// Mengelola spawn wave musuh (Chasers dan Shooters) di titik-titik sekitar arena.
    /// Dilengkapi audio SFX dan konfigurasi collider fisik yang sempurna.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner Instance { get; private set; }

        [Header("Prefabs Musuh")]
        [SerializeField] private GameObject chaserPrefab;
        [SerializeField] private GameObject shooterPrefab;

        [Header("Audio SFX untuk Musuh")]
        [SerializeField] private AudioClip enemyHitClip;
        [SerializeField] private AudioClip enemyExplosionClip;

        [Header("Spawn Positions")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float arenaSpawnRadius = 12.5f;

        [Header("Wave Configuration")]
        [SerializeField] private float timeBetweenWaves = 3.5f;
        [SerializeField] private float spawnInterval = 0.8f;

        private int currentWave = 0;
        private int enemiesToSpawnThisWave = 0;
        private int enemiesAlive = 0;
        private bool isSpawningWave = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            StartCoroutine(StartFirstWaveRoutine());
        }

        private IEnumerator StartFirstWaveRoutine()
        {
            yield return new WaitForSeconds(1.0f);
            StartNextWave();
        }

        public void StartNextWave()
        {
            if (isSpawningWave) return;

            currentWave++;
            if (ArenaGameManager.Instance != null)
            {
                ArenaGameManager.Instance.SetWave(currentWave);
            }

            StartCoroutine(SpawnWaveRoutine(currentWave));
        }

        private IEnumerator SpawnWaveRoutine(int waveNumber)
        {
            isSpawningWave = true;

            int chaserCount = 3 + (waveNumber * 2);
            int shooterCount = waveNumber >= 2 ? (waveNumber - 1) * 2 : 0;
            enemiesToSpawnThisWave = chaserCount + shooterCount;
            enemiesAlive = enemiesToSpawnThisWave;

            // Spawn Chasers
            for (int i = 0; i < chaserCount; i++)
            {
                if (ArenaGameManager.Instance != null && ArenaGameManager.Instance.IsGameOver)
                    yield break;

                SpawnEnemy(EnemyController.EnemyType.Chaser);
                yield return new WaitForSeconds(spawnInterval);
            }

            // Spawn Shooters
            for (int i = 0; i < shooterCount; i++)
            {
                if (ArenaGameManager.Instance != null && ArenaGameManager.Instance.IsGameOver)
                    yield break;

                SpawnEnemy(EnemyController.EnemyType.Shooter);
                yield return new WaitForSeconds(spawnInterval);
            }

            isSpawningWave = false;
        }

        private void SpawnEnemy(EnemyController.EnemyType type)
        {
            Vector3 spawnPos = GetSpawnPosition();

            GameObject enemyGo = null;
            if (type == EnemyController.EnemyType.Chaser)
            {
                if (chaserPrefab != null)
                {
                    enemyGo = Instantiate(chaserPrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    enemyGo = CreateProceduralEnemy(spawnPos, EnemyController.EnemyType.Chaser);
                }
            }
            else
            {
                if (shooterPrefab != null)
                {
                    enemyGo = Instantiate(shooterPrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    enemyGo = CreateProceduralEnemy(spawnPos, EnemyController.EnemyType.Shooter);
                }
            }

            if (enemyGo != null)
            {
                EnemyController ctrl = enemyGo.GetComponent<EnemyController>();
                if (ctrl != null)
                {
                    ctrl.SetAudioClips(enemyHitClip, enemyExplosionClip);
                }
            }
        }

        private Vector3 GetSpawnPosition()
        {
            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                int randomIndex = Random.Range(0, spawnPoints.Length);
                if (spawnPoints[randomIndex] != null)
                {
                    Vector3 pos = spawnPoints[randomIndex].position;
                    pos.y = 1.0f;
                    return pos;
                }
            }

            // Fallback: spawn di lingkaran keliling arena
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * arenaSpawnRadius;
            float z = Mathf.Sin(angle) * arenaSpawnRadius;
            return new Vector3(x, 1.0f, z);
        }

        public void OnEnemyKilled()
        {
            enemiesAlive--;
            if (enemiesAlive <= 0)
            {
                enemiesAlive = 0;
                StartCoroutine(WaveClearedRoutine());
            }
        }

        private IEnumerator WaveClearedRoutine()
        {
            if (ArenaGameManager.Instance != null)
            {
                ArenaGameManager.Instance.ShowWaveNotification("WAVE BERSIH! BERSIAP WAVE BERIKUTNYA...", timeBetweenWaves);
            }

            yield return new WaitForSeconds(timeBetweenWaves);

            if (ArenaGameManager.Instance != null && !ArenaGameManager.Instance.IsGameOver)
            {
                StartNextWave();
            }
        }

        private GameObject CreateProceduralEnemy(Vector3 position, EnemyController.EnemyType type)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = type == EnemyController.EnemyType.Chaser ? "Chaser_Drone" : "Shooter_Drone";
            enemy.transform.position = position;

            // Setup Collider & Material yang presisi
            CapsuleCollider cc = enemy.GetComponent<CapsuleCollider>();
            if (cc != null)
            {
                cc.center = new Vector3(0, 0, 0);
                cc.radius = 0.5f;
                cc.height = 2f;
            }

            Renderer rend = enemy.GetComponent<Renderer>();
            if (rend != null)
            {
                Color color = (type == EnemyController.EnemyType.Chaser) 
                    ? new Color(0.95f, 0.2f, 0.2f) // Merah tajam untuk Chaser
                    : new Color(0.95f, 0.6f, 0.1f); // Oranye untuk Shooter
                rend.material.color = color;
            }

            EnemyController controller = enemy.AddComponent<EnemyController>();
            return enemy;
        }

        public void SetAudioClips(AudioClip hit, AudioClip explosion)
        {
            enemyHitClip = hit;
            enemyExplosionClip = explosion;
        }
    }
}
