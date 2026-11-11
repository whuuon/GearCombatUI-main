using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// UI-only flow controller for the combat screen (no game logic).
// Buttons are wired at runtime by their path inside the HUD:
//   START -> combat state; warning popup remains available with key 7.
//   Settings -> Settings popup (X closes) ; top-right X -> Give Up (Resume / Quit -> gear state)
//   Revive -> back to combat, No Thanks -> Failed ; Victory/Failed buttons -> gear state
//   Tutorial Next/Understood pages ; other buttons give a press "punch" feedback.
// Keyboard (preview): Tab = switch state, 1..8 = popup, 0 = hide popups.
public class UIPreviewSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class ScreenState
    {
        public string name;
        public GameObject hud;        // screen-space combat HUD snapshot
        public GameObject worldUI;    // HP bars / damage numbers in world space
        public GameObject background; // game world render (not UI)
    }

    public ScreenState[] states;
    public int currentState;

    // popup order under "Popups"
    public const int Win = 0, Unlock = 1, Lose = 2, Revive = 3, GiveUp = 4, Tutorial = 5, Warning = 6, Settings = 7;
    const int BuyState = 0, BattleState = 1;

    void Start()
    {
        foreach (var s in states) if (s.hud) Wire(s.hud.transform);
        ShowState(currentState);
        ShowPopup(-1);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) { ShowState((currentState + 1) % states.Length); ShowPopup(-1); }
        if (Input.GetKeyDown(KeyCode.Alpha0)) ShowPopup(-1);
        for (int i = 1; i <= 9; i++)
            if (Input.GetKeyDown(KeyCode.Alpha0 + i)) ShowPopup(i - 1);
    }

    // ---------- wiring ----------

    void Wire(Transform hud)
    {
        // invisible groups must not eat clicks
        foreach (var cg in hud.GetComponentsInChildren<CanvasGroup>(true))
            if (cg.alpha < 0.01f) cg.blocksRaycasts = false;

        foreach (var b in hud.GetComponentsInChildren<Button>(true))
        {
            b.onClick = new Button.ButtonClickedEvent(); // drop listeners that pointed to stripped game scripts
            b.interactable = true;
            var btn = b;
            b.onClick.AddListener(() => StartCoroutine(Punch(btn.transform)));
        }

        On(hud, "GearHandle/ShopContainer/ButtonContainer/StartGameBtn", () => { ShowPopup(-1); ShowState(BattleState); });
        On(hud, "Popups/PopupWarming/Content/ButtonContainer/ButtonYes", () => { ShowPopup(-1); ShowState(BattleState); });
        On(hud, "Popups/PopupWarming/Content/ButtonContainer/ButtonNo", () => ShowPopup(-1));

        On(hud, "LevelProgressBar/SettingsBtn", () => ShowPopup(Settings));
        On(hud, "Popups/SettingsPopup/BG/CloseBtn", () => ShowPopup(-1));

        On(hud, "LevelProgressBar/BackBtn", () => ShowPopup(GiveUp));
        On(hud, "Popups/PopupGiveUp/GiveUpUI/Button/Resume", () => ShowPopup(-1));
        On(hud, "Popups/PopupGiveUp/GiveUpUI/Button/Exit", () => { ShowPopup(-1); ShowState(BuyState); });

        On(hud, "Popups/PopupRevive/ReviseUI/RewardBig/Button", () => { ShowPopup(-1); ShowState(BattleState); });
        On(hud, "Popups/PopupRevive/ReviseUI/NoThanksButton", () => ShowPopup(Lose));

        foreach (var p in new[] { "Popups/PopupWin/WinUI/ButtonAds", "Popups/PopupWin/WinUI/NoThanksButton",
                                  "Popups/PopupLose/LoseUI/ButtonAds", "Popups/PopupLose/LoseUI/NoThanksButton" })
            On(hud, p, () => { ShowPopup(-1); ShowState(BuyState); });

        On(hud, "Popups/PopupTutorior/Tutorior1/ButtonContainer/ButtonNext", () => TutorialPage(hud, 1));
        On(hud, "Popups/PopupTutorior/Tutorior2/ButtonContainer/ButtonNext", () => TutorialPage(hud, 2));
        On(hud, "Popups/PopupTutorior/Tutorior3/ButtonContainer/ButtonUnderstood", () => ShowPopup(-1));
    }

    static void On(Transform hud, string path, UnityEngine.Events.UnityAction action)
    {
        var t = hud.Find(path);
        if (!t && path == "GearHandle/ShopContainer/ButtonContainer/StartGameBtn")
            t = hud.Find("ReferenceUI/ShopContainer/StartGameBtn");
        if (!t) t = hud.Find("ReferenceUI/" + path);
        var b = t ? t.GetComponent<Button>() : null;
        if (b != null) b.onClick.AddListener(action);
        else Debug.LogWarning("[UIPreview] button not found: " + path);
    }

    void TutorialPage(Transform hud, int page)
    {
        var root = hud.Find("Popups/PopupTutorior");
        for (int i = 0; i < 3; i++)
        {
            var t = root.Find("Tutorior" + (i + 1));
            if (t) t.gameObject.SetActive(i == page);
        }
    }

    static IEnumerator Punch(Transform t)
    {
        var baseScale = t.localScale;
        for (float k = 0; k < 1f; k += Time.unscaledDeltaTime / 0.18f)
        {
            float s = 1f - 0.12f * Mathf.Sin(k * Mathf.PI);
            t.localScale = baseScale * s;
            yield return null;
        }
        t.localScale = baseScale;
    }

    // ---------- states & popups ----------

    public void ShowState(int index)
    {
        currentState = index;
        for (int i = 0; i < states.Length; i++)
        {
            bool on = i == index;
            if (states[i].hud) states[i].hud.SetActive(on);
            if (states[i].worldUI) states[i].worldUI.SetActive(on);
            if (states[i].background) states[i].background.SetActive(on);
        }
    }

    Transform PopupsRoot()
    {
        var hud = states[currentState].hud;
        return hud ? hud.transform.Find("Popups") : null;
    }

    public void ShowPopup(int index)
    {
        var current = PopupsRoot();
        foreach (var s in states)
        {
            var root = s.hud ? s.hud.transform.Find("Popups") : null;
            if (root == null) continue;
            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                bool on = i == index && root == current;
                var cg = child.GetComponent<CanvasGroup>();
                if (cg != null) { cg.alpha = on ? 1 : 0; cg.blocksRaycasts = on; cg.interactable = on; }
                // panels that slide in from off-screen are reset to the center
                foreach (RectTransform rt in child)
                    if (on && Mathf.Abs(rt.anchoredPosition.y) > 1500) rt.anchoredPosition = Vector2.zero;
                if (on && i == Tutorial) TutorialPage(s.hud.transform, 0);
            }
        }
    }
}
