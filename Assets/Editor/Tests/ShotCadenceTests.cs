using ArcadeShooter.Combat;
using NUnit.Framework;

namespace ArcadeShooter.Editor.Tests
{
    // The rifle fires 8 rounds per second: a 0.125 s interval with half an interval of jitter tolerance.
    public sealed class ShotCadenceTests
    {
        private const float Interval = 0.125f;
        private const float Tolerance = 0.0625f;

        [Test]
        public void JitteredArrivalsOfAnEightRoundStreamAreAllAccepted()
        {
            var cadence = new ShotCadence();
            // Sent 0.125 s apart; network jitter makes the gaps 0.135, 0.105, 0.15 and 0.10 s.
            foreach (float arrival in new[] { 0f, 0.135f, 0.24f, 0.39f, 0.49f })
                Assert.That(cadence.TryAccept(arrival, Interval, Tolerance), Is.True, "arrival at " + arrival);
        }

        [Test]
        public void SecondShotAtTheSameInstantIsRejected()
        {
            var cadence = new ShotCadence();
            Assert.That(cadence.TryAccept(1f, Interval, Tolerance), Is.True);
            Assert.That(cadence.TryAccept(1f, Interval, Tolerance), Is.False);
        }

        [Test]
        public void ShotEarlierThanTheToleranceIsRejected()
        {
            var cadence = new ShotCadence();
            cadence.TryAccept(0f, Interval, Tolerance);
            Assert.That(cadence.TryAccept(0.05f, Interval, Tolerance), Is.False);
        }

        [Test]
        public void SpammingRequestsCannotExceedTheRifleRate()
        {
            var cadence = new ShotCadence();
            int accepted = 0;
            for (int i = 0; i <= 100; i++)
                if (cadence.TryAccept(i * 0.01f, Interval, Tolerance)) accepted++;
            // One second of requests every 10 ms: 8 shots plus at most one early shot.
            Assert.That(accepted, Is.LessThanOrEqualTo(9));
        }

        [Test]
        public void APauseDoesNotBankExtraShots()
        {
            var cadence = new ShotCadence();
            cadence.TryAccept(0f, Interval, Tolerance);
            Assert.That(cadence.TryAccept(5f, Interval, Tolerance), Is.True);
            Assert.That(cadence.TryAccept(5f, Interval, Tolerance), Is.False);
        }
    }
}
