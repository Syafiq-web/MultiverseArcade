using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MultiverseArcade.Games.Syafiq
{
    /// <summary>
    /// Mengelola spawn wave musuh Zombie Chibi Kartun 3D.
    /// Dilengkapi model kartun lucu (kepala besar, tangan menjulur, bar darah di atas kepala),
    /// audio SFX, dan collider yang sempurna.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner Instance { get; private set; }

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

            // Spawn Chaser Zombies
            for (int i = 0; i < chaserCount; i++)
            {
                if (ArenaGameManager.Instance != null && ArenaGameManager.Instance.IsGameOver)
                    yield break;

                SpawnChibiZombie(EnemyController.EnemyType.ChaserZombie);
                yield return new WaitForSeconds(spawnInterval);
            }

            // Spawn Shooter Zombies
            for (int i = 0; i < shooterCount; i++)
            {
                if (ArenaGameManager.Instance != null && ArenaGameManager.Instance.IsGameOver)
                    yield break;

                SpawnChibiZombie(EnemyController.EnemyType.ShooterZombie);
                yield return new WaitForSeconds(spawnInterval);
            }

            isSpawningWave = false;
        }

        private void SpawnChibiZombie(EnemyController.EnemyType type)
        {
            Vector3 spawnPos = GetSpawnPosition();
            GameObject zombie = CreateChibiZombieModel(spawnPos, type);

            if (zombie != null)
            {
                EnemyController ctrl = zombie.GetComponent<EnemyController>();
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
                    pos.y = 0.0f;
                    return pos;
                }
            }

            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * arenaSpawnRadius;
            float z = Mathf.Sin(angle) * arenaSpawnRadius;
            return new Vector3(x, 0.0f, z);
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

        /// <summary>
        /// Membangun karakter 3D Zombie Chibi Kartun yang imut & ekspresif:
        /// Kepala besar hijau kartun, mata bulat, baju sobek, tangan menjulur zombie, dan bar HP.
        /// </summary>
        private GameObject CreateChibiZombieModel(Vector3 position, EnemyController.EnemyType type)
        {
            GameObject root = new GameObject(type == EnemyController.EnemyType.ChaserZombie ? "Chibi_Zombie_Runner" : "Chibi_Zombie_Shooter");
            root.tag = "Enemy";
            root.transform.position = position;

            // Collider & Controller di root
            CapsuleCollider cc = root.AddComponent<CapsuleCollider>();
            cc.center = new Vector3(0, 0.9f, 0);
            cc.radius = 0.55f;
            cc.height = 1.8f;

            EnemyController controller = root.AddComponent<EnemyController>();

            // Model Container untuk animasi waddle
            GameObject modelHolder = new GameObject("ModelHolder");
            modelHolder.transform.SetParent(root.transform, false);
            controller.SetModelHolder(modelHolder.transform);

            // Warna Zombie Kartun
            Color skinColor = (type == EnemyController.EnemyType.ChaserZombie) 
                ? new Color(0.35f, 0.78f, 0.42f)   // Hijau cerah kartun
                : new Color(0.85f, 0.65f, 0.25f);  // Oranye/Kuning zombie toxic
            Color shirtColor = new Color(0.38f, 0.35f, 0.85f); // Baju ungu/biru kartun
            Color pantsColor = new Color(0.2f, 0.2f, 0.3f);     // Celana gelap
            Color eyeWhite = Color.white;
            Color eyePupil = new Color(0.1f, 0.1f, 0.15f);

            // 1. Kepala Besar Chibi (Big Cute Head)
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Zombie_Head";
            head.transform.SetParent(modelHolder.transform, false);
            head.transform.localPosition = new Vector3(0, 1.25f, 0);
            head.transform.localScale = new Vector3(0.72f, 0.68f, 0.68f);
            head.GetComponent<Renderer>().material.color = skinColor;
            Destroy(head.GetComponent<Collider>());

            // Detail Mata Kiri
            GameObject eyeL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eyeL.transform.SetParent(head.transform, false);
            eyeL.transform.localPosition = new Vector3(-0.2f, 0.08f, 0.52f);
            eyeL.transform.localScale = new Vector3(0.22f, 0.22f, 0.08f);
            eyeL.GetComponent<Renderer>().material.color = eyeWhite;
            Destroy(eyeL.GetComponent<Collider>());

            GameObject pupilL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pupilL.transform.SetParent(eyeL.transform, false);
            pupilL.transform.localPosition = new Vector3(0, 0, 0.6f);
            pupilL.transform.localScale = new Vector3(0.55f, 0.55f, 0.3f);
            pupilL.GetComponent<Renderer>().material.color = eyePupil;
            Destroy(pupilL.GetComponent<Collider>());

            // Detail Mata Kanan
            GameObject eyeR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            eyeR.transform.SetParent(head.transform, false);
            eyeR.transform.localPosition = new Vector3(0.2f, 0.08f, 0.52f);
            eyeR.transform.localScale = new Vector3(0.22f, 0.22f, 0.08f);
            eyeR.GetComponent<Renderer>().material.color = eyeWhite;
            Destroy(eyeR.GetComponent<Collider>());

            GameObject pupilR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pupilR.transform.SetParent(eyeR.transform, false);
            pupilR.transform.localPosition = new Vector3(0, 0, 0.6f);
            pupilR.transform.localScale = new Vector3(0.55f, 0.55f, 0.3f);
            pupilR.GetComponent<Renderer>().material.color = eyePupil;
            Destroy(pupilR.GetComponent<Collider>());

            // Mulut Zombie Kartun
            GameObject mouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mouth.transform.SetParent(head.transform, false);
            mouth.transform.localPosition = new Vector3(0, -0.22f, 0.52f);
            mouth.transform.localScale = new Vector3(0.32f, 0.12f, 0.06f);
            mouth.GetComponent<Renderer>().material.color = new Color(0.15f, 0.05f, 0.05f);
            Destroy(mouth.GetComponent<Collider>());

            // 2. Badan (T-Shirt Sobek)
            GameObject torso = GameObject.CreatePrimitive(PrimitiveType.Cube);
            torso.name = "Zombie_Torso";
            torso.transform.SetParent(modelHolder.transform, false);
            torso.transform.localPosition = new Vector3(0, 0.68f, 0);
            torso.transform.localScale = new Vector3(0.52f, 0.5f, 0.38f);
            torso.GetComponent<Renderer>().material.color = shirtColor;
            Destroy(torso.GetComponent<Collider>());

            // 3. Dua Tangan Menjulur Zombie (Classic Zombie Reaching Pose)
            GameObject armL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armL.name = "Zombie_ArmL";
            armL.transform.SetParent(modelHolder.transform, false);
            armL.transform.localPosition = new Vector3(-0.36f, 0.72f, 0.3f);
            armL.transform.localScale = new Vector3(0.18f, 0.18f, 0.55f);
            armL.GetComponent<Renderer>().material.color = skinColor;
            Destroy(armL.GetComponent<Collider>());

            GameObject armR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            armR.name = "Zombie_ArmR";
            armR.transform.SetParent(modelHolder.transform, false);
            armR.transform.localPosition = new Vector3(0.36f, 0.72f, 0.3f);
            armR.transform.localScale = new Vector3(0.18f, 0.18f, 0.55f);
            armR.GetComponent<Renderer>().material.color = skinColor;
            Destroy(armR.GetComponent<Collider>());

            // 4. Dua Kaki
            GameObject legL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            legL.name = "Zombie_LegL";
            legL.transform.SetParent(modelHolder.transform, false);
            legL.transform.localPosition = new Vector3(-0.16f, 0.24f, 0);
            legL.transform.localScale = new Vector3(0.18f, 0.46f, 0.2f);
            legL.GetComponent<Renderer>().material.color = pantsColor;
            Destroy(legL.GetComponent<Collider>());

            GameObject legR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            legR.name = "Zombie_LegR";
            legR.transform.SetParent(modelHolder.transform, false);
            legR.transform.localPosition = new Vector3(0.16f, 0.24f, 0);
            legR.transform.localScale = new Vector3(0.18f, 0.46f, 0.2f);
            legR.GetComponent<Renderer>().material.color = pantsColor;
            Destroy(legR.GetComponent<Collider>());

            // 5. World-Space Health Bar Billboard di Atas Kepala
            GameObject hpCanvasGo = new GameObject("Zombie_HealthCanvas");
            hpCanvasGo.transform.SetParent(root.transform, false);
            hpCanvasGo.transform.localPosition = new Vector3(0, 1.85f, 0);
            hpCanvasGo.transform.localScale = Vector3.one * 0.006f;

            Canvas hpCanvas = hpCanvasGo.AddComponent<Canvas>();
            hpCanvas.renderMode = RenderMode.WorldSpace;
            RectTransform hprt = hpCanvasGo.GetComponent<RectTransform>();
            hprt.sizeDelta = new Vector2(100, 20);

            // Slider UI
            GameObject sliderGo = new GameObject("HPSlider");
            sliderGo.transform.SetParent(hpCanvasGo.transform, false);
            RectTransform slrt = sliderGo.AddComponent<RectTransform>();
            slrt.sizeDelta = new Vector2(100, 14);

            Slider slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 50;
            slider.value = 50;

            // Slider Background (Merah)
            GameObject sbg = new GameObject("BG");
            sbg.transform.SetParent(sliderGo.transform, false);
            RectTransform sbgrt = sbg.AddComponent<RectTransform>();
            sbgrt.anchorMin = Vector2.zero;
            sbgrt.anchorMax = Vector2.one;
            sbgrt.sizeDelta = Vector2.zero;
            Image sbgImg = sbg.AddComponent<Image>();
            sbgImg.color = new Color(0.4f, 0.1f, 0.1f, 0.9f);

            // Fill Area (Hijau)
            GameObject fillArea = new GameObject("FillArea");
            fillArea.transform.SetParent(sliderGo.transform, false);
            RectTransform fart = fillArea.AddComponent<RectTransform>();
            fart.anchorMin = Vector2.zero;
            fart.anchorMax = Vector2.one;
            fart.sizeDelta = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform frt = fill.AddComponent<RectTransform>();
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.sizeDelta = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.9f, 0.35f, 1f);

            slider.fillRect = frt;
            slider.targetGraphic = sbgImg;

            controller.SetupHealthBar(slider);
            controller.CacheRenderers();

            return root;
        }

        public void SetAudioClips(AudioClip hit, AudioClip explosion)
        {
            enemyHitClip = hit;
            enemyExplosionClip = explosion;
        }
    }
}
