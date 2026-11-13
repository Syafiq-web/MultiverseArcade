using System;
using System.IO;
using UnityEngine;

namespace MultiverseArcade.Core
{
    /// <summary>
    /// Manager persistensi JSON di Application.persistentDataPath dengan migrasi versi.
    /// Sesuai GDD Bagian 2.2.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SAVE_FILE_NAME = "multiverse_save.json";
        private const int CURRENT_VERSION = 1;

        public SaveData Data { get; private set; } = new SaveData();

        private string SaveFilePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Load();
        }

        /// <summary>
        /// Memuat data dari file JSON, dengan fallback nilai default jika file belum ada.
        /// </summary>
        public void Load()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    SaveData loadedData = JsonUtility.FromJson<SaveData>(json);

                    if (loadedData != null)
                    {
                        Data = loadedData;
                        MigrateIfNeeded();
                        Debug.Log($"[SaveManager] Data berhasil dimuat dari: {SaveFilePath}");
                    }
                    else
                    {
                        Data = new SaveData();
                        Save();
                    }
                }
                else
                {
                    Debug.Log("[SaveManager] File save belum ditemukan. Membuat konfigurasi default baru.");
                    Data = new SaveData();
                    Save();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveManager] Gagal membaca save data: {ex.Message}. Menggunakan default.");
                Data = new SaveData();
            }

            EventBus.RaiseSaveLoaded();
        }

        /// <summary>
        /// Menyimpan data saat ini ke dalam format JSON.
        /// </summary>
        public void Save()
        {
            try
            {
                Data.version = CURRENT_VERSION;
                string json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(SaveFilePath, json);
                Debug.Log($"[SaveManager] Data berhasil disimpan (v{Data.version}).");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Gagal menyimpan data: {ex.Message}");
            }
        }

        private void MigrateIfNeeded()
        {
            if (Data.version < CURRENT_VERSION)
            {
                Debug.Log($"[SaveManager] Melakukan migrasi save data dari v{Data.version} ke v{CURRENT_VERSION}");
                // Logika migrasi skema di masa depan ditaruh di sini
                Data.version = CURRENT_VERSION;
                Save();
            }
        }

        public int GetHighScore(string gameId)
        {
            if (string.IsNullOrEmpty(gameId) || Data.highScores == null) return 0;
            var entry = Data.highScores.Find(e => string.Equals(e.gameId, gameId, StringComparison.OrdinalIgnoreCase));
            return entry != null ? entry.highScore : 0;
        }

        public bool SetHighScore(string gameId, int score)
        {
            if (string.IsNullOrEmpty(gameId)) return false;
            if (Data.highScores == null) Data.highScores = new System.Collections.Generic.List<GameHighScoreEntry>();

            var entry = Data.highScores.Find(e => string.Equals(e.gameId, gameId, StringComparison.OrdinalIgnoreCase));
            if (entry == null)
            {
                Data.highScores.Add(new GameHighScoreEntry(gameId, score));
                Save();
                return true;
            }
            else if (score > entry.highScore)
            {
                entry.highScore = score;
                Save();
                return true;
            }

            return false;
        }

        public void SetLastPlayedGame(string gameId)
        {
            if (!string.IsNullOrEmpty(gameId))
            {
                Data.lastPlayedGameId = gameId;
                Save();
            }
        }
    }
}
