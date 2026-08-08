using System;

namespace LegadoPeru.Settings
{
    /// <summary>Configuración persistida SEPARADA del save de campaña (prompt §37).</summary>
    [Serializable]
    public class GameSettingsData
    {
        public float masterVolume = 1f;
        public float cameraSensitivity = 1f;

        /// <summary>0 = Low, 1 = Medium, 2 = High.</summary>
        public int qualityLevel = 1;
    }
}
