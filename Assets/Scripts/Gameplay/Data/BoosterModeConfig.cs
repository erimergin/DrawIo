using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BoosterLevelSetup
{
    public string m_Name = "Level";
    public List<PowerUpData> m_PowerUps = new List<PowerUpData>();
    public float m_MinSpawnRate = 1.0f;
    public float m_MaxSpawnRate = 2.5f;
}

[CreateAssetMenu(fileName = "BoosterModeConfig", menuName = "Config/BoosterModeConfig")]
public class BoosterModeConfig : ScriptableObject
{
    [SerializeField] private List<BoosterLevelSetup> m_Levels = new List<BoosterLevelSetup>();

    public int DesignedLevelCount => m_Levels.Count;

    public BoosterLevelSetup GetSetup(int levelIndex)
    {
        if (m_Levels == null || m_Levels.Count == 0)
            return null;

        int wrapped = ((levelIndex % m_Levels.Count) + m_Levels.Count) % m_Levels.Count;
        return m_Levels[wrapped];
    }
}
