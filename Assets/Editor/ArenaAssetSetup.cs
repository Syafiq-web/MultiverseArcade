#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MultiverseArcade.EditorTools
{
    /// <summary>
    /// Utility untuk mengonfigurasi TextureImporter dan ModelImporter aset 3D & UI Arena Clash.
    /// Memastikan semua PNG di folder UI/Sprites diimpor sebagai Sprite (2D and UI)
    /// dan semua Skin Texture siap untuk URP Material.
    /// </summary>
    public static class ArenaAssetSetup
    {
        private const string UI_SPRITES_DIR = "Assets/Games/Syafiq/UI/Sprites";
        private const string SKINS_DIR = "Assets/Games/Syafiq/Textures/Skins";
        private const string MODELS_DIR = "Assets/Games/Syafiq/Models/Characters";

        [MenuItem("Tools/Multiverse Arcade/Configure Arena Clash Assets", false, 10)]
        public static void ConfigureAllAssets()
        {
            ConfigureUISprites();
            ConfigureSkinTextures();
            ConfigureCharacterModels();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ArenaAssetSetup] Konfigurasi Texture & Model Arena Clash selesai!");
        }

        public static void ConfigureUISprites()
        {
            if (!Directory.Exists(UI_SPRITES_DIR)) return;

            string[] files = Directory.GetFiles(UI_SPRITES_DIR, "*.png", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string assetPath = file.Replace('\\', '/');
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null)
                {
                    bool changed = false;
                    if (importer.textureType != TextureImporterType.Sprite)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.spriteImportMode = SpriteImportMode.Single;
                        changed = true;
                    }
                    if (!importer.alphaIsTransparency)
                    {
                        importer.alphaIsTransparency = true;
                        changed = true;
                    }
                    if (importer.mipmapEnabled)
                    {
                        importer.mipmapEnabled = false;
                        changed = true;
                    }
                    if (changed)
                    {
                        EditorUtility.SetDirty(importer);
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        public static void ConfigureSkinTextures()
        {
            if (!Directory.Exists(SKINS_DIR)) return;

            string[] files = Directory.GetFiles(SKINS_DIR, "*.png", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string assetPath = file.Replace('\\', '/');
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (importer != null)
                {
                    bool changed = false;
                    if (importer.textureType != TextureImporterType.Default)
                    {
                        importer.textureType = TextureImporterType.Default;
                        changed = true;
                    }
                    if (importer.wrapMode != TextureWrapMode.Clamp)
                    {
                        importer.wrapMode = TextureWrapMode.Clamp;
                        changed = true;
                    }
                    if (changed)
                    {
                        EditorUtility.SetDirty(importer);
                        importer.SaveAndReimport();
                    }
                }
            }
        }

        public static void ConfigureCharacterModels()
        {
            if (!Directory.Exists(MODELS_DIR)) return;

            string[] files = Directory.GetFiles(MODELS_DIR, "*.fbx", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string assetPath = file.Replace('\\', '/');
                ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (importer != null)
                {
                    bool changed = false;
                    if (!importer.isReadable)
                    {
                        importer.isReadable = true;
                        changed = true;
                    }
                    if (changed)
                    {
                        EditorUtility.SetDirty(importer);
                        importer.SaveAndReimport();
                    }
                }
            }
        }
    }
}
#endif
