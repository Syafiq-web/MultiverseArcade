using System;
using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Pasang di pemain. Menghitung jatah pakai gadget per pertandingan.
    public class GadgetSlot : MonoBehaviour
    {
        [SerializeField] GadgetAbility gadget;
        [SerializeField] int maxUses = 3;
        [SerializeField] float cooldown = 1f;     // jeda anti-spam antar pemakaian

        public int UsesLeft { get; private set; }
        public int MaxUses => maxUses;
        public event Action<int> OnUsesChanged;

        float nextUse;
        Health hp;

        void Awake()
        {
            UsesLeft = maxUses;
            hp = GetComponent<Health>();
            if (gadget == null) gadget = GetComponent<GadgetAbility>();
        }

        public void Setup(GadgetAbility ability, int uses = 3, float cd = 1f)
        {
            gadget = ability;
            maxUses = uses;
            cooldown = cd;
            UsesLeft = maxUses;
            OnUsesChanged?.Invoke(UsesLeft);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Q))
            {
                Use();
            }
        }

        public bool CanUse => UsesLeft > 0 && Time.time >= nextUse && (hp == null || !hp.IsDead);

        // Hubungkan ke Button.onClick (lewat GadgetButtonUI)
        public void Use()
        {
            if (gadget == null) gadget = GetComponent<GadgetAbility>();
            if (!CanUse || gadget == null || !gadget.Use()) return;

            UsesLeft--;
            nextUse = Time.time + cooldown;
            OnUsesChanged?.Invoke(UsesLeft);
        }

        public void Refill()
        {
            UsesLeft = maxUses;
            OnUsesChanged?.Invoke(UsesLeft);
        }
    }
}
