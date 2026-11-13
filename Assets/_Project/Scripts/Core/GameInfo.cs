using UnityEngine;

namespace MultiverseArcade.Core
{
    /// <summary>
    /// Metadata sebuah game di dalam Multiverse Arcade.
    /// Sesuai GDD Bagian 2.5.
    /// </summary>
    [CreateAssetMenu(fileName = "GameInfo_", menuName = "Multiverse Arcade/Game Info", order = 1)]
    public class GameInfo : ScriptableObject
    {
        [Tooltip("ID unik game (contoh: syafiq, jaya, prima, brili, risa)")]
        public string id;

        [Tooltip("Nama tampilan judul game pada kartu")]
        public string displayName;

        [Tooltip("Nama scene gameplay yang dipanggil")]
        public string sceneName;

        [Tooltip("Ikon / gambar thumbnail kartu")]
        public Sprite thumbnail;

        [Tooltip("Genre game")]
        public string genre;

        [TextArea(2, 5)]
        [Tooltip("Deskripsi singkat tentang game")]
        public string description;

        [TextArea(2, 4)]
        [Tooltip("Panduan kontrol dasar")]
        public string controlsHint;

        [Tooltip("Status ketersediaan game. Jika false, kartu akan menampilkan status Coming Soon")]
        public bool isEnabled;
    }
}
