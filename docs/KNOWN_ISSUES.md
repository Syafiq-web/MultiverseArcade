# Daftar Masalah Diketahui & Panduan Verifikasi Manual (KNOWN_ISSUES.md)
**Proyek:** Multiverse Arcade  
**Game Target:** Arena Clash 3D (`Game_Syafiq.unity`)  
**Unity Version:** 2022.3.62f3 LTS  
**Terakhir Diperbarui:** 2026-11-14 (Sesi Bug Fix Arena Clash)  

---

## 1. Laporan Perbaikan Bug (Bug Fix Report)

### BUG 1: Pause Overlay Tidak Merespons Klik Mouse
- **Gejala Awal:** Menekan tombol `Esc` atau klik tombol `PAUSE (Esc)` menghentikan waktu permainan (`Time.timeScale = 0`), namun semua tombol di panel pause (`Resume`, `Restart`, `Switch Game`, `Main Menu`) tidak merespons klik mouse sama sekali.
- **Akar Masalah (Root Causes):**
  1. **Ketiadaan EventSystem di Scene `Game_Syafiq`:**
     Pada script builder awal `ArenaClashBuilder.cs`, metode pembuatan `EventSystem` sama sekali terlewat (`CreateEventSystem()` tidak dipanggil). Selain itu, `EventSystem` pada scene `Bootstrap.unity` tidak dijadikan persistent (`DontDestroyOnLoad`), sehingga saat transisi scene ke `Game_Syafiq`, tidak ada satupun `EventSystem` aktif di scene. Tanpa `EventSystem` dan `StandaloneInputModule`, Unity UI tidak dapat menerima input klik mouse sama sekali.
  2. **Intersepsi Raycast & Panel Ganda:**
     Elemen UI non-interaktif pada Canvas HUD seperti `TopScoreBanner/Background`, `PlayerHUD/Background`, dan `KillFeedItem/Background` memiliki `raycastTarget = true` bawaan komponen `Image`, yang berpotensi mencegat raycast mouse. Selain itu, saat pause dipicu lewat `GameManager`, `GlobalPauseOverlay` di `GlobalCanvas` dan `PausePanel` lokal di scene terbuka bersamaan (panel ganda).
  3. **Sorting Order & Isolasi Canvas:**
     Panel pause sebelumnya digabung di dalam `ArenaGameCanvas` dengan sorting order 100.
- **Solusi yang Diterapkan:**
  1. Dibuat komponen `SingleEventSystem.cs` dengan `[DefaultExecutionOrder(-1000)]` dan `DontDestroyOnLoad` yang memastikan tepat satu `EventSystem` aktif dengan `StandaloneInputModule` (kompatibel `Active Input Handling = Both`), baik saat scene `Game_Syafiq` dimainkan langsung maupun dari `Bootstrap`. Jika ada EventSystem duplikat, komponen otomatis mendestroy duplikat tersebut.
  2. Dibuat dedicated `PauseCanvas` terpisah dengan `Sorting Order = 500` (tertinggi di scene) lengkap dengan komponen `GraphicRaycaster` mandiri.
  3. Seluruh elemen non-interaktif (`TopScoreBanner`, `PlayerHUD`, `DamageNumber`, `KillFeedItem`, dan background result panel) telah diatur `raycastTarget = false`. Background peredam `PausePanel` (`dimImg`) memiliki `raycastTarget = true` untuk menahan klik tembus ke gameplay, sementara semua tombol (`Button`) berada di lapisan depan dengan child text label `raycastTarget = false`.
  4. Ditambahkan proteksi panel ganda di `GameOverlayUI.cs`: jika `UIManager.Instance` global sudah mengaktifkan pause overlay, panel pause lokal tidak ditampilkan ganda.
  5. Ditambahkan penanganan kursor: saat pause aktif, `Cursor.visible = true` dan `Cursor.lockState = CursorLockMode.None`. Saat resume, kursor dikembalikan normal.
  6. Ditambahkan proteksi kondisi tepi (Edge Cases): pause dilarang keras saat status pertandingan berada dalam `Countdown` (3-2-1) atau `Finished` (Result Panel tampil). Seluruh tombol pause (`Resume`, `Restart`, `Switch Game`, `Main Menu`) selalu mereset `Time.timeScale = 1.0f` dan membersihkan event bus serta projectile pool melalui `OnGameExit()`.

---

### BUG 2: Bar Health World-Space Bermasalah
- **Gejala Awal:** Health bar di atas karakter musuh tidak tampil, tidak berkurang, hilang permanen saat mati, atau tidak muncul kembali saat respawn/rematch.
- **Akar Masalah (Root Causes):**
  1. **Variabel Target Tidak Terserialisasi:**
     Pada `HealthBarWorld.cs` lama, referensi target `_targetHealth` dan `_targetTransform` merupakan private field tanpa atribut `[SerializeField]` dan hanya diisi saat pemanggilan editor script builder. Saat scene disimpan dan dimuat pada Play Mode, kedua variabel tersebut bernilai `null`.
  2. **Deaktivasi Dini pada LateUpdate:**
     Karena `_targetTransform == null` pada runtime Play Mode, kondisi `if (_targetTransform == null) gameObject.SetActive(false);` langsung mematikan GameObject health bar pada frame 1, sehingga bar tidak pernah tampil.
  3. **Siklus Hidup Event Tidak Sinkron:**
     Event `ArenaEvents.OnHealthChanged` sebelumnya hanya disubscribe di metode `Setup()` yang hanya berjalan saat editor script dieksekusi, bukan saat Play Mode (`OnEnable`/`Awake`).
  4. **Pencegatan Raycast oleh Canvas World-Space:**
     Prefab `PF_HealthBarWorld` memiliki `Image` background dan fill dengan `raycastTarget = true` yang berpotensi memblokir raycast mouse game dan UI.
  5. **Masalah Deaktivasi Child Saat Respawn:**
     Mematikan GameObject child saat mati membuat child tetap nonaktif saat karakter diaktifkan kembali (`SetActive(true)` pada parent tidak otomatis mengaktifkan child yang dimatikan manual).
- **Solusi yang Diterapkan:**
  1. Refactor total `HealthBarWorld.cs`: Komponen kini secara otomatis mendeteksi `_targetTransform` via `transform.parent` dan `_targetHealth` via `GetComponentInParent<Health>()` pada `Awake()` dan `OnEnable()`.
  2. Berlangganan event dilakukan pada `OnEnable()` dan dilepas pada `OnDisable()`.
  3. Warna tim disinkronkan secara dinamis berdasarkan `_targetHealth.TeamId`: Tim 0 (Blue) = Cyan/Biru (`#00D9FF`), Tim 1 (Red) = Merah Terang (`#FF4040`).
  4. Visibilitas health bar dikendalikan lewat `_canvas.enabled = isAlive;` (bukan mematikan GameObject). Dengan demikian, skrip tetap aktif, orientasi billboard tetap menghadap kamera, dan saat karakter dihidupkan kembali via `ResetHealth()`, bar langsung pulih penuh tanpa kendala child deactivation.
  5. Seluruh elemen visual health bar (`_backgroundImage`, `_fillImage`) telah diatur `raycastTarget = false;`, `Slider.interactable = false;`, dan `GraphicRaycaster` dihapus dari canvas world-space.
  6. Pada builder `ArenaClashBuilder.cs`, ditambahkan pengecekan pembersihan sebelum instansiasi agar tidak ada health bar ganda pada karakter manapun.

---

## 2. Panduan Verifikasi Manual (Manual Test Checklist)

Jalankan pengujian Play Mode berikut di Unity Editor:

### Test 1: Verifikasi Responsivitas Tombol Pause
1. Buka scene `Assets/Games/Syafiq/Scenes/Game_Syafiq.unity` langsung di Unity Editor.
2. Klik tombol **Play (▶)**. Tunggu hingga hitungan mundur selesai dan permainan dimulai.
3. Tekan tombol **Esc** pada keyboard atau klik tombol **PAUSE (Esc)** di pojok kanan atas.
4. *Hasil yang Diharapkan:*
   - Panel pause muncul di tengah layar (`GAME PAUSED`).
   - Waktu game berhenti (`Time.timeScale = 0`).
   - Kursor mouse bebas dan terlihat (`Cursor.visible = true`).
5. Arahkan mouse ke tombol **RESUME**, **RESTART GAME**, **SWITCH GAME**, dan **MAIN MENU**.
   - Setiap tombol memiliki feedback visual hover.
6. Klik **RESUME** -> Permainan berjalan normal kembali (`Time.timeScale = 1.0f`).
7. Tekan **Esc** kembali -> Klik **RESTART GAME** -> Permainan mengulang match dari hitungan mundur 3-2-1 dengan skor ter-reset ke 0-0.

### Test 2: Uji Ketahanan Loop Pause-Resume (10x Loop Test)
1. Selama permainan berlangsung, tekan tombol **Esc** lalu tekan **Esc** lagi (atau klik **RESUME**).
2. Ulangi proses pause dan resume minimal **10 kali berturut-turut**.
3. *Hasil yang Diharapkan:*
   - State permainan tidak pernah rusak / stuck.
   - Tidak pernah muncul panel ganda.
   - Posisi karakter dan peluru tidak mengalami glitch atau teleportasi.
   - Kursor mouse tetap dapat membidik secara normal setiap kali game di-resume.

### Test 3: Uji Kondisi Tepi (Edge Cases)
1. **Pause saat Countdown:**
   - Tekan **RESTART GAME** atau mulai pertandingan baru.
   - Saat teks hitungan mundur tampil ("3", "2", "1"), tekan tombol **Esc**.
   - *Hasil yang Diharapkan:* Game menolak jeda. Panel pause tidak boleh muncul dan hitungan mundur berlanjut hingga selesai tanpa glitch.
2. **Pause saat Finished (Result Panel Aktif):**
   - Biarkan pertandingan selesai (skor tim mencapai 10 eliminasi).
   - Saat panel kemenangan/kekalahan (`VICTORY!` / `DEFEAT!`) tampil, tekan tombol **Esc**.
   - *Hasil yang Diharapkan:* Game menolak jeda. Panel pause tidak boleh menimpa result panel. Tombol **REMATCH** dan **SWITCH GAME** pada result panel dapat diklik dengan normal.
3. **Pause saat Respawn:**
   - Biarkan karakter pemain atau bot tereliminasi. Saat timer respawn sedang berjalan, tekan tombol **Esc**.
   - *Hasil yang Diharapkan:* Waktu jeda berhenti. Saat di-resume, karakter respawn di titik spawn masing-masing dengan normal.

### Test 4: Uji Bar Health World-Space (6 Karakter)
1. Periksa seluruh 6 karakter di arena:
   - 1 Pemain: `Player_Blaze` (Health bar berwarna Biru Cyan).
   - 2 Bot Tim Biru: `BlueBot_1`, `BlueBot_2` (Health bar berwarna Biru Cyan).
   - 3 Bot Tim Merah: `RedBot_1`, `RedBot_2`, `RedBot_3` (Health bar berwarna Merah Terang).
2. Putar pandangan dan amati pergerakan kamera:
   - Seluruh bar health melayang tepat di atas kepala karakter dan selalu menghadap ke arah kamera (orientasi billboard sempurna).
3. Tembak karakter musuh merah (`RedBot`):
   - Bar health berkurang secara mulus dan akurat sesuai damage tembakan (20 damage per tembakan).
   - Muncul angka damage kuning melayang tanpa menghalangi klik UI.
4. Tembak musuh hingga HP habis (0 HP):
   - Karakter tereliminasi, health bar ikut menghilang seketika.
5. Tunggu 4 detik hingga musuh respawn:
   - Musuh muncul kembali di spawn point dengan bar health penuh (100 HP) dan warna merah tetap sesuai.
6. Lakukan **REMATCH**:
   - Seluruh karakter (pemain, 2 bot biru, 3 bot merah) kembali ke spawn masing-masing dengan bar health penuh dan warna tim yang tepat.
   - Tidak ada bar health yang terduplikasi.

---

## 3. Catatan Teknis & Transparansi Batchmode
- Seluruh script C# (`MultiverseArcade.Core`, `MultiverseArcade.Syafiq`, `MultiverseArcade.Editor`) telah dikompilasi dengan Microsoft MSBuild dan menghasilkan **0 Error dan 0 Warning**.
- Unity batchmode CLI tidak dapat mengakses folder proyek secara bersamaan saat Unity Editor sedang aktif dibuka oleh user karena mekanisme *instance project lock* Windows. Oleh karena itu, pengujian runtime Play Mode diuji melalui Play-Testing checklist di atas.
- Disediakan Editor Validator otomatis yang dapat dipanggil melalui menu Unity:  
  `Tools > Multiverse Arcade > 🔍 Validate Arena Clash (EditMode)` atau `Multiverse > Validate Arena Clash`  
  untuk memvalidasi keutuhan EventSystem, GraphicRaycaster, sorting order Canvas, dan ketiadaan raycast blocking secara instan di Editor.
