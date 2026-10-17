using System;
using System.Linq;
using ArcadeShooter.Missions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ArcadeShooter.Editor
{
    public static class MissionRulesChecks
    {
        [MenuItem("ArcadeShooter/Validation/Check Mission Progress and Extraction")]
        public static void Run()
        {
            uint mask = 0;
            Expect(MissionRules.TryComplete(mask, 0, 3, out mask) && mask == 1, "First objective must complete.");
            Expect(!MissionRules.TryComplete(mask, 0, 3, out uint duplicate) && duplicate == mask,
                "An objective must not complete twice.");
            Expect(!MissionRules.TryComplete(mask, 3, 3, out uint invalid) && invalid == mask,
                "Out-of-range objectives must be rejected.");
            Expect(MissionRules.TryComplete(mask, 2, 3, out mask) && MissionRules.CountCompleted(mask, 3) == 2,
                "Objective count must match the synchronized bitmask.");
            Expect(MissionRules.TryComplete(mask, 1, 3, out mask) && MissionRules.CountCompleted(mask, 3) == 3,
                "All three objectives must complete exactly once.");
            Expect(!MissionRules.AllLivingInside(0, 0), "No living players cannot extract.");
            Expect(!MissionRules.AllLivingInside(2, 1) && MissionRules.AllLivingInside(2, 2),
                "Every living player must be inside.");
            var countdown = new ExtractionCountdown();
            Expect(!countdown.Tick(20, 10, 2, 1) && !countdown.Running, "Countdown must stay idle until the team enters.");
            Expect(!countdown.Tick(21, 10, 2, 2) && countdown.Deadline == 31,
                "Countdown must start on the host time.");
            Expect(!countdown.Tick(30.9, 10, 2, 2) && countdown.Tick(31, 10, 2, 2),
                "Extraction must wait for the full duration.");
            Expect(!countdown.Tick(31.1, 10, 2, 1) && !countdown.Running,
                "Leaving must cancel an already-running countdown.");
            Expect(!countdown.Tick(35, 10, 1, 1) && countdown.Deadline == 45 && countdown.Tick(45, 10, 1, 1),
                "A dead or disconnected player must stop being required; a fresh countdown is needed.");
            var rosterChange = new ExtractionCountdown();
            Expect(!rosterChange.Tick(40, 10, 2, 2) && !rosterChange.Tick(45, 10, 1, 1) &&
                   rosterChange.Deadline == 55 && !rosterChange.Tick(50, 10, 1, 1) &&
                   rosterChange.Tick(55, 10, 1, 1),
                "Changing the required player count during extraction must restart the countdown.");
            countdown.Reset();
            Expect(!countdown.Running, "Replay must reset extraction time.");
            Debug.Log("Mission progress/extraction checks passed (14 checks).");
        }
        private static void Expect(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

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
