using System;
using System.Collections.Generic;
using UnityEngine;

namespace MultiverseArcade.Syafiq.Brawl
{
    // Aturan Gem Grab: tim yang memegang >= 10 gem selama 15 detik menang.
    // Jika gem turun di bawah 10 (carrier mati / tertembak), hitungan mundur batal.
    public class GemGrabManager : MonoBehaviour
    {
        [SerializeField] int gemsToWin = 10;
        [SerializeField] float countdown = 15f;

        public int WinningTeam { get; private set; } = -1;

        public event Action<int, float> OnCountdown;   // (teamId, sisaDetik); teamId -1 = tidak ada hitungan
        public event Action<int, int> OnGemsChanged;   // (blueGems, redGems)
        public event Action<int> OnMatchEnd;

        int leadingTeam = -1;
        float timer;

        void Update()
        {
            if (WinningTeam >= 0) return;

            // Total gem per tim
            int blueTotal = 0;
            int redTotal = 0;
            foreach (var c in GemCarrier.All)
            {
                if (c == null) continue;
                if (c.teamId == 0) blueTotal += c.Gems;
                else if (c.teamId == 1) redTotal += c.Gems;
            }

            OnGemsChanged?.Invoke(blueTotal, redTotal);

            int team = -1;
            if (blueTotal >= gemsToWin && blueTotal > redTotal) team = 0;
            else if (redTotal >= gemsToWin && redTotal > blueTotal) team = 1;

            // Tim pemegang berubah -> reset timer
            if (team != leadingTeam)
            {
                leadingTeam = team;
                timer = countdown;
            }

            if (leadingTeam < 0)
            {
                OnCountdown?.Invoke(-1, 0);
                return;
            }

            timer -= Time.deltaTime;
            OnCountdown?.Invoke(leadingTeam, Mathf.Max(0, timer));

            if (timer <= 0)
            {
                WinningTeam = leadingTeam;
                OnMatchEnd?.Invoke(WinningTeam);
            }
        }
    }
}
