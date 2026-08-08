using System;
using System.Collections.Generic;

namespace LegadoPeru.World
{
    /// <summary>Cinco estados definidos en 07_..., pero Fase 1 solo utiliza Unknown/Visited (prompt §27).</summary>
    public enum DiscoveryState
    {
        Unknown,
        Rumor,
        Mapped,
        Visited,
        Mastered
    }

    [Serializable]
    public class DiscoveryEntryDto
    {
        public string locationId;
        public DiscoveryState state;
    }

    public class DiscoverySystem
    {
        private readonly Dictionary<string, DiscoveryState> states = new Dictionary<string, DiscoveryState>();

        public event Action<string, DiscoveryState> OnStateChanged;

        public DiscoveryState GetState(string locationId) =>
            states.TryGetValue(locationId, out var s) ? s : DiscoveryState.Unknown;

        public void MarkVisited(string locationId)
        {
            if (string.IsNullOrEmpty(locationId) || GetState(locationId) == DiscoveryState.Visited) return;
            states[locationId] = DiscoveryState.Visited;
            OnStateChanged?.Invoke(locationId, DiscoveryState.Visited);
        }

        public List<DiscoveryEntryDto> CaptureState()
        {
            var list = new List<DiscoveryEntryDto>();
            foreach (var kvp in states)
                list.Add(new DiscoveryEntryDto { locationId = kvp.Key, state = kvp.Value });
            return list;
        }

        public void RestoreState(List<DiscoveryEntryDto> data)
        {
            states.Clear();
            if (data == null) return;
            foreach (var entry in data)
                states[entry.locationId] = entry.state;
        }
    }
}
