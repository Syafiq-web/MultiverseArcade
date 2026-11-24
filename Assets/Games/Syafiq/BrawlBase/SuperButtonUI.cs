using UnityEngine;
using UnityEngine.UI;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Pasang di tombol Super. "fill" = Image dengan Image Type: Filled, Fill Method: Radial 360.
    public class SuperButtonUI : MonoBehaviour
    {
        [SerializeField] SuperMeter meter;
        [SerializeField] Image fill;
        [SerializeField] Image glow;      // opsional: lingkaran yang berdenyut saat siap
        [SerializeField] Color chargingColor = new Color(0.55f, 0.45f, 0.05f);
        [SerializeField] Color readyColor = new Color(1f, 0.85f, 0.1f);

        void OnEnable()
        {
            if (meter != null)
            {
                meter.OnChanged += Refresh;
                Refresh(meter.Value01);
            }
        }

        void OnDisable()
        {
            if (meter != null) meter.OnChanged -= Refresh;
        }

        public void Setup(SuperMeter superMeter, Image fillImage, Image glowImage = null)
        {
            if (meter != null) meter.OnChanged -= Refresh;
            meter = superMeter;
            fill = fillImage;
            glow = glowImage;
            if (meter != null && enabled)
            {
                meter.OnChanged += Refresh;
                Refresh(meter.Value01);
            }
        }

        void Refresh(float v)
        {
            if (fill != null)
            {
                fill.fillAmount = v;
                fill.color = (meter != null && meter.IsReady) ? readyColor : chargingColor;
            }
            if (glow != null)
            {
                glow.enabled = (meter != null && meter.IsReady);
            }
        }

        void Update()
        {
            if (glow != null && meter != null && meter.IsReady)
            {
                glow.rectTransform.localScale = Vector3.one * (1f + 0.08f * Mathf.Sin(Time.time * 8f));
            }
        }
    }
}
