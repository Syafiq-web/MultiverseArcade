namespace MultiverseArcade.Core
{
    /// <summary>
    /// Kontrak standar integrasi antara setiap Game individu dengan GameManager global.
    /// Sesuai dengan GDD Bagian 2 (Arsitektur Sistem Global).
    /// </summary>
    public interface IArcadeGame
    {
        /// <summary>
        /// Pengenal unik game (contoh: "syafiq", "jaya", "prima", "brili", "risa").
        /// </summary>
        string GameId { get; }

        /// <summary>
        /// Apakah game saat ini mengizinkan jeda/pause (misal saat playing = true, saat countdown/finished = false).
        /// </summary>
        bool CanPause => true;

        /// <summary>
        /// Dipanggil ketika scene game selesai dimuat dan gameplay dimulai.
        /// </summary>
        void OnGameEnter();

        /// <summary>
        /// Dipanggil saat pemain menekan pause (Esc atau tombol pause global).
        /// </summary>
        void OnGamePause();

        /// <summary>
        /// Dipanggil saat game di-resume dari kondisi pause.
        /// </summary>
        void OnGameResume();

        /// <summary>
        /// Dipanggil saat pemain memilih Restart dari menu pause.
        /// </summary>
        void OnGameRestart();

        /// <summary>
        /// Dipanggil saat pemain meninggalkan game menuju Main Menu atau Game Selection.
        /// Wajib membersihkan state gameplay sebelum scene di-unload.
        /// </summary>
        void OnGameExit();
    }
}
