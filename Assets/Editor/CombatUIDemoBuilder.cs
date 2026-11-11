#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Builds the demo scene from UI snapshots taken while the real game was running
// (Stage 5, iPhone 12 simulator): one "Buy gear" state and one "Battle" state.
public static class CombatUIDemoBuilder
{
    const string Root = "Assets/CombatUI";
    const string ScenePath = Root + "/CombatUI_Demo.unity";
    static readonly string[] States = { "Buy", "Battle" };

    [MenuItem("Combat UI/Rebuild Demo Scene")]
    public static void Build()
    {
        // one-time: move snapshot assets into Assets/CombatUI
        if (AssetDatabase.IsValidFolder("Assets/ZZ_UISnapshot"))
        {
            foreach (var sub in new[] { "Prefabs", "Backgrounds" })
                if (!AssetDatabase.IsValidFolder(Root + "/" + sub)) AssetDatabase.CreateFolder(Root, sub);
            foreach (var g in AssetDatabase.FindAssets("", new[] { "Assets/ZZ_UISnapshot" }))
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                var sub = p.EndsWith(".png") ? "Backgrounds" : "Prefabs";
                AssetDatabase.MoveAsset(p, $"{Root}/{sub}/{System.IO.Path.GetFileName(p)}");
            }
            AssetDatabase.DeleteAsset("Assets/ZZ_UISnapshot");
        }
        foreach (var s in States)
        {
            var ti = (TextureImporter)AssetImporter.GetAtPath($"{Root}/Backgrounds/Background_{s}.png");
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.mipmapEnabled = false;
                ti.maxTextureSize = 4096;
                // rendered at 1536x2048 with the game camera (ortho size 11 => 22 world units tall)
                ti.spritePixelsPerUnit = 2048f / 22f;
                ti.SaveAndReimport();
            }
        }

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // camera identical to the game's MainScene camera
        var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 11;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 1000;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camGo.transform.SetPositionAndRotation(new Vector3(0, 7.95f, -9f), Quaternion.Euler(45, 0, 0));

        new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

        var preview = new GameObject("UI Preview").AddComponent<UIPreviewSwitcher>();
        preview.states = new UIPreviewSwitcher.ScreenState[States.Length];

        for (int i = 0; i < States.Length; i++)
        {
            string s = States[i];
            var st = new UIPreviewSwitcher.ScreenState { name = s };

            // background: render of the game world (not UI), placed in world space exactly on the
            // camera's view plane, 22 units tall (= ortho size 11 * 2) and wide enough for 3:4.
            // Narrow screens (9:21) just crop the sides, like the real game camera does.
            var bg = new GameObject($"Background_{s}");
            bg.transform.SetPositionAndRotation(camGo.transform.position + camGo.transform.forward * 500f, camGo.transform.rotation);
            var sr = bg.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{Root}/Backgrounds/Background_{s}.png");
            sr.sortingOrder = -1000;
            st.background = bg;

            // world-space UI (boat HP, enemy HP, damage numbers) at their in-game positions
            var worldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Root}/Prefabs/WorldUI_{s}.prefab");
            if (worldPrefab) { st.worldUI = (GameObject)PrefabUtility.InstantiatePrefab(worldPrefab); st.worldUI.transform.position = Vector3.zero; }

            // screen-space HUD
            var hudPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{Root}/Prefabs/CombatHUD_{s}.prefab");
            var hud = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab);
            hud.name = $"CombatHUD_{s}";
            var c = hud.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceCamera;
            c.worldCamera = cam;
            c.planeDistance = 3;
            if (hud.GetComponent<ResponsiveCanvasScaler>() == null) hud.AddComponent<ResponsiveCanvasScaler>();
            // static-snapshot fixes: the button shine overlay was frozen mid-sweep (its driver
            // script is gone), and the price tag must draw above the gear icon
            foreach (var im in hud.GetComponentsInChildren<Image>(true))
                if (im.material != null && im.material.name.Contains("Shine")) im.enabled = false;
            foreach (var cv in hud.GetComponentsInChildren<Canvas>(true))
                if (cv.name == "bg_price" && cv.overrideSorting) cv.sortingOrder = 15;
            st.hud = hud;

            preview.states[i] = st;
        }
        preview.currentState = 0;
        preview.ShowState(0);
        preview.ShowPopup(-1);

        AssignmentUIBuilder.Apply(preview);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Combat UI/Reimport Shaders")]
    public static void ReimportShaders()
    {
        foreach (var g in AssetDatabase.FindAssets("t:Shader", new[] { "Assets" }))
            AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(g), ImportAssetOptions.ForceUpdate);
    }
}
#endif
