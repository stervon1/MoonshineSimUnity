using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Headless / CI build entry point.
///
/// Deliberately in the global namespace (not <c>MoonshineSim.Editor</c>) so the
/// command documented in CLAUDE.md keeps working verbatim:
///
///   Unity -batchmode -nographics -projectPath . -executeMethod BuildScript.Build -quit
///
/// Optional extra args (anywhere after the Unity args):
///   -buildTarget StandaloneWindows64 | StandaloneOSX | StandaloneLinux64 | ...
///   -outputPath  "Builds/Custom/WhiteLightning.exe"
///   -devBuild    (adds Development + script debugging flags)
///
/// Behaviour:
///   * Scenes come from Build Settings (enabled entries). If none are set,
///     every *.unity under Assets/Scenes/ is used. If there are still none
///     (the current state of the project), Unity cannot build a player, so
///     this instead runs a player-script compile pass and reports on that —
///     the command stays useful as a CI compile gate until scenes exist.
///   * In -batchmode the process exits 0 on success and non-zero on any
///     failure (a failure throws, which Unity turns into exit code 1).
/// </summary>
public static class BuildScript
{
    private const string DefaultProductName = "WhiteLightning";

    [MenuItem("Tools/White Lightning/Build Windows (x64)")]
    public static void BuildWindowsMenu()
    {
        Run(BuildTarget.StandaloneWindows64, null, false);
    }

    /// <summary>Entry point for <c>-executeMethod BuildScript.Build</c>.</summary>
    public static void Build()
    {
        var args = Environment.GetCommandLineArgs();

        BuildTarget target = ParseTarget(GetArg(args, "-buildTarget"))
                             ?? EditorUserBuildSettings.activeBuildTarget;

        string outputPath = GetArg(args, "-outputPath");
        bool devBuild = args.Any(a => string.Equals(a, "-devBuild", StringComparison.OrdinalIgnoreCase));

        bool ok = Run(target, outputPath, devBuild);

        // A thrown exception out of -executeMethod is what Unity reliably
        // maps to a non-zero exit code; EditorApplication.Exit(1) alone has
        // proven flaky when the build pipeline has already queued shutdown.
        if (!ok)
        {
            throw new Exception("[BuildScript] FAILED — see the errors logged above.");
        }

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    private static bool Run(BuildTarget target, string outputPath, bool devBuild)
    {
        string[] scenes = ResolveScenes();
        if (scenes.Length == 0)
        {
            Debug.LogWarning("[BuildScript] No scenes in Build Settings and none under Assets/Scenes/. " +
                             "Unity cannot build a player without a scene — running a player-script " +
                             "compile check instead.");
            return CompilePlayerScriptsOnly(target);
        }

        Debug.Log($"[BuildScript] {scenes.Length} scene(s):\n  " + string.Join("\n  ", scenes));

        if (string.IsNullOrEmpty(outputPath))
        {
            outputPath = DefaultOutputPath(target);
        }

        string dir = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = target,
            targetGroup = BuildPipeline.GetBuildTargetGroup(target),
            options = devBuild
                ? BuildOptions.Development | BuildOptions.AllowDebugging
                : BuildOptions.None,
        };

        Debug.Log($"[BuildScript] Building {target} -> {outputPath} (dev={devBuild})");

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[BuildScript] SUCCEEDED in {summary.totalTime}. " +
                      $"Output size: {summary.totalSize / (1024 * 1024)} MB at {summary.outputPath}");
            return true;
        }

        Debug.LogError($"[BuildScript] {summary.result} — {summary.totalErrors} error(s), {summary.totalWarnings} warning(s).");
        return false;
    }

    /// <summary>
    /// Compiles the runtime (player) assemblies for <paramref name="target"/>
    /// without needing a scene. Used as a CI compile gate while the project
    /// still has no scenes. Returns false if any assembly fails to compile.
    /// </summary>
    private static bool CompilePlayerScriptsOnly(BuildTarget target)
    {
        var settings = new ScriptCompilationSettings
        {
            target = target,
            group = BuildPipeline.GetBuildTargetGroup(target),
            options = ScriptCompilationOptions.None,
        };

        string outFolder = Path.Combine("Temp", "BuildScriptCompileCheck");
        Directory.CreateDirectory(outFolder);

        Debug.Log($"[BuildScript] Compiling player scripts for {target}...");
        ScriptCompilationResult result = PlayerBuildInterface.CompilePlayerScripts(settings, outFolder);

        int assemblyCount = result.assemblies == null ? 0 : result.assemblies.Count();
        if (assemblyCount == 0)
        {
            Debug.LogError("[BuildScript] Player-script compilation produced no assemblies — compile errors above.");
            return false;
        }

        Debug.Log($"[BuildScript] SUCCEEDED — {assemblyCount} player assembly(ies) compiled cleanly:\n  " +
                  string.Join("\n  ", result.assemblies));
        return true;
    }

    private static string[] ResolveScenes()
    {
        var fromSettings = EditorBuildSettings.scenes
            .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
            .Select(s => s.path)
            .ToArray();
        if (fromSettings.Length > 0)
        {
            return fromSettings;
        }

        const string scenesFolder = "Assets/Scenes";
        if (AssetDatabase.IsValidFolder(scenesFolder))
        {
            return AssetDatabase.FindAssets("t:Scene", new[] { scenesFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .OrderBy(p => p, StringComparer.Ordinal)
                .ToArray();
        }

        return Array.Empty<string>();
    }

    private static string DefaultOutputPath(BuildTarget target)
    {
        string folder = Path.Combine("Builds", target.ToString());
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return Path.Combine(folder, DefaultProductName + ".exe");
            case BuildTarget.StandaloneOSX:
                return Path.Combine(folder, DefaultProductName + ".app");
            case BuildTarget.StandaloneLinux64:
                return Path.Combine(folder, DefaultProductName + ".x86_64");
            default:
                return Path.Combine(folder, DefaultProductName);
        }
    }

    private static BuildTarget? ParseTarget(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return null;
        }

        if (Enum.TryParse(raw, ignoreCase: true, out BuildTarget parsed) && Enum.IsDefined(typeof(BuildTarget), parsed))
        {
            return parsed;
        }

        Debug.LogWarning($"[BuildScript] Unrecognised -buildTarget '{raw}'. Falling back to the active target.");
        return null;
    }

    private static string GetArg(IReadOnlyList<string> args, string name)
    {
        for (int i = 0; i < args.Count - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
