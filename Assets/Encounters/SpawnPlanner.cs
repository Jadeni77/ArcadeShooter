using System.Collections.Generic;
using UnityEngine;

namespace ArcadeShooter.Encounters
{
    // Chooses wave spawn positions. Each marker is tried first; when a player or another body blocks
    // it, nearby ground around the same marker is used instead. If a marker has no usable ground at
    // all, its enemy takes free space around another marker, so one blocked marker cannot stall a wave.
    public static class SpawnPlanner
    {
        // Returns true and the ground position when a candidate near the marker is open and reachable.
        public delegate bool Resolver(Vector3 marker, Vector3 candidate, out Vector3 ground);

        private static readonly float[] FallbackRadii = { 1.25f, 2.5f };
        private const int FallbackDirections = 8;

        public static bool TryPlan(int count, IReadOnlyList<Vector3> markers, int firstMarker,
            IReadOnlyList<Vector3> players, float minPlayerDistance, float minSeparation,
            Resolver resolve, List<Vector3> result)
        {
            result.Clear();
            if (count <= 0) return true;
            if (markers == null || markers.Count == 0 || resolve == null) return false;
            int start = ((firstMarker % markers.Count) + markers.Count) % markers.Count;

            // One spawn per marker first, then any remaining spawns share space around other markers.
            for (int pass = 0; pass < 2 && result.Count < count; pass++)
                for (int k = 0; k < markers.Count && result.Count < count; k++)
                    AddNear(markers[(start + k) % markers.Count], pass == 0 ? 1 : count - result.Count,
                        players, minPlayerDistance, minSeparation, resolve, result);
            return result.Count == count;
        }

        private static void AddNear(Vector3 marker, int wanted, IReadOnlyList<Vector3> players,
            float minPlayerDistance, float minSeparation, Resolver resolve, List<Vector3> result)
        {
            int added = 0;
            for (int i = 0; i < 1 + FallbackRadii.Length * FallbackDirections && added < wanted; i++)
            {
                if (!resolve(marker, Candidate(marker, i), out Vector3 ground) ||
                    !FarFrom(players, ground, minPlayerDistance) || !FarFrom(result, ground, minSeparation)) continue;
                result.Add(ground);
                added++;
            }
        }

        // Candidate 0 is the marker; the rest ring it at increasing radii.
        private static Vector3 Candidate(Vector3 marker, int index)
        {
            if (index == 0) return marker;
            float radius = FallbackRadii[(index - 1) / FallbackDirections];
            float angle = (index - 1) % FallbackDirections * Mathf.PI * 2f / FallbackDirections;
            return marker + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
        }

        private static bool FarFrom(IReadOnlyList<Vector3> points, Vector3 position, float distance)
        {
            if (points == null) return true;
            foreach (Vector3 point in points)
            {
                float x = point.x - position.x, z = point.z - position.z;
                if (x * x + z * z < distance * distance) return false;
            }
            return true;
        }
    }
}
