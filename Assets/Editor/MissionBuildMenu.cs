using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ArcadeShooter.Editor
{
    public static class MissionBuildMenu
    {
        [MenuItem("ArcadeShooter/Validation/Build macOS Development Player")]
        public static void BuildDevelopmentPlayer()
        {
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                locationPathName = "Builds/M6-macOS.app",
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("macOS Development Player build failed: " + report.summary.result);
            Debug.Log("macOS Development Player build passed.");
        }
    }
}
