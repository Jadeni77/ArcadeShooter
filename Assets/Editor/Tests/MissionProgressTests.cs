using ArcadeShooter.Missions;
using NUnit.Framework;

namespace ArcadeShooter.Editor.Tests
{
    public sealed class MissionProgressTests
    {
        [Test]
        public void ObjectiveCompletesOnceAndTheCountFollowsTheMask()
        {
            uint mask = 0;
            Assert.That(MissionRules.TryComplete(mask, 0, 3, out mask), Is.True);
            Assert.That(mask, Is.EqualTo(1u));
            Assert.That(MissionRules.TryComplete(mask, 0, 3, out uint duplicate), Is.False);
            Assert.That(duplicate, Is.EqualTo(1u));
            Assert.That(MissionRules.TryComplete(mask, 2, 3, out mask), Is.True);
            Assert.That(MissionRules.CountCompleted(mask, 3), Is.EqualTo(2));
            Assert.That(MissionRules.TryComplete(mask, 1, 3, out mask), Is.True);
            Assert.That(MissionRules.CountCompleted(mask, 3), Is.EqualTo(3));
        }

        [Test]
        public void OutOfRangeObjectiveIsRejected()
        {
            Assert.That(MissionRules.TryComplete(1u, 3, 3, out uint unchanged), Is.False);
            Assert.That(unchanged, Is.EqualTo(1u));
            Assert.That(MissionRules.TryComplete(1u, -1, 3, out _), Is.False);
        }

        [Test]
        public void ExtractionNeedsALivingPlayerAndEveryLivingPlayerInside()
        {
            Assert.That(MissionRules.AllLivingInside(0, 0), Is.False);
            Assert.That(MissionRules.AllLivingInside(2, 1), Is.False);
            Assert.That(MissionRules.AllLivingInside(2, 2), Is.True);
        }

        [Test]
        public void CountdownStartsWhenTheTeamIsInsideAndWaitsTheFullDuration()
        {
            var countdown = new ExtractionCountdown();
            Assert.That(countdown.Tick(20, 10, 2, 1), Is.False);
            Assert.That(countdown.Running, Is.False);
            Assert.That(countdown.Tick(21, 10, 2, 2), Is.False);
            Assert.That(countdown.Deadline, Is.EqualTo(31));
            Assert.That(countdown.Tick(30.9, 10, 2, 2), Is.False);
            Assert.That(countdown.Tick(31, 10, 2, 2), Is.True);
        }

        [Test]
        public void LeavingTheAreaCancelsTheCountdownAndReentryNeedsAFreshOne()
        {
            var countdown = new ExtractionCountdown();
            countdown.Tick(21, 10, 2, 2);
            Assert.That(countdown.Tick(25, 10, 2, 1), Is.False);
            Assert.That(countdown.Running, Is.False);
            Assert.That(countdown.Tick(26, 10, 2, 2), Is.False);
            Assert.That(countdown.Deadline, Is.EqualTo(36));
        }

        [Test]
        public void TeammateLostInsideTheAreaDoesNotRestartTheCountdown()
        {
            var countdown = new ExtractionCountdown();
            countdown.Tick(40, 10, 2, 2);
            // One of two players inside dies or disconnects; the survivor is still inside.
            Assert.That(countdown.Tick(45, 10, 1, 1), Is.False);
            Assert.That(countdown.Deadline, Is.EqualTo(50));
            Assert.That(countdown.Tick(50, 10, 1, 1), Is.True);
        }

        [Test]
        public void LargerRosterRestartsTheCountdown()
        {
            var countdown = new ExtractionCountdown();
            countdown.Tick(40, 10, 1, 1);
            Assert.That(countdown.Tick(45, 10, 2, 2), Is.False);
            Assert.That(countdown.Deadline, Is.EqualTo(55));
        }

        [Test]
        public void ResetClearsARunningCountdown()
        {
            var countdown = new ExtractionCountdown();
            countdown.Tick(10, 10, 1, 1);
            countdown.Reset();
            Assert.That(countdown.Running, Is.False);
        }
    }
}
