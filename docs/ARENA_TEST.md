# Checklist Pengujian Test Matrix Arena Clash 3D (AC-01 - AC-13)
**Dokumen Referensi:** Multiverse Arcade — Complex Game Design Document v2.0 (Tabel 5.18)  
**Status Pengujian:** Verifikasi Sistem & Logika Kode  

| ID | Area | Skenario Pengujian | Hasil yang Diharapkan | Status Verifikasi |
|---|---|---|---|---|
| **AC-01** | Movement | Gerak diagonal W + D | Kecepatan sama persis dengan gerak lurus (vektor dinormalisasi `Vector3.ClampMagnitude(input, 1f) * speed`) | ✅ Terverifikasi |
| **AC-02** | Movement | Menabrak dinding pembatas / cover | Tidak tembus atau bergetar (Rigidbody dengan `CollisionDetectionMode.Continuous`, dinding tebal 2 unit + BoxCollider) | ✅ Terverifikasi |
| **AC-03** | Aiming | Mouse digerakkan ke segala sudut layar | Karakter berputar hanya pada sumbu Y menghadap titik kursor di bidang lantai (`plane.Raycast`) | ✅ Terverifikasi |
| **AC-04** | Shooting | Tahan tombol klik kiri (tembak terus menerus) | Fire rate terjaga sesuai interval 0,4 s (2,5 tembakan per detik) | ✅ Terverifikasi |
| **AC-05** | Team ID | Menembak karakter satu tim (Friendly Fire) | Proyektil tidak melukai rekan satu tim (`DamageInfo.attackerTeamId == Health.TeamId` ditolak) | ✅ Terverifikasi |
| **AC-06** | Health | Karakter menerima damage hingga HP <= 0 | Karakter mati tepat 1x (`Die()` berpelindung boolean), eliminasi bertambah tepat 1x ke skor tim lawan | ✅ Terverifikasi |
| **AC-07** | AI | Bot terhalang dinding/obstacle di tengah arena | NavMeshAgent menghitung rute memutar menghindari rintangan tanpa macet | ✅ Terverifikasi |
| **AC-08** | AI | Target yang sedang dikejar bot mati | Bot langsung beralih mencari target lawan hidup lainnya melalui evaluasi Utility Score berkala (0,3 - 0,5s) | ✅ Terverifikasi |
| **AC-09** | Match | Skor salah satu tim mencapai 10 eliminasi | Pertandingan langsung selesai, kontrol mati, timer berhenti, dan Result Panel muncul | ✅ Terverifikasi |
| **AC-10** | Match | Waktu match habis (180s) dengan skor imbang | Pertandingan selesai dengan status DRAW (Seri) | ✅ Terverifikasi |
| **AC-11** | UI | Menekan tombol Pause (Esc) saat match berlangsung | Permainan terjeda (`Time.timeScale = 0`), audio SFX dijeda, bot dan timer berhenti bergerak | ✅ Terverifikasi |
| **AC-12** | Restart | Pemain memilih Restart/Rematch berulang kali (20x) | Skor kembali 0, timer kembali 180s, posisi kembali ke spawn, pool dibersihkan, tidak ada kebocoran objek | ✅ Terverifikasi |
| **AC-13** | Integration | Klik Switch Game atau Main Menu saat jeda/selesai | Berpindah scene ke Hub (`GameSelection` / `MainMenu`) dengan `Time.timeScale = 1.0f` dan kursor bebas | ✅ Terverifikasi |
