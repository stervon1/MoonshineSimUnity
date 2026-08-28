using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MoonshineSim.Core;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MoonshineSim.EditorTools
{
    /// <summary>
    /// Keeps <c>Assets/Resources/BuildInfo.json</c> current so the on-screen
    /// <see cref="VersionHud"/> always shows an accurate stamp.
    ///
    ///  • After every script recompile (<see cref="DidReloadScriptsAttribute"/>):
    ///    if any <c>Assets/**/*.cs</c> changed since the last stamp, bump
    ///    <see cref="BuildVersion.build"/> and refresh commit / branch / time.
    ///  • Before every player build (<see cref="IPreprocessBuildWithReport"/>):
    ///    always bump and stamp, and mark it a player build.
    ///
    /// The stamp is a Resources JSON asset, not a generated <c>.cs</c> file, so
    /// rewriting it never kicks off another compile.
    ///
    /// Menu: Tools &gt; White Lightning &gt; Version
    /// </summary>
    [InitializeOnLoad]
    public static class BuildVersionStamper
    {
        private const string JsonPath = "Assets/Resources/BuildInfo.json";

        static BuildVersionStamper()
        {
            // First domain load of a fresh checkout: make sure the file exists.
            if (!File.Exists(JsonPath))
            {
                EditorApplication.delayCall += () => Stamp(force: true, playerBuild: false);
            }
        }

        [DidReloadScripts]
        private static void OnScriptsReloaded() => Stamp(force: false, playerBuild: false);

        [MenuItem("Tools/White Lightning/Version/Bump Build Number", priority = 40)]
        private static void BumpMenu()
        {
            Stamp(force: true, playerBuild: false);
            Debug.Log($"[BuildVersionStamper] Bumped to {Read()?.Short}");
        }

        [MenuItem("Tools/White Lightning/Version/Print Current", priority = 41)]
        private static void PrintMenu()
        {
            var v = Read();
            Debug.Log(v == null ? "[BuildVersionStamper] No stamp yet." : $"[BuildVersionStamper]\n{v.Long}\nsourceHash={v.sourceHash}");
        }

        // --- core --------------------------------------------------------

        /// <param name="force">Bump the build number even if sources are unchanged.</param>
        /// <param name="playerBuild">Mark the stamp as produced by a player build.</param>
        private static void Stamp(bool force, bool playerBuild)
        {
            try
            {
                var current = Read() ?? new BuildVersion();
                string hash = ComputeSourceHash();

                bool sourcesChanged = hash != current.sourceHash;
                if (!force && !sourcesChanged)
                {
                    return; // nothing recompiled that we care about
                }

                var next = new BuildVersion
                {
                    version    = string.IsNullOrEmpty(PlayerSettings.bundleVersion) ? "0.0" : PlayerSettings.bundleVersion,
                    build      = current.build + 1,
                    commit     = Git("rev-parse --short HEAD"),
                    branch     = Git("rev-parse --abbrev-ref HEAD"),
                    builtAtUtc = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
                    sourceHash = hash,
                    playerBuild = playerBuild,
                };

                Directory.CreateDirectory(Path.GetDirectoryName(JsonPath));
                File.WriteAllText(JsonPath, JsonUtility.ToJson(next, prettyPrint: true));
                AssetDatabase.ImportAsset(JsonPath, ImportAssetOptions.ForceUpdate);
                BuildVersion.InvalidateCache();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BuildVersionStamper] Skipped stamp: {e.Message}");
            }
        }

        private static BuildVersion Read()
        {
            try
            {
                if (!File.Exists(JsonPath)) return null;
                return JsonUtility.FromJson<BuildVersion>(File.ReadAllText(JsonPath));
            }
            catch { return null; }
        }

        /// <summary>Fingerprint every <c>Assets/**/*.cs</c> by path + size + last-write time.</summary>
        private static string ComputeSourceHash()
        {
            var sb = new StringBuilder();
            var root = Directory.GetCurrentDirectory();
            var files = Directory.EnumerateFiles(Path.Combine(root, "Assets"), "*.cs", SearchOption.AllDirectories)
                                 .Select(p => p.Replace('\\', '/'))
                                 .OrderBy(p => p, StringComparer.Ordinal);

            foreach (var f in files)
            {
                var fi = new FileInfo(f);
                sb.Append(f).Append('|').Append(fi.Length).Append('|').Append(fi.LastWriteTimeUtc.Ticks).Append('\n');
            }

            using var md5 = MD5.Create();
            byte[] digest = md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            return BitConverter.ToString(digest).Replace("-", "").ToLowerInvariant().Substring(0, 12);
        }

        private static string Git(string args)
        {
            try
            {
                var psi = new ProcessStartInfo("git", args)
                {
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                using var p = Process.Start(psi);
                if (p == null) return "";
                string outText = p.StandardOutput.ReadToEnd().Trim();
                if (!p.WaitForExit(2000)) { try { p.Kill(); } catch { /* ignore */ } return ""; }
                return p.ExitCode == 0 ? outText : "";
            }
            catch { return ""; }
        }

        // --- player build hook ----------------------------------------

        private sealed class Preprocess : IPreprocessBuildWithReport
        {
            public int callbackOrder => -1000;
            public void OnPreprocessBuild(BuildReport report) => Stamp(force: true, playerBuild: true);
        }
    }
}
