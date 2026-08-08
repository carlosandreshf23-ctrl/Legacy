using System;
using System.Collections.Generic;
using LegadoPeru.Characters;
using LegadoPeru.Core;
using LegadoPeru.World;
using UnityEngine;
using UnityEngine.UI;

namespace LegadoPeru.UI
{
    /// <summary>
    /// Mapa esquemático MVP (prompt §28): posición aproximada, zonas visitadas, nombre de
    /// área. No es un mapa geográfico real; representación temporal.
    /// </summary>
    public class MapMVPController : MonoBehaviour
    {
        [Serializable]
        public class MapMarker
        {
            public LocationDefinition location;
            public RectTransform icon;
        }

        public GameObject panelRoot;
        public List<MapMarker> markers = new List<MapMarker>();
        public RectTransform playerMarker;
        public RectTransform mapArea;
        public Vector2 worldOriginXZ;
        public Vector2 worldSizeXZ = new Vector2(400f, 400f);

        private void OnEnable()
        {
            if (ServiceLocator.TryGet(out DiscoverySystem discovery))
            {
                discovery.OnStateChanged += (_, __) => RefreshMarkers();
                RefreshMarkers();
            }
        }

        private void OnDisable()
        {
            if (ServiceLocator.TryGet(out DiscoverySystem discovery))
                discovery.OnStateChanged -= (_, __) => RefreshMarkers();
        }

        private void Update()
        {
            if (panelRoot == null || !panelRoot.activeSelf) return;
            if (playerMarker == null || mapArea == null) return;
            if (!ServiceLocator.TryGet(out PlayerCharacterController player)) return;

            Vector2 normalized = new Vector2(
                (player.transform.position.x - worldOriginXZ.x) / Mathf.Max(0.01f, worldSizeXZ.x),
                (player.transform.position.z - worldOriginXZ.y) / Mathf.Max(0.01f, worldSizeXZ.y));

            playerMarker.anchoredPosition = new Vector2(
                normalized.x * mapArea.rect.width,
                normalized.y * mapArea.rect.height);
        }

        private void RefreshMarkers()
        {
            if (!ServiceLocator.TryGet(out DiscoverySystem discovery)) return;

            foreach (var marker in markers)
            {
                if (marker.icon == null || marker.location == null) continue;

                bool visited = discovery.GetState(marker.location.locationId) == DiscoveryState.Visited;
                var image = marker.icon.GetComponent<Image>();
                if (image != null)
                    image.color = visited ? Color.yellow : new Color(1f, 1f, 1f, 0.25f);
            }
        }
    }
}
