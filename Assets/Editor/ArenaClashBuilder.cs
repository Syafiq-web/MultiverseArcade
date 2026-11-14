#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.Rendering.Universal;
using TMPro;
using MultiverseArcade.Core;
using MultiverseArcade.Syafiq;
using MultiverseArcade.UI;

namespace MultiverseArcade.EditorTools
{
    /// <summary>
    /// Builder Otomatis untuk Arena Clash 3D (Fase 1 sampai 5) & Validasi Bug Fix:
    /// - Arena Blockout 30x30 simetris, NavMeshSurface baking
    /// - Kamera top-down 55 derajat dengan smooth follow & mouse look-ahead
    /// - Player controller WASD (diagonal dinormalisasi) + mouse aiming Y-axis
    /// - Projectile pooling (pool 60) + PlayerCombat (interval 0.4s)
    /// - Bot AI 3v3 (FSM: Idle, Seek, Chase, Attack, Strafe, Retreat, Dead) + Utility target scoring
    /// - Format 3v3 (1 Player + 2 Blue Bot vs 3 Red Bot), Countdown 3-2-1, Match Timer 180s, Target 10 eliminasi
    /// - BUG FIX 1: Tepat satu EventSystem aktif (SingleEventSystem), PauseCanvas terisolasi (Sorting Order 500),
    ///   GraphicRaycaster aktif, non-interactive Raycast Target OFF, proteksi panel ganda, kursor visible saat pause.
    /// - BUG FIX 2: World-space health bar 6 karakter (Player, 2 Blue Bot, 3 Red Bot), billboard camera,
    ///   Raycast Target OFF, warna tim biru/merah, sinkron HP saat respawn dan rematch tanpa duplikat.
    /// </summary>
    public static class ArenaClashBuilder
    {
        private const string SCENE_SYAFIQ = "Assets/Games/Syafiq/Scenes/Game_Syafiq.unity";
        private const string DATA_DIR = "Assets/Games/Syafiq/Data";
        private const string PREFAB_DIR = "Assets/Games/Syafiq/Prefabs";
        private const string MAT_DIR = "Assets/Games/Syafiq/Materials";

        [MenuItem("Multiverse/Build Arena Clash", false, 0)]
        [MenuItem("Tools/Multiverse Arcade/⚔️ Build Arena Clash (Fase 1-5)", false, 0)]
        public static void BuildArenaClash()
        {
            BuildArenaClashInternal(true);
        }

        public static void BuildArenaClashFromCommandLine()
        {
            Debug.Log("[ArenaClashBuilder] Memulai eksekusi BuildArenaClash dari command line...");
            BuildArenaClashInternal(false);
            EditorApplication.Exit(0);
        }

        public static void BuildArenaClashInternal(bool interactive)
        {
            EnsureDirectories();

            Debug.Log("[ArenaClashBuilder] === MEMULAI PEMBANGUNAN MVP ARENA CLASH 3D (FASE 1-5) ===");

            // 1. Data Assets
            SO_Projectile projData = CreateOrLoadProjectileData();
            SO_Hero heroData = CreateOrLoadHeroData(projData);
            SO_BotDifficulty botDiff = CreateOrLoadBotDifficulty();
            SO_MatchSettings matchSettings = CreateOrLoadMatchSettings();

            // 2. Prefabs
            GameObject projPrefab = CreateProjectilePrefab(projData);
            GameObject dmgNumPrefab = CreateDamageNumberPrefab();
            GameObject healthBarPrefab = CreateHealthBarPrefab();

            // 3. Setup Scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ClearScene();

            // Setup Layers & Collision Matrix
            SetupPhysicsLayers();

            // Audio Clips
            AudioClip shootClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/sfx_laser_shoot.wav");
            if (shootClip == null) shootClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/Syafiq/Audio/sfx_laser_shoot.wav");
            AudioClip hurtClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/Syafiq/Audio/sfx_player_hurt.wav");
            AudioClip deathClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/Syafiq/Audio/sfx_explosion.wav");
            AudioClip bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/bgm_arcade_action.wav");

            // Directional Light
            CreateDirectionalLight();

            // BUG FIX 1: Tepat satu EventSystem dengan StandaloneInputModule dan SingleEventSystem
            CreateEventSystem();

            // Arena Geometry (30x30 simetris) & NavMesh
            GameObject arenaRoot = BuildArenaBlockout();

            // Projectile Pool Root
            GameObject poolGo = new GameObject("ProjectilePool");
            ProjectilePool pool = poolGo.AddComponent<ProjectilePool>();
            pool.SetPrefab(projPrefab);

            // Spawn Points
            CreateSpawnPoints(out Transform[] blueSpawns, out Transform[] redSpawns);

            // Player (Tim Blue ID = 0)
            GameObject playerGo = CreatePlayer(blueSpawns[0].position, heroData, projData, shootClip, hurtClip, deathClip, healthBarPrefab);
            PlayerController playerCtrl = playerGo.GetComponent<PlayerController>();
            Health playerHealth = playerGo.GetComponent<Health>();

            // Blue Bots (2 Bot)
            List<Health> blueBots = new List<Health>();
            for (int i = 1; i <= 2; i++)
            {
                Vector3 spawnPos = (i < blueSpawns.Length) ? blueSpawns[i].position : new Vector3(-10f + (i * 2f), 0, -10f);
                GameObject botGo = CreateBot($"BlueBot_{i}", 0, spawnPos, new Color(0f, 0.7f, 1f), projData, botDiff, shootClip, hurtClip, deathClip, healthBarPrefab);
                blueBots.Add(botGo.GetComponent<Health>());
            }

            // Red Bots (3 Bot)
            List<Health> redBots = new List<Health>();
            for (int i = 1; i <= 3; i++)
            {
                Vector3 spawnPos = (i - 1 < redSpawns.Length) ? redSpawns[i - 1].position : new Vector3(10f - (i * 2f), 0, 10f);
                GameObject botGo = CreateBot($"RedBot_{i}", 1, spawnPos, new Color(1f, 0.25f, 0.25f), projData, botDiff, shootClip, hurtClip, deathClip, healthBarPrefab);
                redBots.Add(botGo.GetComponent<Health>());
            }

            // Camera Top-Down
            GameObject camGo = CreateTopDownCamera(playerGo.transform);

            // Match Manager & Entry
            GameObject managerGo = new GameObject("ArenaMatchManager");
            ArenaMatchManager matchMgr = managerGo.AddComponent<ArenaMatchManager>();
            ArenaGameEntry entry = managerGo.AddComponent<ArenaGameEntry>();

            // Setup characters di MatchManager
            matchMgr.SetupCharacters(playerHealth, blueBots, redBots, blueSpawns, redSpawns);

            // Audio Listener & BGM Source
            AudioSource bgmSource = managerGo.AddComponent<AudioSource>();
            bgmSource.clip = bgmClip;
            bgmSource.loop = true;
            bgmSource.volume = 0.65f;
            bgmSource.playOnAwake = true;
            if (bgmClip != null) bgmSource.Play();

            // UI Canvas (ArenaGameCanvas + Isolated PauseCanvas dengan sorting order 500)
            BuildArenaUI(dmgNumPrefab);

            // Save Scene
            EditorSceneManager.SaveScene(scene, SCENE_SYAFIQ);
            Debug.Log($"[ArenaClashBuilder] Scene Arena Clash 3D tersimpan sempurna di {SCENE_SYAFIQ}");

            if (interactive)
            {
                EditorUtility.DisplayDialog("Arena Clash 3D",
                    "MVP Arena Clash 3D (Fase 1 sampai 5) berhasil dibangun & bug fix diterapkan!\n\n" +
                    "- Tepat 1 EventSystem aktif dengan SingleEventSystem\n" +
                    "- Dedicated PauseCanvas (Sorting Order 500) dengan GraphicRaycaster\n" +
                    "- Raycast Target non-aktif pada background HUD & Health Bars\n" +
                    "- World Health Bars pada 6 karakter (Player, 2 Blue Bot, 3 Red Bot) dengan warna tim dan billboard", "OK");
            }
        }

        #region Setup Directories & Assets
        private static void EnsureDirectories()
        {
            string[] dirs = new string[]
            {
                "Assets/_Project/Audio",
                "Assets/Games/Syafiq/Scenes",
                "Assets/Games/Syafiq/Scripts",
                "Assets/Games/Syafiq/Prefabs",
                "Assets/Games/Syafiq/Materials",
                "Assets/Games/Syafiq/Audio",
                "Assets/Games/Syafiq/Data"
            };

            foreach (var d in dirs)
            {
                if (!Directory.Exists(d)) Directory.CreateDirectory(d);
            }
            AssetDatabase.Refresh();
        }

        private static void SetupPhysicsLayers()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            SetLayer(layersProp, 10, "Syafiq_Player");
            SetLayer(layersProp, 11, "Syafiq_Enemy");
            SetLayer(layersProp, 12, "Syafiq_Projectile");
            SetLayer(layersProp, 13, "Syafiq_Wall");
            SetLayer(layersProp, 14, "Syafiq_Ground");

            tagManager.ApplyModifiedProperties();

            // Collision Matrix
            int playerLayer = LayerMask.NameToLayer("Syafiq_Player");
            int enemyLayer = LayerMask.NameToLayer("Syafiq_Enemy");
            int projLayer = LayerMask.NameToLayer("Syafiq_Projectile");
            int wallLayer = LayerMask.NameToLayer("Syafiq_Wall");
            int groundLayer = LayerMask.NameToLayer("Syafiq_Ground");

            if (projLayer >= 0)
            {
                // Projectile menabrak Player, Enemy, Wall
                if (playerLayer >= 0) Physics.IgnoreLayerCollision(projLayer, playerLayer, false);
                if (enemyLayer >= 0) Physics.IgnoreLayerCollision(projLayer, enemyLayer, false);
                if (wallLayer >= 0) Physics.IgnoreLayerCollision(projLayer, wallLayer, false);
                if (groundLayer >= 0) Physics.IgnoreLayerCollision(projLayer, groundLayer, true);

                // Projectile-Projectile nonaktif (AC-06)
                Physics.IgnoreLayerCollision(projLayer, projLayer, true);
            }
            Debug.Log("[ArenaClashBuilder] Physics layers & Collision Matrix berhasil dikonfigurasi.");
        }

        private static void SetLayer(SerializedProperty layersProp, int index, string name)
        {
            SerializedProperty sp = layersProp.GetArrayElementAtIndex(index);
            if (sp != null && sp.stringValue != name)
            {
                sp.stringValue = name;
            }
        }

        private static SO_Projectile CreateOrLoadProjectileData()
        {
            string path = $"{DATA_DIR}/Projectile_Blaster.asset";
            SO_Projectile data = AssetDatabase.LoadAssetAtPath<SO_Projectile>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<SO_Projectile>();
                data.speed = 18f;
                data.damage = 20;
                data.lifetime = 2.5f;
                data.radius = 0.25f;
                AssetDatabase.CreateAsset(data, path);
            }
            return data;
        }

        private static SO_Hero CreateOrLoadHeroData(SO_Projectile projData)
        {
            string path = $"{DATA_DIR}/Hero_Blaze.asset";
            SO_Hero data = AssetDatabase.LoadAssetAtPath<SO_Hero>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<SO_Hero>();
                data.heroName = "Blaze";
                data.maxHealth = 100;
                data.moveSpeed = 5.0f;
                data.fireInterval = 0.4f;
                data.projectileData = projData;
                AssetDatabase.CreateAsset(data, path);
            }
            return data;
        }

        private static SO_BotDifficulty CreateOrLoadBotDifficulty()
        {
            string path = $"{DATA_DIR}/BotDifficulty_Medium.asset";
            SO_BotDifficulty data = AssetDatabase.LoadAssetAtPath<SO_BotDifficulty>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<SO_BotDifficulty>();
                data.difficultyName = "Medium";
                data.moveSpeed = 4.2f;
                data.fireInterval = 0.8f;
                data.aimErrorDegrees = 6.0f;
                data.reactionTime = 0.25f;
                data.retreatHealthRatio = 0.25f;
                AssetDatabase.CreateAsset(data, path);
            }
            return data;
        }

        private static SO_MatchSettings CreateOrLoadMatchSettings()
        {
            string path = $"{DATA_DIR}/MatchSettings_Standard.asset";
            SO_MatchSettings data = AssetDatabase.LoadAssetAtPath<SO_MatchSettings>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<SO_MatchSettings>();
                data.targetEliminations = 10;
                data.matchDuration = 180f;
                data.countdownDuration = 3f;
                data.respawnTime = 4f;
                AssetDatabase.CreateAsset(data, path);
            }
            return data;
        }
        #endregion

        #region Prefabs Creation
        private static GameObject CreateProjectilePrefab(SO_Projectile projData)
        {
            string path = $"{PREFAB_DIR}/PF_BlasterProjectile.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "PF_BlasterProjectile";
            go.transform.localScale = Vector3.one * 0.45f;

            int projLayer = LayerMask.NameToLayer("Syafiq_Projectile");
            if (projLayer >= 0) go.layer = projLayer;

            SphereCollider col = go.GetComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.5f;

            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            Material mat = CreateURPMaterial("Mat_Projectile_Laser", new Color(0f, 1f, 0.95f), 0.9f);
            go.GetComponent<Renderer>().sharedMaterial = mat;

            go.AddComponent<Projectile>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateDamageNumberPrefab()
        {
            string path = $"{PREFAB_DIR}/PF_DamageNumber.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject go = new GameObject("PF_DamageNumber");
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100, 40);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = "20";
            tmp.fontSize = 22;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.yellow;
            tmp.raycastTarget = false; // BUG FIX: Raycast Target OFF

            go.AddComponent<DamageNumber>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateHealthBarPrefab()
        {
            string path = $"{PREFAB_DIR}/PF_HealthBarWorld.prefab";

            // Rebuild prefab untuk memastikan semua perbaikan raycast target dan referensi terpasang
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }

            GameObject go = new GameObject("PF_HealthBarWorld");
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1.2f, 0.2f);

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            // Tidak menambahkan GraphicRaycaster pada world space health bar!

            Slider slider = go.AddComponent<Slider>();
            slider.interactable = false;
            slider.transition = Selectable.Transition.None;

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);
            bgImg.raycastTarget = false; // BUG FIX 2: Non-aktifkan raycast target
            RectTransform bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(go.transform, false);
            RectTransform faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero;
            faRt.anchorMax = Vector2.one;
            faRt.sizeDelta = Vector2.zero;

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0f, 0.85f, 1f);
            fillImg.raycastTarget = false; // BUG FIX 2: Non-aktifkan raycast target
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;

            slider.fillRect = fillRt;
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 100;

            HealthBarWorld hbw = go.AddComponent<HealthBarWorld>();

            // Setup private fields via reflection
            SetPrivateField(hbw, "_canvas", canvas);
            SetPrivateField(hbw, "_slider", slider);
            SetPrivateField(hbw, "_fillImage", fillImg);
            SetPrivateField(hbw, "_backgroundImage", bgImg);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }
        #endregion

        #region Arena Geometry & NavMesh
        private static GameObject BuildArenaBlockout()
        {
            GameObject arenaRoot = new GameObject("Arena_Blockout_30x30");

            int groundLayer = LayerMask.NameToLayer("Syafiq_Ground");
            if (groundLayer < 0) groundLayer = LayerMask.NameToLayer("Default");

            int wallLayer = LayerMask.NameToLayer("Syafiq_Wall");
            if (wallLayer < 0) wallLayer = LayerMask.NameToLayer("Default");

            // 1. Floor (30 x 30)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor_30x30";
            floor.transform.parent = arenaRoot.transform;
            floor.transform.position = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(30, 1, 30);
            floor.layer = groundLayer;
            Material floorMat = CreateURPMaterial("Mat_Arena_Floor", new Color(0.08f, 0.10f, 0.15f), 0.3f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // 2. Boundary Walls
            Material wallMat = CreateURPMaterial("Mat_Arena_Wall", new Color(0.15f, 0.20f, 0.28f), 0.5f);

            CreateWall(arenaRoot.transform, "Wall_North", new Vector3(0, 1.25f, 15.5f), new Vector3(32, 2.5f, 2), wallMat, wallLayer);
            CreateWall(arenaRoot.transform, "Wall_South", new Vector3(0, 1.25f, -15.5f), new Vector3(32, 2.5f, 2), wallMat, wallLayer);
            CreateWall(arenaRoot.transform, "Wall_East", new Vector3(15.5f, 1.25f, 0), new Vector3(2, 2.5f, 30), wallMat, wallLayer);
            CreateWall(arenaRoot.transform, "Wall_West", new Vector3(-15.5f, 1.25f, 0), new Vector3(2, 2.5f, 30), wallMat, wallLayer);

            // 3. Central Zone Covers (5 Obstacles simetris)
            Material coverMat = CreateURPMaterial("Mat_Arena_Cover", new Color(0.25f, 0.35f, 0.5f), 0.6f);
            CreateCover(arenaRoot.transform, "Cover_Center", new Vector3(0, 1f, 0), new Vector3(3f, 2f, 3f), coverMat, wallLayer);
            CreateCover(arenaRoot.transform, "Cover_NorthWest", new Vector3(-6f, 1f, 5f), new Vector3(3.5f, 2f, 2f), coverMat, wallLayer);
            CreateCover(arenaRoot.transform, "Cover_SouthEast", new Vector3(6f, 1f, -5f), new Vector3(3.5f, 2f, 2f), coverMat, wallLayer);
            CreateCover(arenaRoot.transform, "Cover_NorthEast", new Vector3(6f, 1f, 5f), new Vector3(2f, 2f, 3.5f), coverMat, wallLayer);
            CreateCover(arenaRoot.transform, "Cover_SouthWest", new Vector3(-6f, 1f, -5f), new Vector3(2f, 2f, 3.5f), coverMat, wallLayer);

            // 4. NavMeshSurface
            NavMeshSurface navSurface = arenaRoot.AddComponent<NavMeshSurface>();
            navSurface.collectObjects = CollectObjects.All;
            navSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            navSurface.BuildNavMesh();
            Debug.Log("[ArenaClashBuilder] NavMeshSurface berhasil dibake!");

            return arenaRoot;
        }

        private static GameObject CreateWall(Transform parent, string name, Vector3 pos, Vector3 size, Material mat, int layer)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.parent = parent;
            wall.transform.position = pos;
            wall.transform.localScale = size;
            wall.layer = layer;
            wall.GetComponent<Renderer>().sharedMaterial = mat;
            return wall;
        }

        private static GameObject CreateCover(Transform parent, string name, Vector3 pos, Vector3 size, Material mat, int layer)
        {
            GameObject cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cover.name = name;
            cover.transform.parent = parent;
            cover.transform.position = pos;
            cover.transform.localScale = size;
            cover.layer = layer;
            cover.GetComponent<Renderer>().sharedMaterial = mat;

            NavMeshObstacle obs = cover.AddComponent<NavMeshObstacle>();
            obs.carving = true;
            obs.shape = NavMeshObstacleShape.Box;
            obs.size = Vector3.one;
            return cover;
        }

        private static void CreateSpawnPoints(out Transform[] blueSpawns, out Transform[] redSpawns)
        {
            GameObject spawnRoot = new GameObject("SpawnPoints");

            // Blue Spawns (Barat Daya)
            GameObject b1 = new GameObject("Spawn_Blue_1");
            b1.transform.parent = spawnRoot.transform;
            b1.transform.position = new Vector3(-10f, 0f, -10f);

            GameObject b2 = new GameObject("Spawn_Blue_2");
            b2.transform.parent = spawnRoot.transform;
            b2.transform.position = new Vector3(-8f, 0f, -12f);

            GameObject b3 = new GameObject("Spawn_Blue_3");
            b3.transform.parent = spawnRoot.transform;
            b3.transform.position = new Vector3(-12f, 0f, -8f);

            blueSpawns = new Transform[] { b1.transform, b2.transform, b3.transform };

            // Red Spawns (Timur Laut)
            GameObject r1 = new GameObject("Spawn_Red_1");
            r1.transform.parent = spawnRoot.transform;
            r1.transform.position = new Vector3(10f, 0f, 10f);

            GameObject r2 = new GameObject("Spawn_Red_2");
            r2.transform.parent = spawnRoot.transform;
            r2.transform.position = new Vector3(8f, 0f, 12f);

            GameObject r3 = new GameObject("Spawn_Red_3");
            r3.transform.parent = spawnRoot.transform;
            r3.transform.position = new Vector3(12f, 0f, 8f);

            redSpawns = new Transform[] { r1.transform, r2.transform, r3.transform };
        }
        #endregion

        #region Character Creation (Player & Bots)
        private static GameObject CreatePlayer(Vector3 spawnPos, SO_Hero heroData, SO_Projectile projData,
            AudioClip shootClip, AudioClip hurtClip, AudioClip deathClip, GameObject healthBarPrefab)
        {
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player_Blaze";
            player.transform.position = spawnPos;

            int playerLayer = LayerMask.NameToLayer("Syafiq_Player");
            if (playerLayer >= 0) player.layer = playerLayer;

            Material playerMat = CreateURPMaterial("Mat_Player_Blue", new Color(0f, 0.85f, 1f), 0.7f);
            player.GetComponent<Renderer>().sharedMaterial = playerMat;

            // Health
            Health health = player.AddComponent<Health>();
            health.Setup(0, "Blaze", heroData.maxHealth, true);
            health.SetAudioClips(hurtClip, deathClip);

            // Controller
            PlayerController pc = player.AddComponent<PlayerController>();
            pc.SetHeroData(heroData);

            // FirePoint
            GameObject firePt = new GameObject("FirePoint");
            firePt.transform.parent = player.transform;
            firePt.transform.localPosition = new Vector3(0, 0.5f, 0.8f);

            // Combat
            PlayerCombat combat = player.AddComponent<PlayerCombat>();
            combat.SetConfig(projData, heroData.fireInterval, firePt.transform, shootClip);

            // Aim Indicator
            AimIndicator ai = player.AddComponent<AimIndicator>();
            LineRenderer lr = player.GetComponent<LineRenderer>();
            Material laserMat = CreateURPMaterial("Mat_Laser_Sight", new Color(0f, 1f, 0.9f), 0.8f);
            lr.sharedMaterial = laserMat;
            ai.Setup(pc, firePt.transform);

            // World Health Bar (Tim Blue - Biru Cyan)
            AttachWorldHealthBar(player, health, new Color(0f, 0.85f, 1f), healthBarPrefab);

            return player;
        }

        private static GameObject CreateBot(string name, int teamId, Vector3 spawnPos, Color teamColor,
            SO_Projectile projData, SO_BotDifficulty botDiff, AudioClip shootClip, AudioClip hurtClip, AudioClip deathClip, GameObject healthBarPrefab)
        {
            GameObject bot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bot.name = name;
            bot.transform.position = spawnPos;

            int botLayer = teamId == 0 ? LayerMask.NameToLayer("Syafiq_Player") : LayerMask.NameToLayer("Syafiq_Enemy");
            if (botLayer >= 0) bot.layer = botLayer;

            Material botMat = CreateURPMaterial($"Mat_{name}", teamColor, 0.6f);
            bot.GetComponent<Renderer>().sharedMaterial = botMat;

            // NavMeshAgent
            NavMeshAgent agent = bot.AddComponent<NavMeshAgent>();
            agent.speed = botDiff.moveSpeed;

            // Health
            Health health = bot.AddComponent<Health>();
            health.Setup(teamId, name, 100, false);
            health.SetAudioClips(hurtClip, deathClip);

            // FirePoint
            GameObject firePt = new GameObject("FirePoint");
            firePt.transform.parent = bot.transform;
            firePt.transform.localPosition = new Vector3(0, 0.5f, 0.8f);

            // Components AI
            BotMovement movement = bot.AddComponent<BotMovement>();
            BotTargeting targeting = bot.AddComponent<BotTargeting>();
            BotCombat combat = bot.AddComponent<BotCombat>();
            combat.Setup(projData, botDiff.fireInterval, botDiff.aimErrorDegrees, shootClip);

            bot.AddComponent<BotBrain>();

            // World Health Bar
            AttachWorldHealthBar(bot, health, teamColor, healthBarPrefab);

            return bot;
        }

        private static void AttachWorldHealthBar(GameObject owner, Health health, Color teamColor, GameObject prefab)
        {
            if (prefab == null || owner == null) return;

            // BUG FIX 2: Hapus health bar lama jika sudah ada untuk mencegah duplikasi
            HealthBarWorld[] existingBars = owner.GetComponentsInChildren<HealthBarWorld>(true);
            foreach (var b in existingBars)
            {
                if (b != null) Object.DestroyImmediate(b.gameObject);
            }

            GameObject hbGo = (GameObject)PrefabUtility.InstantiatePrefab(prefab, owner.transform);
            hbGo.name = "HealthBarWorld";
            HealthBarWorld hbw = hbGo.GetComponent<HealthBarWorld>();
            if (hbw != null)
            {
                hbw.Setup(health, owner.transform, teamColor);
            }
        }
        #endregion

        #region Camera, Lights & EventSystem
        private static GameObject CreateTopDownCamera(Transform target)
        {
            GameObject camGo = new GameObject("MainCamera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.06f, 0.10f);

            var urpData = camGo.AddComponent<UniversalAdditionalCameraData>();
            urpData.renderShadows = true;

            TopDownCamera tdc = camGo.AddComponent<TopDownCamera>();
            tdc.SetTarget(target);

            camGo.AddComponent<AudioListener>();
            return camGo;
        }

        private static void CreateDirectionalLight()
        {
            GameObject lightGo = new GameObject("Directional Light");
            Light l = lightGo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(0.92f, 0.96f, 1f);
            l.intensity = 1.15f;
            lightGo.transform.rotation = Quaternion.Euler(55, -35, 0);
        }

        private static void CreateEventSystem()
        {
            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
            esGo.AddComponent<SingleEventSystem>();
            Debug.Log("[ArenaClashBuilder] EventSystem dengan StandaloneInputModule & SingleEventSystem berhasil dibuat.");
        }
        #endregion

        #region UI Canvas & Overlays
        private static void BuildArenaUI(GameObject dmgNumPrefab)
        {
            // 1. Arena HUD Canvas (Sorting Order 100)
            GameObject canvasGo = new GameObject("ArenaGameCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // Top Score & Timer Banner
            GameObject topBanner = new GameObject("TopScoreBanner");
            topBanner.transform.SetParent(canvasGo.transform, false);
            RectTransform bannerRt = topBanner.AddComponent<RectTransform>();
            bannerRt.anchoredPosition = new Vector2(0, 480);
            bannerRt.sizeDelta = new Vector2(600, 70);

            Image bannerBg = topBanner.AddComponent<Image>();
            bannerBg.color = new Color(0.06f, 0.08f, 0.14f, 0.9f);
            bannerBg.raycastTarget = false; // BUG FIX: Matikan raycast target pada background non-interaktif

            TextMeshProUGUI blueScoreTxt = CreateTextTMP(topBanner.transform, "BlueScore", "0", 38,
                new Vector2(-160, 0), new Vector2(100, 60), new Color(0f, 0.9f, 1f), FontStyles.Bold, TextAlignmentOptions.Center);

            TextMeshProUGUI timerTxt = CreateTextTMP(topBanner.transform, "Timer", "03:00", 32,
                new Vector2(0, 0), new Vector2(160, 60), Color.white, FontStyles.Bold, TextAlignmentOptions.Center);

            TextMeshProUGUI redScoreTxt = CreateTextTMP(topBanner.transform, "RedScore", "0", 38,
                new Vector2(160, 0), new Vector2(100, 60), new Color(1f, 0.3f, 0.3f), FontStyles.Bold, TextAlignmentOptions.Center);

            // Countdown Banner (Center Screen)
            GameObject countdownGo = new GameObject("CountdownBanner");
            countdownGo.transform.SetParent(canvasGo.transform, false);
            RectTransform cdRt = countdownGo.AddComponent<RectTransform>();
            cdRt.anchoredPosition = new Vector2(0, 80);
            cdRt.sizeDelta = new Vector2(500, 150);

            TextMeshProUGUI countdownTxt = CreateTextTMP(countdownGo.transform, "CountdownText", "3", 84,
                Vector2.zero, new Vector2(500, 150), new Color(0f, 1f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Center);

            // Player HUD (Bottom Left)
            GameObject playerHudGo = new GameObject("PlayerHUD");
            playerHudGo.transform.SetParent(canvasGo.transform, false);
            RectTransform pHudRt = playerHudGo.AddComponent<RectTransform>();
            pHudRt.anchoredPosition = new Vector2(-660, -460);
            pHudRt.sizeDelta = new Vector2(360, 90);

            Image pHudBg = playerHudGo.AddComponent<Image>();
            pHudBg.color = new Color(0.06f, 0.08f, 0.14f, 0.85f);
            pHudBg.raycastTarget = false; // BUG FIX: Matikan raycast target pada background non-interaktif

            CreateTextTMP(playerHudGo.transform, "PlayerLabel", "BLAZE (YOU)", 16,
                new Vector2(0, 24), new Vector2(320, 30), new Color(0f, 1f, 0.9f), FontStyles.Bold, TextAlignmentOptions.Left);

            Slider hpSlider = CreateProgressBar(playerHudGo.transform, "PlayerHPSlider", new Vector2(0, -12), new Vector2(320, 22), new Color(0f, 0.85f, 0.4f));
            TextMeshProUGUI hpTxt = CreateTextTMP(playerHudGo.transform, "HPText", "100 / 100 HP", 14,
                new Vector2(0, -12), new Vector2(320, 22), Color.white, FontStyles.Bold, TextAlignmentOptions.Center);

            // Kill Feed Container (Top Right)
            GameObject killFeedGo = new GameObject("KillFeedContainer");
            killFeedGo.transform.SetParent(canvasGo.transform, false);
            RectTransform kfRt = killFeedGo.AddComponent<RectTransform>();
            kfRt.anchoredPosition = new Vector2(660, 380);
            kfRt.sizeDelta = new Vector2(340, 260);

            GameObject kfItemPrefab = CreateKillFeedItemPrefab(canvasGo.transform);
            KillFeedUI kfUI = canvasGo.AddComponent<KillFeedUI>();
            kfUI.BindContainer(killFeedGo.transform, kfItemPrefab);

            // Damage Number Container
            GameObject dmgNumContainer = new GameObject("DamageNumberContainer");
            dmgNumContainer.transform.SetParent(canvasGo.transform, false);

            // ArenaHUD Component
            ArenaHUD hud = canvasGo.AddComponent<ArenaHUD>();
            hud.BindControls(blueScoreTxt, redScoreTxt, timerTxt, countdownGo, countdownTxt, hpSlider, hpTxt, dmgNumPrefab, dmgNumContainer.transform);

            // Result Panel (pada ArenaGameCanvas)
            BuildResultPanel(canvasGo.transform);

            // Tombol Pause di pojok kanan atas ArenaGameCanvas
            Button pauseBtn = CreateCyberButton(canvasGo.transform, "Btn_Pause", "PAUSE (Esc)",
                new Vector2(760, 460), new Vector2(170, 42), new Color(0.2f, 0.35f, 0.5f, 0.9f));

            // BUG FIX 1: Dedicated PauseCanvas dengan Sorting Order tertinggi (500) dan GraphicRaycaster mandiri
            BuildDedicatedPauseCanvas(pauseBtn);
        }

        private static GameObject CreateKillFeedItemPrefab(Transform parent)
        {
            GameObject item = new GameObject("KillFeedItem");
            item.transform.SetParent(parent, false);
            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(320, 32);

            Image bg = item.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.08f, 0.12f, 0.8f);
            bg.raycastTarget = false; // BUG FIX: Raycast Target OFF

            TextMeshProUGUI tmp = CreateTextTMP(item.transform, "Text", "Blaze  ➜  Red Bot", 14,
                Vector2.zero, new Vector2(300, 30), Color.white, FontStyles.Bold, TextAlignmentOptions.Right);

            item.SetActive(false);
            return item;
        }

        private static void BuildResultPanel(Transform parent)
        {
            GameObject panel = new GameObject("ResultPanel");
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(760, 520);

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.08f, 0.14f, 0.98f);
            bg.raycastTarget = false; // BUG FIX: Raycast Target OFF pada background

            TextMeshProUGUI titleTxt = CreateTextTMP(panel.transform, "Title", "VICTORY!", 58,
                new Vector2(0, 160), new Vector2(600, 80), new Color(0f, 1f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Center);

            TextMeshProUGUI scoreTxt = CreateTextTMP(panel.transform, "Score", "BLUE 10 — 4 RED", 32,
                new Vector2(0, 70), new Vector2(600, 50), Color.white, FontStyles.Bold, TextAlignmentOptions.Center);

            TextMeshProUGUI statsTxt = CreateTextTMP(panel.transform, "Stats", "Target Eliminasi: 10  |  Format 3v3", 18,
                new Vector2(0, 10), new Vector2(600, 40), new Color(0.7f, 0.85f, 1f, 0.85f), FontStyles.Normal, TextAlignmentOptions.Center);

            Button rematchBtn = CreateCyberButton(panel.transform, "Btn_Rematch", "REMATCH",
                new Vector2(0, -70), new Vector2(300, 55), new Color(0f, 0.7f, 0.65f));

            Button switchBtn = CreateCyberButton(panel.transform, "Btn_SwitchGame", "SWITCH GAME",
                new Vector2(0, -140), new Vector2(300, 50), new Color(0.35f, 0.25f, 0.55f));

            ResultPanelUI resultUI = panel.AddComponent<ResultPanelUI>();
            resultUI.BindControls(titleTxt, scoreTxt, statsTxt, rematchBtn, switchBtn);
        }

        private static void BuildDedicatedPauseCanvas(Button pauseBtn)
        {
            // BUG FIX 1: Dedicated PauseCanvas ber-Sorting Order 500 (paling tinggi) dan GraphicRaycaster
            GameObject pauseCanvasGo = new GameObject("PauseCanvas");
            Canvas canvas = pauseCanvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler scaler = pauseCanvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            pauseCanvasGo.AddComponent<GraphicRaycaster>();

            // Pause Overlay Panel
            GameObject pausePanel = new GameObject("PausePanel");
            pausePanel.transform.SetParent(pauseCanvasGo.transform, false);
            RectTransform rt = pausePanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Background peredam (menangkap klik di luar tombol)
            Image dimImg = pausePanel.AddComponent<Image>();
            dimImg.color = new Color(0.04f, 0.06f, 0.12f, 0.94f);
            dimImg.raycastTarget = true;

            CreateTextTMP(pausePanel.transform, "PauseTitle", "GAME PAUSED", 48,
                new Vector2(0, 160), new Vector2(500, 70), new Color(0.95f, 0.85f, 0.2f), FontStyles.Bold, TextAlignmentOptions.Center);

            Button resumeBtn = CreateCyberButton(pausePanel.transform, "Btn_Resume", "RESUME", new Vector2(0, 50), new Vector2(300, 50), new Color(0f, 0.65f, 0.6f));
            Button restartBtn = CreateCyberButton(pausePanel.transform, "Btn_Restart", "RESTART GAME", new Vector2(0, -15), new Vector2(300, 48), new Color(0.2f, 0.35f, 0.55f));
            Button switchBtn = CreateCyberButton(pausePanel.transform, "Btn_SwitchGame", "SWITCH GAME", new Vector2(0, -75), new Vector2(300, 48), new Color(0.45f, 0.3f, 0.6f));
            Button menuBtn = CreateCyberButton(pausePanel.transform, "Btn_MainMenu", "MAIN MENU", new Vector2(0, -135), new Vector2(300, 48), new Color(0.5f, 0.2f, 0.2f));

            GameOverlayUI overlayUI = pauseCanvasGo.AddComponent<GameOverlayUI>();
            overlayUI.BindControls(pausePanel, pauseBtn, resumeBtn, restartBtn, switchBtn, menuBtn);
        }
        #endregion

        #region Validation (EditMode & Batchmode Check)
        [MenuItem("Multiverse/Validate Arena Clash", false, 1)]
        [MenuItem("Tools/Multiverse Arcade/🔍 Validate Arena Clash (EditMode)", false, 1)]
        public static bool ValidateArenaClash()
        {
            Debug.Log("[ArenaClashBuilder] === MEMULAI VALIDASI EDITMODE ARENA CLASH 3D ===");
            bool allPassed = true;

            // Pastikan Scene Game_Syafiq terbuka
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.path != SCENE_SYAFIQ)
            {
                Debug.Log($"[ArenaClashBuilder] Membuka scene {SCENE_SYAFIQ} untuk validasi...");
                EditorSceneManager.OpenScene(SCENE_SYAFIQ);
            }

            // 1. Validasi EventSystem
            EventSystem[] eventSystems = Object.FindObjectsOfType<EventSystem>();
            if (eventSystems.Length == 1)
            {
                Debug.Log($"[VALIDASI 1 PASSED] Tepat 1 EventSystem aktif di scene ({eventSystems[0].name}).");
                var sim = eventSystems[0].GetComponent<StandaloneInputModule>();
                if (sim != null)
                {
                    Debug.Log("[VALIDASI 1 PASSED] StandaloneInputModule terpasang sempurna.");
                }
                else
                {
                    Debug.LogError("[VALIDASI 1 FAILED] StandaloneInputModule TIDAK ditemukan pada EventSystem!");
                    allPassed = false;
                }

                var ses = eventSystems[0].GetComponent<SingleEventSystem>();
                if (ses != null)
                {
                    Debug.Log("[VALIDASI 1 PASSED] SingleEventSystem terpasang untuk mencegah duplikasi.");
                }
                else
                {
                    Debug.LogError("[VALIDASI 1 FAILED] SingleEventSystem TIDAK ditemukan pada EventSystem!");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError($"[VALIDASI 1 FAILED] Ditemukan {eventSystems.Length} EventSystem di scene (harus TEPAT 1)!");
                allPassed = false;
            }

            // 2. Validasi Canvas & GraphicRaycaster
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>();
            Canvas pauseCanvas = null;
            Canvas arenaGameCanvas = null;

            foreach (var c in canvases)
            {
                if (c.name == "PauseCanvas") pauseCanvas = c;
                if (c.name == "ArenaGameCanvas") arenaGameCanvas = c;
            }

            if (pauseCanvas != null)
            {
                var gr = pauseCanvas.GetComponent<GraphicRaycaster>();
                if (gr != null && pauseCanvas.sortingOrder >= 500)
                {
                    Debug.Log($"[VALIDASI 2 PASSED] PauseCanvas memiliki GraphicRaycaster dan Sorting Order tertinggi ({pauseCanvas.sortingOrder}).");
                }
                else
                {
                    Debug.LogError($"[VALIDASI 2 FAILED] PauseCanvas Sorting Order ({pauseCanvas.sortingOrder}) atau GraphicRaycaster tidak valid!");
                    allPassed = false;
                }
            }
            else
            {
                Debug.LogError("[VALIDASI 2 FAILED] Dedicated PauseCanvas TIDAK ditemukan!");
                allPassed = false;
            }

            if (arenaGameCanvas != null)
            {
                var gr = arenaGameCanvas.GetComponent<GraphicRaycaster>();
                if (gr != null)
                {
                    Debug.Log("[VALIDASI 2 PASSED] ArenaGameCanvas memiliki GraphicRaycaster aktif.");
                }
                else
                {
                    Debug.LogError("[VALIDASI 2 FAILED] ArenaGameCanvas TIDAK memiliki GraphicRaycaster!");
                    allPassed = false;
                }
            }

            // 3. Validasi Tombol & Parent Canvas
            Button[] buttons = Object.FindObjectsOfType<Button>(true);
            Debug.Log($"[ArenaClashBuilder] Memeriksa {buttons.Length} tombol UI...");
            foreach (var btn in buttons)
            {
                Canvas parentCanvas = btn.GetComponentInParent<Canvas>();
                if (parentCanvas == null || parentCanvas.GetComponent<GraphicRaycaster>() == null)
                {
                    Debug.LogError($"[VALIDASI 3 FAILED] Tombol '{btn.name}' tidak berada di dalam Canvas dengan GraphicRaycaster!");
                    allPassed = false;
                }
            }
            if (allPassed)
            {
                Debug.Log("[VALIDASI 3 PASSED] Semua tombol UI berada dalam Canvas interaktif dengan GraphicRaycaster.");
            }

            // 4. Validasi Non-interactive Raycast Targets
            Image[] images = Object.FindObjectsOfType<Image>(true);
            foreach (var img in images)
            {
                if (img.gameObject.name == "TopScoreBanner" || img.gameObject.name == "PlayerHUD" ||
                    img.gameObject.name == "KillFeedItem" || (img.transform.parent != null && img.transform.parent.name == "ResultPanel" && img.gameObject.name == "ResultPanel"))
                {
                    if (img.raycastTarget)
                    {
                        Debug.LogWarning($"[VALIDASI 4 WARNING] Elemen dekoratif non-interaktif '{img.name}' masih memiliki raycastTarget = true!");
                    }
                }
            }

            // 5. Validasi World-Space Health Bars pada 6 Karakter
            Health[] allHealth = Object.FindObjectsOfType<Health>();
            int playerBars = 0;
            int blueBars = 0;
            int redBars = 0;

            foreach (var h in allHealth)
            {
                HealthBarWorld[] bars = h.GetComponentsInChildren<HealthBarWorld>(true);
                if (bars.Length != 1)
                {
                    Debug.LogError($"[VALIDASI 5 FAILED] Karakter '{h.CharacterName}' memiliki {bars.Length} HealthBarWorld (harus TEPAT 1)!");
                    allPassed = false;
                }
                else
                {
                    // Periksa tidak ada GraphicRaycaster pada world canvas
                    var gr = bars[0].GetComponent<GraphicRaycaster>();
                    if (gr != null)
                    {
                        Debug.LogError($"[VALIDASI 5 FAILED] HealthBarWorld pada '{h.CharacterName}' memiliki GraphicRaycaster yang dapat mencegat klik!");
                        allPassed = false;
                    }

                    if (h.IsPlayer) playerBars++;
                    else if (h.TeamId == 0) blueBars++;
                    else if (h.TeamId == 1) redBars++;
                }
            }

            if (playerBars == 1 && blueBars == 2 && redBars == 3)
            {
                Debug.Log($"[VALIDASI 5 PASSED] 6 Karakter terverifikasi: 1 Player ({playerBars}), 2 Blue Bot ({blueBars}), 3 Red Bot ({redBars}). Semua memiliki HealthBarWorld tanpa duplikat.");
            }
            else
            {
                Debug.LogError($"[VALIDASI 5 FAILED] Komposisi roster tidak sesuai 3v3! Player={playerBars}, BlueBots={blueBars}, RedBots={redBars}");
                allPassed = false;
            }

            if (allPassed)
            {
                Debug.Log("<color=green>[ArenaClashBuilder] === SEMUA VALIDASI EDITMODE BERHASIL LULUS! ===</color>");
            }
            else
            {
                Debug.LogError("<color=red>[ArenaClashBuilder] === TERDAPAT VALIDASI YANG GAGAL! ===</color>");
            }

            return allPassed;
        }

        public static void ValidateArenaClashFromCommandLine()
        {
            Debug.Log("[ArenaClashBuilder] Menjalankan validasi dari command line...");
            bool passed = ValidateArenaClash();
            EditorApplication.Exit(passed ? 0 : 1);
        }
        #endregion

        #region Helpers
        private static TextMeshProUGUI CreateTextTMP(Transform parent, string name, string text, float fontSize,
            Vector2 pos, Vector2 size, Color color, FontStyles style, TextAlignmentOptions align)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.raycastTarget = false;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return tmp;
        }

        private static Button CreateCyberButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color btnColor)
        {
            GameObject btnGo = new GameObject(name);
            btnGo.transform.SetParent(parent, false);
            Image img = btnGo.AddComponent<Image>();
            img.color = btnColor;
            img.raycastTarget = true; // Tombol interaktif harus menerima raycast

            Button btn = btnGo.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = btnColor;
            cb.highlightedColor = btnColor * 1.25f;
            cb.pressedColor = btnColor * 0.8f;
            cb.disabledColor = new Color(0.2f, 0.2f, 0.25f, 0.5f);
            btn.colors = cb;

            RectTransform rt = btnGo.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            CreateTextTMP(btnGo.transform, "Label", label, 18, Vector2.zero, size, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            return btn;
        }

        private static Slider CreateProgressBar(Transform parent, string name, Vector2 pos, Vector2 size, Color fillColor)
        {
            GameObject sliderGo = new GameObject(name);
            sliderGo.transform.SetParent(parent, false);
            Slider slider = sliderGo.AddComponent<Slider>();
            slider.interactable = false; // Slider status display tidak perlu interaktif
            slider.transition = Selectable.Transition.None;

            RectTransform rt = sliderGo.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderGo.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.12f, 0.18f, 0.9f);
            bgImg.raycastTarget = false; // BUG FIX: Raycast Target OFF
            RectTransform bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderGo.transform, false);
            RectTransform faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = Vector2.zero;
            faRt.anchorMax = Vector2.one;
            faRt.sizeDelta = Vector2.zero;

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = fillColor;
            fillImg.raycastTarget = false; // BUG FIX: Raycast Target OFF
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            fillRt.sizeDelta = Vector2.zero;

            slider.fillRect = fillRt;
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.value = 100f;
            return slider;
        }

        private static Material CreateURPMaterial(string name, Color color, float smoothness = 0.5f)
        {
            string matPath = $"{MAT_DIR}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                mat = new Material(shader);
                mat.color = color;
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.color = color;
            }
            return mat;
        }

        private static void ClearScene()
        {
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in roots)
            {
                if (root != null)
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null)
            {
                field.SetValue(target, value);
            }
            else
            {
                Debug.LogWarning($"[ArenaClashBuilder] Field '{fieldName}' tidak ditemukan pada {target.GetType().Name}");
            }
        }
        #endregion
    }
}
#endif
