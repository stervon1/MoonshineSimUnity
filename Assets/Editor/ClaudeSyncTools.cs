using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MoonshineSim.EditorTools
{
    /// <summary>
    /// Cross-machine continuity helpers. The project syncs between machines via
    /// Unity Version Control; everything a fresh Claude session needs to pick up
    /// where another machine left off lives under <c>docs/claude/</c> so it rides
    /// along with the workspace.
    ///
    /// The actual map generation lives in <c>Tools/regen-claude-map.ps1</c> /
    /// <c>.sh</c> (one implementation per shell, runnable without Unity). This
    /// menu item just shells out to the right one so the logic isn't forked a
    /// third time.
    /// </summary>
    public static class ClaudeSyncTools
    {
        private static string RepoRoot =>
            Directory.GetParent(Application.dataPath)!.FullName;

        [MenuItem("Tools/White Lightning/Regenerate Claude Map", priority = 40)]
        public static void RegenerateClaudeMap()
        {
            var root = RepoRoot;
            string file, exe, args;

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                file = Path.Combine(root, "Tools", "regen-claude-map.ps1");
                exe  = "powershell";
                args = $"-NoProfile -ExecutionPolicy Bypass -File \"{file}\"";
            }
            else
            {
                file = Path.Combine(root, "Tools", "regen-claude-map.sh");
                exe  = "/bin/bash";
                args = $"\"{file}\"";
            }

            if (!File.Exists(file))
            {
                Debug.LogError($"[ClaudeSync] Generator script missing: {file}");
                return;
            }

            try
            {
                var psi = new ProcessStartInfo(exe, args)
                {
                    WorkingDirectory       = root,
                    UseShellExecute        = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    CreateNoWindow         = true,
                };
                using var p = Process.Start(psi);
                string stdout = p!.StandardOutput.ReadToEnd();
                string stderr = p.StandardError.ReadToEnd();
                p.WaitForExit(30_000);

                if (p.ExitCode == 0)
                {
                    Debug.Log($"[ClaudeSync] Regenerated docs/claude/project-map.md\n{stdout}");
                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogError($"[ClaudeSync] Generator exited {p.ExitCode}.\n{stdout}\n{stderr}\n" +
                                   $"Run it yourself from a terminal:\n  {exe} {args}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ClaudeSync] Could not launch the generator ({e.Message}).\n" +
                               $"Run it yourself from a terminal at the repo root:\n" +
                               $"  pwsh Tools/regen-claude-map.ps1   (Windows)\n" +
                               $"  bash Tools/regen-claude-map.sh     (macOS/Linux)");
            }
        }

        [MenuItem("Tools/White Lightning/Open Session Handoff", priority = 41)]
        public static void OpenSessionHandoff()
        {
            var path = Path.Combine(RepoRoot, "docs", "claude", "SESSION.md");
            if (File.Exists(path))
                EditorUtility.OpenWithDefaultApp(path);
            else
                Debug.LogError($"[ClaudeSync] Missing {path}");
        }
    }
}
