using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Minimap / Radar Taktis untuk Arena Clash 3D:
    /// Menampilkan posisi real-time Player (Cyan), Teman Tim (Biru), Musuh (Merah),
    /// dan Power-Up (Kuning) di dalam arena 30x30.
    /// </summary>
    public class MinimapRadarUI : MonoBehaviour
    {
        [Header("Radar Settings")]
        [SerializeField] private RectTransform _radarContainer;
        [SerializeField] private float _radarRadius = 65f; // Radius minimap UI dalam pixel
        [SerializeField] private float _arenaHalfSize = 15f; // Setengah ukuran arena 30x30

        [Header("Blip Prefabs / Sprites")]
        [SerializeField] private Sprite _playerBlipSprite;
        [SerializeField] private Sprite _blueBlipSprite;
        [SerializeField] private Sprite _redBlipSprite;
        [SerializeField] private Sprite _powerUpBlipSprite;

        private Transform _playerTransform;
        private List<Health> _allCharacters = new List<Health>();
        private List<PowerUpItem> _allPowerUps = new List<PowerUpItem>();

        private RectTransform _playerBlipRt;
        private readonly List<RectTransform> _characterBlips = new List<RectTransform>();
        private readonly List<RectTransform> _powerUpBlips = new List<RectTransform>();

        public void Setup(Transform player, Sprite playerSpr, Sprite blueSpr, Sprite redSpr, Sprite puSpr)
        {
            _playerTransform = player;
            _playerBlipSprite = playerSpr;
            _blueBlipSprite = blueSpr;
            _redBlipSprite = redSpr;
            _powerUpBlipSprite = puSpr;

            CreateBlips();
        }

        private void Start()
        {
            RefreshEntities();
        }

        public void RefreshEntities()
        {
            _allCharacters.Clear();
            _allCharacters.AddRange(FindObjectsOfType<Health>());

            _allPowerUps.Clear();
            _allPowerUps.AddRange(FindObjectsOfType<PowerUpItem>());

            if (_radarContainer != null && _characterBlips.Count == 0)
            {
                CreateBlips();
            }
        }

        private void CreateBlips()
        {
            if (_radarContainer == null) return;

            // Clear existing
            foreach (Transform child in _radarContainer)
            {
                if (child.name.StartsWith("Blip_"))
                {
                    Destroy(child.gameObject);
                }
            }
            _characterBlips.Clear();
            _powerUpBlips.Clear();

            // Player Blip
            GameObject pGo = new GameObject("Blip_Player");
            pGo.transform.SetParent(_radarContainer, false);
            _playerBlipRt = pGo.AddComponent<RectTransform>();
            _playerBlipRt.sizeDelta = new Vector2(16, 16);
            Image pImg = pGo.AddComponent<Image>();
            pImg.sprite = _playerBlipSprite;
            pImg.color = new Color(0f, 1f, 0.95f, 1f);

            // Other character blips (up to 5 bots)
            for (int i = 0; i < 6; i++)
            {
                GameObject cGo = new GameObject($"Blip_Char_{i}");
                cGo.transform.SetParent(_radarContainer, false);
                RectTransform rt = cGo.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(12, 12);
                Image img = cGo.AddComponent<Image>();
                img.sprite = _blueBlipSprite;
                img.color = Color.white;
                cGo.SetActive(false);
                _characterBlips.Add(rt);
            }

            // Power up blips (up to 4)
            for (int i = 0; i < 4; i++)
            {
                GameObject puGo = new GameObject($"Blip_PU_{i}");
                puGo.transform.SetParent(_radarContainer, false);
                RectTransform rt = puGo.AddComponent<RectTransform>();
                rt.sizeDelta = new Vector2(10, 10);
                Image img = puGo.AddComponent<Image>();
                img.sprite = _powerUpBlipSprite != null ? _powerUpBlipSprite : _blueBlipSprite;
                img.color = new Color(1f, 0.85f, 0.1f, 0.9f);
                puGo.SetActive(false);
                _powerUpBlips.Add(rt);
            }
        }

        private void Update()
        {
            if (_playerTransform == null)
            {
                var pc = FindObjectOfType<PlayerController>();
                if (pc != null) _playerTransform = pc.transform;
            }

            UpdatePlayerBlip();
            UpdateCharacterBlips();
            UpdatePowerUpBlips();
        }

        private void UpdatePlayerBlip()
        {
            if (_playerBlipRt == null || _playerTransform == null) return;
            Vector2 mapPos = WorldToMinimap(_playerTransform.position);
            _playerBlipRt.anchoredPosition = mapPos;
            _playerBlipRt.localRotation = Quaternion.Euler(0, 0, -_playerTransform.eulerAngles.y);
        }

        private void UpdateCharacterBlips()
        {
            int blipIdx = 0;
            for (int i = 0; i < _allCharacters.Count; i++)
            {
                Health h = _allCharacters[i];
                if (h == null || h.IsPlayer || h.CurrentHealth <= 0 || !h.gameObject.activeInHierarchy) continue;
                if (blipIdx >= _characterBlips.Count) break;

                RectTransform blip = _characterBlips[blipIdx];
                blip.gameObject.SetActive(true);
                blip.anchoredPosition = WorldToMinimap(h.transform.position);

                Image img = blip.GetComponent<Image>();
                if (img != null)
                {
                    if (h.TeamId == 0)
                    {
                        img.sprite = _blueBlipSprite;
                        img.color = new Color(0f, 0.75f, 1f, 1f);
                    }
                    else
                    {
                        img.sprite = _redBlipSprite;
                        img.color = new Color(1f, 0.25f, 0.25f, 1f);
                    }
                }

                blipIdx++;
            }

            for (int i = blipIdx; i < _characterBlips.Count; i++)
            {
                _characterBlips[i].gameObject.SetActive(false);
            }
        }

        private void UpdatePowerUpBlips()
        {
            int puIdx = 0;
            for (int i = 0; i < _allPowerUps.Count; i++)
            {
                PowerUpItem pu = _allPowerUps[i];
                if (pu == null || !pu.gameObject.activeInHierarchy) continue;
                if (puIdx >= _powerUpBlips.Count) break;

                RectTransform blip = _powerUpBlips[puIdx];
                blip.gameObject.SetActive(true);
                blip.anchoredPosition = WorldToMinimap(pu.transform.position);
                puIdx++;
            }

            for (int i = puIdx; i < _powerUpBlips.Count; i++)
            {
                _powerUpBlips[i].gameObject.SetActive(false);
            }
        }

        private Vector2 WorldToMinimap(Vector3 worldPos)
        {
            float normX = Mathf.Clamp(worldPos.x / _arenaHalfSize, -1f, 1f);
            float normZ = Mathf.Clamp(worldPos.z / _arenaHalfSize, -1f, 1f);
            return new Vector2(normX * _radarRadius, normZ * _radarRadius);
        }
    }
}
