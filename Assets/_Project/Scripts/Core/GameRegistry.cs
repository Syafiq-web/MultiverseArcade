using System;
using System.Collections.Generic;
using UnityEngine;

namespace MultiverseArcade.Core
{
    /// <summary>
    /// Registry terpusat yang memuat seluruh katalog game di Multiverse Arcade.
    /// Digunakan oleh GameSelectionUI untuk menghasilkan kartu secara otomatis.
    /// Sesuai GDD Bagian 2.5.
    /// </summary>
    [CreateAssetMenu(fileName = "GameRegistry", menuName = "Multiverse Arcade/Game Registry", order = 0)]
    public class GameRegistry : ScriptableObject
    {
        [Tooltip("Daftar game yang terdaftar di Multiverse Arcade")]
        public List<GameInfo> games = new List<GameInfo>();

        /// <summary>
        /// Mengambil GameInfo berdasarkan ID (case-insensitive).
        /// </summary>
        public GameInfo GetGame(string id)
        {
            if (string.IsNullOrEmpty(id) || games == null) return null;
            return games.Find(g => g != null && string.Equals(g.id, id, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Mengambil GameInfo berdasarkan nama scene.
        /// </summary>
        public GameInfo GetGameByScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName) || games == null) return null;
            return games.Find(g => g != null && string.Equals(g.sceneName, sceneName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
