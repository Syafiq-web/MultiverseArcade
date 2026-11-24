using System;
using System.Collections;
using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Taruh di objek kosong. Mengatur delay dan lokasi respawn tiap tim (markas).
    public class RespawnManager : MonoBehaviour
    {
        public static RespawnManager Instance { get; private set; }

        [Serializable]
        public class TeamSpawns
        {
            public int teamId;
            public Transform[] points;
        }

        [SerializeField] TeamSpawns[] teams;
        [SerializeField] float respawnDelay = 3f;
        [SerializeField] GemGrabManager match;       // opsional: respawn berhenti setelah match selesai

        public event Action<Respawnable, float> OnRespawnTick;   // (unit, sisaDetik); 0 = selesai
        public event Action<Respawnable> OnRespawned;

        void Awake()
        {
            Instance = this;
        }

        public void Setup(TeamSpawns[] teamSpawns, GemGrabManager manager, float delay = 3f)
        {
            teams = teamSpawns;
            match = manager;
            respawnDelay = delay;
        }

        public void Schedule(Respawnable unit) => StartCoroutine(RespawnRoutine(unit));

        IEnumerator RespawnRoutine(Respawnable unit)
        {
            yield return null;                        // beri waktu handler OnDeath lain (jatuhkan gem, VFX) selesai
            if (unit == null) yield break;

            unit.gameObject.SetActive(false);

            float t = respawnDelay;
            while (t > 0)
            {
                OnRespawnTick?.Invoke(unit, t);
                t -= Time.deltaTime;
                yield return null;
            }
            OnRespawnTick?.Invoke(unit, 0);

            if (match != null && match.WinningTeam >= 0) yield break;
            if (unit == null) yield break;

            PickSpawn(unit, out Vector3 pos, out Quaternion rot);
            unit.Respawn(pos, rot);
            OnRespawned?.Invoke(unit);
        }

        // Pilih titik spawn yang paling jauh dari musuh terdekat (anti spawn-kill)
        void PickSpawn(Respawnable unit, out Vector3 pos, out Quaternion rot)
        {
            pos = unit.StartPos;
            rot = unit.StartRot;

            if (teams == null) return;

            foreach (var team in teams)
            {
                if (team == null || team.teamId != unit.TeamId || team.points == null || team.points.Length == 0) continue;

                float bestScore = float.MinValue;
                foreach (var p in team.points)
                {
                    if (p == null) continue;
                    float nearestEnemy = float.MaxValue;
                    foreach (var c in GemCarrier.All)
                    {
                        if (c == null || c.teamId == unit.TeamId) continue;
                        nearestEnemy = Mathf.Min(nearestEnemy, Vector3.Distance(p.position, c.transform.position));
                    }
                    if (nearestEnemy > bestScore)
                    {
                        bestScore = nearestEnemy;
                        pos = p.position;
                        rot = p.rotation;
                    }
                }
                return;
            }
        }
    }
}
