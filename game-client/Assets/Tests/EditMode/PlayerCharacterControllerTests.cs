using LegadoPeru.Characters;
using LegadoPeru.Core;
using NUnit.Framework;
using UnityEngine;

namespace LegadoPeru.Tests
{
    /// <summary>
    /// Test 1 del prompt Fase 1 §41: Save y Load conserva la posición del jugador.
    /// Adaptado en Fase 2 al modelo 2D (posición X/Y/Z + dirección de mirada en vez de
    /// rotación de transform, ver PlayerCharacterController.cs).
    /// </summary>
    public class PlayerCharacterControllerTests
    {
        [TearDown]
        public void TearDown() => ServiceLocator.Clear();

        [Test]
        public void CaptureAndRestore_PreservesPosition()
        {
            var go = new GameObject("TestPlayer");
            var player = go.AddComponent<PlayerCharacterController>();

            go.transform.position = new Vector3(5f, 12f, 0f);

            var captured = player.CaptureState();

            go.transform.position = Vector3.zero;

            player.RestoreState(captured);

            Assert.AreEqual(5f, go.transform.position.x, 0.001f);
            Assert.AreEqual(12f, go.transform.position.y, 0.001f);
            Assert.AreEqual(0f, go.transform.position.z, 0.001f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void CaptureState_DefaultsFacingToDown()
        {
            var go = new GameObject("TestPlayer2");
            var player = go.AddComponent<PlayerCharacterController>();

            var captured = player.CaptureState();

            Assert.AreEqual("down", captured.facing);

            Object.DestroyImmediate(go);
        }
    }
}
