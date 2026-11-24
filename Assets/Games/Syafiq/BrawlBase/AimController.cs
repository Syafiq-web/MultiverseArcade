using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    public class AimController : MonoBehaviour
    {
        [SerializeField] VirtualJoystick aimStick;
        [SerializeField] PlayerController player;
        [SerializeField] Transform muzzle;
        [SerializeField] Projectile projectilePrefab;
        [SerializeField] Transform aimIndicator; // garis/panah di lantai
        [SerializeField] float range = 8f;
        [SerializeField] float fireCooldown = 0.8f;
        [SerializeField] float tapThreshold = 0.2f; // < ini dianggap tap -> auto-aim
        [SerializeField] LayerMask enemyMask;

        float nextFire;
        public int Ammo = 3, MaxAmmo = 3;
        [SerializeField] float reloadTime = 1.5f;
        float reloadTimer;

        // PC Mouse Aiming
        private bool isMouseAiming = false;
        private float mouseHoldTime = 0f;
        private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        private Camera mainCam;

        public float ReloadProgress => Mathf.Clamp01(reloadTimer / reloadTime);

        void Awake()
        {
            if (player == null) player = GetComponent<PlayerController>();
            mainCam = Camera.main;
        }

        void OnEnable()
        {
            if (aimStick != null) aimStick.OnReleased += HandleRelease;
        }

        void OnDisable()
        {
            if (aimStick != null) aimStick.OnReleased -= HandleRelease;
        }

        void Update()
        {
            if (mainCam == null) mainCam = Camera.main;

            // Reload amunisi satu per satu
            if (Ammo < MaxAmmo)
            {
                reloadTimer += Time.deltaTime;
                if (reloadTimer >= reloadTime)
                {
                    Ammo++;
                    reloadTimer = 0;
                }
            }

            bool joystickAiming = aimStick != null && aimStick.IsHeld && aimStick.Direction != Vector2.zero;

            // PC Mouse Aiming support
            if (Input.GetMouseButtonDown(0) && !joystickAiming)
            {
                isMouseAiming = true;
                mouseHoldTime = 0f;
            }

            if (isMouseAiming)
            {
                mouseHoldTime += Time.deltaTime;
            }

            bool aiming = joystickAiming || (isMouseAiming && mouseHoldTime >= tapThreshold);
            if (aimIndicator != null) aimIndicator.gameObject.SetActive(aiming);

            if (aiming)
            {
                Vector3 dir = Vector3.forward;
                if (joystickAiming)
                {
                    dir = new Vector3(aimStick.Direction.x, 0, aimStick.Direction.y).normalized;
                }
                else if (isMouseAiming && mainCam != null)
                {
                    Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                    if (groundPlane.Raycast(ray, out float enter))
                    {
                        Vector3 hitPt = ray.GetPoint(enter);
                        dir = (hitPt - transform.position);
                        dir.y = 0f;
                        if (dir.sqrMagnitude > 0.01f) dir.Normalize();
                        else dir = transform.forward;
                    }
                }

                if (player != null) player.AimOverrideDir = dir;
                if (aimIndicator != null)
                {
                    aimIndicator.position = transform.position + Vector3.up * 0.05f;
                    aimIndicator.rotation = Quaternion.LookRotation(dir);
                    aimIndicator.localScale = new Vector3(1, 1, range);
                }
            }
            else
            {
                if (player != null) player.AimOverrideDir = Vector3.zero;
            }

            // Mouse Release
            if (Input.GetMouseButtonUp(0) && isMouseAiming)
            {
                Vector3 releaseDir = Vector3.zero;
                if (mouseHoldTime < tapThreshold)
                {
                    // Tap cepat mouse = auto-aim ke musuh terdekat
                    if (!TryGetNearestEnemyDir(out releaseDir)) releaseDir = transform.forward;
                }
                else if (mainCam != null)
                {
                    Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                    if (groundPlane.Raycast(ray, out float enter))
                    {
                        Vector3 hitPt = ray.GetPoint(enter);
                        releaseDir = (hitPt - transform.position);
                        releaseDir.y = 0f;
                        if (releaseDir.sqrMagnitude > 0.01f) releaseDir.Normalize();
                        else releaseDir = transform.forward;
                    }
                }

                isMouseAiming = false;
                Fire(releaseDir);
            }
        }

        void HandleRelease(Vector2 stickDir, float holdTime)
        {
            Vector3 dir;
            if (holdTime < tapThreshold || stickDir == Vector2.zero)
            {
                // Tap cepat = auto-aim ke musuh terdekat
                if (!TryGetNearestEnemyDir(out dir)) dir = player != null ? player.transform.forward : transform.forward;
            }
            else
            {
                dir = new Vector3(stickDir.x, 0, stickDir.y).normalized;
            }

            Fire(dir);
        }

        void Fire(Vector3 dir)
        {
            if (Ammo <= 0 || Time.time < nextFire) return;
            Ammo--;
            nextFire = Time.time + fireCooldown;

            Vector3 spawnPos = muzzle != null ? muzzle.position : transform.position + dir * 0.8f + Vector3.up * 0.5f;
            if (projectilePrefab != null)
            {
                var p = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(dir));
                p.Init(range, gameObject);
            }

            // Reveal character dari semak saat menembak
            Hideable h = GetComponent<Hideable>();
            if (h != null) h.RevealFor(2.0f);

            // Trigger camera shake jika ada CameraFollow
            CameraFollow camFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollow>() : null;
            if (camFollow != null) camFollow.Shake(0.12f, 0.1f);
        }

        bool TryGetNearestEnemyDir(out Vector3 dir)
        {
            dir = Vector3.zero;
            int mask = enemyMask != 0 ? enemyMask.value : LayerMask.GetMask("Syafiq_Enemy", "Default");
            var hits = Physics.OverlapSphere(transform.position, range, mask);
            float best = float.MaxValue;
            foreach (var h in hits)
            {
                if (h.gameObject == gameObject || h.transform.IsChildOf(transform)) continue;
                float d = (h.transform.position - transform.position).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    dir = (h.transform.position - transform.position);
                    dir.y = 0;
                    dir.Normalize();
                }
            }
            return best < float.MaxValue;
        }

        public void Setup(VirtualJoystick stick, PlayerController pc, Transform muz, Projectile prefab, Transform indicator, LayerMask mask)
        {
            aimStick = stick;
            player = pc;
            muzzle = muz;
            projectilePrefab = prefab;
            aimIndicator = indicator;
            enemyMask = mask;
        }
    }
}
