using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Pasang di pemain. Tombol Super = VirtualJoystick kedua:
    // drag + lepas = bidik manual, tap cepat = auto-aim.
    [RequireComponent(typeof(SuperMeter))]
    public class SuperController : MonoBehaviour
    {
        [SerializeField] VirtualJoystick superStick;
        [SerializeField] SuperAbility ability;
        [SerializeField] Transform model;              // untuk arah default
        [SerializeField] LayerMask unitMask;
        [SerializeField] float autoAimRange = 8f;
        [SerializeField] float tapThreshold = 0.2f;

        SuperMeter meter;
        GemCarrier carrier;
        Health hp;

        void Awake()
        {
            meter = GetComponent<SuperMeter>();
            carrier = GetComponent<GemCarrier>();
            hp = GetComponent<Health>();
        }

        void OnEnable()
        {
            if (superStick != null) superStick.OnReleased += HandleRelease;
        }

        void OnDisable()
        {
            if (superStick != null) superStick.OnReleased -= HandleRelease;
        }

        public void Setup(VirtualJoystick stick, SuperAbility superAbility, Transform playerModel, LayerMask mask)
        {
            if (superStick != null) superStick.OnReleased -= HandleRelease;
            superStick = stick;
            ability = superAbility;
            model = playerModel;
            unitMask = mask;
            if (superStick != null && enabled) superStick.OnReleased += HandleRelease;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
            {
                HandleRelease(Vector2.zero, 0f); // Quick tap auto-aim Super
            }
        }

        void HandleRelease(Vector2 stick, float holdTime)
        {
            if (hp != null && hp.IsDead) return;
            if (meter == null || !meter.IsReady) return;
            if (ability == null) return;

            Vector3 dir;
            int team = carrier != null ? carrier.teamId : 0;
            if (holdTime < tapThreshold || stick.sqrMagnitude < 0.01f)
            {
                if (!AimUtil.NearestEnemyDir(transform.position, autoAimRange, unitMask, team, out dir))
                    dir = model != null ? model.forward : transform.forward;
            }
            else dir = new Vector3(stick.x, 0, stick.y).normalized;

            if (meter.TryConsume())
            {
                ability.Activate(dir);
            }
        }
    }
}
