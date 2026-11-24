using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] VirtualJoystick aimStick;

        [Header("Posisi & Sudut")]
        [SerializeField] float height = 14f;
        [SerializeField] float distanceBack = 9f;      // mundur di sumbu Z
        [SerializeField] float pitch = 55f;            // sudut miring (top-down miring)

        [Header("Kehalusan")]
        [SerializeField] float smoothTime = 0.15f;
        [SerializeField] float lookAheadAmount = 2.5f; // geser kamera ke arah bidik

        [Header("Batas Map (world XZ)")]
        [SerializeField] Vector2 minBounds = new Vector2(-15, -15);
        [SerializeField] Vector2 maxBounds = new Vector2(15, 15);

        Vector3 velocity;
        float shakeTime, shakePower;

        void Start() => transform.rotation = Quaternion.Euler(pitch, 0, 0);

        void LateUpdate()   // LateUpdate: setelah player selesai bergerak
        {
            if (target == null) return;
            Vector3 focus = target.position;

            // Look-ahead: kamera sedikit bergeser ke arah bidik
            if (aimStick != null && aimStick.IsHeld)
            {
                focus += new Vector3(aimStick.Direction.x, 0, aimStick.Direction.y) * lookAheadAmount;
            }

            // Batasi fokus supaya kamera tidak keluar map
            focus.x = Mathf.Clamp(focus.x, minBounds.x, maxBounds.x);
            focus.z = Mathf.Clamp(focus.z, minBounds.y, maxBounds.y);

            Vector3 desired = focus + new Vector3(0, height, -distanceBack);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);

            // Screen shake
            if (shakeTime > 0)
            {
                transform.position += Random.insideUnitSphere * shakePower;
                shakeTime -= Time.deltaTime;
            }
        }

        public void Shake(float duration = 0.2f, float power = 0.15f)
        {
            shakeTime = duration;
            shakePower = power;
        }

        public void SetTarget(Transform t, VirtualJoystick stick = null)
        {
            target = t;
            aimStick = stick;
        }
    }
}
