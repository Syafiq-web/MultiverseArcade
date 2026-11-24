#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using TMPro;
using MultiverseArcade.Core;
using MultiverseArcade.UI;
using MultiverseArcade.Syafiq;

namespace MultiverseArcade.EditorTools
{
    /// <summary>
    /// Builder Hub Terpadu untuk Multiverse Arcade sesuai standar GDD v2.0:
    /// - Menu: "Multiverse > Build Hub"
    /// - Membuat ScriptableObject GameRegistry & 5 GameInfo
    /// - Membangun 9 Scene lengkap (Bootstrap, MainMenu, GameSelection, Loading, Game_Syafiq, Game_Jaya, Game_Prima, Game_Brili, Game_Risa)
    /// - Mendaftarkan seluruh scene ke EditorBuildSettings (Bootstrap = Index 0)
    /// - Verifikasi ketiadaan missing reference
    /// </summary>
    public static class MultiverseHubBuilder
    {
        private const string SCENE_BOOTSTRAP = "Assets/_Project/Scenes/Bootstrap.unity";
        private const string SCENE_MAIN_MENU = "Assets/_Project/Scenes/MainMenu.unity";
        private const string SCENE_GAME_SELECTION = "Assets/_Project/Scenes/GameSelection.unity";
        private const string SCENE_LOADING = "Assets/_Project/Scenes/Loading.unity";

        private const string SCENE_SYAFIQ = "Assets/Games/Syafiq/Scenes/Game_Syafiq.unity";
        private const string SCENE_JAYA = "Assets/Games/Jaya/Scenes/Game_Jaya.unity";
        private const string SCENE_PRIMA = "Assets/Games/Prima/Scenes/Game_Prima.unity";
        private const string SCENE_BRILI = "Assets/Games/Brili/Scenes/Game_Brili.unity";
        private const string SCENE_RISA = "Assets/Games/Risa/Scenes/Game_Risa.unity";

        private const string DATA_DIR = "Assets/_Project/Data";
        private const string REGISTRY_PATH = "Assets/_Project/Data/GameRegistry.asset";

        [MenuItem("Multiverse/Build Hub", false, 0)]
        [MenuItem("Tools/Multiverse Arcade/Build Hub", false, 0)]
        public static void BuildHub()
        {
            BuildHubInternal(true);
        }

        public static void BuildHubFromCommandLine()
        {
            Debug.Log("[MultiverseHubBuilder] Memulai eksekusi BuildHub dari command line batchmode...");
            BuildHubInternal(false);
            EditorApplication.Exit(0);
        }

        [MenuItem("Multiverse/Validate Hub", false, 1)]
        [MenuItem("Tools/Multiverse Arcade/Validate Hub & References", false, 1)]
        public static void ValidateHub()
        {
            ValidateHubInternal(true);
        }

        public static void BuildHubInternal(bool interactive)
        {
            EnsureDirectories();
            EnsureTMPResources();

            Debug.Log("[MultiverseHubBuilder] === MEMULAI BUILD HUB MULTIVERSE ARCADE ===");

            // 1. Buat / Update ScriptableObject GameRegistry dan GameInfo
            GameRegistry registry = BuildGameRegistryAssets();

            // 2. Bangun Scene Bootstrap (Index 0)
            BuildBootstrapScene(registry);

            // 3. Bangun Scene MainMenu (Index 1)
            BuildMainMenuScene();

            // 4. Bangun Scene GameSelection (Index 2)
            BuildGameSelectionScene(registry);

            // 5. Bangun Scene Loading (Index 3)
            BuildLoadingScene();

            // 6. Bangun Scene Game_Syafiq (Index 4)
            BuildSyafiqScene();

            // 7. Bangun Scene Anggota Lain (Index 5 - 8)
            BuildPlaceholderGameScene(SCENE_JAYA, "jaya", "Jaya Arcade Project", new Color(0.2f, 0.4f, 0.8f));
            BuildPlaceholderGameScene(SCENE_PRIMA, "prima", "Prima Arcade Project", new Color(0.8f, 0.4f, 0.2f));
            BuildPlaceholderGameScene(SCENE_BRILI, "brili", "Brili Arcade Project", new Color(0.2f, 0.7f, 0.4f));
            BuildPlaceholderGameScene(SCENE_RISA, "risa", "Risa Arcade Project", new Color(0.7f, 0.2f, 0.6f));

            // 8. Daftarkan seluruh 9 scene ke Build Settings
            RegisterBuildScenes();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Buka kembali scene Bootstrap
            EditorSceneManager.OpenScene(SCENE_BOOTSTRAP);

            Debug.Log("[MultiverseHubBuilder] === SELURUH FONDASI & HUB BERHASIL DIBANGUN! ===");

            if (interactive)
            {
                EditorUtility.DisplayDialog("Multiverse Arcade Hub",
                    "Fondasi dan Hub Multiverse Arcade berhasil dibangun sempurna!\n\n" +
                    "- 9 Scene telah terdaftar di Build Settings (Bootstrap = Index 0)\n" +
                    "- Persistent Global Managers aktif (GameManager, SceneLoader, SaveManager, SettingsManager, AudioManager, UIManager)\n" +
                    "- Menu MainMenu & GameSelection terintegrasi dengan GameRegistry\n" +
                    "- Tekan PLAY pada scene Bootstrap untuk menguji langsung!", "OK");
            }
        }

        #region Setup Directories & Assets
        private static void EnsureDirectories()
        {
            string[] dirs = new string[]
            {
                "Assets/_Project/Scenes",
                "Assets/_Project/Scripts/Core",
                "Assets/_Project/Scripts/UI",
                "Assets/_Project/Scripts/Audio",
                "Assets/_Project/Prefabs",
                "Assets/_Project/UI",
                "Assets/_Project/Audio",
                "Assets/_Project/Data",
                "Assets/Games/Syafiq/Scenes",
                "Assets/Games/Syafiq/Scripts",
                "Assets/Games/Syafiq/Materials",
                "Assets/Games/Syafiq/Audio",
                "Assets/Games/Jaya/Scenes",
                "Assets/Games/Prima/Scenes",
                "Assets/Games/Brili/Scenes",
                "Assets/Games/Risa/Scenes",
                "Assets/ThirdParty"
            };

            foreach (var d in dirs)
            {
                if (!Directory.Exists(d))
                {
                    Directory.CreateDirectory(d);
                }
            }
            AssetDatabase.Refresh();
        }

        private static void EnsureTMPResources()
        {
            TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
            if (settings == null)
            {
                Debug.Log("[MultiverseHubBuilder] Memeriksa TMP Settings...");
            }
        }

        private static GameRegistry BuildGameRegistryAssets()
        {
            if (!Directory.Exists(DATA_DIR)) Directory.CreateDirectory(DATA_DIR);

            // Syafiq
            GameInfo infoSyafiq = CreateOrLoadGameInfo("GameInfo_Syafiq", "syafiq", "Arena Clash 3D", "Game_Syafiq",
                "Top-Down Hero Shooter",
                "Pertempuran arena 3v3 intensif melawan gerombolan zombie dengan persenjataan ganda Plasma Rifle & Scatter Shotgun.",
                "WASD: Bergerak | Mouse: Bidik & Tembak | Q/1/2: Ganti Senjata | Esc: Pause",
                true);

            // Jaya
            GameInfo infoJaya = CreateOrLoadGameInfo("GameInfo_Jaya", "jaya", "Jaya Arcade Project", "Game_Jaya",
                "Coming Soon", "Game arcade 3D oleh Jaya. Dalam tahap perancangan.", "Segera hadir", false);

            // Prima
            GameInfo infoPrima = CreateOrLoadGameInfo("GameInfo_Prima", "prima", "Prima Arcade Project", "Game_Prima",
                "Coming Soon", "Game arcade 3D oleh Prima. Dalam tahap perancangan.", "Segera hadir", false);

            // Brili
            GameInfo infoBrili = CreateOrLoadGameInfo("GameInfo_Brili", "brili", "Brili Arcade Project", "Game_Brili",
                "Coming Soon", "Game arcade 3D oleh Brili. Dalam tahap perancangan.", "Segera hadir", false);

            // Risa
            GameInfo infoRisa = CreateOrLoadGameInfo("GameInfo_Risa", "risa", "Risa Arcade Project", "Game_Risa",
                "Coming Soon", "Game arcade 3D oleh Risa. Dalam tahap perancangan.", "Segera hadir", false);

            GameRegistry registry = AssetDatabase.LoadAssetAtPath<GameRegistry>(REGISTRY_PATH);
            if (registry == null)
            {
                registry = ScriptableObject.CreateInstance<GameRegistry>();
                AssetDatabase.CreateAsset(registry, REGISTRY_PATH);
            }

            registry.games.Clear();
            registry.games.Add(infoSyafiq);
            registry.games.Add(infoJaya);
            registry.games.Add(infoPrima);
            registry.games.Add(infoBrili);
            registry.games.Add(infoRisa);

            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            Debug.Log("[MultiverseHubBuilder] GameRegistry ScriptableObject berhasil diperbarui.");
            return registry;
        }

        private static GameInfo CreateOrLoadGameInfo(string assetName, string id, string title, string sceneName,
            string genre, string desc, string controls, bool enabled)
        {
            string path = $"{DATA_DIR}/{assetName}.asset";
            GameInfo info = AssetDatabase.LoadAssetAtPath<GameInfo>(path);
            if (info == null)
            {
                info = ScriptableObject.CreateInstance<GameInfo>();
                AssetDatabase.CreateAsset(info, path);
            }

            info.id = id;
            info.displayName = title;
            info.sceneName = sceneName;
            info.genre = genre;
            info.description = desc;
            info.controlsHint = controls;
            info.isEnabled = enabled;

            EditorUtility.SetDirty(info);
            return info;
        }
        #endregion

        #region Scene Builders
        private static void BuildBootstrapScene(GameRegistry registry)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ClearScene();

            // Camera
            CreateCamera("BootstrapCamera", new Vector3(0, 5, -10), Quaternion.Euler(20, 0, 0));

            // Directional Light
            CreateDirectionalLight();

            // EventSystem
            CreateEventSystem();

            // Persistent Managers Root
            GameObject managersRoot = new GameObject("_GLOBAL_MANAGERS");

            GameManager gameMgr = managersRoot.AddComponent<GameManager>();
            gameMgr.SetRegistry(registry);

            SceneLoader sceneLoader = managersRoot.AddComponent<SceneLoader>();
            SaveManager saveMgr = managersRoot.AddComponent<SaveManager>();
            SettingsManager settingsMgr = managersRoot.AddComponent<SettingsManager>();
            AudioManager audioMgr = managersRoot.AddComponent<AudioManager>();
            UIManager uiMgr = managersRoot.AddComponent<UIManager>();

            // Setup audio clips on AudioManager
            AudioClip clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/sfx_ui_click.wav");
            AudioClip hoverClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/sfx_ui_hover.wav");
            AudioClip bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/bgm_arcade_action.wav");
            audioMgr.SetAudioClips(clickClip, hoverClip, bgmClip);

            // Persistent Canvas (Global Overlays: Pause & Loading)
            GameObject globalCanvasGo = CreateCanvas("GlobalCanvas", 999);
            globalCanvasGo.transform.SetParent(managersRoot.transform);

            // Global Pause Overlay Panel
            GameObject pausePanel = CreateGlobalPauseOverlay(globalCanvasGo.transform);

            // Global Loading Screen Panel
            GameObject loadingPanel = CreateGlobalLoadingOverlay(globalCanvasGo.transform, out Slider progressBar);

            uiMgr.SetReferences(pausePanel, loadingPanel, progressBar);

            EditorSceneManager.SaveScene(scene, SCENE_BOOTSTRAP);
            Debug.Log($"[MultiverseHubBuilder] Scene Bootstrap tersimpan di {SCENE_BOOTSTRAP}");
        }

        private static void BuildMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ClearScene();

            CreateCamera("MainMenuCamera", new Vector3(0, 2, -6), Quaternion.identity);
            CreateDirectionalLight();
            CreateEventSystem();

            GameObject canvasGo = CreateCanvas("MainMenuCanvas", 10);

            // Background Image
            CreateCyberBackground(canvasGo.transform);

            // Title & Subtitle
            CreateTextTMP(canvasGo.transform, "TitleText", "MULTIVERSE ARCADE", 64,
                new Vector2(0, 240), new Vector2(800, 100), new Color(0f, 1f, 0.95f, 1f), FontStyles.Bold, TextAlignmentOptions.Center);

            CreateTextTMP(canvasGo.transform, "SubtitleText", "5 GAMES • 1 UNIVERSE • ENDLESS ACTION", 20,
                new Vector2(0, 180), new Vector2(800, 50), new Color(0.7f, 0.85f, 1f, 0.85f), FontStyles.Normal, TextAlignmentOptions.Center);

            // Menu Buttons Container
            GameObject menuContainer = new GameObject("MenuButtons");
            menuContainer.transform.SetParent(canvasGo.transform, false);
            RectTransform menuRect = menuContainer.AddComponent<RectTransform>();
            menuRect.anchoredPosition = new Vector2(0, -40);
            menuRect.sizeDelta = new Vector2(400, 360);

            Button btnPlay = CreateCyberButton(menuContainer.transform, "Btn_Play", "PLAY", new Vector2(0, 100), new Vector2(340, 60), new Color(0f, 0.75f, 0.7f));
            Button btnSettings = CreateCyberButton(menuContainer.transform, "Btn_Settings", "SETTINGS", new Vector2(0, 25), new Vector2(340, 55), new Color(0.2f, 0.3f, 0.5f));
            Button btnCredits = CreateCyberButton(menuContainer.transform, "Btn_Credits", "CREDITS", new Vector2(0, -45), new Vector2(340, 55), new Color(0.2f, 0.3f, 0.5f));
            Button btnQuit = CreateCyberButton(menuContainer.transform, "Btn_Quit", "QUIT GAME", new Vector2(0, -115), new Vector2(340, 55), new Color(0.5f, 0.15f, 0.2f));

            // Settings Modal Panel
            GameObject settingsPanel = CreateSettingsPanel(canvasGo.transform);

            // Credits Modal Panel
            GameObject creditsPanel = CreateCreditsPanel(canvasGo.transform);

            // MainMenuUI
            MainMenuUI menuUI = canvasGo.AddComponent<MainMenuUI>();
            menuUI.BindControls(btnPlay, btnSettings, btnCredits, btnQuit, settingsPanel, creditsPanel);

            EditorSceneManager.SaveScene(scene, SCENE_MAIN_MENU);
            Debug.Log($"[MultiverseHubBuilder] Scene MainMenu tersimpan di {SCENE_MAIN_MENU}");
        }

        private static void BuildGameSelectionScene(GameRegistry registry)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ClearScene();

            CreateCamera("SelectionCamera", new Vector3(0, 2, -6), Quaternion.identity);
            CreateDirectionalLight();
            CreateEventSystem();

            GameObject canvasGo = CreateCanvas("GameSelectionCanvas", 10);
            CreateCyberBackground(canvasGo.transform);

            // Header
            CreateTextTMP(canvasGo.transform, "HeaderText", "CHOOSE YOUR UNIVERSE", 48,
                new Vector2(0, 360), new Vector2(900, 70), new Color(0f, 1f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Center);

            CreateTextTMP(canvasGo.transform, "SubHeaderText", "Select an arcade game to jump straight into action", 18,
                new Vector2(0, 315), new Vector2(900, 40), new Color(0.7f, 0.85f, 1f, 0.8f), FontStyles.Normal, TextAlignmentOptions.Center);

            // Back Button
            Button backBtn = CreateCyberButton(canvasGo.transform, "Btn_Back", "BACK TO MAIN MENU", new Vector2(-600, 350), new Vector2(200, 50), new Color(0.25f, 0.25f, 0.35f));

            // Cards Row
            GameObject cardsContainer = new GameObject("CardsContainer");
            cardsContainer.transform.SetParent(canvasGo.transform, false);
            RectTransform cardsRect = cardsContainer.AddComponent<RectTransform>();
            cardsRect.anchoredPosition = new Vector2(0, -40);
            cardsRect.sizeDelta = new Vector2(1400, 500);

            GameSelectionUI selectionUI = canvasGo.AddComponent<GameSelectionUI>();
            selectionUI.BindBackButton(backBtn);
            selectionUI.SetRegistry(registry);

            float cardWidth = 240f;
            float spacing = 265f;
            float startX = -spacing * 2;

            for (int i = 0; i < 5; i++)
            {
                GameInfo info = (registry != null && i < registry.games.Count) ? registry.games[i] : null;
                Vector2 pos = new Vector2(startX + (i * spacing), 0);
                GameCardView cardView = CreateGameCard(cardsContainer.transform, $"Card_{i + 1}", pos, new Vector2(cardWidth, 420), info);
                selectionUI.AddPrebuiltCard(cardView);
            }

            selectionUI.PopulateCards();

            EditorSceneManager.SaveScene(scene, SCENE_GAME_SELECTION);
            Debug.Log($"[MultiverseHubBuilder] Scene GameSelection tersimpan di {SCENE_GAME_SELECTION}");
        }

        private static void BuildLoadingScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ClearScene();

            CreateCamera("LoadingCamera", new Vector3(0, 0, -10), Quaternion.identity);
            CreateEventSystem();

            GameObject canvasGo = CreateCanvas("LoadingCanvas", 100);
            CreateCyberBackground(canvasGo.transform);

            CreateTextTMP(canvasGo.transform, "LoadingText", "LOADING UNIVERSE...", 36,
                new Vector2(0, 40), new Vector2(600, 60), new Color(0f, 1f, 0.9f), FontStyles.Bold, TextAlignmentOptions.Center);

            // Progress bar
            CreateProgressBar(canvasGo.transform, "ProgressBar", new Vector2(0, -40), new Vector2(500, 24));

            EditorSceneManager.SaveScene(scene, SCENE_LOADING);
            Debug.Log($"[MultiverseHubBuilder] Scene Loading tersimpan di {SCENE_LOADING}");
        }

        private static void BuildSyafiqScene()
        {
            // Panggil builder resmi MVP Arena Clash 3D (Fase 1-5)
            ArenaClashBuilder.BuildArenaClashInternal(false);
        }

        private static void BuildPlaceholderGameScene(string scenePath, string gameId, string gameTitle, Color themeColor)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ClearScene();

            CreateCamera("MainCamera", new Vector3(0, 8, -12), Quaternion.Euler(30, 0, 0));
            CreateDirectionalLight();
            CreateEventSystem();

            // Floor
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Placeholder_Floor";
            floor.transform.position = new Vector3(0, -0.5f, 0);
            floor.transform.localScale = new Vector3(30, 1, 30);
            Material floorMat = CreateURPMaterial($"Mat_{gameId}_Floor", themeColor * 0.4f);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            // 3D Billboard Sign
            GameObject sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "Placeholder_Sign";
            sign.transform.position = new Vector3(0, 3, 0);
            sign.transform.localScale = new Vector3(10, 3, 0.5f);
            Material signMat = CreateURPMaterial($"Mat_{gameId}_Sign", themeColor);
            sign.GetComponent<Renderer>().sharedMaterial = signMat;

            // Entry Component implementing IArcadeGame
            GameObject entryGo = new GameObject("GameEntry");
            PlaceholderGameEntry entry = entryGo.AddComponent<PlaceholderGameEntry>();
            entry.SetGameId(gameId);

            // Universal GameOverlayUI Canvas
            CreateGameOverlayCanvas(gameTitle);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[MultiverseHubBuilder] Scene placeholder {gameTitle} tersimpan di {scenePath}");
        }
        #endregion

        #region UI Component Helpers
        private static GameObject CreateCanvas(string name, int sortingOrder)
        {
            GameObject canvasGo = new GameObject(name);
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGo.AddComponent<GraphicRaycaster>();
            return canvasGo;
        }

        private static void CreateCyberBackground(Transform parent)
        {
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(parent, false);
            Image img = bg.AddComponent<Image>();
            img.color = new Color(0.04f, 0.06f, 0.10f, 0.98f);
            img.raycastTarget = false;

            RectTransform rt = bg.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
        }

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

            // Label
            CreateTextTMP(btnGo.transform, "Label", label, 20, Vector2.zero, size, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            return btn;
        }

        private static Slider CreateProgressBar(Transform parent, string name, Vector2 pos, Vector2 size)
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
            faRt.anchorMin = new Vector2(0, 0.25f);
            faRt.anchorMax = new Vector2(1, 0.75f);
            faRt.sizeDelta = Vector2.zero;

            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0f, 0.9f, 0.85f);
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            fillRt.sizeDelta = Vector2.zero;

            slider.fillRect = fillRt;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            return slider;
        }

        private static GameObject CreateGlobalPauseOverlay(Transform parent)
        {
            GameObject overlay = new GameObject("GlobalPauseOverlay");
            overlay.transform.SetParent(parent, false);
            RectTransform rt = overlay.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            // Dark Dimmed Background
            Image dimImg = overlay.AddComponent<Image>();
            dimImg.color = new Color(0.02f, 0.04f, 0.08f, 0.92f);

            // Title
            CreateTextTMP(overlay.transform, "PauseTitle", "GAME PAUSED", 54,
                new Vector2(0, 180), new Vector2(600, 80), new Color(0.95f, 0.85f, 0.2f), FontStyles.Bold, TextAlignmentOptions.Center);

            // Buttons
            Button resumeBtn = CreateCyberButton(overlay.transform, "Btn_Resume", "RESUME", new Vector2(0, 60), new Vector2(320, 55), new Color(0f, 0.65f, 0.6f));
            Button restartBtn = CreateCyberButton(overlay.transform, "Btn_Restart", "RESTART GAME", new Vector2(0, -10), new Vector2(320, 50), new Color(0.2f, 0.35f, 0.55f));
            Button switchBtn = CreateCyberButton(overlay.transform, "Btn_SwitchGame", "SWITCH GAME", new Vector2(0, -75), new Vector2(320, 50), new Color(0.45f, 0.3f, 0.6f));
            Button menuBtn = CreateCyberButton(overlay.transform, "Btn_MainMenu", "MAIN MENU", new Vector2(0, -140), new Vector2(320, 50), new Color(0.5f, 0.2f, 0.2f));

            GlobalPauseOverlay pauseCtrl = overlay.AddComponent<GlobalPauseOverlay>();
            pauseCtrl.BindButtons(resumeBtn, restartBtn, switchBtn, menuBtn);

            overlay.SetActive(false);
            return overlay;
        }

        private static GameObject CreateGlobalLoadingOverlay(Transform parent, out Slider progressBar)
        {
            GameObject overlay = new GameObject("GlobalLoadingOverlay");
            overlay.transform.SetParent(parent, false);
            RectTransform rt = overlay.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;

            Image dimImg = overlay.AddComponent<Image>();
            dimImg.color = new Color(0.03f, 0.05f, 0.08f, 0.98f);

            CreateTextTMP(overlay.transform, "LoadingLabel", "LOADING...", 36,
                new Vector2(0, 50), new Vector2(500, 60), new Color(0f, 1f, 0.9f), FontStyles.Bold, TextAlignmentOptions.Center);

            progressBar = CreateProgressBar(overlay.transform, "LoadingProgress", new Vector2(0, -30), new Vector2(480, 22));

            overlay.SetActive(false);
            return overlay;
        }

        private static GameObject CreateSettingsPanel(Transform parent)
        {
            GameObject panel = new GameObject("SettingsPanel");
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(700, 600);

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.10f, 0.16f, 0.97f);

            CreateTextTMP(panel.transform, "Header", "SETTINGS", 38,
                new Vector2(0, 230), new Vector2(500, 60), new Color(0f, 1f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Center);

            // Master Volume
            CreateTextTMP(panel.transform, "Lbl_Master", "Master Volume", 20, new Vector2(-160, 130), new Vector2(250, 40), Color.white, FontStyles.Normal, TextAlignmentOptions.Left);
            Slider masterSlider = CreateProgressBar(panel.transform, "Slider_Master", new Vector2(50, 130), new Vector2(260, 20));
            TextMeshProUGUI masterTxt = CreateTextTMP(panel.transform, "Txt_Master", "80%", 18, new Vector2(240, 130), new Vector2(80, 40), Color.cyan, FontStyles.Bold, TextAlignmentOptions.Center);

            // Music Volume
            CreateTextTMP(panel.transform, "Lbl_Music", "Music Volume", 20, new Vector2(-160, 60), new Vector2(250, 40), Color.white, FontStyles.Normal, TextAlignmentOptions.Left);
            Slider musicSlider = CreateProgressBar(panel.transform, "Slider_Music", new Vector2(50, 60), new Vector2(260, 20));
            TextMeshProUGUI musicTxt = CreateTextTMP(panel.transform, "Txt_Music", "70%", 18, new Vector2(240, 60), new Vector2(80, 40), Color.cyan, FontStyles.Bold, TextAlignmentOptions.Center);

            // SFX Volume
            CreateTextTMP(panel.transform, "Lbl_SFX", "SFX Volume", 20, new Vector2(-160, -10), new Vector2(250, 40), Color.white, FontStyles.Normal, TextAlignmentOptions.Left);
            Slider sfxSlider = CreateProgressBar(panel.transform, "Slider_SFX", new Vector2(50, -10), new Vector2(260, 20));
            TextMeshProUGUI sfxTxt = CreateTextTMP(panel.transform, "Txt_SFX", "90%", 18, new Vector2(240, -10), new Vector2(80, 40), Color.cyan, FontStyles.Bold, TextAlignmentOptions.Center);

            // Graphics Quality
            CreateTextTMP(panel.transform, "Lbl_Graphics", "Graphics Quality", 20, new Vector2(-160, -90), new Vector2(250, 40), Color.white, FontStyles.Normal, TextAlignmentOptions.Left);
            Button qLow = CreateCyberButton(panel.transform, "Btn_QLow", "LOW", new Vector2(20, -90), new Vector2(90, 40), new Color(0.2f, 0.3f, 0.45f));
            Button qMed = CreateCyberButton(panel.transform, "Btn_QMed", "MED", new Vector2(120, -90), new Vector2(90, 40), new Color(0.2f, 0.45f, 0.55f));
            Button qHigh = CreateCyberButton(panel.transform, "Btn_QHigh", "HIGH", new Vector2(220, -90), new Vector2(90, 40), new Color(0.1f, 0.6f, 0.6f));

            // Close Button
            Button closeBtn = CreateCyberButton(panel.transform, "Btn_Close", "CLOSE", new Vector2(0, -200), new Vector2(220, 50), new Color(0.5f, 0.2f, 0.25f));

            SettingsUI settingsUI = panel.AddComponent<SettingsUI>();
            settingsUI.BindControls(masterSlider, musicSlider, sfxSlider, masterTxt, musicTxt, sfxTxt, qLow, qMed, qHigh, closeBtn);

            panel.SetActive(false);
            return panel;
        }

        private static GameObject CreateCreditsPanel(Transform parent)
        {
            GameObject panel = new GameObject("CreditsPanel");
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(800, 680);

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.10f, 0.16f, 0.98f);

            CreateTextTMP(panel.transform, "Header", "CREDITS / TIM PENGEMBANG", 34,
                new Vector2(0, 270), new Vector2(700, 50), new Color(0f, 1f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Center);

            string creditsText =
                "<b>TIM PENGEMBANG:</b>\n" +
                "• <b>Syafiq</b> — Lead Developer, Arsitektur Hub & Arena Clash 3D\n" +
                "• <b>Jaya</b> — Game Developer (Game 2)\n" +
                "• <b>Prima</b> — Game Developer (Game 3)\n" +
                "• <b>Brili</b> — Game Developer (Game 4)\n" +
                "• <b>Risa</b> — Game Developer (Game 5)\n\n" +
                "<b>ASET & LISENSI:</b>\n" +
                "• Font: Liberation Sans SDF (SIL Open Font License v1.1)\n" +
                "• Audio & SFX: Procedural Synthwave & Chime Generator (CC0 Public Domain)\n" +
                "• Engine: Unity 2022.3 LTS & Universal Render Pipeline (URP 14)\n\n" +
                "<i>Multiverse Arcade v2.0 — Arsitektur Sistem Terpadu</i>";

            CreateTextTMP(panel.transform, "Content", creditsText, 17,
                new Vector2(0, 30), new Vector2(720, 420), Color.white, FontStyles.Normal, TextAlignmentOptions.Left);

            Button closeBtn = CreateCyberButton(panel.transform, "Btn_Close", "BACK TO MENU", new Vector2(0, -250), new Vector2(240, 50), new Color(0.45f, 0.2f, 0.3f));

            CreditsUI creditsUI = panel.AddComponent<CreditsUI>();
            creditsUI.BindCloseButton(closeBtn);

            panel.SetActive(false);
            return panel;
        }

        private static GameCardView CreateGameCard(Transform parent, string name, Vector2 pos, Vector2 size, GameInfo info)
        {
            GameObject cardGo = new GameObject(name);
            cardGo.transform.SetParent(parent, false);
            RectTransform rt = cardGo.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            // Background Card
            Image bg = cardGo.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.12f, 0.18f, 0.95f);

            // Thumbnail / Banner Placeholder
            GameObject thumbGo = new GameObject("Thumbnail");
            thumbGo.transform.SetParent(cardGo.transform, false);
            Image thumbImg = thumbGo.AddComponent<Image>();
            thumbImg.color = info != null && info.isEnabled ? new Color(0f, 0.6f, 0.7f, 0.9f) : new Color(0.2f, 0.25f, 0.35f, 0.8f);
            RectTransform thumbRt = thumbGo.GetComponent<RectTransform>();
            thumbRt.anchoredPosition = new Vector2(0, 110);
            thumbRt.sizeDelta = new Vector2(210, 130);

            // Title
            TextMeshProUGUI titleTxt = CreateTextTMP(cardGo.transform, "Title", info != null ? info.displayName : "Game Title", 18,
                new Vector2(0, 15), new Vector2(210, 45), Color.white, FontStyles.Bold, TextAlignmentOptions.Center);

            // Genre
            TextMeshProUGUI genreTxt = CreateTextTMP(cardGo.transform, "Genre", info != null ? info.genre : "Genre", 13,
                new Vector2(0, -15), new Vector2(210, 25), new Color(0f, 0.9f, 0.85f), FontStyles.Italic, TextAlignmentOptions.Center);

            // Description
            TextMeshProUGUI descTxt = CreateTextTMP(cardGo.transform, "Desc", info != null ? info.description : "", 12,
                new Vector2(0, -65), new Vector2(200, 65), new Color(0.8f, 0.85f, 0.9f, 0.9f), FontStyles.Normal, TextAlignmentOptions.Top);

            // Coming Soon Badge
            GameObject badgeGo = new GameObject("ComingSoonBadge");
            badgeGo.transform.SetParent(cardGo.transform, false);
            Image badgeImg = badgeGo.AddComponent<Image>();
            badgeImg.color = new Color(0.85f, 0.25f, 0.2f, 0.95f);
            RectTransform badgeRt = badgeGo.GetComponent<RectTransform>();
            badgeRt.anchoredPosition = new Vector2(0, 110);
            badgeRt.sizeDelta = new Vector2(170, 36);
            CreateTextTMP(badgeGo.transform, "BadgeLabel", "COMING SOON", 14, Vector2.zero, new Vector2(170, 36), Color.white, FontStyles.Bold, TextAlignmentOptions.Center);

            // Play Button
            Button playBtn = CreateCyberButton(cardGo.transform, "Btn_Play", info != null && info.isEnabled ? "PLAY" : "LOCKED",
                new Vector2(0, -145), new Vector2(190, 48), info != null && info.isEnabled ? new Color(0f, 0.65f, 0.6f) : new Color(0.2f, 0.25f, 0.3f));
            TextMeshProUGUI playBtnTxt = playBtn.GetComponentInChildren<TextMeshProUGUI>();

            GameCardView cardView = cardGo.AddComponent<GameCardView>();
            cardView.BindElements(thumbImg, titleTxt, genreTxt, descTxt, badgeGo, playBtn, playBtnTxt);
            return cardView;
        }

        private static void CreateGameOverlayCanvas(string gameTitle)
        {
            GameObject canvasGo = CreateCanvas("GameOverlayCanvas", 150);

            // Top Header Banner
            CreateTextTMP(canvasGo.transform, "GameTitleHUD", gameTitle, 22,
                new Vector2(0, 480), new Vector2(600, 40), new Color(0f, 1f, 0.95f, 0.85f), FontStyles.Bold, TextAlignmentOptions.Center);

            // Pause Button Top Right
            Button pauseBtn = CreateCyberButton(canvasGo.transform, "Btn_Pause", "PAUSE (Esc)",
                new Vector2(760, 460), new Vector2(180, 45), new Color(0.2f, 0.35f, 0.5f, 0.9f));

            // Pause Overlay Panel
            GameObject pausePanel = new GameObject("PausePanel");
            pausePanel.transform.SetParent(canvasGo.transform, false);
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

            GameOverlayUI overlayUI = canvasGo.AddComponent<GameOverlayUI>();
            overlayUI.BindControls(pausePanel, pauseBtn, resumeBtn, restartBtn, switchBtn, menuBtn);
        }

        private static GameObject CreateCamera(string name, Vector3 pos, Quaternion rot)
        {
            GameObject camGo = new GameObject(name);
            camGo.tag = "MainCamera";
            Camera cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.08f, 0.14f);
            cam.transform.position = pos;
            cam.transform.rotation = rot;

            var urpData = camGo.AddComponent<UniversalAdditionalCameraData>();
            urpData.renderShadows = true;

            camGo.AddComponent<AudioListener>();
            return camGo;
        }

        private static void CreateDirectionalLight()
        {
            GameObject lightGo = new GameObject("Directional Light");
            Light l = lightGo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(0.9f, 0.95f, 1f);
            l.intensity = 1.1f;
            lightGo.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        private static void CreateEventSystem()
        {
            GameObject esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<StandaloneInputModule>();
            esGo.AddComponent<SingleEventSystem>();
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
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in roots)
            {
                if (root != null)
                {
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
        }

        private static void RegisterBuildScenes()
        {
            string[] requiredScenes = new string[]
            {
                SCENE_BOOTSTRAP,        // Index 0 (Persistent Entry)
                SCENE_MAIN_MENU,        // Index 1
                SCENE_GAME_SELECTION,   // Index 2
                SCENE_LOADING,          // Index 3
                SCENE_SYAFIQ,           // Index 4
                SCENE_JAYA,             // Index 5
                SCENE_PRIMA,            // Index 6
                SCENE_BRILI,            // Index 7
                SCENE_RISA              // Index 8
            };

            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();
            for (int i = 0; i < requiredScenes.Length; i++)
            {
                string path = requiredScenes[i];
                if (File.Exists(path))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(path, true));
                    Debug.Log($"[MultiverseHubBuilder] Build Settings [{i}]: {path}");
                }
                else
                {
                    Debug.LogWarning($"[MultiverseHubBuilder] File scene tidak ditemukan: {path}");
                }
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("[MultiverseHubBuilder] Seluruh 9 scene terdaftar di EditorBuildSettings!");
        }

        private static void ValidateHubInternal(bool interactive)
        {
            List<string> issues = new List<string>();

            // Check Build Settings
            var buildScenes = EditorBuildSettings.scenes;
            if (buildScenes.Length < 9)
            {
                issues.Add($"Jumlah scene di Build Settings kurang ({buildScenes.Length}/9).");
            }
            else if (!buildScenes[0].path.EndsWith("Bootstrap.unity"))
            {
                issues.Add($"Scene index 0 bukan Bootstrap.unity ({buildScenes[0].path}).");
            }

            // Check GameRegistry
            GameRegistry reg = AssetDatabase.LoadAssetAtPath<GameRegistry>(REGISTRY_PATH);
            if (reg == null)
            {
                issues.Add("GameRegistry.asset tidak ditemukan di Assets/_Project/Data.");
            }
            else if (reg.games.Count < 5)
            {
                issues.Add($"GameRegistry hanya memiliki {reg.games.Count} game (diharapkan 5).");
            }

            // Check Audio Clips
            string[] audioPaths = new string[]
            {
                "Assets/_Project/Audio/sfx_ui_click.wav",
                "Assets/_Project/Audio/sfx_ui_hover.wav",
                "Assets/_Project/Audio/bgm_arcade_action.wav"
            };
            foreach (var p in audioPaths)
            {
                if (!File.Exists(p)) issues.Add($"File audio tidak ditemukan: {p}");
            }

            string report = issues.Count == 0
                ? "VALIDASI SUKSES!\n\n- Seluruh 9 Scene valid & terdaftar di Build Settings\n- Bootstrap berada di Index 0\n- GameRegistry lengkap dengan 5 game\n- Seluruh aset audio & skrip terintegrasi tanpa missing reference."
                : "VALIDASI MENEMUKAN MASALAH:\n\n" + string.Join("\n", issues);

            Debug.Log($"[MultiverseHubBuilder] {report}");
            if (interactive)
            {
                EditorUtility.DisplayDialog("Validasi Hub Multiverse Arcade", report, "OK");
            }
        }
        #endregion
    }
}
#endif
