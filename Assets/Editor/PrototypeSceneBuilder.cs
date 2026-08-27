using System.IO;
using System.Linq;
using MoonshineSim.Core;
using MoonshineSim.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace MoonshineSim.EditorTools
{
    /// <summary>
    /// Generates the base prototype scene: `GameState`, the `Still` object with
    /// `StillRunController`, and an EventSystem. No screen UI — the environment,
    /// first-person player, still-interaction rig, workshop stations and the
    /// clipboard are layered on by the other builders (or all at once via
    /// `Rebuild Prototype`).
    ///
    /// Menu: Tools > White Lightning > Steps > Build Prototype Scene
    /// </summary>
    public static class PrototypeSceneBuilder
    {
        public const string ScenePath = "Assets/Scenes/Prototype.unity";

        [MenuItem("Tools/White Lightning/Steps/Build Prototype Scene")]
        public static void BuildPrototypeScene() => Build(silent: false);

        /// <returns>false if the user cancelled (e.g. at the save-changes prompt).</returns>
        public static bool Build(bool silent)
        {
            if (!silent && File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("Prototype scene exists",
                    $"{ScenePath} already exists. Replace it?", "Replace", "Cancel"))
            {
                return false;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return false;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            new GameObject("GameState").AddComponent<GameState>();

            var stillGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stillGO.name = "Still";
            stillGO.transform.position = new Vector3(0f, 0.9f, 0f);
            stillGO.transform.localScale = new Vector3(0.6f, 1f, 0.6f);
            stillGO.AddComponent<StillRunController>();

            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                Debug.LogError($"[PrototypeSceneBuilder] Failed to save scene to {ScenePath}.");
                return false;
            }

            AddSceneToBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Debug.Log($"[PrototypeSceneBuilder] Built base {ScenePath}. Run the other builders (or Rebuild Prototype).");
            return true;
        }

        private static void AddSceneToBuildSettings(string path)
        {
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(s => s.path == path)) return;
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
