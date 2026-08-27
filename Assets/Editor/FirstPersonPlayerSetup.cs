using System.Linq;
using MoonshineSim.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoonshineSim.EditorTools
{
    /// <summary>
    /// Drops a first-person player rig into the active scene: a
    /// CharacterController capsule with <see cref="FirstPersonController"/>,
    /// reusing the existing Main Camera as the eye so Camera.main stays valid.
    ///
    /// Menu: Tools > White Lightning > Add First-Person Player
    /// </summary>
    public static class FirstPersonPlayerSetup
    {
        [MenuItem("Tools/White Lightning/Steps/Add First-Person Player")]
        public static void AddPlayer()
        {
            Scene scene = SceneManager.GetActiveScene();

            var existing = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "Player");
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Player exists",
                        "A 'Player' object already exists in this scene. Replace it?", "Replace", "Cancel"))
                {
                    return;
                }
                Object.DestroyImmediate(existing);
            }

            var player = new GameObject("Player");
            player.transform.position = new Vector3(0f, 1.1f, -3f);

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            var fpc = player.AddComponent<FirstPersonController>();

            // Reuse the current Main Camera as the eye if there is one.
            var cam = Camera.main;
            if (cam == null)
            {
                var camGO = new GameObject("Camera") { tag = "MainCamera" };
                cam = camGO.AddComponent<Camera>();
                camGO.AddComponent<AudioListener>();
            }
            cam.transform.SetParent(player.transform, worldPositionStays: false);
            cam.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            cam.transform.localRotation = Quaternion.identity;

            var so = new SerializedObject(fpc);
            so.FindProperty("cameraPivot").objectReferenceValue = cam.transform;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = player;
            Debug.Log("[FirstPersonPlayerSetup] Player added at " + player.transform.position +
                      ". Press Play — mouse look, WASD move, Shift sprint, Space jump, Esc frees the cursor.");
        }
    }
}
