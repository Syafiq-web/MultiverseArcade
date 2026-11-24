using System;
using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Pasang di pemain dan bot. Terisi dari damage yang BENAR-BENAR diberikan (dilaporkan oleh Projectile).
    public class SuperMeter : MonoBehaviour
    {
        [SerializeField] float damageToFull = 3200f;   // total damage yang dibutuhkan untuk mengisi penuh (disesuaikan agar responsif)

        float charge;

        public float Value01 => Mathf.Clamp01(charge / damageToFull);
        public bool IsReady => charge >= damageToFull;

        public event Action<float> OnChanged;   // nilai 0..1, untuk UI
        public event Action OnReady;
        public event Action OnUsed;

        public void SetDamageToFull(float amount) => damageToFull = amount;

        public void OnDealtDamage(int dmg)
        {
            if (IsReady || dmg <= 0) return;

            charge = Mathf.Min(damageToFull, charge + dmg);
            OnChanged?.Invoke(Value01);
            if (IsReady) OnReady?.Invoke();
        }

        public bool TryConsume()
        {
            if (!IsReady) return false;
            charge = 0;
            OnChanged?.Invoke(0);
            OnUsed?.Invoke();
            return true;
        }

        public void ResetMeter()
        {
            charge = 0;
            OnChanged?.Invoke(0);
        }
    }
}
