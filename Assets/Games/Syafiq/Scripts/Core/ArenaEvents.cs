using System;
using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    public enum MatchState
    {
        Countdown,
        Playing,
        Paused,
        Finished
    }

    public enum MatchResult
    {
        BlueVictory,
        RedVictory,
        Draw
    }

    /// <summary>
    /// Event bus lokal terpisah untuk sistem gameplay Arena Clash 3D.
    /// Sesuai GDD Bagian 5.13.
    /// </summary>
    public static class ArenaEvents
    {
        public static event Action<Health, int, int> OnHealthChanged;
        public static event Action<Health, int, string> OnCharacterDied;
        public static event Action<int, int> OnScoreChanged;
        public static event Action<float> OnMatchTimerUpdated;
        public static event Action<MatchState> OnMatchStateChanged;
        public static event Action<MatchResult, int, int> OnMatchFinished;
        public static event Action<Vector3, int, bool> OnDamageDealt;
        public static event Action<GameObject, int> OnDamageCaused;
        public static event Action<GameObject> OnKillScored;

        public static void RaiseHealthChanged(Health h, int cur, int max) => OnHealthChanged?.Invoke(h, cur, max);
        public static void RaiseCharacterDied(Health v, int killerTeam, string killerName) => OnCharacterDied?.Invoke(v, killerTeam, killerName);
        public static void RaiseScoreChanged(int blue, int red) => OnScoreChanged?.Invoke(blue, red);
        public static void RaiseMatchTimerUpdated(float time) => OnMatchTimerUpdated?.Invoke(time);
        public static void RaiseMatchStateChanged(MatchState state) => OnMatchStateChanged?.Invoke(state);
        public static void RaiseMatchFinished(MatchResult res, int blue, int red) => OnMatchFinished?.Invoke(res, blue, red);
        public static void RaiseDamageDealt(Vector3 pos, int dmg, bool isCrit) => OnDamageDealt?.Invoke(pos, dmg, isCrit);
        public static void RaiseDamageCaused(GameObject attacker, int dmg) => OnDamageCaused?.Invoke(attacker, dmg);
        public static void RaiseKillScored(GameObject killer) => OnKillScored?.Invoke(killer);

        public static void ClearAll()
        {
            OnHealthChanged = null;
            OnCharacterDied = null;
            OnScoreChanged = null;
            OnMatchTimerUpdated = null;
            OnMatchStateChanged = null;
            OnMatchFinished = null;
            OnDamageDealt = null;
            OnDamageCaused = null;
            OnKillScored = null;
        }
    }
}
