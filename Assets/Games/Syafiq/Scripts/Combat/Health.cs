using System;
using System.Collections;
using UnityEngine;
using MultiverseArcade.Core;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Komponen pengelola HP, damage reception, hit reaction, dan kematian karakter.
    /// Sesuai GDD Bagian 5.6.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Header("Team & Identity")]
        [SerializeField] private int _teamId = 0; // 0 = Blue, 1 = Red
        [SerializeField] private string _characterName = "Character";
        [SerializeField] private bool _isPlayer = false;

        [Header("Health Values")]
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private int _currentHealth = 100;

        [Header("Hit Flash Visuals")]
        [SerializeField] private Renderer _modelRenderer;
        [SerializeField] private Color _flashColor = Color.white;

        [Header("Audio")]
        [SerializeField] private AudioClip _hurtSound;
        [SerializeField] private AudioClip _deathSound;

        private bool _isDead = false;
        private Color _originalColor;
        private Material _instancedMat;
        private Coroutine _flashRoutine;

        public int TeamId => _teamId;
        public string CharacterName => _characterName;
        public bool IsPlayer => _isPlayer;
        public int MaxHealth => _maxHealth;
        public int CurrentHealth => _currentHealth;
        public bool IsDead => _isDead;

        private void Awake()
        {
            _currentHealth = _maxHealth;
            if (_modelRenderer != null)
            {
                _instancedMat = _modelRenderer.material;
                _originalColor = _instancedMat.color;
            }
        }

        public void Setup(int teamId, string charName, int maxHp, bool isPlayer = false)
        {
            _teamId = teamId;
            _characterName = charName;
            _maxHealth = maxHp;
            _currentHealth = maxHp;
            _isPlayer = isPlayer;
            _isDead = false;

            if (_modelRenderer != null)
            {
                _instancedMat = _modelRenderer.material;
                _originalColor = _instancedMat.color;
            }
        }

        public bool TakeDamage(DamageInfo info)
        {
            // Tolak jika sudah mati
            if (_isDead) return false;

            // Tolak jika satu tim (Friendly Fire OFF - GDD 5.6)
            if (info.attackerTeamId == _teamId) return false;

            // Integrasi StatusEffectController (Shield -60% damage)
            StatusEffectController sec = GetComponent<StatusEffectController>();
            float dmgMult = sec != null ? sec.DamageTakenMultiplier : 1.0f;
            int actualDmg = Mathf.Max(1, Mathf.RoundToInt(info.amount * dmgMult));

            _currentHealth = Mathf.Max(0, _currentHealth - actualDmg);

            // Audio & Visual Hit Reaction
            PlayHurtAudio();
            TriggerHitFlash();

            // Beri tahu attacker untuk mengisi super meter
            if (info.attacker != null)
            {
                ArenaEvents.RaiseDamageCaused(info.attacker, actualDmg);
            }

            // Trigger floating damage number
            ArenaEvents.RaiseDamageDealt(info.hitPoint != Vector3.zero ? info.hitPoint : transform.position + Vector3.up, actualDmg, info.isCritical);
            ArenaEvents.RaiseHealthChanged(this, _currentHealth, _maxHealth);

            if (_currentHealth <= 0)
            {
                Die(info.attackerTeamId, info.attacker != null ? info.attacker.name : "Opponent", info.attacker);
            }

            return true;
        }

        public void Heal(int amount)
        {
            if (_isDead) return;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            ArenaEvents.RaiseHealthChanged(this, _currentHealth, _maxHealth);
        }

        public void ResetHealth()
        {
            _isDead = false;
            _currentHealth = _maxHealth;
            gameObject.SetActive(true);

            // Reset status effects saat respawn / rematch
            StatusEffectController sec = GetComponent<StatusEffectController>();
            if (sec != null)
            {
                sec.ClearAllEffects();
            }

            if (_instancedMat != null)
            {
                _instancedMat.color = _originalColor;
            }

            ArenaEvents.RaiseHealthChanged(this, _currentHealth, _maxHealth);
        }

        private void Die(int killerTeamId, string killerName, GameObject killerObj = null)
        {
            if (_isDead) return;
            _isDead = true;

            PlayDeathAudio();
            Debug.Log($"[Health] {_characterName} tereliminasi oleh {killerName} (Tim {killerTeamId})");

            // Reset status effects saat mati
            StatusEffectController sec = GetComponent<StatusEffectController>();
            if (sec != null)
            {
                sec.ClearAllEffects();
            }

            if (killerObj != null)
            {
                ArenaEvents.RaiseKillScored(killerObj);
            }

            ArenaEvents.RaiseCharacterDied(this, killerTeamId, killerName);
        }

        private void TriggerHitFlash()
        {
            if (_instancedMat == null) return;
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(HitFlashRoutine());
        }

        private IEnumerator HitFlashRoutine()
        {
            _instancedMat.color = _flashColor;
            yield return new WaitForSeconds(0.08f);
            _instancedMat.color = _originalColor;
            _flashRoutine = null;
        }

        private void PlayHurtAudio()
        {
            if (_hurtSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_hurtSound, 0.7f);
            }
        }

        private void PlayDeathAudio()
        {
            if (_deathSound != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(_deathSound, 1f);
            }
        }

        public void SetAudioClips(AudioClip hurt, AudioClip death)
        {
            _hurtSound = hurt;
            _deathSound = death;
        }
    }
}
