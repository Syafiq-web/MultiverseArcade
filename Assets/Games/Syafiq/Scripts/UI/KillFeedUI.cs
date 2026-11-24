using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Menampilkan notifikasi eliminasi di sudut kanan atas layar.
    /// Sesuai GDD Bagian 5.11.
    /// </summary>
    public class KillFeedUI : MonoBehaviour
    {
        [SerializeField] private GameObject _itemPrefab;
        [SerializeField] private Transform _container;
        [SerializeField] private int _maxEntries = 4;

        private readonly List<GameObject> _activeEntries = new List<GameObject>();

        private void OnEnable()
        {
            ArenaEvents.OnCharacterDied += HandleCharacterDied;
        }

        private void OnDisable()
        {
            ArenaEvents.OnCharacterDied -= HandleCharacterDied;
        }

        public void BindContainer(Transform container, GameObject itemPrefab)
        {
            _container = container;
            _itemPrefab = itemPrefab;
        }

        private void HandleCharacterDied(Health victim, int killerTeamId, string killerName)
        {
            if (_container == null || _itemPrefab == null) return;

            GameObject entry = Instantiate(_itemPrefab, _container);
            entry.SetActive(true);

            TextMeshProUGUI tmp = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null)
            {
                string killerColorHex = killerTeamId == 0 ? "#00E5FF" : "#FF5252";
                string victimColorHex = victim.TeamId == 0 ? "#00E5FF" : "#FF5252";

                tmp.text = $"<color={killerColorHex}>{killerName}</color>  >  <color={victimColorHex}>{victim.CharacterName}</color>";
            }

            _activeEntries.Add(entry);
            Destroy(entry, 4.0f);

            if (_activeEntries.Count > _maxEntries)
            {
                if (_activeEntries[0] != null) Destroy(_activeEntries[0]);
                _activeEntries.RemoveAt(0);
            }
        }
    }
}
