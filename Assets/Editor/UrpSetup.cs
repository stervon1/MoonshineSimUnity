using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MoonshineSim.EditorTools
{
    /// <summary>
    /// One-time fix. The project ships the URP package but has no pipeline
    /// asset assigned, so it renders with the Built-in pipeline and URP
    /// shaders show up magenta. This creates a URP asset + Universal Renderer
    /// under Assets/Settings/ and assigns it as the default render pipeline.
    ///
    /// After running:
    ///  1. Import each pack's bundled *_URP_*.unitypackage (Polytope,
    ///     SimpleNaturePack, Toon Gas Station).
    ///  2. Window > Rendering > Render Pipeline Converter >
    ///     "Convert Built-in to URP" > enable Material Upgrade > Initialize
    ///     And Convert  (covers BrokenVector + anything left).
    ///  3. Re-run the White Lightning scene builders so the generated
    ///     materials pick up URP/Lit.
    ///
    /// Menu: Tools > White Lightning > Set Up URP Pipeline
    /// </summary>
    public static class UrpSetup
    {
        private const string Dir = "Assets/Settings";
        private const string RendererPath = Dir + "/URP-UniversalRenderer.asset";
        private const string AssetPath = Dir + "/URP-Pipeline.asset";

        [MenuItem("Tools/White Lightning/Steps/Set Up URP Pipeline")]
        public static void SetUpUrp()
        {
            if (GraphicsSettings.defaultRenderPipeline is UniversalRenderPipelineAsset active)
            {
                Debug.Log($"[UrpSetup] URP is already the default pipeline ({AssetDatabase.GetAssetPath(active)}). " +
                          "Nothing to do.");
                return;
            }

            if (!AssetDatabase.IsValidFolder(Dir))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }

            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, RendererPath);
            }

            var urp = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(AssetPath);
            if (urp == null)
            {
                urp = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(urp, AssetPath);
            }

            GraphicsSettings.defaultRenderPipeline = urp;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[UrpSetup] Created {AssetPath} and set it as the default render pipeline. " +
                      "The scene will look broken until pack materials are upgraded — see this script's summary " +
                      "for the import + converter steps.");
        }
    }
}
