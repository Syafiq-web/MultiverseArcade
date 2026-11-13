# Rencana Pembangunan Fondasi & Hub "Multiverse Arcade"
**Versi:** 1.0.0  
**Tanggal:** 2026-11-14  
**Target:** Unity 2022.3.62f3 LTS + Universal Render Pipeline (URP 14.0.12)  
**Dokumen Referensi:** Multiverse Arcade — Complex Game Design Document v2.0 (/docs/Multiverse_Arcade_GDD_v2.0.txt)

---

## 1. Tujuan
Membangun arsitektur global, persistent manager, sistem navigasi berbasis state machine, data registry berbasis ScriptableObject, sistem penyimpanan JSON berversi, audio & settings, UI hub (MainMenu, GameSelection, Settings, Credits, Global Pause Overlay, Loading), serta otomatisasi build scene melalui Editor Script (`Multiverse > Build Hub`).

---

## 2. Tahapan Eksekusi

### Tahap 1: Setup Proyek, Assembly Definition, & Struktur Direktori
1. Memastikan folder standar:
   - `Assets/_Project/{Scenes, Scripts, Prefabs, UI, Audio, Data}`
   - `Assets/Games/{Syafiq, Jaya, Prima, Brili, Risa}`
   - `Assets/ThirdParty`
   - `/docs`
2. Konfigurasi Assembly Definition:
   - `MultiverseArcade.Core.asmdef` di `Assets/_Project/Scripts/`
   - `MultiverseArcade.Syafiq.asmdef` di `Assets/Games/Syafiq/Scripts/` (merujuk `MultiverseArcade.Core`)
   - `MultiverseArcade.Editor.asmdef` di `Assets/Editor/` (merujuk `MultiverseArcade.Core`)
3. Menyiapkan `.gitignore`, `README.md`, `Credits.md`, `DECISIONS.md`, dan `KNOWN_ISSUES.md`.

### Tahap 2: Kontrak & Arsitektur Inti (Core)
1. **EventBus (`MultiverseArcade.Core.EventBus`)**:
   - Event global: `OnGameStarted`, `OnGamePaused`, `OnGameResumed`, `OnGameEnded`, `OnSceneLoadRequested`, `OnSettingsChanged`, `OnSaveLoaded`.
2. **Interface `IArcadeGame` (`MultiverseArcade.Core.IArcadeGame`)**:
   - `GameId`, `OnGameEnter()`, `OnGamePause()`, `OnGameResume()`, `OnGameRestart()`, `OnGameExit()`.
3. **Data-Driven Game Registry**:
   - `GameInfo.cs` (ScriptableObject): `id`, `displayName`, `sceneName`, `thumbnail`, `genre`, `description`, `controlsHint`, `isEnabled`.
   - `GameRegistry.cs` (ScriptableObject): Koleksi 5 game (`syafiq`, `jaya`, `prima`, `brili`, `risa`). Hanya Syafiq yang `isEnabled = true`.
4. **SaveManager (`MultiverseArcade.Core.SaveManager`)**:
   - Format JSON disimpan di `Application.persistentDataPath/multiverse_save.json` dengan skema versi (`version = 1`).
   - Menyimpan audio volume (Master, Music, SFX), kualitas grafis, `lastPlayedGameId`, dan High Score per game.
5. **SettingsManager (`MultiverseArcade.Core.SettingsManager`)**:
   - Mengatur Volume, Resolusi, dan `QualitySettings`. Tersinkronisasi dengan SaveManager dan AudioManager.
6. **AudioManager (`MultiverseArcade.Core.AudioManager`)**:
   - Saluran Master, Music, SFX. Persistensi volume, loop BGM, SFX trigger (Click, Hover).
7. **SceneLoader (`MultiverseArcade.Core.SceneLoader`)**:
   - Async loading via `LoadSceneAsync` dengan loading screen / overlay progress bar.
   - Proteksi otomatis reset `Time.timeScale = 1.0f` dan kursor visible saat scene selesai dimuat.
8. **GameManager (`MultiverseArcade.Core.GameManager`)**:
   - State machine aplikasi: `Boot` -> `MainMenu` -> `GameSelection` -> `Loading` -> `InGame` <-> `Paused` -> `Quit`.
   - Pintu masuk navigasi: `GoToMenu()`, `GoToSelection()`, `LoadGame(string id)`, `QuitApplication()`.
   - Pemanggilan `IArcadeGame.OnGameExit()` dan pemulihan `Time.timeScale = 1.0f`.
9. **UIManager & Global Overlay (`MultiverseArcade.Core.UIManager` & `MultiverseArcade.UI.GlobalPauseOverlay`)**:
   - Canvas persistent untuk Global Pause Overlay (Esc toggle) dan Loading Screen.

### Tahap 3: Desain UI Hub & Aset Audio
1. **Audio Aset**:
   - BGM Menu Arcade synthwave (`bgm_arcade_action.wav`).
   - SFX Button Click (`sfx_ui_click.wav`) dan SFX Button Hover (`sfx_ui_hover.wav`).
2. **UI Screens**:
   - `MainMenuUI`: Play (menuju GameSelection), Settings, Credits, Quit.
   - `GameSelectionUI`: 5 kartu game dibuat dinamis dari `GameRegistry`. Kartu aktif dapat diklik, kartu lain bertuliskan "Coming Soon" (disabled).
   - `SettingsUI`: Slider volume (Master, Music, SFX) + label persentase, Dropdown Kualitas Grafis (Low, Medium, High), tombol Back.
   - `CreditsUI`: Profil anggota tim (Syafiq, Jaya, Prima, Brili, Risa), lisensi aset (CC0/OFL/MIT), tombol Back.
   - `GlobalPauseOverlay`: Resume, Restart, Switch Game (GameSelection), Main Menu.

### Tahap 4: Editor Script Automation (`Multiverse > Build Hub`)
1. Programmatic scene generator untuk:
   - `Bootstrap.unity` (Index 0): Inisialisasi Persistent Managers, langsung pindah ke `MainMenu`.
   - `MainMenu.unity` (Index 1): UI Menu Utama, Settings Panel, Credits Panel.
   - `GameSelection.unity` (Index 2): UI Seleksi 5 Game berbasis GameRegistry.
   - `Loading.unity` (Index 3): Additive loading screen fallback.
   - `Game_Syafiq.unity` (Index 4): Entry scene gameplay Arena Clash 3D dengan implementasi `IArcadeGame`.
   - `Game_Jaya.unity`, `Game_Prima.unity`, `Game_Brili.unity`, `Game_Risa.unity` (Index 5 - 8): Scene gameplay kosong (kamera + lantai) bertanda "Placeholder Game".
2. Otomatisasi pendaftaran seluruh 9 scene ke `EditorBuildSettings`.
3. Verifikasi ketiadaan missing reference dan error compile.

### Tahap 5: Verifikasi & Dokumentasi
1. Compile check via Roslyn C# compiler.
2. Validasi file meta, asmdef, dan dependensi.
3. Melengkapi `/docs/DECISIONS.md`, `/docs/KNOWN_ISSUES.md`, `/Credits.md`, dan `README.md`.
