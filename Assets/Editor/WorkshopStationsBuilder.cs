using System.Linq;
using MoonshineSim.Core;
using MoonshineSim.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MoonshineSim.EditorTools
{
    /// <summary>
    /// Places the Milestone 2 world stations into the active scene, all wired to
    /// a <see cref="BatchController"/>: six grain bins, a mash tub, a fermenter,
    /// a proofing station + a 3-dial <see cref="StillGauge"/> board, a carryable jar,
    /// three buyer counters, a workbench (<see cref="UpgradeStation"/>), placeholder
    /// still VFX/audio, plus **signs**, a **next-step arrow**, and right-click
    /// **info cards** on every station.
    ///
    /// All greybox primitives for now — reskin with prop packs in-editor later.
    /// Idempotent. Menu: Tools > White Lightning > Steps > Add Workshop Stations
    /// </summary>
    public static class WorkshopStationsBuilder
    {
        private const string RootName = "WorkshopStations";
        private static Font LegacyFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        [MenuItem("Tools/White Lightning/Steps/Add Workshop Stations")]
        public static void AddStations()
        {
            Scene scene = SceneManager.GetActiveScene();

            var still = Object.FindAnyObjectByType<StillRunController>();
            if (still == null)
            {
                Debug.LogError("[WorkshopStationsBuilder] No StillRunController — run Build Prototype Scene first.");
                return;
            }

            var existing = scene.GetRootGameObjects().FirstOrDefault(g => g.name == RootName);
            if (existing != null) Object.DestroyImmediate(existing);

            var root = new GameObject(RootName);
            Vector3 o = new Vector3(still.transform.position.x, 0f, still.transform.position.z);

            var bcGO = new GameObject("BatchController");
            bcGO.transform.SetParent(root.transform, false);
            var batch = bcGO.AddComponent<BatchController>();

            // --- Jar ---------------------------------------------------
            var jar = MakePrimitive(PrimitiveType.Cylinder, root.transform, "Jar",
                o + new Vector3(0.6f, 0.95f, -0.7f), new Vector3(0.14f, 0.16f, 0.14f),
                Mat("_StnJar", new Color(0.85f, 0.9f, 0.95f, 1f)));
            jar.AddComponent<Rigidbody>().isKinematic = true;
            jar.AddComponent<Carryable>();

            // --- Bucket (carry grain from a bin to the mash tub) ----
            var bucket = MakePrimitive(PrimitiveType.Cube, root.transform, "Bucket",
                o + new Vector3(-3.8f, 0.55f, -4.0f), new Vector3(0.34f, 0.4f, 0.34f),
                Mat("_StnBucket", new Color(0.32f, 0.34f, 0.38f)));
            bucket.AddComponent<Rigidbody>().isKinematic = true;
            var bucketCar = bucket.AddComponent<Carryable>();
            var bucketFill = MakePrimitive(PrimitiveType.Cube, bucket.transform, "Fill",
                bucket.transform.position, Vector3.one, Mat("_StnBucketFill", new Color(0.82f, 0.68f, 0.34f)));
            Object.DestroyImmediate(bucketFill.GetComponent<Collider>());
            bucketFill.transform.localPosition = Vector3.zero;
            bucketFill.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            WireString(bucketCar, "kind", "bucket");
            WireFloat(bucketCar, "capacity", 1f);
            Wire(bucketCar, "fillVisual", bucketFill.transform);

            // --- Grain bins (6) -------------------------------------
            var grains = new[]
            {
                (SpiritStyle.CornWhiskey,  new Color(0.85f, 0.72f, 0.25f)),
                (SpiritStyle.Rye,          new Color(0.55f, 0.40f, 0.25f)),
                (SpiritStyle.MaltedBarley, new Color(0.72f, 0.52f, 0.30f)),
                (SpiritStyle.Wheat,        new Color(0.88f, 0.82f, 0.55f)),
                (SpiritStyle.SugarShine,   new Color(0.93f, 0.93f, 0.90f)),
                (SpiritStyle.Molasses,     new Color(0.32f, 0.22f, 0.16f)),
            };
            for (int i = 0; i < grains.Length; i++)
            {
                AddGrainBin(root, grains[i].Item1, grains[i].Item2,
                    o + new Vector3(-3.8f, 0.4f, -2.5f + i));
            }

            // --- Mash tub + fermenter (greybox) --------------
            var mash = MakePrimitive(PrimitiveType.Cylinder, root.transform, "MashTub",
                o + new Vector3(-2f, 0.5f, 2f), new Vector3(0.9f, 0.5f, 0.9f),
                Mat("_StnMash", new Color(0.5f, 0.42f, 0.3f)));
            var mashPour = mash.AddComponent<PourTarget>();
            WireEnum(mashPour, "accept", (int)VesselContents.Grain);
            WireFloat(mashPour, "unitsPerSecond", 0.8f);
            Wire(mash.AddComponent<MashStation>(), "batch", batch);
            Sign(root.transform, "Mash Tub", mash.transform.position + Vector3.up * 1.1f);
            InfoCard(root.transform, mash, "Mash Tub",
                "Hold E with a bucket of grain to pour it in — the first pour picks the spirit, the amount sets the batch size (capped by your rig). Mashing then cooks the grain in hot water (~148 F / 64 C) so its starch turns to fermentable sugar.");

            var ferm = MakePrimitive(PrimitiveType.Cylinder, root.transform, "Fermenter",
                o + new Vector3(-2f, 0.55f, 3.6f), new Vector3(0.85f, 0.6f, 0.85f),
                Mat("_StnFerm", new Color(0.35f, 0.5f, 0.55f)));
            Wire(ferm.AddComponent<FermentStation>(), "batch", batch);
            Sign(root.transform, "Fermenter", ferm.transform.position + Vector3.up * 1.15f);
            InfoCard(root.transform, ferm, "Fermenter",
                "Yeast eats the sugar over days, turning the mash into an ~8-12% ABV wash (plus CO2). Cool and slow keeps off-flavours down.");

            // --- Proofing station ---------------------------
            var proof = MakePrimitive(PrimitiveType.Cube, root.transform, "ProofingStation",
                o + new Vector3(2.2f, 0.5f, 1.2f), new Vector3(0.8f, 1f, 0.6f),
                Mat("_StnProof", new Color(0.4f, 0.45f, 0.5f)));
            Wire(proof.AddComponent<ProofingStation>(), "batch", batch);
            Sign(root.transform, "Proofing", proof.transform.position + Vector3.up * 1.2f);
            InfoCard(root.transform, proof, "Proofing",
                "Fresh hearts run 120-150 proof. Add water a little at a time to bring it down to what your buyer wants (usually 80-110), watching the gauge.");

            // --- Gauge board: proof / temperature / pressure ---
            var board = new GameObject("GaugeBoard");
            board.transform.SetParent(root.transform, false);
            board.transform.position = still.transform.position + new Vector3(0f, 0.9f, -0.95f);
            MakeDial(board.transform, batch, still, new Vector3(-0.34f, 0f, 0f), GaugeReading.Proof, 160f,
                new Color(0.86f, 0.9f, 0.95f));
            MakeDial(board.transform, batch, still, Vector3.zero, GaugeReading.Temperature, 110f,
                new Color(1f, 0.6f, 0.35f));
            MakeDial(board.transform, batch, still, new Vector3(0.34f, 0f, 0f), GaugeReading.Pressure, 20f,
                new Color(0.72f, 0.76f, 1f));

            // --- Still sign + status readout + info -----------
            Sign(root.transform, "Still", still.transform.position + Vector3.up * 1.4f);
            if (still.GetComponent<StillStatusReadout>() == null)
            {
                Wire(still.gameObject.AddComponent<StillStatusReadout>(), "stillRun", still);
            }
            InfoCard(root.transform, still.gameObject, "The Still",
                "Heat drives alcohol (boils 78 C) off ahead of water (100 C). Take it in cuts: toss the foreshots, set aside the sharp heads, keep the clean hearts, stop before the tails.");

            // --- Buyer counters (3) --------------------------
            GameObject buyer0 = null;
            for (int i = 0; i < 3; i++)
            {
                var counter = MakePrimitive(PrimitiveType.Cube, root.transform, $"Buyer_{i}",
                    o + new Vector3(-3f + i * 3f, 0.5f, -6f), new Vector3(1.4f, 1f, 0.7f),
                    Mat("_StnBuyer", new Color(0.3f, 0.32f, 0.36f)));
                var bc = counter.AddComponent<BuyerCounter>();
                Wire(bc, "batch", batch);
                WireInt(bc, "buyerIndex", i);
                Sign(root.transform, $"Back Door {i + 1}", counter.transform.position + Vector3.up * 1.2f);
                InfoCard(root.transform, counter, "A Buyer",
                    "A back-door buyer. Look at the counter to read their taste, or open the clipboard (Tab) for all three. Match their spirit and proof band for the best price.");
                if (i == 0) buyer0 = counter;
            }

            // --- Workbench (economy / upgrades) ------------
            var bench = MakePrimitive(PrimitiveType.Cube, root.transform, "Workbench",
                o + new Vector3(4.2f, 0.45f, -2f), new Vector3(1.6f, 0.9f, 0.8f),
                Mat("_StnBench", new Color(0.45f, 0.35f, 0.25f)));
            bench.AddComponent<UpgradeStation>();
            Sign(root.transform, "Workbench", bench.transform.position + Vector3.up * 1.2f);
            InfoCard(root.transform, bench, "Workbench",
                "Spend the cash you earn selling jars: bigger boiler, better still, a hollow to work in, and finally a distiller's licence. The clipboard tracks what's next.");

            // --- Next-step arrow ---------------------------
            var markerGO = new GameObject("NextStepMarker");
            markerGO.transform.SetParent(root.transform, false);
            var marker = markerGO.AddComponent<NextStepMarker>();
            var arrowT = BuildArrow(markerGO.transform);
            Wire(marker, "batch", batch);
            Wire(marker, "arrow", arrowT);
            Wire(marker, "grainTarget", bucket != null ? bucket.transform : mash.transform);
            Wire(marker, "mashTarget", mash.transform);
            Wire(marker, "fermentTarget", ferm.transform);
            Wire(marker, "stillTarget", still.transform);
            Wire(marker, "proofTarget", proof.transform);
            Wire(marker, "sellTarget", buyer0 != null ? buyer0.transform : proof.transform);

            // --- Still FX + audio ------------------------
            var stream = MakeParticles(root.transform, "StreamFX",
                still.transform.position + new Vector3(0f, -0.4f, -0.35f), true);
            var steam = MakeParticles(root.transform, "SteamFX",
                still.transform.position + new Vector3(0f, 0.6f, 0f), false);
            var fx = new GameObject("StillFX").AddComponent<StillFX>();
            fx.transform.SetParent(root.transform, false);
            Wire(fx, "stillRun", still);
            Wire(fx, "stream", stream);
            Wire(fx, "steam", steam);

            if (still.GetComponent<AudioSource>() == null) still.gameObject.AddComponent<AudioSource>();
            if (still.GetComponent<StillAudio>() == null)
            {
                Wire(still.gameObject.AddComponent<StillAudio>(), "stillRun", still);
            }

            Wire(batch, "gameState", Object.FindAnyObjectByType<GameState>());
            Wire(batch, "stillRun", still);
            Wire(batch, "jar", jar);

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = root;
            Debug.Log("[WorkshopStationsBuilder] Stations + signs + arrow + info cards placed. " +
                      "Right-click a station for what it does.");
        }

        // --- station helpers -----------------------------------------

        private static GameObject AddGrainBin(GameObject root,
            SpiritStyle style, Color color, Vector3 pos)
        {
            var go = MakePrimitive(PrimitiveType.Cube, root.transform, $"GrainBin_{style}", pos,
                new Vector3(0.7f, 0.8f, 0.7f), Mat($"_StnGrain{style}", color));

            var fs = go.AddComponent<FillSource>();
            WireEnum(fs, "provides", (int)VesselContents.Grain);
            WireEnum(fs, "grainStyle", (int)style);
            WireString(fs, "label", GrainWord(style).ToLowerInvariant());
            WireFloat(fs, "unitsPerSecond", 0.6f);

            Sign(root.transform, GrainWord(style), pos + Vector3.up * 1.15f);
            InfoCard(root.transform, go, $"{GrainWord(style)} bin",
                "Grab the bucket, then hold E here to scoop grain into it — how much you scoop " +
                "sets the batch size. Carry it to the mash tub and hold E to pour it in. " +
                "Corn = sweet whiskey, rye = spicy, malt = clean, wheat = soft, " +
                "sugar/molasses = light shine or rum. (A market to buy grain comes later.)");
            return go;
        }

        private static string GrainWord(SpiritStyle s) => s switch
        {
            SpiritStyle.CornWhiskey => "Corn",
            SpiritStyle.Rye => "Rye",
            SpiritStyle.SugarShine => "Sugar",
            SpiritStyle.Wheat => "Wheat",
            SpiritStyle.MaltedBarley => "Malt",
            SpiritStyle.Molasses => "Molasses",
            _ => s.ToString()
        };

        // --- signs & info cards (world-space uGUI, faces the camera) ---

        private static void Sign(Transform root, string label, Vector3 worldPos)
        {
            var go = NewWorldCanvas(root, "Sign", worldPos, 320f, 70f, 1.05f);
            AddBg(go, new Color(0.12f, 0.1f, 0.07f, 0.92f));
            var t = AddText(go.transform, "Label", label, 40, FontStyle.Bold, TextAnchor.MiddleCenter);
            Stretch((RectTransform)t.transform, 6f);
            go.AddComponent<Billboard>();
        }

        private static void InfoCard(Transform root, GameObject station, string title, string body)
            => InfoCard(root, station, title, body, station.transform.position);

        private static void InfoCard(Transform root, GameObject station, string title, string body, Vector3 at)
        {
            var go = NewWorldCanvas(root, "InfoCard", at + Vector3.up * 2f, 460f, 300f, 3.4f);
            AddBg(go, new Color(0.08f, 0.06f, 0.05f, 0.92f));

            var t = AddText(go.transform, "Title", title, 34, FontStyle.Bold, TextAnchor.UpperCenter);
            var trt = (RectTransform)t.transform;
            trt.anchorMin = new Vector2(0f, 1f); trt.anchorMax = new Vector2(1f, 1f);
            trt.offsetMin = new Vector2(16f, -54f); trt.offsetMax = new Vector2(-16f, -12f);

            var b = AddText(go.transform, "Body", body, 26, FontStyle.Normal, TextAnchor.UpperLeft);
            var brt = (RectTransform)b.transform;
            brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one;
            brt.offsetMin = new Vector2(16f, 16f); brt.offsetMax = new Vector2(-16f, -60f);

            go.SetActive(false);
            go.AddComponent<Billboard>();

            var sic = station.AddComponent<StationInfoCard>();
            Wire(sic, "card", go);
        }

        private static GameObject NewWorldCanvas(Transform parent, string name, Vector3 worldPos,
            float pxW, float pxH, float worldWidth)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas));
            go.transform.SetParent(parent, true);
            go.transform.position = worldPos;
            go.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(pxW, pxH);
            rt.localScale = Vector3.one * (worldWidth / pxW);
            return go;
        }

        private static void AddBg(GameObject canvas, Color color)
        {
            var bg = new GameObject("BG", typeof(RectTransform), typeof(Image));
            bg.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)bg.transform, 0f);
            bg.GetComponent<Image>().color = color;
        }

        private static Text AddText(Transform parent, string name, string content,
            int fontSize, FontStyle style, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = LegacyFont;
            t.fontSize = fontSize;
            t.fontStyle = style;
            t.color = Color.white;
            t.alignment = anchor;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.text = content;
            return t;
        }

        private static void Stretch(RectTransform rt, float inset)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
        }

        private static Transform BuildArrow(Transform parent)
        {
            var arrow = new GameObject("Arrow");
            arrow.transform.SetParent(parent, false);
            var glow = Mat("_StnMarker", new Color(1f, 0.85f, 0.2f));
            var shaft = MakePrimitive(PrimitiveType.Cube, arrow.transform, "Shaft",
                arrow.transform.position + Vector3.up * 0.3f, new Vector3(0.07f, 0.5f, 0.07f), glow);
            var head = MakePrimitive(PrimitiveType.Cube, arrow.transform, "Head",
                arrow.transform.position, new Vector3(0.26f, 0.26f, 0.26f), glow);
            head.transform.localRotation = Quaternion.Euler(45f, 0f, 45f);
            Object.DestroyImmediate(shaft.GetComponent<Collider>());
            Object.DestroyImmediate(head.GetComponent<Collider>());
            return arrow.transform;
        }

        // --- dials --------------------------------------------------

        private static void MakeDial(Transform parent, BatchController batch, StillRunController still,
            Vector3 localOffset, GaugeReading reading, float maxValue, Color faceColor)
        {
            var dial = new GameObject($"Dial_{reading}");
            dial.transform.SetParent(parent, false);
            dial.transform.localPosition = localOffset;

            var face = MakePrimitive(PrimitiveType.Cylinder, dial.transform, "Face",
                dial.transform.position, new Vector3(0.26f, 0.02f, 0.26f), Mat($"_Dial{reading}", faceColor));
            face.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var needle = MakePrimitive(PrimitiveType.Cube, dial.transform, "Needle",
                dial.transform.position + new Vector3(0f, 0.09f, -0.02f),
                new Vector3(0.015f, 0.16f, 0.008f), Mat("_DialNeedle", new Color(0.85f, 0.1f, 0.1f)));
            Object.DestroyImmediate(face.GetComponent<Collider>());
            Object.DestroyImmediate(needle.GetComponent<Collider>());

            var g = dial.AddComponent<StillGauge>();
            Wire(g, "needle", needle.transform);
            Wire(g, "stillRun", still);
            Wire(g, "batch", batch);
            WireEnum(g, "reading", (int)reading);
            WireFloat(g, "maxValue", maxValue);
        }

        // --- primitives / particles / materials / wiring ---------

        private static GameObject MakePrimitive(PrimitiveType type, Transform parent, string name,
            Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, true);
            go.transform.position = pos;
            go.transform.localScale = scale;
            var r = go.GetComponent<MeshRenderer>();
            if (r != null) r.sharedMaterial = mat;
            return go;
        }

        private static ParticleSystem MakeParticles(Transform parent, string name, Vector3 pos, bool downward)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(downward ? 90f : -90f, 0f, 0f);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = downward ? 0.5f : 2.2f;
            main.startSpeed = downward ? 2.6f : 0.5f;
            main.startSize = downward ? 0.035f : 0.25f;
            main.gravityModifier = downward ? 1f : -0.05f;
            main.maxParticles = 300;
            main.startColor = downward ? new Color(1f, 1f, 1f, 0.8f) : new Color(1f, 1f, 1f, 0.15f);

            var em = ps.emission;
            em.rateOverTime = 0f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = downward ? 3f : 22f;
            shape.radius = downward ? 0.02f : 0.15f;

            var r = go.GetComponent<ParticleSystemRenderer>();
            var sh = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Sprites/Default");
            if (sh != null) r.sharedMaterial = new Material(sh);
            return ps;
        }

        private static Material Mat(string assetName, Color color)
        {
            string path = $"Assets/Scenes/{assetName}.mat";
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(urpLit != null ? urpLit : Shader.Find("Standard"));
                if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
                AssetDatabase.CreateAsset(mat, path);
            }
            else if (urpLit != null && mat.shader != urpLit)
            {
                mat.shader = urpLit;
            }
            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static void Wire(Component target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var p = so.FindProperty(field);
            if (p == null) { Debug.LogError($"[WorkshopStationsBuilder] {target.GetType().Name}.{field} not found"); return; }
            p.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireInt(Component target, string field, int value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireEnum(Component target, string field, int value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).enumValueIndex = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireFloat(Component target, string field, float value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void WireString(Component target, string field, string value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
