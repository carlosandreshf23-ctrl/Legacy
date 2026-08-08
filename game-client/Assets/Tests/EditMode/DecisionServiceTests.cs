using LegadoPeru.Calendar;
using LegadoPeru.Core;
using LegadoPeru.Narrative;
using NUnit.Framework;

namespace LegadoPeru.Tests
{
    /// <summary>Test 4 del prompt §41: la decisión persiste. Prueba también el caso de referencia TestDecision_Honesty.</summary>
    public class DecisionServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            var calendar = new GameCalendarSystem();
            calendar.Initialize(WorldDate.Default, minutesPerRealSecond: 4f);
            ServiceLocator.Register(calendar);
        }

        [TearDown]
        public void TearDown() => ServiceLocator.Clear();

        [Test]
        public void RecordDecision_IsRetrievableByDecisionId()
        {
            var service = new DecisionService();
            service.RecordDecision("TestDecision_Honesty", "SAL_MATEO_001", "Prototype_Path", "TRUE");

            bool found = service.TryGetChoice("TestDecision_Honesty", out var choice);

            Assert.IsTrue(found);
            Assert.AreEqual("TRUE", choice);
        }

        [Test]
        public void CaptureAndRestore_PreservesDecisionsAcrossInstances()
        {
            var original = new DecisionService();
            original.RecordDecision("TestDecision_Honesty", "SAL_MATEO_001", "Prototype_Path", "FALSE");

            var captured = original.CaptureState();

            var restored = new DecisionService();
            restored.RestoreState(captured);

            Assert.IsTrue(restored.TryGetChoice("TestDecision_Honesty", out var choice));
            Assert.AreEqual("FALSE", choice);
        }

        [Test]
        public void RecordDecision_StampsCurrentWorldDate()
        {
            var calendar = ServiceLocator.Get<GameCalendarSystem>();
            calendar.AdvanceMinutes(60);

            var service = new DecisionService();
            service.RecordDecision("TestDecision_Honesty", "SAL_MATEO_001", "Prototype_Path", "TRUE");

            var record = service.Records[0];
            Assert.AreEqual(calendar.CurrentDate.hour, record.gameHour);
            Assert.AreEqual(calendar.CurrentDate.year, record.gameYear);
        }
    }
}
