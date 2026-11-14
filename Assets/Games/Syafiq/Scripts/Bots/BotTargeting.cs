using System.Collections.Generic;
using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Sistem pemilihan target AI berbasis Utility Scoring berkala (0.3 - 0.5 s).
    /// Sesuai GDD Bagian 5.9 dan Test Matrix AC-08.
    /// </summary>
    public class BotTargeting : MonoBehaviour
    {
        [Header("Weights")]
        [SerializeField] private float _wDistance = 0.4f;
        [SerializeField] private float _wHealth = 0.3f;
        [SerializeField] private float _wAttackingMe = 0.2f;
        [SerializeField] private float _wLineOfSight = 0.1f;

        [Header("Detection Range")]
        [SerializeField] private float _maxDetectionRange = 22f;

        private Health _myHealth;
        private Transform _playerTransform;
        private Health _currentTarget;
        private float _evalTimer = 0f;
        private float _evalInterval = 0.4f;

        public Health CurrentTarget => _currentTarget;
        public bool HasValidTarget => _currentTarget != null && !_currentTarget.IsDead && _currentTarget.gameObject.activeInHierarchy;

        private void Awake()
        {
            _myHealth = GetComponent<Health>();
            _evalInterval = Random.Range(0.35f, 0.45f); // Stagger interval to avoid CPU spikes
        }

        private void Start()
        {
            FindPlayerReference();
        }

        private void FindPlayerReference()
        {
            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null) _playerTransform = pc.transform;
        }

        public void UpdateTargeting(float dt)
        {
            _evalTimer += dt;
            if (_evalTimer >= _evalInterval)
            {
                _evalTimer = 0f;
                EvaluateBestTarget();
            }

            // Validasi target saat ini
            if (_currentTarget != null && (_currentTarget.IsDead || !_currentTarget.gameObject.activeInHierarchy))
            {
                _currentTarget = null;
                EvaluateBestTarget();
            }
        }

        public void EvaluateBestTarget()
        {
            if (_myHealth == null) return;

            Health[] allCharacters = FindObjectsOfType<Health>();
            Health bestTarget = null;
            float highestScore = -1f;

            foreach (var charHealth in allCharacters)
            {
                if (charHealth == null || charHealth == _myHealth) continue;
                if (charHealth.IsDead || !charHealth.gameObject.activeInHierarchy) continue;

                // Jangan target rekan satu tim!
                if (charHealth.TeamId == _myHealth.TeamId) continue;

                float dist = Vector3.Distance(transform.position, charHealth.transform.position);
                if (dist > _maxDetectionRange) continue;

                // Utility Formula GDD 5.9:
                // score = w1*(1 - dist/range) + w2*(1 - HP/maxHP) + w3*(attackingMe) + w4*(clearLoS)
                float distFactor = Mathf.Clamp01(1f - (dist / _maxDetectionRange));
                float hpFactor = Mathf.Clamp01(1f - ((float)charHealth.CurrentHealth / Mathf.Max(1, charHealth.MaxHealth)));
                float attackingMeFactor = 0f;
                var targetBot = charHealth.GetComponent<BotTargeting>();
                if (targetBot != null && targetBot.CurrentTarget == _myHealth) attackingMeFactor = 1f;
                float losFactor = HasClearLineOfSight(charHealth.transform.position) ? 1f : 0.2f;

                float score = (_wDistance * distFactor) + (_wHealth * hpFactor) + (_wAttackingMe * attackingMeFactor) + (_wLineOfSight * losFactor);

                if (score > highestScore)
                {
                    highestScore = score;
                    bestTarget = charHealth;
                }
            }

            _currentTarget = bestTarget;
        }

        public bool HasClearLineOfSight(Vector3 targetPos)
        {
            Vector3 origin = transform.position + Vector3.up * 0.8f;
            Vector3 dir = (targetPos + Vector3.up * 0.8f) - origin;
            float dist = dir.magnitude;

            if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, LayerMask.GetMask("Syafiq_Wall", "Default")))
            {
                return false; // Terhalang tembok
            }
            return true;
        }

        public void SetEvaluationInterval(float interval)
        {
            _evalInterval = Mathf.Max(0.1f, interval);
        }

        public Transform GetPlayerTransform()
        {
            if (_playerTransform == null) FindPlayerReference();
            return _playerTransform;
        }
    }
}
