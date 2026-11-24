using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    public enum HeroSkillType
    {
        Dash,
        Shield
    }

    public enum HeroSuperType
    {
        SalvoBurst,
        GroundSlam
    }

    /// <summary>
    /// Data ScriptableObject untuk konfigurasi hero secara data-driven.
    /// Sesuai GDD Bagian 5.4 dan 5.5.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Hero_New", menuName = "Arena Clash/Hero Data")]
    public class SO_Hero : ScriptableObject
    {
        [Header("Identity")]
        public string heroId = "blaze";
        public string heroName = "Blaze";
        public string role = "Gunner Seimbang";
        [TextArea(2, 4)]
        public string description = "Hero penembak seimbang dengan mobilitas tinggi dan salvo burst.";
        public Color heroColor = new Color(0f, 0.85f, 1f);

        [Header("Visual & Assets")]
        public Sprite portrait;
        public GameObject characterModelPrefab;
        public Material characterMaterial;

        [Header("Base Stats")]
        public int maxHealth = 100;
        public float moveSpeed = 5.0f;

        [Header("Primary Weapon")]
        public SO_Projectile projectileData;
        public float fireInterval = 0.4f;
        public int projectilesPerShot = 1; // 1 untuk Blaze, 3 untuk Shotgun Bulwark
        public float spreadAngle = 0f; // 0 untuk Blaze, 24 untuk Bulwark (-12 s.d. +12)

        [Header("Skill (Space)")]
        public HeroSkillType skillType = HeroSkillType.Dash;
        public string skillName = "Tactical Dash";
        public float skillCooldown = 6.0f;
        public float dashDistance = 4.0f; // Jarak dash Blaze
        public float shieldDuration = 2.0f; // Durasi shield Bulwark
        public float shieldDamageReduction = 0.6f; // -60% damage untuk Bulwark

        [Header("Super Ability (E)")]
        public HeroSuperType superType = HeroSuperType.SalvoBurst;
        public string superName = "Salvo Burst";
        public int superSalvoCount = 5; // 5 peluru untuk Blaze
        public float superSalvoInterval = 0.08f;
        public float slamRadius = 6.0f; // Radius hentakan Bulwark
        public int slamDamage = 25; // Damage hentakan Bulwark
        public float slamStunDuration = 1.0f; // Durasi stun 1 detik Bulwark
    }
}
