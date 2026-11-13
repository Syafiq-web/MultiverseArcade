using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    [CreateAssetMenu(fileName = "SO_Hero_Blaze", menuName = "Arena Clash/Hero Data")]
    public class SO_Hero : ScriptableObject
    {
        public string heroName = "Blaze";
        public string role = "Gunner Seimbang";
        public int maxHealth = 100;
        public float moveSpeed = 5.0f;
        public float fireInterval = 0.4f;
        public SO_Projectile projectileData;
    }
}
