using System.Collections;
using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Kebal sesaat memakai Health.SetInvulnerable.
    [RequireComponent(typeof(Health))]
    public class ShieldGadget : GadgetAbility
    {
        [SerializeField] float seconds = 1.5f;
        [SerializeField] GameObject shieldVfx;

        void OnDisable()
        {
            if (shieldVfx != null) shieldVfx.SetActive(false);
        }

        public void Setup(GameObject vfx = null, float duration = 1.5f)
        {
            shieldVfx = vfx;
            seconds = duration;
            if (shieldVfx != null) shieldVfx.SetActive(false);
        }

        public override bool Use()
        {
            GetComponent<Health>().SetInvulnerable(seconds);
            StartCoroutine(Show());
            return true;
        }

        IEnumerator Show()
        {
            if (shieldVfx != null) shieldVfx.SetActive(true);
            yield return new WaitForSeconds(seconds);
            if (shieldVfx != null) shieldVfx.SetActive(false);
        }
    }
}
