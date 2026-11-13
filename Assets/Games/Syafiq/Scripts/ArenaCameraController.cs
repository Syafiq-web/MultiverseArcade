using UnityEngine;

namespace MultiverseArcade.Games.Syafiq
{
    /// <summary>
    /// Kamera isometrik top-down yang mengikuti posisi Player secara mulus
    /// dilengkapi dengan efek Screen Shake saat Player terkena tembakan/serangan.
    /// </summary>
    public class ArenaCameraController : MonoBehaviour
    {
        public static ArenaCameraController Instance { get; private set; }

        [Header("Target & Offset")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 16f, -11f);
        [SerializeField] private float smoothSpeed = 10f;

        [Header("Camera Angle")]
        [SerializeField] private float pitchAngle = 55f;

        private Vector3 currentVelocity;
        private float shakeTimeRemaining = 0f;
        private float shakeMagnitude = 0f;

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
            transform.rotation = Quaternion.Euler(pitchAngle, 0f, 0f);

            if (target == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null) target = player.transform;
                else return;
            }

            Vector3 targetPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, 1f / smoothSpeed);

            // Efek Screen Shake jika aktif
            if (shakeTimeRemaining > 0)
            {
                Vector3 randomOffset = Random.insideUnitSphere * shakeMagnitude;
                randomOffset.y = 0; // Jaga stabilitas vertikal
                transform.position += randomOffset;
                shakeTimeRemaining -= Time.deltaTime;
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void TriggerShake(float duration = 0.2f, float magnitude = 0.35f)
        {
            shakeTimeRemaining = duration;
            shakeMagnitude = magnitude;
        }
    }
}
