using System;
using System.Collections.Generic;
using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    public enum StatusEffectType
    {
        Slow,       // -30% speed (2s)
        Stun,       // Tidak bisa gerak / tembak / skill (1s)
        Shield,     // -60% damage received (2s)
        Haste,      // +25% speed (4s)
        DamageBoost // +30% damage dealt (5s)
    }

    [Serializable]
    public class ActiveStatusEffect
    {
        public StatusEffectType type;
        public float durationRemaining;
        public float totalDuration;

        public ActiveStatusEffect(StatusEffectType type, float duration)
        {
            this.type = type;
            this.durationRemaining = duration;
            this.totalDuration = duration;
        }
    }

    /// <summary>
    /// Mengelola status effect aktif per karakter (Slow, Stun, Shield, Haste, Damage Boost).
    /// Efek dihitung secara multiplikatif, timer menggunakan scaled time (berhenti saat game pause),
    /// dan di-reset total saat karakter mati, respawn, atau rematch.
    /// Sesuai GDD Bagian 5.6.
    /// </summary>
    [DisallowMultipleComponent]
    public class StatusEffectController : MonoBehaviour
    {
        public event Action<StatusEffectType, bool> OnStatusEffectToggled;

        private readonly List<ActiveStatusEffect> _activeEffects = new List<ActiveStatusEffect>();

        private float _moveSpeedMult = 1.0f;
        private float _damageTakenMult = 1.0f;
        private float _damageDealtMult = 1.0f;
        private bool _isStunned = false;
        private bool _hasShield = false;

        public float MoveSpeedMultiplier => _isStunned ? 0f : _moveSpeedMult;
        public float DamageTakenMultiplier => _damageTakenMult;
        public float DamageDealtMultiplier => _damageDealtMult;
        public bool IsStunned => _isStunned;
        public bool HasShield => _hasShield;

        private void Update()
        {
            if (_activeEffects.Count == 0) return;

            float dt = Time.deltaTime;
            bool changed = false;

            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                _activeEffects[i].durationRemaining -= dt;
                if (_activeEffects[i].durationRemaining <= 0f)
                {
                    StatusEffectType expiredType = _activeEffects[i].type;
                    _activeEffects.RemoveAt(i);
                    OnStatusEffectToggled?.Invoke(expiredType, false);
                    changed = true;
                }
            }

            if (changed)
            {
                RecalculateMultipliers();
            }
        }

        public void ApplyEffect(StatusEffectType type, float duration)
        {
            // Jika efek serupa sudah ada, perbarui durasi ke yang lebih panjang
            ActiveStatusEffect existing = _activeEffects.Find(e => e.type == type);
            if (existing != null)
            {
                existing.durationRemaining = Mathf.Max(existing.durationRemaining, duration);
                existing.totalDuration = Mathf.Max(existing.totalDuration, duration);
            }
            else
            {
                _activeEffects.Add(new ActiveStatusEffect(type, duration));
                OnStatusEffectToggled?.Invoke(type, true);
            }

            RecalculateMultipliers();
        }

        public bool HasEffect(StatusEffectType type)
        {
            return _activeEffects.Exists(e => e.type == type);
        }

        public void ClearAllEffects()
        {
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                OnStatusEffectToggled?.Invoke(_activeEffects[i].type, false);
            }
            _activeEffects.Clear();
            RecalculateMultipliers();
        }

        private void RecalculateMultipliers()
        {
            _moveSpeedMult = 1.0f;
            _damageTakenMult = 1.0f;
            _damageDealtMult = 1.0f;
            _isStunned = false;
            _hasShield = false;

            foreach (var effect in _activeEffects)
            {
                switch (effect.type)
                {
                    case StatusEffectType.Slow:
                        _moveSpeedMult *= 0.70f; // -30% speed
                        break;
                    case StatusEffectType.Haste:
                        _moveSpeedMult *= 1.25f; // +25% speed
                        break;
                    case StatusEffectType.Stun:
                        _isStunned = true;
                        break;
                    case StatusEffectType.Shield:
                        _damageTakenMult *= 0.40f; // -60% damage received
                        _hasShield = true;
                        break;
                    case StatusEffectType.DamageBoost:
                        _damageDealtMult *= 1.30f; // +30% damage dealt
                        break;
                }
            }
        }
    }
}
