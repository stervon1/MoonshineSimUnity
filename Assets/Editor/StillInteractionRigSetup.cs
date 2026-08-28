using System.Linq;
using MoonshineSim.Gameplay;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MoonshineSim.EditorTools
{
    /// <summary>
    /// Wires up the look-at still-run mechanic for the prototype:
    ///
    ///  * three white block <see cref="Interactable"/>s in front of the still
    ///    (Start / Cut to hearts / Cut to tails), each bound to the matching
    ///    StillRunController method;
    ///  * a <see cref="PlayerInteractor"/> on the first-person camera;
    ///  * an "InteractionHUD" overlay with just a reticle + look-at prompt
    ///    (the clipboard carries all readout);
    ///  * a code-authored <c>proofCurve</c> + a 90 s run on the StillRunController
    ///    (replacing the sine fallback — tune it in the Inspector afterwards).
    ///
    /// Idempotent. Menu: Tools > White Lightning > Steps > Add Still Interaction Rig
    /// </summary>
    public static class StillInteractionRigSetup
    {
        private static Font LegacyFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        [MenuItem("Tools/White Lightning/Steps/Add Still Interaction Rig")]
        public static void AddRig()
        {
            Scene scene = SceneManager.GetActiveScene();

            var still = Object.FindAnyObjectByType<StillRunController>();
            if (still == null)
            {
                Debug.LogError("[StillInteractionRigSetup] No StillRunController in the scene. " +
                               "Run 'Build Prototype Scene' first.");
                return;
            }

            foreach (var name in new[] { "StillControls", "InteractionHUD" })
            {
                var ex = scene.GetRootGameObjects().FirstOrDefault(g => g.name == name);
                if (ex != null) Object.DestroyImmediate(ex);
            }

            ConfigureStill(still);

            // --- White block controls ------------------------------------
            var controlsRoot = new GameObject("StillControls");
            var white = LoadOrCreateWhite();
            Vector3 basePos = still.transform.position + new Vector3(0f, -0.2f, -0.95f);

            MakeBlock(controlsRoot, white, basePos + new Vector3(-0.9f, 0f, 0f), "Start the run",
                still.StartRun);
            MakeBlock(controlsRoot, white, basePos + new Vector3(-0.3f, 0f, 0f), "Cut to hearts",
                still.CallCutToHearts);
            MakeBlock(controlsRoot, white, basePos + new Vector3(0.3f, 0f, 0f), "Cut to tails",
                still.CallCutToTails);
            MakeBlock(controlsRoot, white, basePos + new Vector3(0.9f, 0f, 0f), "Vent the boiler",
                still.Vent);
            var coolingBlock = MakeBlock(controlsRoot, white, basePos + new Vector3(1.5f, 0f, 0f), "Adjust cooling water",
                still.AdjustCoolingWater);
            AddCoolingCueLight(coolingBlock, still);

            // --- HUD --------------------------------------------------
            var hudGO = new GameObject("InteractionHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = hudGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = hudGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var reticle = MakeImage(hudGO.transform, "Reticle", new Vector2(10f, 10f), Vector2.zero);
            reticle.color = new Color(1f, 1f, 1f, 0.35f);

            var prompt = MakeText(hudGO.transform, "Prompt", new Vector2(0f, -54f), new Vector2(900f, 48f), 26);

            // --- PlayerInteractor on the FP camera ------------------
            var fpc = Object.FindAnyObjectByType<FirstPersonController>();
            var cam = fpc != null ? fpc.GetComponentInChildren<Camera>() : Camera.main;
            if (cam != null)
            {
                var old = cam.GetComponent<PlayerInteractor>();
                if (old != null) Object.DestroyImmediate(old);

                var interactor = cam.gameObject.AddComponent<PlayerInteractor>();

                var holdT = cam.transform.Find("HoldPoint");
                if (holdT == null)
                {
                    var hp = new GameObject("HoldPoint");
                    hp.transform.SetParent(cam.transform, false);
                    hp.transform.localPosition = new Vector3(0.32f, -0.3f, 0.75f);
                    holdT = hp.transform;
                }

                var so = new SerializedObject(interactor);
                so.FindProperty("promptLabel").objectReferenceValue = prompt;
                so.FindProperty("reticle").objectReferenceValue = reticle;
                so.FindProperty("holdPoint").objectReferenceValue = holdT;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning("[StillInteractionRigSetup] No first-person camera found — " +
                                 "run 'Add First-Person Player', then re-run this. Blocks + HUD were still created.");
            }

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = controlsRoot;
            Debug.Log("[StillInteractionRigSetup] Rig added. Walk up to the still, look at a white block, press E.");
        }

        // --- helpers -------------------------------------------------

        private static void ConfigureStill(StillRunController still)
        {
            var curve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.12f, 0.42f),
                new Keyframe(0.40f, 1f),
                new Keyframe(0.72f, 0.82f),
                new Keyframe(1f, 0.12f));
            for (int i = 0; i < curve.length; i++)
            {
                curve.SmoothTangents(i, 0.3f);
            }

            var so = new SerializedObject(still);
            so.FindProperty("runDurationSeconds").floatValue = 90f;
            so.FindProperty("proofCurve").animationCurveValue = curve;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject MakeBlock(GameObject parent, Material mat, Vector3 pos, string verb, UnityEngine.Events.UnityAction action)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = verb;
            block.transform.SetParent(parent.transform, true);
            block.transform.position = pos;
            block.transform.localScale = Vector3.one * 0.24f;
            block.GetComponent<MeshRenderer>().sharedMaterial = mat;

            var interactable = block.AddComponent<Interactable>();
            var so = new SerializedObject(interactable);
            so.FindProperty("verb").stringValue = verb;
            so.ApplyModifiedPropertiesWithoutUndo();

            UnityEventTools.AddPersistentListener(interactable.onActivate, action);
            return block;
        }

        /// <summary>Small point light on the cooling-water block that pulses via
        /// <see cref="StillTaskCue"/> while the intermittent task needs tending.</summary>
        private static void AddCoolingCueLight(GameObject coolingBlock, StillRunController still)
        {
            var lightGO = new GameObject("CoolingCueLight");
            lightGO.transform.SetParent(coolingBlock.transform, false);
            lightGO.transform.localPosition = Vector3.up * 1.2f;

            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.35f, 0.25f);
            light.range = 1.5f;
            light.intensity = 0f;
            light.shadows = LightShadows.None;

            var cue = lightGO.AddComponent<StillTaskCue>();
            var cueSo = new SerializedObject(cue);
            cueSo.FindProperty("stillRun").objectReferenceValue = still;
            cueSo.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Material LoadOrCreateWhite()
        {
            const string path = "Assets/Scenes/_WhiteBlock.mat";
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(urpLit != null ? urpLit : Shader.Find("Standard"));
                if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                {
                    AssetDatabase.CreateFolder("Assets", "Scenes");
                }
                AssetDatabase.CreateAsset(mat, path);
            }
            else if (urpLit != null && mat.shader != urpLit)
            {
                mat.shader = urpLit;
            }
            var c = new Color(0.92f, 0.92f, 0.92f);
            mat.color = c;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Image MakeImage(Transform parent, string name, Vector2 size, Vector2 anchoredPos)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            return go.GetComponent<Image>();
        }

        private static Text MakeText(Transform parent, string name, Vector2 anchoredPos, Vector2 size, int fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            var text = go.GetComponent<Text>();
            text.font = LegacyFont;
            text.fontSize = fontSize;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);
            return text;
        }
    }
}
