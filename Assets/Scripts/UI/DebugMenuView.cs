using UnityEngine.UI;
using Zenject;

public class DebugMenuView : View<DebugMenuView>
{
    public bool m_AlwaysVisible = true;

    public Toggle m_BoosterModeToggle;
    public Toggle m_SkinScreenToggle;

    private IFeatureService m_FeatureService;

    [Inject]
    public void Construct(IFeatureService featureService)
    {
        m_FeatureService = featureService;
    }

    protected override void Awake()
    {
        base.Awake();

        if (m_BoosterModeToggle != null)
            m_BoosterModeToggle.onValueChanged.AddListener(OnBoosterModeChanged);
        if (m_SkinScreenToggle != null)
            m_SkinScreenToggle.onValueChanged.AddListener(OnSkinScreenChanged);

        RefreshToggles();
    }

    public void Open()
    {
        RefreshToggles();
        Transition(true);
    }

    public void OnBackButton()
    {
        Transition(false);
    }

    protected override void OnGamePhaseChanged(GamePhase _GamePhase)
    {
        base.OnGamePhaseChanged(_GamePhase);

        if (!m_AlwaysVisible)
        {
            if (_GamePhase != GamePhase.MAIN_MENU && m_Visible)
                Transition(false);
            return;
        }

        if (_GamePhase == GamePhase.MAIN_MENU)
        {
            RefreshToggles();
            Transition(true);
        }
        else
        {
            Transition(false);
        }
    }

    private void RefreshToggles()
    {
        if (m_BoosterModeToggle != null)
            m_BoosterModeToggle.SetIsOnWithoutNotify(m_FeatureService.IsEnabled(GameFeature.BoosterMode));
        if (m_SkinScreenToggle != null)
            m_SkinScreenToggle.SetIsOnWithoutNotify(m_FeatureService.IsEnabled(GameFeature.SkinScreen));
    }

    private void OnBoosterModeChanged(bool value)
    {
        m_FeatureService.SetEnabled(GameFeature.BoosterMode, value);
        ApplyToMenu();
    }

    private void OnSkinScreenChanged(bool value)
    {
        m_FeatureService.SetEnabled(GameFeature.SkinScreen, value);
        ApplyToMenu();
    }

    private void ApplyToMenu()
    {
        if (MainMenuView.Instance != null)
            MainMenuView.Instance.ApplyFeatureVisibility();
    }
}
