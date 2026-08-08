using LegadoPeru.Characters;
using LegadoPeru.Core;
using NUnit.Framework;
using UnityEngine;

namespace LegadoPeru.Tests
{
    /// <summary>Test 1 del prompt §41: Save y Load conserva la posición (y rotación) del jugador.</summary>
    public class PlayerCharacterControllerTests
    {
        [TearDown]
        public void TearDown() => ServiceLocator.Clear();

        [Test]
        public void CaptureAndRestore_PreservesPositionAndYaw()
        {
            var go = new GameObject("TestPlayer");
            var player = go.AddComponent<PlayerCharacterController>();

            go.transform.position = new Vector3(5f, 0f, 12f);
            go.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

            var captured = player.CaptureState();

            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;

            player.RestoreState(captured);

            Assert.AreEqual(5f, go.transform.position.x, 0.001f);
            Assert.AreEqual(0f, go.transform.position.y, 0.001f);
            Assert.AreEqual(12f, go.transform.position.z, 0.001f);
            Assert.AreEqual(90f, go.transform.eulerAngles.y, 0.001f);

            Object.DestroyImmediate(go);
        }
    }
}
