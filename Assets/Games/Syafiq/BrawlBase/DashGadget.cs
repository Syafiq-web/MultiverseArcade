using System.Collections;
using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Dash singkat ke arah hadap karakter. Berhenti otomatis jika menabrak dinding (CharacterController).
    [RequireComponent(typeof(CharacterController))]
    public class DashGadget : GadgetAbility
    {
        [SerializeField] Transform model;       // arah hadap (sama dengan model di PlayerController)
        [SerializeField] float distance = 4.5f;
        [SerializeField] float duration = 0.18f;

        bool dashing;

        void OnDisable() => dashing = false;

        public void Setup(Transform playerModel, float dashDistance = 4.5f, float dashDuration = 0.18f)
        {
            model = playerModel;
            distance = dashDistance;
            duration = dashDuration;
        }

        public override bool Use()
        {
            if (dashing) return false;
            StartCoroutine(Dash());
            return true;
        }

        IEnumerator Dash()
        {
            dashing = true;
            var cc = GetComponent<CharacterController>();

            Vector3 dir = model != null ? model.forward : transform.forward;
            dir.y = 0;
            dir.Normalize();

            float speed = distance / duration;
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                if (cc != null && cc.enabled)
                {
                    cc.Move(dir * speed * Time.deltaTime);
                }
                yield return null;
            }
            dashing = false;
        }
    }
}
