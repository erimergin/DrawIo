using UnityEngine;

public class FeatureService : IFeatureService
{
    private const string c_KeyPrefix = "Feature_";

    public bool IsEnabled(GameFeature feature)
    {
        return PlayerPrefs.GetInt(GetKey(feature), 1) == 1;
    }

    public void SetEnabled(GameFeature feature, bool enabled)
    {
        PlayerPrefs.SetInt(GetKey(feature), enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private static string GetKey(GameFeature feature)
    {
        return c_KeyPrefix + feature.ToString();
    }
}
