using LegadoPeru.World;
using NUnit.Framework;

namespace LegadoPeru.Tests
{
    /// <summary>Test 6 del prompt §41: la zona visitada persiste.</summary>
    public class DiscoverySystemTests
    {
        [Test]
        public void MarkVisited_ChangesStateFromUnknownToVisited()
        {
            var discovery = new DiscoverySystem();

            Assert.AreEqual(DiscoveryState.Unknown, discovery.GetState("Prototype_Coast"));

            discovery.MarkVisited("Prototype_Coast");

            Assert.AreEqual(DiscoveryState.Visited, discovery.GetState("Prototype_Coast"));
        }

        [Test]
        public void CaptureAndRestore_PreservesVisitedLocations()
        {
            var original = new DiscoverySystem();
            original.MarkVisited("Prototype_Coast");
            original.MarkVisited("Prototype_Path");

            var captured = original.CaptureState();

            var restored = new DiscoverySystem();
            restored.RestoreState(captured);

            Assert.AreEqual(DiscoveryState.Visited, restored.GetState("Prototype_Coast"));
            Assert.AreEqual(DiscoveryState.Visited, restored.GetState("Prototype_Path"));
            Assert.AreEqual(DiscoveryState.Unknown, restored.GetState("Prototype_House"));
        }
    }
}
