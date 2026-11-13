# Credits & Lisensi Aset — Multiverse Arcade

## 1. Tim Pengembang
- **Syafiq** — Lead Developer, Arsitektur Sistem Global & Pengembang Game 1 (Arena Clash 3D)
- **Jaya** — Game Developer (Game 2)
- **Prima** — Game Developer (Game 3)
- **Brili** — Game Developer (Game 4)
- **Risa** — Game Developer (Game 5)

---

## 2. Aset Audio (Musik & Sound Effects)
Seluruh aset audio pada proyek ini dihasilkan secara prosedural / berlisensi bebas CC0 (Public Domain):

| Nama Aset | Jenis | Lisensi | URL Sumber / Generator | Digunakan Pada |
|---|---|---|---|---|
| `bgm_arcade_action.wav` | Musik BGM | CC0 (Public Domain) | Procedural Synthwave Synthesis | Main Menu, Game Selection, Arena Clash 3D |
| `sfx_ui_click.wav` | SFX UI | CC0 (Public Domain) | Procedural Frequency Sweep Generator | Seluruh Tombol UI (Play, Settings, Back, Pause) |
| `sfx_ui_hover.wav` | SFX UI | CC0 (Public Domain) | Procedural Frequency Sweep Generator | Hover Tombol UI |
| `sfx_laser_shoot.wav` | SFX Gameplay | CC0 (Public Domain) | Procedural Sound Generator | Tembakan Plasma Rifle & Shotgun |
| `sfx_enemy_hit.wav` | SFX Gameplay | CC0 (Public Domain) | Procedural Sound Generator | Respon Tertembak Zombie |
| `sfx_player_hurt.wav` | SFX Gameplay | CC0 (Public Domain) | Procedural Sound Generator | Respon Kerusakan Player |
| `sfx_explosion.wav` | SFX Gameplay | CC0 (Public Domain) | Procedural Sound Generator | Kematian Zombie & Slime Splatter |
| `sfx_game_over.wav` | SFX UI/Gameplay | CC0 (Public Domain) | Procedural Sound Generator | Layar Game Over |
| `sfx_weapon_switch.wav` | SFX Gameplay | CC0 (Public Domain) | Procedural Sound Generator | Ganti Senjata (Plasma Rifle / Shotgun) |

---

## 3. Tipografi & Font
| Nama Font | Pembuat | Lisensi | URL Sumber | Digunakan Pada |
|---|---|---|---|---|
| Liberation Sans SDF | Red Hat / Unity Technologies | SIL Open Font License (OFL) v1.1 | https://github.com/liberationfonts | Seluruh Teks TextMesh Pro (Header, Card, Dialog, HUD) |

---

## 4. Model 3D & Grafis
- **Arena, Tembok, & Karakter:** Dibuat secara prosedural menggunakan Unity Primitive Meshes (Cylinder, Cube, Sphere) dengan shader Universal Render Pipeline (URP Lit / Simple Lit) untuk memastikan performa tinggi (< 300 draw calls) dan kompatibilitas tanpa dependensi eksternal berbayar.
- **Ikon UI & Gradient:** Prosedural Unity Canvas UGUI primitives & procedural sprites.
