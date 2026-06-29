using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using Zenject;

public class SkinSelectionView : View<SkinSelectionView>
{
    [Header("3D rig")]
    public GameObject m_RigRoot;
    public Camera m_SkinCamera;
    public Transform m_SlideRoot3D;
    public Transform m_GridRoot;
    public Transform m_PreviewAnchor;

    [Header("Slide animation")]
    public RectTransform m_OverlayRoot;

    [Header("Colour")]
    public Image m_BackButtonBg;
    public float m_SlideDuration = 0.35f;
    public Ease m_SlideEase = Ease.InOutCubic;
    public float m_World3DSlideOffset = 30f;

    [Header("Grid layout")]
    public int m_Columns = 3;
    public float m_SpacingX = 3.0f;
    public float m_SpacingY = 3.5f;
    public float m_ItemScale = 1.0f;
    public int m_VisibleRows = 3;

    [Header("Feel")]
    public float m_RotationSpeed = 45f;
    public float m_PreviewScale = 2.0f;
    public float m_DragSensitivity = 0.02f;
    public float m_TapThreshold = 12f;

    private IStatsService m_StatsService;
    private IFeatureService m_FeatureService;

    private bool m_Built;
    private readonly List<GameObject> m_Items = new List<GameObject>();
    private GameObject m_PreviewInstance;
    private int m_SelectedIndex;
    private float m_GridBaseY;

    private float m_ScrollY;
    private float m_MaxScroll;

    private float m_Slide3DHomeX;
    private Vector2 m_OverlayHomePos;

    private bool m_PointerDown;
    private Vector3 m_LastPointerPos;
    private float m_DragDistance;

    [Inject]
    public void Construct(IStatsService statsService, IFeatureService featureService)
    {
        m_StatsService = statsService;
        m_FeatureService = featureService;
    }

    protected override void Awake()
    {
        base.Awake();

        if (m_GridRoot != null)
            m_GridBaseY = m_GridRoot.localPosition.y;

        if (m_SlideRoot3D != null)
            m_Slide3DHomeX = m_SlideRoot3D.localPosition.x;

        if (m_OverlayRoot != null)
            m_OverlayHomePos = m_OverlayRoot.anchoredPosition;

        HideRig();
    }

    public void Open()
    {
        if (!m_FeatureService.IsEnabled(GameFeature.SkinScreen))
            return;

        if (!m_Built)
            Build();

        if (m_RigRoot != null) m_RigRoot.SetActive(true);
        if (m_SkinCamera != null) m_SkinCamera.enabled = true;

        m_Group.alpha = 1f;
        m_Group.interactable = true;
        m_Group.blocksRaycasts = true;
        m_Visible = true;

        SelectSkin(Mathf.Clamp(m_StatsService.FavoriteSkin, 0, Mathf.Max(0, m_Items.Count - 1)), false);

        if (MainMenuView.Instance != null)
            MainMenuView.Instance.SlideOut();

        SlideContent(true);
    }

    public void OnBackButton()
    {
        Close();
    }

    public void Close()
    {
        m_Group.interactable = false;
        m_Group.blocksRaycasts = false;
        m_Visible = false;

        if (MainMenuView.Instance != null)
            MainMenuView.Instance.SlideIn();

        SlideContent(false);
    }

    private void SlideContent(bool inward)
    {
        if (m_SlideRoot3D != null)
        {
            m_SlideRoot3D.DOKill();
            Vector3 p = m_SlideRoot3D.localPosition;

            if (inward)
            {
                p.x = m_Slide3DHomeX + m_World3DSlideOffset;
                m_SlideRoot3D.localPosition = p;
                m_SlideRoot3D.DOLocalMoveX(m_Slide3DHomeX, m_SlideDuration).SetEase(m_SlideEase);
            }
            else
            {
                m_SlideRoot3D.DOLocalMoveX(m_Slide3DHomeX + m_World3DSlideOffset, m_SlideDuration)
                    .SetEase(m_SlideEase).OnComplete(HideRig);
            }
        }
        else if (!inward)
        {
            HideRig();
        }

        if (m_OverlayRoot != null)
        {
            float width = ((RectTransform)m_OverlayRoot.parent).rect.width;
            m_OverlayRoot.DOKill();

            if (inward)
            {
                m_OverlayRoot.anchoredPosition = new Vector2(m_OverlayHomePos.x + width, m_OverlayHomePos.y);
                m_OverlayRoot.DOAnchorPosX(m_OverlayHomePos.x, m_SlideDuration).SetEase(m_SlideEase);
            }
            else
            {
                m_OverlayRoot.DOAnchorPosX(m_OverlayHomePos.x + width, m_SlideDuration).SetEase(m_SlideEase);
            }
        }
    }

    private void HideRig()
    {
        if (m_RigRoot != null) m_RigRoot.SetActive(false);
        if (m_SkinCamera != null) m_SkinCamera.enabled = false;
    }

    protected override void OnGamePhaseChanged(GamePhase _GamePhase)
    {
        base.OnGamePhaseChanged(_GamePhase);

        if (_GamePhase != GamePhase.MAIN_MENU && m_Visible)
        {
            m_Group.alpha = 0f;
            m_Group.interactable = false;
            m_Group.blocksRaycasts = false;
            m_Visible = false;
            HideRig();
        }
    }

    private void Build()
    {
        List<SkinData> skins = GameService.m_Skins;
        if (skins == null || m_GridRoot == null)
            return;

        for (int i = 0; i < skins.Count; ++i)
            m_Items.Add(CreateSkinItem(skins[i], i, m_GridRoot, m_ItemScale, true));

        int rows = Mathf.CeilToInt(skins.Count / (float)m_Columns);
        int hiddenRows = Mathf.Max(0, rows - m_VisibleRows);
        m_MaxScroll = hiddenRows * m_SpacingY;

        m_Built = true;
    }

    private GameObject CreateSkinItem(SkinData skin, int index, Transform parent, float scale, bool inGrid)
    {
        GameObject go = Instantiate(skin.Brush.m_Prefab, parent);
        go.transform.localScale = Vector3.one * scale;
        go.transform.localRotation = Quaternion.identity;

        if (inGrid)
        {
            int row = index / m_Columns;
            int col = index % m_Columns;
            float x = (col - (m_Columns - 1) * 0.5f) * m_SpacingX;
            float y = -row * m_SpacingY;
            go.transform.localPosition = new Vector3(x, y, 0f);
        }
        else
        {
            go.transform.localPosition = Vector3.zero;
        }

        TintBrush(go, skin);

        RotatingObject rot = go.AddComponent<RotatingObject>();
        rot.m_DegreesPerSecond = m_RotationSpeed;

        int layer = inGrid
            ? (m_RigRoot != null ? m_RigRoot.layer : go.layer)
            : (m_PreviewAnchor != null ? m_PreviewAnchor.gameObject.layer : go.layer);
        SetLayerRecursive(go.transform, layer);

        return go;
    }

    private static void TintBrush(GameObject go, SkinData skin)
    {
        Brush brush = go.GetComponent<Brush>();
        Color c = skin.Color.m_Colors[0];
        if (brush != null && brush.m_Renderers != null)
        {
            for (int r = 0; r < brush.m_Renderers.Count; ++r)
                brush.m_Renderers[r].material.color = c;
        }
        else
        {
            foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
                r.material.color = c;
        }
    }

    private static void SetLayerRecursive(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        for (int i = 0; i < t.childCount; ++i)
            SetLayerRecursive(t.GetChild(i), layer);
    }

    private void SelectSkin(int index, bool save)
    {
        if (m_Items.Count == 0)
            return;

        m_SelectedIndex = Mathf.Clamp(index, 0, m_Items.Count - 1);

        SkinData selected = GameService.m_Skins[m_SelectedIndex];
        Color selectedColor = selected.Color.m_Colors[0];

        if (m_BackButtonBg != null)
            m_BackButtonBg.color = selectedColor;

        if (m_PreviewInstance != null)
            Destroy(m_PreviewInstance);

        if (m_PreviewAnchor != null)
            m_PreviewInstance = CreateSkinItem(selected, m_SelectedIndex, m_PreviewAnchor, m_PreviewScale, false);

        if (save)
        {
            m_StatsService.FavoriteSkin = m_SelectedIndex;
            GameService.m_PlayerSkinID = m_SelectedIndex;
            GameService.SetColor(GameService.ComputeCurrentPlayerColor(true, 0));
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!m_Visible)
            return;

        HandleInput();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_PointerDown = true;
            m_LastPointerPos = Input.mousePosition;
            m_DragDistance = 0f;
        }

        if (m_PointerDown && Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - m_LastPointerPos;
            m_LastPointerPos = Input.mousePosition;
            m_DragDistance += delta.magnitude;

            m_ScrollY = Mathf.Clamp(m_ScrollY + delta.y * m_DragSensitivity, 0f, m_MaxScroll);
            ApplyScroll();
        }

        if (Input.GetMouseButtonUp(0))
        {
            bool wasTap = m_DragDistance < m_TapThreshold;
            m_PointerDown = false;

            if (wasTap && !IsPointerOverUI())
                TrySelectAtPointer();
        }
    }

    private void ApplyScroll()
    {
        if (m_GridRoot == null)
            return;

        Vector3 p = m_GridRoot.localPosition;
        p.y = m_GridBaseY + m_ScrollY;
        m_GridRoot.localPosition = p;
    }

    private void TrySelectAtPointer()
    {
        if (m_SkinCamera == null || m_GridRoot == null)
            return;

        if (!m_SkinCamera.pixelRect.Contains(Input.mousePosition))
            return;

        Ray ray = m_SkinCamera.ScreenPointToRay(Input.mousePosition);
        Plane gridPlane = new Plane(m_GridRoot.forward, m_GridRoot.position);
        if (!gridPlane.Raycast(ray, out float enter))
            return;

        Vector3 local = m_GridRoot.InverseTransformPoint(ray.GetPoint(enter));

        int col = Mathf.RoundToInt(local.x / m_SpacingX + (m_Columns - 1) * 0.5f);
        int row = Mathf.RoundToInt(-local.y / m_SpacingY);
        if (col < 0 || col >= m_Columns || row < 0)
            return;

        int index = row * m_Columns + col;
        if (index < 0 || index >= m_Items.Count)
            return;

        float cellX = (col - (m_Columns - 1) * 0.5f) * m_SpacingX;
        float cellY = -row * m_SpacingY;
        if (Mathf.Abs(local.x - cellX) > m_SpacingX * 0.5f || Mathf.Abs(local.y - cellY) > m_SpacingY * 0.5f)
            return;

        SelectSkin(index, true);
    }

    private static bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
