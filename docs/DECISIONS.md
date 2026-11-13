# Log Keputusan Arsitektur & Teknis (DECISIONS.md)
**Proyek:** Multiverse Arcade  
**Dokumen Referensi:** Multiverse Arcade — Complex Game Design Document v2.0  
**Tanggal:** 2026-11-14  

Dokumen ini mencatat setiap keputusan arsitektural dan teknis yang diambil berdasarkan klausul "Rekomendasi" pada GDD v2.0 atau pertimbangan kompatibilitas sistem.

---

### DEC-001: Sistem Input Terpadu
- **Pilihan yang Tersedia:** Input Manager (Old) vs. Input System (New) vs. Both.
- **Rujukan GDD:** Bagian 3.1 & Catatan Input. ("Rekomendasi: Input Manager lama untuk kemudahan tim pemula; semua game memakai Esc = Pause.")
- **Keputusan:** Mengaktifkan mode `Active Input Handling = Both` (nilai internal `2` pada `ProjectSettings.asset`) dengan implementasi standar kode berbasis `Input.GetKeyDown(KeyCode.Escape)` dan sumbu klasik (`Horizontal`, `Vertical`, `Fire1`).
- **Alasan:** Memastikan kemudahan pengembangan bagi anggota tim pemula tanpa membatasi jika ada modul atau package yang memanfaatkan modul Input baru.

---

### DEC-002: Inisialisasi Manager Global (Bootstrap Scene)
- **Pilihan yang Tersedia:** Lazy-loaded singleton prefab di tiap scene vs. Bootstrap Scene tunggal (Index 0).
- **Rujukan GDD:** Bagian 2.1 & 2.2. ("Bootstrap: Scene pertama (index 0). Membuat manager global, lalu memuat MainMenu. Manager global dibuat sekali di Bootstrap dan dipertahankan dengan DontDestroyOnLoad.")
- **Keputusan:** Menggunakan Scene `Bootstrap.unity` pada index build `0` yang berisi root persistent GameObject `_GLOBAL_MANAGERS` (`GameManager`, `SceneLoader`, `SaveManager`, `SettingsManager`, `AudioManager`, `UIManager`).
- **Alasan:** Menjamin tidak ada duplikasi singleton, tidak ada race condition saat start, dan urutan inisialisasi state dapat diprediksi secara konsisten.

---

### DEC-003: Format dan Lokasi Save Data
- **Pilihan yang Tersedia:** `PlayerPrefs` vs. SQLite vs. JSON di `Application.persistentDataPath`.
- **Rujukan GDD:** Bagian 2.2. ("SaveManager: Simpan pengaturan dan progres ringan (JSON di Application.persistentDataPath).")
- **Keputusan:** Menggunakan file `multiverse_save.json` di `Application.persistentDataPath` dengan field `version = 1`.
- **Alasan:** Mendukung struktur data hierarkis (high score per game, volume audio multi-kanal, level kualitas grafis) dengan overhead minimal dan kemudahan migrasi skema.

---

### DEC-004: Penanganan Kartu Game Belum Tersedia (GameSelection)
- **Pilihan yang Tersedia:** Menyembunyikan kartu game vs. Menampilkan kartu non-aktif bertuliskan "Coming Soon".
- **Rujukan GDD:** Bagian 2.5 & Prompt. ("Hanya Arena Clash yang isEnabled = true nanti; sisanya tampil 'Coming Soon' dan tidak bisa diklik.")
- **Keputusan:** Kartu untuk Jaya, Prima, Brili, dan Risa ditampilkan di UI dengan overlay badge "Coming Soon" dan tombol interaktif yang di-disable (`interactable = false`).
- **Alasan:** Memberikan transparansi visual pada status pengerjaan proyek tanpa menimbulkan risiko error navigasi ke scene yang belum siap.

---

### DEC-005: Pemuatan Scene dan Proteksi Time.timeScale
- **Pilihan yang Tersedia:** `SceneManager.LoadScene` langsung dari masing-masing script vs. Pemuatan terpusat via `SceneLoader`.
- **Rujukan GDD:** Bagian 2.3. ("Loading selalu lewat SceneLoader; tidak ada pemanggilan SceneManager.LoadScene langsung dari game. Setiap transisi keluar dari InGame wajib memanggil IArcadeGame.OnGameExit() lalu Time.timeScale = 1.")
- **Keputusan:** Seluruh pergantian scene wajib melalui `SceneLoader.Instance.LoadScene(...)` yang memiliki listener `SceneManager.sceneLoaded` untuk mereset `Time.timeScale = 1.0f`, mengaktifkan kursor, dan menghilangkan loading overlay secara deterministik.
- **Alasan:** Menghilangkan bug game freeze (stuck di `timeScale = 0`) setelah pemain melakukan pause dan berpindah scene.

---

### DEC-006: Assembly Definition Boundary
- **Pilihan yang Tersedia:** Satu assembly monolith (`Assembly-CSharp`) vs. Pemisahan per modul (`MultiverseArcade.Core`, `MultiverseArcade.Syafiq`, `MultiverseArcade.Editor`).
- **Rujukan GDD:** Bagian 3.4. ("Tiap game punya asmdef sendiri yang merujuk MultiverseArcade.Core.")
- **Keputusan:** Dibuat 3 asmdef awal: `MultiverseArcade.Core` di folder `Assets/_Project/Scripts/`, `MultiverseArcade.Syafiq` di `Assets/Games/Syafiq/Scripts/`, dan `MultiverseArcade.Editor` di `Assets/Editor/`.
- **Alasan:** Mempercepat waktu kompilasi script dan mencegah dependensi silang terlarang antar-game anggota.
