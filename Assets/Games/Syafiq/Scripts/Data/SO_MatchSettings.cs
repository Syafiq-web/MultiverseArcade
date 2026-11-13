using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    [CreateAssetMenu(fileName = "SO_MatchSettings_Standard", menuName = "Arena Clash/Match Settings")]
    public class SO_MatchSettings : ScriptableObject
    {
        public int targetEliminations = 10;
        public float matchDuration = 180f;
        public float countdownDuration = 3f;
        public float respawnTime = 4f;
    }
}
