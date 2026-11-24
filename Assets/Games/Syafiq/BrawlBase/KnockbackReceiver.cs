using UnityEngine;
using UnityEngine.AI;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Pasang di BOT agar bisa terdorong. Pemain memakai PlayerController.AddKnockback.
    [RequireComponent(typeof(NavMeshAgent))]
    public class KnockbackReceiver : MonoBehaviour
    {
        [SerializeField] float decay = 8f;

        NavMeshAgent agent;
        Vector3 vel;

        void Awake()     => agent = GetComponent<NavMeshAgent>();
        void OnDisable() => vel = Vector3.zero;

        public void Add(Vector3 force)
        {
            force.y = 0;
            vel += force;
        }

        void Update()
        {
            if (vel.sqrMagnitude < 0.01f) return;
            if (agent != null && agent.isOnNavMesh) agent.Move(vel * Time.deltaTime);
            vel = Vector3.Lerp(vel, Vector3.zero, decay * Time.deltaTime);
        }
    }

    public static class Knockback
    {
        public static void Apply(GameObject target, Vector3 force)
        {
            if (target.TryGetComponent(out PlayerController pc)) pc.AddKnockback(force);
            else if (target.TryGetComponent(out KnockbackReceiver kr)) kr.Add(force);
        }
    }
}
