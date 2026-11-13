using UnityEngine;

namespace MultiverseArcade.Core
{
    /// <summary>
    /// Placeholder game entry untuk scene game anggota yang belum diimplementasikan
    /// (Game_Jaya, Game_Prima, Game_Brili, Game_Risa).
    /// Mengimplementasikan IArcadeGame agar integrasi global hub tetap utuh.
    /// </summary>
    public class PlaceholderGameEntry : MonoBehaviour, IArcadeGame
    {
        [SerializeField] private string _gameId = "placeholder";
        public string GameId => _gameId;

        public void OnGameEnter()
        {
            Debug.Log($"[PlaceholderGameEntry] Memasuki gameplay: {_gameId}");
        }

        public void OnGamePause() { }
        public void OnGameResume() { }

        public void OnGameRestart()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.RestartCurrentScene();
            }
        }

        public void OnGameExit()
        {
            Debug.Log($"[PlaceholderGameEntry] Meninggalkan gameplay: {_gameId}");
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterArcadeGame(this);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.UnregisterArcadeGame(this);
            }
        }

        public void SetGameId(string id)
        {
            _gameId = id;
        }
    }
}
