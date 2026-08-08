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

        [Tooltip("Esquina inferior-izquierda del area jugable, en unidades de mundo (X,Y — plano cenital 2D).")]
        public Vector2 worldOriginXY;
        public Vector2 worldSizeXY = new Vector2(20f, 14f);

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
                (player.transform.position.x - worldOriginXY.x) / Mathf.Max(0.01f, worldSizeXY.x),
                (player.transform.position.y - worldOriginXY.y) / Mathf.Max(0.01f, worldSizeXY.y));

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
