using System;

namespace MultiverseArcade.Core
{
    /// <summary>
    /// Event Bus global untuk komunikasi longgar (decoupled) antar-sistem tanpa referensi langsung.
    /// Sesuai GDD Bagian 2.4.
    /// </summary>
    public static class EventBus
    {
        public static event Action<string> OnGameStarted;
        public static event Action OnGamePaused;
        public static event Action OnGameResumed;
        public static event Action<string, int> OnGameEnded;
        public static event Action<string> OnSceneLoadRequested;
        public static event Action OnSettingsChanged;
        public static event Action OnSaveLoaded;

        public static void RaiseGameStarted(string gameId) => OnGameStarted?.Invoke(gameId);
        public static void RaiseGamePaused() => OnGamePaused?.Invoke();
        public static void RaiseGameResumed() => OnGameResumed?.Invoke();
        public static void RaiseGameEnded(string gameId, int score) => OnGameEnded?.Invoke(gameId, score);
        public static void RaiseSceneLoadRequested(string sceneName) => OnSceneLoadRequested?.Invoke(sceneName);
        public static void RaiseSettingsChanged() => OnSettingsChanged?.Invoke();
        public static void RaiseSaveLoaded() => OnSaveLoaded?.Invoke();
    }
}
