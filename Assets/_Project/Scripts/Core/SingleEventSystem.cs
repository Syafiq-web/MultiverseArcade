using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiverseArcade.Core
{
    /// <summary>
    /// Memastikan hanya ada TEPAT SATU EventSystem yang aktif di seluruh aplikasi,
    /// baik saat game dijalankan dari Bootstrap maupun scene Game_Syafiq dibuka langsung di editor.
    /// Mencegah EventSystem ganda dan konflik input module.
    /// Sesuai standar teknis Unity StandaloneInputModule (Active Input Handling = Both).
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public class SingleEventSystem : MonoBehaviour
    {
        public static SingleEventSystem Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // Sudah ada EventSystem aktif dari scene sebelumnya / persistent manager
                Destroy(gameObject);
                return;
            }

            // Periksa juga apakah ada EventSystem lain yang sudah aktif di scene
            EventSystem[] existingSystems = FindObjectsOfType<EventSystem>();
            for (int i = 0; i < existingSystems.Length; i++)
            {
                if (existingSystems[i] != null && existingSystems[i].gameObject != gameObject)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
