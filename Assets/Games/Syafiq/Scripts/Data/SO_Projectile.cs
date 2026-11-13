using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    [CreateAssetMenu(fileName = "SO_Projectile_Blaze", menuName = "Arena Clash/Projectile Data")]
    public class SO_Projectile : ScriptableObject
    {
        [Tooltip("Besar damage proyektil saat mengenai musuh")]
        public int damage = 20;

        [Tooltip("Kecepatan luncur proyektil (u/s)")]
        public float speed = 14f;

        [Tooltip("Batas waktu hidup proyektil dalam detik")]
        public float lifetime = 0.8f;

        [Tooltip("Radius collider proyektil")]
        public float radius = 0.25f;
    }
}
