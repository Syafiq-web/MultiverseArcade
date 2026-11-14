using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MultiverseArcade.Core;

namespace MultiverseArcade.Syafiq
{
    public enum PowerUpType
    {
        HealthPack,     // +40 HP (Respawn 20s)
        SpeedBoost,     // Haste 4s (Respawn 25s)
        DamageBoost     // +30% Damage 5s (Respawn 30s)
    }

    /// <summary>
    /// Objek pickup power-up di arena dengan trigger deteksi, animasi rotasi/bobbing,
    /// dan timer respawn otomatis. Sesuai GDD Bagian 5.7.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class PowerUpItem : MonoBehaviour
    {
        public static readonly List<PowerUpItem> ActivePowerUps = new List<PowerUpItem>();

        [Header("Config")]
        [SerializeField] private PowerUpType _type = PowerUpType.HealthPack;
        [SerializeField] private float _respawnTime = 20f;
        [SerializeField] private int _healAmount = 40;
        [SerializeField] private float _hasteDuration = 4.0f;
        [SerializeField] private float _damageBoostDuration = 5.0f;

        [Header("Visual & Audio")]
        [SerializeField] private GameObject _visualModel;
        [SerializeField] private AudioClip _pickupSound;

        private SphereCollider _collider;
        private bool _isAvailable = true;
        private Vector3 _initialLocalPos;
        private Coroutine _respawnRoutine;

        public PowerUpType Type => _type;
        public bool IsAvailable => _isAvailable;

        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();
            _collider.isTrigger = true;
            _collider.radius = 1.0f;

            if (_visualModel == null && transform.childCount > 0)
            {
                _visualModel = transform.GetChild(0).gameObject;
            }

            if (_visualModel != null)
            {
                _initialLocalPos = _visualModel.transform.localPosition;
            }

            // Durasi respawn sesuai GDD 5.7
            switch (_type)
            {
                case PowerUpType.HealthPack:
                    _respawnTime = 20f;
                    break;
                case PowerUpType.SpeedBoost:
                    _respawnTime = 25f;
                    break;
                case PowerUpType.DamageBoost:
                    _respawnTime = 30f;
                    break;
            }
        }

        private void OnEnable()
        {
            if (!ActivePowerUps.Contains(this))
            {
                ActivePowerUps.Add(this);
            }
        }

        private void OnDisable()
        {
            ActivePowerUps.Remove(this);
            if (_respawnRoutine != null) StopCoroutine(_respawnRoutine);
        }

        private void Update()
        {
            if (!_isAvailable || _visualModel == null) return;

            // Animasi rotasi halus dan melayang (bobbing)
            _visualModel.transform.Rotate(Vector3.up, 60f * Time.deltaTime, Space.World);
            float bobOffset = Mathf.Sin(Time.time * 3f) * 0.12f;
            _visualModel.transform.localPosition = _initialLocalPos + new Vector3(0, bobOffset, 0);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isAvailable) return;

            Health targetHealth = other.GetComponent<Health>();
            if (targetHealth == null || targetHealth.IsDead) return;

            bool consumed = false;

            switch (_type)
            {
                case PowerUpType.HealthPack:
                    // Ambil jika HP belum penuh
                    if (targetHealth.CurrentHealth < targetHealth.MaxHealth)
                    {
                        targetHealth.Heal(_healAmount);
                        consumed = true;
                    }
                    break;

                case PowerUpType.SpeedBoost:
                    StatusEffectController secSpeed = other.GetComponent<StatusEffectController>();
                    if (secSpeed != null)
                    {
                        secSpeed.ApplyEffect(StatusEffectType.Haste, _hasteDuration);
                        consumed = true;
                    }
                    break;

                case PowerUpType.DamageBoost:
                    StatusEffectController secDmg = other.GetComponent<StatusEffectController>();
                    if (secDmg != null)
                    {
                        secDmg.ApplyEffect(StatusEffectType.DamageBoost, _damageBoostDuration);
                        consumed = true;
                    }
                    break;
            }

            if (consumed)
            {
                Collect();
            }
        }

        private void Collect()
        {
            _isAvailable = false;
            if (_collider != null) _collider.enabled = false;
            if (_visualModel != null) _visualModel.SetActive(false);

            if (_pickupSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_pickupSound, 0.8f);
            }

            if (_respawnRoutine != null) StopCoroutine(_respawnRoutine);
            _respawnRoutine = StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(_respawnTime);
            ResetPowerUp();
        }

        public void ResetPowerUp()
        {
            if (_respawnRoutine != null)
            {
                StopCoroutine(_respawnRoutine);
                _respawnRoutine = null;
            }

            _isAvailable = true;
            if (_collider != null) _collider.enabled = true;
            if (_visualModel != null) _visualModel.SetActive(true);
        }

        public void Setup(PowerUpType type, float respawnTime, AudioClip sound)
        {
            _type = type;
            _respawnTime = respawnTime;
            _pickupSound = sound;
        }
    }
}
