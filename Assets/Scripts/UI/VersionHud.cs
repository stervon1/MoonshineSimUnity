using MoonshineSim.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MoonshineSim.UI
{
    /// <summary>
    /// Always-on version stamp in the top-left corner of the screen.
    ///
    /// Bootstraps itself via <see cref="RuntimeInitializeOnLoadMethodAttribute"/>
    /// before the first scene loads, so it shows in every scene (editor Play mode
    /// and player builds alike) with no scene wiring. The text comes from
    /// <see cref="BuildVersion.Current"/>, which the editor stamps on every
    /// recompile and every build.
    ///
    /// Press F2 to hide / show it.
    /// </summary>
    [AddComponentMenu("")] // created only by the bootstrap below
    public sealed class VersionHud : MonoBehaviour
    {
        private const string RootName = "~VersionHud";
        private const KeyCode ToggleKey = KeyCode.F2;

        private Text _label;
        private Text _shadow;
        private bool _visible = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (GameObject.Find(RootName) != null) return;

            var go = new GameObject(RootName, typeof(Canvas), typeof(CanvasScaler));
            DontDestroyOnLoad(go);

            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue; // above every other HUD (clipboard is 9)

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<VersionHud>();
        }

        private void Awake()
        {
            // Drop shadow (offset copy behind the label) so it stays readable on any background.
            _shadow = MakeLabel("Shadow", new Color(0f, 0f, 0f, 0.6f), new Vector2(17f, -13f));
            _label  = MakeLabel("Text", new Color(1f, 1f, 1f, 0.85f), new Vector2(16f, -12f));
            Refresh();
        }

        private Text MakeLabel(string name, Color color, Vector2 topLeftInset)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(transform, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0f, 1f); // top-left corner
            rt.anchoredPosition = topLeftInset;
            rt.sizeDelta = new Vector2(900f, 60f);

            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 15;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.color = color;
            return text;
        }

        private void Update()
        {
            if (Input.GetKeyDown(ToggleKey))
            {
                _visible = !_visible;
                if (_label != null) _label.enabled = _visible;
                if (_shadow != null) _shadow.enabled = _visible;
            }
        }

        /// <summary>Re-read the stamp and repaint.</summary>
        public void Refresh()
        {
            BuildVersion.InvalidateCache();
            string s = BuildVersion.Current.Short;
            if (_label != null) _label.text = s;
            if (_shadow != null) _shadow.text = s;
        }
    }
}
