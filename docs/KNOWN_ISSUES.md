# Daftar Masalah Diketahui & Panduan Verifikasi Manual (KNOWN_ISSUES.md)
**Proyek:** Multiverse Arcade  
**Status Hub:** Selesai dibangun (Bootstrap, Main Menu, Game Selection, Loading, Global Managers, Registry, Save System)  
**Terakhir Diperbarui:** 2026-11-14  

---

## 1. Status Game Anggota (Jaya, Prima, Brili, Risa)
- **Status:** Dalam tahap perencanaan (Coming Soon).
- **Kondisi:**
  - Kartu pada menu `GameSelection` dikunci (`isEnabled = false`) dengan badge visual "Coming Soon" dan tombol tidak dapat diklik.
  - Scene `Game_Jaya`, `Game_Prima`, `Game_Brili`, dan `Game_Risa` telah dibuat dengan lantai, kamera, pencahayaan, billboard 3D bertuliskan "Coming Soon", serta komponen `PlaceholderGameEntry` yang mengimplementasikan `IArcadeGame`.
  - Tombol overlay navigasi (`Resume`, `Restart`, `Switch Game`, `Main Menu`) berfungsi penuh pada semua scene tersebut.
- **Rencana Mitigasi:** Masing-masing anggota tim akan mengimplementasikan gameplay game mereka di branch feature masing-masing tanpa mengubah arsitektur `MultiverseArcade.Core`.

---

## 2. Peringatan URP Camera Data di Unity Editor
- **Masalah:** Jika scene dimuat di Unity Editor saat proses Domain Reload belum selesai sempurna, tampilan Game View kadang dapat menampilkan *"No cameras rendering"* jika display terpilih selain "Display 1".
- **Mitigasi yang Diterapkan:**
  - Seluruh kamera di scene `Bootstrap`, `MainMenu`, `GameSelection`, `Game_Syafiq`, dan scene anggota telah secara programatik dilengkapi komponen `UniversalAdditionalCameraData` bawaan URP 14.
  - Disediakan menu shortcut di toolbar Unity:
    - `Tools > Multiverse Arcade > 📂 Open MainMenu Scene`
    - `Tools > Multiverse Arcade > 📂 Open Syafiq Game Scene`
    - `Tools > Multiverse Arcade > 📂 Open GameSelection Scene`

---

## 3. Langkah-Langkah Verifikasi Manual (Play-Testing Checklist)

Lakukan pengujian berikut di Unity Editor untuk memastikan stabilitas Hub:

1. **Uji Urutan Scene & Bootstrap (Index 0):**
   - Buka scene `Assets/_Project/Scenes/Bootstrap.unity`.
   - Tekan tombol **Play (▶)** di Unity Editor.
   - *Ekspektasi:* Objek `_GLOBAL_MANAGERS` diinisialisasi sekali (berstatus `DontDestroyOnLoad`), BGM mulai berputar, dan scene berpindah secara mulus ke `MainMenu.unity`.

2. **Uji Navigasi Main Menu -> Game Selection:**
   - Di `MainMenu`, klik tombol **PLAY**.
   - *Ekspektasi:* Scene `GameSelection.unity` terbuka dan menampilkan 5 kartu game. Kartu Syafiq (Arena Clash 3D) aktif dengan tombol **PLAY**, sedangkan 4 kartu lainnya berlabel **LOCKED** / **Coming Soon** dan tidak dapat diklik.

3. **Uji Pengaturan Volume & Kualitas Grafis (Persistensi Save JSON):**
   - Di `MainMenu`, klik **SETTINGS**.
   - Geser slider *Master Volume*, *Music Volume*, atau *SFX Volume*.
   - Ubah kualitas grafis ke *Low* / *Medium* / *High*.
   - Tekan tombol **CLOSE**, lalu hentikan Play Mode (**Stop ■**).
   - Tekan tombol **Play (▶)** kembali, buka menu **SETTINGS**.
   - *Ekspektasi:* Posisi slider volume dan pengaturan grafis tetap persis seperti yang diubah sebelumnya (tersimpan di `multiverse_save.json`).

4. **Uji In-Game & Global Pause Overlay (Esc Key):**
   - Di menu `GameSelection`, klik tombol **PLAY** pada kartu Arena Clash 3D.
   - Scene `Game_Syafiq` terbuka dan gameplay dimulai.
   - Tekan tombol **Esc** pada keyboard.
   - *Ekspektasi:* Global Pause Overlay muncul di layar, waktu permainan berhenti (`Time.timeScale = 0`), dan efek suara permainan dijeda.
   - Klik **RESUME** -> Permainan berjalan normal kembali (`Time.timeScale = 1.0f`).
   - Tekan **Esc** lagi -> klik **SWITCH GAME** -> Langsung kembali ke menu `GameSelection` dengan `Time.timeScale` normal.

5. **Stress Test Pergantian Scene (20x Switch Test):**
   - Lakukan perpindahan berulang-ulang:
     `MainMenu` ➔ `GameSelection` ➔ `Game_Syafiq` ➔ `Pause (Esc)` ➔ `Switch Game` ➔ `Game_Syafiq` (ulangi minimal 20x).
   - *Ekspektasi:* Tidak ada lonjakan memori (tidak ada duplikasi `_GLOBAL_MANAGERS`), tidak ada freeze, dan Console bersih dari pesan error merah.
