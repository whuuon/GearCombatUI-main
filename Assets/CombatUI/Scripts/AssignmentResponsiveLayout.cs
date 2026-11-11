using UnityEngine;
using UnityEngine.UI;

/// <summary>Positions the assignment UI within a centered 740-unit design area.</summary>
[ExecuteAlways]
public class AssignmentResponsiveLayout : MonoBehaviour
{
    public RectTransform sky;
    public RectTransform sea;
    public RectTransform field;
    public RectTransform board;
    public RectTransform shop;
    public RectTransform banner;
    public RectTransform header;
    public RectTransform boosters;
    public RectTransform reward;
    public bool battle;

    const float DesignWidth = 740f;
    static readonly Vector2 TopLeft = new Vector2(0f, 1f);
    Vector2 lastCanvasSize = new Vector2(-1f, -1f);
    int lastConfiguration;

    void OnEnable()
    {
        Invalidate();
        Refresh();
    }

    void OnValidate()
    {
        Invalidate();
    }

    void Update() => Refresh();

    void OnRectTransformDimensionsChange() => Refresh();

    void Invalidate() => lastCanvasSize = new Vector2(-1f, -1f);

    public void Refresh()
    {
        var canvas = GetComponentInParent<Canvas>();
        var canvasRect = canvas ? canvas.transform as RectTransform : null;
        if (!canvasRect) return;

        Vector2 size = canvasRect.rect.size;
        if (size.x <= 0f || size.y <= 0f) return;
        int configuration = ConfigurationHash();
        if (size == lastCanvasSize && configuration == lastConfiguration) return;

        // Cache before applying changes, because layout can trigger dimension callbacks.
        lastCanvasSize = size;
        lastConfiguration = configuration;

        float centerLeft = (size.x - DesignWidth) * 0.5f;
        float height = size.y;
        // The four gear rows end at y=671 in the 698-unit board design.
        // Keep only 16 units of bottom padding, independently of screen height.
        float boardWidth = battle ? 570f : 698f;
        float boardHeight = 687f * boardWidth / 698f;
        float boardBottom = height * (battle ? 0.871f : 0.663f);
        float boardTop = boardBottom - boardHeight;
        Place(sky, (size.x-height)*.5f, 0f, height, height);
        Place(field, centerLeft, 0f, DesignWidth, 1616f);
        Place(header, centerLeft, 0f, DesignWidth, height * 0.17f);

        Place(board, centerLeft + (battle ? 19f : 21f), boardTop, boardWidth, boardHeight);

        SetVisible(shop, !battle);
        Place(shop, centerLeft, height * 0.663f, DesignWidth, height * (0.89f - 0.663f));

        Place(banner, 0f, height * 0.89f, size.x, height * 0.11f);
        if (banner && banner.childCount > 0)
        {
            float adHeight=height*.11f;
            float adWidth=Mathf.Min(size.x,adHeight*402f/96f);
            Place(banner.GetChild(0) as RectTransform,(size.x-adWidth)*.5f,0f,adWidth,adHeight);
        }

        SetVisible(boosters, battle);
        Place(boosters, centerLeft + 614f, height * 0.56f, 114f, height * (0.872f - 0.56f));

        SetVisible(reward, !battle);
        Place(reward, centerLeft + 506f, boardTop - 91f, 217f, 100f);
    }

    static void Place(RectTransform target, float left, float top, float width, float height)
    {
        if (!target) return;
        if (target.anchorMin != TopLeft) target.anchorMin = TopLeft;
        if (target.anchorMax != TopLeft) target.anchorMax = TopLeft;
        if (target.pivot != TopLeft) target.pivot = TopLeft;
        var position = new Vector2(left, -top);
        var size = new Vector2(width, height);
        if (target.anchoredPosition != position) target.anchoredPosition = position;
        if (target.sizeDelta != size) target.sizeDelta = size;
    }

    static void SetVisible(RectTransform target, bool visible)
    {
        if (target && target.gameObject.activeSelf != visible) target.gameObject.SetActive(visible);
    }

    int ConfigurationHash()
    {
        unchecked
        {
            int hash = battle ? 1 : 0;
            hash = hash * 31 + Id(sky);
            hash = hash * 31 + Id(sea);
            hash = hash * 31 + Id(field);
            hash = hash * 31 + Id(board);
            hash = hash * 31 + Id(shop);
            hash = hash * 31 + Id(banner);
            hash = hash * 31 + Id(header);
            hash = hash * 31 + Id(boosters);
            hash = hash * 31 + Id(reward);
            return hash;
        }
    }

    static int Id(RectTransform target) => target ? target.GetInstanceID() : 0;
}
