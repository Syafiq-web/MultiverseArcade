using UnityEngine;
using UnityEngine.UI;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Health bar world-space di atas kepala karakter dengan orientasi billboard menghadap kamera.
    /// Dilengkapi proteksi raycast blocking, sinkronisasi warna tim, serta handling respawn & rematch.
    /// Sesuai GDD Bagian 5.11 dan bug fix spesifikasi.
    /// </summary>
    public class HealthBarWorld : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider _slider;
        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Vector3 _offset = new Vector3(0, 2.2f, 0);

        [Header("Target Tracking")]
        [SerializeField] private Health _targetHealth;
        [SerializeField] private Transform _targetTransform;

        private Camera _mainCam;
        private static readonly Color TeamBlueColor = new Color(0f, 0.85f, 1f);
        private static readonly Color TeamRedColor = new Color(1f, 0.25f, 0.25f);

        private void Awake()
        {
            if (_canvas == null) _canvas = GetComponent<Canvas>();
            if (_slider == null) _slider = GetComponent<Slider>();
            if (_targetTransform == null) _targetTransform = transform.parent;
            if (_targetHealth == null) _targetHealth = GetComponentInParent<Health>();
            _mainCam = Camera.main;

            // BUG FIX 2: Hapus GraphicRaycaster jika ada agar tidak mencegat raycast mouse UI
            var raycaster = GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                Destroy(raycaster);
            }

            // BUG FIX 2: Matikan Raycast Target pada background dan fill image
            if (_fillImage != null) _fillImage.raycastTarget = false;
            if (_backgroundImage != null) _backgroundImage.raycastTarget = false;
            if (_slider != null)
            {
                _slider.interactable = false;
                _slider.transition = Selectable.Transition.None;
            }
        }

        private void OnEnable()
        {
            if (_targetTransform == null) _targetTransform = transform.parent;
            if (_targetHealth == null) _targetHealth = GetComponentInParent<Health>();

            ArenaEvents.OnHealthChanged += HandleHealthChanged;

            // Pastikan data dan visual sinkron saat aktif (misal respawn / rematch)
            Refresh();
        }

        private void OnDisable()
        {
            ArenaEvents.OnHealthChanged -= HandleHealthChanged;
        }

        public void Setup(Health health, Transform targetTransform, Color teamColor)
        {
            _targetHealth = health;
            _targetTransform = targetTransform;

            if (_fillImage != null)
            {
                _fillImage.color = teamColor;
                _fillImage.raycastTarget = false;
            }

            if (_backgroundImage != null)
            {
                _backgroundImage.raycastTarget = false;
            }

            if (_slider != null)
            {
                _slider.interactable = false;
                _slider.minValue = 0;
                _slider.maxValue = health != null ? health.MaxHealth : 100;
                _slider.value = health != null ? health.CurrentHealth : 100;
            }

            Refresh();
        }

        public void Refresh()
        {
            if (_targetHealth == null)
            {
                _targetHealth = GetComponentInParent<Health>();
            }

            if (_targetTransform == null)
            {
                _targetTransform = transform.parent;
            }

            if (_targetHealth != null)
            {
                // Warna tim: Tim 0 (Blue) = Cyan/Biru, Tim 1 (Red) = Merah
                if (_fillImage != null)
                {
                    _fillImage.color = _targetHealth.TeamId == 0 ? TeamBlueColor : TeamRedColor;
                    _fillImage.raycastTarget = false;
                }

                if (_backgroundImage != null)
                {
                    _backgroundImage.raycastTarget = false;
                }

                if (_slider != null)
                {
                    _slider.interactable = false;
                    _slider.minValue = 0;
                    _slider.maxValue = _targetHealth.MaxHealth;
                    _slider.value = _targetHealth.CurrentHealth;
                }

                bool isAlive = !_targetHealth.IsDead && _targetHealth.CurrentHealth > 0;
                SetVisibility(isAlive);
            }
        }

        private void HandleHealthChanged(Health h, int current, int max)
        {
            if (h == _targetHealth && _slider != null)
            {
                _slider.maxValue = max;
                _slider.value = current;

                bool isAlive = !h.IsDead && current > 0;
                SetVisibility(isAlive);
            }
        }

        private void SetVisibility(bool visible)
        {
            if (_canvas != null)
            {
                _canvas.enabled = visible;
            }
            else
            {
                gameObject.SetActive(visible);
            }
        }

        private void LateUpdate()
        {
            if (_targetTransform == null)
            {
                _targetTransform = transform.parent;
                if (_targetTransform == null) return;
            }

            if (_targetHealth == null)
            {
                _targetHealth = GetComponentInParent<Health>();
            }

            if (_targetHealth != null && (_targetHealth.IsDead || _targetHealth.CurrentHealth <= 0))
            {
                SetVisibility(false);
                return;
            }

            SetVisibility(true);

            // Posisi di atas kepala karakter
            transform.position = _targetTransform.position + _offset;

            // Billboard selalu menghadap kamera utama
            if (_mainCam == null) _mainCam = Camera.main;
            if (_mainCam != null)
            {
                transform.rotation = _mainCam.transform.rotation;
            }
        }
    }
}
