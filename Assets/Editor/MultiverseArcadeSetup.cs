#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MultiverseArcade.EditorTools
{
    /// <summary>
    /// Script Editor untuk mengotomatisasi konfigurasi awal Multiverse Arcade.
    /// Mendaftarkan seluruh 7 scene ke EditorBuildSettings secara otomatis.
    /// </summary>
    public static class MultiverseArcadeSetup
    {
        private static readonly string[] RequiredScenePaths = new string[]
        {
            "Assets/_Project/Scenes/MainMenu.unity",
            "Assets/_Project/Scenes/GameSelection.unity",
            "Assets/Games/Syafiq/Scenes/Game_Syafiq.unity",
            "Assets/Games/Jaya/Scenes/Game_Jaya.unity",
            "Assets/Games/Prima/Scenes/Game_Prima.unity",
            "Assets/Games/Brili/Scenes/Game_Brili.unity",
            "Assets/Games/Risa/Scenes/Game_Risa.unity"
        };

        [MenuItem("Tools/Multiverse Arcade/Setup Build Scenes")]
        public static void SetupBuildScenes()
        {
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

            foreach (string scenePath in RequiredScenePaths)
            {
                if (System.IO.File.Exists(scenePath))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    Debug.Log($"[MultiverseArcadeSetup] Menambahkan scene ke Build Settings: {scenePath}");
                }
                else
                {
                    Debug.LogWarning($"[MultiverseArcadeSetup] File scene tidak ditemukan: {scenePath}");
                }
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("[MultiverseArcadeSetup] Berhasil mendaftarkan seluruh Scene ke Build Settings!");
        }
    }
}
#endif
