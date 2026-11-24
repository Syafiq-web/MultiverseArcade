using UnityEngine;
using UnityEngine.UI;

namespace MultiverseArcade.Syafiq.Brawl
{
    public class BrawlHUD : MonoBehaviour
    {
        [Header("Ammo Bar (3 Segments)")]
        [SerializeField] private Image[] ammoSegments;
        [SerializeField] private Image reloadOverlay;

        [Header("Health Bar")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Text healthText;

        private AimController aimCtrl;
        private Health playerHealth;

        public void Bind(AimController aim, Health health)
        {
            aimCtrl = aim;
            playerHealth = health;
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += HandleHealthChanged;
                HandleHealthChanged(playerHealth.Current, playerHealth.Max);
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void Update()
        {
            if (aimCtrl == null) return;

            // 3 Ammo segments
            if (ammoSegments != null)
            {
                for (int i = 0; i < ammoSegments.Length; i++)
                {
                    if (ammoSegments[i] != null)
                    {
                        ammoSegments[i].color = (i < aimCtrl.Ammo)
                            ? new Color(1f, 0.65f, 0f, 1f) // Bright Orange Ammo
                            : new Color(0.25f, 0.25f, 0.25f, 0.6f); // Empty
                    }
                }
            }

            // Reload Progress indicator
            if (reloadOverlay != null)
            {
                reloadOverlay.fillAmount = aimCtrl.Ammo < aimCtrl.MaxAmmo ? aimCtrl.ReloadProgress : 1f;
            }
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (healthSlider != null)
            {
                healthSlider.maxValue = max;
                healthSlider.value = current;
            }
            if (healthText != null)
            {
                healthText.text = $"{current} / {max}";
            }
        }
    }
}
