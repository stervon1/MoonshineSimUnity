using System.Linq;
using MoonshineSim.Core;
using MoonshineSim.Gameplay;
using MoonshineSim.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MoonshineSim.EditorTools
{
    /// <summary>
    /// Adds the toggleable job clipboard to the active scene: a top-right HUD
    /// panel (hidden by default, Tab to toggle) plus a <see cref="ClipboardController"/>
    /// wired to the scene's GameState and StillRunController.
    ///
    /// Idempotent. Menu: Tools > White Lightning > Add Clipboard
    /// </summary>
    public static class ClipboardSetup
    {
        private const string RootName = "ClipboardHUD";
        private static Font LegacyFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        [MenuItem("Tools/White Lightning/Steps/Add Clipboard")]
        public static void AddClipboard()
        {
            Scene scene = SceneManager.GetActiveScene();

            var existing = scene.GetRootGameObjects().FirstOrDefault(g => g.name == RootName);
            if (existing != null) Object.DestroyImmediate(existing);

            // --- Canvas ---------------------------------------------------
            var hud = new GameObject(RootName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = hud.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9;
            var scaler = hud.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            // --- Panel (top-right, hidden by default) -------------------
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(hud.transform, false);
            var prt = (RectTransform)panel.transform;
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(1f, 1f);
            prt.sizeDelta = new Vector2(400f, 560f);
            prt.anchoredPosition = new Vector2(-24f, -24f);
            panel.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.06f, 0.82f);

            // Title: a fixed-height bar pinned to the top edge.
            var title = MakeText(prt, "Title",
                anchorMin: new Vector2(0f, 1f), anchorMax: new Vector2(1f, 1f),
                offsetMin: new Vector2(16f, -40f), offsetMax: new Vector2(-16f, -12f),
                fontSize: 18, style: FontStyle.Bold);
            title.text = "CLIPBOARD   [Tab]";

            // Body: stretches to fill the panel below the title.
            var body = MakeText(prt, "Body",
                anchorMin: new Vector2(0f, 0f), anchorMax: new Vector2(1f, 1f),
                offsetMin: new Vector2(16f, 16f), offsetMax: new Vector2(-16f, -46f),
                fontSize: 16, style: FontStyle.Normal);
            body.text = "…";

            panel.SetActive(false);

            // --- Controller --------------------------------------------
            var go = new GameObject("Clipboard");
            var controller = go.AddComponent<ClipboardController>();

            var so = new SerializedObject(controller);
            so.FindProperty("batch").objectReferenceValue = Object.FindAnyObjectByType<BatchController>();
            so.FindProperty("gameState").objectReferenceValue = Object.FindAnyObjectByType<GameState>();
            so.FindProperty("stillRun").objectReferenceValue = Object.FindAnyObjectByType<StillRunController>();
            so.FindProperty("panelRoot").objectReferenceValue = panel;
            so.FindProperty("bodyText").objectReferenceValue = body;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = go;
            Debug.Log("[ClipboardSetup] Clipboard added (hidden). Press Tab in Play mode to toggle it.");
        }

        private static Text MakeText(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax,
            int fontSize, FontStyle style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            var text = go.GetComponent<Text>();
            text.font = LegacyFont;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }
    }
}
