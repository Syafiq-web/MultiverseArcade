#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using MultiverseArcade.Syafiq.Brawl;

namespace MultiverseArcade.EditorTools
{
    public static class BrawlStarsSceneBuilder
    {
        private const string SCENE_PATH = "Assets/Games/Syafiq/Scenes/Game_Syafiq.unity";
        private const string PREFAB_DIR = "Assets/Games/Syafiq/Prefabs";
        private const string MAT_DIR = "Assets/Games/Syafiq/Materials";

        [MenuItem("Tools/Multiverse Arcade/Brawl Stars/Build Brawl Stars Scene", false, 0)]
        [MenuItem("Multiverse/Build Brawl Stars Scene", false, 1)]
        public static void BuildScene()
        {
            BuildSceneInternal();
        }

        public static void BuildSceneFromCommandLine()
        {
            BuildSceneInternal();
        }

        private static void BuildSceneInternal()
        {
            Debug.Log("[BrawlStarsSceneBuilder] === MEMULAI BUILD BRAWL STARS SCENE ===");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Directional Light
            GameObject lightGo = new GameObject("Directional Light");
            Light l = lightGo.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = new Color(1f, 0.96f, 0.88f);
            l.intensity = 1.2f;
            l.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Materials
            Material grassMat = CreateURPMaterial("Mat_Brawl_Grass", new Color(0.24f, 0.62f, 0.28f), 0.2f);
            Material wallMat = CreateURPMaterial("Mat_Brawl_Wall", new Color(0.38f, 0.44f, 0.52f), 0.4f);
            Material crateMat = CreateURPMaterial("Mat_Brawl_Crate", new Color(0.72f, 0.48f, 0.26f), 0.3f);
            Material bushMat = CreateURPMaterial("Mat_Brawl_Bush", new Color(0.15f, 0.50f, 0.20f), 0.2f);
            Material playerMat = CreateURPMaterial("Mat_Brawl_Player", new Color(0f, 0.75f, 1f), 0.5f);
            Material enemyMat = CreateURPMaterial("Mat_Brawl_Enemy", new Color(1f, 0.28f, 0.28f), 0.5f);
            Material bulletMat = CreateURPMaterial("Mat_Brawl_Bullet", new Color(1f, 0.85f, 0.1f), 0.9f);
            Material aimMat = CreateURPMaterial("Mat_Brawl_Aim", new Color(1f, 0.3f, 0.2f, 0.7f), 0.1f);

            // 2. Floor Plane (Lantai Lapangan Brawl Stars)
            GameObject arenaRoot = new GameObject("Arena_BrawlStars");
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor_Plane";
            floor.transform.parent = arenaRoot.transform;
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(3.2f, 1f, 3.2f); // 32x32 m
            floor.GetComponent<Renderer>().sharedMaterial = grassMat;

            // 3. Batas Tembok Luar (Wall Tag)
            CreateWall(arenaRoot.transform, "Wall_North", new Vector3(0, 1.25f, 16f), new Vector3(34, 2.5f, 2), wallMat);
            CreateWall(arenaRoot.transform, "Wall_South", new Vector3(0, 1.25f, -16f), new Vector3(34, 2.5f, 2), wallMat);
            CreateWall(arenaRoot.transform, "Wall_East", new Vector3(16f, 1.25f, 0), new Vector3(2, 2.5f, 32), wallMat);
            CreateWall(arenaRoot.transform, "Wall_West", new Vector3(-16f, 1.25f, 0), new Vector3(2, 2.5f, 32), wallMat);

            // 4. Rintangan Tembok & Peti Kayu di Dalam Arena (Sesuai Layout Brawl Stars)
            // Tengah (Center choke points)
            CreateWall(arenaRoot.transform, "Wall_CenterLeft", new Vector3(-4f, 1f, 0), new Vector3(1.5f, 2f, 4f), wallMat);
            CreateWall(arenaRoot.transform, "Wall_CenterRight", new Vector3(4f, 1f, 0), new Vector3(1.5f, 2f, 4f), wallMat);
            CreateWall(arenaRoot.transform, "Crate_CenterTop", new Vector3(0, 0.75f, 3f), new Vector3(2f, 1.5f, 1.5f), crateMat);
            CreateWall(arenaRoot.transform, "Crate_CenterBottom", new Vector3(0, 0.75f, -3f), new Vector3(2f, 1.5f, 1.5f), crateMat);

            // Flanks (Kiri & Kanan)
            CreateWall(arenaRoot.transform, "Wall_FlankNorthWest", new Vector3(-8f, 1f, 7f), new Vector3(3f, 2f, 1.5f), wallMat);
            CreateWall(arenaRoot.transform, "Wall_FlankSouthEast", new Vector3(8f, 1f, -7f), new Vector3(3f, 2f, 1.5f), wallMat);
            CreateWall(arenaRoot.transform, "Wall_FlankNorthEast", new Vector3(8f, 1f, 7f), new Vector3(3f, 2f, 1.5f), wallMat);
            CreateWall(arenaRoot.transform, "Wall_FlankSouthWest", new Vector3(-8f, 1f, -7f), new Vector3(3f, 2f, 1.5f), wallMat);

            // 5. Semak-Semak (Bush.cs dengan Collider isTrigger)
            CreateBushPatch(arenaRoot.transform, "Bush_CenterLeft", new Vector3(-4f, 0.5f, -4f), new Vector3(3.5f, 1f, 3f), bushMat);
            CreateBushPatch(arenaRoot.transform, "Bush_CenterRight", new Vector3(4f, 0.5f, 4f), new Vector3(3.5f, 1f, 3f), bushMat);
            CreateBushPatch(arenaRoot.transform, "Bush_FlankWest", new Vector3(-11f, 0.5f, 0), new Vector3(2.5f, 1f, 8f), bushMat);
            CreateBushPatch(arenaRoot.transform, "Bush_FlankEast", new Vector3(11f, 0.5f, 0), new Vector3(2.5f, 1f, 8f), bushMat);

            // 6. Prefab Peluru (Sphere kecil + Rigidbody Kinematic + Collider isTrigger + Projectile)
            Projectile projPrefab = CreateProjectilePrefab(bulletMat);

            // 7. UI Canvas (Screen Space - Overlay) dengan Dua Joystick & HUD
            GameObject canvasGo = new GameObject("Canvas_BrawlHUD");
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Left Joystick (Movement)
            VirtualJoystick leftStick = CreateVirtualJoystick(canvasGo.transform, "Joystick_Move", new Vector2(250, 250), new Color(0.1f, 0.55f, 0.95f, 0.5f), new Color(0.2f, 0.75f, 1f, 0.85f));

            // Right Joystick (Aim & Shoot)
            VirtualJoystick rightStick = CreateVirtualJoystick(canvasGo.transform, "Joystick_Aim", new Vector2(-250, 250), new Color(0.95f, 0.35f, 0.15f, 0.5f), new Color(1f, 0.55f, 0.2f, 0.85f));

            // 8. Player: Capsule + CharacterController + PlayerController + AimController + Hideable
            GameObject playerGo = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            playerGo.name = "Player_Brawler";
            playerGo.transform.position = new Vector3(0, 1.0f, -10f); // Spawn di sisi Selatan

            CharacterController cc = playerGo.AddComponent<CharacterController>();
            cc.radius = 0.5f;
            cc.height = 1.8f;
            cc.center = new Vector3(0, 0, 0);

            // Model Visual Child
            GameObject modelGo = new GameObject("VisualModel");
            modelGo.transform.parent = playerGo.transform;
            modelGo.transform.localPosition = new Vector3(0, -0.9f, 0);

            // Ganti renderer capsule default ke model visual
            MeshRenderer playerMr = playerGo.GetComponent<MeshRenderer>();
            if (playerMr != null) playerMr.enabled = false;

            // Load 3D model FBX jika ada
            GameObject charFbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Games/Syafiq/Models/Characters/characterMedium.fbx");
            Texture2D skinTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Games/Syafiq/Textures/Skins/skin_soldier.png");
            if (skinTex != null) playerMat.mainTexture = skinTex;

            if (charFbx != null)
            {
                GameObject fbxInst = (GameObject)Object.Instantiate(charFbx, modelGo.transform);
                fbxInst.name = "CharacterMesh";
                fbxInst.transform.localPosition = Vector3.zero;
                fbxInst.transform.localRotation = Quaternion.identity;
                foreach (var r in fbxInst.GetComponentsInChildren<Renderer>()) r.sharedMaterial = playerMat;
                foreach (var c in fbxInst.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            }
            else
            {
                // Fallback visual capsule
                GameObject vis = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                vis.transform.parent = modelGo.transform;
                vis.transform.localPosition = new Vector3(0, 0.9f, 0);
                vis.GetComponent<Renderer>().sharedMaterial = playerMat;
                Object.DestroyImmediate(vis.GetComponent<Collider>());
            }

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

            // AimController
            AimController ac = playerGo.AddComponent<AimController>();
            LayerMask enemyMask = LayerMask.GetMask("Syafiq_Enemy", "Default");
            ac.Setup(rightStick, pc, muzzleGo.transform, projPrefab, aimLine.transform, enemyMask);

            // Health & Hideable
            Health playerHp = playerGo.AddComponent<Health>();
            playerHp.Max = 3600;
            playerHp.Current = 3600;

            Hideable playerHide = playerGo.AddComponent<Hideable>();
            playerHide.Setup(true);

            // 9. Enemy Bots (Target dummy & musuh)
            CreateEnemyBot(new Vector3(-6f, 1.0f, 8f), enemyMat, projPrefab);
            CreateEnemyBot(new Vector3(6f, 1.0f, 8f), enemyMat, projPrefab);
            CreateEnemyBot(new Vector3(0f, 1.0f, 10f), enemyMat, projPrefab);

            // 10. HUD Ammo & Health Slider
            CreateHUDUI(canvasGo.transform, ac, playerHp);

            // 11. Main Camera dengan CameraFollow
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

            // 12. EventSystem
            if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            // Simpan Scene
            EditorSceneManager.SaveScene(scene, SCENE_PATH);
            Debug.Log($"[BrawlStarsSceneBuilder] Scene berhasil dibangun dan disimpan ke {SCENE_PATH}!");
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

            // Visual semak low-poly
            GameObject vis = GameObject.CreatePrimitive(PrimitiveType.Cube);
            vis.name = "BushVisual";
            vis.transform.parent = bushGo.transform;
            vis.transform.localPosition = Vector3.zero;
            vis.transform.localScale = new Vector3(size.x, 0.7f, size.z);
            vis.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(vis.GetComponent<Collider>());

            return bushGo;
        }

        private static Projectile CreateProjectilePrefab(Material mat)
        {
            string path = $"{PREFAB_DIR}/PF_BrawlBullet.prefab";
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "PF_BrawlBullet";
            go.transform.localScale = Vector3.one * 0.45f;
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

        private static GameObject CreateEnemyBot(Vector3 pos, Material mat, Projectile projPrefab)
        {
            GameObject bot = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bot.name = "Enemy_Bot";
            bot.transform.position = pos;

            int enemyLayer = LayerMask.NameToLayer("Syafiq_Enemy");
            if (enemyLayer >= 0) bot.layer = enemyLayer;

            CharacterController cc = bot.AddComponent<CharacterController>();
            cc.radius = 0.5f;
            cc.height = 1.8f;

            // Visual Model
            GameObject modelGo = new GameObject("VisualModel");
            modelGo.transform.parent = bot.transform;
            modelGo.transform.localPosition = new Vector3(0, -0.9f, 0);

            MeshRenderer mr = bot.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;

            GameObject charFbx = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Games/Syafiq/Models/Characters/characterMedium.fbx");
            Texture2D skinTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Games/Syafiq/Textures/Skins/skin_robot.png");
            if (skinTex != null) mat.mainTexture = skinTex;

            if (charFbx != null)
            {
                GameObject fbxInst = (GameObject)Object.Instantiate(charFbx, modelGo.transform);
                fbxInst.name = "BotMesh";
                fbxInst.transform.localPosition = Vector3.zero;
                fbxInst.transform.localRotation = Quaternion.Euler(0, 180, 0);
                foreach (var r in fbxInst.GetComponentsInChildren<Renderer>()) r.sharedMaterial = mat;
                foreach (var c in fbxInst.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            }
            else
            {
                GameObject vis = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                vis.transform.parent = modelGo.transform;
                vis.transform.localPosition = new Vector3(0, 0.9f, 0);
                vis.GetComponent<Renderer>().sharedMaterial = mat;
                Object.DestroyImmediate(vis.GetComponent<Collider>());
            }

            Health hp = bot.AddComponent<Health>();
            hp.Max = 3200;
            hp.Current = 3200;

            Hideable hide = bot.AddComponent<Hideable>();
            hide.Setup(false);

            return bot;
        }

        private static VirtualJoystick CreateVirtualJoystick(Transform parent, string name, Vector2 anchoredPos, Color bgColor, Color handleColor)
        {
            GameObject stickGo = new GameObject(name);
            stickGo.transform.parent = parent;
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

        private static void CreateHUDUI(Transform canvasTrans, AimController ac, Health hp)
        {
            GameObject hudGo = new GameObject("BrawlHUD");
            hudGo.transform.parent = canvasTrans;
            RectTransform hudRt = hudGo.AddComponent<RectTransform>();
            hudRt.anchorMin = new Vector2(0.5f, 0);
            hudRt.anchorMax = new Vector2(0.5f, 0);
            hudRt.anchoredPosition = new Vector2(0, 70);
            hudRt.sizeDelta = new Vector2(300, 80);

            BrawlHUD hud = hudGo.AddComponent<BrawlHUD>();

            // Ammo Container (3 Bars)
            GameObject ammoContainer = new GameObject("AmmoContainer");
            ammoContainer.transform.parent = hudGo.transform;
            RectTransform ammoRt = ammoContainer.AddComponent<RectTransform>();
            ammoRt.anchoredPosition = new Vector2(0, 20);
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
            hpGo.transform.parent = hudGo.transform;
            RectTransform hpRt = hpGo.AddComponent<RectTransform>();
            hpRt.anchoredPosition = new Vector2(0, -15);
            hpRt.sizeDelta = new Vector2(240, 22);

            Slider slider = hpGo.AddComponent<Slider>();

            GameObject bg = new GameObject("Background");
            bg.transform.parent = hpGo.transform;
            RectTransform bgrt = bg.AddComponent<RectTransform>();
            bgrt.anchorMin = Vector2.zero;
            bgrt.anchorMax = Vector2.one;
            bgrt.sizeDelta = Vector2.zero;
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

            GameObject fill = new GameObject("Fill");
            fill.transform.parent = hpGo.transform;
            RectTransform fillRt = fill.AddComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.2f, 0.9f, 0.3f, 1f);

            slider.targetGraphic = fillImg;
            slider.fillRect = fillRt;

            // Bind to BrawlHUD
            SetPrivateField(hud, "ammoSegments", segs);
            SetPrivateField(hud, "healthSlider", slider);
            hud.Bind(ac, hp);
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
