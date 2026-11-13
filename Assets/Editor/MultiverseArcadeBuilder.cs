#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using MultiverseArcade.Core;
using MultiverseArcade.UI;
using MultiverseArcade.Games.Syafiq;

namespace MultiverseArcade.EditorTools
{
    /// <summary>
    /// Builder Editor otomatis untuk membangun seluruh Scene pada proyek Multiverse Arcade:
    /// 1. MainMenu.unity (Title, Cyber background, Play & Quit button, MainMenuUI)
    /// 2. GameSelection.unity (5 Game Cards, Play buttons, Back button, GameSelectionUI)
    /// 3. Game_Syafiq.unity (Full 3D Arena Shooter 3v3: Cyber floor, neon boundary walls, pillars, player, spawner, HUD, GameOver screen, GameOverlayUI)
    /// 4. Teammate scenes (Game_Jaya, Game_Prima, Game_Brili, Game_Risa: Demo arenas with GameOverlayUI navigation)
    /// </summary>
    [InitializeOnLoad]
    public static class MultiverseArcadeAutoBuilder
    {
        static MultiverseArcadeAutoBuilder()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorPrefs.GetBool("MultiverseArcade_AutoBuilt_v5", false))
                {
                    EditorPrefs.SetBool("MultiverseArcade_AutoBuilt_v5", true);
                    Debug.Log("[MultiverseArcadeAutoBuilder] Membangun ulang dan memperbarui semua scene Multiverse Arcade...");
                    MultiverseArcadeBuilder.BuildAllScenesInternal(false);
                }
            };
        }
    }

    public static class MultiverseArcadeBuilder
    {
        private const string SCENE_MAIN_MENU = "Assets/_Project/Scenes/MainMenu.unity";
        private const string SCENE_GAME_SELECTION = "Assets/_Project/Scenes/GameSelection.unity";
        private const string SCENE_SYAFIQ = "Assets/Games/Syafiq/Scenes/Game_Syafiq.unity";
        private const string SCENE_JAYA = "Assets/Games/Jaya/Scenes/Game_Jaya.unity";
        private const string SCENE_PRIMA = "Assets/Games/Prima/Scenes/Game_Prima.unity";
        private const string SCENE_BRILI = "Assets/Games/Brili/Scenes/Game_Brili.unity";
        private const string SCENE_RISA = "Assets/Games/Risa/Scenes/Game_Risa.unity";

        [MenuItem("Tools/Multiverse Arcade/🚀 Build All Scenes (Auto Setup)", false, 0)]
        public static void BuildAllScenes()
        {
            BuildAllScenesInternal(true);
        }

        public static void BuildAllScenesInternal(bool showDialog)
        {
            EnsureTMPResources();

            Debug.Log("[MultiverseArcadeBuilder] === MEMULAI AUTO BUILD SEMUA SCENE ===");

            BuildMainMenuScene();
            BuildGameSelectionScene();
            BuildSyafiqScene();
            BuildTeammateScene(SCENE_JAYA, "Space Defender", "Jaya", "Arcade Space Shooter", new Color(0.2f, 0.4f, 0.9f));
            BuildTeammateScene(SCENE_PRIMA, "Dungeon Crawler", "Prima", "Action RPG Dungeon", new Color(0.7f, 0.2f, 0.3f));
            BuildTeammateScene(SCENE_BRILI, "Speed Racer", "Brili", "Sci-Fi High Speed Racing", new Color(0.9f, 0.6f, 0.1f));
            BuildTeammateScene(SCENE_RISA, "Puzzle Matrix", "Risa", "Cyber Logic Puzzle", new Color(0.6f, 0.2f, 0.8f));

            MultiverseArcadeSetup.SetupBuildScenes();

            // Buka kembali MainMenu sebagai scene aktif
            EditorSceneManager.OpenScene(SCENE_MAIN_MENU);
            Debug.Log("[MultiverseArcadeBuilder] 🎉 SELURUH SCENE BERHASIL DIBANGUN & DIKONFIGURASI!");
            if (showDialog)
            {
                EditorUtility.DisplayDialog("Multiverse Arcade Builder", "Semua scene (MainMenu, GameSelection, Game_Syafiq, dan scene tim) berhasil dibangun & dikonfigurasi penuh!\n\nScene MainMenu sudah aktif dibuka dan siap di-Play.", "Mantap!");
            }
        }

        private static void EnsureTMPResources()
        {
            string tmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";
            if (!File.Exists(tmpSettingsPath))
            {
                string pkgPath = "Library/PackageCache/com.unity.textmeshpro@3.0.7/Package Resources/TMP Essential Resources.unitypackage";
                if (File.Exists(pkgPath))
                {
                    AssetDatabase.ImportPackage(pkgPath, false);
                    AssetDatabase.Refresh();
                    Debug.Log("[MultiverseArcadeBuilder] TMP Essential Resources berhasil diimpor.");
                }
            }
        }

        #region 1. MainMenu Scene Builder
        [MenuItem("Tools/Multiverse Arcade/Build MainMenu Scene", false, 10)]
        public static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.OpenScene(SCENE_MAIN_MENU);
            ClearScene();

            // 1. Camera & Lighting
            GameObject camGo = CreateCamera(new Vector3(0, 1, -10), Quaternion.identity, new Color(0.05f, 0.07f, 0.12f));
            CreateDirectionalLight(new Vector3(50, -30, 0), Color.white, 1.0f);
            CreateEventSystem();

            // 2. Core Managers
            CreateCoreManagers();

            // 3. UI Canvas
            GameObject canvasGo = CreateCanvas("MainMenuCanvas", 1920, 1080);
            Transform root = canvasGo.transform;

            // Background Image
            GameObject bgGo = CreateUIObject("Background", root);
            RectTransform bgRect = bgGo.GetComponent<RectTransform>();
            StretchFull(bgRect);
            Image bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.06f, 0.08f, 0.14f, 1f);

            // Container Tengah
            GameObject centerPanel = CreateUIObject("CenterPanel", root);
            RectTransform centerRect = centerPanel.GetComponent<RectTransform>();
            centerRect.anchorMin = new Vector2(0.5f, 0.5f);
            centerRect.anchorMax = new Vector2(0.5f, 0.5f);
            centerRect.pivot = new Vector2(0.5f, 0.5f);
            centerRect.sizeDelta = new Vector2(800, 600);

            // Title
            TextMeshProUGUI title = CreateTMPText("TitleText", centerPanel.transform, "MULTIVERSE ARCADE", 64, FontStyles.Bold, new Color(0f, 0.9f, 1f));
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 140);
            titleRect.sizeDelta = new Vector2(800, 100);

            // Subtitle
            TextMeshProUGUI subtitle = CreateTMPText("SubtitleText", centerPanel.transform, "★ 5 GAMES • 1 EXPERIENCE • INFINITE ACTION ★", 20, FontStyles.Normal, new Color(1f, 0.85f, 0.3f));
            RectTransform subRect = subtitle.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0, 70);
            subRect.sizeDelta = new Vector2(800, 50);

            // Play Button
            Button btnPlay = CreateButton("PlayButton", centerPanel.transform, "MASUK ARCADE (PLAY)", new Vector2(0, -30), new Vector2(340, 64), new Color(0f, 0.75f, 0.9f), Color.white, 24);

            // Quit Button
            Button btnQuit = CreateButton("QuitButton", centerPanel.transform, "KELUAR (QUIT)", new Vector2(0, -115), new Vector2(340, 54), new Color(0.8f, 0.2f, 0.25f), Color.white, 20);

            // Version info footer
            TextMeshProUGUI footer = CreateTMPText("FooterText", root, "Multiverse Arcade v1.0 • Built with Unity URP & Antigravity Pair-Programming", 14, FontStyles.Italic, new Color(0.5f, 0.6f, 0.7f));
            RectTransform footRect = footer.GetComponent<RectTransform>();
            footRect.anchorMin = new Vector2(0.5f, 0f);
            footRect.anchorMax = new Vector2(0.5f, 0f);
            footRect.pivot = new Vector2(0.5f, 0f);
            footRect.anchoredPosition = new Vector2(0, 20);
            footRect.sizeDelta = new Vector2(800, 40);

            // MainMenuUI Component
            MainMenuUI menuUI = canvasGo.AddComponent<MainMenuUI>();
            SetPrivateField(menuUI, "playButton", btnPlay);
            SetPrivateField(menuUI, "quitButton", btnQuit);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MultiverseArcadeBuilder] Scene MainMenu berhasil dibangun!");
        }
        #endregion

        #region 2. GameSelection Scene Builder
        [MenuItem("Tools/Multiverse Arcade/Build GameSelection Scene", false, 11)]
        public static void BuildGameSelectionScene()
        {
            var scene = EditorSceneManager.OpenScene(SCENE_GAME_SELECTION);
            ClearScene();

            CreateCamera(new Vector3(0, 1, -10), Quaternion.identity, new Color(0.04f, 0.05f, 0.1f));
            CreateDirectionalLight(new Vector3(50, -30, 0), Color.white, 1.0f);
            CreateEventSystem();
            CreateCoreManagers();

            GameObject canvasGo = CreateCanvas("GameSelectionCanvas", 1920, 1080);
            Transform root = canvasGo.transform;

            // Background
            GameObject bgGo = CreateUIObject("Background", root);
            StretchFull(bgGo.GetComponent<RectTransform>());
            Image bgImg = bgGo.AddComponent<Image>();
            bgImg.color = new Color(0.05f, 0.06f, 0.12f, 1f);

            // Header Title
            TextMeshProUGUI header = CreateTMPText("HeaderText", root, "PILIH GAME ARKADE", 48, FontStyles.Bold, new Color(0f, 0.95f, 1f));
            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0.5f, 1f);
            headerRect.anchorMax = new Vector2(0.5f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0, -50);
            headerRect.sizeDelta = new Vector2(800, 70);

            // Subheader
            TextMeshProUGUI subHeader = CreateTMPText("SubheaderText", root, "Pilih salah satu karya anggota tim untuk dimainkan:", 18, FontStyles.Normal, new Color(0.7f, 0.8f, 0.9f));
            RectTransform subHeaderRect = subHeader.GetComponent<RectTransform>();
            subHeaderRect.anchorMin = new Vector2(0.5f, 1f);
            subHeaderRect.anchorMax = new Vector2(0.5f, 1f);
            subHeaderRect.pivot = new Vector2(0.5f, 1f);
            subHeaderRect.anchoredPosition = new Vector2(0, -115);
            subHeaderRect.sizeDelta = new Vector2(800, 40);

            // Cards Container (Horizontal Layout)
            GameObject cardsContainer = CreateUIObject("CardsContainer", root);
            RectTransform cardsRect = cardsContainer.GetComponent<RectTransform>();
            cardsRect.anchorMin = new Vector2(0.5f, 0.5f);
            cardsRect.anchorMax = new Vector2(0.5f, 0.5f);
            cardsRect.pivot = new Vector2(0.5f, 0.5f);
            cardsRect.anchoredPosition = new Vector2(0, -15);
            cardsRect.sizeDelta = new Vector2(1650, 520);

            HorizontalLayoutGroup hlg = cardsContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 25;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // Buat 5 Kartu Game
            Button btnSyafiq = CreateGameCard(cardsContainer.transform, "Arena Clash 3D", "Syafiq (Hero)", "3v3 Arena Shooter dengan wave survival & laser blaster.", "▶ MAINKAN SEKARANG", new Color(0f, 0.8f, 0.9f), true);
            Button btnJaya   = CreateGameCard(cardsContainer.transform, "Space Defender", "Jaya", "Retro Arcade Space Shooter melawan armada alien.", "EXPLORE DEMO", new Color(0.2f, 0.5f, 0.9f), false);
            Button btnPrima  = CreateGameCard(cardsContainer.transform, "Dungeon Crawler", "Prima", "Eksplorasi dungeon gelap dan hadapi monster berbahaya.", "EXPLORE DEMO", new Color(0.85f, 0.25f, 0.35f), false);
            Button btnBrili  = CreateGameCard(cardsContainer.transform, "Speed Racer", "Brili", "Balapan kecepatan tinggi di trek neon futuristik.", "EXPLORE DEMO", new Color(0.95f, 0.65f, 0.1f), false);
            Button btnRisa   = CreateGameCard(cardsContainer.transform, "Puzzle Matrix", "Risa", "Tantangan teka-teki logika grid cyber.", "EXPLORE DEMO", new Color(0.65f, 0.3f, 0.85f), false);

            // Back Button
            Button btnBack = CreateButton("BackButton", root, "< KEMBALI KE MENU UTAMA", new Vector2(0, 50), new Vector2(340, 52), new Color(0.18f, 0.22f, 0.3f), Color.white, 18);
            RectTransform backRect = btnBack.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.5f, 0f);
            backRect.anchorMax = new Vector2(0.5f, 0f);
            backRect.pivot = new Vector2(0.5f, 0f);
            backRect.anchoredPosition = new Vector2(0, 40);

            // GameSelectionUI Component
            GameSelectionUI selectionUI = canvasGo.AddComponent<GameSelectionUI>();
            SetPrivateField(selectionUI, "btnGameSyafiq", btnSyafiq);
            SetPrivateField(selectionUI, "btnGameJaya", btnJaya);
            SetPrivateField(selectionUI, "btnGamePrima", btnPrima);
            SetPrivateField(selectionUI, "btnGameBrili", btnBrili);
            SetPrivateField(selectionUI, "btnGameRisa", btnRisa);
            SetPrivateField(selectionUI, "btnBackToMainMenu", btnBack);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MultiverseArcadeBuilder] Scene GameSelection berhasil dibangun!");
        }

        private static Button CreateGameCard(Transform parent, string title, string author, string description, string buttonText, Color themeColor, bool isPlayable)
        {
            GameObject card = CreateUIObject($"Card_{author}", parent);
            RectTransform cr = card.GetComponent<RectTransform>();
            cr.sizeDelta = new Vector2(300, 480);

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = new Color(0.09f, 0.12f, 0.18f, 0.95f);

            // Top Banner Glow
            GameObject topBanner = CreateUIObject("TopBanner", card.transform);
            RectTransform tbr = topBanner.GetComponent<RectTransform>();
            tbr.anchorMin = new Vector2(0, 1);
            tbr.anchorMax = new Vector2(1, 1);
            tbr.pivot = new Vector2(0.5f, 1);
            tbr.sizeDelta = new Vector2(0, 8);
            Image tbImg = topBanner.AddComponent<Image>();
            tbImg.color = themeColor;

            // Status Tag
            string tagText = isPlayable ? "● READY TO PLAY" : "○ IN DEVELOPMENT";
            Color tagColor = isPlayable ? new Color(0.2f, 0.9f, 0.4f) : new Color(0.8f, 0.8f, 0.8f);
            TextMeshProUGUI tag = CreateTMPText("Tag", card.transform, tagText, 12, FontStyles.Bold, tagColor);
            RectTransform tagRect = tag.GetComponent<RectTransform>();
            tagRect.anchoredPosition = new Vector2(0, 195);
            tagRect.sizeDelta = new Vector2(260, 25);

            // Title
            TextMeshProUGUI t = CreateTMPText("Title", card.transform, title, 22, FontStyles.Bold, Color.white);
            RectTransform tr = t.GetComponent<RectTransform>();
            tr.anchoredPosition = new Vector2(0, 145);
            tr.sizeDelta = new Vector2(260, 50);

            // Author
            TextMeshProUGUI a = CreateTMPText("Author", card.transform, $"By: {author}", 14, FontStyles.Italic, themeColor);
            RectTransform ar = a.GetComponent<RectTransform>();
            ar.anchoredPosition = new Vector2(0, 95);
            ar.sizeDelta = new Vector2(260, 30);

            // Card Graphic Preview (Cube box representation)
            GameObject preview = CreateUIObject("PreviewBox", card.transform);
            RectTransform pr = preview.GetComponent<RectTransform>();
            pr.anchoredPosition = new Vector2(0, 15);
            pr.sizeDelta = new Vector2(260, 110);
            Image prImg = preview.AddComponent<Image>();
            prImg.color = new Color(themeColor.r * 0.25f, themeColor.g * 0.25f, themeColor.b * 0.25f, 0.8f);

            TextMeshProUGUI iconTxt = CreateTMPText("Icon", preview.transform, isPlayable ? "⚡ ARENA 3D ⚡" : "🎮 ARCADE", 16, FontStyles.Bold, themeColor);
            StretchFull(iconTxt.GetComponent<RectTransform>());

            // Description
            TextMeshProUGUI d = CreateTMPText("Description", card.transform, description, 13, FontStyles.Normal, new Color(0.7f, 0.75f, 0.85f));
            RectTransform dr = d.GetComponent<RectTransform>();
            dr.anchoredPosition = new Vector2(0, -90);
            dr.sizeDelta = new Vector2(260, 70);

            // Button
            Color btnBgColor = isPlayable ? themeColor : new Color(0.2f, 0.25f, 0.35f);
            Button btn = CreateButton("PlayBtn", card.transform, buttonText, new Vector2(0, -185), new Vector2(260, 50), btnBgColor, Color.white, 16);

            return btn;
        }
        #endregion

        #region 3. Game_Syafiq Scene Builder (Arena Clash 3D)
        [MenuItem("Tools/Multiverse Arcade/Build Syafiq Arena Scene", false, 12)]
        public static void BuildSyafiqScene()
        {
            var scene = EditorSceneManager.OpenScene(SCENE_SYAFIQ);
            ClearScene();

            // Materials
            Material floorMat = CreateURPMaterial("M_CyberFloor", new Color(0.08f, 0.1f, 0.14f), 0.2f);
            Material wallMat = CreateURPMaterial("M_NeonWall", new Color(0.12f, 0.15f, 0.22f), 0.1f);
            Material pillarMat = CreateURPMaterial("M_NeonPillar", new Color(0.15f, 0.2f, 0.3f), 0.3f);
            Material playerMat = CreateURPMaterial("M_PlayerTeal", new Color(0f, 0.85f, 0.95f), 0.5f);
            Material visorMat = CreateURPMaterial("M_VisorDark", new Color(0.02f, 0.05f, 0.1f), 0.9f);
            Material neonGlowMat = CreateURPMaterial("M_NeonCyanGlow", new Color(0f, 1f, 1f), 0.8f);

            // 1. Directional Light & Atmosphere
            CreateDirectionalLight(new Vector3(50, -45, 0), new Color(0.85f, 0.9f, 1.0f), 1.2f);
            CreateCoreManagers();

            // 2. Arena Geometry
            GameObject arenaRoot = new GameObject("--- ARENA ENVIRONMENT ---");

            // Floor (30x1x30)
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Arena_Floor";
            floor.transform.parent = arenaRoot.transform;
            floor.transform.position = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(32, 1, 32);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // 4 Boundary Walls
            CreateWall(arenaRoot.transform, "Wall_North", new Vector3(0, 1.5f, 16f), new Vector3(32, 3, 1), wallMat, neonGlowMat);
            CreateWall(arenaRoot.transform, "Wall_South", new Vector3(0, 1.5f, -16f), new Vector3(32, 3, 1), wallMat, neonGlowMat);
            CreateWall(arenaRoot.transform, "Wall_East",  new Vector3(16f, 1.5f, 0),  new Vector3(1, 3, 32), wallMat, neonGlowMat);
            CreateWall(arenaRoot.transform, "Wall_West",  new Vector3(-16f, 1.5f, 0), new Vector3(1, 3, 32), wallMat, neonGlowMat);

            // 4 Obstacle Pillars
            CreatePillar(arenaRoot.transform, "Pillar_NW", new Vector3(-7, 2f, 7), pillarMat);
            CreatePillar(arenaRoot.transform, "Pillar_NE", new Vector3(7, 2f, 7), pillarMat);
            CreatePillar(arenaRoot.transform, "Pillar_SW", new Vector3(-7, 2f, -7), pillarMat);
            CreatePillar(arenaRoot.transform, "Pillar_SE", new Vector3(7, 2f, -7), pillarMat);

            // 4 Spawn Points
            GameObject spawnRoot = new GameObject("--- ENEMY SPAWN POINTS ---");
            Transform[] spawnPoints = new Transform[4];
            spawnPoints[0] = CreateSpawnPoint(spawnRoot.transform, "Spawn_NW", new Vector3(-12, 1, 12));
            spawnPoints[1] = CreateSpawnPoint(spawnRoot.transform, "Spawn_NE", new Vector3(12, 1, 12));
            spawnPoints[2] = CreateSpawnPoint(spawnRoot.transform, "Spawn_SW", new Vector3(-12, 1, -12));
            spawnPoints[3] = CreateSpawnPoint(spawnRoot.transform, "Spawn_SE", new Vector3(12, 1, -12));

            // Spawner System
            GameObject spawnerGo = new GameObject("EnemySpawner");
            EnemySpawner spawner = spawnerGo.AddComponent<EnemySpawner>();
            SetPrivateField(spawner, "spawnPoints", spawnPoints);

            // 3. Player Character
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player_Hero";
            player.tag = "Player";
            player.transform.position = new Vector3(0, 1f, 0);
            player.GetComponent<Renderer>().sharedMaterial = playerMat;

            // Setup CharacterController
            CharacterController cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = new Vector3(0, 0, 0);

            // Model holder & Visor
            GameObject visor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visor.name = "Visor";
            visor.transform.parent = player.transform;
            visor.transform.localPosition = new Vector3(0, 0.45f, 0.4f);
            visor.transform.localScale = new Vector3(0.6f, 0.25f, 0.35f);
            visor.GetComponent<Renderer>().sharedMaterial = visorMat;
            Object.DestroyImmediate(visor.GetComponent<Collider>());

            // Weapon Gun Barrel & Fire Point
            GameObject gun = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gun.name = "PlasmaBlaster";
            gun.transform.parent = player.transform;
            gun.transform.localPosition = new Vector3(0.45f, 0.1f, 0.5f);
            gun.transform.localRotation = Quaternion.Euler(90, 0, 0);
            gun.transform.localScale = new Vector3(0.18f, 0.45f, 0.18f);
            gun.GetComponent<Renderer>().sharedMaterial = neonGlowMat;
            Object.DestroyImmediate(gun.GetComponent<Collider>());

            GameObject firePointGo = new GameObject("FirePoint");
            firePointGo.transform.parent = player.transform;
            firePointGo.transform.localPosition = new Vector3(0.45f, 0.1f, 1.1f);

            // PlayerController
            PlayerController playerCtrl = player.AddComponent<PlayerController>();
            SetPrivateField(playerCtrl, "firePoint", firePointGo.transform);

            // 4. Camera with Top-Down Isometric Tracker
            GameObject camGo = CreateCamera(new Vector3(0, 16f, -11f), Quaternion.Euler(55, 0, 0), new Color(0.04f, 0.05f, 0.08f));
            ArenaCameraController camCtrl = camGo.AddComponent<ArenaCameraController>();
            SetPrivateField(camCtrl, "target", player.transform);

            CreateEventSystem();

            // 5. Arena UI HUD & Game Over Screen
            GameObject hudCanvasGo = CreateCanvas("ArenaHUDCanvas", 1920, 1080);
            Transform hudRoot = hudCanvasGo.transform;

            // --- Top Left Health Bar ---
            GameObject hpContainer = CreateUIObject("HP_Container", hudRoot);
            RectTransform hpContainerRect = hpContainer.GetComponent<RectTransform>();
            hpContainerRect.anchorMin = new Vector2(0, 1);
            hpContainerRect.anchorMax = new Vector2(0, 1);
            hpContainerRect.pivot = new Vector2(0, 1);
            hpContainerRect.anchoredPosition = new Vector2(40, -40);
            hpContainerRect.sizeDelta = new Vector2(300, 50);

            Slider hpSlider = CreateHealthBar(hpContainer.transform, new Vector2(280, 24));
            TextMeshProUGUI hpText = CreateTMPText("HP_Text", hpContainer.transform, "HP: 100 / 100", 16, FontStyles.Bold, Color.white);
            RectTransform hptr = hpText.GetComponent<RectTransform>();
            hptr.anchoredPosition = new Vector2(0, 18);
            hptr.sizeDelta = new Vector2(280, 25);

            // --- Top Center Wave Display ---
            TextMeshProUGUI waveText = CreateTMPText("WaveText", hudRoot, "WAVE: 1", 32, FontStyles.Bold, new Color(1f, 0.85f, 0.2f));
            RectTransform wtr = waveText.GetComponent<RectTransform>();
            wtr.anchorMin = new Vector2(0.5f, 1);
            wtr.anchorMax = new Vector2(0.5f, 1);
            wtr.pivot = new Vector2(0.5f, 1);
            wtr.anchoredPosition = new Vector2(0, -40);
            wtr.sizeDelta = new Vector2(300, 45);

            // --- Top Right Score Display (Disebelah kiri tombol Pause agar tidak tumpang tindih) ---
            TextMeshProUGUI scoreText = CreateTMPText("ScoreText", hudRoot, "SCORE: 0", 26, FontStyles.Bold, new Color(0f, 0.95f, 1f));
            RectTransform str = scoreText.GetComponent<RectTransform>();
            str.anchorMin = new Vector2(1, 1);
            str.anchorMax = new Vector2(1, 1);
            str.pivot = new Vector2(1, 1);
            str.anchoredPosition = new Vector2(-220, -35);
            str.sizeDelta = new Vector2(250, 45);
            scoreText.alignment = TextAlignmentOptions.Right;

            // --- Center Banner Notification ---
            TextMeshProUGUI notifText = CreateTMPText("WaveNotification", hudRoot, "WAVE 1 BERSIAP!", 44, FontStyles.Bold, new Color(0f, 1f, 0.7f));
            RectTransform ntr = notifText.GetComponent<RectTransform>();
            ntr.anchorMin = new Vector2(0.5f, 0.5f);
            ntr.anchorMax = new Vector2(0.5f, 0.5f);
            ntr.pivot = new Vector2(0.5f, 0.5f);
            ntr.anchoredPosition = new Vector2(0, 120);
            ntr.sizeDelta = new Vector2(900, 80);

            // --- Game Over Panel ---
            GameObject goPanel = CreateUIObject("GameOverPanel", hudRoot);
            StretchFull(goPanel.GetComponent<RectTransform>());
            Image goBg = goPanel.AddComponent<Image>();
            goBg.color = new Color(0.04f, 0.05f, 0.08f, 0.94f);

            GameObject goBox = CreateUIObject("GameOverBox", goPanel.transform);
            RectTransform goBoxRect = goBox.GetComponent<RectTransform>();
            goBoxRect.anchorMin = new Vector2(0.5f, 0.5f);
            goBoxRect.anchorMax = new Vector2(0.5f, 0.5f);
            goBoxRect.pivot = new Vector2(0.5f, 0.5f);
            goBoxRect.sizeDelta = new Vector2(620, 480);

            TextMeshProUGUI goTitle = CreateTMPText("GameOverTitle", goBox.transform, "HERO GUGUR!", 52, FontStyles.Bold, new Color(0.95f, 0.2f, 0.25f));
            RectTransform gotr = goTitle.GetComponent<RectTransform>();
            gotr.anchoredPosition = new Vector2(0, 160);
            gotr.sizeDelta = new Vector2(550, 60);

            TextMeshProUGUI finalScore = CreateTMPText("FinalScoreText", goBox.transform, "FINAL SCORE: 0", 26, FontStyles.Normal, Color.white);
            RectTransform fstr = finalScore.GetComponent<RectTransform>();
            fstr.anchoredPosition = new Vector2(0, 95);
            fstr.sizeDelta = new Vector2(500, 40);

            TextMeshProUGUI highScore = CreateTMPText("HighScoreText", goBox.transform, "HIGH SCORE: 0", 22, FontStyles.Bold, new Color(1f, 0.85f, 0.3f));
            RectTransform hstr = highScore.GetComponent<RectTransform>();
            hstr.anchoredPosition = new Vector2(0, 50);
            hstr.sizeDelta = new Vector2(500, 40);

            Button btnRetry = CreateButton("BtnRetry", goBox.transform, "COBA LAGI (RETRY)", new Vector2(0, -25), new Vector2(320, 52), new Color(0f, 0.75f, 0.9f), Color.white, 20);
            Button btnSelection = CreateButton("BtnBackSelect", goBox.transform, "PILIH GAME LAIN (SELECTION)", new Vector2(0, -90), new Vector2(320, 50), new Color(0.2f, 0.25f, 0.35f), Color.white, 18);
            Button btnMainMenu = CreateButton("BtnMainMenu", goBox.transform, "MENU UTAMA (MAIN MENU)", new Vector2(0, -155), new Vector2(320, 50), new Color(0.75f, 0.2f, 0.25f), Color.white, 18);

            goPanel.SetActive(false);

            // ArenaGameManager Setup
            GameObject arenaMgrGo = new GameObject("ArenaGameManager");
            ArenaGameManager arenaMgr = arenaMgrGo.AddComponent<ArenaGameManager>();
            SetPrivateField(arenaMgr, "healthSlider", hpSlider);
            SetPrivateField(arenaMgr, "healthText", hpText);
            SetPrivateField(arenaMgr, "scoreText", scoreText);
            SetPrivateField(arenaMgr, "waveText", waveText);
            SetPrivateField(arenaMgr, "waveNotificationText", notifText);
            SetPrivateField(arenaMgr, "gameOverPanel", goPanel);
            SetPrivateField(arenaMgr, "finalScoreText", finalScore);
            SetPrivateField(arenaMgr, "highScoreText", highScore);
            SetPrivateField(arenaMgr, "btnRetry", btnRetry);
            SetPrivateField(arenaMgr, "btnBackSelection", btnSelection);
            SetPrivateField(arenaMgr, "btnMainMenu", btnMainMenu);

            // 6. GameOverlayUI (Universal Pause / Menu)
            CreateOverlayCanvas();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MultiverseArcadeBuilder] Scene Game_Syafiq berhasil dibangun lengkap!");
        }
        #endregion

        #region 4. Teammate Demo Scenes
        private static void BuildTeammateScene(string scenePath, string gameTitle, string author, string genre, Color themeColor)
        {
            var scene = EditorSceneManager.OpenScene(scenePath);
            ClearScene();

            CreateCamera(new Vector3(0, 5, -10), Quaternion.Euler(25, 0, 0), new Color(0.05f, 0.06f, 0.1f));
            CreateDirectionalLight(new Vector3(50, -30, 0), Color.white, 1.0f);
            CreateEventSystem();
            CreateCoreManagers();

            // Ground
            Material floorMat = CreateURPMaterial($"M_Floor_{author}", themeColor * 0.3f, 0.2f);
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "DemoFloor";
            floor.transform.position = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(20, 1, 20);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // Center Pedestal
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.position = new Vector3(0, 0.5f, 0);
            pedestal.transform.localScale = new Vector3(3, 1f, 3);
            pedestal.GetComponent<Renderer>().sharedMaterial = CreateURPMaterial($"M_Ped_{author}", themeColor, 0.5f);

            // World Space Info Billboard
            GameObject canvasGo = CreateCanvas("DemoInfoCanvas", 800, 600);
            canvasGo.transform.position = new Vector3(0, 3.5f, 0);
            Canvas c = canvasGo.GetComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            RectTransform crt = canvasGo.GetComponent<RectTransform>();
            crt.sizeDelta = new Vector2(800, 600);
            crt.localScale = Vector3.one * 0.005f;

            TextMeshProUGUI titleTxt = CreateTMPText("Title", canvasGo.transform, gameTitle, 52, FontStyles.Bold, themeColor);
            RectTransform ttr = titleTxt.GetComponent<RectTransform>();
            ttr.anchoredPosition = new Vector2(0, 100);
            ttr.sizeDelta = new Vector2(750, 70);

            TextMeshProUGUI authorTxt = CreateTMPText("Author", canvasGo.transform, $"Dibuat oleh: {author}\nGenre: {genre}", 28, FontStyles.Italic, Color.white);
            RectTransform atr = authorTxt.GetComponent<RectTransform>();
            atr.anchoredPosition = new Vector2(0, 10);
            atr.sizeDelta = new Vector2(750, 80);

            TextMeshProUGUI statusTxt = CreateTMPText("Status", canvasGo.transform, "⏳ GAME SEDANG DALAM PENGEMBANGAN ⏳\nTekan ESC atau tombol PAUSE untuk kembali", 24, FontStyles.Bold, new Color(1f, 0.85f, 0.3f));
            RectTransform str = statusTxt.GetComponent<RectTransform>();
            str.anchoredPosition = new Vector2(0, -90);
            str.sizeDelta = new Vector2(750, 80);

            // Universal Pause Overlay
            CreateOverlayCanvas();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[MultiverseArcadeBuilder] Scene {author} ({gameTitle}) berhasil dibangun!");
        }
        #endregion

        #region UI & Prefab Helpers
        private static void CreateOverlayCanvas()
        {
            GameObject canvasGo = CreateCanvas("GameOverlayCanvas", 1920, 1080);
            canvasGo.GetComponent<Canvas>().sortingOrder = 100;
            Transform root = canvasGo.transform;

            // Pause Button (Top Right)
            Button btnPause = CreateButton("BtnPause", root, "PAUSE [ESC]", new Vector2(-35, -35), new Vector2(160, 46), new Color(0.18f, 0.22f, 0.32f, 0.95f), Color.white, 16);
            RectTransform prt = btnPause.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(1, 1);
            prt.anchorMax = new Vector2(1, 1);
            prt.pivot = new Vector2(1, 1);

            // Pause Panel
            GameObject pausePanel = CreateUIObject("PausePanel", root);
            StretchFull(pausePanel.GetComponent<RectTransform>());
            Image pbg = pausePanel.AddComponent<Image>();
            pbg.color = new Color(0.04f, 0.05f, 0.08f, 0.88f);

            GameObject box = CreateUIObject("PauseMenuBox", pausePanel.transform);
            RectTransform boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.pivot = new Vector2(0.5f, 0.5f);
            boxRect.sizeDelta = new Vector2(500, 460);

            TextMeshProUGUI pTitle = CreateTMPText("PauseTitle", box.transform, "GAME PAUSED", 48, FontStyles.Bold, new Color(0f, 0.9f, 1f));
            RectTransform ptr = pTitle.GetComponent<RectTransform>();
            ptr.anchoredPosition = new Vector2(0, 150);
            ptr.sizeDelta = new Vector2(450, 60);

            Button btnResume  = CreateButton("BtnResume", box.transform, "LANJUTKAN (RESUME)", new Vector2(0, 70), new Vector2(340, 52), new Color(0f, 0.75f, 0.85f), Color.white, 18);
            Button btnRestart = CreateButton("BtnRestart", box.transform, "ULANGI (RESTART)", new Vector2(0, 5), new Vector2(340, 52), new Color(0.3f, 0.35f, 0.45f), Color.white, 18);
            Button btnSwitch  = CreateButton("BtnSwitchGame", box.transform, "GANTI GAME (SELECTION)", new Vector2(0, -60), new Vector2(340, 52), new Color(0.3f, 0.35f, 0.45f), Color.white, 18);
            Button btnMain    = CreateButton("BtnMainMenu", box.transform, "MENU UTAMA", new Vector2(0, -125), new Vector2(340, 52), new Color(0.8f, 0.22f, 0.28f), Color.white, 18);

            pausePanel.SetActive(false);

            GameOverlayUI overlayUI = canvasGo.AddComponent<GameOverlayUI>();
            SetPrivateField(overlayUI, "pausePanel", pausePanel);
            SetPrivateField(overlayUI, "pauseButton", btnPause);
            SetPrivateField(overlayUI, "resumeButton", btnResume);
            SetPrivateField(overlayUI, "restartButton", btnRestart);
            SetPrivateField(overlayUI, "switchGameButton", btnSwitch);
            SetPrivateField(overlayUI, "mainMenuButton", btnMain);
        }

        private static Slider CreateHealthBar(Transform parent, Vector2 size)
        {
            GameObject sliderGo = CreateUIObject("HealthSlider", parent);
            RectTransform sr = sliderGo.GetComponent<RectTransform>();
            sr.sizeDelta = size;
            sr.anchoredPosition = new Vector2(0, -10);

            Slider slider = sliderGo.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 100;

            // Background
            GameObject bg = CreateUIObject("Background", sliderGo.transform);
            StretchFull(bg.GetComponent<RectTransform>());
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

            // Fill Area
            GameObject fillArea = CreateUIObject("Fill Area", sliderGo.transform);
            RectTransform far = fillArea.GetComponent<RectTransform>();
            far.anchorMin = new Vector2(0, 0);
            far.anchorMax = new Vector2(1, 1);
            far.offsetMin = new Vector2(4, 4);
            far.offsetMax = new Vector2(-4, -4);

            GameObject fill = CreateUIObject("Fill", fillArea.transform);
            RectTransform fr = fill.GetComponent<RectTransform>();
            fr.anchorMin = new Vector2(0, 0);
            fr.anchorMax = new Vector2(1, 1);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.15f, 0.85f, 0.4f, 1f);

            slider.fillRect = fr;
            slider.targetGraphic = bgImg;

            return slider;
        }

        private static Button CreateButton(string name, Transform parent, string text, Vector2 anchoredPos, Vector2 size, Color bgColor, Color textColor, int fontSize)
        {
            GameObject btnGo = CreateUIObject(name, parent);
            RectTransform rect = btnGo.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            Image img = btnGo.AddComponent<Image>();
            img.color = bgColor;

            Button btn = btnGo.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = Color.Lerp(bgColor, Color.white, 0.25f);
            cb.pressedColor = Color.Lerp(bgColor, Color.black, 0.25f);
            btn.colors = cb;

            TextMeshProUGUI label = CreateTMPText("Text", btnGo.transform, text, fontSize, FontStyles.Bold, textColor);
            StretchFull(label.GetComponent<RectTransform>());

            return btn;
        }

        private static TextMeshProUGUI CreateTMPText(string name, Transform parent, string text, int fontSize, FontStyles style, Color color)
        {
            GameObject go = CreateUIObject(name, parent);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }

        private static GameObject CreateCanvas(string name, int width, int height)
        {
            GameObject canvasGo = new GameObject(name);
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<GraphicRaycaster>();

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(width, height);
            scaler.matchWidthOrHeight = 0.5f;

            return canvasGo;
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static GameObject CreateCamera(Vector3 position, Quaternion rotation, Color clearColor)
        {
            GameObject camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = clearColor;
            camGo.AddComponent<AudioListener>();
            camGo.transform.position = position;
            camGo.transform.rotation = rotation;
            return camGo;
        }

        private static void CreateDirectionalLight(Vector3 rotation, Color color, float intensity)
        {
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            lightGo.transform.rotation = Quaternion.Euler(rotation);
        }

        private static void CreateEventSystem()
        {
            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
        }

        private static void CreateCoreManagers()
        {
            if (Object.FindObjectOfType<GameManager>() == null)
            {
                GameObject gmGo = new GameObject("GameManager");
                gmGo.AddComponent<GameManager>();
            }

            if (Object.FindObjectOfType<SceneLoader>() == null)
            {
                GameObject slGo = new GameObject("SceneLoader");
                slGo.AddComponent<SceneLoader>();
            }
        }

        private static void CreateWall(Transform parent, string name, Vector3 pos, Vector3 scale, Material wallMat, Material glowMat)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.parent = parent;
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = wallMat;

            // Trim neon
            GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trim.name = "NeonTrim";
            trim.transform.parent = wall.transform;
            trim.transform.localPosition = new Vector3(0, 0.45f, 0);
            trim.transform.localScale = new Vector3(1.02f, 0.1f, 1.05f);
            trim.GetComponent<Renderer>().sharedMaterial = glowMat;
            Object.DestroyImmediate(trim.GetComponent<Collider>());
        }

        private static void CreatePillar(Transform parent, string name, Vector3 pos, Material mat)
        {
            GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = name;
            pillar.transform.parent = parent;
            pillar.transform.position = pos;
            pillar.transform.localScale = new Vector3(2f, 2f, 2f);
            pillar.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static Transform CreateSpawnPoint(Transform parent, string name, Vector3 pos)
        {
            GameObject sp = new GameObject(name);
            sp.transform.parent = parent;
            sp.transform.position = pos;
            return sp.transform;
        }

        private static Material CreateURPMaterial(string name, Color color, float smoothness = 0.5f)
        {
            string matPath = $"Assets/Games/Syafiq/Materials/{name}.mat";
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
            GameObject[] all = Object.FindObjectsOfType<GameObject>();
            foreach (var go in all)
            {
                // Jangan hapus DontDestroyOnLoad atau editor objects
                if (go.hideFlags == HideFlags.None && go.scene.isLoaded)
                {
                    Object.DestroyImmediate(go);
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
                Debug.LogWarning($"[MultiverseArcadeBuilder] Field '{fieldName}' tidak ditemukan pada {target.GetType().Name}");
            }
        }
        #endregion
    }
}
#endif
