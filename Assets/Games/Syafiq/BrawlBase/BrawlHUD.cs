using UnityEngine;
using UnityEngine.UI;

namespace MultiverseArcade.Syafiq.Brawl
{
    public class BrawlHUD : MonoBehaviour
    {
        [Header("Ammo Bar (3 Segments)")]
        [SerializeField] private Image[] ammoSegments;
        [SerializeField] private Image reloadOverlay;

        [Header("Player Health Bar")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Text healthText;
        [SerializeField] private Text playerGemsText;

        [Header("Gem Grab Match Status")]
        [SerializeField] private Text blueGemsText;
        [SerializeField] private Text redGemsText;
        [SerializeField] private GameObject countdownBanner;
        [SerializeField] private Text countdownText;
        [SerializeField] private GameObject matchEndBanner;
        [SerializeField] private Text matchEndTitle;

        private AimController aimCtrl;
        private Health playerHealth;
        private GemCarrier playerCarrier;
        private GemGrabManager gemManager;

        public void Bind(AimController aim, Health health, GemCarrier carrier, GemGrabManager manager)
        {
            aimCtrl = aim;
            playerHealth = health;
            playerCarrier = carrier;
            gemManager = manager;

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += HandleHealthChanged;
                HandleHealthChanged(playerHealth.Current, playerHealth.Max);
            }

            if (playerCarrier != null)
            {
                playerCarrier.OnChanged += HandleCarrierChanged;
                HandleCarrierChanged(playerCarrier);
            }

            if (gemManager != null)
            {
                gemManager.OnGemsChanged += HandleGemsChanged;
                gemManager.OnCountdown += HandleCountdown;
                gemManager.OnMatchEnd += HandleMatchEnd;
            }

            if (countdownBanner != null) countdownBanner.SetActive(false);
            if (matchEndBanner != null) matchEndBanner.SetActive(false);
        }

        private void OnDestroy()
        {
            if (playerHealth != null) playerHealth.OnHealthChanged -= HandleHealthChanged;
            if (playerCarrier != null) playerCarrier.OnChanged -= HandleCarrierChanged;
            if (gemManager != null)
            {
                gemManager.OnGemsChanged -= HandleGemsChanged;
                gemManager.OnCountdown -= HandleCountdown;
                gemManager.OnMatchEnd -= HandleMatchEnd;
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

        private void HandleCarrierChanged(GemCarrier carrier)
        {
            if (playerGemsText != null && carrier != null)
            {
                playerGemsText.text = $"Gems: {carrier.Gems}";
            }
        }

        private void HandleGemsChanged(int blue, int red)
        {
            if (blueGemsText != null) blueGemsText.text = $"{blue}";
            if (redGemsText != null) redGemsText.text = $"{red}";
        }

        private void HandleCountdown(int leadingTeam, float seconds)
        {
            if (countdownBanner == null) return;

            if (leadingTeam < 0 || seconds <= 0)
            {
                countdownBanner.SetActive(false);
            }
            else
            {
                countdownBanner.SetActive(true);
                string teamName = leadingTeam == 0 ? "BLUE TEAM" : "RED TEAM";
                if (countdownText != null)
                {
                    countdownText.text = $"{teamName} COUNTDOWN: {Mathf.CeilToInt(seconds)}s";
                    countdownText.color = leadingTeam == 0 ? new Color(0f, 0.85f, 1f) : new Color(1f, 0.3f, 0.3f);
                }
            }
        }

        private void HandleMatchEnd(int winningTeam)
        {
            if (matchEndBanner != null)
            {
                matchEndBanner.SetActive(true);
                bool playerWon = winningTeam == 0;
                if (matchEndTitle != null)
                {
                    matchEndTitle.text = playerWon ? "VICTORY!" : "DEFEAT";
                    matchEndTitle.color = playerWon ? new Color(0f, 1f, 0.5f) : new Color(1f, 0.25f, 0.25f);
                }
            }
        }
    }
}
