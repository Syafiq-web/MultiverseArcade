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


---

### DEC-007: Desain Arena 30x30 Simetris & Dynamic NavMesh Carving
- **Pilihan yang Tersedia:** Arena asimetris dengan pre-baked static meshes vs. Arena simetris 30x30 u dengan NavMeshObstacle Carving.
- **Rujukan GDD:** Bagian 5.8. ("Ukuran 30 x 30 u, simetris sumbu tengah agar adil bagi kedua tim. Tiga lane, obstacle memakai NavMeshObstacle carve.")
- **Keputusan:** Arena 30x30 dibangun dengan pembatas dinding 2 u, zona Blue Spawn di barat daya (-10, 0, -10), Red Spawn di timur laut (10, 0, 10), dan 5 obstacle cover simetris dengan `NavMeshObstacle` (carve = true).
- **Alasan:** Memberikan keseimbangan kompetitif 3v3 dan menjamin AI bot tidak menabrak rintangan atau tersangkut.

---

### DEC-008: Proteksi Friendly Fire via Team ID
- **Pilihan yang Tersedia:** Memisahkan layer proyektil per tim (`Blue_Projectile`, `Red_Projectile`) vs. Pengecekan Team ID di `Health.cs`.
- **Rujukan GDD:** Prompt & Bagian 5.6. ("Friendly fire dicegah lewat Team ID, bukan layer.")
- **Keputusan:** Proyektil menggunakan satu layer bersama `Syafiq_Projectile`. Komponen `Health` memeriksa `info.attackerTeamId == _teamId`. Jika sama, damage ditolak secara deterministik.
- **Alasan:** Menghemat kuota 32 layer Unity dan memastikan performa collision matrix tetap efisien.

---

### DEC-009: Prewarmed Object Pooling untuk Proyektil
- **Pilihan yang Tersedia:** `Instantiate` dan `Destroy` per tembakan vs. `ProjectilePool` kapasitas 60.
- **Rujukan GDD:** Bagian 5.5, 5.17 & AC-14. ("Object pooling untuk proyektil (pool 60) dan efek.")
- **Keputusan:** Menggunakan `ProjectilePool` dengan 60 instance `PF_Projectile` yang di-prewarm saat start.
- **Alasan:** Menghilangkan alokasi memori GC per frame dan menjamin framerate stabil >= 60 FPS pada pertempuran 3v3 dengan tembakan beruntun.

---

### DEC-010: Integrasi Pause Global Tanpa Duplikasi
- **Pilihan yang Tersedia:** Membuat canvas pause khusus di dalam scene `Game_Syafiq` vs. Menghubungkan ke `GlobalPauseOverlay` (Esc) yang sudah ada di Hub.
- **Rujukan GDD:** Prompt & Bagian 2.2. ("Pakai Global Pause Overlay yang sudah ada, jangan buat duplikat; hubungkan ke OnGamePause/OnGameResume.")
- **Keputusan:** Menghubungkan tombol Esc dan event `IArcadeGame.OnGamePause()` / `OnGameResume()` ke sistem pause global Hub.
- **Alasan:** Menjamin konsistensi UX di seluruh 5 game arcade dan mencegah duplikasi kode atau konflik timeScale.

---

### DEC-011: Proteksi Gerak Dash & Rigidbody Non-Kinematic
- **Pilihan yang Tersedia:** Teleportasi instan `transform.position += dir * dist` vs. `Physics.SphereCast` dengan batas koordinat arena dan gerak via `Rigidbody.velocity`.
- **Rujukan GDD:** Prompt Fase 6 & Bagian 5.4. ("Dash TIDAK boleh menembus tembok atau keluar arena. PlayerController harus memakai Rigidbody NON-kinematic dengan gerak via velocity sehingga tidak tembus tembok, cover, dan bot.")
- **Keputusan:** `PlayerController` menggunakan `Rigidbody` non-kinematic dengan `FreezeRotationX | FreezeRotationZ | FreezePositionY` dan `CollisionDetectionMode.Continuous`. Skill Dash Blaze (`ExecuteDash`) menggunakan `Physics.SphereCast` dengan stopping distance 0.2 unit sebelum collider rintangan serta clamping posisi absolut dalam batas aman arena (`[-13.8, 13.8]`).
- **Alasan:** Menghilangkan resiko clipping/tembus tembok atau keluar arena secara fisik tanpa mengorbankan responsivitas kendali pemain.

---

### DEC-012: Arsitektur Efek Status (StatusEffectController) & Multiplier
- **Pilihan yang Tersedia:** Polling boolean di masing-masing skrip gerak/tembak vs. Komponen modular `StatusEffectController` per karakter.
- **Rujukan GDD:** Bagian 5.5, 5.7 & Prompt Fase 6. ("StatusEffectController per karakter: Slow, Stun, Shield, Haste, Damage Boost dihitung sebagai multiplier, timer memakai scaled time, di-reset saat mati/respawn/rematch.")
- **Keputusan:** Menambahkan komponen `StatusEffectController` ke Player dan seluruh Bot AI. Menghitung pengali (`SpeedMultiplier`, `DamageMultiplier`, `DamageTakenMultiplier`) dan flag `IsStunned`. Timer dihitung menggunakan `Time.deltaTime` sehingga otomatis terjeda saat game pause (`timeScale = 0`).
- **Alasan:** Memudahkan ekstensi jenis efek status di masa depan secara decoupled dan memastikan konsistensi sinkronisasi timer dengan sistem pause.
