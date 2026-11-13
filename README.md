# MULTIVERSE ARCADE 🎮

**Multiverse Arcade** adalah aplikasi "hub" berbasis Unity 3D yang memuat 5 game 3D berbeda di bawah satu sistem navigasi, audio, pengaturan, dan penyimpanan yang seragam.
Proyek ini dikembangkan berdasarkan **Multiverse Arcade — Complex Game Design Document v2.0** (`/docs/Multiverse_Arcade_GDD_v2.0.txt`).

---

## 👥 Tim Pengembang
1. **Syafiq** — *Lead Developer*, Arsitektur Sistem Global & Game 1 (*Arena Clash 3D*)
2. **Jaya** — Game Developer (*Game 2*)
3. **Prima** — Game Developer (*Game 3*)
4. **Brili** — Game Developer (*Game 4*)
5. **Risa** — Game Developer (*Game 5*)

---

## 🏛️ Arsitektur Sistem Global

### 1. Struktur Scene & Build Settings
Urutan scene wajib di Build Settings:
- `[0] Assets/_Project/Scenes/Bootstrap.unity` — Persistent entry scene. Membuat manager global, lalu memuat `MainMenu`.
- `[1] Assets/_Project/Scenes/MainMenu.unity` — Menu utama (Play, Settings, Credits, Quit).
- `[2] Assets/_Project/Scenes/GameSelection.unity` — Katalog pemilihan game (5 kartu dinamis dari `GameRegistry`).
- `[3] Assets/_Project/Scenes/Loading.unity` — Layar loading async additive + progress tracking.
- `[4] Assets/Games/Syafiq/Scenes/Game_Syafiq.unity` — Arena Clash 3D (3v3 Top-Down Hero Arena Shooter).
- `[5] Assets/Games/Jaya/Scenes/Game_Jaya.unity` — Gameplay Arena Jaya (Coming Soon).
- `[6] Assets/Games/Prima/Scenes/Game_Prima.unity` — Gameplay Arena Prima (Coming Soon).
- `[7] Assets/Games/Brili/Scenes/Game_Brili.unity` — Gameplay Arena Brili (Coming Soon).
- `[8] Assets/Games/Risa/Scenes/Game_Risa.unity` — Gameplay Arena Risa (Coming Soon).

### 2. Persistent Global Managers (`DontDestroyOnLoad`)
Dibuat sekali di scene `Bootstrap.unity`:
- **GameManager**: State machine aplikasi (`Boot` ➔ `MainMenu` ➔ `GameSelection` ➔ `Loading` ➔ `InGame` ⇄ `Paused` ➔ `Quit`). Menangani pendaftaran `IArcadeGame`, navigasi global, dan pemanggilan `OnGameExit()`.
- **SceneLoader**: Pemuatan async terpusat (`LoadSceneAsync`), loading progress bar, proteksi timeout, dan jaminan `Time.timeScale = 1.0f` serta kursor aktif di setiap scene.
- **AudioManager**: Pengaturan volume (Master, Music, SFX), pemutaran musik looping, crossfade antar-scene, efek suara UI (click & hover), serta auto-pause SFX saat jeda.
- **UIManager**: Canvas persistent untuk Global Pause Overlay (shortcut `Esc`) dan Global Loading Screen.
- **SaveManager**: Penyimpanan JSON di `Application.persistentDataPath/multiverse_save.json` dengan skema versi (`version = 1`), menyimpan volume audio, kualitas grafis, `lastPlayedGameId`, dan High Score per game.
- **SettingsManager**: Mengatur sinkronisasi volume audio dan level kualitas grafis (`QualitySettings`).
- **EventBus**: Komunikasi decoupled antar-sistem (`OnGameStarted`, `OnGamePaused`, `OnGameResumed`, `OnGameEnded`, `OnSceneLoadRequested`, `OnSettingsChanged`).

### 3. Data-Driven Game Registry (`ScriptableObject`)
- `GameRegistry.asset` memuat daftar 5 `GameInfo`:
  - `syafiq` (`Arena Clash 3D`) ➔ `isEnabled = true` (Dapat dimainkan langsung).
  - `jaya`, `prima`, `brili`, `risa` ➔ `isEnabled = false` (Menampilkan badge "Coming Soon" dan tombol terkunci).

### 4. Kontrak `IArcadeGame`
Setiap scene game mengimplementasikan interface `IArcadeGame`:
```csharp
public interface IArcadeGame
{
    string GameId { get; }
    void OnGameEnter();
    void OnGamePause();
    void OnGameResume();
    void OnGameRestart();
    void OnGameExit();
}
```

---

## 🕹️ Panduan Menjalankan & Bermain

### Cara Build & Setup Hub Otomatis
1. Buka project di **Unity 2022.3.62f3 LTS**.
2. Di menu bar atas, klik **`Multiverse > Build Hub`** (atau **`Tools > Multiverse Arcade > 🚀 Build Hub (Full Setup)`**).
3. Script otomatis membuat ScriptableObject, merakit seluruh 9 scene, dan menyusun Build Settings secara presisi.

### Kontrol Permainan
- **Global:**
  - `Esc` : Buka / Tutup Global Pause Overlay (Resume, Restart, Switch Game, Main Menu).
  - `Mouse Klik Kiri` : Navigasi tombol UI.
- **Arena Clash 3D (Game Syafiq):**
  - `W / A / S / D` : Bergerak ke segala arah.
  - `Kursor Mouse` : Membidik reticle / laser sight.
  - `Klik Kiri (Fire)` : Menembak proyektil.
  - `Q` / `Tombol 1 & 2` / `Scroll Wheel` : Berganti senjata antara **Plasma Rifle** (rapid fire) dan **Scatter Shotgun** (6 peluru sebar area).

---

## 📁 Struktur Direktori
```text
Assets/
├── _Project/               <-- SISTEM GLOBAL (Persistent Managers, Hub UI, Registry Data)
│   ├── Audio/              (BGM & SFX UI)
│   ├── Data/               (GameRegistry.asset & GameInfo assets)
│   ├── Scenes/             (Bootstrap, MainMenu, GameSelection, Loading)
│   ├── Scripts/
│   │   ├── Core/           (GameManager, SceneLoader, SaveManager, SettingsManager, EventBus, IArcadeGame)
│   │   ├── Audio/          (AudioManager)
│   │   └── UI/             (MainMenuUI, GameSelectionUI, SettingsUI, CreditsUI, GlobalPauseOverlay)
│   ├── UI/
│   └── Prefabs/
├── Games/                  <-- MODUL INDIVIDUAL ANGGOTA
│   ├── Syafiq/             (Arena Clash 3D: Scripts, Scenes, Materials, Audio)
│   ├── Jaya/               (Game_Jaya.unity)
│   ├── Prima/              (Game_Prima.unity)
│   ├── Brili/              (Game_Brili.unity)
│   └── Risa/               (Game_Risa.unity)
├── Editor/                 (MultiverseHubBuilder.cs, MultiverseArcadeBuilder.cs)
└── ThirdParty/             (Aset eksternal berlisensi bebas)
```

---

## 📄 Dokumentasi Tambahan
- `/docs/BUILD_PLAN.md` : Rencana arsitektur dan tahapan eksekusi.
- `/docs/DECISIONS.md` : Log keputusan teknis berdasarkan rekomendasi GDD v2.0.
- `/docs/KNOWN_ISSUES.md` : Daftar status dan panduan pengujian manual 20x switch test.
- `/Credits.md` : Daftar tim dan atribusi lisensi aset lengkap (CC0, OFL, dsb).
