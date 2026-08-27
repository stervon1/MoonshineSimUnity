using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoonshineSim.EditorTools
{
    /// <summary>
    /// One-click rebuild of the whole prototype. Individual steps stay under
    /// Tools &gt; White Lightning &gt; Steps for iterating on one thing.
    /// </summary>
    public static class MoonshinePipeline
    {
        [MenuItem("Tools/White Lightning/Rebuild Prototype", priority = 0)]
        public static void RebuildPrototype()
        {
            if (!EditorUtility.DisplayDialog("Rebuild Prototype",
                    "Regenerates Assets/Scenes/Prototype.unity from scratch (URP → scene → outdoor → " +
                    "player → still rig → stations → clipboard). Unsaved changes to that scene are lost. Continue?",
                    "Rebuild", "Cancel"))
            {
                return;
            }

            UrpSetup.SetUpUrp();

            if (!PrototypeSceneBuilder.Build(silent: true))
            {
                Debug.LogWarning("[MoonshinePipeline] Aborted at the base-scene step.");
                return;
            }

            // Each step is isolated — a failure in one is logged and the rest
            // still run, so a bad prop reference can't wipe the clipboard etc.
            Step("Outdoor",        OutdoorSceneBuilder.BuildOutdoorSandbox);
            Step("Player",         FirstPersonPlayerSetup.AddPlayer);
            Step("Still rig",      StillInteractionRigSetup.AddRig);
            Step("Stations",       WorkshopStationsBuilder.AddStations);
            Step("Clipboard",      ClipboardSetup.AddClipboard);

            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[MoonshinePipeline] Prototype rebuilt and saved. Press Play.");
        }

        private static void Step(string label, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Debug.LogError($"[MoonshinePipeline] '{label}' step failed — continuing.\n{e}");
            }
        }
    }
}
