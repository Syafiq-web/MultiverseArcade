# Rencana Eksekusi Fase 6 — Gameplay Mendalam (Arena Clash 3D)

**Project:** Multiverse Arcade  
**Game:** Arena Clash 3D (`Game_Syafiq.unity`)  
**Target:** Fase 6 SAJA (Gameplay Mendalam)  
**Engine:** Unity 2022.3.62f3 LTS  
**Tanggal:** 2026-11-14  

---

## 1. Tujuan Fase 6
Meningkatkan kedalaman mekanik gameplay Arena Clash 3D dari MVP menjadi pertempuran arena taktis yang seimbang, variatif, dan responsif, tanpa merusak arsitektur `MultiverseArcade.Core` maupun mengubah file game rekan tim lain.

---

## 2. Rincian Pekerjaan & Urutan Eksekusi

### Bagian 1: Hero Data & Selection Screen (Commit 1)
- Ekspansi `SO_Hero`:
  - `Blaze`: HP 100, Speed 5.0, Blaster 20 dmg (interval 0.4s), Skill: Dash (4u, cd 6s), Super: Salvo (5 peluru).
  - `Bulwark`: HP 160, Speed 3.8, Shotgun 3 pelet x 8 dmg (interval 0.8s), Skill: Shield (2s, -60% dmg, cd 10s), Super: Ground Slam (area radius 6u, 25 dmg, stun 1s).
- Pembuatan ScriptableObject `SO_Hero_Bulwark.asset` dan `SO_Projectile_BulwarkPellet.asset`.
- Pembuatan `HeroSelectionUI` sebelum match dimulai (2 kartu hero Blaze/Bulwark + 3 tombol tingkat kesulitan Easy/Normal/Hard + tombol Start).
- Integrasi `SaveManager` untuk menyimpan hero dan difficulty terakhir. Rematch otomatis memakai hero terakhir.

### Bagian 2: Rigidbody Non-Kinematic & Skill Dash Blaze (Commit 2)
- Verifikasi `PlayerController`: memastikan `Rigidbody.isKinematic = false`, `constraints = FreezeRotationX | FreezeRotationZ`, pergerakan berbasis velocity.
- Implementasi `PlayerSkillController`:
  - Skill Dash (Space) untuk Blaze: jarak 4u, cooldown 6s.
  - Pencegahan tembus dinding: `SphereCast` / `Raycast` deteksi collision terhadap layer `Syafiq_Wall` dan boundary arena sebelum lerp/dash.
- Pembaruan HUD: radial fill cooldown indicator untuk skill Space.

### Bagian 3: Super Meter & Super Abilities (Commit 3)
- Super charge: +10% per 20 damage diberikan, +25% per eliminasi. Max 100%.
- Super activation tombol `E` saat meter 100%:
  - Blaze: Salvo burst 5 peluru dalam 0.4s.
  - Bulwark: Ground Slam shockwave radius 6u, dealing 25 damage + 1s Stun.
- Pembaruan HUD: progress bar Super Meter + visual ready.

### Bagian 4: StatusEffectController (Commit 4)
- Komponen `StatusEffectController` di setiap karakter (`Player` dan `Bots`):
  - Slow (-30% speed, 2s)
  - Stun (kecepatan 0, tidak bisa tembak/skill, 1s)
  - Shield (-60% damage received, 2s)
  - Haste (+25% speed, 4s)
  - Damage Boost (+30% damage dealt, 5s)
- Multiplier terintegrasi ke `PlayerController`, `BotMovement`, `Health.TakeDamage`, `PlayerCombat`, dan `BotCombat`.
- Timer memakai scaled time (otomatis freeze saat pause). Reset saat death/respawn/rematch.

### Bagian 5: Power-Up System (Commit 5)
- Script `PowerUpItem.cs` & `PowerUpSpawner.cs`.
- 3 titik tetap di arena:
  1. Health Pack (+40 HP, respawn 20s) di lane utara/barat.
  2. Speed Boost (Haste 4s, respawn 25s) di lane selatan/timur.
  3. Damage Boost (+30% dmg 5s, respawn 30s) di center lane.
- AI Bot mengambil power-up jika HP < 50% (Health Pack) atau opportunistik jika dekat (< 6u).

### Bagian 6: Bot Difficulty (Easy, Normal, Hard) & Variasi Hero Bot (Commit 6)
- Data asset `SO_BotDifficulty_Easy.asset` dan `SO_BotDifficulty_Hard.asset`.
- Roster variasi: Bot di tim Blue dan Red bisa menggunakan Blaze atau Bulwark secara data-driven.
- Bot AI skill & super usage:
  - Bot Bulwark menggunakan Shield saat HP < 50% dalam pertempuran.
  - Bot Bulwark menggunakan Super Slam saat ada musuh dalam radius 5u.
  - Bot Blaze menggunakan Super Salvo saat target terkunci dalam jarak tembak.

### Bagian 7: Pembaruan Builder & EditMode Test Simulation (Commit 7)
- Perbarui `ArenaClashBuilder.cs`: menu `Multiverse > Build Arena Clash` membuat semua aset Fase 6 (Hero Selection Canvas, Power-ups, HUD super/skill, audio sfx).
- Script simulasi bot-vs-bot 50 match di Editor untuk validasi win rate (target Blue 50-60%) dan rekam ke `/docs/BALANCE_REPORT.md`.
- Pembaruan dokumen uji `/docs/ARENA_TEST.md` (AC-01 s.d. AC-15 + kasus uji Fase 6).

---

## 3. Kompilasi & Verifikasi
- Menggunakan MSBuild via CLI untuk validasi 0 error/0 warning.
- Validasi EditMode terotomatisasi via builder.
