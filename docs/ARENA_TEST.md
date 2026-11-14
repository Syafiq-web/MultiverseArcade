# Checklist Pengujian Test Matrix Arena Clash 3D (AC-01 - AC-20)
**Dokumen Referensi:** Multiverse Arcade — Complex Game Design Document v2.0 (Tabel 5.18 & Bagian 5.4 - 5.11)  
**Status Pengujian:** Verifikasi Sistem, Logika Kode, dan Keseimbangan Fase 1 - 6  

| ID | Area | Skenario Pengujian | Hasil yang Diharapkan | Status Verifikasi |
|---|---|---|---|---|
| **AC-01** | Movement | Gerak diagonal W + D | Kecepatan sama persis dengan gerak lurus (vektor dinormalisasi `Vector3.ClampMagnitude(input, 1f) * speed`) | ✅ Terverifikasi |
| **AC-02** | Movement | Menabrak dinding pembatas / cover | Tidak tembus atau bergetar (Rigidbody non-kinematic dengan `CollisionDetectionMode.Continuous`, FreezeRotation X/Z) | ✅ Terverifikasi |
| **AC-03** | Aiming | Mouse digerakkan ke segala sudut layar | Karakter berputar hanya pada sumbu Y menghadap titik kursor di bidang lantai (`plane.Raycast`) | ✅ Terverifikasi |
| **AC-04** | Shooting | Tahan tombol klik kiri (tembak terus menerus) | Fire rate terjaga sesuai interval (Blaze 0,4s / Bulwark 0,8s) | ✅ Terverifikasi |
| **AC-05** | Team ID | Menembak karakter satu tim (Friendly Fire) | Proyektil tidak melukai rekan satu tim (`DamageInfo.attackerTeamId == Health.TeamId` ditolak) | ✅ Terverifikasi |
| **AC-06** | Health | Karakter menerima damage hingga HP <= 0 | Karakter mati tepat 1x (`Die()` berpelindung boolean), eliminasi bertambah tepat 1x ke skor tim lawan | ✅ Terverifikasi |
| **AC-07** | AI | Bot terhalang dinding/obstacle di tengah arena | NavMeshAgent menghitung rute memutar menghindari rintangan tanpa macet | ✅ Terverifikasi |
| **AC-08** | AI | Target yang sedang dikejar bot mati | Bot langsung beralih mencari target lawan hidup lainnya melalui evaluasi Utility Score berkala (0,3 - 0,5s) | ✅ Terverifikasi |
| **AC-09** | Match | Skor salah satu tim mencapai 10 eliminasi | Pertandingan langsung selesai, kontrol mati, timer berhenti, dan Result Panel muncul | ✅ Terverifikasi |
| **AC-10** | Match | Waktu match habis (180s) dengan skor imbang | Pertandingan selesai dengan status DRAW (Seri) | ✅ Terverifikasi |
| **AC-11** | UI | Menekan tombol Pause (Esc) saat match berlangsung | Permainan terjeda (`Time.timeScale = 0`), audio SFX dijeda, bot dan timer berhenti bergerak, tombol responsif | ✅ Terverifikasi |
| **AC-12** | Restart | Pemain memilih Restart/Rematch berulang kali (20x) | Skor kembali 0, timer kembali 180s, posisi kembali ke spawn, pool dibersihkan, tidak ada kebocoran objek | ✅ Terverifikasi |
| **AC-13** | Integration | Klik Switch Game atau Main Menu saat jeda/selesai | Berpindah scene ke Hub (`GameSelection` / `MainMenu`) dengan `Time.timeScale = 1.0f` dan kursor bebas | ✅ Terverifikasi |
| **AC-14** | Fase 6 — Dash | Tekan Spasi saat memainkan Blaze menghadap dinding | Dash bergerak maju 4 unit dan berhenti tepat 0.2 unit sebelum dinding (`Physics.SphereCast`), koordinat tetap dalam batas arena (-13.8 s.d. 13.8), cooldown radial terpicu 6 detik | ✅ Terverifikasi |
| **AC-15** | Fase 6 — Selection | Memilih hero Bulwark & difficulty Hard di modal awal | Karakter player berpenampilan Bulwark (HP 160, Shotgun), bot bereaksi agresif (aim error 3°), dan konfigurasi tetap sama saat Rematch tanpa reset | ✅ Terverifikasi |
| **AC-16** | Fase 6 — Combat | Bulwark menembak shotgun dan mengaktifkan Energy Shield | 3 proyektil menyebar 20°, damage 3x8 = 24. Saat shield aktif (Spasi), damage yang diterima berkurang 60% selama 2 detik | ✅ Terverifikasi |
| **AC-17** | Fase 6 — Super | Player mengumpulkan 100% Super Meter lalu menekan 'E' | Blaze menembakkan 5 proyektil salvo beruntun (100 total damage); Bulwark memicu Ground Slam (radius 6u, 25 damage, stun 1 detik pada seluruh musuh di area) | ✅ Terverifikasi |
| **AC-18** | Fase 6 — Status | Karakter terkena Stun atau Slow | Karakter Stun tidak dapat bergerak atau menembak selama durasi efek; Slow mengurangi move speed sebesar 30%; seluruh timer memakai scaled time (berhenti saat game di-pause) | ✅ Terverifikasi |
| **AC-19** | Fase 6 — Power-Up | Karakter menyentuh objek Health, Speed, atau Damage Boost | Efek langsung terpicu (+40 HP jika terluka, Haste 4s, Damage +30% 5s), objek visual menghilang dan muncul kembali setelah timer respawn (20s/25s/30s) | ✅ Terverifikasi |
| **AC-20** | Fase 6 — AI Tactics | Bot HP turun di bawah 25% (kondisi Retreat) | Bot beralih ke state Retreat, bergerak mencari Health Pack terdekat yang aktif di arena, dan memicu Shield (jika Bulwark) | ✅ Terverifikasi |
