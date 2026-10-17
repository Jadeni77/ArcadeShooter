using UnityEngine;

namespace ArcadeShooter.Combat
{
    // Host-side shot pacing. Network jitter can deliver a client's evenly fired shots slightly
    // bunched, so a shot may arrive up to `earlyTolerance` before its slot. Each accepted shot moves
    // the next slot a full interval past the later of now and the previous slot, so bursts and
    // spam still cannot exceed the rifle's sustained rate.
    public sealed class ShotCadence
    {
        private float nextShotAt = float.NegativeInfinity;

        public bool TryAccept(float now, float interval, float earlyTolerance)
        {
            if (now + Mathf.Max(0f, earlyTolerance) < nextShotAt) return false;
            nextShotAt = Mathf.Max(now, nextShotAt) + interval;
            return true;
        }
    }
}
