using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameModeService : IGameModeService
{
    private const string c_BoosterLevelSave = "BoosterLevel";

    private BoosterModeConfig m_Config;

    public GameMode CurrentMode { get; private set; } = GameMode.Classic;

    public int CurrentBoosterLevel
    {
        get { return PlayerPrefs.GetInt(c_BoosterLevelSave, 0); }
        private set { PlayerPrefs.SetInt(c_BoosterLevelSave, value); PlayerPrefs.Save(); }
    }

    [Inject]
    public void Construct(BoosterModeConfig config)
    {
        m_Config = config;
    }

    public void SetMode(GameMode mode)
    {
        CurrentMode = mode;
    }

    public BoosterLevelSetup GetCurrentBoosterSetup()
    {
        if (CurrentMode != GameMode.Booster || m_Config == null)
            return null;

        return m_Config.GetSetup(CurrentBoosterLevel);
    }

    public List<PowerUpData> GetActivePowerUps()
    {
        BoosterLevelSetup setup = GetCurrentBoosterSetup();
        if (setup == null || setup.m_PowerUps == null || setup.m_PowerUps.Count == 0)
            return null;

        return setup.m_PowerUps;
    }

    public void AdvanceBoosterLevel()
    {
        CurrentBoosterLevel = CurrentBoosterLevel + 1;
    }
}
