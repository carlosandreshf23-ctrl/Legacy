using LegadoPeru.Core;
using UnityEngine;

namespace LegadoPeru.World
{
    /// <summary>Volumen de trigger que marca la entrada del jugador a una LocationDefinition.</summary>
    [RequireComponent(typeof(Collider))]
    public class LocationTrigger : MonoBehaviour
    {
        [SerializeField] private LocationDefinition location;

        public void SetLocation(LocationDefinition def) => location = def;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || location == null) return;
            if (ServiceLocator.TryGet(out LocationSystem locationSystem))
                locationSystem.EnterLocation(location);
        }
    }
}
