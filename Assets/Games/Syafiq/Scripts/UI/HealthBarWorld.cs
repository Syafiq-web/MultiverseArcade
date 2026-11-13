using UnityEngine;
using UnityEngine.UI;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Health bar world-space di atas kepala karakter dengan orientasi billboard menghadap kamera.
    /// Sesuai GDD Bagian 5.11.
    /// </summary>
    public class HealthBarWorld : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private Image _fillImage;
        [SerializeField] private Vector3 _offset = new Vector3(0, 2.2f, 0);

        private Health _targetHealth;
        private Camera _mainCam;
        private Transform _targetTransform;

        private void Awake()
        {
            _mainCam = Camera.main;
        }

        public void Setup(Health health, Transform targetTransform, Color teamColor)
        {
            _targetHealth = health;
            _targetTransform = targetTransform;

            if (_fillImage != null)
            {
                _fillImage.color = teamColor;
            }

            if (_slider != null)
            {
                _slider.minValue = 0;
                _slider.maxValue = health != null ? health.MaxHealth : 100;
                _slider.value = health != null ? health.CurrentHealth : 100;
            }

            ArenaEvents.OnHealthChanged += HandleHealthChanged;
        }

        private void OnDestroy()
        {
            ArenaEvents.OnHealthChanged -= HandleHealthChanged;
        }

        private void HandleHealthChanged(Health h, int current, int max)
        {
            if (h == _targetHealth && _slider != null)
            {
                _slider.maxValue = max;
                _slider.value = current;
                gameObject.SetActive(!h.IsDead && current > 0);
            }
        }

        private void LateUpdate()
        {
            if (_targetTransform == null || (_targetHealth != null && _targetHealth.IsDead))
            {
                if (gameObject.activeSelf) gameObject.SetActive(false);
                return;
            }

            if (!gameObject.activeSelf) gameObject.SetActive(true);

            transform.position = _targetTransform.position + _offset;

            // Billboard menghadap kamera
            if (_mainCam != null)
            {
                transform.rotation = _mainCam.transform.rotation;
            }
        }
    }
}
