using System;
using LegadoPeru.Core;
using LegadoPeru.DebugTools;

namespace LegadoPeru.World
{
    /// <summary>
    /// Primera versión de LocationSystem (prompt §26; especializa el patrón general de
    /// InformationSystem para geografía, ver 03_DATA_MODEL.md §9). Registra la ubicación
    /// actual del jugador y delega el descubrimiento a DiscoverySystem.
    /// </summary>
    public class LocationSystem
    {
        public string CurrentLocationId { get; private set; }
        public string CurrentRegionId { get; private set; }

        public event Action<LocationDefinition> OnLocationChanged;

        public void EnterLocation(LocationDefinition location)
        {
            if (location == null || location.locationId == CurrentLocationId) return;

            CurrentLocationId = location.locationId;
            CurrentRegionId = location.regionId;

            if (ServiceLocator.TryGet(out DiscoverySystem discovery))
                discovery.MarkVisited(location.locationId);

            DebugLog.Log(DebugLog.Category.World, $"Entered location {location.locationId}");
            OnLocationChanged?.Invoke(location);
        }

        public string CaptureState() => CurrentLocationId;

        public void RestoreState(string locationId) => CurrentLocationId = locationId;
    }
}
