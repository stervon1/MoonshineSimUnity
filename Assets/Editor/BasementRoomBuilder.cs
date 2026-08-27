using System.Collections.Generic;
using System.Linq;
using MoonshineSim.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoonshineSim.EditorTools
{
    /// <summary>
    /// Blocks in a basement still room from the BrokenVector LowPolyDungeon
    /// kit (2 m modular grid) into the currently open scene.
    ///
    /// Idempotent: removes any previous "BasementRoom" root (and the outdoor
    /// "OutdoorEnvironment" root, since the two environments share world
    /// origin) before rebuilding. Leaves GameState / Canvas / the existing
    /// "Still" object and its wiring alone, and does not touch the camera when
    /// it is part of a first-person Player rig.
    ///
    /// This is a greybox starting point, not final composition: expect to
    /// nudge modular pieces flush with vertex snapping (hold V) and scatter
    /// clutter by hand afterwards.
    ///
    /// Menu: Tools > White Lightning > Build Basement Room
    /// </summary>
    public static class BasementRoomBuilder
    {
        private const string RootName = "BasementRoom";
        private const float Tile = 2f;             // LowPolyDungeon grid size
        private const string P = "Assets/BrokenVector/LowPolyDungeon/Prefabs/";

        // Room size in tiles (interior). 9 x 7 -> 18 m x 14 m.
        private const int WidthTiles = 9;
        private const int DepthTiles = 7;

        // Nudge these in one place if the kit's pivots need it.
        private const float WallY = 0f;
        private const float WallInwardOffset = 0f;

        [MenuItem("Tools/White Lightning/Steps/Build Basement Room")]
        public static void BuildBasementRoom()
        {
            Scene scene = SceneManager.GetActiveScene();
            foreach (var name in new[] { RootName, "OutdoorEnvironment" })
            {
                var ex = scene.GetRootGameObjects().FirstOrDefault(g => g.name == name);
                if (ex != null)
                {
                    Object.DestroyImmediate(ex);
                }
            }

            var root = new GameObject(RootName);
            var rng = new System.Random(20260827);

            float halfW = WidthTiles * Tile * 0.5f;
            float halfD = DepthTiles * Tile * 0.5f;

            // --- Floor -----------------------------------------------------
            var floorVariants = LoadMany(
                "Tiles/Basement_Var1", "Tiles/Basement_Var2",
                "Tiles/Basement_Var3", "Tiles/Basement_Var4");
            var floorRoot = Child(root, "Floor");
            for (int ix = 0; ix < WidthTiles; ix++)
            for (int iz = 0; iz < DepthTiles; iz++)
            {
                var pos = new Vector3(ix * Tile - halfW + Tile * 0.5f, 0f, iz * Tile - halfD + Tile * 0.5f);
                Place(Pick(floorVariants, rng), floorRoot, pos, Quaternion.Euler(0f, 90f * rng.Next(4), 0f));
            }

            // --- Walls (door gap on the south edge, middle tile) --------
            var wallVariants = LoadMany(
                "Tiles/Basement_Wall_Var1", "Tiles/Basement_Wall_Var2", "Tiles/Basement_Wall_Var3");
            var wallRoot = Child(root, "Walls");
            int doorIndex = WidthTiles / 2;

            for (int ix = 0; ix < WidthTiles; ix++)
            {
                float x = ix * Tile - halfW + Tile * 0.5f;
                Place(Pick(wallVariants, rng), wallRoot,
                    new Vector3(x, WallY, halfD - WallInwardOffset), Quaternion.Euler(0f, 180f, 0f));
                if (ix != doorIndex)
                {
                    Place(Pick(wallVariants, rng), wallRoot,
                        new Vector3(x, WallY, -halfD + WallInwardOffset), Quaternion.identity);
                }
            }

            for (int iz = 0; iz < DepthTiles; iz++)
            {
                float z = iz * Tile - halfD + Tile * 0.5f;
                Place(Pick(wallVariants, rng), wallRoot,
                    new Vector3(-halfW + WallInwardOffset, WallY, z), Quaternion.Euler(0f, 90f, 0f));
                Place(Pick(wallVariants, rng), wallRoot,
                    new Vector3(halfW - WallInwardOffset, WallY, z), Quaternion.Euler(0f, 270f, 0f));
            }

            var doorX = doorIndex * Tile - halfW + Tile * 0.5f;
            Place(Load("Tiles/Door_Basement_Left"), wallRoot,
                new Vector3(doorX, WallY, -halfD + WallInwardOffset), Quaternion.identity);

            // --- Pillars (corners inset one tile + long-wall midpoints) --
            var pillar = Load("Tiles/Pillar");
            var pillarRoot = Child(root, "Pillars");
            float px = halfW - Tile, pz = halfD - Tile;
            foreach (var p in new[]
            {
                new Vector3(-px, 0f, pz), new Vector3(px, 0f, pz),
                new Vector3(-px, 0f, -pz), new Vector3(px, 0f, -pz),
                new Vector3(-px, 0f, 0f), new Vector3(px, 0f, 0f),
            })
            {
                Place(pillar, pillarRoot, p, Quaternion.identity);
            }

            // --- Still station (back-centre) ----------------------------
            var stationRoot = Child(root, "StillStation");
            var stationPos = new Vector3(0f, 0f, halfD - Tile * 1.25f);

            var oven = Place(Load("Furniture/Oven"), stationRoot, stationPos, Quaternion.identity);
            Place(Load("Clutter/FirewoodStack"), stationRoot, stationPos + new Vector3(1.6f, 0f, 0f), Quaternion.Euler(0f, 35f, 0f));
            Place(Load("Clutter/Cauldron_Full"), stationRoot, stationPos + new Vector3(-1.5f, 0f, 0.2f), Quaternion.identity);

            var still = Object.FindAnyObjectByType<StillRunController>();
            if (still != null && oven != null)
            {
                still.transform.position = stationPos + new Vector3(0f, 1.1f, 0f);
                still.transform.localScale = Vector3.one * 0.6f;
            }

            // --- Dressing ---------------------------------------------
            var props = Child(root, "Props");
            var barrel = LoadMany("Furniture/Barrel_Big", "Furniture/Barrel_Closed", "Furniture/Barrel_Open");
            for (int i = 0; i < 8; i++)
            {
                Place(Pick(barrel, rng), props,
                    new Vector3(-halfW + Tile * 0.9f, 0f, -halfD + Tile * 1.0f + i * 1.6f),
                    Quaternion.Euler(0f, rng.Next(360), 0f));
            }
            Place(Load("Furniture/Shelf_Wall"), props, new Vector3(halfW - 0.15f, 1.4f, 0f), Quaternion.Euler(0f, 270f, 0f));
            Place(Load("Furniture/Desk"), props, new Vector3(halfW - Tile * 1.1f, 0f, -halfD + Tile * 0.9f), Quaternion.Euler(0f, 210f, 0f));
            Place(Load("Clutter/Jug"), props, new Vector3(halfW - Tile * 1.1f, 0.75f, -halfD + Tile * 0.9f), Quaternion.identity);
            Place(Load("Clutter/Jar_Full"), props, new Vector3(halfW - Tile * 1.4f, 0.75f, -halfD + Tile * 0.9f), Quaternion.identity);
            Place(Load("Clutter/Bucket"), props, new Vector3(0.8f, 0f, -halfD + Tile * 0.7f), Quaternion.identity);

            // --- Lighting -------------------------------------------
            var lightRoot = Child(root, "Lighting");
            var torch = Load("Lamps/Torch_Wall");
            for (int iz = 1; iz < DepthTiles; iz += 2)
            {
                float z = iz * Tile - halfD + Tile * 0.5f;
                Place(torch, lightRoot, new Vector3(-halfW + 0.15f, 2.2f, z), Quaternion.Euler(0f, 90f, 0f));
                Place(torch, lightRoot, new Vector3(halfW - 0.15f, 2.2f, z), Quaternion.Euler(0f, 270f, 0f));
            }

            // Torch prefabs ship shadow-casting point lights — a dozen of them
            // blows the additional-light shadow atlas. Keep them as unshadowed
            // glow; the StillLamp below is the one shadowed point light.
            foreach (var tl in lightRoot.GetComponentsInChildren<Light>(true))
            {
                if (tl.type != LightType.Directional) tl.shadows = LightShadows.None;
            }

            var lamp = new GameObject("StillLamp");
            lamp.transform.SetParent(lightRoot.transform, false);
            lamp.transform.position = stationPos + new Vector3(0f, 2.2f, 0.5f);
            var l = lamp.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, 0.72f, 0.42f);
            l.intensity = 3.2f;
            l.range = 9f;
            l.shadows = LightShadows.Soft;

            var dir = Object.FindObjectsByType<Light>(FindObjectsInactive.Include)
                .FirstOrDefault(x => x.type == LightType.Directional);
            if (dir != null)
            {
                dir.intensity = 0.15f;
                dir.color = new Color(0.6f, 0.68f, 0.85f);
            }
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.12f, 0.12f, 0.16f);

            // --- Camera (skip if it belongs to a Player rig) ----------
            var cam = Camera.main;
            if (cam != null && cam.GetComponentInParent<FirstPersonController>() == null)
            {
                cam.transform.position = new Vector3(0f, 3.2f, -halfD - 3.5f);
                cam.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.03f, 0.03f, 0.04f);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = root;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }
            Debug.Log($"[BasementRoomBuilder] Blocked in a {WidthTiles * Tile}x{DepthTiles * Tile} m room. " +
                      "Refine with vertex snapping, then save the scene.");
        }

        // --- helpers -----------------------------------------------------

        private static GameObject Child(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            return go;
        }

        private static GameObject Load(string relPath)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(P + relPath + ".prefab");
            if (go == null)
            {
                Debug.LogWarning($"[BasementRoomBuilder] Prefab not found: {P}{relPath}.prefab");
            }
            return go;
        }

        private static List<GameObject> LoadMany(params string[] relPaths)
        {
            return relPaths.Select(Load).Where(g => g != null).ToList();
        }

        private static GameObject Pick(List<GameObject> pool, System.Random rng)
        {
            return pool.Count == 0 ? null : pool[rng.Next(pool.Count)];
        }

        private static GameObject Place(GameObject prefab, GameObject parent, Vector3 pos, Quaternion rot)
        {
            if (prefab == null)
            {
                return null;
            }
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.scene);
            inst.transform.SetParent(parent.transform, true);
            inst.transform.SetPositionAndRotation(pos, rot);
            return inst;
        }
    }
}
