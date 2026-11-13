using System;
using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    [Serializable]
    public struct DamageInfo
    {
        public int amount;
        public int attackerTeamId;
        public GameObject attacker;
        public Vector3 hitPoint;
        public Vector3 hitNormal;
        public bool isCritical;

        public DamageInfo(int amount, int attackerTeamId, GameObject attacker, Vector3 hitPoint, Vector3 hitNormal = default, bool isCritical = false)
        {
            this.amount = amount;
            this.attackerTeamId = attackerTeamId;
            this.attacker = attacker;
            this.hitPoint = hitPoint;
            this.hitNormal = hitNormal;
            this.isCritical = isCritical;
        }
    }
}
