using System;
using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Komponen Health untuk brawler dan objek pertempuran.
    // Menyediakan event OnDamaged (untuk feedback/Super meter), OnDeath, dan OnHealthChanged.
    public class Health : MonoBehaviour
    {
        public int Max = 3200;
        public int Current;

        // true = objek dihancurkan saat mati (dummy/target latihan).
        // Respawnable otomatis mematikan ini untuk pemain dan bot.
        public bool destroyOnDeath = true;

        public bool IsDead { get; private set; }
        public bool IsInvulnerable => Time.time < invulnerableUntil;

        public event Action<Health, int> OnDamaged;   // (health, jumlah damage)
        public event Action<Health> OnDeath;
        public event Action<int, int> OnHealthChanged; // (current, max) untuk slider HUD

        float invulnerableUntil;

        void Awake()
        {
            Current = Max;
        }

        public void TakeDamage(int dmg)
        {
            if (IsDead || IsInvulnerable || dmg <= 0) return;

            Current = Mathf.Max(0, Current - dmg);
            OnDamaged?.Invoke(this, dmg);
            OnHealthChanged?.Invoke(Current, Max);

            if (Current <= 0)
            {
                IsDead = true;
                OnDeath?.Invoke(this);               // GemCarrier menjatuhkan gem di sini
                if (destroyOnDeath) Destroy(gameObject);
            }
        }

        public void SetInvulnerable(float seconds) => invulnerableUntil = Time.time + seconds;

        public void ResetFull()
        {
            IsDead = false;
            Current = Max;
            OnHealthChanged?.Invoke(Current, Max);
        }
    }
}
