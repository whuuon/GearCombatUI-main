using UnityEngine;
using UnityEngine.UI;

// Responsive HUD. The game scales its UI by screen HEIGHT (reference 900x1600, match = 1).
// On very tall phones (9:21) that leaves less than the ~740 units of width the HUD layout needs,
// so the sides get cut. This blends toward width-matching only as much as needed to keep
// at least `minWidth` units visible; wider screens (9:16, 9:19.5, 3:4) keep pure height matching.
[ExecuteAlways]
[RequireComponent(typeof(CanvasScaler))]
public class ResponsiveCanvasScaler : MonoBehaviour
{
    public float minWidth = 740f;

    CanvasScaler scaler;
    Vector2 lastScreen;

    void OnEnable()
    {
        scaler = GetComponent<CanvasScaler>();
        lastScreen = Vector2.zero;
        Apply();
    }

    void Update() => Apply();

    void Apply()
    {
        if (scaler == null || Screen.width <= 0 || Screen.height <= 0) return;
        var screen = new Vector2(Screen.width, Screen.height);
        if (screen == lastScreen) return;
        lastScreen = screen;

        var r = scaler.referenceResolution;
        float wLog = Mathf.Log(screen.x / r.x);
        float hLog = Mathf.Log(screen.y / r.y);
        float match = 1f;                              // default: match height (as the game does)
        float widthAtHeightMatch = screen.x / (screen.y / r.y);
        if (widthAtHeightMatch < minWidth && !Mathf.Approximately(hLog, wLog))
        {
            float wanted = Mathf.Log(screen.x / minWidth);   // scale that shows exactly minWidth
            match = Mathf.Clamp01((wanted - wLog) / (hLog - wLog));
        }
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = match;
    }
}
