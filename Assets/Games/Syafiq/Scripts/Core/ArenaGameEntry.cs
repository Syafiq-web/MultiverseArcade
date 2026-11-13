using UnityEngine;
using MultiverseArcade.Core;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Titik masuk integrasi Arena Clash 3D dengan GameManager global.
    /// Mengimplementasikan kontrak IArcadeGame.
    /// Sesuai GDD Bagian 2.5.1 & 5.13.
    /// </summary>
    public class ArenaGameEntry : MonoBehaviour, IArcadeGame
    {
        public static ArenaGameEntry Instance { get; private set; }

        public string GameId => "syafiq";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterArcadeGame(this);
            }
            else
            {
                OnGameEnter();
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnregisterArcadeGame(this);
            }
            OnGameExit();
        }

        public void OnGameEnter()
        {
            Debug.Log("[ArenaGameEntry] Memulai sesi gameplay Arena Clash 3D...");
            Time.timeScale = 1.0f;
            if (ArenaMatchManager.Instance != null)
            {
                ArenaMatchManager.Instance.StartMatchCountdown();
            }
        }

        public void OnGamePause()
        {
            Debug.Log("[ArenaGameEntry] Arena Clash dipause.");
            Time.timeScale = 0.0f;
            if (ArenaMatchManager.Instance != null)
            {
                ArenaMatchManager.Instance.PauseMatch();
            }
        }

        public void OnGameResume()
        {
            Debug.Log("[ArenaGameEntry] Arena Clash di-resume.");
            Time.timeScale = 1.0f;
            if (ArenaMatchManager.Instance != null)
            {
                ArenaMatchManager.Instance.ResumeMatch();
            }
        }

        public void OnGameRestart()
        {
            Debug.Log("[ArenaGameEntry] Me-restart match Arena Clash...");
            Time.timeScale = 1.0f;
            if (ArenaMatchManager.Instance != null)
            {
                ArenaMatchManager.Instance.RestartMatch();
            }
            else if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.RestartCurrentScene();
            }
        }

        public void OnGameExit()
        {
            Debug.Log("[ArenaGameEntry] Membersihkan state Arena Clash sebelum keluar...");
            Time.timeScale = 1.0f;
            StopAllCoroutines();
            ArenaEvents.ClearAll();

            if (ProjectilePool.Instance != null)
            {
                ProjectilePool.Instance.ClearPool();
            }
        }
    }
}
