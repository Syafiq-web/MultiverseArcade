using System;
using System.Collections.Generic;

namespace MultiverseArcade.Core
{
    [Serializable]
    public class GameHighScoreEntry
    {
        public string gameId;
        public int highScore;

        public GameHighScoreEntry() { }

        public GameHighScoreEntry(string gameId, int highScore)
        {
            this.gameId = gameId;
            this.highScore = highScore;
        }
    }

    /// <summary>
    /// Struktur data penyimpanan terpusat Multiverse Arcade berbasis JSON berversi.
    /// Sesuai GDD Bagian 2.2.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;

        // Pengaturan Audio
        public float masterVolume = 0.8f;
        public float musicVolume = 0.7f;
        public float sfxVolume = 0.9f;

        // Pengaturan Grafis (0: Low, 1: Medium, 2: High)
        public int graphicQuality = 2;

        // Progres Sesi & Permainan
        public string lastPlayedGameId = "syafiq";
        public List<GameHighScoreEntry> highScores = new List<GameHighScoreEntry>();
    }
}
