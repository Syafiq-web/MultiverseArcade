using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Proyektil blaster dengan deteksi collision presisi dan pooling lifecycle.
    /// Sesuai GDD Bagian 5.5 & 5.6.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Specs")]
        [SerializeField] private int _damage = 20;
        [SerializeField] private float _speed = 14f;
        [SerializeField] private float _lifetime = 0.8f;
        [SerializeField] private float _radius = 0.25f;

        private int _ownerTeamId = -1;
        private GameObject _owner;
        private Vector3 _velocity;
        private float _aliveTimer = 0f;
        private bool _isActive = false;

        public void Launch(Vector3 spawnPos, Vector3 direction, int teamId, GameObject owner, SO_Projectile data = null)
        {
            transform.position = spawnPos;
            transform.rotation = Quaternion.LookRotation(direction);

            _ownerTeamId = teamId;
            _owner = owner;

            if (data != null)
            {
                _damage = data.damage;
                _speed = data.speed;
                _lifetime = data.lifetime;
                _radius = data.radius;
            }

            _velocity = direction.normalized * _speed;
            _aliveTimer = 0f;
            _isActive = true;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!_isActive) return;

            float dt = Time.deltaTime;
            _aliveTimer += dt;

            if (_aliveTimer >= _lifetime)
            {
                Despawn();
                return;
            }

            Vector3 step = _velocity * dt;
            float stepDistance = step.magnitude;

            // Continuous SphereCast sweeping to guarantee hits without missing at high speeds
            Ray ray = new Ray(transform.position, _velocity.normalized);
            int layerMask = LayerMask.GetMask("Syafiq_Player", "Syafiq_Enemy", "Syafiq_Wall", "Default");

            if (Physics.SphereCast(ray, _radius, out RaycastHit hit, stepDistance, layerMask, QueryTriggerInteraction.Ignore))
            {
                HandleHit(hit.collider, hit.point, hit.normal);
                return;
            }

            transform.position += step;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;
            HandleHit(other, transform.position, -transform.forward);
        }

        private void HandleHit(Collider col, Vector3 point, Vector3 normal)
        {
            if (col == null) return;

            // Jangan tabrak diri sendiri
            if (_owner != null && (col.gameObject == _owner || col.transform.IsChildOf(_owner.transform)))
            {
                return;
            }

            Health targetHealth = col.GetComponentInParent<Health>();
            if (targetHealth != null)
            {
                // Cek Friendly Fire
                if (targetHealth.TeamId == _ownerTeamId)
                {
                    return; // Lewati rekan satu tim
                }

                DamageInfo dmg = new DamageInfo(_damage, _ownerTeamId, _owner, point, normal);
                targetHealth.TakeDamage(dmg);
                Despawn();
                return;
            }

            // Tabrak dinding atau cover
            Despawn();
        }

        public void Despawn()
        {
            if (!_isActive) return;
            _isActive = false;

            if (ProjectilePool.Instance != null)
            {
                ProjectilePool.Instance.ReturnProjectile(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
