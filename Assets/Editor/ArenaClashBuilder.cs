#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
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
    /// Builder Otomatis untuk Arena Clash 3D (Fase 1 sampai 5):
    /// - Arena Blockout 30x30 simetris, NavMeshSurface baking
    /// - Kamera top-down 55 derajat dengan smooth follow & mouse look-ahead
    /// - Player controller WASD (diagonal dinormalisasi) + mouse aiming Y-axis
    /// - Projectile pooling (pool 60) + PlayerCombat (interval 0.4s)
    /// - Bot AI 3v3 (FSM: Idle, Seek, Chase, Attack, Strafe, Retreat, Dead) + Utility target scoring
    /// - Format 3v3 (1 Player + 2 Blue Bot vs 3 Red Bot), Countdown 3-2-1, Match Timer 180s, Target 10 eliminasi
    /// - HUD, World Health Bars, Kill Feed, Floating Damage Number, Result Panel, Restart & Hub Integration
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

            // UI Canvas
            BuildArenaUI(dmgNumPrefab);

            // Save Scene
            EditorSceneManager.SaveScene(scene, SCENE_SYAFIQ);
            Debug.Log($"[ArenaClashBuilder] Scene Arena Clash 3D tersimpan sempurna di {SCENE_SYAFIQ}");

            if (interactive)
            {
                EditorUtility.DisplayDialog("Arena Clash 3D",
                    "MVP Arena Clash 3D (Fase 1 sampai 5) berhasil dibangun!\n\n" +
                    "- Arena 30x30 simetris dengan NavMesh baked\n" +
                    "- Player WASD + Mouse Aiming Y-axis\n" +
                    "- 3v3 Team Deathmatch (1 Player + 2 Blue Bot vs 3 Red Bot)\n" +
                    "- Projectile Pool 60 & Friendly Fire Protection\n" +
                    "- Bot AI FSM + Utility Target Scoring\n" +
                    "- HUD, Health Bars, Countdown 3-2-1, Timer 180s, & Result Panel\n" +
                    "- Tekan PLAY untuk menguji langsung!", "Siap Bertanding!");
            }
        }

        #region Setup Assets & Directories
        private static void EnsureDirectories()
        {
            string[] dirs = { DATA_DIR, PREFAB_DIR, MAT_DIR, "Assets/Games/Syafiq/Scenes" };
            foreach (var d in dirs)
            {
                if (!Directory.Exists(d)) Directory.CreateDirectory(d);
            }
            AssetDatabase.Refresh();
        }

        private static void SetupPhysicsLayers()
        {
            int projLayer = LayerMask.NameToLayer("Syafiq_Projectile");
            if (projLayer >= 0)
            {
                Physics.IgnoreLayerCollision(projLayer, projLayer, true);
            }
        }

        private static SO_Projectile CreateOrLoadProjectileData()
        {
            string path = $"{DATA_DIR}/SO_Projectile_Blaze.asset";
            SO_Projectile data = AssetDatabase.LoadAssetAtPath<SO_Projectile>(path);
            if (data == null)
            {
                data = ScriptableObject.CreateInstance<SO_Projectile>();
                data.damage = 20;
                data.speed = 14f;
                data.lifetime = 0.8f;
                data.radius = 0.25f;
                AssetDatabase.CreateAsset(data, path);
            }
            return data;
        }

        private static SO_Hero CreateOrLoadHeroData(SO_Projectile projData)
        {
            string path = $"{DATA_DIR}/SO_Hero_Blaze.asset";
            SO_Hero hero = AssetDatabase.LoadAssetAtPath<SO_Hero>(path);
            if (hero == null)
            {
                hero = ScriptableObject.CreateInstance<SO_Hero>();
                hero.heroName = "Blaze";
                hero.role = "Gunner Seimbang";
                hero.maxHealth = 100;
                hero.moveSpeed = 5.0f;
                hero.fireInterval = 0.4f;
                hero.projectileData = projData;
                AssetDatabase.CreateAsset(hero, path);
            }
            return hero;
        }

        private static SO_BotDifficulty CreateOrLoadBotDifficulty()
        {
            string path = $"{DATA_DIR}/SO_BotDifficulty_Normal.asset";
            SO_BotDifficulty diff = AssetDatabase.LoadAssetAtPath<SO_BotDifficulty>(path);
            if (diff == null)
            {
                diff = ScriptableObject.CreateInstance<SO_BotDifficulty>();
                diff.difficultyName = "Normal";
                diff.reactionTime = 0.5f;
                diff.aimErrorDegrees = 8f;
                diff.fireInterval = 1.0f;
                diff.moveSpeed = 4.2f;
                diff.retreatHealthRatio = 0.25f;
                AssetDatabase.CreateAsset(diff, path);
            }
            return diff;
        }

        private static SO_MatchSettings CreateOrLoadMatchSettings()
        {
            string path = $"{DATA_DIR}/SO_MatchSettings_Standard.asset";
            SO_MatchSettings settings = AssetDatabase.LoadAssetAtPath<SO_MatchSettings>(path);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<SO_MatchSettings>();
                settings.targetEliminations = 10;
                settings.matchDuration = 180f;
                settings.countdownDuration = 3f;
                settings.respawnTime = 4f;
                AssetDatabase.CreateAsset(settings, path);
            }
            return settings;
        }

        private static GameObject CreateProjectilePrefab(SO_Projectile data)
        {
            string path = $"{PREFAB_DIR}/PF_Projectile.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "PF_Projectile";
            go.transform.localScale = Vector3.one * 0.4f;

            int projLayer = LayerMask.NameToLayer("Syafiq_Projectile");
            if (projLayer >= 0) go.layer = projLayer;

            Collider col = go.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;

            Material mat = CreateURPMaterial("Mat_Blaze_Laser", new Color(0f, 1f, 0.95f), 0.9f);
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
            tmp.raycastTarget = false;

            go.AddComponent<DamageNumber>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject CreateHealthBarPrefab()
        {
            string path = $"{PREFAB_DIR}/PF_HealthBarWorld.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            GameObject go = new GameObject("PF_HealthBarWorld");
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1.2f, 0.2f);

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            go.AddComponent<Slider>();

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(go.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);
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
            fillImg.color = Color.green;
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;

            Slider slider = go.GetComponent<Slider>();
            slider.fillRect = fillRt;
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 100;

            HealthBarWorld hbw = go.AddComponent<HealthBarWorld>();

            // Setup private fields via reflection
            SetPrivateField(hbw, "_slider", slider);
            SetPrivateField(hbw, "_fillImage", fillImg);

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

            // 2. Thick Boundary Walls (>= 1 unit thick, GDD Fase 1: 2 unit tebal, tinggi 2.5)
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

            // World Health Bar
            AttachWorldHealthBar(player, health, new Color(0f, 0.9f, 1f), healthBarPrefab);

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
            if (prefab == null) return;
            GameObject hbGo = (GameObject)PrefabUtility.InstantiatePrefab(prefab, owner.transform);
            hbGo.name = "HealthBarWorld";
            HealthBarWorld hbw = hbGo.GetComponent<HealthBarWorld>();
            if (hbw != null)
            {
                hbw.Setup(health, owner.transform, teamColor);
            }
        }
        #endregion

        #region Camera & Lights
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
        #endregion

        #region UI Canvas
        private static void BuildArenaUI(GameObject dmgNumPrefab)
        {
            GameObject canvasGo = new GameObject("ArenaGameCanvas");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();

            // 1. Top Score & Timer Banner
            GameObject topBanner = new GameObject("TopScoreBanner");
            topBanner.transform.SetParent(canvasGo.transform, false);
            RectTransform bannerRt = topBanner.AddComponent<RectTransform>();
            bannerRt.anchoredPosition = new Vector2(0, 480);
            bannerRt.sizeDelta = new Vector2(600, 70);

            Image bannerBg = topBanner.AddComponent<Image>();
            bannerBg.color = new Color(0.06f, 0.08f, 0.14f, 0.9f);

            TextMeshProUGUI blueScoreTxt = CreateTextTMP(topBanner.transform, "BlueScore", "0", 38,
                new Vector2(-160, 0), new Vector2(100, 60), new Color(0f, 0.9f, 1f), FontStyles.Bold, TextAlignmentOptions.Center);

            TextMeshProUGUI timerTxt = CreateTextTMP(topBanner.transform, "Timer", "03:00", 32,
                new Vector2(0, 0), new Vector2(160, 60), Color.white, FontStyles.Bold, TextAlignmentOptions.Center);

            TextMeshProUGUI redScoreTxt = CreateTextTMP(topBanner.transform, "RedScore", "0", 38,
                new Vector2(160, 0), new Vector2(100, 60), new Color(1f, 0.3f, 0.3f), FontStyles.Bold, TextAlignmentOptions.Center);

            // 2. Countdown Banner (Center Screen)
            GameObject countdownGo = new GameObject("CountdownBanner");
            countdownGo.transform.SetParent(canvasGo.transform, false);
            RectTransform cdRt = countdownGo.AddComponent<RectTransform>();
            cdRt.anchoredPosition = new Vector2(0, 80);
            cdRt.sizeDelta = new Vector2(500, 150);

            TextMeshProUGUI countdownTxt = CreateTextTMP(countdownGo.transform, "CountdownText", "3", 84,
                Vector2.zero, new Vector2(500, 150), new Color(0f, 1f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Center);

            // 3. Player HUD (Bottom Left)
            GameObject playerHudGo = new GameObject("PlayerHUD");
            playerHudGo.transform.SetParent(canvasGo.transform, false);
            RectTransform pHudRt = playerHudGo.AddComponent<RectTransform>();
            pHudRt.anchoredPosition = new Vector2(-660, -460);
            pHudRt.sizeDelta = new Vector2(360, 90);

            Image pHudBg = playerHudGo.AddComponent<Image>();
            pHudBg.color = new Color(0.06f, 0.08f, 0.14f, 0.85f);

            CreateTextTMP(playerHudGo.transform, "PlayerLabel", "BLAZE (YOU)", 16,
                new Vector2(0, 24), new Vector2(320, 30), new Color(0f, 1f, 0.9f), FontStyles.Bold, TextAlignmentOptions.Left);

            Slider hpSlider = CreateProgressBar(playerHudGo.transform, "PlayerHPSlider", new Vector2(0, -12), new Vector2(320, 22), new Color(0f, 0.85f, 0.4f));
            TextMeshProUGUI hpTxt = CreateTextTMP(playerHudGo.transform, "HPText", "100 / 100 HP", 14,
                new Vector2(0, -12), new Vector2(320, 22), Color.white, FontStyles.Bold, TextAlignmentOptions.Center);

            // 4. Kill Feed Container (Top Right)
            GameObject killFeedGo = new GameObject("KillFeedContainer");
            killFeedGo.transform.SetParent(canvasGo.transform, false);
            RectTransform kfRt = killFeedGo.AddComponent<RectTransform>();
            kfRt.anchoredPosition = new Vector2(660, 380);
            kfRt.sizeDelta = new Vector2(340, 260);

            GameObject kfItemPrefab = CreateKillFeedItemPrefab(canvasGo.transform);
            KillFeedUI kfUI = canvasGo.AddComponent<KillFeedUI>();
            kfUI.BindContainer(killFeedGo.transform, kfItemPrefab);

            // 5. Damage Number Container
            GameObject dmgNumContainer = new GameObject("DamageNumberContainer");
            dmgNumContainer.transform.SetParent(canvasGo.transform, false);

            // 6. ArenaHUD Component
            ArenaHUD hud = canvasGo.AddComponent<ArenaHUD>();
            hud.BindControls(blueScoreTxt, redScoreTxt, timerTxt, countdownGo, countdownTxt, hpSlider, hpTxt, dmgNumPrefab, dmgNumContainer.transform);

            // 7. Result Panel
            BuildResultPanel(canvasGo.transform);

            // 8. Global Pause Overlay helper
            BuildGameOverlayCanvas(canvasGo.transform);
        }

        private static GameObject CreateKillFeedItemPrefab(Transform parent)
        {
            GameObject item = new GameObject("KillFeedItem");
            item.transform.SetParent(parent, false);
            RectTransform rt = item.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(320, 32);

            Image bg = item.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.08f, 0.12f, 0.8f);

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

        private static void BuildGameOverlayCanvas(Transform parent)
        {
            // Pause button on top right
            Button pauseBtn = CreateCyberButton(parent, "Btn_Pause", "PAUSE (Esc)",
                new Vector2(760, 460), new Vector2(170, 42), new Color(0.2f, 0.35f, 0.5f, 0.9f));

            // Pause Overlay Panel
            GameObject pausePanel = new GameObject("PausePanel");
            pausePanel.transform.SetParent(parent, false);
            RectTransform rt = pausePanel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image dimImg = pausePanel.AddComponent<Image>();
            dimImg.color = new Color(0.04f, 0.06f, 0.12f, 0.94f);

            CreateTextTMP(pausePanel.transform, "PauseTitle", "GAME PAUSED", 48,
                new Vector2(0, 160), new Vector2(500, 70), new Color(0.95f, 0.85f, 0.2f), FontStyles.Bold, TextAlignmentOptions.Center);

            Button resumeBtn = CreateCyberButton(pausePanel.transform, "Btn_Resume", "RESUME", new Vector2(0, 50), new Vector2(300, 50), new Color(0f, 0.65f, 0.6f));
            Button restartBtn = CreateCyberButton(pausePanel.transform, "Btn_Restart", "RESTART GAME", new Vector2(0, -15), new Vector2(300, 48), new Color(0.2f, 0.35f, 0.55f));
            Button switchBtn = CreateCyberButton(pausePanel.transform, "Btn_SwitchGame", "SWITCH GAME", new Vector2(0, -75), new Vector2(300, 48), new Color(0.45f, 0.3f, 0.6f));
            Button menuBtn = CreateCyberButton(pausePanel.transform, "Btn_MainMenu", "MAIN MENU", new Vector2(0, -135), new Vector2(300, 48), new Color(0.5f, 0.2f, 0.2f));

            GameOverlayUI overlayUI = parent.gameObject.AddComponent<GameOverlayUI>();
            overlayUI.BindControls(pausePanel, pauseBtn, resumeBtn, restartBtn, switchBtn, menuBtn);
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

            RectTransform rt = sliderGo.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            // Background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderGo.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.12f, 0.18f, 0.9f);
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
