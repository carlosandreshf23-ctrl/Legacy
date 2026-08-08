using LegadoPeru.Calendar;
using NUnit.Framework;

namespace LegadoPeru.Tests
{
    /// <summary>Test 5 del prompt §41: GameDate persiste tras Save/Load (aquí: Capture/Restore).</summary>
    public class GameCalendarSystemTests
    {
        [Test]
        public void CaptureAndRestore_PreservesExactDate()
        {
            var calendar = new GameCalendarSystem();
            calendar.Initialize(new WorldDate(1820, 9, 1, 8, 0), minutesPerRealSecond: 4f);
            calendar.AdvanceMinutes(125); // +2h05m

            var captured = calendar.CaptureState();

            var restored = new GameCalendarSystem();
            restored.Initialize(WorldDate.Default, minutesPerRealSecond: 4f);
            restored.RestoreState(captured);

            Assert.AreEqual(captured, restored.CurrentDate);
            Assert.AreEqual(10, restored.CurrentDate.hour);
            Assert.AreEqual(5, restored.CurrentDate.minute);
        }

        [Test]
        public void AddMinutes_RollsOverDayAndMonthCorrectly()
        {
            // 31 de agosto 1820, 23:50 + 20 minutos -> 1 de septiembre 1820, 00:10
            var date = new WorldDate(1820, 8, 31, 23, 50);
            var result = date.AddMinutes(20);

            Assert.AreEqual(1820, result.year);
            Assert.AreEqual(9, result.month);
            Assert.AreEqual(1, result.day);
            Assert.AreEqual(0, result.hour);
            Assert.AreEqual(10, result.minute);
        }

        [Test]
        public void AddMinutes_RollsOverYearCorrectly()
        {
            var date = new WorldDate(1820, 12, 31, 23, 0);
            var result = date.AddMinutes(120);

            Assert.AreEqual(1821, result.year);
            Assert.AreEqual(1, result.month);
            Assert.AreEqual(1, result.day);
            Assert.AreEqual(1, result.hour);
        }

        [Test]
        public void AdvanceMinutes_FiresOnDateChangedAndOnHourChanged()
        {
            var calendar = new GameCalendarSystem();
            calendar.Initialize(new WorldDate(1820, 9, 1, 7, 55), minutesPerRealSecond: 4f);

            WorldDate? dateChanged = null;
            int? hourChanged = null;
            calendar.OnDateChanged += d => dateChanged = d;
            calendar.OnHourChanged += h => hourChanged = h;

            calendar.AdvanceMinutes(10); // cruza a las 08:05

            Assert.IsTrue(dateChanged.HasValue);
            Assert.AreEqual(8, hourChanged);
        }
    }
}
