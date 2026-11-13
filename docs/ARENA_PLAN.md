# Rencana Implementasi MVP Arena Clash 3D (Fase 1 - 5)
**Game:** Arena Clash 3D (Game Syafiq)  
**Dokumen Referensi:** Multiverse Arcade — Complex Game Design Document v2.0 (Bab 5)  
**Engine:** Unity 2022.3.62f3 LTS + Universal Render Pipeline (URP 14.0.12)  
**Target:** MVP 3v3 Team Deathmatch (Player + 2 Bot Blue vs 3 Bot Red)  

---

## 1. Arsitektur Folder & Script (`Assets/Games/Syafiq/`)
```text
Assets/Games/Syafiq/
├── Data/              (ScriptableObjects: SO_Hero, SO_Projectile, SO_MatchSettings, SO_BotDifficulty)
├── Materials/         (Material lantai, dinding, kapsul hero biru/merah, proyektil, cover)
├── Audio/             (sfx_laser_shoot, sfx_enemy_hit, sfx_player_hurt, sfx_explosion, bgm_arcade_action)
├── Prefabs/           (PF_Projectile, PF_DamageNumber)
└── Scripts/
    ├── Core/          ArenaGameEntry.cs (IArcadeGame), ArenaEvents.cs
    ├── Data/          SO_Hero.cs, SO_Projectile.cs, SO_MatchSettings.cs, SO_BotDifficulty.cs
    ├── Player/        PlayerController.cs, PlayerCombat.cs
    ├── Combat/        Health.cs, Projectile.cs, ProjectilePool.cs, DamageInfo.cs
    ├── Bots/          BotBrain.cs (FSM: Idle, Seek, Chase, Attack, Strafe, Retreat, Dead), BotTargeting.cs, BotMovement.cs, BotCombat.cs
    ├── Match/         ArenaMatchManager.cs, SpawnManager.cs
    ├── Camera/        TopDownCamera.cs
    └── UI/            ArenaHUD.cs, HealthBarWorld.cs, KillFeedUI.cs, ResultPanelUI.cs, DamageNumber.cs, AimIndicator.cs
```

---

## 2. Rincian Fase Eksekusi

### Fase 1: Arena Blockout, Kamera Top-Down, & Player Movement/Aiming
1. **Arena Blockout (30x30 u simetris):**
   - Lantai 30x30 di layer `Syafiq_Ground`.
   - Dinding pembatas tebal (2 u) tinggi (2 u) dengan BoxCollider di layer `Syafiq_Wall`.
   - Zona Blue Spawn (-10, 0, -10) & Red Spawn (10, 0, 10).
   - Central Zone dengan 5 pilar/obstacle cover simetris dengan NavMeshObstacle.
2. **Kamera Top-Down Angled (`TopDownCamera.cs`):**
   - Sudut X ~55°, jarak Y ~13, Z offset ~ -9.
   - Smooth damp follow (0.15s) + look-ahead ke arah posisi mouse.
   - Dilengkapi `UniversalAdditionalCameraData`.
3. **Player Movement & Aiming (`PlayerController.cs`):**
   - CapsuleCollider + Rigidbody (interpolated, freeze rotation X/Z).
   - Gerak WASD dengan vektor input dinormalisasi (kecepatan diagonal konsisten = 5.0 u/s).
   - Bidik mouse menggunakan `Physics.Raycast` / Plane ke bidang lantai `Y = 0` (rotasi hanya pada sumbu Y).

### Fase 2: Proyektil, Pooling, Combat, & Health
1. **`DamageInfo` (struct):** `amount`, `attackerTeamId`, `attackerTransform`, `hitPoint`.
2. **`Health.cs`:**
   - HP 100 / MaxHP 100.
   - Menolak damage dari tim yang sama (`Team ID` sama = Friendly Fire OFF).
   - Menolak damage jika sudah mati (`isDead`), mati hanya sekali (`Die()`).
   - Memicu event `OnHealthChanged(current, max)` dan `OnCharacterDied(victim, attackerTeamId)`.
3. **`Projectile.cs` & `ProjectilePool.cs` (Pool 60):**
   - Kecepatan 14 u/s, jangkauan 10 u, lifetime 0.8s, damage 20.
   - Layer `Syafiq_Projectile`.
   - Mengabaikan tabrakan sesama proyektil (`Physics.IgnoreLayerCollision`).
   - Tabrakan dengan tembok atau musuh mengembalikan proyektil ke pool + spawn efek.
4. **`PlayerCombat.cs`:**
   - Interval tembakan 0.4s (fire rate 2.5 shot/sec).
   - Mengambil proyektil dari `ProjectilePool`.
   - Menggunakan audio SFX via `AudioManager.Instance.PlaySFX`.

### Fase 3: NavMesh & Bot AI (3v3 FSM)
1. **NavMesh Baking:**
   - Komponen `NavMeshSurface` di-bake dengan agent radius 0.5, step height rendah.
2. **`BotBrain.cs` (FSM States):**
   - `Idle`, `Seek` (menuju titik tengah/patroli), `Chase` (mengejar target), `Attack` (berhenti & menembak), `Strafe` (bergerak menyamping sambil menembak), `Retreat` (mundur saat HP < 25%), `Dead`.
3. **`BotTargeting.cs` (Utility Scoring berkala 0.3 - 0.5s):**
   - $Score = 0.4 \cdot (1 - dist/range) + 0.3 \cdot (1 - HP/maxHP) + 0.2 \cdot (targetAttackingMe) + 0.1 \cdot (clearLoS)$.
   - Menghindari fokus pada target yang sudah dikejar 2 bot lain.
   - Stuck detection: jika bot terjebak > 1 detik, recalculate rute.
4. **Perilaku Tim:**
   - Bot rekan (Blue) mengikuti pemain jika tidak ada musuh dalam radius 7 u.
   - Tidak ada friendly fire. Visual kapsul jelas (Biru vs Merah).

### Fase 4: Team ID & Match Flow Manager
1. **Format 3v3:**
   - Tim Blue (ID = 0): 1 Player + 2 Bot Blue.
   - Tim Red (ID = 1): 3 Bot Red.
2. **`ArenaMatchManager.cs`:**
   - Target eliminasi: 10 skor, atau timer 180 detik.
   - Match States: `Countdown` (3-2-1-GO, input terkunci) ➔ `Playing` ➔ `Finished`.
   - Evaluasi kemenangan: Blue Wins / Red Wins / Draw.
   - Saat `Finished`: input dinonaktifkan, bot berhenti, timer berhenti.

### Fase 5: HUD Lengkap, Pause, Result Panel, Restart & Persistensi
1. **`ArenaHUD.cs`:**
   - Top Header: Skor Blue — Timer (berubah merah pada 10 detik terakhir) — Skor Red.
   - Player HUD: Bar HP player di kiri bawah.
   - Aim Indicator: Garis laser reticle di lantai.
   - World-Space Health Bar di atas kepala semua karakter (warna biru untuk tim Blue, merah untuk tim Red).
   - Kill Feed di pojok kanan atas (`"Blaze ➜ Red Bot 02"`).
   - Floating Damage Numbers melayang saat serangan mengenai target.
2. **Pause Integration:**
   - Memakai `GlobalPauseOverlay` (Esc) yang sudah ada di Hub.
   - `IArcadeGame.OnGamePause()` dan `OnGameResume()`.
3. **Result Panel:**
   - Banner Pemenang (VICTORY / DEFEAT / DRAW).
   - Skor akhir & statistik (Kills, Deaths).
   - Tombol **REMATCH** (Restart match) dan **SWITCH GAME** (kembali ke GameSelection via Hub).
4. **Restart & Persistensi:**
   - Restart me-reset skor, timer, posisi semua karakter, pool proyektil, dan `Time.timeScale = 1.0f`.
   - Simpan High Score ke `SaveManager.Instance.SetHighScore("syafiq", score)`.
   - Memastikan `GameRegistry` untuk Arena Clash tetap aktif.

---

## 3. Editor Automation (`Multiverse > Build Arena Clash`)
Membuat script editor `ArenaClashBuilder.cs` yang otomatis merakit scene `Game_Syafiq.unity`, membake NavMesh, mengonfigurasi prefabs dan ScriptableObjects, serta memvalidasi ketiadaan missing reference.
