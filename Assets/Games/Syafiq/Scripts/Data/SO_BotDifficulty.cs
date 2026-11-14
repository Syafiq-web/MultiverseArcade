using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Konfigurasi parameter tingkat kesulitan bot AI secara data-driven.
    /// Sesuai GDD Bagian 5.9.
    /// </summary>
    [CreateAssetMenu(fileName = "SO_BotDifficulty_Normal", menuName = "Arena Clash/Bot Difficulty")]
    public class SO_BotDifficulty : ScriptableObject
    {
        public string difficultyName = "Normal";
        public float reactionTime = 0.5f;
        public float aimErrorDegrees = 8f;
        public float fireInterval = 1.0f;
        public float moveSpeed = 4.2f;
        public float retreatHealthRatio = 0.25f;
        public float targetEvaluationInterval = 0.4f;
        public bool allowStrafe = true;
        public bool allowRetreat = true;
    }
}
