using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public sealed class PowerUp_PaintBombRandom : PowerUp
{
    public float m_Radius = 6.0f;
    public float m_FillDuration = 0.3f;

    private ITerrainService m_TerrainService;

    [Inject]
    public void ChildConstruct(ITerrainService terrainService)
    {
        m_TerrainService = terrainService;
    }

    public override void OnPlayerTouched(Player _Player)
    {
        base.OnPlayerTouched(_Player);

        Vector3 target = new Vector3(
            Random.Range(-m_TerrainService.WorldHalfWidth, m_TerrainService.WorldHalfWidth),
            0.0f,
            Random.Range(-m_TerrainService.WorldHalfHeight, m_TerrainService.WorldHalfHeight));

        m_TerrainService.ClampPosition(ref target, Constants.c_SpawnBorderOffset);
        m_TerrainService.FillCircle(_Player, target, m_Radius, m_FillDuration);
    }
}
