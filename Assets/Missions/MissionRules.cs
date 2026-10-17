using System;

namespace ArcadeShooter.Missions
{
    // Small deterministic rules shared by host progression and focused tests.
    public static class MissionRules
    {
        public static bool IsComplete(uint mask, int index, int count) =>
            index >= 0 && index < count && index < 32 && (mask & (1u << index)) != 0;

        public static bool TryComplete(uint mask, int index, int count, out uint updated)
        {
            updated = mask;
            if (index < 0 || index >= count || index >= 32 || IsComplete(mask, index, count)) return false;
            updated |= 1u << index;
            return true;
        }

        public static int CountCompleted(uint mask, int count)
        {
            int total = 0;
            for (int i = 0; i < Math.Min(32, count); i++) if (IsComplete(mask, i, count)) total++;
            return total;
        }

        public static bool AllLivingInside(int living, int inside) => living > 0 && inside == living;
    }

    // Cancel, rather than pause, when a required player leaves. A deadline uses server time.
    public sealed class ExtractionCountdown
    {
        public double Deadline { get; private set; } = -1;
        private int requiredPlayers = -1;
        public bool Running => Deadline >= 0;
        public bool Tick(double now, float duration, int living, int inside)
        {
            if (!MissionRules.AllLivingInside(living, inside)) { Reset(); return false; }
            if (!Running || living != requiredPlayers)
            {
                Deadline = now + Math.Max(0, duration);
                requiredPlayers = living;
            }
            return now >= Deadline;
        }
        public void Reset()
        {
            Deadline = -1;
            requiredPlayers = -1;
        }
    }
}
