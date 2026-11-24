#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Unity.AI.Navigation;
using MultiverseArcade.Syafiq.Brawl;

namespace MultiverseArcade.EditorTools
{
    public static class BrawlStarsSceneBuilder
    {
        private const string SCENE_PATH = "Assets/Games/Syafiq/Scenes/Game_Syafiq.unity";
        private const string PREFAB_DIR = "Assets/Games/Syafiq/Prefabs";
        private const string MAT_DIR = "Assets/Games/Syafiq/Materials";

        [MenuItem("Tools/Multiverse Arcade/Brawl Stars/Build Gem Grab Scene (Full 3v3)", false, 0)]
        [MenuItem("Multiverse/Build Gem Grab Scene", false, 1)]
        public static void BuildScene()
        {
            BuildSceneInternal();
        }

        [InitializeOnLoadMethod]
        private static void AutoRunOnce()
        {
            if (!EditorPrefs.GetBool("BrawlStars_GemGrab_AutoBuilt_v3", false))
            {
                EditorPrefs.SetBool("BrawlStars_GemGrab_AutoBuilt_v3", true);
                EditorApplication.delayCall += () =>
                {
                    BuildSceneInternal();
                };
            }
        }

        public static void BuildSceneFromCommandLine()
        {
            BuildSceneInternal();
        }

        private static void BuildSceneInternal()
        {
            Debug.Log("[BrawlStarsSceneBuilder] === MEMULAI BUILD BRAWL STARS GEM GRAB (3v3) ===");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Layer setup
            int unitLayer = LayerMask.NameToLayer("Unit");
            if (unitLayer < 0) unitLayer = LayerMask.NameToLayer("Default");
            LayerMask unitMask = (1 << unitLayer) | (1 << LayerMask.NameToLayer("Default"));

            // 1. Directional Light (Cahaya matahari hangat khas Brawl Stars)
            GameObject lightGo = new GameObject("Directional Light");
            Light l = lightGo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(1f, 0.96f, 0.88f);
            l.intensity = 1.25f;
            l.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // RenderSettings ambient
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.72f, 0.90f);
            RenderSettings.ambientEquatorColor = new Color(0.35f, 0.55f, 0.30f);
            RenderSettings.ambientGroundColor = new Color(0.20f, 0.28f, 0.18f);

            // Materials
            Material grassMat = CreateURPMaterial("Mat_Brawl_Grass", new Color(0.24f, 0.65f, 0.28f), 0.15f);
            Material wallMat = CreateURPMaterial("Mat_Brawl_Wall", new Color(0.42f, 0.48f, 0.56f), 0.35f);
            Material crateMat = CreateURPMaterial("Mat_Brawl_Crate", new Color(0.74f, 0.50f, 0.28f), 0.25f);
            Material bushMat = CreateURPMaterial("Mat_Brawl_Bush", new Color(0.12f, 0.48f, 0.18f, 0.9f), 0.2f);
            Material gemMat = CreateURPMaterial("Mat_Brawl_Gem", new Color(0.1f, 1f, 0.5f), 0.9f);
            Material mineMat = CreateURPMaterial("Mat_Brawl_Mine", new Color(0.25f, 0.22f, 0.35f), 0.4f);
            Material bulletBlueMat = CreateURPMaterial("Mat_Brawl_BulletBlue", new Color(0f, 0.9f, 1f), 0.95f);
            Material bulletRedMat = CreateURPMaterial("Mat_Brawl_BulletRed", new Color(1f, 0.3f, 0.3f), 0.95f);
            Material aimMat = CreateURPMaterial("Mat_Brawl_Aim", new Color(1f, 0.35f, 0.15f, 0.75f), 0.1f);
            Material shieldMat = CreateURPMaterial("Mat_Brawl_ShieldBubble", new Color(0.2f, 0.85f, 1f, 0.45f), 0.8f);

            // 2. Arena Root & Floor Plane
            GameObject arenaRoot = new GameObject("Arena_GemGrab_32x32");
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor_Plane";
            floor.transform.parent = arenaRoot.transform;
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(3.2f, 1f, 3.2f); // 32x32 m
            floor.GetComponent<Renderer>().sharedMaterial = grassMat;

            // 3. Batas Tembok Luar (Tagged Wall)
            CreateWall(arenaRoot.transform, "Wall_North", new Vector3(0, 1.25f, 16f), new Vector3(34, 2.5f, 2), wallMat);
            CreateWall(arenaRoot.transform, "Wall_South", new Vector3(0, 1.25f, -16f), new Vector3(34, 2.5f, 2), wallMat);
            CreateWall(arenaRoot.transform, "Wall_East", new Vector3(16f, 1.25f, 0), new Vector3(2, 2.5f, 32), wallMat);
            CreateWall(arenaRoot.transform, "Wall_West", new Vector3(-16f, 1.25f, 0), new Vector3(2, 2.5f, 32), wallMat);

            // 4. Rintangan Tembok & Peti Kayu Sesuai Layout Gem Grab
            CreateWall(arenaRoot.transform, "Wall_CenterLeft", new Vector3(-4.5f, 1f, 0), new Vector3(1.6f, 2f, 4.5f), wallMat);
            CreateWall(arenaRoot.transform, "Wall_CenterRight", new Vector3(4.5f, 1f, 0), new Vector3(1.6f, 2f, 4.5f), wallMat);

            CreateWall(arenaRoot.transform, "Crate_CenterNorth", new Vector3(0, 0.75f, 3.5f), new Vector3(2.5f, 1.5f, 1.5f), crateMat);
            CreateWall(arenaRoot.transform, "Crate_CenterSouth", new Vector3(0, 0.75f, -3.5f), new Vector3(2.5f, 1.5f, 1.5f), crateMat);

            CreateWall(arenaRoot.transform, "Wall_NW", new Vector3(-8.5f, 1f, 7.5f), new Vector3(3.5f, 2f, 1.5f), wallMat);
            CreateWall(arenaRoot.transform, "Wall_SE", new Vector3(8.5f, 1f, -7.5f), new Vector3(3.5f, 2f, 1.5f), wallMat);
            CreateWall(arenaRoot.transform, "Wall_NE", new Vector3(8.5f, 1f, 7.5f), new Vector3(3.5f, 2f, 1.5f), wallMat);
            CreateWall(arenaRoot.transform, "Wall_SW", new Vector3(-8.5f, 1f, -7.5f), new Vector3(3.5f, 2f, 1.5f), wallMat);

            // 5. Semak-Semak Stealth (Bush.cs dengan isTrigger Collider)
            CreateBushPatch(arenaRoot.transform, "Bush_CenterLeft", new Vector3(-4.5f, 0.5f, -4f), new Vector3(3.5f, 1f, 3.2f), bushMat);
            CreateBushPatch(arenaRoot.transform, "Bush_CenterRight", new Vector3(4.5f, 0.5f, 4f), new Vector3(3.5f, 1f, 3.2f), bushMat);
            CreateBushPatch(arenaRoot.transform, "Bush_WestFlank", new Vector3(-11.5f, 0.5f, 0), new Vector3(2.5f, 1f, 8f), bushMat);
            CreateBushPatch(arenaRoot.transform, "Bush_EastFlank", new Vector3(11.5f, 0.5f, 0), new Vector3(2.5f, 1f, 8f), bushMat);

            // 6. Prefabs: Gem & Peluru
            Gem gemPrefab = CreateGemPrefab(gemMat);
            Projectile playerProjPrefab = CreateProjectilePrefab("PF_BrawlBullet_Player", bulletBlueMat);
            Projectile enemyProjPrefab = CreateProjectilePrefab("PF_BrawlBullet_Enemy", bulletRedMat);

            // 7. Gem Mine di Tengah Arena
            GameObject mineGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            mineGo.name = "GemMine_Center";
            mineGo.transform.parent = arenaRoot.transform;
            mineGo.transform.position = new Vector3(0, 0.15f, 0);
            mineGo.transform.localScale = new Vector3(2.8f, 0.3f, 2.8f);
            mineGo.GetComponent<Renderer>().sharedMaterial = mineMat;
            Object.DestroyImmediate(mineGo.GetComponent<Collider>());

            GemMine gemMine = mineGo.AddComponent<GemMine>();
            gemMine.Setup(gemPrefab, 7.0f);

            // 8. GemGrabManager
            GameObject managerGo = new GameObject("GemGrabManager");
            GemGrabManager gemManager = managerGo.AddComponent<GemGrabManager>();

            // 9. Titik Spawn Tim (Markas Blue & Red)
            GameObject spawnsRoot = new GameObject("SpawnPoints");
            spawnsRoot.transform.parent = arenaRoot.transform;

            Transform[] blueSpawns = new Transform[3];
            Vector3[] bluePositions = new Vector3[] {
                new Vector3(-5f, 1.0f, -11f),
                new Vector3(0f, 1.0f, -12f),
                new Vector3(5f, 1.0f, -11f)
            };
            for (int i = 0; i < 3; i++)
            {
                GameObject p = new GameObject($"SpawnPoint_Blue_{i + 1}");
                p.transform.parent = spawnsRoot.transform;
                p.transform.position = bluePositions[i];
                p.transform.rotation = Quaternion.Euler(0, 0, 0);
                blueSpawns[i] = p.transform;
            }

            Transform[] redSpawns = new Transform[3];
            Vector3[] redPositions = new Vector3[] {
                new Vector3(-5f, 1.0f, 11f),
                new Vector3(0f, 1.0f, 12f),
                new Vector3(5f, 1.0f, 11f)
            };
            for (int i = 0; i < 3; i++)
            {
                GameObject p = new GameObject($"SpawnPoint_Red_{i + 1}");
                p.transform.parent = spawnsRoot.transform;
                p.transform.position = redPositions[i];
                p.transform.rotation = Quaternion.Euler(0, 180, 0);
                redSpawns[i] = p.transform;
            }

            GameObject rmGo = new GameObject("RespawnManager");
            RespawnManager rm = rmGo.AddComponent<RespawnManager>();
            var blueTeam = new RespawnManager.TeamSpawns { teamId = 0, points = blueSpawns };
            var redTeam = new RespawnManager.TeamSpawns { teamId = 1, points = redSpawns };
            rm.Setup(new RespawnManager.TeamSpawns[] { blueTeam, redTeam }, gemManager, 3f);

            // 10. UI Canvas Overlay
            GameObject canvasGo = new GameObject("Canvas_BrawlHUD");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Joysticks
            VirtualJoystick leftStick = CreateVirtualJoystick(canvasGo.transform, "Joystick_Move", new Vector2(250, 250), new Color(0.1f, 0.55f, 0.95f, 0.5f), new Color(0.2f, 0.75f, 1f, 0.85f));
            VirtualJoystick rightStick = CreateVirtualJoystick(canvasGo.transform, "Joystick_Aim", new Vector2(-250, 250), new Color(0.95f, 0.35f, 0.15f, 0.5f), new Color(1f, 0.55f, 0.2f, 0.85f));

            // 11. Player (Blue Team, teamId = 0)
            GameObject playerGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGo.name = "Player_Brawler";
            playerGo.transform.position = bluePositions[1]; // Spawn Selatan (Tengah)
            playerGo.layer = unitLayer;

            CharacterController cc = playerGo.AddComponent<CharacterController>();
            cc.radius = 0.5f;
            cc.height = 1.8f;
            cc.center = Vector3.zero;

            // Visual Model
            GameObject modelGo = new GameObject("VisualModel");
            modelGo.transform.parent = playerGo.transform;
            modelGo.transform.localPosition = new Vector3(0, -0.9f, 0);
            MeshRenderer playerMr = playerGo.GetComponent<MeshRenderer>();
            if (playerMr != null) playerMr.enabled = false;

            GameObject charFbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Games/Syafiq/Models/Characters/characterMedium.fbx");
            Texture2D soldierSkin = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Games/Syafiq/Textures/Skins/skin_soldier.png");
            Material pVisMat = CreateURPMaterial("Mat_Player_Soldier", Color.white, 0.5f);
            if (soldierSkin != null) pVisMat.mainTexture = soldierSkin;

            Attach3DModel(modelGo.transform, charFbx, pVisMat, Quaternion.identity);

            PlayerController pc = playerGo.AddComponent<PlayerController>();
            pc.SetMoveStick(leftStick);
            pc.SetModel(modelGo.transform);

            // Muzzle
            GameObject muzzleGo = new GameObject("Muzzle");
            muzzleGo.transform.parent = playerGo.transform;
            muzzleGo.transform.localPosition = new Vector3(0, 0.2f, 0.8f);

            // Aim Indicator Line
            GameObject aimLine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            aimLine.name = "AimIndicator_Line";
            aimLine.transform.parent = playerGo.transform;
            aimLine.transform.localPosition = new Vector3(0, -0.95f, 4f);
            aimLine.transform.localScale = new Vector3(0.15f, 0.05f, 8f);
            aimLine.GetComponent<Renderer>().sharedMaterial = aimMat;
            Object.DestroyImmediate(aimLine.GetComponent<Collider>());
            aimLine.SetActive(false);

            AimController ac = playerGo.AddComponent<AimController>();
            ac.Setup(rightStick, pc, muzzleGo.transform, playerProjPrefab, aimLine.transform, unitMask);

            Health playerHp = playerGo.AddComponent<Health>();
            playerHp.Max = 3600;
            playerHp.Current = 3600;

            GemCarrier playerCarrier = playerGo.AddComponent<GemCarrier>();
            playerCarrier.teamId = 0;
            playerCarrier.SetGemPrefab(gemPrefab);

            Hideable playerHide = playerGo.AddComponent<Hideable>();
            playerHide.Setup(true);

            // Shield Bubble & Respawnable & HitFeedback
            GameObject pShield = CreateShieldBubble(playerGo.transform, shieldMat);
            Respawnable playerResp = playerGo.AddComponent<Respawnable>();
            playerResp.Setup(pShield);

            HitFeedback playerHit = playerGo.AddComponent<HitFeedback>();
            playerHit.Setup(true);

            // 12. Bots (3v3 Match)
            // Blue Teammates (teamId = 0)
            CreateBotUnit("BlueBot_1", 0, bluePositions[0], unitLayer, unitMask, playerProjPrefab, gemPrefab, shieldMat, "skin_adventurer.png");
            CreateBotUnit("BlueBot_2", 0, bluePositions[2], unitLayer, unitMask, playerProjPrefab, gemPrefab, shieldMat, "cyborgFemaleA.png");

            // Red Enemies (teamId = 1)
            CreateBotUnit("RedBot_1", 1, redPositions[1], unitLayer, unitMask, enemyProjPrefab, gemPrefab, shieldMat, "skin_robot.png");
            CreateBotUnit("RedBot_2", 1, redPositions[0], unitLayer, unitMask, enemyProjPrefab, gemPrefab, shieldMat, "criminalMaleA.png");
            CreateBotUnit("RedBot_3", 1, redPositions[2], unitLayer, unitMask, enemyProjPrefab, gemPrefab, shieldMat, "skin_orc.png");

            // 13. HUD UI Setup
            CreateBrawlHUD(canvasGo.transform, ac, playerHp, playerCarrier, gemManager, playerResp);

            // 14. CameraFollow
            GameObject camGo = GameObject.FindWithTag("MainCamera");
            if (camGo == null)
            {
                camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                camGo.AddComponent<Camera>();
            }
            camGo.transform.position = new Vector3(0, 14f, -19f);
            CameraFollow camFollow = camGo.GetComponent<CameraFollow>();
            if (camFollow == null) camFollow = camGo.AddComponent<CameraFollow>();
            camFollow.SetTarget(playerGo.transform, rightStick);

            // 15. EventSystem
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // 16. Bake NavMeshSurface
            NavMeshSurface navSurface = arenaRoot.AddComponent<NavMeshSurface>();
            navSurface.collectObjects = CollectObjects.All;
            navSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            navSurface.BuildNavMesh();
            Debug.Log("[BrawlStarsSceneBuilder] NavMeshSurface Gem Grab berhasil dibake!");

            // Simpan Scene
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Log($"[BrawlStarsSceneBuilder] Scene Gem Grab berhasil disimpan ke {SCENE_PATH}!");
        }

        private static GameObject CreateWall(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.tag = "Wall";
            wall.transform.parent = parent;
            wall.transform.position = pos;
            wall.transform.localScale = size;
            wall.GetComponent<Renderer>().sharedMaterial = mat;
            return wall;
        }

        private static GameObject CreateBushPatch(Transform parent, string name, Vector3 pos, Vector3 size, Material mat)
        {
            GameObject bushGo = new GameObject(name);
            bushGo.tag = "Bush";
            bushGo.transform.parent = parent;
            bushGo.transform.position = pos;

            BoxCollider bc = bushGo.AddComponent<BoxCollider>();
            bc.isTrigger = true;
            bc.size = size;

            bushGo.AddComponent<Bush>();

            GameObject vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.name = "BushVisual";
            vis.transform.parent = bushGo.transform;
            vis.transform.localPosition = Vector3.zero;
            vis.transform.localScale = new Vector3(size.x, 0.7f, size.z);
            vis.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(vis.GetComponent<Collider>());

            return bushGo;
        }

        private static GameObject CreateShieldBubble(Transform parent, Material mat)
        {
            GameObject shield = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            shield.name = "ShieldBubble_VFX";
            shield.transform.parent = parent;
            shield.transform.localPosition = Vector3.zero;
            shield.transform.localScale = new Vector3(1.7f, 2.1f, 1.7f);
            shield.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(shield.GetComponent<Collider>());
            shield.SetActive(false);
            return shield;
        }

        private static Gem CreateGemPrefab(Material mat)
        {
            string path = $"{PREFAB_DIR}/PF_BrawlGem.prefab";
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "PF_BrawlGem";
            go.transform.localScale = new Vector3(0.45f, 0.6f, 0.45f);
            go.transform.rotation = Quaternion.Euler(45, 45, 0);
            go.GetComponent<Renderer>().sharedMaterial = mat;

            Collider col = go.GetComponent<Collider>();
            col.isTrigger = true;

            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            Gem g = go.AddComponent<Gem>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Gem>();
        }

        private static Projectile CreateProjectilePrefab(string name, Material mat)
        {
            string path = $"{PREFAB_DIR}/{name}.prefab";
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.localScale = Vector3.one * 0.4f;
            go.GetComponent<Renderer>().sharedMaterial = mat;

            Collider col = go.GetComponent<Collider>();
            col.isTrigger = true;

            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            Projectile p = go.AddComponent<Projectile>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab.GetComponent<Projectile>();
        }

        private static GameObject CreateBotUnit(string name, int teamId, Vector3 pos, int layer, LayerMask mask, Projectile proj, Gem gemPrefab, Material shieldMat, string skinName)
        {
            GameObject bot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bot.name = name;
            bot.transform.position = pos;
            bot.layer = layer;

            MeshRenderer mr = bot.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;

            CharacterController cc = bot.AddComponent<CharacterController>();
            cc.radius = 0.5f;
            cc.height = 1.8f;
            cc.center = Vector3.zero;

            NavMeshAgent agent = bot.AddComponent<NavMeshAgent>();
            agent.speed = 4.2f;
            agent.radius = 0.5f;
            agent.height = 1.8f;
            agent.stoppingDistance = 0.2f;

            // Visual Model
            GameObject modelGo = new GameObject("VisualModel");
            modelGo.transform.parent = bot.transform;
            modelGo.transform.localPosition = new Vector3(0, -0.9f, 0);

            GameObject charFbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Games/Syafiq/Models/Characters/characterMedium.fbx");
            Texture2D skinTex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Games/Syafiq/Textures/Skins/{skinName}");
            Material bMat = CreateURPMaterial($"Mat_{name}", teamId == 0 ? new Color(0.2f, 0.7f, 1f) : new Color(1f, 0.3f, 0.3f), 0.5f);
            if (skinTex != null) bMat.mainTexture = skinTex;

            Attach3DModel(modelGo.transform, charFbx, bMat, Quaternion.Euler(0, teamId == 0 ? 0 : 180, 0));

            // Muzzle
            GameObject muzzleGo = new GameObject("Muzzle");
            muzzleGo.transform.parent = bot.transform;
            muzzleGo.transform.localPosition = new Vector3(0, 0.2f, 0.8f);

            Health hp = bot.AddComponent<Health>();
            hp.Max = 3200;
            hp.Current = 3200;

            GemCarrier carrier = bot.AddComponent<GemCarrier>();
            carrier.teamId = teamId;
            carrier.SetGemPrefab(gemPrefab);

            Hideable hide = bot.AddComponent<Hideable>();
            hide.Setup(false);

            GameObject bShield = CreateShieldBubble(bot.transform, shieldMat);
            Respawnable bResp = bot.AddComponent<Respawnable>();
            bResp.Setup(bShield);

            HitFeedback bHit = bot.AddComponent<HitFeedback>();
            bHit.Setup(false);

            BotAI ai = bot.AddComponent<BotAI>();
            ai.Setup(proj, muzzleGo.transform, mask);

            return bot;
        }

        private static void Attach3DModel(Transform parent, GameObject fbx, Material mat, Quaternion rot)
        {
            if (fbx != null)
            {
                GameObject inst = (GameObject)Object.Instantiate(fbx, parent);
                inst.name = "Mesh";
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = rot;
                foreach (var r in inst.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
                foreach (var c in inst.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            }
            else
            {
                GameObject vis = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                vis.transform.parent = parent;
                vis.transform.localPosition = new Vector3(0, 0.9f, 0);
                vis.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(vis.GetComponent<Collider>());
            }
        }

        private static VirtualJoystick CreateVirtualJoystick(Transform canvasTrans, string name, Vector2 anchoredPos, Color bgColor, Color handleColor)
        {
            GameObject stickGo = new GameObject(name);
            stickGo.transform.parent = canvasTrans;
            RectTransform stickRt = stickGo.AddComponent<RectTransform>();
            stickRt.sizeDelta = new Vector2(180, 180);

            if (anchoredPos.x < 0)
            {
                stickRt.anchorMin = new Vector2(1, 0);
                stickRt.anchorMax = new Vector2(1, 0);
            }
            else
            {
                stickRt.anchorMin = new Vector2(0, 0);
                stickRt.anchorMax = new Vector2(0, 0);
            }
            stickRt.anchoredPosition = anchoredPos;

            Image bgImg = stickGo.AddComponent<Image>();
            bgImg.color = bgColor;

            GameObject handleGo = new GameObject("Handle");
            handleGo.transform.parent = stickGo.transform;
            RectTransform handleRt = handleGo.AddComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(70, 70);
            handleRt.anchoredPosition = Vector2.zero;

            Image handleImg = handleGo.AddComponent<Image>();
            handleImg.color = handleColor;

            VirtualJoystick vj = stickGo.AddComponent<VirtualJoystick>();
            return vj;
        }

        private static void CreateBrawlHUD(Transform canvasTrans, AimController ac, Health hp, GemCarrier carrier, GemGrabManager manager, Respawnable playerResp)
        {
            GameObject hudGo = new GameObject("BrawlHUD");
            hudGo.transform.parent = canvasTrans;
            RectTransform hudRt = hudGo.AddComponent<RectTransform>();
            hudRt.anchorMin = Vector2.zero;
            hudRt.anchorMax = Vector2.one;
            hudRt.sizeDelta = Vector2.zero;

            BrawlHUD hud = hudGo.AddComponent<BrawlHUD>();

            // --- Top Header: Gem Counters ---
            GameObject topPanel = new GameObject("TopPanel_Gems");
            topPanel.transform.parent = hudGo.transform;
            RectTransform topRt = topPanel.AddComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0.5f, 1f);
            topRt.anchorMax = new Vector2(0.5f, 1f);
            topRt.anchoredPosition = new Vector2(0, -50);
            topRt.sizeDelta = new Vector2(400, 60);

            // Blue Gem Score Box
            GameObject blueBox = new GameObject("BlueGems");
            blueBox.transform.parent = topPanel.transform;
            RectTransform blueRt = blueBox.AddComponent<RectTransform>();
            blueRt.anchoredPosition = new Vector2(-110, 0);
            blueRt.sizeDelta = new Vector2(140, 50);
            Image blueBg = blueBox.AddComponent<Image>();
            blueBg.color = new Color(0.05f, 0.45f, 0.85f, 0.9f);

            GameObject blueTxtGo = new GameObject("ScoreText");
            blueTxtGo.transform.parent = blueBox.transform;
            RectTransform bTxtRt = blueTxtGo.AddComponent<RectTransform>();
            bTxtRt.sizeDelta = new Vector2(140, 50);
            Text bTxt = blueTxtGo.AddComponent<Text>();
            bTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bTxt.fontSize = 28;
            bTxt.fontStyle = FontStyle.Bold;
            bTxt.alignment = TextAnchor.MiddleCenter;
            bTxt.color = Color.white;
            bTxt.text = "BLUE: 0";

            // Red Gem Score Box
            GameObject redBox = new GameObject("RedGems");
            redBox.transform.parent = topPanel.transform;
            RectTransform redRt = redBox.AddComponent<RectTransform>();
            redRt.anchoredPosition = new Vector2(110, 0);
            redRt.sizeDelta = new Vector2(140, 50);
            Image redBg = redBox.AddComponent<Image>();
            redBg.color = new Color(0.85f, 0.2f, 0.2f, 0.9f);

            GameObject redTxtGo = new GameObject("ScoreText");
            redTxtGo.transform.parent = redBox.transform;
            RectTransform rTxtRt = redTxtGo.AddComponent<RectTransform>();
            rTxtRt.sizeDelta = new Vector2(140, 50);
            Text rTxt = redTxtGo.AddComponent<Text>();
            rTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            rTxt.fontSize = 28;
            rTxt.fontStyle = FontStyle.Bold;
            rTxt.alignment = TextAnchor.MiddleCenter;
            rTxt.color = Color.white;
            rTxt.text = "RED: 0";

            // --- Countdown Banner ---
            GameObject cdBanner = new GameObject("CountdownBanner");
            cdBanner.transform.parent = hudGo.transform;
            RectTransform cdRt = cdBanner.AddComponent<RectTransform>();
            cdRt.anchorMin = new Vector2(0.5f, 1f);
            cdRt.anchorMax = new Vector2(0.5f, 1f);
            cdRt.anchoredPosition = new Vector2(0, -120);
            cdRt.sizeDelta = new Vector2(500, 50);
            Image cdBg = cdBanner.AddComponent<Image>();
            cdBg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

            GameObject cdTxtGo = new GameObject("CountdownText");
            cdTxtGo.transform.parent = cdBanner.transform;
            RectTransform cdTxtRt = cdTxtGo.AddComponent<RectTransform>();
            cdTxtRt.sizeDelta = new Vector2(500, 50);
            Text cdTxt = cdTxtGo.AddComponent<Text>();
            cdTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            cdTxt.fontSize = 26;
            cdTxt.fontStyle = FontStyle.Bold;
            cdTxt.alignment = TextAnchor.MiddleCenter;
            cdTxt.color = Color.yellow;
            cdTxt.text = "COUNTDOWN: 15s";

            // --- Match End Banner ---
            GameObject endBanner = new GameObject("MatchEndBanner");
            endBanner.transform.parent = hudGo.transform;
            RectTransform endRt = endBanner.AddComponent<RectTransform>();
            endRt.anchorMin = new Vector2(0.5f, 0.5f);
            endRt.anchorMax = new Vector2(0.5f, 0.5f);
            endRt.anchoredPosition = Vector2.zero;
            endRt.sizeDelta = new Vector2(600, 200);
            Image endBg = endBanner.AddComponent<Image>();
            endBg.color = new Color(0.08f, 0.08f, 0.12f, 0.95f);

            GameObject endTitleGo = new GameObject("Title");
            endTitleGo.transform.parent = endBanner.transform;
            RectTransform etRt = endTitleGo.AddComponent<RectTransform>();
            etRt.sizeDelta = new Vector2(550, 100);
            etRt.anchoredPosition = new Vector2(0, 30);
            Text etTxt = endTitleGo.AddComponent<Text>();
            etTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            etTxt.fontSize = 54;
            etTxt.fontStyle = FontStyle.Bold;
            etTxt.alignment = TextAnchor.MiddleCenter;
            etTxt.color = Color.white;
            etTxt.text = "VICTORY!";

            // --- Respawn Banner ---
            GameObject respBanner = new GameObject("RespawnBanner");
            respBanner.transform.parent = hudGo.transform;
            RectTransform respRt = respBanner.AddComponent<RectTransform>();
            respRt.anchorMin = new Vector2(0.5f, 0.5f);
            respRt.anchorMax = new Vector2(0.5f, 0.5f);
            respRt.anchoredPosition = new Vector2(0, -60);
            respRt.sizeDelta = new Vector2(400, 50);
            Image respBg = respBanner.AddComponent<Image>();
            respBg.color = new Color(0.1f, 0.1f, 0.15f, 0.85f);

            GameObject respTxtGo = new GameObject("RespawnText");
            respTxtGo.transform.parent = respBanner.transform;
            RectTransform respTxtRt = respTxtGo.AddComponent<RectTransform>();
            respTxtRt.sizeDelta = new Vector2(400, 50);
            Text respTxt = respTxtGo.AddComponent<Text>();
            respTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            respTxt.fontSize = 24;
            respTxt.fontStyle = FontStyle.Bold;
            respTxt.alignment = TextAnchor.MiddleCenter;
            respTxt.color = new Color(1f, 0.85f, 0.2f);
            respTxt.text = "RESPAWNING IN 3s";
            respBanner.SetActive(false);

            // --- Bottom Center: Ammo & HP & Carried Gems ---
            GameObject bottomPanel = new GameObject("BottomPanel_Status");
            bottomPanel.transform.parent = hudGo.transform;
            RectTransform bpRt = bottomPanel.AddComponent<RectTransform>();
            bpRt.anchorMin = new Vector2(0.5f, 0f);
            bpRt.anchorMax = new Vector2(0.5f, 0f);
            bpRt.anchoredPosition = new Vector2(0, 75);
            bpRt.sizeDelta = new Vector2(320, 100);

            // Ammo 3 Segments
            GameObject ammoContainer = new GameObject("AmmoContainer");
            ammoContainer.transform.parent = bottomPanel.transform;
            RectTransform ammoRt = ammoContainer.AddComponent<RectTransform>();
            ammoRt.anchoredPosition = new Vector2(0, 25);
            ammoRt.sizeDelta = new Vector2(240, 16);
            HorizontalLayoutGroup hlg = ammoContainer.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            Image[] segs = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject seg = new GameObject($"Ammo_{i + 1}");
                seg.transform.parent = ammoContainer.transform;
                segs[i] = seg.AddComponent<Image>();
                segs[i].color = new Color(1f, 0.65f, 0f, 1f);
            }

            // Health Slider
            GameObject hpGo = new GameObject("HealthBar");
            hpGo.transform.parent = bottomPanel.transform;
            RectTransform hpRt = hpGo.AddComponent<RectTransform>();
            hpRt.anchoredPosition = new Vector2(0, -5);
            hpRt.sizeDelta = new Vector2(240, 22);

            Slider slider = hpGo.AddComponent<Slider>();
            GameObject hpBg = new GameObject("Background");
            hpBg.transform.parent = hpGo.transform;
            RectTransform hpBgRt = hpBg.AddComponent<RectTransform>();
            hpBgRt.anchorMin = Vector2.zero;
            hpBgRt.anchorMax = Vector2.one;
            hpBgRt.sizeDelta = Vector2.zero;
            Image hpBgImg = hpBg.AddComponent<Image>();
            hpBgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.85f);

            GameObject fill = new GameObject("Fill");
            fill.transform.parent = hpGo.transform;
            RectTransform fillRt = fill.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.9f, 0.35f, 1f);
            slider.fillRect = fillRt;

            GameObject hpTxtGo = new GameObject("HpText");
            hpTxtGo.transform.parent = hpGo.transform;
            RectTransform hpTxtRt = hpTxtGo.AddComponent<RectTransform>();
            hpTxtRt.sizeDelta = new Vector2(240, 22);
            Text hpTxt = hpTxtGo.AddComponent<Text>();
            hpTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            hpTxt.fontSize = 14;
            hpTxt.fontStyle = FontStyle.Bold;
            hpTxt.alignment = TextAnchor.MiddleCenter;
            hpTxt.color = Color.white;

            // Player Carried Gems Text
            GameObject pGemsGo = new GameObject("PlayerGemsText");
            pGemsGo.transform.parent = bottomPanel.transform;
            RectTransform pgRt = pGemsGo.AddComponent<RectTransform>();
            pgRt.anchoredPosition = new Vector2(0, -32);
            pgRt.sizeDelta = new Vector2(240, 22);
            Text pGemsTxt = pGemsGo.AddComponent<Text>();
            pGemsTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            pGemsTxt.fontSize = 16;
            pGemsTxt.fontStyle = FontStyle.Bold;
            pGemsTxt.alignment = TextAnchor.MiddleCenter;
            pGemsTxt.color = new Color(0.1f, 1f, 0.5f);
            pGemsTxt.text = "Gems: 0";

            // Bind Fields
            SetPrivateField(hud, "ammoSegments", segs);
            SetPrivateField(hud, "healthSlider", slider);
            SetPrivateField(hud, "healthText", hpTxt);
            SetPrivateField(hud, "playerGemsText", pGemsTxt);
            SetPrivateField(hud, "blueGemsText", bTxt);
            SetPrivateField(hud, "redGemsText", rTxt);
            SetPrivateField(hud, "countdownBanner", cdBanner);
            SetPrivateField(hud, "countdownText", cdTxt);
            SetPrivateField(hud, "matchEndBanner", endBanner);
            SetPrivateField(hud, "matchEndTitle", etTxt);
            SetPrivateField(hud, "respawnBanner", respBanner);
            SetPrivateField(hud, "respawnLabel", respTxt);

            hud.Bind(ac, hp, carrier, manager, playerResp);
        }

        private static Material CreateURPMaterial(string name, Color color, float smoothness = 0.5f)
        {
            string matPath = $"{MAT_DIR}/{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

            if (mat == null)
            {
                mat = new Material(shader);
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
                AssetDatabase.CreateAsset(mat, matPath);
            }
            else
            {
                mat.shader = shader;
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                EditorUtility.SetDirty(mat);
            }
            return mat;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            if (field != null) field.SetValue(target, value);
        }
    }
}
#endif
