namespace MultiverseArcade.Core
{
    /// <summary>
    /// State global aplikasi Multiverse Arcade sesuai GDD Bagian 2.3.
    /// Boot -> MainMenu -> GameSelection -> Loading -> InGame <-> Paused -> Quit.
    /// </summary>
    public enum AppState
    {
        Boot,
        MainMenu,
        GameSelection,
        Loading,
        InGame,
        Paused,
        Quit
    }
}
