using System.Collections.Generic;

public enum GameMode
{
    Classic,
    Booster
}

public interface IGameModeService
{
    GameMode CurrentMode { get; }
    int CurrentBoosterLevel { get; }
    void SetMode(GameMode mode);
    BoosterLevelSetup GetCurrentBoosterSetup();
    List<PowerUpData> GetActivePowerUps();
    void AdvanceBoosterLevel();
}
