using UnityEngine;
using TMPro;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Angka damage melayang saat serangan proyektil mengenai karakter.
    /// Sesuai GDD Bagian 5.11.
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private float _floatSpeed = 1.8f;
        [SerializeField] private float _duration = 0.6f;

        private float _timer = 0f;
        private Camera _mainCam;
        private Color _initColor;

        private void Awake()
        {
            _mainCam = Camera.main;
            if (_text == null) _text = GetComponentInChildren<TextMeshProUGUI>();
            if (_text != null) _initColor = _text.color;
        }

        public void Spawn(Vector3 worldPos, int damage, bool isCrit = false)
        {
            transform.position = worldPos + Vector3.up * 0.5f;
            _timer = 0f;

            if (_text != null)
            {
                _text.text = damage.ToString();
                _text.color = isCrit ? Color.yellow : Color.white;
                _text.fontSize = isCrit ? 26 : 20;
            }

            gameObject.SetActive(true);
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            transform.position += Vector3.up * (_floatSpeed * Time.deltaTime);

            if (_mainCam != null)
            {
                transform.rotation = _mainCam.transform.rotation;
            }

            if (_text != null)
            {
                float alpha = Mathf.Clamp01(1f - (_timer / _duration));
                Color c = _text.color;
                c.a = alpha;
                _text.color = c;
            }

            if (_timer >= _duration)
            {
                Destroy(gameObject);
            }
        }
    }
}
