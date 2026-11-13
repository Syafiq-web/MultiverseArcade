# MULTIVERSE ARCADE 🎮

Proyek Game Terintegrasi berbasis Unity 3D yang memuat 5 game berbeda dalam 1 aplikasi Unity.

## 👥 Tim & Pembagian Game
1. **Syafiq** -> `Assets/Games/Syafiq/` (Arena Clash 3D - Hero Arena Shooter 3v3)
2. **Jaya** -> `Assets/Games/Jaya/`
3. **Prima** -> `Assets/Games/Prima/`
4. **Brili** -> `Assets/Games/Brili/`
5. **Risa** -> `Assets/Games/Risa/`

---

## 📁 Struktur Folder Project
```text
Assets/
├── _Project/               <-- SISTEM GLOBAL (Main Menu, Game Selection, Manager Global)
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   └── GameSelection.unity
│   ├── Scripts/
│   │   ├── Core/           (GameManager, SceneLoader, SceneNames)
│   │   ├── Audio/          (AudioManager)
│   │   └── UI/             (MainMenuUI, GameSelectionUI, GameOverlayUI)
│   ├── Prefabs/
│   ├── UI/
│   └── Audio/
├── Games/                  <-- SANDBOX INDIVIDUAL ANGGOTA
│   ├── Syafiq/Scenes/Game_Syafiq.unity
│   ├── Jaya/Scenes/Game_Jaya.unity
│   ├── Prima/Scenes/Game_Prima.unity
│   ├── Brili/Scenes/Game_Brili.unity
│   └── Risa/Scenes/Game_Risa.unity
├── Editor/                 <-- Tool Setup Otomatis (Build Scene Setup)
└── ThirdParty/             <-- Aset eksternal bersama
```

---

## 🌿 Aturan Kerja Git & Branching

Untuk menghindari **conflict scene / metadata**, setiap anggota WAJIB mengikuti aturan berikut:

### 1. Daftar Branch
* `main` : Branch rilis stabil (hanya di-merge saat integrasi final).
* `dev` : Branch pengembangan bersama.
* `feature/game-syafiq` : Pengerjaan game Syafiq.
* `feature/game-jaya` : Pengerjaan game Jaya.
* `feature/game-prima` : Pengerjaan game Prima.
* `feature/game-brili` : Pengerjaan game Brili.
* `feature/game-risa` : Pengerjaan game Risa.

### 2. Aturan Emas Mencegah Conflict
* **Bekerjalah HANYA di folder masing-masing** (`Assets/Games/<NamaAnda>/`).
* Jangan mengedit Scene milik orang lain atau Scene global (`MainMenu` / `GameSelection`) tanpa koordinasi.
* Commit selalu menyertakan file `.meta` yang bersangkutan.

---

## 🕹️ Integrasi Navigasi (Switch Game & Pause)
Setiap game wajib menyediakan tombol **Switch Game** dan **Main Menu**:
1. Gunakan komponen `GameOverlayUI.cs` di Canvas game Anda.
2. Script ini sudah otomatis menangani:
   * **Pause / Resume** (shortcut keyboard `Escape`).
   * **Switch Game** (kembali ke pemilihan game).
   * **Main Menu** (kembali ke menu utama).
   * **Reset TimeScale** (mencegah bug game macet/beku saat pindah scene).

---

## ⚙️ Setup Otomatis Build Settings
Saat pertama kali membuka project di Unity Editor:
1. Klik menu di taskbar atas: **`Tools > Multiverse Arcade > Setup Build Scenes`**.
2. Seluruh 7 Scene akan otomatis didaftarkan ke Unity Build Settings secara berurutan.
