public enum GameFeature
{
    BoosterMode,
    SkinScreen
}

public interface IFeatureService
{
    bool IsEnabled(GameFeature feature);
    void SetEnabled(GameFeature feature, bool enabled);
}
