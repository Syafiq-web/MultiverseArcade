using UnityEngine;
using MultiverseArcade.Core;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Proyektil blaster dengan deteksi collision presisi, warna laser dinamis per tim,
    /// trail renderer, dan efek benturan (impact).
    /// Sesuai GDD Bagian 5.5 & 5.6.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Specs")]
        [SerializeField] private int _damage = 20;
        [SerializeField] private float _speed = 14f;
        [SerializeField] private float _lifetime = 0.8f;
        [SerializeField] private float _radius = 0.25f;

        [Header("Effects & Audio")]
        [SerializeField] private AudioClip _hitSound;

        private int _ownerTeamId = -1;
        private GameObject _owner;
        private Vector3 _velocity;
        private float _aliveTimer = 0f;
        private bool _isActive = false;

        private Renderer _renderer;
        private TrailRenderer _trail;
        private MaterialPropertyBlock _mpb;

        private static readonly Color BlueLaserColor = new Color(0f, 1f, 0.95f, 1f);
        private static readonly Color RedLaserColor = new Color(1f, 0.25f, 0.3f, 1f);

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _trail = GetComponent<TrailRenderer>();
            _mpb = new MaterialPropertyBlock();

            if (_hitSound == null)
            {
                #if UNITY_EDITOR
                _hitSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Games/Syafiq/Audio/sfx_enemy_hit.wav");
                #endif
            }
        }

        public void Launch(Vector3 spawnPos, Vector3 direction, int teamId, GameObject owner, SO_Projectile data = null, float damageMultiplier = 1.0f)
        {
            transform.position = spawnPos;
            transform.rotation = Quaternion.LookRotation(direction);

            _ownerTeamId = teamId;
            _owner = owner;

            if (data != null)
            {
                _damage = Mathf.RoundToInt(data.damage * damageMultiplier);
                _speed = data.speed;
                _lifetime = data.lifetime;
                _radius = data.radius;
            }
            else
            {
                _damage = Mathf.RoundToInt(_damage * damageMultiplier);
            }

            _velocity = direction.normalized * _speed;
            _aliveTimer = 0f;
            _isActive = true;

            // Pewarnaan laser & reset trail sesuai tim penembak
            Color laserColor = (teamId == 0) ? BlueLaserColor : RedLaserColor;
            if (_renderer != null)
            {
                if (_mpb == null) _mpb = new MaterialPropertyBlock();
                _renderer.GetPropertyBlock(_mpb);
                _mpb.SetColor("_BaseColor", laserColor);
                _mpb.SetColor("_Color", laserColor);
                _renderer.SetPropertyBlock(_mpb);
            }

            if (_trail != null)
            {
                _trail.Clear();
                _trail.startColor = laserColor;
                _trail.endColor = new Color(laserColor.r, laserColor.g, laserColor.b, 0f);
            }

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

                // Play hit sound effect
                if (_hitSound != null && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(_hitSound, 0.65f);
                }

                SpawnImpactEffect(point, normal, true);
                Despawn();
                return;
            }

            // Tabrak dinding atau cover
            SpawnImpactEffect(point, normal, false);
            Despawn();
        }

        private void SpawnImpactEffect(Vector3 point, Vector3 normal, bool hitEnemy)
        {
            GameObject spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spark.name = "ImpactSpark";
            spark.transform.position = point + normal * 0.1f;
            spark.transform.localScale = Vector3.one * (hitEnemy ? 0.32f : 0.18f);

            Collider c = spark.GetComponent<Collider>();
            if (c != null) Destroy(c);

            Renderer r = spark.GetComponent<Renderer>();
            if (r != null)
            {
                Shader unlit = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
                if (unlit != null)
                {
                    Material m = new Material(unlit);
                    m.color = hitEnemy ? new Color(1f, 0.95f, 0.3f) : new Color(0.3f, 0.85f, 1f);
                    r.sharedMaterial = m;
                }
            }

            Destroy(spark, 0.08f);
        }

        public void Despawn()
        {
            if (!_isActive) return;
            _isActive = false;

            if (_trail != null)
            {
                _trail.Clear();
            }

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
