using System.Linq;
using MoonshineSim.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoonshineSim.EditorTools
{
    /// <summary>
    /// Blocks in an open-air still clearing into the active scene — a roomy
    /// mechanics sandbox that also seeds the Milestone 3 Tier-2 "mountain
    /// hollow" setting. Uses SimpleNaturePack + Polytope Studio flora,
    /// resolved by prefab name so the deep pack folders don't need hardcoding.
    ///
    /// Idempotent: removes any previous "OutdoorEnvironment" root and the
    /// "BasementRoom" root (shared world origin) before rebuilding. Leaves
    /// GameState / Canvas / the "Still" object alone, and does not move the
    /// camera when it belongs to a first-person Player rig.
    ///
    /// Menu: Tools > White Lightning > Build Outdoor Sandbox
    /// </summary>
    public static class OutdoorSceneBuilder
    {
        private const string RootName = "OutdoorEnvironment";

        [MenuItem("Tools/White Lightning/Steps/Build Outdoor Sandbox")]
        public static void BuildOutdoorSandbox()
        {
            Scene scene = SceneManager.GetActiveScene();
            foreach (var name in new[] { RootName, "BasementRoom" })
            {
                var ex = scene.GetRootGameObjects().FirstOrDefault(g => g.name == name);
                if (ex != null)
                {
                    Object.DestroyImmediate(ex);
                }
            }

            var root = new GameObject(RootName);
            var rng = new System.Random(20260827);

            // --- Ground --------------------------------------------------
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(root.transform, false);
            ground.transform.localScale = Vector3.one * 8f;   // 80 x 80 m
            ground.GetComponent<MeshRenderer>().sharedMaterial = GroundMaterial();

            // --- Still pad + heat -------------------------------------
            var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pad.name = "StillPad";
            pad.transform.SetParent(root.transform, false);
            pad.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            pad.transform.localScale = new Vector3(4f, 0.2f, 4f);
            pad.GetComponent<MeshRenderer>().sharedMaterial =
                SolidMaterial("_OutdoorPad", new Color(0.34f, 0.32f, 0.30f));

            var firewood = Find("FirewoodStack");
            if (firewood != null)
            {
                Place(firewood, root, new Vector3(1.4f, 0.2f, 0.6f), Quaternion.Euler(0f, 40f, 0f));
            }

            var still = Object.FindAnyObjectByType<StillRunController>();
            if (still != null)
            {
                still.transform.position = new Vector3(0f, 1.2f, 0f);
                still.transform.localScale = Vector3.one * 0.6f;
            }

            var stillLamp = new GameObject("StillLamp");
            stillLamp.transform.SetParent(root.transform, false);
            stillLamp.transform.localPosition = new Vector3(0.6f, 1.6f, 0.6f);
            var pl = stillLamp.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = new Color(1f, 0.68f, 0.38f);
            pl.intensity = 2.2f;
            pl.range = 7f;
            pl.shadows = LightShadows.None; // outdoor is sun-lit; a warm fill needs no shadow map

            // --- Flora scatter -------------------------------------
            Scatter(root, "Trees", rng, 45, rMin: 9f, rMax: 40f, sMin: 0.85f, sMax: 1.5f,
                "Tree_01", "Tree_02", "Tree_03", "Tree_04", "Tree_05",
                "PT_Pine_Tree_03_green", "PT_Fruit_Tree_01_green", "PT_Fruit_Tree_01_apples");

            Scatter(root, "Rocks", rng, 30, rMin: 6f, rMax: 42f, sMin: 0.6f, sMax: 1.8f,
                "Rock_01", "Rock_02", "Rock_03", "Rock_04", "Rock_05",
                "PT_Generic_Rock_01", "PT_River_Rock_Pile_02");

            Scatter(root, "Undergrowth", rng, 110, rMin: 3f, rMax: 38f, sMin: 0.7f, sMax: 1.3f,
                "Bush_01", "Bush_02", "Bush_03",
                "Flowers_01", "Flowers_02", "PT_Generic_Shrub_01_green");

            // Dense low grass so the ground reads as a meadow, not a plane.
            Scatter(root, "GrassTufts", rng, 150, rMin: 2.5f, rMax: 24f, sMin: 0.6f, sMax: 1.25f,
                "Grass_01", "Grass_02", "PT_Grass_02");

            // --- Sky + sun ------------------------------------------
            var dir = Object.FindObjectsByType<Light>(FindObjectsInactive.Include)
                .FirstOrDefault(x => x.type == LightType.Directional);
            if (dir != null)
            {
                dir.intensity = 1.25f;
                dir.color = new Color(1f, 0.96f, 0.88f);
                dir.shadows = LightShadows.Soft;
                dir.transform.rotation = Quaternion.Euler(48f, -25f, 0f);
            }
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
            RenderSettings.ambientIntensity = 1f;

            // Keep the additional-light shadow atlas sane: only the sun casts
            // shadows. Prop/torch prefabs (and any leftover env) get their
            // point/spot shadows switched off.
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsInactive.Include))
            {
                if (l.type != LightType.Directional && l.shadows != LightShadows.None)
                {
                    l.shadows = LightShadows.None;
                }
            }

            // --- Camera (skip if part of a Player rig) -------------
            var cam = Camera.main;
            if (cam != null && cam.GetComponentInParent<FirstPersonController>() == null)
            {
                cam.transform.position = new Vector3(0f, 3f, -12f);
                cam.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
                cam.clearFlags = CameraClearFlags.Skybox;
            }

            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = root;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }

            string playerHint = Object.FindAnyObjectByType<FirstPersonController>() == null
                ? " Run 'Add First-Person Player' to walk it."
                : "";
            Debug.Log($"[OutdoorSceneBuilder] Built the outdoor clearing." + playerHint +
                      " Save the scene (File > Save As > OutdoorSandbox.unity for a separate file).");
        }

        // --- helpers ---------------------------------------------------

        private static void Scatter(GameObject root, string groupName, System.Random rng, int count,
            float rMin, float rMax, float sMin, float sMax, params string[] prefabNames)
        {
            var pool = prefabNames.Select(Find).Where(g => g != null).ToArray();
            if (pool.Length == 0)
            {
                return;
            }

            var group = new GameObject(groupName);
            group.transform.SetParent(root.transform, false);

            for (int i = 0; i < count; i++)
            {
                double a = rng.NextDouble() * System.Math.PI * 2.0;
                float r = Mathf.Sqrt(Mathf.Lerp(rMin * rMin, rMax * rMax, (float)rng.NextDouble()));
                var pos = new Vector3(r * (float)System.Math.Cos(a), 0f, r * (float)System.Math.Sin(a));

                var inst = Place(pool[rng.Next(pool.Length)], group, pos,
                    Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f));
                if (inst != null)
                {
                    inst.transform.localScale *= Mathf.Lerp(sMin, sMax, (float)rng.NextDouble());
                }
            }
        }

        private static GameObject Find(string prefabName)
        {
            foreach (var guid in AssetDatabase.FindAssets(prefabName + " t:prefab"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("/" + prefabName + ".prefab"))
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
            }
            Debug.LogWarning($"[OutdoorSceneBuilder] Prefab not found by name: {prefabName}");
            return null;
        }

        private static GameObject Place(GameObject prefab, GameObject parent, Vector3 pos, Quaternion rot)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.scene);
            inst.transform.SetParent(parent.transform, true);
            inst.transform.SetPositionAndRotation(pos, rot);
            return inst;
        }

        private static Material SolidMaterial(string assetName, Color color)
        {
            var mat = LoadOrCreateMat($"Assets/Scenes/{assetName}.mat");
            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }
            return mat;
        }

        private static Material GroundMaterial()
        {
            var mat = LoadOrCreateMat("Assets/Scenes/_OutdoorGround.mat");
            var tex = FindTexture("PT_Ground_Grass_Green_01") ?? FindTexture("PT_Grass_01");
            var green = new Color(0.42f, 0.55f, 0.30f);
            var tint = tex != null ? Color.white : green;

            mat.color = tint;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", tint);
            }
            if (tex != null)
            {
                var scale = new Vector2(24f, 24f);
                mat.mainTexture = tex;
                mat.mainTextureScale = scale;
                if (mat.HasProperty("_BaseMap"))
                {
                    mat.SetTexture("_BaseMap", tex);
                    mat.SetTextureScale("_BaseMap", scale);
                }
            }
            return mat;
        }

        private static Material LoadOrCreateMat(string path)
        {
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
                // Move it onto URP/Lit once URP is the active pipeline. Never
                // downgrade to Standard here — that is what caused the magenta.
                mat.shader = urpLit;
            }
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Texture2D FindTexture(string texName)
        {
            foreach (var guid in AssetDatabase.FindAssets(texName + " t:texture2D"))
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (p.EndsWith("/" + texName + ".png") || p.EndsWith("/" + texName + ".tga") ||
                    p.EndsWith("/" + texName + ".jpg"))
                {
                    return AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                }
            }
            return null;
        }
    }
}
