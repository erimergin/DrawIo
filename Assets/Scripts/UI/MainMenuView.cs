using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Zenject;

public class MainMenuView : View<MainMenuView>
{
    private const string m_BestScorePrefix = "BEST SCORE ";

    public Text m_BestScoreText;
    public Image m_BestScoreBar;
    public GameObject m_BestScoreObject;
    public InputField m_InputField;
    public List<Image> m_ColoredImages;
    public List<Text> m_ColoredTexts;

    public GameObject m_BrushGroundLight;
    public GameObject m_BrushesPrefab;
    public int m_IdSkin = 0;
    public GameObject m_PointsPerRank;
    public RankingView m_RankingView;

    [Header("Ranks")]
    public string[] m_Ratings;

    [Header("Booster Mode")]
    public GameObject m_BoosterModeButton;
    public Image m_BoosterModeButtonBg;
    public TMP_Text m_BoosterLevelText;

    [Header("Skin Screen")]
    public GameObject m_OldBrushSelect;
    public GameObject m_NewBrushSelect;
    public Image m_NewBrushSelectBg;

    [Header("Slide animation")]
    public float m_SlideDuration = 0.35f;
    public Ease m_SlideEase = Ease.InOutCubic;

    private Vector2 m_SlideHomePos;

    private IStatsService m_StatsService;
    private IFeatureService m_FeatureService;
    private IGameModeService m_GameModeService;

    [Inject]
    public void Construct(IStatsService statsService, IFeatureService featureService, IGameModeService gameModeService)
    {
        m_StatsService = statsService;
        m_FeatureService = featureService;
        m_GameModeService = gameModeService;
    }

    protected override void Awake()
    {
        base.Awake();

        m_IdSkin = m_StatsService.FavoriteSkin;

        ApplyFeatureVisibility();

        if (SlideTarget != null)
            m_SlideHomePos = SlideTarget.anchoredPosition;
    }

    public void ApplyFeatureVisibility()
    {
        bool booster = m_FeatureService.IsEnabled(GameFeature.BoosterMode);
        if (m_BoosterModeButton != null)
            m_BoosterModeButton.SetActive(booster);

        RefreshBoosterLevel();

        bool skinScreen = m_FeatureService.IsEnabled(GameFeature.SkinScreen);
        if (m_OldBrushSelect != null)
            m_OldBrushSelect.SetActive(!skinScreen);
        if (m_NewBrushSelect != null)
            m_NewBrushSelect.SetActive(skinScreen);
    }

    public void OnOpenSkinScreenButton()
    {
        if (m_FeatureService.IsEnabled(GameFeature.SkinScreen) && SkinSelectionView.Instance != null)
            SkinSelectionView.Instance.Open();
    }

    public void OnDebugMenuButton()
    {
        if (DebugMenuView.Instance != null)
            DebugMenuView.Instance.Open();
    }

    private RectTransform SlideTarget => (RectTransform)transform;

    public void SlideOut()
    {
        m_Group.interactable = false;
        m_Group.blocksRaycasts = false;

        RectTransform target = SlideTarget;
        float width = ((RectTransform)target.parent).rect.width;
        target.DOKill();
        target.DOAnchorPosX(m_SlideHomePos.x - width, m_SlideDuration).SetEase(m_SlideEase);
    }

    public void SlideIn()
    {
        RectTransform target = SlideTarget;
        target.DOKill();
        target.DOAnchorPosX(m_SlideHomePos.x, m_SlideDuration).SetEase(m_SlideEase)
            .OnComplete(() =>
            {
                m_Group.interactable = true;
                m_Group.blocksRaycasts = true;
            });
    }

    private void RefreshBoosterLevel()
    {
        if (m_BoosterLevelText == null)
            return;

        bool show = m_FeatureService.IsEnabled(GameFeature.BoosterMode);
        m_BoosterLevelText.gameObject.SetActive(show);

        if (show)
            m_BoosterLevelText.text = string.Format("LVL {0:00}", m_GameModeService.CurrentBoosterLevel + 1);
    }

    public void OnPlayButton()
    {
        if (GameService.currentPhase == GamePhase.MAIN_MENU)
            GameService.ChangePhase(GamePhase.LOADING);
    }

    public void OnBoosterModeButton()
    {
        if (m_FeatureService.IsEnabled(GameFeature.BoosterMode))
            GameService.StartBoosterMode();
    }

    protected override void OnGamePhaseChanged(GamePhase _GamePhase)
    {
        base.OnGamePhaseChanged(_GamePhase);

        switch (_GamePhase)
        {
            case GamePhase.MAIN_MENU:
                m_BrushGroundLight.SetActive(true);
                RefreshBoosterLevel();
                Transition(true);
                break;

            case GamePhase.LOADING:
                m_BrushGroundLight.SetActive(false);

                    m_BrushesPrefab.SetActive(false);

                if (m_Visible)
                    Transition(false);
                break;
        }
    }

    public void SetTitleColor(Color _Color)
    {
        m_BrushesPrefab.SetActive(true);
        int favoriteSkin = Mathf.Min(m_StatsService.FavoriteSkin, GameService.m_Skins.Count - 1);
        m_BrushesPrefab.GetComponent<BrushMainMenu>().Set(GameService.m_Skins[favoriteSkin]);
        string playerName = m_StatsService.GetNickname();

        if (playerName != null)
            m_InputField.text = playerName;

        for (int i = 0; i < m_ColoredImages.Count; ++i)
            m_ColoredImages[i].color = _Color;

        for (int i = 0; i < m_ColoredTexts.Count; i++)
            m_ColoredTexts[i].color = _Color;

        if (m_BoosterModeButtonBg != null && m_FeatureService.IsEnabled(GameFeature.BoosterMode))
            m_BoosterModeButtonBg.color = _Color;

        if (m_NewBrushSelectBg != null && m_FeatureService.IsEnabled(GameFeature.SkinScreen))
            m_NewBrushSelectBg.color = _Color;

        m_RankingView.gameObject.SetActive(true);
        m_RankingView.RefreshNormal();
    }

    public void OnSetPlayerName(string _Name)
    {
        m_StatsService.SetNickname(_Name);
    }

    public string GetRanking(int _Rank)
    {
        return m_Ratings[_Rank];
    }

    public int GetRankingCount()
    {
        return m_Ratings.Length;
    }

    public void LeftButtonBrush()
    {
        ChangeBrush(m_IdSkin - 1);
    }

    public void RightButtonBrush()
    {
        ChangeBrush(m_IdSkin + 1);
    }

    public void ChangeBrush(int _NewBrush)
    {
        _NewBrush = Mathf.Clamp(_NewBrush, 0, GameService.m_Skins.Count);
        m_IdSkin = _NewBrush;
        if (m_IdSkin >= GameService.m_Skins.Count)
            m_IdSkin = 0;
        GameService.m_PlayerSkinID = m_IdSkin;
        int favoriteSkin = Mathf.Min(m_StatsService.FavoriteSkin, GameService.m_Skins.Count - 1);
        m_BrushesPrefab.GetComponent<BrushMainMenu>().Set(GameService.m_Skins[favoriteSkin]);
        m_StatsService.FavoriteSkin = m_IdSkin;
        GameService.SetColor(GameService.ComputeCurrentPlayerColor(true, 0));
    }
}
