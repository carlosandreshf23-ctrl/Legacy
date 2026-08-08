using UnityEngine;

namespace LegadoPeru.World
{
    /// <summary>
    /// Dato de diseño de una subzona del Sandbox (prompt §26). IDs provisionales en Fase 1:
    /// Prototype_Coast / Prototype_Path / Prototype_House.
    /// </summary>
    [CreateAssetMenu(menuName = "Legado/Location Definition", fileName = "LocationDefinition")]
    public class LocationDefinition : ScriptableObject
    {
        public string locationId;
        public string displayName;
        public string regionId = "REGION_PROTOTYPE_SANDBOX";
    }
}
