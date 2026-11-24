using UnityEngine;
using UnityEngine.AI;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Butuh: NavMeshAgent, GemCarrier, Health, Hideable. Bake NavMesh di scene.
    [RequireComponent(typeof(NavMeshAgent), typeof(GemCarrier), typeof(Health))]
    public class BotAI : MonoBehaviour
    {
        enum State { Collect, Attack, Retreat }

        [Header("Senjata")]
        [SerializeField] Projectile projectilePrefab;
        [SerializeField] Transform muzzle;
        [SerializeField] float attackRange = 7f;
        [SerializeField] float fireCooldown = 0.9f;
        [SerializeField, Range(0, 20)] float aimError = 6f; // derajat, supaya bot tidak sempurna

        [Header("Persepsi")]
        [SerializeField] float sightRange = 9f;
        [SerializeField] float hiddenDetectRange = 2.5f; // musuh di semak baru terlihat jika sedekat ini
        [SerializeField] LayerMask unitMask;

        [Header("Perilaku")]
        [SerializeField, Range(0, 1)] float retreatHpPercent = 0.35f;
        [SerializeField, Range(0, 1)] float recoverHpPercent = 0.8f;
        [SerializeField] float thinkInterval = 0.2f; // AI tidak perlu berpikir tiap frame
        [SerializeField] float regenPerSecond = 400f;
        [SerializeField] float regenDelay = 3f;

        State state = State.Collect;
        NavMeshAgent agent;
        GemCarrier carrier;
        Health hp;
        GemMine mine;
        Transform target;
        float nextThink, nextFire, lastDamageTime;
        int lastHp;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            carrier = GetComponent<GemCarrier>();
            hp = GetComponent<Health>();
            if (muzzle == null) muzzle = transform;
        }

        void Start()
        {
            mine = Object.FindFirstObjectByType<GemMine>();
            if (hp != null) lastHp = hp.Current;
        }

        void Update()
        {
            Regen();

            if (Time.time >= nextThink)
            {
                nextThink = Time.time + thinkInterval;
                Think();
            }

            // Menembak dilakukan tiap frame agar responsif
            if (state == State.Attack && target != null) TryShoot();
        }

        // ---------- Keputusan ----------
        void Think()
        {
            if (hp == null || agent == null || !agent.isOnNavMesh) return;

            target = FindEnemy();
            float hpPercent = (float)hp.Current / Mathf.Max(1, hp.Max);

            if (hpPercent <= retreatHpPercent) state = State.Retreat;
            else if (state == State.Retreat && hpPercent >= recoverHpPercent) state = State.Collect;

            if (state != State.Retreat)
            {
                state = target != null ? State.Attack : State.Collect;
            }

            switch (state)
            {
                case State.Collect: DoCollect(); break;
                case State.Attack:  DoAttack();  break;
                case State.Retreat: DoRetreat(); break;
            }
        }

        void DoCollect()
        {
            if (agent == null || !agent.isOnNavMesh) return;
            agent.isStopped = false;

            Gem best = null;
            float bestD = float.MaxValue;
            foreach (var g in Gem.All)
            {
                if (g == null) continue;
                float d = (g.transform.position - transform.position).sqrMagnitude;
                if (d < bestD) { bestD = d; best = g; }
            }

            if (best != null) agent.SetDestination(best.transform.position);
            else if (mine != null) agent.SetDestination(mine.transform.position); // tunggu dekat tambang
        }

        void DoAttack()
        {
            if (agent == null || !agent.isOnNavMesh) return;
            if (target == null) { DoCollect(); return; }

            float dist = Vector3.Distance(transform.position, target.position);
            if (dist > attackRange * 0.75f)
            {
                agent.isStopped = false;
                agent.SetDestination(target.position);
            }
            else
            {
                agent.isStopped = true; // sudah dalam jangkauan tembak
            }
        }

        void DoRetreat()
        {
            if (agent == null || !agent.isOnNavMesh) return;
            Bush bush = FindNearestBush();
            if (bush != null)
            {
                agent.isStopped = false;
                agent.SetDestination(bush.transform.position);
            }
            else
            {
                // Menjauh dari musuh terdekat jika tidak ada semak
                if (target != null)
                {
                    Vector3 away = transform.position + (transform.position - target.position).normalized * 5f;
                    agent.isStopped = false;
                    agent.SetDestination(away);
                }
            }
        }

        // ---------- Persepsi ----------
        Transform FindEnemy()
        {
            Transform best = null;
            float bestD = float.MaxValue;
            int mask = unitMask != 0 ? unitMask.value : LayerMask.GetMask("Unit", "Default");

            foreach (var col in Physics.OverlapSphere(transform.position, sightRange, mask))
            {
                if (col.gameObject == gameObject) continue;
                if (!col.TryGetComponent(out GemCarrier other) || (carrier != null && other.teamId == carrier.teamId)) continue;

                float d = Vector3.Distance(transform.position, col.transform.position);

                // Musuh di dalam semak hanya terlihat jika sangat dekat
                if (col.TryGetComponent(out Hideable h) && h.InBush && d > hiddenDetectRange) continue;

                if (d < bestD) { bestD = d; best = col.transform; }
            }
            return best;
        }

        Bush FindNearestBush()
        {
            Bush best = null;
            float bestD = float.MaxValue;
            foreach (var b in Object.FindObjectsByType<Bush>(FindObjectsSortMode.None))
            {
                if (b == null) continue;
                float d = (b.transform.position - transform.position).sqrMagnitude;
                if (d < bestD) { bestD = d; best = b; }
            }
            return best;
        }

        // ---------- Aksi ----------
        void TryShoot()
        {
            if (target == null) return;
            Vector3 to = target.position - transform.position;
            to.y = 0;
            if (to.magnitude > attackRange) return;

            // Hadap musuh
            if (to.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to), 12f * Time.deltaTime);
            }

            if (Time.time < nextFire) return;
            nextFire = Time.time + fireCooldown;

            // Tambahkan error bidik
            Quaternion err = Quaternion.Euler(0, Random.Range(-aimError, aimError), 0);
            Vector3 shootDir = err * to.normalized;

            Vector3 spawnPos = muzzle != null ? muzzle.position : transform.position + shootDir * 0.8f + Vector3.up * 0.5f;
            if (projectilePrefab != null)
            {
                var p = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(shootDir));
                p.Init(attackRange + 1f, gameObject);
            }

            // Reveal bot dari semak saat menembak
            Hideable h = GetComponent<Hideable>();
            if (h != null) h.RevealFor(2.0f);
        }

        void Regen()
        {
            if (hp == null) return;
            if (hp.Current < lastHp) lastDamageTime = Time.time;
            if (Time.time - lastDamageTime > regenDelay && hp.Current < hp.Max)
            {
                hp.Current = Mathf.Min(hp.Max, hp.Current + Mathf.RoundToInt(regenPerSecond * Time.deltaTime));
            }
            lastHp = hp.Current;
        }

        public void Setup(Projectile proj, Transform muz, LayerMask mask)
        {
            projectilePrefab = proj;
            muzzle = muz;
            unitMask = mask;
        }
    }
}
